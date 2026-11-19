# HAVEN Home Editorial Refinement — V2

Ngày thực hiện: 2026-11-19  
Trạng thái: **PARTIALLY COMPLETED**

## Repository và bảo toàn thay đổi

- Branch: `develop`
- HEAD trước và sau: `5fdad8cc5c3c53942ba18afd83b22260d669cf54`
- Working tree đã có nhiều thay đổi từ các đợt Avatar/Profile/Admin và Home trước đó. Các thay đổi này được giữ nguyên.
- Ba file Home vốn đã modified trước khi bắt đầu; bản trước khi chỉnh được sao lưu ở `%TEMP%\haven-home-refinement-2026-11-19`.
- Không reset, clean, checkout, commit hoặc push.

## Hiện trạng và thay đổi

Source trước khi chỉnh có 3 slide. Nút Previous/Next nằm dưới banner cùng indicator. Ảnh đang chạy hiệu ứng zoom, trong khi kicker, heading, mô tả và CTA cùng nhận các animation xuất hiện khi slide active; các hiệu ứng này chồng thời điểm với `carousel-fade` của Bootstrap. Đây là xung đột animation nhìn thấy trong CSS/JS source; do browser không khởi tạo được trong phiên này nên chưa thể xác nhận lại hiện tượng bằng render thực tế.

Đã cập nhật:

- `src/Haven.Web/Views/Home/Index.cshtml`
  - Tăng carousel lên 5 slide, mỗi slide dùng một ảnh riêng.
  - Đưa Previous/Next vào hai mép banner; indicator và nút pause giữ ở cạnh dưới.
  - Thay phần giới thiệu bằng bố cục ảnh và nội dung editorial, gồm ba điểm trải nghiệm và link tìm kiếm hiện có.
  - Biến phần nhu cầu sống thành ba panel ảnh riêng; nội dung được mô tả là cảm hứng không gian, không phải tin đăng.
  - Mở rộng hai hành trình thành bốn bước và gắn CTA tới route tìm kiếm/đăng tin đang có.
  - Search form, tên query parameters, property type IDs, route, dữ liệu tin đăng, FAQ, lưu ý minh bạch và nghiệp vụ không bị thay đổi.
- `src/Haven.Web/wwwroot/css/home.css`
  - Đặt arrows hai bên hero, kích thước 46px desktop và khoảng 38px mobile.
  - Làm mềm fade và giảm chuyển động chữ còn dịch nhẹ bằng `opacity`/`transform`; bỏ animation zoom có opacity trên ảnh để tránh xung đột với fade của Bootstrap.
  - Tạo bố cục editorial/lifestyle và timeline hai hành trình; đưa các panel về một cột trên mobile.
  - Tắt transition/animation của Home khi bật `prefers-reduced-motion`.
- `src/Haven.Web/wwwroot/js/home.js`
  - Đồng bộ `aria-hidden`, `inert` và `aria-current` của indicators theo slide đang hoạt động.
  - Reset chu kỳ autoplay sau mỗi lần chuyển; tạm dừng khi hover/focus hoặc người dùng chủ động pause; không autoplay khi reduced motion được bật.
  - Giữ hành vi keyboard/touch do Bootstrap Carousel hiện có cung cấp.
- Asset ảnh mới, được tạo riêng trong phiên này, không dùng URL bên ngoài:
  - `home-architecture-story.jpg` — villa đương đại, đá/gỗ và ánh sáng tự nhiên.
  - `home-tropical-living.jpg` — nhà nhiệt đới kết nối với vườn.
  - `home-landscape.jpg` — khu đất dân cư, dùng cho loại hình đất.

Ảnh mới được lưu JPEG cục bộ, lần lượt khoảng 596 KB, 390 KB và 616 KB. Ba slide đầu tiếp tục dùng ảnh có sẵn. Tất cả 5 slide có đường dẫn ảnh khác nhau. Danh mục vẫn lấy từ `Model.PropertyTypes` và link lọc giữ `propertyTypeId`; các tên phổ biến Apartment/Căn hộ, House/Nhà, Land/Đất, Villa/Biệt thự được ánh xạ tới hình phù hợp. Danh mục tên tùy chỉnh ngoài các nhóm này dùng ảnh kiến trúc có sẵn làm fallback.

## Kiểm tra tự động

- `dotnet build HAVEN.slnx --no-restore`: lần đầu bị khóa khi ghi `Haven.Web.exe` bởi website đang chạy (PID 35736). Không dừng ứng dụng. Build lại với `BaseOutputPath` riêng dưới `%TEMP%`: **thành công, 0 warning, 0 error**.
- `dotnet test HAVEN.slnx --no-build`: **Unit 82/82 pass; Integration 81/81 pass**.
- `node --check src/Haven.Web/wwwroot/js/home.js`: **pass**.
- `git diff --check`: **pass**; Git chỉ in cảnh báo chuẩn hóa line ending cho một số file đã dirty từ trước.

## Browser và responsive QA

Đã thử khởi tạo browser automation để xem website đang chạy. Công cụ trả lỗi hệ thống `failed to write kernel assets: The system cannot find the path specified`; không có screenshot mới trong phiên này. Không dùng HTTP/headless/CDP để thay thế.

Vì không quan sát được render, các viewport 1920, 1600, 1440, 1366, 1280, 1024, 768, 430, 390, 375 và 360px đều **chưa được xác nhận trực quan**. Chưa kiểm tra trực tiếp 10 lần chuyển slide, menu, console, network, image loading hoặc layout trên trình duyệt. Website vẫn chạy bằng tiến trình cũ và chưa được khởi động lại/reload sau build; không thể coi code mới đã được nghiệm thu live.

## Kết luận

Source Home và assets đã được refinement; build, test và JavaScript syntax đều đạt. Trạng thái vẫn là **PARTIALLY COMPLETED** vì browser automation không khởi tạo được và giao diện mới chưa được render trực tiếp ở desktop/mobile. Sau khi browser khả dụng, cần reload site và kiểm tra hero controls, autoplay/pause, ít nhất 10 lần chuyển slide, category names đang bật trong database, các CTA và các viewport desktop/mobile nêu trên trước khi nghiệm thu hoàn tất.
