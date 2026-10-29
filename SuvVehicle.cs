namespace VehicleRentalSystem
{
	//suv vehicle
	public class SuvVehicle : Vehicle
	{
		public SuvVehicle(string id, string licensePlate, string model, decimal dailyRate, int mileage, VehicleCondition condition = VehicleCondition.Excellent)
			: base(id, licensePlate, model, dailyRate, mileage, condition) { }

		public override decimal CalculateLateFee(int extraDays)
		{
			if (extraDays <= 0) return 0m;
			return extraDays * DailyRate * 1.5m; // 50% penalty for suv
		}

		public override decimal CalculateDamageFee(VehicleCondition returnCondition)
		{
			return returnCondition switch
			{
				VehicleCondition.MinorDamage => 250m,
				VehicleCondition.MajorDamage => 800m,
				_ => 0m
			};
		}
	}
}
