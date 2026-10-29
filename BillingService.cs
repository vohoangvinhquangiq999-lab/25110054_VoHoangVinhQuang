using System;

namespace VehicleRentalSystem
{
	// billing Service: Handles financial transactions and check-in return calculations
	public class BillingService
	{
		private const long TicksPerDay = TimeSpan.TicksPerDay;

		// Tinh hoa don, KHONG thay doi xe hay booking (de xem truoc / de test cong thuc rieng)
		public Bill PreviewBill(Booking booking, Vehicle vehicle, DateTime actualReturnDate, int returnMileage, VehicleCondition returnCondition)
		{
			ValidateReturn(booking, vehicle, actualReturnDate, returnMileage);

			// 1. base cost: moi ngay BAT DAU tinh la 1 ngay, toi thieu 1 ngay
			int totalDays = Math.Max(1, BillableDays(booking.EndDate - booking.StartDate));
			decimal baseCost = totalDays * vehicle.DailyRate;

			// 2. late fee: tra tre bao nhieu ngay (tinh ca ngay le), tra som/dung han = 0
			int extraDays = actualReturnDate > booking.EndDate ? BillableDays(actualReturnDate - booking.EndDate) : 0;
			decimal lateFee = vehicle.CalculateLateFee(extraDays);

			// 3. damage fee
			decimal damageFee = vehicle.CalculateDamageFee(returnCondition);

			return new Bill(booking.BookingId, baseCost, lateFee, damageFee);
		}

		// Tra xe: validate TAT CA truoc -> tinh tien -> cuoi cung moi ghi trang thai (validate-then-commit)
		public Bill GenerateBill(Booking booking, Vehicle vehicle, DateTime actualReturnDate, int returnMileage, VehicleCondition returnCondition)
		{
			Bill bill = PreviewBill(booking, vehicle, actualReturnDate, returnMileage, returnCondition);

			// 4. commit: tu day khong con dieu kien nao co the nem loi (da kiem tra o ValidateReturn)
			vehicle.UpdateAfterRental(returnMileage, returnCondition);
			booking.CompleteBooking();

			return bill;
		}

		private static void ValidateReturn(Booking booking, Vehicle vehicle, DateTime actualReturnDate, int returnMileage)
		{
			if (booking == null) throw new ArgumentNullException(nameof(booking));
			if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));

			if (booking.VehicleId != vehicle.Id)
				throw new ArgumentException($"Booking '{booking.BookingId}' belongs to vehicle '{booking.VehicleId}', not '{vehicle.Id}'.");

			if (actualReturnDate < booking.StartDate)
				throw new ArgumentException("Return date cannot be before the rental start date.");

			booking.EnsureCanComplete();
			vehicle.EnsureMileageNotDecreasing(returnMileage);
		}

		// lam tron LEN theo ngay bang so nguyen (khong dung double): 25 gio -> 2 ngay, 1 gio -> 1 ngay
		private static int BillableDays(TimeSpan span)
		{
			if (span <= TimeSpan.Zero) return 0;
			return (int)((span.Ticks + TicksPerDay - 1) / TicksPerDay);
		}
	}
}
