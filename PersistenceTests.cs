using System;
using System.IO;
using System.Linq;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Tuan 3 - luu/nap JSON (InventoryRepository). Moi test dung file tam rieng va tu xoa.
	public class PersistenceTests : IDisposable
	{
		private readonly string _path = Path.Combine(Path.GetTempPath(), "vrs_" + Guid.NewGuid().ToString("N") + ".json");

		public void Dispose()
		{
			if (File.Exists(_path)) File.Delete(_path);
		}

		private static DateTime Dec(int day) => new DateTime(2026, 12, day);

		private static InventoryService CreateInventory()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new EconomyVehicle("V001", "51H-123.45", "Toyota Vios", 40.5m, 15000, VehicleCondition.MinorDamage));
			inventory.AddVehicle(new SuvVehicle("V002", "51K-999.99", "Ford Everest", 90m, 8000));
			return inventory;
		}

		[Fact]
		public void SaveThenLoad_PreservesVehiclesIncludingConcreteType()
		{
			InventoryRepository.Save(CreateInventory(), _path);

			var loaded = InventoryRepository.Load(_path);

			Assert.Equal(2, loaded.Vehicles.Count);
			var eco = Assert.IsType<EconomyVehicle>(loaded.GetVehicle("V001"));
			Assert.Equal("Toyota Vios", eco.Model);
			Assert.Equal(40.5m, eco.DailyRate);
			Assert.Equal(15000, eco.Mileage);
			Assert.Equal(VehicleCondition.MinorDamage, eco.Condition);
			Assert.IsType<SuvVehicle>(loaded.GetVehicle("V002"));
		}

		[Fact]
		public void SaveThenLoad_PreservesBookingsAndStatuses()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V001", "Alice", Dec(1), Dec(5));
			inventory.CreateBooking("BK002", "V002", "Bob", Dec(1), Dec(5));
			inventory.CancelBooking("BK002");
			inventory.GetBooking("BK001").CompleteBooking();

			InventoryRepository.Save(inventory, _path);
			var loaded = InventoryRepository.Load(_path);

			Assert.Equal(BookingStatus.Completed, loaded.GetBooking("BK001").Status);
			Assert.Equal(BookingStatus.Cancelled, loaded.GetBooking("BK002").Status);
			Assert.Equal("Bob", loaded.GetBooking("BK002").CustomerName);
			Assert.Equal(Dec(1), loaded.GetBooking("BK002").StartDate);
		}

		[Fact]
		public void Load_ThenOverlapRuleStillWorks()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));
			InventoryRepository.Save(inventory, _path);

			var loaded = InventoryRepository.Load(_path);

			Assert.Throws<DoubleBookingException>(() =>
				loaded.CreateBooking("BK002", "V002", "Bob", Dec(14), Dec(18)));
			Assert.Empty(loaded.SearchAvailableVehicles(typeof(SuvVehicle), Dec(12), Dec(13)));
		}

		[Fact]
		public void Load_ThenBillingStillWorks()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V001", "Alice", Dec(1), Dec(5));
			InventoryRepository.Save(inventory, _path);

			var loaded = InventoryRepository.Load(_path);
			var bill = new BillingService().GenerateBill(
				loaded.GetBooking("BK001"), loaded.GetVehicle("V001"), Dec(6), 15400, VehicleCondition.Excellent);

			Assert.Equal(162m + 48.6m, bill.TotalAmount); // 4*40.5 + 1*40.5*1.2
		}

		[Fact]
		public void Load_FileWithoutOptionalFields_UsesDefaults()
		{
			File.WriteAllText(_path, @"{ ""Vehicles"": [ { ""Type"": ""EconomyVehicle"", ""Id"": ""V001"", ""DailyRate"": 40 } ] }");

			var loaded = InventoryRepository.Load(_path);

			var car = loaded.GetVehicle("V001");
			Assert.Equal(VehicleCondition.Excellent, car.Condition);
			Assert.Equal(0, car.Mileage);
			Assert.Empty(loaded.Bookings);
		}

		[Fact]
		public void Load_MissingFile_ThrowsPersistenceException()
		{
			Assert.Throws<PersistenceException>(() => InventoryRepository.Load(_path));
		}

		[Fact]
		public void Load_CorruptedJson_ThrowsPersistenceException()
		{
			File.WriteAllText(_path, "{ this is not json");

			Assert.Throws<PersistenceException>(() => InventoryRepository.Load(_path));
		}

		[Fact]
		public void Load_UnknownVehicleType_ThrowsPersistenceException()
		{
			File.WriteAllText(_path, @"{ ""Vehicles"": [ { ""Type"": ""Helicopter"", ""Id"": ""V001"", ""DailyRate"": 40 } ] }");

			var ex = Assert.Throws<PersistenceException>(() => InventoryRepository.Load(_path));

			Assert.Contains("Helicopter", ex.Message);
		}

		[Fact]
		public void Load_InvalidDailyRateInFile_ThrowsPersistenceException()
		{
			File.WriteAllText(_path, @"{ ""Vehicles"": [ { ""Type"": ""SuvVehicle"", ""Id"": ""V001"", ""DailyRate"": -5 } ] }");

			Assert.Throws<PersistenceException>(() => InventoryRepository.Load(_path));
		}

		[Fact]
		public void Load_OverlappingBookingsInFile_ThrowsPersistenceException()
		{
			File.WriteAllText(_path, @"{
				""Vehicles"": [ { ""Type"": ""SuvVehicle"", ""Id"": ""V001"", ""DailyRate"": 90 } ],
				""Bookings"": [
					{ ""BookingId"": ""A"", ""VehicleId"": ""V001"", ""CustomerName"": ""x"", ""StartDate"": ""2026-12-10T00:00:00"", ""EndDate"": ""2026-12-15T00:00:00"", ""Status"": ""Confirmed"" },
					{ ""BookingId"": ""B"", ""VehicleId"": ""V001"", ""CustomerName"": ""y"", ""StartDate"": ""2026-12-14T00:00:00"", ""EndDate"": ""2026-12-18T00:00:00"", ""Status"": ""Confirmed"" } ] }");

			Assert.Throws<PersistenceException>(() => InventoryRepository.Load(_path));
		}

		[Fact]
		public void Load_BookingForUnknownVehicle_ThrowsPersistenceException()
		{
			File.WriteAllText(_path, @"{ ""Bookings"": [
				{ ""BookingId"": ""A"", ""VehicleId"": ""V404"", ""CustomerName"": ""x"", ""StartDate"": ""2026-12-10T00:00:00"", ""EndDate"": ""2026-12-15T00:00:00"" } ] }");

			Assert.Throws<PersistenceException>(() => InventoryRepository.Load(_path));
		}

		[Fact]
		public void Save_ToMissingFolder_ThrowsPersistenceException()
		{
			var badPath = Path.Combine(Path.GetTempPath(), "no_such_folder_" + Guid.NewGuid().ToString("N"), "x.json");

			Assert.Throws<PersistenceException>(() => InventoryRepository.Save(CreateInventory(), badPath));
		}
	}
}
