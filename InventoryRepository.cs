using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VehicleRentalSystem
{
	// Tuan 3: luu / nap kho xe + lich dat xe ra file JSON.
	// Tach rieng khoi InventoryService (SRP): InventoryService khong biet gi ve file.
	public static class InventoryRepository
	{
		private static readonly JsonSerializerOptions Options = new()
		{
			WriteIndented = true,
			Converters = { new JsonStringEnumConverter() }
		};

		// DTO (data transfer object): chi de doc/ghi JSON, KHONG phai lop nghiep vu.
		// Truong thieu trong file cu se nhan gia tri mac dinh => them truong moi khong lam hong du lieu cu.
		private class VehicleData
		{
			public string Type { get; set; } = "";   // ten lop, vd "EconomyVehicle"
			public string Id { get; set; } = "";
			public string LicensePlate { get; set; } = "";
			public string Model { get; set; } = "";
			public decimal DailyRate { get; set; }
			public int Mileage { get; set; }
			public VehicleCondition Condition { get; set; } = VehicleCondition.Excellent;
		}

		private class BookingData
		{
			public string BookingId { get; set; } = "";
			public string VehicleId { get; set; } = "";
			public string CustomerName { get; set; } = "";
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
		}

		private class InventoryData
		{
			public List<VehicleData> Vehicles { get; set; } = new();
			public List<BookingData> Bookings { get; set; } = new();
		}

		public static void Save(InventoryService inventory, string path)
		{
			var data = new InventoryData
			{
				Vehicles = inventory.Vehicles.Select(v => new VehicleData
				{
					Type = v.GetType().Name,
					Id = v.Id,
					LicensePlate = v.LicensePlate,
					Model = v.Model,
					DailyRate = v.DailyRate,
					Mileage = v.Mileage,
					Condition = v.Condition
				}).ToList(),
				Bookings = inventory.Bookings.Select(b => new BookingData
				{
					BookingId = b.BookingId,
					VehicleId = b.VehicleId,
					CustomerName = b.CustomerName,
					StartDate = b.StartDate,
					EndDate = b.EndDate,
					Status = b.Status
				}).ToList()
			};

			try
			{
				File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new PersistenceException($"Cannot write file '{path}'.", ex);
			}
		}

		public static InventoryService Load(string path)
		{
			InventoryData data;
			try
			{
				data = JsonSerializer.Deserialize<InventoryData>(File.ReadAllText(path), Options)
					?? throw new PersistenceException($"File '{path}' is empty.");
			}
			catch (JsonException ex)
			{
				throw new PersistenceException($"File '{path}' is not valid inventory data.", ex);
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new PersistenceException($"Cannot read file '{path}'.", ex);
			}

			var inventory = new InventoryService();
			try
			{
				foreach (var v in data.Vehicles) inventory.AddVehicle(CreateVehicle(v));
				foreach (var b in data.Bookings)
					inventory.RestoreBooking(Booking.Restore(b.BookingId, b.VehicleId, b.CustomerName, b.StartDate, b.EndDate, b.Status));
			}
			catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException)
			{
				// du lieu trong file vi pham quy tac nghiep vu (rate <= 0, booking trung lich, xe khong ton tai...)
				throw new PersistenceException($"File '{path}' contains invalid data: {ex.Message}", ex);
			}

			return inventory;
		}

		// Tim lop con cua Vehicle theo ten bang reflection => THEM LOAI XE MOI KHONG CAN SUA FILE NAY (Open/Closed).
		// Quy uoc: lop con phai nam cung assembly voi Vehicle va co constructor (id, plate, model, rate, mileage, condition).
		private static Vehicle CreateVehicle(VehicleData d)
		{
			Type type = typeof(Vehicle).Assembly.GetTypes()
				.FirstOrDefault(t => !t.IsAbstract && typeof(Vehicle).IsAssignableFrom(t) && t.Name == d.Type);

			if (type == null)
				throw new PersistenceException($"Unknown vehicle type '{d.Type}' for vehicle '{d.Id}'.");

			try
			{
				return (Vehicle)Activator.CreateInstance(type, d.Id, d.LicensePlate, d.Model, d.DailyRate, d.Mileage, d.Condition);
			}
			catch (TargetInvocationException ex) when (ex.InnerException != null)
			{
				// constructor nem ArgumentException (vd rate <= 0): tra loi goc ra ngoai thay vi loi bao boc cua reflection
				throw ex.InnerException;
			}
		}
	}
}
