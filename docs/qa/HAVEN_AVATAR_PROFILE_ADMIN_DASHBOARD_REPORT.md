# HAVEN — Avatar, Public Profile & Admin Dashboard Report

## Kết luận

**FIXED (SQL schema mismatch).** Migration đã được áp dụng thành công vào SQL Server Development local đã xác minh. Ứng dụng đã khởi động lại và các truy vấn EF/Identity đọc `AvatarPath` thành công. Visual QA và các màn yêu cầu đăng nhập chưa được xác minh bằng browser.

## Repository trước khi sửa

- Branch: `develop`
- HEAD: `5fdad8cc5c3c53942ba18afd83b22260d669cf54`
- Working tree đã có nhiều thay đổi chưa commit từ các đợt trước, gồm giao diện Home, login/register, layout public/Admin, CSS/JS, sửa lockout Admin, integration tests và các báo cáo QA. Các thay đổi đó được giữ nguyên.
- Không reset, clean, checkout, commit hoặc push.
- Đã sao lưu trước các file dirty có liên quan vào thư mục task tạm trong `%TEMP%`.

## A. Avatar

- Thêm `AvatarPath` nullable vào `HavenUser` và migration `20261119170000_AddUserAvatar` cho `AspNetUsers`.
- Upload nằm tại `POST /account/profile/avatar`, chỉ cho người dùng đăng nhập và lấy user từ principal hiện tại; request không nhận UserId đích.
- Lưu dưới `wwwroot/uploads/avatars` bằng tên GUID sinh phía server. Chấp nhận JPEG/PNG/WebP theo MIME và magic bytes; giới hạn 5 MB; không dùng tên file gửi lên; xóa avatar cũ sau khi cập nhật Identity thành công.
- Thêm preview, hủy, trạng thái chờ gửi và thông báo qua feedback hiện có. Ảnh lỗi được ẩn để hiện chữ cái fallback.
- Partial avatar dùng chung tại header public, header Admin, bảng User Management, trang hồ sơ cá nhân, trang hồ sơ công khai và khối người đăng trên trang chi tiết tin.
- Đã kiểm thử dịch vụ lưu trữ với magic bytes hợp lệ, MIME không khớp, vượt dung lượng, tên file GUID và thao tác xóa. Chưa kiểm thử tải ảnh qua form đã đăng nhập và xác nhận persist trên SQL Server thật.
- Migration chưa được áp dụng. Cần chạy migration trên database Development phù hợp trước khi dùng tính năng trên database có schema cũ.

## B. Public Profile

- Route: `/members/{id}`. ID là khóa Identity hiện có; email, số điện thoại, role, password hash và các trường nội bộ không được đưa vào ViewModel.
- Profile hiển thị tên, avatar, tổng số tin đang được công khai và danh sách tin đã duyệt, phân trang 12 tin/trang.
- Tin nháp, chờ duyệt, từ chối hoặc trạng thái khác không được truy vấn vào danh sách công khai.
- Liên kết từ người đăng trong Property Details dùng truy vấn riêng theo listing đã duyệt. API `PublicListingDetails` giữ nguyên shape; test hiện có tiếp tục xác nhận API không trả ownerId/email.
- Tái sử dụng partial `_PropertyCards` để trình bày danh sách tin.
- Integration tests xác nhận khách chưa đăng nhập xem được profile, profile lạ trả 404, chỉ tin Approved xuất hiện và không lộ email.

## C. Admin Dashboard và biểu đồ

- Nguyên nhân biểu đồ: cột đặt `height` theo phần trăm trong vùng `.admin-chart` chỉ có `min-height`, nên chiều cao cha không xác định để tính tỷ lệ phần trăm. Công thức cũ còn ép mọi cột tối thiểu 3%, làm dữ liệu 0/số nhỏ bị sai.
- Sửa bằng chiều cao vùng vẽ xác định 15rem và hàm tỷ lệ `ChartBarScale`; số 0 có chiều cao 0, giá trị lớn nhất 100%, các giá trị khác theo tỷ lệ thực, có tối thiểu 1% để cột dương nhỏ vẫn nhìn thấy.
- Trục và số đếm dùng dữ liệu dashboard hiện có; không thêm dữ liệu mẫu. Thêm quick action Quản lý người dùng và phần mô tả dashboard. Trạng thái rỗng/validation hiện có được giữ lại.
- Unit tests bao phủ `[2, 8, 3, 15, 1]`, toàn 0, một giá trị, các giá trị bằng nhau và outlier `[1, 100, 2, 3, 4]`.

## File thay đổi trong đợt này

- Application: `ChartBarScale.cs`, `PublicOwnerProfile.cs`, `IListingRepository.cs`, `ListingApplicationService.cs`.
- Infrastructure: `HavenUser.cs`, `EfListingRepository.cs`, `ApplicationDbContextModelSnapshot.cs`, migration `20261119170000_AddUserAvatar.cs`.
- Web: `AccountController.cs`, `MembersController.cs`, `PropertiesController.cs`, `Program.cs`, ViewModels Account/Admin/Properties và `PublicProfileViewModel.cs`.
- Views: Account Profile, Property Details, Members Profile, shared Avatar partial, AccountAvatar ViewComponent, public/Admin layouts, Admin User Management, Admin Dashboard và Analytics Charts.
- Assets: `AvatarImageStorage.cs`, `avatar.js`, `avatar-placeholder.svg`, cập nhật `frontend.css` và `admin.css`.
- Tests: `ChartBarScaleTests.cs`, `AvatarImageStorageTests.cs`, `PublicMemberProfileTests.cs`; cập nhật fake `IListingRepository` trong `PublicListingsApiTests.cs`.

