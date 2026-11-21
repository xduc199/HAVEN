# Báo cáo rà soát nội dung và thiết kế sáu trang HAVEN

**Trạng thái:** Hoàn thành phần biên tập và triển khai trong source; chưa phải xác nhận pháp lý cuối cùng.  
**Ngày rà soát:** 21/11/2026  
**Branch / HEAD trước khi làm:** `develop` / `7ccdf9d931c42eff5010cffd5a5e4ebc77cd4141`

## Phạm vi và bảo toàn repository

Working tree đã có nhiều file chỉnh sửa và một file mới từ các công việc trước khi bắt đầu đợt này. Các thay đổi đó được giữ nguyên; không reset, clean, commit hoặc push. Phần công việc này tập trung vào sáu trang thông tin, phiên bản tài liệu chính sách và integration tests phụ thuộc phiên bản chính sách.

## Kết quả theo trang

- **`/About` — Giới thiệu HAVEN:** viết lại theo chức năng đang có: tìm kiếm, xem chi tiết, lưu/yêu thích, so sánh, đăng và quản lý tin, gửi yêu cầu tư vấn. Nêu rõ tin do người đăng cung cấp và HAVEN không thay các bên xác minh hồ sơ, nhận đặt cọc/thanh toán hay ký hợp đồng.
- **`/Terms` — Điều khoản sử dụng:** làm rõ tài khoản, nội dung, quyền và trách nhiệm người dùng, quy tắc tin đăng, xử lý vi phạm, vai trò của nền tảng và giới hạn trách nhiệm theo cách không cam kết xác minh tuyệt đối.
- **`/Privacy` — Quyền riêng tư:** mô tả nhóm dữ liệu được source xử lý, mục đích, thông tin công khai, lưu trữ theo cấu hình và các yêu cầu quyền riêng tư hiện có. Phân biệt yêu cầu được ghi nhận để Admin xem xét với việc hệ thống tự động thực hiện xóa/chỉnh sửa; source hiện không tự động xóa dữ liệu khi người dùng gửi yêu cầu.
- **`/PostingRules` — Quy chế đăng tin:** phản ánh các trạng thái Draft, Pending, Approved, Rejected và Hidden; tin công khai là tin đã duyệt. Nêu trách nhiệm về tính chính xác, quyền đăng và hình ảnh. Không mô tả nút báo cáo tin vi phạm chưa được xác nhận trong giao diện.
- **`/Complaints` — Phản ánh và khiếu nại:** phân biệt luồng yêu cầu tư vấn với luồng phản ánh/quyền riêng tư thực tế; nêu hạn chế hiện tại của biểu mẫu và không tự tạo email, số điện thoại hoặc SLA. Yêu cầu phản ánh tin cần định danh tin đăng; biểu mẫu không nhận tệp đính kèm.
- **`/CookiePolicy` — Chính sách cookie:** ghi nhận cookie xác thực và chống giả mạo theo cấu hình ASP.NET Core, cookie thông báo tạm thời của ứng dụng, việc không thấy giao diện cookie consent trong source và việc layout tải Google Fonts. Không tuyên bố chắc chắn rằng mọi môi trường đều không có cookie bên thứ ba.

Bốn trang tài liệu dài dùng chung bố cục nội dung có breadcrumb, tóm tắt, phiên bản/ngày hiệu lực, mục lục thích ứng mobile, liên kết tài liệu liên quan và thông báo cần rà soát pháp lý. Trang About có CTA đi đến route tìm kiếm và đăng tin hiện có.

## Dữ kiện nghiệp vụ đã đối chiếu với source

- Việc gửi yêu cầu tư vấn yêu cầu tài khoản và gắn với tin đăng; dữ liệu liên hệ được lưu trong hệ thống và có hàng đợi quản trị. Không mô tả rằng email hay SMS được gửi tự động.
- Quyền riêng tư có trang cá nhân để gửi yêu cầu và hàng đợi Admin; yêu cầu có thể được cập nhật trạng thái/kết quả, nhưng không có thao tác tự động xóa dữ liệu tương ứng.
- Tin đăng chỉ được hiển thị công khai khi trạng thái được duyệt. Xóa tin hiện được giới hạn theo quyền/trạng thái trong source.
- Source hiện cấu hình cookie xác thực và antiforgery. Không tìm thấy tích hợp analytics/ads/pixel trong các file ứng dụng đã rà soát. Đây là kết quả tìm trong source, không phải bảo đảm cho cấu hình hạ tầng hoặc phiên bản triển khai khác.
- Thông tin pháp nhân và kênh liên hệ trong cấu hình hiện chưa được chủ dự án khai báo; trang không bịa thông tin thay thế.

