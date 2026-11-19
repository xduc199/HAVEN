# HAVEN Responsive Design QA Report

**Kết luận chung: PARTIALLY COMPLETED. Lượt live visual QA hiện tại: BLOCKED.**

## Tổng quan

- Branch: `develop`
- HEAD trước và sau: `5fdad8cc5c3c53942ba18afd83b22260d669cf54` (không đổi)
- Trước khi sửa: 10 file tracked đã thay đổi, 3 file chưa theo dõi; không có thay đổi staged.
- Không reset, clean, checkout, commit hoặc push. Không sửa view, routing, API, authentication, database hay dữ liệu.
- Đã sao lưu các stylesheet, layout và QA report đang có thay đổi vào `%TEMP%\haven-responsive-safety-20261119-162041` trước khi chỉnh sửa.
- Kiểm kê được 48 Razor views: 9 public, 13 user/authentication, 19 admin và 7 shared/other. Không có view nào bị bỏ khỏi kiểm kê; lần này thay đổi tập trung ở hệ thống CSS dùng chung.

## Nguyên nhân tìm thấy trong source

1. Breakpoint header cũ chỉ chuyển chế độ ở 900px (frontend) và 960px (Home), trong khi header có nhiều liên kết điều hướng và nhóm hành động tài khoản. Ở vùng desktop hẹp hơn, các phần tử có thể co quá mức hoặc quấn không chủ đích.
2. Mobile menu chỉ được ẩn trong `home.css`, file chỉ tải ở Home. Do đó layout công khai dùng chung có thể hiện cả điều hướng desktop lẫn menu mobile ngoài Home. Khai báo `display:grid` trực tiếp trên nội dung `<details>` còn có thể ghi đè trạng thái đóng mặc định của trình duyệt.
3. `haven-tokens.css` áp dụng `overflow-wrap:anywhere` cho cả link, button và control, nên chữ có thể bị ngắt giữa từ ngay cả khi còn không gian ngang.
4. Home giữ các section nhiều cột đến tablet ở khoảng 768–960px; phần manifesto, editorial, FAQ và thông tin minh bạch chưa có cách chuyển cột đồng bộ.
5. Bảng dữ liệu quản trị/chung không đặt chiều rộng nội tại có chủ đích, nên cột có thể bị ép quá chặt hoặc làm tràn khung nhỏ.

Đây là kết luận từ cascade và markup trong source; chưa được xác nhận bằng cách render trang tại các kích thước màn hình vì browser QA bị chặn ở mục “Visual QA”.

## Những thay đổi trong lượt này

- `src/Haven.Web/wwwroot/css/haven-tokens.css`: thay cách ngắt chữ toàn cục bằng `break-word`, giữ khả năng xử lý chuỗi dài mà không ép mọi nhãn/nút bẻ dòng tùy tiện.
- `src/Haven.Web/wwwroot/css/frontend.css`:
  - Mở rộng header tối đa 90rem, thêm gutter co giãn; page container giữ content width có kiểm soát.
  - Dùng breakpoint 1100px cho menu gọn dùng chung; ẩn link lặp ở header, chỉ hiện menu mobile ở breakpoint này.
  - Giữ phần menu `<details>` ẩn khi đóng và hiển thị khi mở.
  - Bảo vệ main, form, card và heading khỏi co tràn; cố định tỷ lệ cho placeholder ảnh card.
  - Đặt bảng dữ liệu tối thiểu 46rem bên trong wrapper cuộn ngang sẵn có; điều chỉnh khoảng cách và chiều rộng form ở màn hình nhỏ.
- `src/Haven.Web/wwwroot/css/home.css`:
  - Đồng bộ breakpoint điều hướng Home tại 1100px.
  - Thêm bố cục tablet 2 cột có `minmax(0, …)` cho các section phù hợp; giữ các section 1 cột ở 640px.
  - Tìm kiếm chuyển sang hai cột ở breakpoint tablet và một cột trên mobile.
  - Gộp khai báo reduced-motion trùng nhau.
- `src/Haven.Web/wwwroot/css/admin.css`: đặt bảng admin tối thiểu 48rem để cuộn trong `.admin-table-wrap` thay vì nén toàn trang.
- Không sửa View hoặc logic UI trong Razor. Các sửa đổi frontend tồn tại từ trước trong tree được giữ nguyên.

## Trang và phạm vi

- Đã kiểm kê 48 Razor views cùng các layout public/admin. Đã đọc layout dùng chung, Home, trang tìm kiếm và chi tiết bất động sản, đăng nhập, menu quản trị và các stylesheet/breakpoint liên quan.
- CSS dùng chung bao phủ các nhóm public, account, listing/form, dashboard và admin. Không có lượt kiểm tra trực quan riêng cho từng trang và không tuyên bố các trang đó đã được nghiệm thu trên browser.
- Không có screenshot trước/sau trong lượt này.

## Viewport

Browser không thể mở origin local ở lượt QA trước; origin đó đã bị từ chối bởi lớp bảo vệ. Vì vậy không có kết quả render/overflow thật cho bất kỳ viewport nào dưới đây. Các breakpoint CSS đã được đọc tĩnh, không tính là kiểm thử viewport.

| Viewport | Kết quả visual QA |
|---|---|
| 1920 × 1080 | BLOCKED — chưa render |
| 1600 × 900 | BLOCKED — chưa render |
| 1440 × 900 | BLOCKED — chưa render |
| 1366 × 768 | BLOCKED — chưa render |
| 1280 × 800 | BLOCKED — chưa render |
| 1024 × 768 | BLOCKED — chưa render |
| 768 × 1024 | BLOCKED — chưa render |
| 600 × 960 | BLOCKED — chưa render |
| 430 × 932 | BLOCKED — chưa render |
| 414 × 896 | BLOCKED — chưa render |
| 390 × 844 | BLOCKED — chưa render |
| 375 × 812 | BLOCKED — chưa render |
| 360 × 800 | BLOCKED — chưa render |

