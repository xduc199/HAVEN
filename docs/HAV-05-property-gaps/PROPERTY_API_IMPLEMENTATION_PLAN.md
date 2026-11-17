# HAV-05 Property API — kế hoạch triển khai

**Ngày kiểm tra:** 28/10/2026  
**Branch:** `feature/HAV-05-property-api`  
**Base đã xác minh:** `HEAD` và `develop` cùng ở `89993a71d85adc541bc63fa3b4494b0be503cd5d`.

Đây là kế hoạch, không phải xác nhận endpoint đã hoàn tất. Các contract bên dưới là đề xuất cần thành viên liên quan xác nhận trước khi triển khai. Không thay đổi implementation trong lượt này.

## 1. Trạng thái hiện tại đã đối chiếu source

- `Listing` và `ListingApplicationService` có create/update/owned-get/submit-review và approve/reject/hide. Domain kiểm tra validation và state transition; test service/domain tương ứng đang có.
- `PublicListingsController` có `GET /api/listings/{id}`. Query repository lọc `Approved`; service trả DTO công khai và loại `OwnerId`/`RejectionReason`; controller trả `400` cho ID không dương và `404` nếu không có kết quả.
- `IListingRepository`/`EfListingRepository` chưa có truy vấn danh sách theo owner. `GetOwnedAsync` hiện tải một listing theo ID rồi so sánh owner trong Application; đây không thay cho truy vấn owner-filter ở DB khi liệt kê.
- `Program.cs` đăng ký DI cho listing và gọi `UseAuthorization()`, nhưng không thấy đăng ký authentication scheme/Identity, `UseAuthentication()` hoặc admin policy. `UseAuthorization()` đơn lẻ không xác thực principal.
- Toàn bộ source controller/service/repository chưa cho thấy personal/admin endpoints hoặc Search endpoint `GET /api/listings`.
- Integration tests dùng `WebApplicationFactory` và stub repository cho public detail. Chưa thấy authentication test handler/fixture hoặc test persistence với SQL Server trong các test liên quan.
- Git: branch đúng; chỉ có báo cáo audit trước đó là file untracked. Không thấy staged/unstaged diff hay thay đổi implementation.

Chi tiết kiểm tra trước đó và test counts có tại [PROPERTY_API_AUDIT.md](PROPERTY_API_AUDIT.md). Không chạy lại build/test trong lượt lập kế hoạch này vì không đổi code.

## 2. Bảng dependency theo endpoint

| Endpoint | Trạng thái | Dependency để mở | Owner phối hợp |
|---|---|---|---|
| `GET /api/listings/{id}` | Đã có implementation và integration test | Không có dependency chặn | Property giữ contract hiện có |
| `POST /api/listings` | Chưa có controller/HTTP contract | Scheme mặc định, claim user ID, quy tắc principal thiếu ID; auth integration test | Person 1, Person 5 |
| `GET /api/my/listings` | Chưa có endpoint/repository query | Auth contract; owner-filter DB; quyết định sort/pagination và owner DTO | Person 1, Person 5 |
| `GET /api/my/listings/{id}` | Chưa có endpoint | Auth contract; chính sách conceal ownership (đề xuất 404); owner response DTO | Person 1, Person 5 |
| `PUT /api/my/listings/{id}` | Chưa có endpoint | Auth contract; validation/exception-to-HTTP policy; 404 ownership; auth integration tests | Person 1, Person 5 |
| `POST /api/my/listings/{id}/submit-review` | Chưa có endpoint | Auth contract; ownership; state-conflict HTTP semantics; auth integration tests | Person 1, Person 5 |
| `GET /api/listings` | Chưa thấy implementation; Search thuộc phạm vi khác | Chủ sở hữu route, filter/sort/page contract, DTO và public visibility | Person 3; Person 5 về persistence tests |
| Admin list/detail nếu còn trong scope | Chưa có endpoint; chưa xác định scope/contract | Admin policy/role, quyền xem nội bộ, DTO và query semantics | Person 1; Person 3 nếu cùng query/search; Person 5 |
| `POST /api/admin/listings/{id}/approve` | Application use case có; HTTP endpoint chưa có | Admin policy đã đăng ký, auth fixture, lỗi state conflict | Person 1, Person 5 |
| `POST /api/admin/listings/{id}/reject` | Application use case có; HTTP endpoint chưa có | Như approve; request DTO và validation lý do từ chối | Person 1, Person 5 |
| `POST /api/admin/listings/{id}/hide` | Application use case có; HTTP endpoint chưa có | Như approve; state conflict contract | Person 1, Person 5 |

## 3. Contract đề xuất cần thống nhất

### Person 1 — Identity/authentication/authorization

1. Authentication scheme/default challenge được đăng ký ở đâu và principal đáng tin cậy cho các API là loại nào?
2. Claim chuẩn chứa ID tài khoản là claim nào (`NameIdentifier`, `sub` hay claim riêng)? ID có thể null/rỗng không; quy tắc chuẩn hóa/casing ra sao?
3. User API dùng `[Authorize]` mặc định hay policy cụ thể? Khi authenticated nhưng thiếu user ID claim thì trả `401`, `403` hay lỗi cấu hình nội bộ?
4. Tên policy/role admin chính xác là gì, ai đăng ký, và role claim mapping được cấu hình ra sao?
5. Endpoint admin cần chỉ policy admin hay còn quyền riêng cho moderation?

