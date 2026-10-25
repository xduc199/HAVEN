# Kiến trúc hiện tại của HAVEN

Tài liệu này ghi lại kiến trúc có thể xác nhận từ cấu hình solution, project reference và điểm khởi động ứng dụng trên nhánh `develop`. Đây là mô tả **trạng thái mã nguồn hiện tại**, không phải cam kết rằng các chức năng nghiệp vụ bất động sản đã được triển khai đầy đủ.

## 1. Phạm vi và căn cứ

Các căn cứ đã đối chiếu:

- `HAVEN.slnx`: liệt kê bốn project ứng dụng trong `src/` và hai project kiểm thử trong `tests/`.
- Các file `.csproj`: xác nhận framework mục tiêu và quan hệ tham chiếu giữa các project.
- `src/Haven.Web/Program.cs`: xác nhận các dịch vụ MVC và pipeline HTTP được cấu hình tại điểm khởi động.
- `.github/workflows/ci.yml`: xác nhận CI restore, build và test solution.

Tài liệu chỉ khẳng định điều được hỗ trợ bởi các cấu hình này. Các chi tiết như danh sách entity, use case, repository, controller, DbContext, migration, connection string hoặc tích hợp database cần được kiểm tra từ mã nguồn tương ứng trước khi mô tả ở mức chi tiết hơn.

## 2. Tổng quan solution

```text
HAVEN.slnx
|
+-- src/
|   +-- Haven.Domain/          (net10.0)
|   +-- Haven.Application/     (net10.0) ---> Haven.Domain
|   +-- Haven.Infrastructure/  (net10.0) ---> Haven.Application
|   |                                         Haven.Domain
|   +-- Haven.Web/             (ASP.NET Core MVC, net10.0)
|       |                                      |
|       +--------------------------------------+
|             tham chiếu Application
|             và Infrastructure
|
+-- tests/
    +-- Haven.UnitTests/       (xUnit)
    +-- Haven.IntegrationTests/ (xUnit)
```

Sơ đồ thể hiện các project reference khai báo trong file project; nó **không** khẳng định mọi tầng đã được nối với nhau bằng dependency injection hoặc đã có luồng nghiệp vụ chạy xuyên suốt.

## 3. Trách nhiệm dự kiến theo project

### 3.1. `Haven.Domain`

**Vai trò theo phân lớp:** nơi đặt mô hình và quy tắc miền nghiệp vụ cốt lõi, nếu các thành phần đó đã được triển khai.

**Điều xác nhận được:** project là thư viện .NET `net10.0`, bật nullable reference types và implicit usings. Project không khai báo `ProjectReference` trong file `.csproj` đã kiểm tra.

**Cần xác minh trong mã nguồn:** các entity/value object, quy tắc bất biến, enum, domain service và event thực tế. Không nên liệt kê các entity bất động sản như `Property`, `Listing` hoặc `User` là hiện có nếu chưa tìm thấy chúng trong repository.

### 3.2. `Haven.Application`

**Vai trò theo phân lớp:** điều phối các ca sử dụng của ứng dụng và định nghĩa các hợp đồng mà tầng ngoài cần thực hiện, nếu đã được xây dựng theo hướng này.

**Điều xác nhận được:** project là thư viện .NET `net10.0` và tham chiếu `Haven.Domain`.

**Cần xác minh:** các use case/handler, DTO, validation, interface, application service và cách xử lý lỗi hiện có. Chưa có căn cứ để khẳng định đã triển khai tìm kiếm, lọc, lưu yêu thích, tài khoản hoặc quản lý tin đăng.

### 3.3. `Haven.Infrastructure`

**Vai trò theo phân lớp:** nơi đặt các adapter kỹ thuật và triển khai hợp đồng hạ tầng, nếu đã có trong mã nguồn.

**Điều xác nhận được:** project là thư viện .NET `net10.0`, tham chiếu `Haven.Application` và `Haven.Domain`. File project khai báo các package `Microsoft.EntityFrameworkCore.Design` và `Microsoft.EntityFrameworkCore.SqlServer` phiên bản `10.0.13`, cùng `System.Security.Cryptography.Pkcs` phiên bản `9.0.21`.

