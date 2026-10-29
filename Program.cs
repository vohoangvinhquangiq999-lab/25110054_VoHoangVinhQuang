using System;
using System.Globalization;
using System.IO;
namespace VehicleRentalSystem
{
	
	internal class Program
	{
		private static void Main(string[] args)
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			Console.WriteLine("VEHICLE RENTAL SYSTEM - DEMO TEST");

			// setup system
			var inventory = new InventoryService();
			var billing = new BillingService();

			// populate vehicle
			var ecoCar = new EconomyVehicle("V001", "51H-123.45", "Toyota Vios", dailyRate: 40.0, mileage: 15000);
			var suvCar = new SuvVehicle("V002", "51K-999.99", "Ford Everest", dailyRate: 90.0, mileage: 8000);

			inventory.AddVehicle(ecoCar);
			inventory.AddVehicle(suvCar);

			// TEST CASE 1: Happy Path End-to-End Scenario
			Console.WriteLine("[TEST 1] Happy Path End-to-End Scenario");
			DateTime start1 = new DateTime(2026, 11, 01);
			DateTime end1 = new DateTime(2026, 11, 05); // 4 Days

			// Search
			var availableEco = inventory.SearchAvailableVehicles(typeof(EconomyVehicle), start1, end1);
			Console.WriteLine($"Search Result (Economy vehicles available): {availableEco.Count} found.");

			// Book
			Booking booking1 = inventory.CreateBooking("BK001", "V001", "John Doe", start1, end1);
			Console.WriteLine($"Booking Created: {booking1.BookingId} for Vehicle {booking1.VehicleId}");

			// return with 1 day late and minordamage
			DateTime actualReturn1 = new DateTime(2026, 11, 06);
			Bill bill1 = billing.GenerateBill(booking1, ecoCar, actualReturn1, returnMileage: 15400, returnCondition: VehicleCondition.MinorDamage);

			bill1.PrintInvoice();
			Console.WriteLine($"Updated Vehicle V001 Mileage: {ecoCar.Mileage} km, Condition: {ecoCar.Condition}\n");

			// TEST CASE 2: Validation and error scenario (Double booking Rejection)
	
			Console.WriteLine("[TEST 2] Error Scenario: Double Booking Rejection");
			DateTime start2 = new DateTime(2026, 12, 10);
			DateTime end2 = new DateTime(2026, 12, 15);

			inventory.CreateBooking("BK002", "V002", "Alice Smith", start2, end2);
			Console.WriteLine($"Successfully booked V002 for Alice ({start2:yyyy-MM-dd} to {end2:yyyy-MM-dd}).");

			try
			{
				Console.WriteLine("Attempting overlapping booking for Bob (2026-12-14 to 2026-12-18)...");
				DateTime startOverlap = new DateTime(2026, 12, 14);
				DateTime endOverlap = new DateTime(2026, 12, 18);

				// This call MUST fail
				inventory.CreateBooking("BK003", "V002", "Bob Johnson", startOverlap, endOverlap);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[SUCCESSFULLY REJECTED] Exception Caught: {ex.Message}\n");
			}

			// TEST CASE 3: Polymorphism verification (economy vs suv Behavior)
			Console.WriteLine("[TEST 3] Polymorphism Scenario: Pricing & Fee Variation");
			DateTime start3 = new DateTime(2026, 12, 01);
			DateTime end3 = new DateTime(2026, 12, 03); // 2 Days
			DateTime returnLate = new DateTime(2026, 12, 05); // 2 Days Late

			// Create 2 identical duration bookings for Economy and SUV
			Booking ecoBooking = inventory.CreateBooking("BK_ECO", "V001", "Customer A", start3, end3);
			Booking suvBooking = inventory.CreateBooking("BK_SUV", "V002", "Customer B", start3, end3);

			// Return both 2 days late with Major Damage
			Bill ecoBill = billing.GenerateBill(ecoBooking, ecoCar, returnLate, 16000, VehicleCondition.MajorDamage);
			Bill suvBill = billing.GenerateBill(suvBooking, suvCar, returnLate, 9000, VehicleCondition.MajorDamage);

			Console.WriteLine("Economy Vehicle Bill Breakdown:");
			Console.WriteLine($"    Base: ${ecoBill.BaseCost:F2}, Late Fee: ${ecoBill.LateFee:F2}, Damage Fee: ${ecoBill.DamageFee:F2} -> Total: ${ecoBill.TotalAmount:F2}");

			Console.WriteLine("SUV Vehicle Bill Breakdown:");
			Console.WriteLine($"    Base: ${suvBill.BaseCost:F2}, Late Fee: ${suvBill.LateFee:F2}, Damage Fee: ${suvBill.DamageFee:F2} -> Total: ${suvBill.TotalAmount:F2}\n");
			// TEST CASE 4 (tuan 3): xe hong nang bi khoa, sua xong cho thue lai; huy booking tha lich
			Console.WriteLine("[TEST 4] Lifecycle: Out-of-service Vehicle & Cancel Booking");
			DateTime start4 = new DateTime(2027, 1, 10);
            DateTime end4 = new DateTime(2027, 1, 12);

            // ecoCar vua tra voi MajorDamage o Test 3 => khong cho thue
            try
            {
	        inventory.CreateBooking("BK004", "V001", "Dave", start4, end4);
            }
            catch (VehicleUnavailableException ex)
            {
	        Console.WriteLine($"[SUCCESSFULLY REJECTED] {ex.GetType().Name}: {ex.Message}");
            }

            ecoCar.Repair();
            suvCar.Repair();
            inventory.CreateBooking("BK004", "V001", "Dave", start4, end4);
            Console.WriteLine($"After Repair(), V001 booked successfully (BK004). Condition: {ecoCar.Condition}");

            Console.WriteLine($"SUV available {start2:yyyy-MM-dd}..{end2:yyyy-MM-dd} while BK002 is active: {inventory.SearchAvailableVehicles(typeof(SuvVehicle), start2, end2).Count} found.");
            inventory.CancelBooking("BK002"); // Alice huy lich V002 12/2026
            Console.WriteLine($"After cancelling BK002: {inventory.SearchAvailableVehicles(typeof(SuvVehicle), start2, end2).Count} found.\n");

            // TEST CASE 5 (tuan 3): luu / nap file JSON
            Console.WriteLine("[TEST 5] Persistence: Save & Load JSON");
            string file = Path.Combine(Path.GetTempPath(), "fleet_demo.json");
            InventoryRepository.Save(inventory, file);
            InventoryService loaded = InventoryRepository.Load(file);
            Console.WriteLine($"Saved to {file}");
            Console.WriteLine($"Loaded {loaded.Vehicles.Count} vehicles, {loaded.Bookings.Count} bookings. V001 mileage: {loaded.GetVehicle("V001").Mileage} km.");
            File.Delete(file);

		}
	}
}
