# 25110054_VoHoangVinhQuang
# Tên dự án: Vehicle Rental System
# Sinh viên thực hiện: Võ Hoàng Vinh Quang
# MSSV: 25110054
# Môn học: Object-Oriented Programming – HK1 2026-2027
C# / .NET Core / Visual Studio <br>
GitHub: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang <br>
Program.cs: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/Program.cs <br>
README.md: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/README.md <br>
WEEKLY REPORT WEEK 1: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/WEEKLY_REPORT_WEEK1.md <br>
WEEKLY REPORT WEEK 2: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/blob/master/WEEKLY_REPORT_WEEK2.md <br>
COMMITS: https://github.com/vohoangvinhquangiq999-lab/25110054_VoHoangVinhQuang/commits/master/ <br>
## TỔNG QUAN VỀ DỰ ÁN Vehicle Rental System:
### Phân tích bài toán thực tế (Problem Restatement):
Doanh nghiệp cho thuê xe cần một hệ thống quản lý tập trung đội xe (xe bình dân Economy, xe gầm cao SUV,...), xử lý lịch đặt xe (Booking), tra cứu xe trống theo khoảng thời gian và tính toán chi phí khi khách trả xe. Các thách thức cốt lõi cần giải quyết: 
- Tra cứu và chống trùng lịch (Double booking): Đảm bảo một xe không bị đặt trùng bởi 2 khách trong cùng một khoảng thời gian. 
- Giá và phí phạt linh hoạt (Price & Fees): Phí thuê theo ngày, phí trả muộn (Late Fee) và phí đền bù hư hỏng (Damage Fee) thay đổi tùy theo dòng xe. 
- Quản lý vòng đời và trạng thái xe: Theo dõi số km đã đi (Mileage) và tình trạng kỹ thuật (Condition) liên tục qua các lượt thuê.
