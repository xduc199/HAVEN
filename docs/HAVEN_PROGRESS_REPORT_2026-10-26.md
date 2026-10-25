# Báo cáo tiến độ dự án HAVEN

**Ngày báo cáo:** 26/10/2026  
**Giai đoạn:** Thiết lập nền tảng kỹ thuật và chuẩn bị phát triển Backend  
**Repository:** `xduc199/HAVEN`  
**Nhánh tích hợp:** `develop`

---

## 1. Tổng quan

HAVEN là dự án nền tảng web hướng tới hỗ trợ người dùng khám phá và tìm kiếm bất động sản theo nhu cầu.

Trong giai đoạn hiện tại, mục tiêu chính là xây dựng nền tảng kỹ thuật có cấu trúc rõ ràng, môi trường phát triển thống nhất, quy trình kiểm soát mã nguồn và công cụ hỗ trợ kiểm thử API. Giai đoạn này tập trung vào việc chuẩn bị kiến trúc và môi trường, chưa đặt mục tiêu hoàn thiện các chức năng nghiệp vụ bất động sản.

### Mục tiêu của giai đoạn

- Chuẩn hóa môi trường .NET.
- Thiết lập cấu trúc solution theo hướng Clean Architecture.
- Chuẩn bị SQL Server bằng Docker Compose.
- Thiết lập quy trình Git và Pull Request.
- Tự động hóa kiểm tra mã nguồn bằng GitHub Actions.
- Cấu hình Scalar và OpenAPI để phục vụ quá trình phát triển API.
- Chuẩn bị quy trình chạy ứng dụng thuận tiện.
- Ghi nhận chính xác trạng thái hiện tại để làm cơ sở triển khai tiếp.

## 2. Kiến trúc và công nghệ

### 2.1. Công nghệ sử dụng

| Thành phần | Công nghệ | Trạng thái |
|---|---|---|
| Ngôn ngữ | C# | Đã xác lập |
| Framework | ASP.NET Core MVC | Đã có nền tảng |
| .NET SDK | 10.0.401 | Đã xác nhận trên máy phát triển |
| Kiến trúc | Clean Architecture định hướng phân lớp | Đã tổ chức solution |
| ORM dự kiến | Entity Framework Core 10 | Đã khai báo package hạ tầng |
| Cơ sở dữ liệu | SQL Server 2022 Developer | Đã có cấu hình Docker Compose |
| Container | Docker Compose | Đã có cấu hình |
| API documentation | OpenAPI | Đã cấu hình trên máy phát triển |
| API explorer | Scalar.AspNetCore | Đã cấu hình và người dùng xác nhận PASS |
| Unit testing | xUnit | Đã có project kiểm thử |
| Integration testing | xUnit | Đã có project kiểm thử |
| CI | GitHub Actions | Đã có workflow |
| Quản lý mã nguồn | Git và GitHub | Đang sử dụng |

### 2.2. Cấu trúc solution

```text
HAVEN/
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── Haven.Web/
│   ├── Haven.Application/
│   ├── Haven.Domain/
│   └── Haven.Infrastructure/
├── tests/
│   ├── Haven.UnitTests/
│   └── Haven.IntegrationTests/
├── docs/
│   ├── ARCHITECTURE.md
│   └── WORKFLOW.md
├── compose.yaml
├── global.json
├── HAVEN.slnx
└── README.md
```

Đây là cấu trúc nền tảng. Việc các project tham chiếu lẫn nhau không đồng nghĩa mọi luồng nghiệp vụ hoặc đăng ký Dependency Injection đã được triển khai hoàn chỉnh.

### 2.3. Vai trò các tầng

- **Haven.Domain:** Dự kiến chứa mô hình miền nghiệp vụ, thực thể và quy tắc cốt lõi. Entity chính thức cần được xác định trong giai đoạn phân tích và thiết kế.
- **Haven.Application:** Dự kiến chứa use case, DTO, interface và logic điều phối nghiệp vụ. Chưa xác nhận có use case bất động sản hoàn chỉnh.
- **Haven.Infrastructure:** Dự kiến triển khai truy cập dữ liệu và tích hợp kỹ thuật. Đã có `ApplicationDbContext` và package Entity Framework Core, nhưng chưa xác nhận cấu hình database hoàn chỉnh.
- **Haven.Web:** Điểm vào của ứng dụng ASP.NET Core MVC. Đã có cấu hình MVC, middleware và định tuyến trang web. Scalar/OpenAPI được bổ sung để phục vụ giai đoạn phát triển API.

## 3. Kết quả đã hoàn thành

### 3.1. Chuẩn hóa môi trường .NET

File `global.json` xác định SDK mục tiêu là `10.0.401`, với chính sách `rollForward` là `latestFeature`.

Lệnh kiểm tra:

```powershell
dotnet --version
```

Kết quả được cung cấp:

```text
10.0.401
```

**Trạng thái: PASS.**

### 3.2. Build solution