## Phiên bản chính sách và test

Các tài liệu hiện hành được đặt phiên bản `1.1.0`, hiệu lực 21/11/2026; phiên bản `1.0.0` ngày 17/11/2026 vẫn có thể truy cập qua lựa chọn phiên bản cũ. Integration test được cập nhật để đăng ký và consent dùng đúng phiên bản hiện hành, đồng thời vẫn kiểm tra route lịch sử.

Lưu ý triển khai: source hiện yêu cầu phiên bản hiện hành trong luồng đăng ký mới; chưa có luồng buộc tài khoản cũ chấp thuận lại khi tài liệu đổi phiên bản. Chủ dự án cần quyết định yêu cầu re-consent trước khi vận hành nếu cần.

## File thay đổi trong phạm vi

- `src/Haven.Web/Controllers/PoliciesController.cs` — biên soạn nội dung, mô tả phiên bản, summary và thông tin operator lấy từ cấu hình.
- `src/Haven.Web/Services/PolicyVersions.cs` — cập nhật các phiên bản hiện hành.
- `src/Haven.Web/ViewModels/Policies/PolicyViewModels.cs` — truyền tóm tắt tài liệu sang View.
- `src/Haven.Web/Views/Policies/About.cshtml` — cấu trúc/nội dung trang giới thiệu.
- `src/Haven.Web/Views/Policies/Document.cshtml` — mẫu dùng chung cho năm trang chính sách.
- `src/Haven.Web/wwwroot/css/frontend.css` — style có scope cho About và trang tài liệu.
- `tests/Haven.IntegrationTests/PoliciesAndPrivacyTests.cs` cùng test đăng ký liên quan — cập nhật phiên bản consent mong đợi sang `1.1.0`.

Các file khác đang hiện dirty trong `git status` thuộc thay đổi đã có trước đó; không được xem là do đợt này nếu không nằm trong danh sách trên.

## Kiểm thử

- `dotnet build HAVEN.slnx --configuration Release --no-restore` — **PASS**, 0 warning, 0 error.
- `dotnet test HAVEN.slnx --configuration Release --no-build --verbosity quiet` — **PASS**, Unit 82/82 và Integration 81/81.
- `git diff --check` — **PASS**, không phát hiện whitespace error.
- Integration tests xác nhận các route chính sách trả HTTP 200, phiên bản cũ truy cập được, phiên bản không tồn tại trả Not Found, consent mới lưu phiên bản hiện hành và trang quyền riêng tư/hàng đợi Admin được bảo vệ đúng theo test.
- **Browser visual QA: chưa xác minh.** Công cụ browser được cấp quyền đã dừng thao tác vì không xác định URL trình duyệt Windows đủ chắc chắn để áp dụng chính sách điều khiển. Không dùng HTTP/CDP/headless để vượt giới hạn này; do đó không khẳng định đã kiểm tra bố cục trực tiếp hoặc console trình duyệt.

## Điểm cần chủ dự án xác nhận

1. Tên pháp nhân/đơn vị vận hành, địa chỉ và kênh tiếp nhận yêu cầu chính thức.
2. Vai trò pháp lý chính xác của HAVEN đối với quy định về thương mại điện tử, dịch vụ bất động sản và bảo vệ người tiêu dùng.
3. Nhà cung cấp hosting, lưu trữ tệp, log/giám sát và các bên xử lý dữ liệu ngoài source.
4. Thời hạn lưu trữ, quy trình thực hiện quyền dữ liệu, xác minh danh tính người yêu cầu và thời gian phản hồi.
5. Có cần cookie consent cho bất kỳ dịch vụ nào ở môi trường triển khai không; hiện source không có màn hình consent.
6. Có cần yêu cầu người dùng hiện hữu chấp thuận phiên bản chính sách mới hay không.

## Rà soát pháp lý

Nội dung được biên tập theo source và đối chiếu các văn bản chính thức có liên quan gồm Luật Bảo vệ dữ liệu cá nhân 91/2025/QH15, Nghị định 356/2025/NĐ-CP, Luật Bảo vệ quyền lợi người tiêu dùng 19/2023/QH15, Luật Kinh doanh bất động sản 29/2023/QH15, Luật Giao dịch điện tử 20/2023/QH15 và Luật Thương mại điện tử 122/2025/QH15. Cần luật sư/chuyên gia pháp chế và chủ thể vận hành rà soát trước khi dùng làm chính sách chính thức, nhất là phân loại pháp lý của nền tảng và căn cứ xử lý dữ liệu.
