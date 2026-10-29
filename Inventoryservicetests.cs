using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// availability search + double-booking prevention.
	/// every test builds its own fresh InventoryService, so tests are independent and repeatable.
	public class InventoryServiceTests
	{
		private static DateTime Dec(int day) => new DateTime(2026, 12, day);

		private static InventoryService CreateInventory()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new EconomyVehicle("V001", "51H-123.45m", "Toyota Vios", 40.0m, 15000));
			inventory.AddVehicle(new EconomyVehicle("V003", "51H-555.55m", "Hyundai Accent", 42.0m, 9000));
			inventory.AddVehicle(new SuvVehicle("V002", "51K-999.99m", "Ford Everest", 90.0m, 8000));
			return inventory;
		}

		// search available vehicle

		[Fact]
		public void Search_FiltersByCategory()
		{
			var inventory = CreateInventory();

			var economy = inventory.SearchAvailableVehicles(typeof(EconomyVehicle), Dec(1), Dec(5));
			var suv = inventory.SearchAvailableVehicles(typeof(SuvVehicle), Dec(1), Dec(5));

			Assert.Equal(new[] { "V001", "V003" }, economy.Select(v => v.Id).OrderBy(id => id).ToArray());
			Assert.Equal(new[] { "V002" }, suv.Select(v => v.Id).ToArray());
		}

		[Fact]
		public void Search_BaseVehicleType_ReturnsAllCategories()
		{
			var inventory = CreateInventory();

			var all = inventory.SearchAvailableVehicles(typeof(Vehicle), Dec(1), Dec(5));

			Assert.Equal(3, all.Count);
		}

		[Fact]
		public void Search_ExcludesVehicleBookedInOverlappingRange()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V001", "Alice", Dec(10), Dec(15));

			var result = inventory.SearchAvailableVehicles(typeof(EconomyVehicle), Dec(14), Dec(18));

			// V001 is booked -> only V003 remains
			var only = Assert.Single(result);
			Assert.Equal("V003", only.Id);
		}

		[Fact]
		public void Search_IncludesVehicleWhenBookingEndsExactlyAtSearchStart()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));

			var result = inventory.SearchAvailableVehicles(typeof(SuvVehicle), Dec(15), Dec(18));

			Assert.Single(result);
		}

		[Fact]
		public void Search_IncludesVehicleWhenBookingIsOutsideRange()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));

			var result = inventory.SearchAvailableVehicles(typeof(SuvVehicle), Dec(1), Dec(3));

			Assert.Single(result);
		}

		[Fact]
		public void Search_AllVehiclesOfCategoryBooked_ReturnsEmptyList()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));

			var result = inventory.SearchAvailableVehicles(typeof(SuvVehicle), Dec(12), Dec(13));

			Assert.Empty(result);
		}

		[Theory]
		[InlineData(5, 5)]  // end == start
		[InlineData(8, 5)]  // end before start
		public void Search_InvalidDateRange_Throws(int startDay, int endDay)
		{
			var inventory = CreateInventory();

			Assert.Throws<ArgumentException>(() =>
				inventory.SearchAvailableVehicles(typeof(EconomyVehicle), Dec(startDay), Dec(endDay)));
		}

		// create booking

		[Fact]
		public void CreateBooking_ValidRequest_ReturnsConfirmedBooking()
		{
			var inventory = CreateInventory();

			var booking = inventory.CreateBooking("BK001", "V001", "John Doe", Dec(1), Dec(5));

			Assert.Equal("BK001", booking.BookingId);
			Assert.Equal("V001", booking.VehicleId);
			Assert.Equal(BookingStatus.Confirmed, booking.Status);
		}

		[Fact]
		public void CreateBooking_UnknownVehicle_ThrowsVehicleNotFound()
		{
			var inventory = CreateInventory();

			Assert.Throws<VehicleNotFoundException>(() =>
				inventory.CreateBooking("BK001", "V999", "John Doe", Dec(1), Dec(5)));
		}

		[Fact]
		public void CreateBooking_OverlappingDates_ThrowsDoubleBookingError()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK002", "V002", "Alice Smith", Dec(10), Dec(15));

			var ex = Assert.Throws<DoubleBookingException>(() =>
				inventory.CreateBooking("BK003", "V002", "Bob Johnson", Dec(14), Dec(18)));

			Assert.Contains("Double-Booking", ex.Message);
			Assert.Contains("V002", ex.Message);
		}

		[Fact]
		public void CreateBooking_RejectedBooking_IsNotStored()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK002", "V002", "Alice Smith", Dec(10), Dec(15));
			Assert.Throws<DoubleBookingException>(() =>
				inventory.CreateBooking("BK003", "V002", "Bob Johnson", Dec(14), Dec(18)));

			var booking = inventory.CreateBooking("BK004", "V002", "Carol", Dec(15), Dec(18));

			Assert.Equal("BK004", booking.BookingId);
		}

		[Fact]
		public void CreateBooking_AdjacentDates_Succeeds()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));

			var booking = inventory.CreateBooking("BK002", "V002", "Bob", Dec(15), Dec(18));

			Assert.Equal(BookingStatus.Confirmed, booking.Status);
		}

		[Fact]
		public void CreateBooking_SameDatesDifferentVehicle_Succeeds()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V001", "Alice", Dec(10), Dec(15));

			var booking = inventory.CreateBooking("BK002", "V003", "Bob", Dec(10), Dec(15));

			Assert.Equal("V003", booking.VehicleId);
		}

		[Fact]
		public void CreateBooking_InvalidDateRange_ThrowsAndStoresNothing()
		{
			var inventory = CreateInventory();

			Assert.Throws<ArgumentException>(() =>
				inventory.CreateBooking("BK001", "V001", "Alice", Dec(15), Dec(10)));

			var result = inventory.SearchAvailableVehicles(typeof(EconomyVehicle), Dec(1), Dec(30));
			Assert.Equal(2, result.Count);
		}

		// get vehicle

		[Fact]
		public void GetVehicle_ExistingId_ReturnsThatVehicle()
		{
			var inventory = CreateInventory();

			var vehicle = inventory.GetVehicle("V002");

			Assert.IsType<SuvVehicle>(vehicle);
			Assert.Equal("Ford Everest", vehicle.Model);
		}

		[Fact]
		public void GetVehicle_UnknownId_ThrowsVehicleNotFound()
		{
			var inventory = CreateInventory();

			Assert.Throws<VehicleNotFoundException>(() => inventory.GetVehicle("V999"));
		}
	}
}