**Lưu ý:** việc khai báo package EF Core không tự chứng minh rằng đã có DbContext, migration, cấu hình kết nối hoặc database được khởi tạo. Những phần đó cần được xác minh trực tiếp trong mã nguồn và cấu hình runtime.

### 3.4. `Haven.Web`

**Vai trò:** điểm vào ứng dụng web ASP.NET Core MVC.

**Điều xác nhận được từ `Program.cs`:**
- Tạo builder và ứng dụng bằng `WebApplication.CreateBuilder(args)` và `builder.Build()`.
- Đăng ký MVC bằng `AddControllersWithViews()`.
- Trong môi trường không phải Development, cấu hình exception handler tại `/Home/Error` và HSTS.
- Cấu hình HTTPS redirection, routing và authorization middleware.
- Map static assets và route mặc định `{controller=Home}/{action=Index}/{id?}`.

File `Haven.Web.csproj` tham chiếu `Haven.Application` và `Haven.Infrastructure`.

**Điểm cần lưu ý:** trong `Program.cs` đã kiểm tra, chưa thấy lời gọi đăng ký dịch vụ từ hai project Application/Infrastructure, đăng ký DbContext, authentication middleware hoặc khởi tạo database. Điều này chỉ mô tả file hiện tại; trước khi kết luận về toàn bộ runtime, cần kiểm tra các extension method, cấu hình khác và mã nguồn liên quan. Middleware `UseAuthorization()` không đồng nghĩa ứng dụng đã cấu hình cơ chế xác thực người dùng.

### 3.5. Các project kiểm thử

Solution có:
- `tests/Haven.UnitTests`
- `tests/Haven.IntegrationTests`

Cả hai file project đã kiểm tra đều nhắm `net10.0` và khai báo xUnit, `Microsoft.NET.Test.Sdk`, cùng `coverlet.collector`.

Tên project cho biết ý định phân chia unit test và integration test; cần xem nội dung test thực tế để xác định hành vi, dependency và mức độ bao phủ đang được kiểm tra. Không suy ra độ bao phủ test chỉ từ việc project tồn tại.

## 4. Quan hệ phụ thuộc đang khai báo

Từ các file `.csproj` đã kiểm tra, quan hệ trực tiếp là:

| Project | Project reference trực tiếp |
|---|---|
| `Haven.Domain` | Không có trong file project đã kiểm tra |
| `Haven.Application` | `Haven.Domain` |
| `Haven.Infrastructure` | `Haven.Application`, `Haven.Domain` |
| `Haven.Web` | `Haven.Application`, `Haven.Infrastructure` |

Đây là đồ thị tham chiếu project hiện tại. Trong Clean Architecture theo hướng dependency inversion, tầng nghiệp vụ thường không phụ thuộc ngược vào implementation hạ tầng. Tuy nhiên, việc `Haven.Application` tham chiếu `Haven.Domain` và `Haven.Infrastructure` tham chiếu `Haven.Application` chưa đủ để xác nhận toàn bộ kiến trúc tuân thủ Clean Architecture; cần xem các namespace, interface và luồng gọi thực tế.

## 5. Luồng khởi động web hiện có

Luồng có thể đọc trực tiếp từ `Program.cs`:

```text
WebApplication.CreateBuilder(args)
          |
          v
AddControllersWithViews()
          |
          v
builder.Build()
          |
          v
Exception handler + HSTS (non-Development)
          |
          v
HTTPS redirection
          |
          v
Routing
          |
          v
Authorization
          |
          v
MapStaticAssets()
          |
          v
MapControllerRoute(default: Home/Index/{id?})
          |
          v
app.Run()
```

Sơ đồ này chỉ mô tả pipeline được cấu hình tại file đã kiểm tra. Không suy ra rằng request đã gọi application service hoặc truy cập SQL Server khi chưa có bằng chứng về controller, đăng ký DI và data access.

## 6. Cơ sở dữ liệu và hạ tầng

Các tài liệu cấu hình repository mô tả SQL Server chạy bằng Docker Compose cho môi trường phát triển, và project Infrastructure khai báo package EF Core SQL Server.

