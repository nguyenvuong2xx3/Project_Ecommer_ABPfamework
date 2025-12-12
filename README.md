Tên đồ án: Xây dựng Website bán điện thoại di dộng
---
Họ và tên: Nguyễn Quốc Vương
----
Đại học Mỏ Địa - Chất - chuyên nghành: công nghệ phần mềm


---
Mô tả ngắn
----------
Đây là một dự án e‑commerce (ABP/ASP.NET-based admin framework) chứa các module quản lý cho một hệ thống thương mại điện tử: quản lý người dùng, vai trò, danh mục, sản phẩm, biến thể sản phẩm, biển quảng cáo, khuyến mãi/giảm giá, đơn hàng, giỏ hàng, bình luận, thông báo và đánh giá. Mục tiêu của README này là giúp người khác clone, cài đặt và chạy được project trên máy local.

Tính năng (Modules)
-------------------
- Quản lý người dùng: đăng ký, đăng nhập, quản lý thông tin người dùng, đặt lại mật khẩu, kích hoạt/tắt tài khoản.
- Quản lý vai trò: tạo/sửa/xóa vai trò, gán quyền cho vai trò, gán vai trò cho người dùng.
- Quản lý danh mục: CRUD danh mục sản phẩm (phân cấp/hierarchy).
- Quản lý sản phẩm: CRUD sản phẩm, hình ảnh, mô tả, trạng thái hiển thị.
- Quản lý biến thể sản phẩm: size, màu, thuộc tính khác và tồn kho theo biến thể.
- Quản lý biển quảng cáo: tạo/xuất bản/cập nhật banner, gán vị trí hiển thị.
- Quản lý khuyến mãi / giảm giá: tạo mã giảm giá, chương trình khuyến mãi theo sản phẩm/danh mục.
- Quản lý đơn đặt hàng: tạo đơn, xem lịch sử, thay đổi trạng thái (pending, processing, shipped, delivered, cancelled).
- Quản lý giỏ hàng: thêm/xóa/sửa số lượng, chuyển giỏ thành đơn hàng.
- Quản lý bình luận: duyệt/xóa bình luận của khách hàng.
- Quản lý thông báo: gửi/đọc thông báo hệ thống cho người dùng.
- Quản lý đánh giá: CRUD đánh giá, tính điểm trung bình cho sản phẩm.

Yêu cầu trước khi cài đặt
-------------------------
- Git
- .NET SDK (nếu backend dùng ABP/ASP.NET Core): khuyến nghị .NET 7 hoặc .NET 8 tùy repo
- Node.js (v16+) + npm hoặc Yarn (nếu có phần frontend riêng)
- Cơ sở dữ liệu: SQL Server
- dotnet-ef (nếu cần chạy migrations): dotnet tool install --global dotnet-ef
- Visual studio 2022

Cài đặt appsetting
----------
sửa 2 file appsettingsDefault -> appsetting tại 2 project Acme.SimpleTaskApp.Migrator và Acme.SimpleTaskApp.Web.Mvc
điền chuỗi kết nối theo trên máy chạy:
"ConnectionStrings": {
  "Default": "Server=localhost\\SQLEXPRESS;Database=MyProjectDb;User ID=sa;Password=your_password;MultipleActiveResultSets=true;TrustServerCertificate=true;"
}

Khôi phục lại thư viện
----------
thực hiện tổ hợp phím CTRL + ` trên bàn phím
nhập lệnh: libman restore

Cài đặt dự án mặc định  (Set as Startup Project)
------------------------
Chọn dự án Acme.SimpleTaskApp.Web.Mvc chuột phải chọn Set as Startup Project

Thực hiện nhập file có đuôi .sql để lấy dữ liệu 
-------------------

Nếu chạy dự án lại từ đầu làm theo cách sau: 
-------------------
B1: Trên thanh công cụ phía trên chọn Acme.SimpleTaskApp.Migrator 
Thực hiện chạy dự án để khởi tạo database
B2: Trên thanh công cụ chọn Tools -> Nuget Package Manager -> Package Manager Console chạy lệnh: update-database
Lưu ý: tại cmd nhập lệnh chuyển Default project  Acme.SimpleTaskApp.EntityFrameworkCore
-----------
TRƯỜNG HỢP FILE SQL KHÔNG THỂ IMPORT VÀO SQL SERVER VUI LÒNG CHẠY LẠI DỰ ÁN VỚI CÁCH BÊN TRÊN VÀ DÙNG FILE "Mẫu product.xlsx" ĐỂ THÊM HÀNG LOẠT SẢN PHẨM VÀ BIẾN THỂ
-------------------
