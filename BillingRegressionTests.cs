using System;
using Xunit;

namespace VehicleRentalSystem.Tests
{
	/// Tuan 3 - refactoring "validate-then-commit" cua BillingService.GenerateBill
	/// va cac ca bien cua cach tinh so ngay (lam tron len theo ngay bat dau).
	/// Moi test tu tao doi tuong rieng nen doc lap va lap lai duoc.
	public class BillingRegressionTests
	{
		private static DateTime Day(int month, int day) => new DateTime(2026, month, day);

		private static (InventoryService inv, BillingService billing, EconomyVehicle eco, SuvVehicle suv) Setup()
		{
			var inv = new InventoryService();
			var eco = new EconomyVehicle("V001", "51H-123.45", "Toyota Vios", 40m, 15000);
			var suv = new SuvVehicle("V002", "51K-999.99", "Ford Everest", 90m, 8000);
			inv.AddVehicle(eco);
			inv.AddVehicle(suv);
			return (inv, new BillingService(), eco, suv);
		}

		// --- Loi cu: lan tra xe thu 2 bi tu choi nhung xe van bi ghi de ---

		[Fact]
		public void GenerateBill_SecondCallRejected_DoesNotChangeVehicle()
		{
			var (inv, billing, eco, _) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John", Day(11, 1), Day(11, 5));
			billing.GenerateBill(booking, eco, Day(11, 5), 15400, VehicleCondition.Excellent);

			Assert.Throws<InvalidBookingStateException>(() =>
				billing.GenerateBill(booking, eco, Day(11, 5), 99999, VehicleCondition.MajorDamage));

			// xe phai giu nguyen trang thai cua lan tra thanh cong dau tien
			Assert.Equal(15400, eco.Mileage);
			Assert.Equal(VehicleCondition.Excellent, eco.Condition);
		}

		[Fact]
		public void GenerateBill_CancelledBooking_ThrowsAndDoesNotChangeVehicle()
		{
			var (inv, billing, eco, _) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John", Day(11, 1), Day(11, 5));
			inv.CancelBooking("BK001");

			Assert.Throws<InvalidBookingStateException>(() =>
				billing.GenerateBill(booking, eco, Day(11, 5), 15400, VehicleCondition.MinorDamage));

			Assert.Equal(15000, eco.Mileage);
			Assert.Equal(VehicleCondition.Excellent, eco.Condition);
		}

		// --- Loi cu: tinh tien bang xe khong thuoc booking ---

		[Fact]
		public void GenerateBill_VehicleDoesNotMatchBooking_ThrowsAndChangesNothing()
		{
			var (inv, billing, eco, suv) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John", Day(11, 1), Day(11, 5));

			Assert.Throws<ArgumentException>(() =>
				billing.GenerateBill(booking, suv, Day(11, 5), 8100, VehicleCondition.Excellent));

			Assert.Equal(BookingStatus.Confirmed, booking.Status);
			Assert.Equal(8000, suv.Mileage);
			Assert.Equal(15000, eco.Mileage);
		}

		[Fact]
		public void GenerateBill_ReturnBeforeStart_Throws()
		{
			var (inv, billing, eco, _) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John", Day(11, 10), Day(11, 15));

			Assert.Throws<ArgumentException>(() =>
				billing.GenerateBill(booking, eco, Day(11, 9), 15100, VehicleCondition.Excellent));
			Assert.Equal(BookingStatus.Confirmed, booking.Status);
		}

		[Fact]
		public void GenerateBill_NullArguments_Throw()
		{
			var (inv, billing, eco, _) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John", Day(11, 1), Day(11, 5));

			Assert.Throws<ArgumentNullException>(() => billing.GenerateBill(null, eco, Day(11, 5), 15100, VehicleCondition.Excellent));
			Assert.Throws<ArgumentNullException>(() => billing.GenerateBill(booking, null, Day(11, 5), 15100, VehicleCondition.Excellent));
		}

		// --- PreviewBill: chi tinh, khong ghi ---

