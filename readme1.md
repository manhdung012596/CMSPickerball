# 🚀 HƯỚNG DẪN CÀI ĐẶT & CHẠY DỰ ÁN CMS PICKLEBALL
> **Dự án:** CMS Pickleball — Hệ thống Quản lý Sân Thể Thao & Bán Hàng Phụ Kiện  
> **Nền tảng:** ASP.NET Core 8.0 (.NET 8) + Blazor Server + EF Core + SQL Server  

---

## 📋 MỤC LỤC
1. [Yêu cầu hệ thống (Prerequisites)](#1-yêu-cầu-hệ-thống-prerequisites)
2. [Hướng dẫn cài đặt nhanh (Quick Start)](#2-hướng-dẫn-cài-đặt-nhanh-quick-start)
3. [Hướng dẫn chi tiết từng bước](#3-hướng-dẫn-chi-tiết-từng-bước)
   - [Bước 1: Tải mã nguồn](#bước-1-tải-mã-nguồn)
   - [Bước 2: Cấu hình chuỗi kết nối CSDL (Connection String)](#bước-2-cấu-hình-chuỗi-kết-nối-csdl-connection-string)
   - [Bước 3: Khởi tạo Cơ sở dữ liệu (Migration)](#bước-3-khởi-tạo-cơ-sở-dữ-liệu-migration)
   - [Bước 4: Khởi chạy ứng dụng](#bước-4-khởi-chạy-ứng-dụng)
4. [Thông tin tài khoản mặc định](#4-thông-tin-tài-khoản-mặc-định)
5. [Các đường dẫn truy cập chính](#5-các-đường-dẫn-truy-cập-chính)
6. [Xử lý lỗi thường gặp (Troubleshooting)](#6-xử-lý-lỗi-thường-gặp-troubleshooting)

---

## 1. Yêu cầu hệ thống (Prerequisites)

Trước khi bắt đầu cài đặt, hãy đảm bảo máy tính của bạn đã cài đặt đầy đủ các phần mềm sau:

| STT | Phần mềm | Phiên bản khuyến nghị | Link tải / Ghi chú |
|:---:|:---|:---|:---|
| 1 | **.NET SDK** | **.NET 8.0 SDK** (LTS) | [Tải .NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) |
| 2 | **Hệ quản trị CSDL** | **SQL Server** (2019/2022 Express hoặc LocalDB) | [Tải SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) |
| 3 | **Quản lý CSDL** | **SSMS** (SQL Server Management Studio) | [Tải SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) |
| 4 | **IDE / Trình biên soạn** | **Visual Studio 2022** (với workload *ASP.NET and web development*) hoặc **VS Code** | [Tải Visual Studio](https://visualstudio.microsoft.com/vs/) |
| 5 | **Git** | Phiên bản mới nhất | [Tải Git](https://git-scm.com/) |

> 💡 **Kiểm tra .NET SDK trên máy:**  
> Mở Command Prompt / PowerShell và gõ:
> ```bash
> dotnet --version
> ```
> Kết quả hiển thị dạng `8.0.xxx` là đạt yêu cầu.

---

## 2. Hướng dẫn cài đặt nhanh (Quick Start)

Dành cho người đã quen thuộc với .NET và SQL Server:

```bash
# 1. Di chuyển vào thư mục dự án
cd SoccerPitchMvc

# 2. Cài công cụ EF Core CLI (nếu máy chưa có)
dotnet tool install --global dotnet-ef

# 3. Cập nhật cơ sở dữ liệu từ Migration
dotnet ef database update

# 4. Khởi chạy ứng dụng
dotnet run
```

Sau đó mở trình duyệt truy cập: `https://localhost:7198` hoặc địa chỉ hiển thị trên terminal.

---

## 3. Hướng dẫn chi tiết từng bước

### Bước 1: Tải mã nguồn

Clone dự án từ Git hoặc giải nén thư mục dự án:
```bash
git clone <URL_CUA_REPO>
cd doan
```

---

### Bước 2: Cấu hình chuỗi kết nối CSDL (Connection String)

1. Mở file [appsettings.json](file:///d:/doan/SoccerPitchMvc/appsettings.json) trong thư mục `SoccerPitchMvc/`.
2. Chỉnh sửa dòng `DefaultConnection` cho phù hợp với máy của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=cmssanbong;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### 📌 Hướng dẫn chỉnh `Server=YOUR_SERVER_NAME`:
- **Nếu dùng SQL Server Express:**  
  `Server=.\\SQLEXPRESS` hoặc `Server=TEN_MAY_BAN\\SQLEXPRESS` hoặc `Server=localhost\\SQLEXPRESS`
- **Nếu dùng SQL Server LocalDB:**  
  `Server=(localdb)\\mssqllocaldb;Database=cmssanbong;Trusted_Connection=True;MultipleActiveResultSets=true`
- **Nếu dùng tài khoản SQL Server (`sa`):**  
  `Server=localhost;Database=cmssanbong;User Id=sa;Password=MatKhauCuaBan;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true`

---

### Bước 3: Khởi tạo Cơ sở dữ liệu (Migration)

Dự án sử dụng **Entity Framework Core Code-First**, toàn bộ bảng biểu và dữ liệu quan hệ đã có sẵn trong thư mục `Migrations`.

Bạn có thể chạy migration theo 1 trong 2 cách sau:

#### Cách 1: Sử dụng Terminal / PowerShell (Khuyến nghị)
Mở terminal tại thư mục `SoccerPitchMvc/` và thực thi:
```bash
# Cài đặt EF Core CLI (chỉ cần chạy 1 lần nếu máy chưa có)
dotnet tool install --global dotnet-ef

# Cập nhật CSDL
dotnet ef database update
```

#### Cách 2: Sử dụng Package Manager Console (Trong Visual Studio 2022)
1. Mở file `SoccerPitchManager.slnx` bằng **Visual Studio 2022**.
2. Chọn menu **Tools** ➜ **NuGet Package Manager** ➜ **Package Manager Console**.
3. Chọn Default project là `SoccerPitchMvc`.
4. Gõ lệnh:
```powershell
Update-Database
```

*(Sau khi lệnh chạy thành công, CSDL `cmssanbong` cùng toàn bộ 20 bảng dữ liệu sẽ tự động được tạo trên SQL Server).*

---

### Bước 4: Khởi chạy ứng dụng

#### Cách 1: Dùng dòng lệnh (CLI)
Tại thư mục `SoccerPitchMvc/`:
```bash
dotnet run
```
Màn hình terminal sẽ hiển thị URL dạng:
```text
Now listening on: https://localhost:7198
Now listening on: http://localhost:5198
```

#### Cách 2: Dùng Visual Studio 2022
1. Đặt `SoccerPitchMvc` làm **Startup Project** (chuột phải vào project `SoccerPitchMvc` ➜ **Set as Startup Project**).
2. Nhấn nút **Play** (hoặc phím `F5` / `Ctrl + F5`) để chạy dự án. Trình duyệt sẽ tự động mở trang web.

---

## 4. Thông tin tài khoản mặc định

Hệ thống có cơ chế **Auto-Seed Data** trong `Program.cs`. Khi chạy ứng dụng lần đầu, hệ thống sẽ tự động tạo các Role (`Admin`, `Staff`, `Customer`) và tài khoản quản trị viên:

| Thông tin | Tài khoản Quản trị (Admin) |
|:---|:---|
| **URL Đăng nhập** | `/admin/login` hoặc `/Account/Login` |
| **Email / Username** | `admin@elitepickleball.com` |
| **Mật khẩu (Password)** | `Admin123!` |
| **Vai trò (Role)** | `Admin` (Toàn quyền quản trị) |

> 👤 **Tài khoản khách hàng (Customer):**  
> Khách hàng có thể tự đăng ký tài khoản mới trực tiếp tại trang `/Account/Register` hoặc đặt sân vãng lai tại giao diện khách hàng.

---

## 5. Các đường dẫn truy cập chính

Sau khi chạy web thành công, bạn có thể truy cập các phân hệ sau:

| Phân hệ | Đường dẫn (URL) | Mô tả |
|:---|:---|:---|
| 🏠 **Trang chủ / Đặt sân** | `https://localhost:7198/` | Xem sân, tìm khung giờ, đặt sân, xem tin tức & sản phẩm |
| 📊 **Dashboard Quản trị** | `https://localhost:7198/admin` | Thống kê doanh thu, tỷ lệ lấp đầy, đơn đặt hôm nay |
| 🏸 **Quản lý Sân & Slot** | `https://localhost:7198/Pitches` | Thêm, sửa, cấu hình giá sân và khung giờ hoạt động |
| 📅 **Lịch Đặt sân** | `https://localhost:7198/Bookings` | Xem lịch trực quan, xác nhận duyệt đơn, check-in |
| 🛒 **Bán hàng POS & Kho** | `https://localhost:7198/admin/shop` | Bán nước/phụ kiện tại quầy, xuất nhập kho |
| 💰 **Sổ quỹ & Thu chi** | `https://localhost:7198/CashTransactions` | Quản lý phiếu thu, phiếu chi, đối soát tài chính |
| 🎁 **Khuyến mại & Mã Voucher** | `https://localhost:7198/admin/promotions` | Cấu hình voucher giảm giá, ưu đãi thành viên |
| 👥 **Phân quyền người dùng** | `https://localhost:7198/admin/authorization` | Phân quyền Admin / Staff / Customer |

---

## 6. Xử lý lỗi thường gặp (Troubleshooting)

### 🔴 Lỗi 1: `Cannot open database "cmssanbong"` hoặc `A network-related or instance-specific error`
* **Nguyên nhân:** Tên Server SQL Server chưa đúng hoặc SQL Server Service chưa được bật.
* **Cách khắc phục:**
  1. Mở **SQL Server Configuration Manager** ➜ Kiểm tra **SQL Server (SQLEXPRESS)** đã ở trạng thái *Running*.
  2. Mở SSMS, copy chính xác Server Name khi đăng nhập SSMS và paste vào `Server=...` trong file `appsettings.json`.
  3. Đảm bảo có `TrustServerCertificate=True;` trong chuỗi kết nối.

---

### 🔴 Lỗi 2: `'dotnet-ef' is not recognized as an internal or external command`
* **Nguyên nhân:** Máy tính chưa cài đặt công cụ dòng lệnh Entity Framework CLI.
* **Cách khắc phục:** Chạy lệnh sau trong PowerShell/CMD:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
  *(Nếu đã cài nhưng lỗi, thử chạy: `dotnet tool update --global dotnet-ef`)*

---

### 🔴 Lỗi 3: Cảnh báo SSL / HTTPS Certificate khi chạy trên trình duyệt
* **Nguyên nhân:** Chứng chỉ phát triển HTTPS cục bộ của .NET chưa được tin cậy.
* **Cách khắc phục:** Mở terminal và chạy lệnh:
  ```bash
  dotnet dev-certs https --trust
  ```
  Sau đó chọn **Yes** ở hộp thoại xác nhận.

---

### 🔴 Lỗi 4: Không hiển thị giao diện Admin / Lỗi SignalR Blazor
* **Nguyên nhân:** Kết nối WebSocket bị chặn hoặc chưa đăng nhập đúng quyền.
* **Cách khắc phục:**
  1. Đăng nhập bằng tài khoản `admin@elitepickleball.com` / `Admin123!`.
  2. Đảm bảo trình duyệt hỗ trợ WebSocket và không bị chặn bởi extension/antivirus.

---

## 📞 Hỗ trợ kỹ thuật
Nếu gặp bất kỳ khó khăn nào trong quá trình cài đặt và trải nghiệm dự án, vui lòng liên hệ quản trị viên hoặc tạo issue trong repository để được hỗ trợ kịp thời.
