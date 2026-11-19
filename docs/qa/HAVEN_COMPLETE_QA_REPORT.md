# HAVEN — Complete QA Report

Ngày: 2026-11-19  
Kết luận: **PARTIALLY COMPLETED**

## 1. Tóm tắt

Đã sửa ba lỗi có căn cứ trong giao diện/motion: Home giữ lưới tìm kiếm năm cột xuống tablet; desktop không có liên kết Register riêng trong khi CTA “Đăng tin” dẫn người chưa đăng nhập sang luồng đăng nhập; và scroll reveal xóa marker cần thiết cho transition. Đã kiểm thử đăng ký/đăng nhập/logout, tạo và gửi 10 tin QA qua API thật trên SQL Server local, kiểm tra 49 API operation, HTTP smoke test các trang Public/User/Admin, build và toàn bộ unit/integration tests.

Chưa thể kết luận nghiệm thu hoàn tất vì không thực hiện được visual QA trong trình duyệt và không có tài khoản/quyền Admin hợp lệ để kiểm thử duyệt tin. Cổng quyền của Browser Use từ chối mở `http://localhost:5104` và thông báo người dùng đã từ chối quyền truy cập origin; tôi không tìm cách vượt chặn này.

## 2. Git và bảo toàn dữ liệu

- Branch: `develop`.
- HEAD trước và sau: `5fdad8cc5c3c53942ba18afd83b22260d669cf54` (không đổi).
- Working tree trước khi tiếp tục: 8 file tracked đã sửa và 1 file chưa theo dõi (`haven-tokens.css`).
- Working tree hiện tại: 10 file tracked đã sửa và 3 file chưa theo dõi (token CSS cùng hai báo cáo QA).
- `git diff --check`: exit code 0.
- Không reset, clean, checkout, đổi branch, commit hoặc push.
- Backup an toàn trước các chỉnh sửa của lượt này nằm tại `%TEMP%\haven-qa-safety-20261119-154506`, kèm SHA-256 cho 9 file đang dirty lúc tạo backup.

### Sự cố `home.css`

`home.css` đã bị ghi đè trong lần làm việc trước và được dựng lại từ nội dung commit gần nhất cùng phần brand/motion. Lượt này không tìm thấy Local History phù hợp cho `home.css`; `git log --follow` không có phiên bản sau commit gốc chứa nội dung unstaged. Vì vậy không có bằng chứng để khôi phục chính xác CSS chưa commit đã mất. File hiện tại được giữ nguyên và chỉ bổ sung các điều chỉnh responsive trong báo cáo này; không tuyên bố đó là bản phục hồi nguyên trạng. Bản backup của lượt này bảo vệ trạng thái hiện tại, không khôi phục được nội dung đã mất trước đó.

## 3. Frontend và Home

### Lỗi và thay đổi

- `.haven-home__search-form` giữ năm cột tới breakpoint 640px. Ở vùng 641–960px, các trường bị ép vào một hàng hẹp. Đã chuyển vùng này sang hai cột; keyword và nút tìm kiếm chiếm toàn hàng. Dưới 640px vẫn xếp một cột.
- CTA cuối Home giữ flex ngang ở tablet; đã bật wrap và căn đầu để tránh ép nội dung với nút CTA.
- Scroll reveal thêm class `.is-visible` rồi xóa ngay `data-motion-pending`, làm mất selector đang định nghĩa transition và khiến reveal có thể nhảy. Đã giữ marker sau khi observer hiện phần tử; reduced-motion vẫn bỏ qua reveal theo điều kiện sẵn có.
- Desktop header đã có Register riêng, cạnh Login; mobile menu vẫn giữ Login/Register. Thêm liên kết chuyển đổi ở cuối form Login và Register.
- Các chỉnh sửa của lượt này nằm trong [`home.css`](../../src/Haven.Web/wwwroot/css/home.css), [`site.js`](../../src/Haven.Web/wwwroot/js/site.js), [`_Layout.cshtml`](../../src/Haven.Web/Views/Shared/_Layout.cshtml), [`Login.cshtml`](../../src/Haven.Web/Views/Account/Login.cshtml), [`Register.cshtml`](../../src/Haven.Web/Views/Account/Register.cshtml) và [`frontend.css`](../../src/Haven.Web/wwwroot/css/frontend.css). Các file branding, layout và admin khác đã dirty trước lượt này và được giữ nguyên.

