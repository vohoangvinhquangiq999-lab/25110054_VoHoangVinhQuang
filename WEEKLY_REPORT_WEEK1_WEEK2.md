\# BÁO CÁO TIẾN ĐỘ TỔNG HỢP (TUẦN 1 \& TUẦN 2)

\*\*Tên dự án:\*\* Vehicle Rental System (Hệ thống Quản lý Cho thuê Xe)  

\*\*Môn học:\*\* Lập trình Hướng đối tượng (OOP) – HK1 2026-2027  

\*\*Sinh viên thực hiện:\*\* Võ Hoàng Vĩnh Quang (MSSV: 25110054)  

\*\*Repository Link:\*\* \[https://github.com/vohoangvinhquangiq999-lab/25110054\_VoHoangVinhQuang\_](https://github.com/vohoangvinhquangiq999-lab/25110054\_VoHoangVinhQuang\_)  



\---



\## BÁO CÁO TIẾN ĐỘ CHUNG



\### 1. Phân tích Bài toán thực tế (Domain Analysis \& Problem Restatement)

Doanh nghiệp cho thuê xe cần một hệ thống quản lý tập trung danh mục xe (Economy, SUV,...), xử lý lịch đặt xe (Booking), tra cứu xe trống theo khoảng thời gian và tự động tính toán chi phí khi khách trả xe bao gồm cả phí phạt trả muộn và hư hỏng.



Các bài toán kỹ thuật cốt lõi cần giải quyết:

\- \*\*Chống trùng lịch (Double-booking):\*\* Đảm bảo một xe không bị đặt trùng bởi 2 khách trong cùng một khoảng thời gian.

\- \*\*Tính phí linh hoạt (Polymorphic Pricing \& Fees):\*\* Áp dụng đơn giá, tỷ lệ phạt trả muộn (Late Fee) và phí hư hỏng (Damage Fee) khác nhau theo từng dòng xe.

\- \*\*Quản lý trạng thái xe (State \& Invariants):\*\* Đảm bảo tính toàn vẹn của số km đã đi (Odometer non-decreasing) và quy trình chuyển đổi trạng thái giao dịch.

