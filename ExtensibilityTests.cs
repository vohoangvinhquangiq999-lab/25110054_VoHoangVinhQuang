using System;
using System.IO;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Tuan 4 - Open/Closed: LuxuryVehicle chi la 1 file moi, nhung chay duoc qua
	/// tim kiem, dat xe, tinh tien va luu/nap file ma khong sua bat ky lop nao co san.
	public class ExtensibilityTests : IDisposable
	{
		private readonly string _path = Path.Combine(Path.GetTempPath(), "vrs_lux_" + Guid.NewGuid().ToString("N") + ".json");

		public void Dispose()
		{
			if (File.Exists(_path)) File.Delete(_path);
		}

		private static DateTime Dec(int day) => new DateTime(2026, 12, day);

		private static InventoryService CreateInventory()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new EconomyVehicle("V001", "51H-123.45", "Toyota Vios", 40m, 15000));
			inventory.AddVehicle(new SuvVehicle("V002", "51K-999.99", "Ford Everest", 90m, 8000));
			inventory.AddVehicle(new LuxuryVehicle("V003", "51A-777.77", "Mercedes S-Class", 200m, 1000));
			return inventory;
		}

		[Fact]
		public void Luxury_LateFee_Uses100PercentPenalty()
		{
			Vehicle lux = new LuxuryVehicle("V003", "p", "m", 200m, 0);

			Assert.Equal(0m, lux.CalculateLateFee(0));
			Assert.Equal(400m, lux.CalculateLateFee(1));
			Assert.Equal(800m, lux.CalculateLateFee(2));
		}

		[Theory]
		[InlineData(VehicleCondition.Excellent, 0.0)]
		[InlineData(VehicleCondition.MinorDamage, 500.0)]
		[InlineData(VehicleCondition.MajorDamage, 1500.0)]
		public void Luxury_DamageFee_MatchesRule(VehicleCondition condition, double expectedValue)
		{
			Vehicle lux = new LuxuryVehicle("V003", "p", "m", 200m, 0);

			Assert.Equal((decimal)expectedValue, lux.CalculateDamageFee(condition));
		}

		[Fact]
		public void Luxury_CanBeFoundByOwnCategoryAndByBaseType()
		{
			var inventory = CreateInventory();

			Assert.Equal("V003", Assert.Single(inventory.SearchAvailableVehicles(typeof(LuxuryVehicle), Dec(1), Dec(3))).Id);
			Assert.Equal(3, inventory.SearchAvailableVehicles(typeof(Vehicle), Dec(1), Dec(3)).Count);
			Assert.Equal("V001", Assert.Single(inventory.SearchAvailableVehicles(typeof(EconomyVehicle), Dec(1), Dec(3))).Id); // khong lan sang loai khac
		}

		[Fact]
		public void Luxury_DoubleBookingIsStillRejected()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V003", "Alice", Dec(10), Dec(15));

			Assert.Throws<DoubleBookingException>(() =>
				inventory.CreateBooking("BK002", "V003", "Bob", Dec(14), Dec(18)));
		}

		[Fact]
		public void Luxury_EndToEnd_ThreeCategoriesGiveThreeDifferentTotals()
		{
			var inventory = CreateInventory();
			var billing = new BillingService();
			decimal ecoTotal, suvTotal, luxTotal;

			// cung 2 ngay thue, tra tre 2 ngay, hu hai nang
			ecoTotal = Bill(inventory, billing, "BK_E", "V001");
			suvTotal = Bill(inventory, billing, "BK_S", "V002");
			luxTotal = Bill(inventory, billing, "BK_L", "V003");

			Assert.Equal(526m, ecoTotal);   // 80 + 96 + 350
			Assert.Equal(1250m, suvTotal);  // 180 + 270 + 800
			Assert.Equal(2700m, luxTotal);  // 400 + 800 + 1500
		}

		private static decimal Bill(InventoryService inv, BillingService billing, string bookingId, string vehicleId)
		{
			var booking = inv.CreateBooking(bookingId, vehicleId, "Customer", Dec(1), Dec(3));
			var vehicle = inv.GetVehicle(vehicleId);
			return billing.GenerateBill(booking, vehicle, Dec(5), vehicle.Mileage + 100, VehicleCondition.MajorDamage).TotalAmount;
		}

		[Fact]
		public void Luxury_SaveThenLoad_KeepsConcreteTypeWithoutEditingRepository()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V003", "Alice", Dec(10), Dec(12));
			InventoryRepository.Save(inventory, _path);

			var loaded = InventoryRepository.Load(_path);

			var lux = Assert.IsType<LuxuryVehicle>(loaded.GetVehicle("V003"));
			Assert.Equal("Mercedes S-Class", lux.Model);
			Assert.Equal(200m, lux.DailyRate);
			Assert.Equal("V003", loaded.GetBooking("BK001").VehicleId);
		}

		[Fact]
		public void Luxury_InvalidRate_StillRejectedByBaseClassInvariant()
		{
			Assert.Throws<ArgumentException>(() => new LuxuryVehicle("V003", "p", "m", 0m, 0));
		}
	}
}
