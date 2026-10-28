# Báo cáo tiến độ — Hoàn thiện các gap HAV-05

**Ngày kiểm tra:** 27/10/2026
**Repository:** `xduc199/HAVEN`
**Branch làm việc:** `feature/HAV-05-property-gaps`
**Branch nền:** `develop` tại `9c5d2c8` (`Merge pull request #19 from xduc199/main`)
**Trạng thái tại lúc lập báo cáo:** Đã triển khai và kiểm tra local; chưa commit, push hoặc tạo Pull Request.

## 1. Phạm vi

Rà soát và hoàn thiện các gap được giao cho HAV-05: đọc chi tiết tin công khai, application use cases cho moderation, kiểm thử ownership/state transitions, validation độ dài thống nhất với EF mapping và kiểm tra route/response Web.

## 2. Kết quả theo mục tiêu

### A. Chi tiết listing công khai — hoàn thành

- Bổ sung `IListingRepository.GetPublicByIdAsync`; EF query chỉ truy xuất listing có ID tương ứng và `Status == Approved`.
- Bổ sung `ListingApplicationService.GetPublicDetailsAsync`; trả `null` nếu không có listing hoặc trạng thái không phải `Approved`.
- Bổ sung DTO `PublicListingDetails`, chỉ chứa các trường công khai cần cho trang chi tiết; không có `OwnerId` hoặc `RejectionReason`.
- Thêm `GET /api/listings/{id}`. ID không dương trả `400 Bad Request`; listing không tồn tại hoặc không công khai trả `404 Not Found`.
- Map attribute-routed controllers và xác nhận route xuất hiện trong OpenAPI Development.

Contract phản hồi `200 OK`: `Id`, `PropertyTypeId`, `Title`, `Description`, `Purpose`, `PriceAmount`, `AreaSqm`, `Province`, `District`, `AddressLine`, `Bedrooms`, `Bathrooms`, `CreatedAtUtc`.

### B. Application use cases moderation — hoàn thành ở tầng Application

- Bổ sung `ApproveAsync`, `RejectAsync` và `HideAsync`. Các use case tải listing, gọi hành vi chuyển trạng thái domain hiện có, lưu qua repository và trả `null` nếu không tìm thấy.
- Domain tiếp tục là nơi kiểm tra state transition; trường hợp trạng thái không hợp lệ ném `InvalidOperationException` và không gọi lưu.
- Lý do từ chối rỗng/whitespace bị từ chối bởi domain; dữ liệu không hợp lệ không được lưu.

**Authorization:** rà soát `Program.cs` và Web source không thấy ASP.NET Core Identity, authentication hoặc role authorization được cấu hình. Vì vậy không tạo endpoint moderation có thể gọi qua HTTP. Các use case chỉ được thêm ở Application; cần hoàn thiện cơ chế xác thực và bảo vệ Admin trước khi công khai thao tác moderation.

### C. Unit tests — hoàn thành

- Bao phủ public detail với `Approved`, `Draft`, `Pending`, `Rejected`, `Hidden` và listing không tồn tại.
- Bao phủ update thành công, update làm listing đã duyệt quay về `Pending`, ownership sai, property type inactive; bao phủ gửi duyệt và ownership sai.
- Bao phủ approve/reject/hide thành công, reason không hợp lệ, transition sai và listing không tồn tại.
- Thêm kiểm tra độ dài đúng giới hạn và vượt giới hạn cho các trường Listing; thêm boundary test cho rejection reason.

### D. Domain validation và EF mapping — hoàn thành

- Domain áp dụng giới hạn: `OwnerId` 450, `Title` 200, `Description` 5000, `Province` 100, `District` 100, `AddressLine` 300, `RejectionReason` 1000 ký tự.
- EF configuration dùng các hằng số giới hạn từ domain để tránh lệch giá trị.
- Không thay đổi schema; không tạo migration.

### E. Web integration và OpenAPI — hoàn thành trong phạm vi test hiện có

- Integration tests xác nhận HTTP status, DTO không lộ field nội bộ, ID không hợp lệ, listing không public/không tồn tại và OpenAPI có route.
- Test Web thay repository bằng stub; chưa chạy truy vấn EF với SQL Server thật.

## 3. File thay đổi

### Source

- `src/Haven.Application/Abstractions/IListingRepository.cs`
- `src/Haven.Application/Listings/ListingApplicationService.cs`
- `src/Haven.Application/Listings/PublicListingDetails.cs` (mới)
- `src/Haven.Domain/Properties/Listing.cs`
- `src/Haven.Infrastructure/Configurations/ListingConfiguration.cs`
- `src/Haven.Infrastructure/Repositories/EfListingRepository.cs`
- `src/Haven.Web/Controllers/PublicListingsController.cs` (mới)
- `src/Haven.Web/Program.cs`

### Tests

- `tests/Haven.UnitTests/Listings/ListingApplicationServiceTests.cs`
- `tests/Haven.UnitTests/Properties/ListingLengthValidationTests.cs` (mới)
- `tests/Haven.IntegrationTests/PublicListingsApiTests.cs` (mới)

### Báo cáo

- `docs/HAV-05-property-gaps/PROGRESS_REPORT.md` (tài liệu này)

## 4. Build và test

Các lệnh đã chạy trên branch làm việc:

```powershell
dotnet build .\HAVEN.slnx --configuration Release
dotnet test .\HAVEN.slnx --configuration Release
git diff --check
```

Kết quả lần chạy cuối:

- Build: thành công, 0 warning, 0 error.
- Unit tests: 61 passed, 0 failed, 0 skipped.
- Integration tests: 8 passed, 0 failed, 0 skipped.
- Tổng cộng: 69 passed, 0 failed, 0 skipped.
- `git diff --check`: không báo lỗi whitespace. Git có cảnh báo quy đổi LF sang CRLF cho `ListingApplicationServiceTests.cs`; đây không phải lỗi kiểm tra.

## 5. Database, migration, API và cấu hình

- Không đổi schema, migration, connection string hoặc cấu hình database.
- Không thêm package.
- API mới chỉ phục vụ public detail; chưa có API moderation.
- `GET /api/listings/{id}` được ghi nhận trong OpenAPI Development.
- Chưa có bằng chứng test EF query thực tế với SQL Server; chỉ xác minh biểu thức truy vấn trong `EfListingRepository` và HTTP flow với repository stub.

## 6. Git, PR và trạng thái working tree

- Lúc bắt đầu: `develop` sạch và đồng bộ tại `9c5d2c8`.
- Branch mới: `feature/HAV-05-property-gaps`, tạo từ `develop` sau `fetch`/`pull --ff-only`.
- Tại lúc ghi nhận báo cáo có 7 file đã sửa và 5 file mới chưa được stage, gồm báo cáo này.
- Tại thời điểm đó chưa commit, push, tạo PR hoặc merge.
- CI/GitHub Actions chưa được chạy hoặc xác minh.

## 7. Giới hạn và bước tiếp theo

1. Người 4 có thể tích hợp public detail theo route và response contract ở mục 2A.
2. Trước khi tạo moderation endpoint, cần có authentication và authorization Admin phía server; không dùng dữ liệu client để giả lập quyền.
3. Nếu cần xác nhận hành vi query/persistence, bổ sung integration test dùng SQL Server/test database được nhóm duyệt; hiện test Web dùng stub và không xác minh DB.
4. Review diff, sau đó người dùng quyết định có stage/commit/push và mở PR với base `develop` hay không.