### Person 3 — Backend Search

1. `GET /api/listings` có thuộc Search và dùng chung controller `PublicListingsController` hay route/controller riêng?
2. Filter đã chốt gồm field nào (property type, purpose, giá, diện tích, vị trí)? Có sort key và thứ tự mặc định nào?
3. Pagination dùng `page/pageSize` hay cursor; giới hạn tối đa, giá trị mặc định và response metadata nào?
4. Search chỉ trả Approved như public detail chứ? Có cần trả total count không?
5. Search repository/query abstraction và DTO nào sẽ là nguồn dùng chung để tránh Property tạo truy vấn song song?

### Person 5 — test/persistence

1. Có `WebApplicationFactory` fixture dùng chung và cơ chế thay auth scheme bằng test-only handler không? Claims/roles test phải cùng tên contract Person 1 chốt.
2. Test database chuẩn là SQL Server/container hay provider khác? Có sẵn fixture/CI service và quy tắc cleanup nào?
3. Ai sở hữu integration test cho Property API; có convention test file/fixture cần giữ không?
4. Với owner list, Person 5 có yêu cầu test xác nhận filter chạy ở DB và pagination/query không tải toàn bộ rows không?

## 4. Contract HTTP sơ bộ

Đề xuất để review, chưa phải contract đã duyệt:

| Hành vi | Response đề xuất | Ghi chú |
|---|---|---|
| Tạo thành công | `201 Created` + owner response DTO và Location tới `/api/my/listings/{id}` | Server lấy OwnerId từ claim; không có OwnerId trong request |
| Validation request/domain | `400 Bad Request` | Cần thống nhất exception mapping để lỗi validation không thành 500 |
| Chưa đăng nhập | `401 Unauthorized` | Sau khi scheme/challenge được Person 1 xác nhận |
| Đã đăng nhập, thiếu quyền admin | `403 Forbidden` | Admin endpoint; không áp dụng thay cho conceal ownership |
| ID không tồn tại hoặc không thuộc user | `404 Not Found` | Che giấu existence; áp dụng cho owner detail/update/submit |
| State transition không hợp lệ | `409 Conflict` | Không đổi rule domain; chỉ ánh xạ HTTP |
| Đọc/cập nhật thành công | `200 OK` với DTO | DTO owner có thể gồm Status và RejectionReason của chính chủ; không trả EF entity |
| Gửi duyệt/moderation thành công | `200 OK` với DTO hoặc `204 No Content` | Chọn một convention thống nhất trước implementation |
| Danh sách rỗng | `200 OK` với items rỗng | Pagination metadata theo Search/shared API convention nếu đã có |

## 5. Endpoint có thể triển khai ngay

Không có endpoint mới trong yêu cầu này có thể mở an toàn ngay: personal write/read đòi principal đã xác thực, admin cần policy, còn public list cần quyết định của Person 3. Public detail đã có; chỉ giữ nguyên. Sau khi Person 1 chốt contract, có thể triển khai các personal endpoints trong phạm vi Property mà không phải xây Identity. Sau khi policy admin sẵn sàng, có thể thêm moderation controller mỏng gọi application service hiện hữu.

## 6. File dự kiến cần thay đổi

Danh sách dự kiến, sẽ điều chỉnh theo contract/thực trạng lúc triển khai. Không file nào trong danh sách dưới đây được sửa bởi kế hoạch này.

### Personal API

- `src/Haven.Web/Program.cs` — chỉ tích hợp scheme/policy đã được Identity đăng ký nếu cần; không tự định nghĩa authentication.
- `src/Haven.Web/Controllers/MyListingsController.cs` — các route `/api/my/listings...`, đọc user ID từ principal và gọi application service.
- `src/Haven.Web/Contracts/Listings/CreateListingRequest.cs`, `UpdateListingRequest.cs` — request contracts nếu convention/validation dự án chọn DTO ở Web.
- `src/Haven.Application/Listings/OwnerListingDetails.cs` (hoặc tên thống nhất) — response DTO chủ sở hữu, không trả entity.
- `src/Haven.Application/Abstractions/IListingRepository.cs` — query theo owner có filter/pagination đã thống nhất.
- `src/Haven.Infrastructure/Repositories/EfListingRepository.cs` — thực hiện filter/sort/pagination trong database.
- `src/Haven.Application/Listings/ListingApplicationService.cs` — thêm use case list/map DTO nếu đặt ở Application theo convention đã chọn; giữ domain logic hiện có.
- `tests/Haven.UnitTests/Listings/ListingApplicationServiceTests.cs` và có thể test mới cho owner query/service.
- `tests/Haven.IntegrationTests/MyListingsApiTests.cs` và test-only auth fixture được chia sẻ/duyệt cùng Person 5.

### Moderation API