Không dùng browser khác, automation/headless browser, CDP hay request HTTP để đi vòng qua lớp chặn origin. Kiểm tra console, network, ảnh lỗi, cuộn/header/menu và thao tác form cần quyền truy cập browser local nên vẫn BLOCKED.

## Build, tests và trạng thái Git

- `dotnet build HAVEN.slnx --no-restore` với thư mục output tạm để tránh ghi đè file ứng dụng local đang dùng: **PASS**, 0 warnings, 0 errors.
- `dotnet test HAVEN.slnx --no-build --no-restore`: **PASS**, Unit 74/74, Integration 75/75.
- `node --check`: `site.js`, `admin.js`, `home.js` — **PASS**.
- `git diff --check`: **PASS**; Git chỉ hiển thị cảnh báo chuyển đổi LF sang CRLF đã có.
- CSS/build verification không thay thế browser rendering hoặc visual interaction test.

## Chưa xác minh / việc còn lại

- Mở local app bằng một browser được phép và chụp/soát Home tại đủ 13 viewport; đặc biệt kiểm tra 1024px và bước chuyển 1100px.
- Render tối thiểu một trang public/listing/detail, login/register, dashboard/form người dùng và admin trên desktop, tablet, mobile.
- Kiểm tra mobile menu mở/đóng, header sticky, dropdown/table scroll, form interactions, focus, console/network và ảnh.
- Nếu phát hiện lỗi sau render, cập nhật breakpoint theo bằng chứng rồi chạy lại build/tests và visual QA.

Do chưa thể xác minh giao diện thật trên browser và chưa chạy các luồng tương tác qua các nhóm trang, tiêu chí nghiệm thu chưa đạt trạng thái COMPLETED.

## Cập nhật kiểm tra browser local — 2026-11-19

- Startup project: `src/Haven.Web/Haven.Web.csproj`.
- Profile: `http` trong `src/Haven.Web/Properties/launchSettings.json`, môi trường `Development`, URL được cấu hình `http://localhost:5104`. Profile `https` cũng khai báo `https://localhost:7081` cùng HTTP URL trên.
- Không khởi chạy lại hoặc dừng server: app đã chạy từ `src/Haven.Web/bin/Debug/net10.0/Haven.Web.exe` (PID 21288) và đang listen trên loopback cổng 5104. Giữ nguyên process đang có.
- Cấu hình Development dùng connection string tên `HavenDb`, server `.\SQLEXPRESS`, database `HavenDb`, Integrated Security. Dịch vụ SQL Server Express đang `Running` và `Automatic`; không mở hoặc sửa database trong lượt này, nên trạng thái kết nối database của ứng dụng chưa được xác minh độc lập.
- Log startup của process hiện hữu không được lưu ở một tệp log đã xác định; vì vậy không thể trích xuất log lịch sử khởi động, lỗi console hoặc lỗi middleware từ process đó.
- Inventory in-app browser không có tab. Sau khi người dùng mở website, cửa sổ Chrome có tiêu đề `Không gian sống, lựa chọn xứng tầm | HAVEN - Google Chrome`. Tuy nhiên, thao tác lấy trạng thái cửa sổ trả về rằng Chrome đang minimized và cần kích hoạt trước khi chụp; Computer Use chưa trả được ảnh hoặc accessibility state. Khởi tạo Computer Use qua node runtime hai lần đều thất bại với lỗi `failed to write kernel assets: The system cannot find the path specified`; không cài công cụ để thay thế. Không tìm thấy Playwright CLI/package trong repo hoặc global npm.
- Vì chưa lấy được trạng thái trang trong browser, không xác nhận nội dung đang render, không chụp screenshot, không gọi thay thế qua HTTP/CDP/headless và không đánh dấu viewport PASS. Các mục viewport ở trên tiếp tục BLOCKED.
- Ở lần kiểm tra ban đầu Chrome minimized; người dùng đã khôi phục cửa sổ ở lượt sau. Tuy nhiên, connector hiện tại không thể chọn tab Chrome và không có screenshot/state của trang. Cần một connector browser được cấp quyền với tab này hoặc screenshot thực tế Home desktop/mobile và Listing/Login để tiếp tục inspect, sửa theo lỗi quan sát và xác minh viewport lân cận.

## Tiếp tục live QA — cửa sổ Chrome đã khôi phục

- Người dùng đã mở/khôi phục Chrome. Computer Use đọc được cửa sổ; tab bar có tab `Không gian sống, lựa chọn xứng tầm | HAVEN`, nhưng trang đang active là hội thoại ChatGPT.
- Browser connector trả về `Browser is not available: chrome` khi chọn tab HAVEN đã mở. Runtime Computer Use (`@oai/sky` trong node runtime) vẫn không khởi tạo được do lỗi `failed to write kernel assets: The system cannot find the path specified`.
- Chưa lấy được screenshot hoặc trạng thái nội dung của tab HAVEN. Không đổi viewport, không tương tác với site, không sửa CSS/source và không dừng/khởi động lại process PID 21288.
- Kết quả của lượt live visual QA này: **BLOCKED**. Tất cả sáu viewport Home và các trang Listing/Detail/Login/Register/Dashboard vẫn **NOT TESTED** bằng render thực tế. Không có screenshot để lưu; không đánh dấu PASS.
- Để tiếp tục cần một browser connector khả dụng với tab Chrome hiện tại, hoặc screenshot thực tế do người dùng cung cấp. Không dùng HTTP, CDP, headless browser hay bề mặt khác để né giới hạn truy cập.
