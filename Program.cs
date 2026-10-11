using System;
using System.Collections.Generic;
using System.Linq;

namespace VehicleRentalSystem
{
	public enum VehicleCondition
	{
		Excellent,
		MinorDamage,
		MajorDamage
	}

	public enum BookingStatus
	{
		Confirmed,
		Completed,
		Cancelled
	}


	// abstract class vehicle
	public abstract class Vehicle
	{
		public string Id { get; }
		public string LicensePlate { get; }
		public string Model { get; }
		public double DailyRate { get; protected set; }
		public int Mileage { get; private set; }
		public VehicleCondition Condition { get; private set; }

		protected Vehicle(string id, string licensePlate, string model, double dailyRate, int mileage, VehicleCondition condition)
		{
			if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Vehicle ID cannot be empty.");
			if (dailyRate <= 0) throw new ArgumentException("Daily rate must be positive.");

			Id = id;
			LicensePlate = licensePlate;
			Model = model;
			DailyRate = dailyRate;
			Mileage = mileage;
			Condition = condition;
		}

		// encapsulation: Protects state invariant for mileage and condition
		public void UpdateAfterRental(int newMileage, VehicleCondition returnCondition)
		{
			if (newMileage < Mileage)
			{
				throw new ArgumentException($"Invalid Odometer: New mileage ({newMileage}) cannot be less than current mileage ({Mileage}).");
			}

			Mileage = newMileage;
			Condition = returnCondition;
		}

		// polymorphism: fee Calculation methods
		public abstract double CalculateLateFee(int extraDays);
		public abstract double CalculateDamageFee(VehicleCondition returnCondition);
	}

	//economy vehicle
	public class EconomyVehicle : Vehicle
	{
		public EconomyVehicle(string id, string licensePlate, string model, double dailyRate, int mileage, VehicleCondition condition = VehicleCondition.Excellent)
			: base(id, licensePlate, model, dailyRate, mileage, condition) { }

		public override double CalculateLateFee(int extraDays)
		{
			if (extraDays <= 0) return 0.0;
			return extraDays * DailyRate * 1.2; // 20% penalty
		}

		public override double CalculateDamageFee(VehicleCondition returnCondition)
		{
			return returnCondition switch
			{
				VehicleCondition.MinorDamage => 100.0,
				VehicleCondition.MajorDamage => 350.0,
				_ => 0.0
			};
		}
	}

	//suv vehicle
	public class SuvVehicle : Vehicle
	{
		public SuvVehicle(string id, string licensePlate, string model, double dailyRate, int mileage, VehicleCondition condition = VehicleCondition.Excellent)
			: base(id, licensePlate, model, dailyRate, mileage, condition) { }

		public override double CalculateLateFee(int extraDays)
		{
			if (extraDays <= 0) return 0.0;
			return extraDays * DailyRate * 1.5; // 50% penalty for suv
		}

		public override double CalculateDamageFee(VehicleCondition returnCondition)
		{
			return returnCondition switch
			{
				VehicleCondition.MinorDamage => 250.0,
				VehicleCondition.MajorDamage => 800.0,
				_ => 0.0
			};
		}
	}

	// encapsulation booking class
	public class Booking
	{
		public string BookingId { get; }
		public string VehicleId { get; }
		public string CustomerName { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public BookingStatus Status { get; private set; }

		public Booking(string bookingId, string vehicleId, string customerName, DateTime startDate, DateTime endDate)
		{
			if (endDate <= startDate)
				throw new ArgumentException("Invalid Range: End date must be strictly after start date.");

			BookingId = bookingId;
			VehicleId = vehicleId;
			CustomerName = customerName;
			StartDate = startDate;
			EndDate = endDate;
			Status = BookingStatus.Confirmed;
		}

		// overlap detection algorithm
		public bool OverlapsWith(DateTime start, DateTime end)
		{
			if (Status == BookingStatus.Cancelled) return false;
			return StartDate < end && EndDate > start;
		}

		public void CompleteBooking()
		{
			if (Status != BookingStatus.Confirmed)
				throw new InvalidOperationException("Only confirmed bookings can be completed.");

			Status = BookingStatus.Completed;
		}
	}