		[Fact]
		public void PreviewBill_ReturnsSameAmountAsGenerateBill_WithoutSideEffects()
		{
			var (inv, billing, eco, _) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John", Day(11, 1), Day(11, 5));

			var preview = billing.PreviewBill(booking, eco, Day(11, 6), 15400, VehicleCondition.MinorDamage);

			Assert.Equal(308m, preview.TotalAmount);
			Assert.Equal(BookingStatus.Confirmed, booking.Status);
			Assert.Equal(15000, eco.Mileage);

			var real = billing.GenerateBill(booking, eco, Day(11, 6), 15400, VehicleCondition.MinorDamage);
			Assert.Equal(preview.TotalAmount, real.TotalAmount);
		}

		// --- Cach tinh ngay: moi ngay bat dau tinh la 1 ngay ---

		[Fact]
		public void GenerateBill_ReturnedFiveHoursLate_ChargedOneLateDay()
		{
			var (inv, billing, _, suv) = Setup();
			var booking = inv.CreateBooking("BK001", "V002", "Jane", Day(12, 1), Day(12, 3));

			var bill = billing.GenerateBill(booking, suv, new DateTime(2026, 12, 3, 5, 0, 0), 8100, VehicleCondition.Excellent);

			Assert.Equal(135m, bill.LateFee); // 1 ngay * 90 * 1.5
		}

		[Fact]
		public void GenerateBill_ReturnedExactlyOneDayLate_ChargedOneDayNotTwo()
		{
			var (inv, billing, _, suv) = Setup();
			var booking = inv.CreateBooking("BK001", "V002", "Jane", Day(12, 1), Day(12, 3));

			var bill = billing.GenerateBill(booking, suv, Day(12, 4), 8100, VehicleCondition.Excellent);

			Assert.Equal(135m, bill.LateFee);
		}

		[Fact]
		public void GenerateBill_ReturnedOneDayAndOneHourLate_ChargedTwoDays()
		{
			var (inv, billing, _, suv) = Setup();
			var booking = inv.CreateBooking("BK001", "V002", "Jane", Day(12, 1), Day(12, 3));

			var bill = billing.GenerateBill(booking, suv, new DateTime(2026, 12, 4, 1, 0, 0), 8100, VehicleCondition.Excellent);

			Assert.Equal(270m, bill.LateFee);
		}

		[Fact]
		public void GenerateBill_BookingOfThirtySixHours_BilledAsTwoDays()
		{
			var (inv, billing, eco, _) = Setup();
			var booking = inv.CreateBooking("BK001", "V001", "John",
				new DateTime(2026, 11, 1, 8, 0, 0), new DateTime(2026, 11, 2, 20, 0, 0));

			var bill = billing.GenerateBill(booking, eco, new DateTime(2026, 11, 2, 20, 0, 0), 15100, VehicleCondition.Excellent);

			Assert.Equal(80m, bill.BaseCost);
		}

		// --- Decimal: khong sai so thap phan ---

		[Fact]
		public void Money_UsesDecimal_NoFloatingPointError()
		{
			var car = new EconomyVehicle("V001", "p", "m", 33.33m, 0);

			Assert.Equal(119.988m, car.CalculateLateFee(3)); // 3 * 33.33 * 1.2 chinh xac
		}

		// --- Bill: bat bien + hoa don ---

		[Fact]
		public void Bill_NegativeFee_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Bill("BK001", -1m, 0m, 0m));
		}

		[Fact]
		public void Bill_EmptyBookingId_Throws()
		{
			Assert.Throws<ArgumentException>(() => new Bill(" ", 1m, 0m, 0m));
		}

		[Fact]
		public void Bill_ToInvoiceText_ContainsAllAmountsWithTwoDecimals()
		{
			var text = new Bill("BK001", 160m, 48m, 100m).ToInvoiceText();

			Assert.Contains("BK001", text);
			Assert.Contains("160.00", text);
			Assert.Contains("48.00", text);
			Assert.Contains("100.00", text);
			Assert.Contains("308.00", text);
		}
	}
}