Đã chạy:

```powershell
dotnet build HAVEN.slnx --configuration Release
```

Kết quả người dùng cung cấp: `Build succeeded`. Cả sáu project được build thành công.

Kết quả này xác nhận mã nguồn hiện tại trên máy phát triển vượt qua bước biên dịch Release; không tự chứng minh các chức năng nghiệp vụ hoặc kết nối database hoạt động.

### 3.3. Chạy kiểm thử

Đã chạy:

```powershell
dotnet test HAVEN.slnx --configuration Release --no-build
```

Kết quả:

| Chỉ số | Kết quả |
|---|---:|
| Tổng số test | 2 |
| Thành công | 2 |
| Thất bại | 0 |
| Bị bỏ qua | 0 |

**Trạng thái: PASS về mặt thực thi kiểm thử.**

Hai file kiểm thử hiện tại vẫn là test mẫu rỗng, chưa có assertion kiểm tra hành vi nghiệp vụ thực tế. Không nên diễn giải kết quả này thành việc toàn bộ logic HAVEN đã được kiểm thử.

### 3.4. Scalar và OpenAPI

Đã cấu hình trên máy phát triển:

- Package `Scalar.AspNetCore`.
- OpenAPI thông qua `AddOpenApi()`.
- Endpoint tài liệu thông qua `MapOpenApi()`.
- Giao diện Scalar thông qua `MapScalarApiReference()`.
- Giới hạn việc hiển thị tài liệu API trong môi trường Development.

Địa chỉ dự kiến theo launch profile HTTP:

- Scalar: `http://localhost:5104/scalar`
- OpenAPI JSON: `http://localhost:5104/openapi/v1.json`

Người dùng xác nhận bước cấu hình đã PASS.

**Lưu ý về GitHub:** Cần xác nhận các thay đổi Scalar mới đã được commit và push trước khi coi chúng là một phần của nhánh `develop`.

### 3.5. Docker Compose và SQL Server

Repository đã có `compose.yaml` sử dụng SQL Server 2022 Developer.

Các điểm cấu hình chính:

- Đọc mật khẩu SQL Server từ biến môi trường `MSSQL_SA_PASSWORD`.
- Ánh xạ cổng `127.0.0.1:1433:1433`.
- Sử dụng volume `haven_sql_data` để lưu dữ liệu.
- Yêu cầu mật khẩu thông qua `.env`.
- Có `.env.example` làm mẫu cấu hình.

File `.gitignore` đã loại trừ `.env` và các file đầu ra build phổ biến.

**Trạng thái:** Đã có cấu hình trong repository. Chưa có bằng chứng xác nhận container đang chạy, ứng dụng kết nối thành công hoặc database đã được khởi tạo.

### 3.6. Tài liệu và quy trình phát triển

Các tài liệu đã được tích hợp vào `develop` qua Pull Request trước đó:

- `docs/WORKFLOW.md`: quy trình nhận nhiệm vụ, phân tích, tạo nhánh, kiểm thử, mở Pull Request và tích hợp.
- `docs/ARCHITECTURE.md`: mô tả kiến trúc hiện tại dựa trên cấu trúc project và cấu hình thực tế.

Các tài liệu nhấn mạnh việc đọc mã nguồn trước khi sửa, kiểm tra diff, chạy test và không tự giả định những chức năng chưa tồn tại.

**Trạng thái: Đã có trên nhánh `develop`.**

### 3.7. CI trên GitHub

Workflow `.github/workflows/ci.yml` đã được cấu hình để:

1. Checkout mã nguồn.
2. Cài .NET SDK 10.
3. Restore dependencies.
4. Build solution ở chế độ Release.
5. Chạy test.

Workflow được thiết lập cho các sự kiện push và Pull Request nhắm tới `main` hoặc `develop`.

**Trạng thái:** Đã có cấu hình. Chưa xác nhận một lượt chạy GitHub Actions cụ thể đã thành công.

### 3.8. Lệnh chạy ứng dụng

Đã thống nhất dùng file `run.ps1` tại thư mục gốc để chạy ứng dụng nhanh hơn. Nội dung đề xuất:

```powershell
$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

dotnet run --project src/Haven.Web/Haven.Web.csproj --launch-profile http

if ($LASTEXITCODE -ne 0) {
    Write-Error "HAVEN không thể khởi động. Vui lòng kiểm tra lỗi ở trên."
}
```

Chạy từ thư mục gốc:

```powershell
.\run.ps1
```

**Trạng thái:** Nội dung đã được thống nhất trong quá trình làm việc; cần xác nhận file đã được tạo, kiểm thử và đưa lên GitHub.

## 4. Trạng thái Backend

HAVEN hiện vẫn ở giai đoạn nền tảng kỹ thuật.

