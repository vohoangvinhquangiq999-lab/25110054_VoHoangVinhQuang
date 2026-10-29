# BÁO CÁO TIẾN ĐỘ DỰ ÁN TUẦN 3 (FEATURES, PATTERNS & REFACTORING)

GitHub: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang  

Program.cs: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/Program.cs  

README.md: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/README.md  

WEEKLY REPORT WEEK 3: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/WEEKLY_REPORT_WEEK3.md  

COMMITS: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/commits/master/

Tách `Program.cs` thành nhiều file theo lớp (không đổi hành vi): https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/commit/f78e38ceca0b1bc52bd122ef3655f63651f8d8d2

JSON: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/commit/1c29a0f17d9250ef83a8ab12186f65368a87fa8b

## 1. Tính năng đã hoàn thiện trong tuần 3

| # | Tính năng | Vị trí trong code | Lý do |
|---|---|---|---|
| 1 | Hủy booking | `Booking.Cancel()`, `InventoryService.CancelBooking()` | Enum `Cancelled` có từ tuần 2 nhưng không có đường nào đặt được |
| 2 | Xe hỏng nặng bị khóa đến khi sửa | `Vehicle.IsRentable`, `Vehicle.Repair()`, kiểm tra trong `SearchAvailableVehicles`/`CreateBooking` | Theo dõi tình trạng xe qua các lượt thuê – tình trạng phải có tác dụng thật |
| 3 | Tính ngày theo "ngày bắt đầu tính là 1 ngày" | `BillingService.BillableDays` | Trả trễ 5 giờ trước đây không bị tính phí |
| 4 | Lưu/nạp JSON | `Persistence/InventoryRepository.cs` | Dữ liệu không mất khi tắt chương trình |
| 5 | Kiểm tra đầu vào | `AddVehicle` (Id trùng), `CreateBooking` (BookingId trùng), `Booking` (chuỗi rỗng), `Vehicle` (km âm) | Chặn dữ liệu sai ngay từ cửa vào |
| 6 | `PreviewBill` | `BillingService` | Xem trước hóa đơn mà không đổi trạng thái; tách "tính" khỏi "ghi" |

### Exception riêng (cải thiện xử lý lỗi)

7 lớp exception trong `Domain/Exceptions.cs`. Mỗi lớp **kế thừa từ exception chuẩn** (vd `DoubleBookingException : InvalidOperationException`) nên mã cũ catch kiểu cha vẫn chạy, còn mã mới catch được đúng lỗi nghiệp vụ và đọc được dữ liệu kèm theo (`ex.VehicleId`). Lý do: `catch (Exception)` hoặc `InvalidOperationException` chung chung không phân biệt được "đặt trùng" với "hoàn tất booking hai lần".

### Lưu/nạp file JSON

- `InventoryRepository` là lớp **riêng**, `InventoryService` không biết gì về file (SRP).
- Dùng lớp `VehicleData`, `BookingData` chỉ để đọc/ghi; lớp nghiệp vụ giữ nguyên `private set`. Khi nạp, dữ liệu **vẫn đi qua constructor** nên các invariant vẫn được kiểm tra: file có giá thuê ≤ 0 hoặc hai booking trùng lịch sẽ bị từ chối bằng `PersistenceException`.
- Trường thiếu trong file cũ nhận giá trị mặc định ⇒ thêm trường mới không làm hỏng dữ liệu cũ.
- Loại xe được lưu bằng **tên lớp**; khi nạp, `Activator` tạo lại bằng reflection.


## 2. Nhật ký AI tuần 3

| Ngày | Công cụ | Prompt | Kết quả AI | Kiểm chứng | Chỉnh sửa / sai sót |
|---|---|---|---|---|---|
| 19/10/2026 | Claude | Đánh giá code, viết unit test và gỡ lỗi build <!-- SV: xác nhận --> | Nhận xét ưu/nhược; bộ test xUnit (5 file); cách thêm project test | Chạy `dotnet test`, đối chiếu giá trị mong đợi tính tay | Sửa lỗi build bằng cách tham chiếu project chính |
| 29/10/2026 | Claude Sonnet 5.5 | Dựa vào file `.cs`, sửa lỗi và chia file Program.cs thành nhiều file theo lớp (không đổi hành vi) | Code tuần 3 (F1–F6, R1–R4, exception, JSON), test mới, các báo cáo, UML | AI chạy build + test trong môi trường riêng; thăm dò lỗi trên code cũ; "mutation check" (đưa logic cũ vào thì 3 test hồi quy fail). | **Lỗi của AI ở lượt 08/10 được tìm ra:** `GenerateBill` ghi trạng thái trước khi kiểm tra hết, `Cancelled` không có đường vào, dùng `double` cho tiền, `Bill` có public setter. |


## 3. Kế hoạch tuần 4

1. Thêm một loại xe mới (`LuxuryVehicle`) 
2. Test cho loại mới: phí, tìm kiếm, đặt trùng, tính tiền end-to-end, lưu/nạp file.
3. Chốt UML khớp code, hoàn thiện README + báo cáo + nhật ký AI đầy đủ.