	// invoice
	public class Bill
	{
		public string BookingId { get; set; } = string.Empty;
		public double BaseCost { get; set; }
		public double LateFee { get; set; }
		public double DamageFee { get; set; }
		public double TotalAmount => BaseCost + LateFee + DamageFee;

		public void PrintInvoice()
		{
			Console.WriteLine($"         INVOICE FOR BOOKING: {BookingId}");
			Console.WriteLine($" Base Rental Fee : ${BaseCost,10:F2}");
			Console.WriteLine($" Late Return Fee : ${LateFee,10:F2}");
			Console.WriteLine($" Damage Charge   : ${DamageFee,10:F2}");
			Console.WriteLine($" TOTAL AMOUNT    : ${TotalAmount,10:F2}");
		}
	}

	// inventory service
	public class InventoryService
	{
		private readonly List<Vehicle> _vehicles = new();
		private readonly List<Booking> _bookings = new();

		public void AddVehicle(Vehicle vehicle) => _vehicles.Add(vehicle);

		public List<Vehicle> SearchAvailableVehicles(Type categoryType, DateTime start, DateTime end)
		{
			if (end <= start)
				throw new ArgumentException("End date must be strictly after start date.");

			return _vehicles
				.Where(v => categoryType.IsInstanceOfType(v))
				.Where(v => !_bookings.Any(b => b.VehicleId == v.Id && b.OverlapsWith(start, end)))
				.ToList();
		}

		public Booking CreateBooking(string bookingId, string vehicleId, string customerName, DateTime start, DateTime end)
		{
			var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId)
				?? throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");

			// check for double booking
			bool isBooked = _bookings.Any(b => b.VehicleId == vehicleId && b.OverlapsWith(start, end));
			if (isBooked)
			{
				throw new InvalidOperationException($"Double-Booking Error: Vehicle '{vehicleId}' is already booked during {start:yyyy-MM-dd} to {end:yyyy-MM-dd}.");
			}

			var booking = new Booking(bookingId, vehicleId, customerName, start, end);
			_bookings.Add(booking);
			return booking;
		}

		public Vehicle GetVehicle(string vehicleId)
		{
			return _vehicles.FirstOrDefault(v => v.Id == vehicleId)
				?? throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");
		}
	}

	// billing Service: Handles financial transactions and check-in return calculations
	public class BillingService
	{
		public Bill GenerateBill(Booking booking, Vehicle vehicle, DateTime actualReturnDate, int returnMileage, VehicleCondition returnCondition)
		{
			// 1. calculate base rental cost
			int totalDays = (booking.EndDate - booking.StartDate).Days;
			if (totalDays <= 0) totalDays = 1;
			double baseCost = totalDays * vehicle.DailyRate;

			// 2. calculate late fee 
			int extraDays = (actualReturnDate - booking.EndDate).Days;
			double lateFee = vehicle.CalculateLateFee(Math.Max(0, extraDays));

			// 3. calculate damage fee
			double damageFee = vehicle.CalculateDamageFee(returnCondition);

			// 4. update vehicle state and booking lifecycle
			vehicle.UpdateAfterRental(returnMileage, returnCondition);
			booking.CompleteBooking();

			return new Bill
			{
				BookingId = booking.BookingId,
				BaseCost = baseCost,
				LateFee = lateFee,
				DamageFee = damageFee
			};
		}
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
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
			Console.WriteLine($"    Base: ${ecoBill.BaseCost}, Late Fee: ${ecoBill.LateFee}, Damage Fee: ${ecoBill.DamageFee} -> Total: ${ecoBill.TotalAmount}");

			Console.WriteLine("SUV Vehicle Bill Breakdown:");
			Console.WriteLine($"    Base: ${suvBill.BaseCost}, Late Fee: ${suvBill.LateFee}, Damage Fee: ${suvBill.DamageFee} -> Total: ${suvBill.TotalAmount}");
		}
	}
}