Các file dirty khác từ trước được giữ nguyên, gồm Login/Register/Home, `home.css`, `site.css`, `site.js`, `home.js`, `admin.js`, `MvcWorkflowTests.cs`, `haven-tokens.css`, `auth.css`, ảnh Home và báo cáo QA cũ.

## Kiểm tra

- `dotnet build HAVEN.slnx --no-restore`: **PASS**, 0 warning, 0 error. Build dùng output tạm để tránh ghi đè/đụng file output ứng dụng đang tồn tại.
- Unit tests: **82/82 PASS**.
- Integration tests: **81/81 PASS**.
- JavaScript syntax (`node --check avatar.js`): **PASS**.
- `git diff --check`: **PASS**; chỉ có cảnh báo line ending CRLF/LF trên các file dirty có sẵn.
- Browser visual QA: **BLOCKED**. Runtime Computer Use không khởi tạo được (`failed to write kernel assets: The system cannot find the path specified`), nên không có screenshot thật và không kết luận layout đã nghiệm thu trực quan.
- Database: migration đã áp dụng; không có dữ liệu người dùng bị thay đổi hoặc xóa.

## Còn cần xác minh

1. Thử upload avatar qua form bằng tài khoản thật và xác nhận bản ghi lưu bền vững trên SQL Server.
2. Kiểm tra qua browser các trang yêu cầu đăng nhập (Profile, Admin Dashboard và User Management).
3. Kiểm tra trực quan desktop/mobile, dropdown tài khoản, profile, Property Details và Dashboard chart.
4. Xác nhận avatar hiển thị qua browser cho header và Admin khi có phiên hợp lệ; test tự động không thay thế live browser.

Không có ảnh chụp trình duyệt đính kèm trong báo cáo này vì browser không chạy được trong phiên kiểm thử.


## Cập nhật 2026-11-19 — Khắc phục SQL Server `AvatarPath`

- Branch/HEAD trước thao tác: `develop` / `5fdad8cc5c3c53942ba18afd83b22260d669cf54`. Working tree vẫn giữ nguyên các thay đổi chưa commit; không reset/clean/checkout/commit/push.
- DbContext: `Haven.Infrastructure.Context.ApplicationDbContext`; DbContext và migrations assembly/project: `src/Haven.Infrastructure`; startup project: `src/Haven.Web`; provider: SQL Server.
- Launch profile `http` đặt environment `Development`, URL `http://localhost:5104`; profile `https` cũng dùng `Development`. Không có biến môi trường ghi đè connection string và không tìm thấy User Secrets file tương ứng. `appsettings.Development.json` trỏ bằng Windows integrated authentication tới `xduc\SQLEXPRESS`, database `HavenDb`. Không có credential được ghi vào báo cáo.
- SQL xác nhận server `xduc\SQLEXPRESS`, database `HavenDb`. Trước migration: `AspNetUsers` có 5 user, chưa có `AvatarPath`, migration chưa có trong history. EF migrations list đánh dấu migration là Pending.
- Backup trước cập nhật schema: `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\HavenDb_pre_AddUserAvatar_20261119.bak`. `BACKUP DATABASE ... WITH COPY_ONLY, CHECKSUM` thành công; `RESTORE VERIFYONLY ... WITH CHECKSUM` báo bộ backup hợp lệ.
- Áp dụng migration hiện có `20261119170000_AddUserAvatar` bằng EF Core, không tạo migration mới. EF thực hiện đúng một thay đổi schema: `ALTER TABLE [AspNetUsers] ADD [AvatarPath] nvarchar(256) NULL`, rồi ghi migration vào history.
- Xác minh sau migration: metadata trả `AvatarPath | nvarchar | YES | 256`; `__EFMigrationsHistory` có migration này; số user vẫn là 5. Các cột Identity khác không bị migration sửa.
- Khởi động lại ứng dụng từ repo bằng `dotnet run --no-build --no-restore --project src/Haven.Web --launch-profile http`. Log xác nhận environment `Development`, URL `http://localhost:5104`; không có `Invalid column name 'AvatarPath'`.
- GET smoke trả 200 cho Home, Login, Property Details và Public Profile. EF log cho thấy truy vấn từ `UserManager.Users`/Identity đọc `AspNetUsers.AvatarPath` thành công trên database mục tiêu.
- Build `dotnet build HAVEN.slnx --no-restore`: PASS, 0 warning/0 error. `dotnet test HAVEN.slnx --no-build`: Unit 82/82 PASS, Integration 81/81 PASS.
- Chưa có browser visual QA. Các màn yêu cầu đăng nhập/Admin và thao tác upload ảnh qua form trên phiên người dùng thật chưa kiểm tra. Khi dùng profile HTTP, middleware có cảnh báo không xác định HTTPS port; các GET HTTP ở trên trả 200. Đây không ảnh hưởng lỗi schema.

**Kết quả lỗi gốc: FIXED** trên Development DB `HavenDb` tại `xduc\SQLEXPRESS`. Visual/authenticated QA của toàn bộ tính năng Avatar vẫn cần kiểm tra riêng.
