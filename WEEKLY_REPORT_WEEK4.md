# BÁO CÁO TIẾN ĐỘ DỰ ÁN TUẦN 4 (EXTEND, FINALIZE & PREPARE DEFENSE)

GitHub: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang  

Program.cs: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/Program.cs  

README.md: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/README.md  

WEEKLY REPORT WEEK 4: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/WEEKLY_REPORT_WEEK4.md  

COMMITS: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/commits/master/

Thêm `LuxuryVehicle.cs` (chỉ thêm file mới): https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/commit/4a7bb7befa99dde8be1213072c06933e82f06421

## 1. Extensibility demo: thêm loại xe mới KHÔNG sửa lớp có sẵn

**Yêu cầu giả định:** công ty thêm dòng xe hạng sang: phí trả trễ +100% giá thuê, phí hư hại 500 (nhẹ) / 1500 (nặng).

**Việc đã làm:** thêm đúng **một file** `Domain/LuxuryVehicle.cs` kế thừa `Vehicle` và ghi đè hai phương thức tính phí; thêm một file test `ExtensibilityTests.cs`.

Kết quả test `Luxury_EndToEnd_ThreeCategoriesGiveThreeDifferentTotals`: cùng thuê 2 ngày, trả trễ 2 ngày, hư nặng ⇒ Economy **526**, SUV **1250**, Luxury **2700**.

## 2. nguyên lý OOP

| Nguyên lý | Lớp/phương thức cụ thể | Giải thích |
|---|---|---|
| **Polymorphism** | `Vehicle.CalculateLateFee`, `CalculateDamageFee` (abstract); override ở `EconomyVehicle`, `SuvVehicle`, `LuxuryVehicle`; `BillingService.PreviewBill` gọi qua kiểu `Vehicle` | Công thức phí khác nhau theo loại xe. Nếu dùng `if (vehicle is Suv)` trong `BillingService`, mỗi loại xe mới phải sửa lớp đó (vi phạm Open/Closed) |
| **Encapsulation** | `Booking.Status` (`private set`) chỉ đổi qua `CompleteBooking()`/`Cancel()`; `Vehicle.Mileage`/`Condition` chỉ đổi qua `UpdateAfterRental`/`Repair`; `Bill` bất biến; `InventoryService.Vehicles` là `IReadOnlyList` | Vòng đời đặt xe và số km là trạng thái có luật. Chỉ có một cửa vào nên luật không thể bị lách |
| **SRP** | `InventoryService` (tìm/đặt), `BillingService` (tiền), `InventoryRepository` (file), `Bill.ToInvoiceText` (định dạng), `Program` (demo) | Mỗi lý do thay đổi (giá, thuế, định dạng file, giao diện) chỉ chạm một lớp |
| **Open/Closed** | `LuxuryVehicle` (tuần 4) | Mở rộng bằng cách thêm, không sửa |

## 3. Nhật ký AI tuần 4

| Ngày | Công cụ | Prompt | Kết quả AI | Kiểm chứng | Chỉnh sửa / sai sót |
|---|---|---|---|---|---|
| 29/10/2026 | Claude Sonnet 5.5 | Hỗ trợ viết file LuxuryVehicle.cs , kiểm tra lại các test, logic | AI chạy build + test, thăm dò lỗi trên code cũ, đưa ra cách viết cho file .cs mới. | Hoàn thiện file LuxuryVehicle.cs |