- `src/Haven.Web/Controllers/AdminListingsController.cs` — chỉ sau khi policy/role admin đã tồn tại; action approve/reject/hide gọi service hiện có.
- `src/Haven.Web/Contracts/Listings/RejectListingRequest.cs` — request lý do từ chối, nếu contract không đặt ở nơi khác.
- `tests/Haven.IntegrationTests/AdminListingsApiTests.cs` và auth fixture của Person 5.
- `Program.cs` chỉ nếu cần gắn policy có sẵn, không đăng ký role/policy phỏng đoán.

### Public Search/list

Không liệt kê file triển khai Property: Person 3 xác nhận ownership và contract trước; dùng Search controller/query/DTO/repository hiện có hoặc do Search sở hữu.

## 7. Test cases và điều kiện nghiệm thu

| Endpoint | Test cần có | Điều kiện nghiệm thu |
|---|---|---|
| `GET /api/listings/{id}` (đã có) | Approved `200`; Draft/Pending/Rejected/Hidden và missing đều `404`; ID không hợp lệ `400`; JSON không có OwnerId/RejectionReason; cancellation/metadata theo contract | Giữ test hiện có; bổ sung thiếu sót nếu review xác nhận. Không đổi response đã thống nhất âm thầm |
| `POST /api/listings` | Anonymous `401`; authenticated tạo `201`; OwnerId lấy claim kể cả body có field dư; active property type thành công; thiếu/inactive type, enum, required field, giá/diện tích <=0, room âm trả `400`; Draft ban đầu; DTO không lộ entity | Scheme thật được dùng; request không thể chọn owner/status; domain/service được gọi; Location/response đúng contract |
| `GET /api/my/listings` | Anonymous `401`; owner A chỉ nhận rows A; rỗng `200`; page biên/size ngoài range; không có dữ liệu người khác; response pagination chuẩn | Filter owner và page ở DB; thứ tự ổn định; không trả EF entity; DTO/state theo hợp đồng |
| `GET /api/my/listings/{id}` | Anonymous `401`; owner `200`; missing và owner mismatch cùng `404`; DTO owner có thể chứa state/rejection theo contract nhưng không lộ trường hệ thống thừa | Không đọc listing người khác; owner ID từ principal; cancellation được truyền |
| `PUT /api/my/listings/{id}` | Anonymous `401`; owner cập nhật; mismatch/missing `404`; validation `400`; Pending/Hidden `409`; Approved -> Pending; Rejected -> Draft và clear rejection metadata; payload giả OwnerId/Status không đổi dữ liệu hệ thống | Application/domain là nơi enforce rule; không ghi khi sai owner/validation/state; response đúng DTO |
| `POST /api/my/listings/{id}/submit-review` | Anonymous `401`; owner Draft/Rejected -> Pending; mismatch/missing `404`; Pending/Approved/Hidden -> `409`; không ghi khi transition lỗi | Chỉ Draft/Rejected chuyển được; không thể submit listing khác; lý do rejection cũ được domain xử lý đúng |
| Moderation approve/reject/hide | Anonymous `401`; authenticated non-admin `403`; admin transition hợp lệ; listing missing `404`; state sai `409`; reject reason thiếu/sai `400`; không lưu khi lỗi | Mọi action gắn admin policy thật; không có bypass; application/domain transition giữ nguyên |
| `GET /api/listings` | Test do Search sở hữu: Approved-only, filter từng loại/kết hợp, sort ổn định, paging/total và validation; ẩn state khác | Chỉ triển khai sau khi Person 3 duyệt route/contract; query filter/page ở DB; không lộ dữ liệu nội bộ |

Integration tests phải dùng auth fixture test-only, không đăng ký handler giả trong production. Persistence tests phải theo provider/fixture nhóm đã chốt; test hiện tại dùng stub không được xem là bằng chứng truy vấn SQL Server.

## 8. Câu hỏi chuyển cho thành viên

**Person 1:** Xin xác nhận scheme/default authenticate và challenge; claim chuẩn chứa user ID và quy tắc normalize; hành vi khi thiếu claim; tên admin role/policy cùng mapping role claims; có policy moderation chuyên biệt không?

**Person 3:** Xin xác nhận `GET /api/listings` thuộc Search, route/controller dùng chung hay riêng; filter/sort keys, mặc định và biên; paging convention/maximum/metadata; chỉ Approved hay không; total count và query abstraction/DTO sẽ cung cấp.

**Person 5:** Xin chỉ ra fixture `WebApplicationFactory`/test authentication hiện có hoặc contract để dùng chung; provider/database test và CI service; convention ownership/cleanup của integration tests; test nào cần SQL Server thực để xác nhận owner filter/pagination.

## 9. Git

Sau khi thêm tài liệu này, dự kiến chỉ có hai file báo cáo untracked:

- `docs/HAV-05-property-gaps/PROPERTY_API_AUDIT.md`
- `docs/HAV-05-property-gaps/PROPERTY_API_IMPLEMENTATION_PLAN.md`

Không commit, push hoặc tạo PR. Endpoint nào cũng tiếp tục ở trạng thái chưa hoàn thành cho đến khi có implementation và test evidence thực tế.
