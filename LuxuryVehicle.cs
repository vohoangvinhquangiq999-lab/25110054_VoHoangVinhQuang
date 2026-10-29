namespace VehicleRentalSystem
{
	// Tuan 4 - extensibility demo: loai xe MOI, them bang file moi, KHONG sua Vehicle / BillingService /
	// InventoryService / InventoryRepository / Program.
	//  - Phi tra tre: +100% (x2.0)
	//  - Phi hu hai : co dinh theo muc (minor 500, major 1500)
	public class LuxuryVehicle : Vehicle
	{
		public LuxuryVehicle(string id, string licensePlate, string model, decimal dailyRate, int mileage, VehicleCondition condition = VehicleCondition.Excellent)
			: base(id, licensePlate, model, dailyRate, mileage, condition) { }

		public override decimal CalculateLateFee(int extraDays)
		{
			if (extraDays <= 0) return 0m;
			return extraDays * DailyRate * 2.0m; // 100% penalty for luxury
		}

		public override decimal CalculateDamageFee(VehicleCondition returnCondition)
		{
			return returnCondition switch
			{
				VehicleCondition.MinorDamage => 500m,
				VehicleCondition.MajorDamage => 1500m,
				_ => 0m
			};
		}
	}
}
