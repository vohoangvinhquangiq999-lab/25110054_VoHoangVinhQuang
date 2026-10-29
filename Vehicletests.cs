using System;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Invariants protected by Vehicle (encapsulation):
	///  - Id must not be empty
	///  - DailyRate must be positive
	///  - Mileage can never go down
	public class VehicleTests
	{
		private static EconomyVehicle CreateEconomy(
			int mileage = 15000,
			VehicleCondition condition = VehicleCondition.Excellent)
			=> new EconomyVehicle("V001", "51H-123.45m", "Toyota Vios", 40.0m, mileage, condition);

		// Constructor invariants

		[Theory]
		[InlineData("")]
		[InlineData("   ")]
		public void Constructor_EmptyOrWhitespaceId_Throws(string id)
		{
			Assert.Throws<ArgumentException>(() =>
				new EconomyVehicle(id, "51H-123.45m", "Toyota Vios", 40.0m, 0));
		}

		[Theory]
		[InlineData(0.0)]
		[InlineData(-1.0)]
		[InlineData(-40.5)]
		public void Constructor_NonPositiveDailyRate_Throws(double dailyRate)
		{
			Assert.Throws<ArgumentException>(() =>
				new SuvVehicle("V002", "51K-999.99m", "Ford Everest", (decimal)dailyRate, 0));
		}

		[Fact]
		public void Constructor_ValidArguments_SetsAllProperties()
		{
			var car = new EconomyVehicle("V001", "51H-123.45m", "Toyota Vios", 40.0m, 15000, VehicleCondition.MinorDamage);

			Assert.Equal("V001", car.Id);
			Assert.Equal("51H-123.45m", car.LicensePlate);
			Assert.Equal("Toyota Vios", car.Model);
			Assert.Equal(40.0m, car.DailyRate);
			Assert.Equal(15000, car.Mileage);
			Assert.Equal(VehicleCondition.MinorDamage, car.Condition);
		}

		[Fact]
		public void Constructor_WithoutCondition_DefaultsToExcellent()
		{
			var car = new SuvVehicle("V002", "51K-999.99m", "Ford Everest", 90.0m, 8000);

			Assert.Equal(VehicleCondition.Excellent, car.Condition);
		}

		// updateAfterRental: mileage / condition tracking

		[Fact]
		public void UpdateAfterRental_ValidValues_UpdatesMileageAndCondition()
		{
			var car = CreateEconomy(mileage: 15000);

			car.UpdateAfterRental(15400, VehicleCondition.MinorDamage);

			Assert.Equal(15400, car.Mileage);
			Assert.Equal(VehicleCondition.MinorDamage, car.Condition);
		}

		[Fact]
		public void UpdateAfterRental_SameMileage_IsAllowed()
		{
			var car = CreateEconomy(mileage: 15000);

			car.UpdateAfterRental(15000, VehicleCondition.Excellent);

			Assert.Equal(15000, car.Mileage);
		}

		[Fact]
		public void UpdateAfterRental_MileageDecreases_ThrowsAndKeepsOldState()
		{
			var car = CreateEconomy(mileage: 15000, condition: VehicleCondition.Excellent);

			var ex = Assert.Throws<InvalidOdometerException>(() =>
				car.UpdateAfterRental(14999, VehicleCondition.MajorDamage));

			Assert.Contains("Invalid Odometer", ex.Message);
			// a rejected update must not change anything (state invariant is preserved)
			Assert.Equal(15000, car.Mileage);
			Assert.Equal(VehicleCondition.Excellent, car.Condition);
		}

		[Fact]
		public void UpdateAfterRental_TracksMileageAcrossSeveralRentals()
		{
			var car = CreateEconomy(mileage: 1000);

			car.UpdateAfterRental(1200, VehicleCondition.Excellent);
			car.UpdateAfterRental(1750, VehicleCondition.MinorDamage);

			Assert.Equal(1750, car.Mileage);
			Assert.Equal(VehicleCondition.MinorDamage, car.Condition);
		}
	}
}