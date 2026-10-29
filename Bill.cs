using System;
using System.Globalization;

namespace VehicleRentalSystem
{
	// invoice: bat bien (immutable) - tao xong khong ai sua duoc so tien
	public class Bill
	{
		public string BookingId { get; }
		public decimal BaseCost { get; }
		public decimal LateFee { get; }
		public decimal DamageFee { get; }
		public decimal TotalAmount => BaseCost + LateFee + DamageFee;

		public Bill(string bookingId, decimal baseCost, decimal lateFee, decimal damageFee)
		{
			if (string.IsNullOrWhiteSpace(bookingId)) throw new ArgumentException("Booking ID cannot be empty.");
			if (baseCost < 0 || lateFee < 0 || damageFee < 0) throw new ArgumentException("Fees cannot be negative.");

			BookingId = bookingId;
			BaseCost = baseCost;
			LateFee = lateFee;
			DamageFee = damageFee;
		}

		// Tra ve chuoi (de test duoc), khong ghi thang ra Console; dung InvariantCulture de luon in "160.00" thay vi "160,00"
		public string ToInvoiceText()
		{
			return string.Join(Environment.NewLine,
				FormattableString.Invariant($"         INVOICE FOR BOOKING: {BookingId}"),
				FormattableString.Invariant($" Base Rental Fee : ${BaseCost,10:F2}"),
				FormattableString.Invariant($" Late Return Fee : ${LateFee,10:F2}"),
				FormattableString.Invariant($" Damage Charge   : ${DamageFee,10:F2}"),
				FormattableString.Invariant($" TOTAL AMOUNT    : ${TotalAmount,10:F2}"));
		}
	}
}
