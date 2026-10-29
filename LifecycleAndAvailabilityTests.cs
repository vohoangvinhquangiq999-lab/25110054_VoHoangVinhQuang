using System;
using System.Linq;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Tuan 3 - tinh nang moi: huy booking, xe hong nang bi khoa den khi Repair(), kiem tra du lieu dau vao.
	public class LifecycleAndAvailabilityTests
	{
		private static DateTime Dec(int day) => new DateTime(2026, 12, day);

		private static InventoryService CreateInventory()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new EconomyVehicle("V001", "51H-123.45", "Toyota Vios", 40m, 15000));
			inventory.AddVehicle(new SuvVehicle("V002", "51K-999.99", "Ford Everest", 90m, 8000));
			return inventory;
		}

		// ---- Cancel ----

		[Fact]
		public void Cancel_ConfirmedBooking_BecomesCancelled()
		{
			var booking = new Booking("BK001", "V001", "Alice", Dec(10), Dec(15));

			booking.Cancel();

			Assert.Equal(BookingStatus.Cancelled, booking.Status);
		}

		[Fact]
		public void Cancel_AlreadyCancelled_Throws()
		{
			var booking = new Booking("BK001", "V001", "Alice", Dec(10), Dec(15));
			booking.Cancel();

			Assert.Throws<InvalidBookingStateException>(() => booking.Cancel());
		}

		[Fact]
		public void Cancel_CompletedBooking_ThrowsAndStaysCompleted()
		{
			var booking = new Booking("BK001", "V001", "Alice", Dec(10), Dec(15));
			booking.CompleteBooking();

			Assert.Throws<InvalidBookingStateException>(() => booking.Cancel());
			Assert.Equal(BookingStatus.Completed, booking.Status);
		}

		[Fact]
		public void Complete_CancelledBooking_Throws()
		{
			var booking = new Booking("BK001", "V001", "Alice", Dec(10), Dec(15));
			booking.Cancel();

			Assert.Throws<InvalidBookingStateException>(() => booking.CompleteBooking());
		}

		[Fact]
		public void CancelBooking_FreesTheDatesForAnotherCustomer()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));

			inventory.CancelBooking("BK001");

			Assert.Single(inventory.SearchAvailableVehicles(typeof(SuvVehicle), Dec(12), Dec(13)));
			var again = inventory.CreateBooking("BK002", "V002", "Bob", Dec(10), Dec(15));
			Assert.Equal(BookingStatus.Confirmed, again.Status);
		}

		[Fact]
		public void CancelBooking_UnknownId_ThrowsBookingNotFound()
		{
			var inventory = CreateInventory();

			Assert.Throws<BookingNotFoundException>(() => inventory.CancelBooking("NOPE"));
		}

		// ---- Xe hong nang ----

		[Fact]
		public void Vehicle_MajorDamage_IsNotRentable_UntilRepaired()
		{
			var car = new EconomyVehicle("V001", "p", "m", 40m, 100, VehicleCondition.MajorDamage);
			Assert.False(car.IsRentable);

			car.Repair();

			Assert.True(car.IsRentable);
			Assert.Equal(VehicleCondition.Excellent, car.Condition);
			Assert.Equal(100, car.Mileage); // sua chua khong dong cham toi so km
		}

		[Fact]
		public void Vehicle_MinorDamage_IsStillRentable()
		{
			var car = new EconomyVehicle("V001", "p", "m", 40m, 100, VehicleCondition.MinorDamage);

			Assert.True(car.IsRentable);
		}

		[Fact]
		public void CreateBooking_VehicleWithMajorDamage_ThrowsVehicleUnavailable()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new SuvVehicle("V002", "p", "m", 90m, 0, VehicleCondition.MajorDamage));

			Assert.Throws<VehicleUnavailableException>(() =>
				inventory.CreateBooking("BK001", "V002", "Alice", Dec(1), Dec(3)));
		}

		[Fact]
		public void Search_ExcludesVehicleWithMajorDamage()
		{
			var inventory = new InventoryService();
			inventory.AddVehicle(new SuvVehicle("V002", "p", "m", 90m, 0, VehicleCondition.MajorDamage));
			inventory.AddVehicle(new SuvVehicle("V003", "p", "m", 95m, 0));

			var result = inventory.SearchAvailableVehicles(typeof(SuvVehicle), Dec(1), Dec(3));

			Assert.Equal("V003", Assert.Single(result).Id);
		}

		[Fact]
		public void ReturnWithMajorDamage_LocksVehicle_ThenRepairUnlocksIt()
		{
			var inventory = CreateInventory();
			var billing = new BillingService();
			var eco = (EconomyVehicle)inventory.GetVehicle("V001");
			var booking = inventory.CreateBooking("BK001", "V001", "John", Dec(1), Dec(3));

			billing.GenerateBill(booking, eco, Dec(3), 15500, VehicleCondition.MajorDamage);

			Assert.Throws<VehicleUnavailableException>(() =>
				inventory.CreateBooking("BK002", "V001", "Mary", Dec(10), Dec(12)));

			eco.Repair();
			var next = inventory.CreateBooking("BK002", "V001", "Mary", Dec(10), Dec(12));
			Assert.Equal("BK002", next.BookingId);
		}

		// ---- Validate dau vao ----

		[Fact]
		public void Vehicle_NegativeMileage_Throws()
		{
			Assert.Throws<ArgumentException>(() => new EconomyVehicle("V001", "p", "m", 40m, -1));
		}

		[Theory]
		[InlineData("", "V001", "Alice")]
		[InlineData("BK001", " ", "Alice")]
		[InlineData("BK001", "V001", "")]
		public void Booking_EmptyTextField_Throws(string bookingId, string vehicleId, string customer)
		{
			Assert.Throws<ArgumentException>(() => new Booking(bookingId, vehicleId, customer, Dec(1), Dec(2)));
		}

		[Fact]
		public void AddVehicle_DuplicateId_Throws()
		{
			var inventory = CreateInventory();

			Assert.Throws<ArgumentException>(() =>
				inventory.AddVehicle(new SuvVehicle("V001", "p", "m", 10m, 0)));
			Assert.Equal(2, inventory.Vehicles.Count);
		}

		[Fact]
		public void AddVehicle_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new InventoryService().AddVehicle(null));
		}

		[Fact]
		public void CreateBooking_DuplicateBookingId_ThrowsAndStoresNothing()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V001", "Alice", Dec(1), Dec(3));

			Assert.Throws<ArgumentException>(() =>
				inventory.CreateBooking("BK001", "V002", "Bob", Dec(1), Dec(3)));
			Assert.Single(inventory.Bookings);
		}

		[Fact]
		public void Collections_AreReadOnlyViews()
		{
			var inventory = CreateInventory();

			Assert.False(inventory.Vehicles is System.Collections.Generic.List<Vehicle>);
			Assert.Equal(2, inventory.Vehicles.Count);
			Assert.Empty(inventory.Bookings);
		}

		[Fact]
		public void DoubleBookingException_ExposesVehicleId()
		{
			var inventory = CreateInventory();
			inventory.CreateBooking("BK001", "V002", "Alice", Dec(10), Dec(15));

			var ex = Assert.Throws<DoubleBookingException>(() =>
				inventory.CreateBooking("BK002", "V002", "Bob", Dec(14), Dec(18)));

			Assert.Equal("V002", ex.VehicleId);
		}
	}
}
