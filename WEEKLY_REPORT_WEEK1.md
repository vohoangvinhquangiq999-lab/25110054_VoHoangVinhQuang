# BÁO CÁO TIẾN ĐỘ DỰ ÁN TUẦN 1
## 1. Thiết kế các Lớp
	- Vehicle (Abstract Class): Lớp cơ sở chứa thông tin chung (Id, LicensePlate, DailyRate, Mileage, Condition). 
	- EconomyVehicle & SuvVehicle: Các dòng xe cụ thể triển khai quy tắc tính phí riêng. 
	- Booking: Đại diện cho một giao dịch đặt xe, quản lý thông tin khách hàng, ngày bắt đầu/kết thúc và trạng thái đặt. 
	- Bill: Chứa kết quả tính toán tài chính (Phí gốc, Phí trễ, Phí hư hỏng, Tổng tiền). 
	- InventoryService: Quản lý danh mục xe và lịch đặt xe trong hệ thống. 
	- BillingService: Thực hiện quy trình trả xe và xuất hóa đơn. 
Giải thích lựa chọn mối quan hệ giữa các lớp: Tính kế thừa (Inheritance) giữa Vehicle và EconomyVehicle/SuvVehicle. Mối quan hệ này phù hợp vì xe Economy hay SUV đều thực sự là một loại xe (Vehicle), dùng chung thuộc tính nhận diện nhưng thể hiện hành vi tính phí khác nhau. 

## 2. Sơ đồ UML: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/uml.png





