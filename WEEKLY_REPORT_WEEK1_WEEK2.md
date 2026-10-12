BÁO CÁO TIẾN ĐỘ DỰ ÁN (TUẦN 1 & TUẦN 2)
Tên dự án: Vehicle Rental System
Sinh viên thực hiện: Võ Hoàng Vinh Quang
MSSV: 25110054
Môn học: Lập trình Hướng đối tượng (OOP) – HK1 2026-2027
C# / .NET Core / Visual Studio
GitHub: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang
I. TỔNG QUAN VỀ DỰ ÁN & BỐI CẢNH (DOMAIN ANALYSIS)
1. Phân tích bài toán thực tế (Problem Restatement)
Doanh nghiệp cho thuê xe cần một hệ thống quản lý tập trung đội xe (xe bình dân Economy, xe gầm cao SUV,...), xử lý lịch đặt xe (Booking), tra cứu xe trống theo khoảng thời gian và tính toán chi phí khi khách trả xe. Các thách thức cốt lõi cần giải quyết: 
Tra cứu và chống trùng lịch (Double booking): Đảm bảo một xe không bị đặt trùng bởi 2 khách trong cùng một khoảng thời gian. 
Giá và phí phạt linh hoạt (Price & Fees): Phí thuê theo ngày, phí trả muộn (Late Fee) và phí đền bù hư hỏng (Damage Fee) thay đổi tùy theo dòng xe. 
Quản lý vòng đời và trạng thái xe: Theo dõi số km đã đi (Mileage) và tình trạng kỹ thuật (Condition) liên tục qua các lượt thuê.
II. BÁO CÁO TUẦN 1: PHÂN TÍCH VÀ THIẾT KẾ (ANALYZE & DESIGN)
1. Thiết kế các Lớp
	Vehicle (Abstract Class): Lớp cơ sở chứa thông tin chung (Id, LicensePlate, DailyRate, Mileage, Condition). 
	EconomyVehicle & SuvVehicle: Các dòng xe cụ thể triển khai quy tắc tính phí riêng. 
	Booking: Đại diện cho một giao dịch đặt xe, quản lý thông tin khách hàng, ngày bắt đầu/kết thúc và trạng thái đặt. 
	Bill: Chứa kết quả tính toán tài chính (Phí gốc, Phí trễ, Phí hư hỏng, Tổng tiền). 
	InventoryService: Quản lý danh mục xe và lịch đặt xe trong hệ thống. 
	BillingService: Thực hiện quy trình trả xe và xuất hóa đơn. 
Giải thích lựa chọn mối quan hệ giữa các lớp: Tính kế thừa (Inheritance) giữa Vehicle và EconomyVehicle/SuvVehicle. Mối quan hệ này phù hợp vì xe Economy hay SUV đều thực sự là một loại xe (Vehicle), dùng chung thuộc tính nhận diện nhưng thể hiện hành vi tính phí khác nhau. 

2. Sơ đồ UML


III. BÁO CÁO TUẦN 2: THỰC THI LỚP CỐT LÕI VÀ ĐIỀU KIỆN TIÊN QUYẾT (CORE CLASSES & INVARIANTS)
A. Giải thích Chi tiết các Nguyên lý OOP
1. Polymorphism (Đa hình)
	Vị trí xuất hiện trong Code:
Khai báo phương thức thuần ảo (Abstract) trong Vehicle.cs:
Cài đặt cụ thể tại EconomyVehicle.cs (Phạt 20% đơn giá/ngày; Phạt hư hỏng $100/$350) và SuvVehicle.cs (Phạt 50% đơn giá/ngày; Phạt hư hỏng $250/$800). 

Triệu gọi đa hình tại BillingService.GenerateBill:





            1.2 Giải thích cụ thể lý do áp dụng: 
