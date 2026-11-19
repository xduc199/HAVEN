# HAVEN API Test Report

Ngày kiểm thử: 2026-11-19  
Branch: `develop`  
HEAD: `5fdad8cc5c3c53942ba18afd83b22260d669cf54`  
Môi trường: ứng dụng local `Development`, SQL Server local `.\SQLEXPRESS`, database `HavenDb`; không gọi môi trường production.

## Phạm vi và cách kiểm thử

OpenAPI tại `/openapi/v1.json` liệt kê 49 operation. Từng operation được gọi bằng HTTP với phiên anonymous hoặc tài khoản QA role `User`, dùng antiforgery token cho các request thay đổi dữ liệu. Các endpoint quản trị được gọi bằng user thường để kiểm tra chặn quyền. Hai route media MVC ngoài OpenAPI (`/listing-images/{id}`, `/listing-videos/{id}`) cũng được gọi với ID không tồn tại.

HTTP status dưới đây là kết quả thực tế của request đại diện. Với endpoint có ghi dữ liệu, các ca hợp lệ được chạy trên database local và dữ liệu QA; không phải mọi tổ hợp giá trị biên đều được fuzz. Không có API nào trả 5xx trong lượt kiểm tra cuối. **BLOCKED** nghĩa là chưa thể kiểm tra bằng tài khoản có quyền tương ứng.

## Danh mục 49 operation

| Method | Route | Auth/role | Kết quả thực tế |
|---|---|---|---|
| GET | `/api/admin/analytics` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/reports` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/consultations` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/consultations/{id}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| PUT | `/api/admin/consultations/{id}/status` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/dashboard` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/listings` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/listings/{id}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/moderation/pending` | Admin | 403 với user thường; luồng Admin BLOCKED |
| POST | `/api/admin/moderation/{id}/approve` | Admin | 403 với user thường; luồng Admin BLOCKED |
| POST | `/api/admin/moderation/{id}/reject` | Admin | 403 với user thường; luồng Admin BLOCKED |
| POST | `/api/admin/moderation/{id}/hide` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/property-types` | Admin | 403 với user thường; luồng Admin BLOCKED |
| POST | `/api/admin/property-types` | Admin | 403 với user thường; luồng Admin BLOCKED |
| PUT | `/api/admin/property-types/{id}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/tags` | Admin | 403 với user thường; luồng Admin BLOCKED |
| POST | `/api/admin/tags` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/tags/{id}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| PUT | `/api/admin/tags/{id}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/admin/listings/{listingId}/tags` | Admin | 403 với user thường; luồng Admin BLOCKED |
| POST | `/api/admin/listings/{listingId}/tags/{tagId}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| DELETE | `/api/admin/listings/{listingId}/tags/{tagId}` | Admin | 403 với user thường; luồng Admin BLOCKED |
| GET | `/api/auth/antiforgery` | Public | 200; nhận request token và cookie antiforgery |
| POST | `/api/auth/register` | Public | 201 hợp lệ; 400 payload sai và email trùng |
| POST | `/api/auth/login` | Public | 200 hợp lệ; 401 thông tin đăng nhập sai |
| POST | `/api/auth/logout` | Đăng nhập | 204; sau đó `GET /api/auth/me` trả 401 |
| POST | `/api/auth/change-password` | Đăng nhập | 400 với current password sai; mật khẩu không bị đổi |
| GET | `/api/auth/me` | Đăng nhập | 200 và thông tin/role của phiên hiện tại; 401 sau logout |
| PUT | `/api/auth/me` | Đăng nhập | 200 khi cập nhật tên hiển thị bằng giá trị QA hiện tại |
| POST | `/api/consultations` | Đăng nhập | 400 payload thiếu/không hợp lệ; không tạo yêu cầu tư vấn |
| GET | `/api/consultations` | Đăng nhập | 200 |
| GET | `/api/consultations/{id}` | Đăng nhập | 404 với ID không tồn tại |
| GET | `/api/favorites` | Đăng nhập | 200 |
| POST | `/api/favorites/{listingId}` | Đăng nhập | 201; thêm lại cùng mục trả 409 |
| DELETE | `/api/favorites/{listingId}` | Đăng nhập | 204; mục QA được gỡ sau kiểm thử |
| GET | `/api/locations/provinces` | Public | 200 |
| GET | `/api/locations/provinces/{provinceCode}/wards` | Public | 200; mã tỉnh không có dữ liệu trả danh sách rỗng |
| POST | `/api/my/listings` | Đăng nhập | 201 cho 10 tin QA; 400 cho dữ liệu không hợp lệ |
| GET | `/api/my/listings` | Đăng nhập | 200; tài khoản tạo tin đọc lại được 10 tin của mình |
| GET | `/api/my/listings/{id}` | Đăng nhập/chủ tin | 200 với tin thuộc user; 404 với ID không tồn tại |
| PUT | `/api/my/listings/{id}` | Đăng nhập/chủ tin | 200 khi tin còn Draft; 400 area bằng 0; 409 khi tin đã Pending |
| DELETE | `/api/my/listings/{id}` | Đăng nhập/chủ tin | 404 với ID không tồn tại; không xóa tin QA Pending |
| POST | `/api/my/listings/{id}/submit` | Đăng nhập/chủ tin | 200 Draft → Pending; gửi lại Pending trả 409 |
| GET | `/api/property-types` | Public | 200; lấy được danh mục active hiện có |
| GET | `/api/listings` | Public | 200; QA Pending không nằm trong kết quả |
| GET | `/api/listings/compare` | Public | 200 với hai tin công khai hợp lệ; ID không tồn tại trả 404 |
| GET | `/api/listings/{id}` | Public | 200 với tin Approved; tin QA Pending trả 404 |
| GET | `/api/tags` | Public | 200 |
| GET | `/api/tags/{slug}/listings` | Public | 404 với slug không tồn tại |

Media ngoài OpenAPI:

| Method | Route | Kết quả |
|---|---|---|
| GET | `/listing-images/{id}` | 404 với ID ảnh không tồn tại |
| GET | `/listing-videos/{id}` | 404 với ID video không tồn tại |

## Kết quả tổng hợp

- OpenAPI: 49/49 operation đã được gọi ít nhất một lần theo ngữ cảnh nêu trên.
- Kết quả ma trận operation: 14×200, 1×201, 2×204, 4×400, 1×401, 22×403, 3×404, 2×409. Với logout 204 là request xác thực cuối riêng; các 4xx là phản hồi dự kiến cho validation, quyền, không tìm thấy hoặc xung đột trạng thái.
- Endpoint Admin: 22/22 trả 403 khi caller là tài khoản `User`; không có phiên Admin hợp lệ để kiểm thử kết quả thành công, duyệt/từ chối/ẩn tin, hoặc API quản trị có tác dụng phụ.
- Authentication: đăng ký hợp lệ, duplicate, validation, login hợp lệ/sai, `me`, logout và trạng thái cookie được kiểm tra. Chưa có email confirmation trong cấu hình hiện tại.
- Listing: tạo, đọc theo owner, cập nhật Draft, lỗi validation, submit, xung đột state, tìm kiếm công khai và ẩn tin Pending được kiểm tra.
- Consultation POST chỉ chạy ca dữ liệu sai để không tạo dữ liệu QA khó dọn; luồng tạo tư vấn hợp lệ chưa được xác minh.
- Upload media hợp lệ chưa được chạy; chỉ kiểm tra route với ID không tồn tại. Không có ảnh/video QA được lưu.
- Các test này không thay thế kiểm thử mọi giá trị biên, database concurrency, SQL failure injection hoặc tài khoản Admin được cấp quyền.