### Kiểm tra thực tế và giới hạn

- HTTP Home trả 200; HTML có đủ 10 section, stylesheet Home và ảnh hero đều trả 200.
- GET `/account/register` và `/account/login` trả 200; HTML của mỗi trang có link sang trang còn lại; trang chủ đã render link Register.
- `/profile/listings/create` khi anonymous chuyển 302 sang Login với `ReturnUrl` đúng. Login thành công quay lại đúng route và route đích trả 200.
- CSS hiện có breakpoint mobile 640px và tablet 960px. **Không có viewport nào được xác minh bằng browser**: 1440, 1366, 1280, 1024, 768, 430, 390, 375, 360 đều chưa visual-check. Tràn ngang, tỷ lệ ảnh, animation và console trên trình duyệt vì vậy còn **BLOCKED**.

## 4. Đăng ký, đăng nhập, đăng xuất

- Source có `AccountController.Register` GET/POST tại `/account/register` và view đúng vị trí `Views/Account/Register.cshtml`; API đăng ký là `POST /api/auth/register`.
- Register yêu cầu tên, email, password/confirm, đồng ý Điều khoản và xử lý dữ liệu cá nhân; Identity password tối thiểu 12 ký tự, có chữ hoa/thường, số và ký tự đặc biệt. Email trùng trả validation error (API 400). Email xác nhận hiện không được cấu hình; Register thành công cấp role `User`, tạo cookie và redirect về Home.
- Login GET/POST có validation; mật khẩu sai API trả 401, thông tin đúng trả 200. MVC login hợp lệ trả 302; redirect `ReturnUrl` nội bộ về luồng đăng tin đã được kiểm tra. Redirect sang vùng Admin bị chặn theo code.
- Logout MVC trả 302 về Home; logout API trả 204. Sau logout, `GET /api/auth/me` trả 401.
- Tạo account QA qua API thành công sau khi áp dụng migration local; cookie, user identity và role `User` được xác nhận.
- MVC Register duplicate trả lại form 200; API duplicate trả 400. Form Login thiếu trường trả lại view 200 với ModelState.

## 5. Database và dữ liệu QA

Ứng dụng chạy ở `Development`, connection target xác nhận là SQL Server local `.\SQLEXPRESS`, database `HavenDb`; không dùng production. Đăng ký ban đầu trả 500 vì database thiếu migration `AddPolicyConsentAndPrivacyRequests`. Đã đọc migration: migration chỉ tạo `PolicyConsents`, `PrivacyRequests` và index, không xóa bảng/dữ liệu hiện có; sau đó áp dụng migration này lên database local để kiểm thử đăng ký. Không chạy migration trên môi trường dùng chung hoặc production.

### Tài khoản và tin đăng

- Tạo ba tài khoản QA với email `haven.qa.user+20261119-01@example.test`, `...-02@example.test`, `...-03@example.test`; cả ba là role `User`. Không ghi password vào báo cáo.
- Tài khoản `...-03` (user ID `1c2defca-9268-42bd-aad7-46f163190010`) là chủ sở hữu 10 tin; ID tin `4–13`.
- Hai loại đang active trong danh mục phù hợp với dữ liệu thực có là `Chung cư` và `Khu đất`; phân bổ 6 và 4 tin, giá/diện tích/phòng và mua-bán/cho-thuê có biến thể. Không tạo loại bất động sản mới.
- Tất cả tiêu đề có tiền tố `[HAVEN QA]`, mô tả/địa chỉ ghi rõ dữ liệu chỉ dùng QA. Tạo qua `POST /api/my/listings`, cập nhật tin đầu khi còn Draft, sau đó gửi cả 10 qua `/submit`.
- Trạng thái hiện tại của cả 10: `Pending` (giá trị domain 2). Public search `[HAVEN QA]` trả 0 và chi tiết tin Pending trả 404. Không tự duyệt tin.
- Không upload ảnh/video để tránh tạo file media không thể dọn qua trạng thái Pending. Route media chỉ được kiểm tra với ID không tồn tại.
- Không dọn dữ liệu: domain chỉ cho chủ sở hữu xóa Draft/Rejected; 10 tin đang Pending. Không có API xóa account và không có phiên Admin được cấp. Hai account QA đầu không có listing; account thứ hai phát sinh trong lần thử tạo dữ liệu bị lỗi cú pháp script, được giữ lại thay vì sửa trực tiếp database. Có 3 user QA và 10 listing QA còn trong database local.