| Thành phần | Trạng thái thực tế |
|---|---|
| ASP.NET Core MVC | Đã có khung ứng dụng |
| Scalar/OpenAPI | Đã cấu hình trên máy phát triển |
| API nghiệp vụ bất động sản | Chưa triển khai |
| Entity miền nghiệp vụ chính thức | Chưa xác nhận |
| Use case và DTO nghiệp vụ | Chưa xác nhận |
| Repository và luồng truy cập dữ liệu hoàn chỉnh | Chưa xác nhận |
| Đăng ký Dependency Injection đầy đủ | Chưa xác nhận |
| Connection string ứng dụng | Chưa xác nhận hoàn chỉnh |
| EF Core migrations | Chưa xác nhận có migration |
| Database khởi tạo thành công | Chưa xác nhận |
| Authentication/JWT | Chưa triển khai hoàn chỉnh |
| Authorization theo vai trò | Chưa triển khai hoàn chỉnh |
| Kiểm thử nghiệp vụ | Chưa có bằng chứng xác nhận |

Scalar là công cụ hiển thị tài liệu và gửi request HTTP. Giao diện Scalar hoạt động không đồng nghĩa Backend nghiệp vụ đã tồn tại.

## 5. Quản lý mã nguồn và tích hợp

Repository sử dụng hai nhánh chính:

- `main`: nhánh ổn định.
- `develop`: nhánh tích hợp.

Quy trình là tạo nhánh công việc từ `develop`, thực hiện thay đổi, kiểm thử, push nhánh và mở Pull Request.

Các Pull Request trước đây đã được merge vào `develop`, bao gồm:

- PR #2: README và hướng dẫn đóng góp.
- PR #4: issue templates.
- PR #6: tài liệu quy trình phát triển.
- PR #8: tài liệu kiến trúc hiện tại.

Trạng thái đã ghi nhận trước đó: `develop` đồng bộ với `origin/develop` tại commit `efdcd66d5d2675d1af09e4486c603c0666610304`.

Đối với thay đổi Scalar và `run.ps1`, cần kiểm tra trạng thái local và tạo Pull Request mới. Không coi những thay đổi này đã được push nếu chúng chưa xuất hiện trên GitHub.

## 6. Hạng mục còn tồn tại

### Ưu tiên 1 — Chốt cấu hình nền tảng

- Đưa thay đổi Scalar/OpenAPI lên GitHub.
- Đưa file `run.ps1` lên GitHub.
- Lưu báo cáo này trong `docs/`.
- Kiểm tra diff và trạng thái working tree.
- Chạy lại build và test sau thay đổi cuối.
- Xác nhận CI trên Pull Request.

### Ưu tiên 2 — Chuẩn bị Backend

- Xác định phạm vi MVP và chức năng ưu tiên.
- Phân tích mô hình dữ liệu.
- Thiết kế entity và quan hệ dữ liệu.
- Hoàn thiện Dependency Injection giữa các tầng.
- Xác định và triển khai cấu hình database phù hợp.
- Thiết lập migration đầu tiên sau khi mô hình dữ liệu được thống nhất.

### Ưu tiên 3 — Phát triển nghiệp vụ

Sau khi hoàn tất phân tích, triển khai lần lượt các nhóm chức năng được thống nhất, chẳng hạn:

- Tài khoản và xác thực.
- Quản lý thông tin bất động sản.
- Tìm kiếm và lọc.
- Quản lý danh sách yêu thích.
- Các chức năng quản trị cần thiết.

Danh sách trên là định hướng dự kiến, không phải cam kết rằng các chức năng đã được triển khai.

### Ưu tiên 4 — Chất lượng và bảo mật

- Viết unit test có assertion thực tế.
- Xây dựng integration test kiểm tra HTTP và database khi phù hợp.
- Kiểm tra xác thực, phân quyền và validation.
- Kiểm tra xử lý lỗi, logging và cấu hình môi trường.
- Xác nhận CI chạy thành công trước khi tích hợp.

## 7. Kết luận

HAVEN đã hoàn thành phần lớn công việc thiết lập nền tảng ban đầu: chuẩn hóa SDK, tổ chức solution, chuẩn bị Docker Compose, tài liệu hóa quy trình và kiến trúc, thiết lập CI, đồng thời cấu hình Scalar/OpenAPI trên máy phát triển.

Kết quả build và test local đã được người dùng xác nhận thành công. Tuy nhiên, số lượng test còn ít và chưa kiểm tra nghiệp vụ thực tế. Kết nối database, migrations, API nghiệp vụ và xác thực vẫn là các hạng mục cần triển khai sau.

**Trạng thái phù hợp của dự án: HOÀN THÀNH GIAI ĐOẠN THIẾT LẬP NỀN TẢNG BAN ĐẦU — SẴN SÀNG CHUYỂN SANG GIAI ĐOẠN PHÂN TÍCH VÀ PHÁT TRIỂN BACKEND.**

Trước khi đóng sổ hoàn toàn trên GitHub, cần tích hợp các thay đổi local cuối cùng vào một Pull Request, xác nhận CI và cập nhật trạng thái sau khi merge.