Tuy nhiên, để xác nhận ứng dụng kết nối được cơ sở dữ liệu, cần kiểm tra đồng thời:

1. Nội dung `compose.yaml` và các biến môi trường được sử dụng.
2. Connection string thực tế được đọc từ cấu hình runtime (không ghi bí mật vào tài liệu).
3. Vị trí đăng ký DbContext và dịch vụ hạ tầng.
4. DbContext, entity mapping và migration nếu có.
5. Quy trình áp dụng migration hoặc khởi tạo schema.
6. Kiểm thử kết nối và thao tác dữ liệu.

Không đưa password, token hoặc connection string có thông tin xác thực vào tài liệu, log hay repository.

## 7. CI và kiểm thử tự động

Theo `.github/workflows/ci.yml`, workflow `HAVEN - CI` chạy khi push hoặc mở Pull Request vào `main` và `develop`. Các bước gồm:

1. Checkout source.
2. Thiết lập .NET SDK `10.0.x`.
3. `dotnet restore HAVEN.slnx`.
4. `dotnet build HAVEN.slnx --configuration Release --no-restore`.
5. `dotnet test HAVEN.slnx --configuration Release --no-build`.

CI xác nhận solution có thể được restore/build/test trong môi trường workflow tại thời điểm chạy; điều này không tự chứng minh mọi chức năng nghiệp vụ, kết nối database hoặc kịch bản production đã được xác minh.

## 8. Trạng thái xác minh

| Hạng mục | Trạng thái theo bằng chứng đã xem |
|---|---|
| Solution có Web, Application, Domain, Infrastructure | Đã xác nhận từ `HAVEN.slnx` |
| Unit test và integration test projects tồn tại | Đã xác nhận từ `HAVEN.slnx` |
| Project reference giữa các tầng | Đã xác nhận từ các file `.csproj` |
| Web đăng ký MVC và cấu hình route mặc định | Đã xác nhận từ `Program.cs` |
| EF Core SQL Server package được khai báo | Đã xác nhận từ `Haven.Infrastructure.csproj` |
| DbContext, migration và database initialization | Chưa xác minh trong phạm vi file đã đọc |
| Dependency injection cho Application/Infrastructure | Chưa thấy trong `Program.cs` đã đọc; cần kiểm tra toàn bộ mã nguồn |
| Use case và entity nghiệp vụ bất động sản | Chưa xác minh từ mã nguồn |
| Authentication đã cấu hình đầy đủ | Chưa xác minh |
| Chất lượng/độ bao phủ test | Chưa đánh giá từ nội dung test |
| Khả năng kết nối SQL Server từ ứng dụng | Chưa xác minh |

## 9. Các bước xác minh kiến trúc tiếp theo

Trước khi mở rộng tài liệu thành sơ đồ class hoặc sequence diagram chi tiết, hãy kiểm tra trên nhánh `develop`:

1. Liệt kê file trong bốn project `src/`.
2. Đọc `Program.cs`, các controller, service-registration extension và cấu hình ứng dụng.
3. Tìm interface/implementation và xác định chiều gọi giữa Web, Application, Domain và Infrastructure.
4. Tìm DbContext, entity configuration, migrations và nơi đọc connection string.
5. Đọc các test hiện có để nối từng hành vi với bằng chứng kiểm thử.
6. Cập nhật tài liệu khi đã có bằng chứng; giữ các điểm chưa rõ ở trạng thái “chưa xác minh”.

## 10. Tài liệu liên quan

- [README.md](../README.md) — trạng thái repository và cách chạy local.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — quy ước đóng góp.
- [WORKFLOW.md](WORKFLOW.md) — quy trình phát triển.
- [CI workflow](../.github/workflows/ci.yml) — restore, build và test tự động.
- [Solution file](../HAVEN.slnx) — danh sách project trong solution.

> Tài liệu này cần được cập nhật cùng với thay đổi kiến trúc. Khi một thành phần chưa được xác nhận từ mã nguồn, hãy ghi rõ điều đó thay vì mô tả như một tính năng đã hoàn thành.
