using System;

namespace VehicleRentalSystem
{
	// abstract class vehicle
	public abstract class Vehicle
	{
		public string Id { get; }
		public string LicensePlate { get; }
		public string Model { get; }
		public decimal DailyRate { get; protected set; }
		public int Mileage { get; private set; }
		public VehicleCondition Condition { get; private set; }

		// xe hong nang thi tam ngung cho thue cho den khi Repair()
		public bool IsRentable => Condition != VehicleCondition.MajorDamage;

		protected Vehicle(string id, string licensePlate, string model, decimal dailyRate, int mileage, VehicleCondition condition)
		{
			if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Vehicle ID cannot be empty.");
			if (dailyRate <= 0) throw new ArgumentException("Daily rate must be positive.");
			if (mileage < 0) throw new ArgumentException("Mileage cannot be negative.");

			Id = id;
			LicensePlate = licensePlate;
			Model = model;
			DailyRate = dailyRate;
			Mileage = mileage;
			Condition = condition;
		}

		// Kiem tra ma KHONG thay doi trang thai: BillingService goi truoc khi ghi bat ky thu gi (validate-then-commit)
		public void EnsureMileageNotDecreasing(int newMileage)
		{
			if (newMileage < Mileage)
				throw new InvalidOdometerException(newMileage, Mileage);
		}

		// encapsulation: Protects state invariant for mileage and condition
		public void UpdateAfterRental(int newMileage, VehicleCondition returnCondition)
		{
			EnsureMileageNotDecreasing(newMileage);

			Mileage = newMileage;
			Condition = returnCondition;
		}

		// Sua chua xong: dua xe ve tinh trang tot (khong dung toi Mileage)
		public void Repair()
		{
			Condition = VehicleCondition.Excellent;
		}

		// polymorphism: fee Calculation methods
		public abstract decimal CalculateLateFee(int extraDays);
		public abstract decimal CalculateDamageFee(VehicleCondition returnCondition);
	}
}
