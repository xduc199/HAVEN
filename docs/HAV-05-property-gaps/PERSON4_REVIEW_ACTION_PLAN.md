# Kế hoạch xử lý review UI Person 4

**Ngày rà soát:** 28/10/2026  
**Commit được rà:** `825b84c55d3fdb62fb3c5b45c4797d7a0a157dc2` (`bobaycho`)  
**Nhánh làm việc hiện tại:** `feature/HAV-05-property-api`, HEAD `89993a71d85adc541bc63fa3b4494b0be503cd5d`.

Tài liệu này là kế hoạch, không thay đổi source hoặc chốt contract thay Person 1/3/5. Các đánh dấu:

- **Đã xác minh:** có bằng chứng trực tiếp từ commit/source/ref Git đã đọc.
- **Đề xuất:** phương án để owner cân nhắc.
- **Chờ xác nhận:** phụ thuộc chưa được owner xác nhận.

## 1. Tóm tắt trạng thái

- **Đã xác minh:** commit `825b84c` có parent `1cbee2073cafa48fee90435687f35294c2927b12`, author/committer `thientran11122-beep`, message `bobaycho`. Thay đổi là MVC UI, CSS, JavaScript và ảnh.
- **Đã xác minh trong refs cục bộ:** `origin/feature/frontend-mvc` trỏ commit đó; cached `origin/main` trỏ merge `483ad4b` (PR #24), có `825b84c` là parent thứ hai. HEAD/local `develop`/cached `origin/develop` đang ở `89993a7` và chưa chứa commit. Không fetch nên không khẳng định trạng thái GitHub hiện tại.
- **Đã xác minh trong snapshot commit:** HomeController có `MuaBan`, `ChoThue`, `DuAn`; ba Razor view ở `Views/Home` đúng convention. Các action này không nhận query parameters. Không có Search, Account, TinDang hoặc Contact controller/action trong snapshot.
- **Đã xác minh trong source ở HEAD:** `PublicListingsController` chỉ khai báo API `GET /api/listings/{id}`. Không tìm thấy `GET /api/listings`, MVC Property route, Search service/controller, Account routes hoặc test fixture xác thực trong source liên quan. Đây là trạng thái HEAD hiện tại, không phải giả định về remote mới hơn.
- Không có file kế hoạch cùng tên trước khi tạo. Ba tài liệu HAV-05 untracked hiện có được giữ nguyên.
- Không chạy build/test; không sửa implementation.

## 2. Findings được xác minh lại

| Finding | Kết quả xác minh, vị trí trong snapshot `825b84c` | Tác động/ưu tiên | Person 4 có thể tự xử lý? | Phụ thuộc |
|---|---|---|---|---|
| REVIEW-001 — route UI chưa tồn tại/không được xác minh | **Còn đúng một phần, cụ thể hóa bên dưới.** `_Layout.cshtml:47,51` trỏ `/Account/Login`, `/Account/Register`; `:257,265–268` trỏ `/TinDang/Create`, `/Home/DieuKhoan`, `/Home/CauHoiThuongGap`, `/Home/LienHe`, `/Home/HoTro`. `Index.cshtml:24,40–41,234,249,349–409` trỏ `/Property...`; `MuaBan.cshtml:334` trỏ `/Home/Contact`. Trong commit, HomeController chỉ có Index, Privacy, Error, MuaBan, ChoThue, DuAn. `/Home/MuaBan`, `/Home/ChoThue`, `/Home/DuAn`, `/Home/Privacy` hợp lệ trong snapshot commit. `GET /api/listings/{id}` không phục vụ các link MVC `/Property`. | Link tới route thiếu sẽ 404 theo conventional routing hiện có. **Medium** cho CTA chính; link phụ có thể tạm ẩn/đánh dấu chưa khả dụng. | Phần link sang action Home đã có thì có thể sửa độc lập; không tự tạo Account/Search/TinDang/Contact routes. | Person 1: Account/auth. Person 3: Property/Search. Person 4 xác nhận Contact/help pages có nằm trong scope UI không. |
| REVIEW-002 — form lọc không làm đổi kết quả | **Còn đúng.** `MuaBan.cshtml:19–57,77–126`, `ChoThue.cshtml:29–69,99–155`, `DuAn.cshtml:45–83` gửi query tới các action Home parameterless. Các action trả view tĩnh, không gọi Search/service; card được viết trực tiếp trong Razor. Một số ô sort không có `name`, nên không được gửi như query parameter. | Người dùng nhập filter nhưng danh sách giữ nguyên. **Medium**; hiện là mock UI, không nên hiểu là Search đã tích hợp. | Có thể sửa wording/disable hoặc ghi rõ prototype, nhưng không thể hoàn thiện filtering thực sự mà không có contract/dữ liệu Search. | Person 3 chốt route/query/DTO/pagination; Person 4 ánh xạ UI; Person 5 xác nhận test end-to-end. |
| REVIEW-003 — `site.js` nạp hai lần | **Còn đúng.** `_Layout.cshtml:14` và `:74` include cùng file. `site.js:6–32` gắn partner click handlers không có guard; `:39–43` có guard riêng cho chat. Index có `havenPartnerTrack` và các nút `data-partner-scroll` (`Index.cshtml:497–580`). | Hai lượt DOMContentLoaded đăng ký listener riêng; mỗi nút partner được gắn hai lần và có thể cuộn track hai lần. Chat hiện có guard, carousel không có. **Medium**. | **Có**, hoàn toàn độc lập. | Không cần dependency bên ngoài. Person 5 có thể kiểm tra hành vi sau sửa. |
| REVIEW-004 — hai footer | **Còn đúng.** `_Layout.cshtml:67–71` render footer mặc định; `:222–326` render footer HAVEN khác. Không phải partial do view khác; cả hai nằm trực tiếp trong layout dùng chung. | Hai footer cùng xuất hiện trên mọi view dùng layout, nội dung bản quyền/privacy cũ lặp. **Medium/Low** tùy bố cục thực tế. | **Có**, nhưng Person 4 cần chọn footer chủ đích và kiểm tra style cũ trước khi bỏ. | Person 5/browser review để xác nhận layout ở desktop/mobile. |
| REVIEW-005 — `lang="en"` | **Còn đúng.** `_Layout.cshtml:2` khai báo English; nội dung trang/layout là tiếng Việt. | Trình đọc màn hình và xử lý ngôn ngữ có thể phát âm nội dung theo English. **Low**, dễ sửa độc lập. | **Có.** | Xác nhận ngôn ngữ mặc định của sản phẩm nếu có đa ngôn ngữ. |
| Ảnh chưa có tham chiếu | **Đã xác minh tham chiếu trong toàn snapshot source:** `haven-hero.jpg` được CSS dùng (`site.css:245`); `logo3.png` là favicon (`_Layout.cshtml:10`). Không tìm thấy tham chiếu cho `caotangfooter.jpg`, `logo.png`, `logo2.png` trong Razor/CSS/JS của commit. | Ba file là asset dư trong snapshot hiện tại, nhưng có thể là dự kiến cho phần UI sau. Xóa làm mất tài nguyên chưa xác minh ý định. **Low/Informational**. | Person 4 có thể xác nhận mục đích; không nên xóa chỉ dựa trên grep. | Hỏi Person 4 về ý định. Không cần Person 2. |
| Trailing whitespace | `git diff --check parent commit` trước đây báo whitespace ở `_Layout.cshtml:58` (dòng trống có spaces). | Không ảnh hưởng runtime nhưng làm diff check báo lỗi. **Low**. | **Có**, sửa khi Person 4 cập nhật file. | Không. |

### Quan sát thêm có căn cứ

- Ba view `MuaBan`, `ChoThue`, `DuAn` tự chứa `<main>` trong khi layout đã bọc `@RenderBody()` bằng `<main>` (`_Layout.cshtml:62–64`; views `:6`). HTML có main landmark lồng nhau. **Low**, Person 4 có thể chọn một nơi sở hữu landmark và kiểm tra accessibility.
- `MuaBan.cshtml` dùng `#biet-thu-1`, `#can-ho-1`, v.v.; `ChoThue.cshtml` dùng `href="#"`; project cards trỏ `#tu-van-du-an`. Đây không phải route chi tiết listing; không thấy detail MVC route trong source. Cần phối hợp Search/Property trước khi gắn link thật. Có thể giữ card không tương tác hoặc ghi rõ nội dung demo.
- Layout có form newsletter `action="#"` (`_Layout.cshtml:303`) và chat JavaScript chỉ hiện thông báo rằng chưa gửi máy chủ (`site.js:79–91`). Hành vi này minh bạch ở chat; newsletter chưa có xử lý. Cần tránh trình bày như subscription đã được lưu.

## 3. Route map

| Link/route từ UI | Route/action được xác minh | Module chịu trách nhiệm | Đã xác minh? | Hướng xử lý |
|---|---|---|---|---|
| `/`, `Home/Index` | Conventional route tới `HomeController.Index` | Person 4/Web | Có trong commit và HEAD | Giữ; smoke test hiện tại kiểm tra `/` trả 200 nhưng chưa kiểm tra nội dung mới. |
| `/Home/MuaBan` / `asp-action="MuaBan"` | `HomeController.MuaBan` trong commit `825b84c`; chưa có trong HEAD `develop` | Person 4 | Có trong commit, chưa có trong HEAD | Route hợp lệ nếu commit UI được đưa vào snapshot chạy. |
| `/Home/ChoThue` | `HomeController.ChoThue` trong commit | Person 4 | Có trong commit, chưa có trong HEAD | Như trên. |
| `/Home/DuAn` | `HomeController.DuAn` trong commit | Person 4 | Có trong commit, chưa có trong HEAD | Như trên. |
| `/Home/Privacy` | `HomeController.Privacy` | Web/Person 4 | Có trong commit và HEAD | Hợp lệ. |
| `/Property` và `/Property?location=...` | Không thấy MVC route/controller; API hiện có là `/api/listings/{id}` | Person 3 (Search), phối hợp Person 4 | Chưa có route MVC; chưa có public list API tại HEAD | Không tự đổi sang route/API khác. Chờ Person 3 xác nhận contract; tạm ẩn/đánh dấu CTA nếu chưa sẵn sàng. |
| `/Account/Login`, `/Account/Register` | Không thấy Account controller/route trong snapshot hoặc source tại HEAD | Person 1 (Identity), Person 4 (UI) | Chưa xác minh có route | Person 1 xác nhận route, auth flow và return URL; Person 4 dùng contract sau đó. |
| `/TinDang/Create` | Không thấy TinDang controller/route; API cá nhân HAV-05 còn chờ Identity | Person 2 phụ trách Property API; Person 1 auth; Person 4 link UI | Chưa có route trong source đã xem | Không tạo MVC/API thay thế trong lượt này; Person 2 chỉ cung cấp API sau khi Identity contract sẵn sàng. |
| `/Home/Contact` | Không có action `Contact` trong HomeController | Person 4 hoặc owner Consultation/Contact chưa xác định | Chưa xác minh | Xác nhận phạm vi/owner; dùng mailto hiện có nếu đó là lựa chọn được duyệt, không tự thêm action. |
| `/Home/DieuKhoan`, `/Home/CauHoiThuongGap`, `/Home/LienHe`, `/Home/HoTro` | Không có actions tương ứng trong HomeController đã rà | Person 4 hoặc owner nội dung/Support chưa xác định | Chưa xác minh | Hỏi owner nội dung; không để link giả như route đã tồn tại. |
| `/api/listings/{id}` | `PublicListingsController.GetById`, chỉ trả chi tiết public theo ID | Person 2 / Property | Có ở HEAD | Không thay route Search; API này không làm đích thay thế cho `/Property`. |
| `#biet-thu-1`, `#can-ho-1`, `#nha-pho-1`, `#nha-vuon-1`, `#tu-van-du-an`, `#` | Fragment trong cùng document, không phải MVC endpoint | Person 4; Person 3 nếu cần listing detail | Fragment được khai báo; đích từng ID cần xác nhận trong markup | Kiểm tra ID target; nếu không có target hoặc chi tiết thật, bỏ affordance “Chi tiết”/“Khám phá” hoặc chờ route được thống nhất. |
| `mailto:contact@haven.vn`, social URLs | External URI | Chủ nội dung/UI chưa xác định | Link hiện có; tính chính xác email/tài khoản chưa xác minh | Owner nội dung xác nhận địa chỉ/tài khoản trước nghiệm thu sản phẩm. |

## 4. Search dependency và dữ liệu hiển thị

**Đã xác minh:**

- Mua bán gửi `keyword`, `location`, `type`; sidebar gửi `location`, `minPrice`, `maxPrice`, `bedrooms`. Sort select không có `name`.
- Cho thuê gửi `keyword`, `location`, `propertyType`; sidebar gửi `location`, `propertyType`, `maxPrice`, `bedrooms`. Sort select không có `name`.
- Dự án gửi `keyword`, `location`, `type`; sort select không có `name`.
- Home actions trong commit đều không nhận các tham số này hoặc gọi service/repository. Card title/price/area/location được viết trực tiếp trong Razor.
- Tại HEAD hiện tại, không tìm thấy Search service/controller/query; public API Property chỉ có detail theo ID. Không suy ra trạng thái remote mới hơn.

**Chờ Person 3 xác nhận:** route Search có phải `/api/listings` hay MVC route; query names/types/casing; mục tiêu và phạm vi filter; sort key/default; pagination; response DTO; status visibility (public chỉ Approved); cách xử lý query sai/không có kết quả. Person 4 sau đó ánh xạ form/view model theo contract thực tế. Person 5 chọn cách kiểm thử API/UI integration.

**Đề xuất an toàn trước khi contract có:** Person 4 có thể hoàn thiện bố cục/validation HTML và giữ query values sau round-trip, nhưng không tuyên bố kết quả đã được lọc. Nếu demo tĩnh là mục đích, ghi rõ dữ liệu mẫu hoặc vô hiệu hóa nút/filter tương tác. Không gọi route chưa tồn tại và không tạo query contract mới.

## 5. Script, layout, CSS và ngôn ngữ

- `site.js` được include **hai lần** trong layout (head và cuối body). Do cả hai script thực thi trước `DOMContentLoaded`, hai callback khởi tạo carousel đều chạy. Mỗi callback gắn click listener cho cùng buttons. Chat có guard `data-initialized`, carousel không có. **Sửa tối thiểu đề xuất:** giữ một include duy nhất; nếu vì lý do khác cần include lại, tách/init guard riêng. Sau đó xác minh một click tạo đúng một lần scroll.
- Layout có footer mặc định ở dòng 67 và footer HAVEN ở dòng 222. Không có partial mới cho footer trong commit. **Sửa tối thiểu đề xuất:** chọn một footer làm chuẩn, xóa/di chuyển footer còn lại trong thay đổi UI của Person 4. CSS `.footer` cũ được khai báo trong `Views/Shared/_Layout.cshtml.css`/stylesheet gốc của Web; cần kiểm tra computed layout sau khi chọn, không suy đoán trước footer nào phải giữ.
- Layout dùng `<main>` quanh `RenderBody`; ba view mới cũng có `<main>`. **Đề xuất:** giữ một main landmark duy nhất.
- Layout khai báo `lang="en"`; nội dung chủ yếu tiếng Việt. **Đề xuất:** đổi theo ngôn ngữ mặc định đã xác nhận, hoặc dùng localization contract nếu ứng dụng đa ngôn ngữ.
- `git diff --check parent commit` có trailing whitespace ở `_Layout.cshtml:58`; bỏ whitespace khi Person 4 chỉnh layout.

## 6. Tài nguyên tĩnh

| Asset mới trong commit | Tham chiếu tại snapshot | Kết luận có căn cứ | Hành động |
|---|---|---|---|
| `caotangfooter.jpg` | Không tìm thấy trong Razor/CSS/JS | Chưa dùng trong snapshot; có thể dành cho kế hoạch UI sau | Person 4 xác nhận; không tự xóa |
| `haven-hero.jpg` | `site.css:245` | Được dùng làm hero background | Giữ |
| `logo.png` | Không tìm thấy trong Razor/CSS/JS | Chưa dùng trong snapshot; ý định chưa biết | Person 4 xác nhận; không tự xóa |
| `logo2.png` | Không tìm thấy trong Razor/CSS/JS | Chưa dùng trong snapshot; ý định chưa biết | Person 4 xác nhận; không tự xóa |
| `logo3.png` | `_Layout.cshtml:10` | Được dùng làm favicon | Giữ |

“Không tìm thấy tham chiếu trong commit” không chứng minh tài nguyên không còn được dự kiến dùng. Không xóa, đổi tên hoặc di chuyển asset khi chưa có xác nhận.

## 7. Kế hoạch sửa và phân công

### Ưu tiên 1 — Person 4, có thể làm độc lập

1. Chỉ include `site.js` một lần; kiểm tra carousel một click/một scroll.
2. Chọn một footer, loại render trùng; kiểm tra CSS footer cũ và mới ở desktop/mobile.
3. Sửa `lang` phù hợp; bỏ trailing whitespace.
4. Chỉ để fragment links có target element; các card không có detail route cần được thể hiện là mẫu hoặc không tương tác.
5. Xác nhận mục đích của ảnh `caotangfooter.jpg`, `logo.png`, `logo2.png`; chưa xóa tài nguyên.
6. Giữ nhãn/trạng thái rõ cho form lọc demo cho đến khi Search contract tồn tại.

### Ưu tiên 2 — Person 1 và Person 3, rồi Person 4 tích hợp

- **Person 1:** xác nhận route login/register, scheme/flow Identity, cách xử lý return URL và trạng thái khi route chưa phát hành. Person 4 không dựng luồng đăng nhập giả.
- **Person 3:** chốt public Search route/controller ownership, query parameter names/types, filter/sort/pagination/response, public status và empty/error semantics. Person 4 không tự tạo API contract.
- **Person 4:** sau khi có các xác nhận, nối Razor UI với route/contract thật; các link đến module chưa sẵn sàng phải ẩn hoặc ghi rõ chưa khả dụng theo quyết định nhóm.

### Ưu tiên 3 — Person 5 kiểm thử và CI

- Xác nhận test sở hữu cho các MVC routes mới và test fixture `WebApplicationFactory` hiện tại.
- Bổ sung/điều phối kiểm thử route `/Home/MuaBan`, `/Home/ChoThue`, `/Home/DuAn` trên đúng snapshot đã tích hợp; kiểm tra links không trả 404 và query chỉ được coi là filter khi Search response chứng minh kết quả.
- Kiểm tra một click carousel, footer count, language metadata; kiểm tra render thực tế ở desktop/mobile theo convention UI nhóm.
- Chạy build/test/CI sau khi implementation được cập nhật; báo rõ lệnh, snapshot và artifacts.

### Vai trò Person 2 — Backend Property

- Không sửa MVC UI hoặc route Identity/Search.
- Hiện không có yêu cầu code HAV-05 trực tiếp từ các findings này. Person 2 chỉ cung cấp/điều chỉnh API Property khi Person 1 chốt principal/auth và Person 3 chốt hợp đồng Search/route có phụ thuộc Property; mọi thay đổi thuộc backend Property phải có task/contract xác nhận riêng.

## 8. Tiêu chí nghiệm thu đề xuất

- [ ] Mỗi layout chỉ nạp `site.js` một lần, trừ khi có lý do và bảo vệ init được ghi rõ.
- [ ] Một lần bấm carousel chỉ tạo một bước cuộn; kiểm tra trên trang có track và nút.
- [ ] Mỗi trang chỉ render footer được chọn; không lặp bản quyền/footer cũ; CSS không làm footer mới chồng lấn.
- [ ] Chỉ có một `<main>` landmark cho nội dung trang.
- [ ] `lang` phản ánh ngôn ngữ thực tế hoặc localization đã thống nhất.
- [ ] Link nội bộ trỏ route/action tồn tại trong snapshot tích hợp; route Identity/Search chưa có thì không trình bày như đã hoạt động.
- [ ] Fragment CTA có ID target; listing card không tuyên bố mở chi tiết nếu chưa có route/detail data.
- [ ] Search form dùng đúng tên/kiểu query và response do Person 3 duyệt. Nếu chưa nối backend, trạng thái demo được chỉ rõ và không tuyên bố đã lọc.
- [ ] Không xóa asset chưa được owner xác nhận; mỗi asset được giữ có mục đích hoặc owner xác nhận.
- [ ] `git diff --check` sạch trên diff sửa chữa.
- [ ] Build, unit/integration tests và kiểm tra UI được chạy trên đúng branch/PR snapshot; lưu kết quả thực tế, không suy ra từ commit cũ.

## 9. Thông tin chưa xác minh và giới hạn

- Remote chưa fetch; `origin/main`, `origin/develop` là refs cache tại workspace, không khẳng định GitHub mới nhất.
- Commit `825b84c` không phải HEAD hiện tại. Source ở HEAD được dùng để xác minh route/API hiện có tại `develop`; snapshot commit được đọc qua Git để xác minh UI/action của Person 4.
- Chưa có xác nhận từ Person 1/3/5 về auth contract, Search contract hoặc test ownership.
- Chưa xác định ảnh chưa dùng là asset thừa hay dành cho feature sau.
- Không chạy build/test, browser hoặc responsive visual review theo yêu cầu lượt này.

## 10. Git và file tạo

Trạng thái trước khi tạo tài liệu: branch `feature/HAV-05-property-api`, HEAD `89993a71d85adc541bc63fa3b4494b0be503cd5d`; ba tài liệu HAV-05 đang untracked; không có staged/unstaged tracked changes. `PERSON4_REVIEW_ACTION_PLAN.md` chưa tồn tại trước khi ghi.

Tài liệu mới này là file duy nhất được tạo trong lượt rà soát. Ba file untracked cũ được giữ nguyên. Không sửa source/test/cấu hình/tài liệu cũ; không checkout, fetch, commit, push, merge, rebase hoặc tạo PR.
