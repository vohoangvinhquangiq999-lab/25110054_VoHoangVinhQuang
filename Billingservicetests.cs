using System;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Billing (Single Responsibility: pricing separated from search/availability)
	/// Each test creates its own vehicle, inventory and billing objects,
	/// Reference prices:  Economy 40/day (late x1.2, minor 100, major 350)
	///                    SUV     90/day (late x1.5, minor 250, major 800)
	public class BillingServiceTests
	{
		private static DateTime Day(int month, int day) => new DateTime(2026, month, day);

		private sealed class Fixture
		{
			public InventoryService Inventory { get; } = new InventoryService();
			public BillingService Billing { get; } = new BillingService();
			public EconomyVehicle Eco { get; } = new EconomyVehicle("V001", "51H-123.45m", "Toyota Vios", 40.0m, 15000);
			public SuvVehicle Suv { get; } = new SuvVehicle("V002", "51K-999.99m", "Ford Everest", 90.0m, 8000);

			public Fixture()
			{
				Inventory.AddVehicle(Eco);
				Inventory.AddVehicle(Suv);
			}
		}

		//Happy path

		[Fact]
		public void GenerateBill_EconomyOneDayLateWithMinorDamage_Totals308()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5)); // 4 days

			var bill = f.Billing.GenerateBill(booking, f.Eco, Day(11, 6), 15400, VehicleCondition.MinorDamage);

			Assert.Equal("BK001", bill.BookingId);
			Assert.Equal(160.0m, bill.BaseCost);   // 4 * 40
			Assert.Equal(48.0m, bill.LateFee);     // 1 * 40 * 1.2m
			Assert.Equal(100.0m, bill.DamageFee);
			Assert.Equal(308.0m, bill.TotalAmount);
		}

		[Fact]
		public void GenerateBill_ReturnOnTime_NoLateFee()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5));

			var bill = f.Billing.GenerateBill(booking, f.Eco, Day(11, 5), 15200, VehicleCondition.Excellent);

			Assert.Equal(0.0m, bill.LateFee);
			Assert.Equal(160.0m, bill.TotalAmount);
		}

		[Fact]
		public void GenerateBill_ReturnEarly_NeverProducesNegativeLateFee()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5));

			var bill = f.Billing.GenerateBill(booking, f.Eco, Day(11, 3), 15100, VehicleCondition.Excellent);

			Assert.Equal(0.0m, bill.LateFee);
		}

		[Fact]
		public void GenerateBill_ExcellentCondition_NoDamageFee()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V002", "Jane", Day(11, 1), Day(11, 3));

			var bill = f.Billing.GenerateBill(booking, f.Suv, Day(11, 3), 8100, VehicleCondition.Excellent);

			Assert.Equal(0.0m, bill.DamageFee);
			Assert.Equal(180.0m, bill.TotalAmount); // 2 * 90
		}

		[Fact]
		public void GenerateBill_BookingShorterThanOneDay_IsBilledAsOneDay()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe",
				new DateTime(2026, 11, 1, 8, 0, 0), new DateTime(2026, 11, 1, 20, 0, 0));

			var bill = f.Billing.GenerateBill(booking, f.Eco, new DateTime(2026, 11, 1, 20, 0, 0), 15050, VehicleCondition.Excellent);

			Assert.Equal(40.0m, bill.BaseCost);
		}

		// Polymorphism scenario

		[Fact]
		public void GenerateBill_EconomyTwoDaysLateMajorDamage_Totals526()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK_ECO", "V001", "Customer A", Day(12, 1), Day(12, 3)); // 2 days

			var bill = f.Billing.GenerateBill(booking, f.Eco, Day(12, 5), 16000, VehicleCondition.MajorDamage);

			Assert.Equal(80.0m, bill.BaseCost);
			Assert.Equal(96.0m, bill.LateFee);     // 2 * 40 * 1.2m
			Assert.Equal(350.0m, bill.DamageFee);
			Assert.Equal(526.0m, bill.TotalAmount);
		}

		[Fact]
		public void GenerateBill_SuvTwoDaysLateMajorDamage_Totals1250()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK_SUV", "V002", "Customer B", Day(12, 1), Day(12, 3)); // 2 days

			var bill = f.Billing.GenerateBill(booking, f.Suv, Day(12, 5), 9000, VehicleCondition.MajorDamage);

			Assert.Equal(180.0m, bill.BaseCost);
			Assert.Equal(270.0m, bill.LateFee);    // 2 * 90 * 1.5m
			Assert.Equal(800.0m, bill.DamageFee);
			Assert.Equal(1250.0m, bill.TotalAmount);
		}

		[Fact]
		public void GenerateBill_SameBookingDatesAndReturn_DifferentCategoriesGiveDifferentTotals()
		{
			var f = new Fixture();
			var ecoBooking = f.Inventory.CreateBooking("BK_ECO", "V001", "Customer A", Day(12, 1), Day(12, 3));
			var suvBooking = f.Inventory.CreateBooking("BK_SUV", "V002", "Customer B", Day(12, 1), Day(12, 3));

			var ecoBill = f.Billing.GenerateBill(ecoBooking, f.Eco, Day(12, 5), 16000, VehicleCondition.MajorDamage);
			var suvBill = f.Billing.GenerateBill(suvBooking, f.Suv, Day(12, 5), 9000, VehicleCondition.MajorDamage);

			Assert.NotEqual(ecoBill.TotalAmount, suvBill.TotalAmount);
			Assert.True(suvBill.TotalAmount > ecoBill.TotalAmount);
		}

		// Side effects: vehicle state+booking lifecycle

		[Fact]
		public void GenerateBill_UpdatesVehicleMileageAndCondition()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5));

			f.Billing.GenerateBill(booking, f.Eco, Day(11, 5), 15400, VehicleCondition.MinorDamage);

			Assert.Equal(15400, f.Eco.Mileage);
			Assert.Equal(VehicleCondition.MinorDamage, f.Eco.Condition);
		}

		[Fact]
		public void GenerateBill_MarksBookingAsCompleted()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5));

			f.Billing.GenerateBill(booking, f.Eco, Day(11, 5), 15400, VehicleCondition.Excellent);

			Assert.Equal(BookingStatus.Completed, booking.Status);
		}

		[Fact]
		public void GenerateBill_SameBookingTwice_ThrowsInvalidBookingState()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5));
			f.Billing.GenerateBill(booking, f.Eco, Day(11, 5), 15400, VehicleCondition.Excellent);

			Assert.Throws<InvalidBookingStateException>(() =>
				f.Billing.GenerateBill(booking, f.Eco, Day(11, 5), 15400, VehicleCondition.Excellent));
		}

		[Fact]
		public void GenerateBill_ReturnMileageBelowCurrent_ThrowsAndBookingStaysConfirmed()
		{
			var f = new Fixture();
			var booking = f.Inventory.CreateBooking("BK001", "V001", "John Doe", Day(11, 1), Day(11, 5));

			Assert.Throws<InvalidOdometerException>(() =>
				f.Billing.GenerateBill(booking, f.Eco, Day(11, 5), 14000, VehicleCondition.MinorDamage));

			Assert.Equal(BookingStatus.Confirmed, booking.Status);
			Assert.Equal(15000, f.Eco.Mileage);
			Assert.Equal(VehicleCondition.Excellent, f.Eco.Condition);
		}

		// Bill

		[Fact]
		public void Bill_TotalAmount_IsSumOfBaseLateAndDamage()
		{
			var bill = new Bill("BK001", 100.0m, 20.0m, 5.0m);

			Assert.Equal(125.0m, bill.TotalAmount);
		}
	}
}