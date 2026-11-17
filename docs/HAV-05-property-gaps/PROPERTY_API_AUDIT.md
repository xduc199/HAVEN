# HAV-05 Property API — audit và bàn giao

**Ngày kiểm tra:** 28/10/2026  
**Branch:** `feature/HAV-05-property-api`  
**HEAD / `develop`:** `89993a71d85adc541bc63fa3b4494b0be503cd5d` (đồng nhất khi kiểm tra)

## Kết luận

Domain, application service, EF Core persistence và API chi tiết công khai theo ID đã có. Chưa thể nghiệm thu toàn bộ HAV-05: API quản lý cá nhân cần Identity/authentication; API moderation cần thêm authorization admin. Không có thay đổi code hoặc schema trong lượt audit này.

## Bảng audit

| Hạng mục | Hiện trạng thực tế | Thay đổi cần thiết | Phụ thuộc | Quyết định |
|---|---|---|---|---|
| Domain Listing và validation | Có; tạo Draft, validate trường/giá/diện tích/phòng/enum; giữ các state transition theo yêu cầu | Không | — | Đã hoàn thành; unit tests hiện có |
| Application use cases | Có create, owned get, update, submit review và approve/reject/hide | Chưa cần thay đổi | — | Đã hoàn thành ở tầng Application |
| EF repository, mapping, migration | Có truy vấn theo ID/public ID, add/save; cấu hình và migration Listing/PropertyType | Chỉ thêm truy vấn owner khi endpoint cá nhân được mở | Identity/API cá nhân | Không đổi schema hoặc migration |
| Public detail | `GET /api/listings/{id}`; chỉ Approved; DTO không có OwnerId/RejectionReason; thiếu/ẩn đều 404 | Không | — | Đã hoàn thành, có integration tests |
| Personal API | Chưa tìm thấy route/controller tương ứng trong toàn `src`; service chưa có danh sách theo owner | Thêm endpoint, owner-filter ở DB, DTO và xử lý lỗi sau khi có principal contract | Person 1 — Identity | Chờ; không tạo endpoint ghi khi chưa xác thực |
| Admin moderation API | Chưa tìm thấy route; application methods đã có | Thêm endpoint và integration tests sau khi xác nhận policy/role admin | Person 1 — Identity/authorization | Chờ; không tạo endpoint quản trị không bảo vệ |
| Public search/list | Chưa tìm thấy `GET /api/listings` hay Search service/controller trong `src` | Chốt route/contract và chủ sở hữu trước khi triển khai | Person 3 — Backend Search | Không triển khai trong HAV-05 Property |
| API docs/acceptance | Báo cáo tiến độ trên nhánh gaps mô tả trạng thái cũ | Tài liệu audit hiện tại được ghi tại đây | Person 5 — docs/QA, nếu chuẩn hóa nghiệm thu chung | Bàn giao trạng thái và bằng chứng kiểm tra |

## API contract và trạng thái

| Method | Route | Authentication/authorization | Kết quả hiện tại |
|---|---|---|---|
| GET | `/api/listings/{id}` | Public | `200` với tin Approved; `400` nếu ID không dương; `404` nếu không tồn tại/chưa Approved |
| POST | `/api/listings` | Cần user đã xác thực | Chưa triển khai; OwnerId phải lấy từ principal, tạo tin ở Draft |
| GET | `/api/my/listings` | Cần user đã xác thực | Chưa triển khai; cần truy vấn lọc owner ở database |
| GET | `/api/my/listings/{id}` | Cần user đã xác thực | Chưa triển khai; tài nguyên của người khác phải không bị lộ |
| PUT | `/api/my/listings/{id}` | Cần user đã xác thực | Chưa triển khai; service/domain đã áp dụng quyền sở hữu và state transition |
| POST | `/api/my/listings/{id}/submit-review` | Cần user đã xác thực | Chưa triển khai; service/domain chỉ cho Draft hoặc Rejected |
| GET | `/api/listings` | Public, do Search xác định | Chưa thấy implementation; cần phối hợp Person 3 |
| POST | `/api/admin/listings/{id}/approve`, `/reject`, `/hide` | Cần user đã xác thực và policy admin | Chưa triển khai; use case Application có sẵn |

`Program.cs` có `UseAuthorization()` nhưng chưa đăng ký authentication scheme/Identity hoặc role/policy admin. Vì vậy API cá nhân và moderation chưa được mở. Không nhận OwnerId/Status hoặc metadata hệ thống từ client. Quy tắc domain hiện có được giữ nguyên.

## Bằng chứng kiểm tra

- `dotnet build HAVEN.slnx`: thành công, 0 warning, 0 error.
- Unit tests: 61 passed, 0 failed, 0 skipped.
- Integration tests: 8 passed, 0 failed, 0 skipped.
- `git diff --check`: sạch trước khi thêm tài liệu này.
- Test API công khai dùng repository stub; chưa xác minh truy vấn persistence trên SQL Server thật.
- Không chạy CI/GitHub Actions trong lượt này.

## Bước tiếp theo

1. Person 1 xác nhận auth scheme, claim chứa user ID, cách chuẩn hóa ID và policy/role admin.
2. Sau hợp đồng Identity, Property bổ sung API cá nhân, owner-filter trong repository, DTO và integration tests theo test-auth convention.
3. Person 3 xác nhận chủ sở hữu/contract cho `GET /api/listings`.
4. Person 5 phối hợp test persistence/SQL Server và quy chuẩn báo cáo nghiệm thu nếu cần.

Không commit, push hoặc tạo PR.
