namespace VehicleRentalSystem
{
	//economy vehicle
	public class EconomyVehicle : Vehicle
	{
		public EconomyVehicle(string id, string licensePlate, string model, decimal dailyRate, int mileage, VehicleCondition condition = VehicleCondition.Excellent)
			: base(id, licensePlate, model, dailyRate, mileage, condition) { }

		public override decimal CalculateLateFee(int extraDays)
		{
			if (extraDays <= 0) return 0m;
			return extraDays * DailyRate * 1.2m; // 20% penalty
		}

		public override decimal CalculateDamageFee(VehicleCondition returnCondition)
		{
			return returnCondition switch
			{
				VehicleCondition.MinorDamage => 100m,
				VehicleCondition.MajorDamage => 350m,
				_ => 0m
			};
		}
	}
}
