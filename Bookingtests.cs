using System;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Booking controls its own lifecycle (encapsulation) and owns the overlap algorithm.
	/// Overlap rule for half-open ranges [start, end):
	///   two ranges overlap
	public class BookingTests
	{
		private static DateTime Dec(int day) => new DateTime(2026, 12, day);

		// reference booking used by the overlap tests
		private static Booking CreateReferenceBooking()
			=> new Booking("BK001", "V001", "Alice", Dec(10), Dec(15));

		// constructor/date-range invariant

		[Fact]
		public void Constructor_EndEqualsStart_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
				new Booking("BK001", "V001", "Alice", Dec(10), Dec(10)));
		}

		[Fact]
		public void Constructor_EndBeforeStart_Throws()
		{
			var ex = Assert.Throws<ArgumentException>(() =>
				new Booking("BK001", "V001", "Alice", Dec(15), Dec(10)));

			Assert.Contains("Invalid Range", ex.Message);
		}

		[Fact]
		public void Constructor_ValidRange_StartsAsConfirmed()
		{
			var booking = CreateReferenceBooking();

			Assert.Equal("BK001", booking.BookingId);
			Assert.Equal("V001", booking.VehicleId);
			Assert.Equal("Alice", booking.CustomerName);
			Assert.Equal(Dec(10), booking.StartDate);
			Assert.Equal(Dec(15), booking.EndDate);
			Assert.Equal(BookingStatus.Confirmed, booking.Status);
		}

		// Lifecycle: confirmed -> completed (only once)

		[Fact]
		public void CompleteBooking_WhenConfirmed_BecomesCompleted()
		{
			var booking = CreateReferenceBooking();

			booking.CompleteBooking();

			Assert.Equal(BookingStatus.Completed, booking.Status);
		}

		[Fact]
		public void CompleteBooking_CalledTwice_ThrowsAndStaysCompleted()
		{
			var booking = CreateReferenceBooking();
			booking.CompleteBooking();

			Assert.Throws<InvalidOperationException>(() => booking.CompleteBooking());

			Assert.Equal(BookingStatus.Completed, booking.Status);
		}

		//  Overlap detection
		

		[Theory]
		[InlineData(14, 18)] // overlaps the end of the reference booking
		[InlineData(8, 12)]  // overlaps the start of the reference booking
		[InlineData(11, 13)] // fully inside
		[InlineData(5, 20)]  // fully contains the reference booking
		[InlineData(10, 15)] // identical range
		public void OverlapsWith_OverlappingRange_ReturnsTrue(int startDay, int endDay)
		{
			var booking = CreateReferenceBooking();

			Assert.True(booking.OverlapsWith(Dec(startDay), Dec(endDay)));
		}

		[Theory]
		[InlineData(15, 18)] // starts exactly when the reference ends
		[InlineData(5, 10)]  // ends exactly when the reference starts
		[InlineData(1, 3)]   // completely before
		[InlineData(20, 25)] // completely after
		public void OverlapsWith_NonOverlappingRange_ReturnsFalse(int startDay, int endDay)
		{
			var booking = CreateReferenceBooking();

			Assert.False(booking.OverlapsWith(Dec(startDay), Dec(endDay)));
		}

		[Fact]
		public void OverlapsWith_IsSymmetric()
		{
			var a = new Booking("A", "V001", "Alice", Dec(10), Dec(15));
			var b = new Booking("B", "V001", "Bob", Dec(14), Dec(18));

			Assert.Equal(
				a.OverlapsWith(b.StartDate, b.EndDate),
				b.OverlapsWith(a.StartDate, a.EndDate));
		}
	}
}