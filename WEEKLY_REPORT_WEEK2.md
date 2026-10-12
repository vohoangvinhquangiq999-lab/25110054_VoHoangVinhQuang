\# BÁO CÁO TIẾN ĐỘ DỰ ÁN TUẦN 2 (CORE CLASSES \& INVARIANTS)

\## 1. Giải thích Chi tiết các Nguyên lý OOP

\*\* Encapsulation (Đóng gói): \*\* Bảo vệ trạng thái nội bộ của đối tượng bằng cách ẩn các setter (private set, protected set), chỉ cho phép thay đổi dữ liệu thông qua các phương thức có kiểm tra điều kiện.   

\*\* Polymorphism (Đa hình): \*\* Xây dựng lớp cơ sở abstract class Vehicle và hai lớp con EconomyVehicle, SuvVehicle ghi đè (override) các phương thức tính phí trễ hạn và phí hư hỏng.   

Vehicle, Booking, Bill: Mô hình hóa dữ liệu và quy tắc nghiệp vụ cốt lõi.  

InventoryService: Quản lý kho xe, tra cứu và xử lý chống trùng lịch đặt.  

BillingService: Đảm nhận việc quyết toán hóa đơn khi trả xe.

\## 2. Invariants Protection

\*\* Lớp Vehicle: \*\*

Ràng buộc 1 (Giá thuê dương): Ngay tại Constructor, nếu dailyRate <= 0, hệ thống lập tức ném ra ArgumentException.

Ràng buộc 2 (Không tua ngược số km - Mileage Invariant): Số km xe chạy (Mileage) chỉ có thể tăng hoặc giữ nguyên. Thuộc tính Mileage có setter là private. Việc cập nhật số km bắt buộc phải thông qua phương thức UpdateAfterRental. Trong phương thức này, nếu newMileage < Mileage, hệ thống sẽ ném ngoại lệ ngăn chặn hành vi ghi đè dữ liệu sai lệch.   

\*\* Lớp Booking: \*\*

Ràng buộc thời gian thuê: Ngày kết thúc (EndDate) bắt buộc phải sau ngày bắt đầu (StartDate). Kiểm tra này được thực hiện trực tiếp trong Constructor của Booking.   

Ràng buộc trạng thái: Đơn đặt xe chỉ được chuyển sang trạng thái Completed thông qua phương thức CompleteBooking() khi trạng thái hiện tại đang là Confirmed.

\*\* Lớp InventoryService (Overlap Detection): \*\*

Ngăn chặn tình trạng đặt trùng xe (Double-booking) bằng biểu thức logic trong Booking.OverlapsWith:StartDate < end \&\& EndDate > start.

Nếu đã có lịch đặt kích hoạt trùng thời gian, CreateBooking sẽ ném ngoại lệ InvalidOperationException để từ chối giao dịch.

\## 3. Chạy các test case mẫu:

