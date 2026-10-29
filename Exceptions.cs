using System;
using System.Collections.Generic;

namespace VehicleRentalSystem
{
	// Tuan 3: exception rieng cho tung loi nghiep vu.
	// Moi lop deu ke thua tu exception chuan nen code cu van catch duoc bang kieu cha.

	/// <summary>Xe da co lich dat trung khoang thoi gian.</summary>
	public class DoubleBookingException : InvalidOperationException
	{
		public string VehicleId { get; }

		public DoubleBookingException(string vehicleId, DateTime start, DateTime end)
			: base($"Double-Booking Error: Vehicle '{vehicleId}' is already booked during {start:yyyy-MM-dd} to {end:yyyy-MM-dd}.")
		{
			VehicleId = vehicleId;
		}
	}

	/// <summary>Xe khong ton tai trong kho.</summary>
	public class VehicleNotFoundException : KeyNotFoundException
	{
		public string VehicleId { get; }

		public VehicleNotFoundException(string vehicleId)
			: base($"Vehicle with ID {vehicleId} not found.")
		{
			VehicleId = vehicleId;
		}
	}

	/// <summary>Booking khong ton tai.</summary>
	public class BookingNotFoundException : KeyNotFoundException
	{
		public string BookingId { get; }

		public BookingNotFoundException(string bookingId)
			: base($"Booking with ID {bookingId} not found.")
		{
			BookingId = bookingId;
		}
	}

	/// <summary>Chuyen trang thai booking khong hop le (vd: hoan tat 2 lan, huy booking da hoan tat).</summary>
	public class InvalidBookingStateException : InvalidOperationException
	{
		public InvalidBookingStateException(string message) : base(message) { }
	}

	/// <summary>So km tra xe nho hon so km hien tai (tua nguoc cong-to-met).</summary>
	public class InvalidOdometerException : ArgumentException
	{
		public InvalidOdometerException(int newMileage, int currentMileage)
			: base($"Invalid Odometer: New mileage ({newMileage}) cannot be less than current mileage ({currentMileage}).")
		{
		}
	}

	/// <summary>Xe dang hong nang, khong duoc cho thue cho den khi sua chua.</summary>
	public class VehicleUnavailableException : InvalidOperationException
	{
		public VehicleUnavailableException(string vehicleId)
			: base($"Vehicle '{vehicleId}' is out of service (major damage) and cannot be rented until repaired.")
		{
		}
	}

	/// <summary>Loi doc/ghi file du lieu (file hong, loai xe khong biet, ...).</summary>
	public class PersistenceException : Exception
	{
		public PersistenceException(string message, Exception inner = null) : base(message, inner) { }
	}
}
