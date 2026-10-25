# HAVEN 🏡

> **Home Awaits, Vision Empowers Navigation.**  
> Nền tảng khám phá và tìm kiếm bất động sản theo nhu cầu.

HAVEN là dự án web hướng tới việc hỗ trợ người dùng khám phá và tìm kiếm bất động sản theo nhu cầu. Repository hiện đang ở giai đoạn khởi tạo nền tảng kỹ thuật; các chức năng nghiệp vụ sẽ được bổ sung và cập nhật cùng tiến độ phát triển.

## 📌 Trạng thái dự án

- **Loại dự án:** Ứng dụng web ASP.NET Core MVC.
- **Framework:** .NET 10.
- **Cơ sở dữ liệu dự kiến sử dụng trong môi trường phát triển:** SQL Server 2022 chạy bằng Docker.
- **ORM:** Entity Framework Core.
- **Kiến trúc solution hiện tại:** Web, Application, Domain, Infrastructure và các project kiểm thử.
- **CI:** GitHub Actions tự động restore, build và chạy test khi push hoặc tạo Pull Request vào `main` / `develop`.

> Lưu ý: cấu trúc project đã được khởi tạo, nhưng không có nghĩa mọi chức năng tìm kiếm hoặc nghiệp vụ bất động sản đã hoàn thành.

## 🧰 Công nghệ

- C# / ASP.NET Core MVC
- .NET SDK 10.0.401 (được khai báo trong `global.json`)
- Entity Framework Core 10
- Microsoft SQL Server 2022
- Docker Compose
- xUnit
- GitHub Actions

## 🗂️ Cấu trúc repository

```text
HAVEN/
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── Haven.Web/                 # Điểm vào ứng dụng web, MVC, static assets
│   ├── Haven.Application/         # Tầng ứng dụng
│   ├── Haven.Domain/              # Tầng miền nghiệp vụ
│   └── Haven.Infrastructure/      # Tích hợp hạ tầng và EF Core
├── tests/
│   ├── Haven.UnitTests/
│   └── Haven.IntegrationTests/
├── compose.yaml                   # SQL Server cho môi trường phát triển
├── global.json                    # Phiên bản .NET SDK
└── HAVEN.slnx                     # Solution
```

## ⚙️ Yêu cầu môi trường

Cài đặt các công cụ sau trên máy phát triển:

1. Git.
2. .NET SDK 10.0.401 hoặc phiên bản tương thích theo `global.json`.
3. Docker Desktop (đã khởi động Docker Engine).
4. Trình soạn thảo phù hợp, khuyến nghị Visual Studio hoặc Visual Studio Code.

Kiểm tra cài đặt:

```powershell
git --version
dotnet --version
docker --version
docker compose version
```

## 🚀 Cài đặt và chạy local

### 1. Clone repository

```powershell
git clone https://github.com/xduc199/HAVEN.git
cd HAVEN
```

### 2. Tạo file cấu hình môi trường

Tạo file `.env` từ file mẫu:

```powershell
Copy-Item .env.example .env
```

Mở `.env` và đặt giá trị mạnh, riêng cho máy phát triển của bạn:

```dotenv
MSSQL_SA_PASSWORD=ReplaceWithYourOwnStrongPassword123!
```

Đây chỉ là ví dụ định dạng. Hãy thay bằng mật khẩu riêng đáp ứng yêu cầu của SQL Server. **Không commit hoặc chia sẻ file `.env`**; Git đã được cấu hình bỏ qua file này. Chỉ commit `.env.example` với giá trị minh họa, không chứa bí mật.

### 3. Khởi động SQL Server

```powershell
docker compose up -d
docker compose ps
```

Dịch vụ SQL Server được ánh xạ tại `127.0.0.1:1433`. Dữ liệu phát triển được lưu trong Docker volume `haven_sql_data`.

Dừng container mà không xóa dữ liệu:

```powershell
docker compose down
```

Không dùng `docker compose down -v` trừ khi bạn chủ ý xóa volume dữ liệu phát triển.

### 4. Restore, build và test

Chạy từ thư mục gốc repository:

```powershell
dotnet restore HAVEN.slnx
dotnet build HAVEN.slnx --configuration Release --no-restore
dotnet test HAVEN.slnx --configuration Release --no-build
```

### 5. Chạy ứng dụng web

```powershell
dotnet run --project src/Haven.Web/Haven.Web.csproj
```

Mở địa chỉ HTTP/HTTPS mà terminal hiển thị. URL cụ thể phụ thuộc cấu hình chạy local của máy bạn.

> Lưu ý: SQL Server container có thể chạy độc lập, nhưng để ứng dụng thực sự kết nối cơ sở dữ liệu, cấu hình connection string và đăng ký dịch vụ phải được hoàn thiện/đặt đúng theo cấu hình của môi trường hiện tại. Không tự giả định ứng dụng đã có sẵn luồng khởi tạo database.

## 🌿 Quy trình nhánh

- `main`: nhánh ổn định.
- `develop`: nhánh tích hợp công việc.
- `feature/<ten-tinh-nang>`: phát triển tính năng.
- `fix/<mo-ta-loi>`: sửa lỗi thông thường.
- `hotfix/<mo-ta>`: xử lý lỗi khẩn cấp theo thống nhất của nhóm.

Không phát triển tính năng trực tiếp trên `main` hoặc `develop`. Tạo nhánh công việc từ `develop`, sau đó mở Pull Request về `develop`. Chỉ đưa phiên bản đã được nhóm xác nhận từ `develop` sang `main` qua Pull Request.

Xem hướng dẫn đầy đủ tại [CONTRIBUTING.md](CONTRIBUTING.md).

## ✅ CI — kiểm tra tự động

Workflow tại `.github/workflows/ci.yml` thực hiện:

1. Checkout source.
2. Cài .NET SDK 10.
3. Restore dependencies.
4. Build solution ở cấu hình Release.
5. Chạy test.

Trước khi yêu cầu review, hãy chạy các lệnh restore/build/test ở máy local và kiểm tra CI của Pull Request. Do repository hiện tại chưa có branch protection được GitHub cưỡng chế trên gói đang sử dụng, nhóm cần tuân thủ quy trình Pull Request bằng thao tác và review thực tế.

## 🔐 An toàn thông tin

- Không commit `.env`, mật khẩu, token, API key hoặc connection string chứa thông tin xác thực.
- Không đưa dữ liệu cá nhân hay dữ liệu thật của khách hàng vào repository hoặc môi trường phát triển dùng chung.
- Nếu bí mật bị commit hoặc chia sẻ, hãy thu hồi/thay mới bí mật đó; chỉ xóa file khỏi commit sau không làm bí mật đã lộ trở nên an toàn.
- Không xóa Docker volume nếu chưa xác nhận dữ liệu có thể bị xóa.

## 🤝 Đóng góp

Đọc [CONTRIBUTING.md](CONTRIBUTING.md) trước khi tạo nhánh, commit hoặc Pull Request.

## 📄 Giấy phép

Chưa khai báo giấy phép mã nguồn mở. Không mặc định rằng repository được cấp phép sử dụng, sửa đổi hoặc phân phối lại cho công chúng.
