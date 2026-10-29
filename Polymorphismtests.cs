using System;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// polymorphism: fee rules vary by vehicle category
	/// works with InventoryService and BillingService without editing any existing class.
	public class PolymorphismTests
	{
		private static readonly DateTime Start = new DateTime(2026, 12, 1);
		private static readonly DateTime End = new DateTime(2026, 12, 3);

		// a new variant added without touching Vehicle/EconomyVehicle/SuvVehicle/BillingService
		private sealed class LuxuryVehicle : Vehicle
		{
			public LuxuryVehicle(string id)
				: base(id, "51A-777.77m", "Mercedes S-Class", 200.0m, 1000, VehicleCondition.Excellent) { }

			public override decimal CalculateLateFee(int extraDays)
				=> extraDays <= 0 ? 0m : extraDays * DailyRate * 2.0m; // 100% penalty

			public override decimal CalculateDamageFee(VehicleCondition returnCondition)
				=> returnCondition == VehicleCondition.Excellent ? 0m : 1500m;
		}

		// Late fee

		[Theory]
		[InlineData(0)]
		[InlineData(-3)]
		public void CalculateLateFee_ZeroOrNegativeExtraDays_IsZero(int extraDays)
		{
			Vehicle eco = new EconomyVehicle("E1", "plate", "Vios", 40.0m, 0);
			Vehicle suv = new SuvVehicle("S1", "plate", "Everest", 90.0m, 0);

			Assert.Equal(0.0m, eco.CalculateLateFee(extraDays));
			Assert.Equal(0.0m, suv.CalculateLateFee(extraDays));
		}

		[Theory]
		[InlineData(1, 48.0)]   // 1 * 40 * 1.2
		[InlineData(3, 144.0)]  // 3 * 40 * 1.2
		public void Economy_CalculateLateFee_Uses20PercentPenalty(int extraDays, double expectedValue)
		{
			Vehicle eco = new EconomyVehicle("E1", "plate", "Vios", 40.0m, 0);

			Assert.Equal((decimal)expectedValue, eco.CalculateLateFee(extraDays));
		}

		[Theory]
		[InlineData(1, 135.0)]  // 1 * 90 * 1.5
		[InlineData(2, 270.0)]  // 2 * 90 * 1.5
		public void Suv_CalculateLateFee_Uses50PercentPenalty(int extraDays, double expectedValue)
		{
			Vehicle suv = new SuvVehicle("S1", "plate", "Everest", 90.0m, 0);

			Assert.Equal((decimal)expectedValue, suv.CalculateLateFee(extraDays));
		}

		// Damage fee

		[Theory]
		[InlineData(VehicleCondition.Excellent, 0.0)]
		[InlineData(VehicleCondition.MinorDamage, 100.0)]
		[InlineData(VehicleCondition.MajorDamage, 350.0)]
		public void Economy_CalculateDamageFee_MatchesRule(VehicleCondition condition, double expectedValue)
		{
			Vehicle eco = new EconomyVehicle("E1", "plate", "Vios", 40.0m, 0);

			Assert.Equal((decimal)expectedValue, eco.CalculateDamageFee(condition));
		}

		[Theory]
		[InlineData(VehicleCondition.Excellent, 0.0)]
		[InlineData(VehicleCondition.MinorDamage, 250.0)]
		[InlineData(VehicleCondition.MajorDamage, 800.0)]
		public void Suv_CalculateDamageFee_MatchesRule(VehicleCondition condition, double expectedValue)
		{
			Vehicle suv = new SuvVehicle("S1", "plate", "Everest", 90.0m, 0);

			Assert.Equal((decimal)expectedValue, suv.CalculateDamageFee(condition));
		}

		// Same call through the base type, different behavior

		[Fact]
		public void SameCallOnBaseType_GivesDifferentFeesPerSubtype()
		{
			Vehicle eco = new EconomyVehicle("E1", "plate", "Vios", 40.0m, 0);
			Vehicle suv = new SuvVehicle("S1", "plate", "Everest", 90.0m, 0);

			decimal ecoLate = eco.CalculateLateFee(2);
			decimal suvLate = suv.CalculateLateFee(2);
			decimal ecoDamage = eco.CalculateDamageFee(VehicleCondition.MajorDamage);
			decimal suvDamage = suv.CalculateDamageFee(VehicleCondition.MajorDamage);

			Assert.Equal(96.0m, ecoLate);
			Assert.Equal(270.0m, suvLate);
			Assert.Equal(350.0m, ecoDamage);
			Assert.Equal(800.0m, suvDamage);
			Assert.NotEqual(ecoLate, suvLate);
			Assert.NotEqual(ecoDamage, suvDamage);
		}

		// new variant, no edits to existing class

		[Fact]
		public void NewSubtype_WorksWithBillingServiceWithoutEditingExistingClasses()
		{
			var inventory = new InventoryService();
			var billing = new BillingService();
			var lux = new LuxuryVehicle("V003");
			inventory.AddVehicle(lux);

			// 3 days booked (Dec 1 - Dec 4), returned 1 day late (Dec 5), major damage
			var booking = inventory.CreateBooking("BK_LUX", "V003", "Customer C",
				new DateTime(2026, 12, 1), new DateTime(2026, 12, 4));
			var bill = billing.GenerateBill(booking, lux, new DateTime(2026, 12, 5), 1500, VehicleCondition.MajorDamage);

			Assert.Equal(600.0m, bill.BaseCost);    // 3 * 200
			Assert.Equal(400.0m, bill.LateFee);     // 1 * 200 * 2.0m
			Assert.Equal(1500.0m, bill.DamageFee);
			Assert.Equal(2500.0m, bill.TotalAmount);
		}

		[Fact]
		public void NewSubtype_CanBeFoundByCategorySearch()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new EconomyVehicle("V001", "plate", "Vios", 40.0m, 0));
			inventory.AddVehicle(new LuxuryVehicle("V003"));

			var result = inventory.SearchAvailableVehicles(typeof(LuxuryVehicle), Start, End);

			var found = Assert.Single(result);
			Assert.Equal("V003", found.Id);
		}
	}
}