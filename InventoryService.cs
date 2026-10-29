using System;
using System.Collections.Generic;
using System.Linq;

namespace VehicleRentalSystem
{
	// inventory service: quan ly kho xe + lich dat xe (tim xe trong, chong dat trung)
	public class InventoryService
	{
		private readonly List<Vehicle> _vehicles = new();
		private readonly List<Booking> _bookings = new();

		// Chi doc: ben ngoai khong the Add/Remove truc tiep vao danh sach noi bo
		public IReadOnlyList<Vehicle> Vehicles => _vehicles.AsReadOnly();
		public IReadOnlyList<Booking> Bookings => _bookings.AsReadOnly();

		public void AddVehicle(Vehicle vehicle)
		{
			if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
			if (_vehicles.Any(v => v.Id == vehicle.Id))
				throw new ArgumentException($"Vehicle ID '{vehicle.Id}' already exists.");

			_vehicles.Add(vehicle);
		}

		public List<Vehicle> SearchAvailableVehicles(Type categoryType, DateTime start, DateTime end)
		{
			if (end <= start)
				throw new ArgumentException("End date must be strictly after start date.");

			return _vehicles
				.Where(v => categoryType.IsInstanceOfType(v))
				.Where(v => v.IsRentable)
				.Where(v => !_bookings.Any(b => b.VehicleId == v.Id && b.OverlapsWith(start, end)))
				.ToList();
		}

		public Booking CreateBooking(string bookingId, string vehicleId, string customerName, DateTime start, DateTime end)
		{
			var vehicle = GetVehicle(vehicleId);

			if (_bookings.Any(b => b.BookingId == bookingId))
				throw new ArgumentException($"Booking ID '{bookingId}' already exists.");

			if (!vehicle.IsRentable)
				throw new VehicleUnavailableException(vehicleId);

			// check for double booking
			if (_bookings.Any(b => b.VehicleId == vehicleId && b.OverlapsWith(start, end)))
				throw new DoubleBookingException(vehicleId, start, end);

			var booking = new Booking(bookingId, vehicleId, customerName, start, end);
			_bookings.Add(booking);
			return booking;
		}

		// Tuan 3: huy booking -> lich duoc nha ra (OverlapsWith bo qua booking Cancelled)
		public void CancelBooking(string bookingId)
		{
			GetBooking(bookingId).Cancel();
		}

		public Vehicle GetVehicle(string vehicleId)
		{
			return _vehicles.FirstOrDefault(v => v.Id == vehicleId)
				?? throw new VehicleNotFoundException(vehicleId);
		}

		public Booking GetBooking(string bookingId)
		{
			return _bookings.FirstOrDefault(b => b.BookingId == bookingId)
				?? throw new BookingNotFoundException(bookingId);
		}

		// Dung khi nap du lieu tu file: them booking co san (khong tao moi), van kiem tra xe ton tai + khong trung lich
		internal void RestoreBooking(Booking booking)
		{
			GetVehicle(booking.VehicleId);

			if (_bookings.Any(b => b.BookingId == booking.BookingId))
				throw new ArgumentException($"Booking ID '{booking.BookingId}' already exists.");

			if (_bookings.Any(b => b.VehicleId == booking.VehicleId && b.OverlapsWith(booking.StartDate, booking.EndDate)
				&& booking.Status != BookingStatus.Cancelled))
				throw new DoubleBookingException(booking.VehicleId, booking.StartDate, booking.EndDate);

			_bookings.Add(booking);
		}
	}
}
