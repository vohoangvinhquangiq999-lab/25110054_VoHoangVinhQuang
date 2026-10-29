using System;

namespace VehicleRentalSystem
{
	// encapsulation booking class: tu quan ly vong doi Confirmed -> Completed | Cancelled
	public class Booking
	{
		public string BookingId { get; }
		public string VehicleId { get; }
		public string CustomerName { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public BookingStatus Status { get; private set; }

		public Booking(string bookingId, string vehicleId, string customerName, DateTime startDate, DateTime endDate)
		{
			if (string.IsNullOrWhiteSpace(bookingId)) throw new ArgumentException("Booking ID cannot be empty.");
			if (string.IsNullOrWhiteSpace(vehicleId)) throw new ArgumentException("Vehicle ID cannot be empty.");
			if (string.IsNullOrWhiteSpace(customerName)) throw new ArgumentException("Customer name cannot be empty.");
			if (endDate <= startDate)
				throw new ArgumentException("Invalid Range: End date must be strictly after start date.");

			BookingId = bookingId;
			VehicleId = vehicleId;
			CustomerName = customerName;
			StartDate = startDate;
			EndDate = endDate;
			Status = BookingStatus.Confirmed;
		}

		// Chi dung khi doc lai tu file: dung lai booking voi trang thai da luu (van qua constructor nen van kiem tra ngay thang)
		internal static Booking Restore(string bookingId, string vehicleId, string customerName,
			DateTime startDate, DateTime endDate, BookingStatus status)
		{
			var booking = new Booking(bookingId, vehicleId, customerName, startDate, endDate);
			booking.Status = status;
			return booking;
		}

		// overlap detection algorithm: hai khoang nua mo [start, end) giao nhau <=> StartA < EndB && EndA > StartB
		public bool OverlapsWith(DateTime start, DateTime end)
		{
			if (Status == BookingStatus.Cancelled) return false;
			return StartDate < end && EndDate > start;
		}

		// Kiem tra ma KHONG doi trang thai (BillingService goi truoc khi ghi)
		public void EnsureCanComplete()
		{
			if (Status != BookingStatus.Confirmed)
				throw new InvalidBookingStateException("Only confirmed bookings can be completed.");
		}

		public void CompleteBooking()
		{
			EnsureCanComplete();
			Status = BookingStatus.Completed;
		}

		// Tuan 3: huy booking (truoc day enum Cancelled ton tai nhung khong co duong nao dat duoc)
		public void Cancel()
		{
			if (Status != BookingStatus.Confirmed)
				throw new InvalidBookingStateException("Only confirmed bookings can be cancelled.");

			Status = BookingStatus.Cancelled;
		}
	}
}