Trong thực tế, xe SUV có chi phí khấu hao và sửa chữa cao hơn nhiều so với xe Economy. Dùng đa hình cho phép BillingService chỉ cần nhận đối tượng lớp cha Vehicle và gọi hàm tính phí mà không cần kiểm tra kiểu xe bằng if-else. Sau này nếu mở rộng thêm dòng xe LuxuryVehicle, ta chỉ cần tạo class mới kế thừa Vehicle mà không phải chỉnh sửa bất kỳ dòng code nào trong BillingService. 
2. Encapsulation (Đóng gói)
	Vị trí xuất hiện trong Code:
Lớp Vehicle: Mileage và Condition được thiết lập private set. Dữ liệu chỉ được sửa đổi thông qua phương thức kiểm soát UpdateAfterRental.
Lớp Booking: Status có private set, chỉ chuyển trạng thái qua CompleteBooking(). Ràng buộc ngày (EndDate > StartDate) được kiểm tra ngay trong Constructor.
	Bảo vệ các Điều kiện Tiên quyết (Invariants Protected):
Invariant 1 (Odometer Non-decreasing): Số km trên xe không bao giờ được phép giảm đi sau khi thuê.
Invariant 2 (Valid Date Range & State Lifecycle): Ngày trả xe phải sau ngày nhận xe, và đơn hàng chỉ được hoàn tất khi đang ở trạng thái Confirmed.
2.3 Hệ quả nếu field để public:
Nếu Mileage có public set, mã bên ngoài có thể trực tiếp gán car.Mileage = 5000 (giảm từ 15000km xuống 5000km), làm sai lệch lịch trình bảo dưỡng xe và gây mất an toàn kỹ thuật.
Nếu Booking.Status để public, lập trình viên có thể vô tình chuyển trạng thái đơn hàng từ Cancelled sang Completed mà bỏ qua bước tính toán hóa đơn, dẫn đến thất thoát tài chính.
3. Single Responsibility Principle
	Vị trí xuất hiện trong Code:
	Lớp InventoryService: Chịu trách nhiệm duy nhất về quản lý kho xe, tìm kiếm xe trống (SearchAvailableVehicles) và khởi tạo đơn đặt xe (CreateBooking). 
	Lớp BillingService: Chịu trách nhiệm duy nhất về tính toán hóa đơn thanh toán, áp dụng phí phạt và xử lý quy trình trả xe (GenerateBill). 
3.2 Giải thích cụ thể lý do áp dụng (Why it was the right tool):
	Tra cứu lịch trống thuộc về mảng Quản lý kho & Lịch trình, còn tính hóa đơn thuộc về mảng Tài chính & Kế toán.
	Nếu gộp chung vào một "God Class", khi doanh nghiệp thay đổi công thức ưu đãi/thuế giá trị gia tăng, ta buộc phải sửa đổi lớp chứa cả thuật toán tìm kiếm xe trống, dễ dẫn đến rủi ro phát sinh bug lan truyền. Việc tách biệt giúp bảo trì độc lập. 
B. Thuật toán Kiểm tra Trùng lịch (Overlap Detection Algorithm)
Cài đặt logic trong C# (Booking.cs):





Giải thích thuật toán: Hai khoảng thời gian [A_start,A_end ] và [B_start,B_end ] giao nhau khi và chỉ khi: Ngày bắt đầu của đợt này nhỏ hơn Ngày kết thúc của đợt kia AND Ngày kết thúc của đợt này lớn hơn Ngày bắt đầu của đợt kia.

C. Mẫu Kiểm thử và Kết quả Chạy Thực tế Mã nguồn trong Program.cs thực thi 3 kịch bản kiểm thử bắt buộc:
IV. NHẬT KÝ SỬ DỤNG AI (AI USAGE LOG - WEEKS 1 and 2)
NGÀY	CÔNG CỤ AI	YÊU CẦU
04/10/2026	Gemini, ChatGPT	Gợi ý thiết kế các lớp OOP cho bài toán Vehicle Rental System bằng C#
08/10/2026	Gemini, Claude	Sửa lỗi và hỗ trợ viết file Program.cs
11/10/2026	Claude	Đánh giá code, viết unit test và gỡ lỗi build.