## 6. API và E2E

- OpenAPI có 49 operation; từng operation được gọi ít nhất một lần. Chi tiết method/route/status nằm trong [`HAVEN_API_TEST_REPORT.md`](HAVEN_API_TEST_REPORT.md).
- 22/22 API Admin trả 403 khi caller role `User`; đây là bằng chứng authorization hoạt động, không phải kiểm thử thành công nghiệp vụ Admin. Không có cấu hình bootstrap Admin trong môi trường này và tôi không tự nâng quyền user.
- Public/user pages được HTTP smoke-test: Home, Properties, 2 chi tiết công khai, compare, chính sách công khai, Contact, Login/Register, profile, change password, privacy center, listing list/create/detail, favorites và consultations đều trả 200. Home/MuaBan và Home/ChoThue chuyển 302 tới filter tương ứng.
- Các route `/Admin`, Dashboard, Listings, Users, PropertyTypes, Tags, Consultations, Reports, PrivacyRequests đều chuyển tới AccessDenied với user thường.
- Đã kiểm tra asset hero/CSS/JS trả 200; media ID không tồn tại trả 404.
- Không có ảnh/video lỗi hoặc console/network browser để đánh giá vì browser origin bị chặn. E2E admin duyệt/từ chối, test upload hợp lệ và consultation POST hợp lệ chưa chạy.

## 7. Build, tests và kiểm tra tĩnh

- Build thường ban đầu không thể copy apphost vì `Haven.Web.exe` đang được tiến trình local port 5104 sử dụng. Build `UseAppHost=false` cũng bị khóa DLL.
- Build lại sang output tạm thành công: 0 warning, 0 error. Unit tests chạy từ output tạm có một test kiến trúc fail do test dò repository root từ thư mục output tạm; khi chạy từ working directory chuẩn, toàn bộ test pass.
- `dotnet test HAVEN.slnx --no-build --no-restore`: 74 unit tests pass, 75 integration tests pass, 149/149.
- `node --check` chạy trên `site.js`, `home.js`, `api-forms.js`; không báo lỗi cú pháp.
- `git diff --check`: pass.

## 8. Trạng thái cuối và việc còn lại

**PARTIALLY COMPLETED** vì browser visual QA và luồng Admin cần tài khoản có quyền hợp lệ chưa thể xác minh. Các lỗi đã sửa được xác nhận qua source và HTTP; responsive CSS có thay đổi nhưng chưa có đánh giá bằng trình duyệt.

Để hoàn tất nghiệm thu cần cho phép Browser Use tới origin local, rồi kiểm tra các viewport đã liệt kê và console/network; đồng thời cung cấp cơ chế/tài khoản Admin hợp lệ để duyệt hoặc từ chối một tin Pending. Không yêu cầu tạo hoặc nâng quyền Admin trong lần này.

## 9. Files

Tracked files đang thay đổi sau lượt này:

- `src/Haven.Web/Areas/Admin/Views/Shared/_AdminLayout.cshtml`
- `src/Haven.Web/Views/Account/Login.cshtml`
- `src/Haven.Web/Views/Account/Register.cshtml`
- `src/Haven.Web/Views/Home/Index.cshtml`
- `src/Haven.Web/Views/Shared/_Layout.cshtml`
- `src/Haven.Web/wwwroot/css/admin.css`
- `src/Haven.Web/wwwroot/css/frontend.css`
- `src/Haven.Web/wwwroot/css/home.css`
- `src/Haven.Web/wwwroot/css/site.css`
- `src/Haven.Web/wwwroot/js/site.js`

Untracked files:

- `src/Haven.Web/wwwroot/css/haven-tokens.css` (đã tồn tại trước lượt này)
- `docs/qa/HAVEN_API_TEST_REPORT.md`
- `docs/qa/HAVEN_COMPLETE_QA_REPORT.md`
