# HAV-05 — quyết định dependency trước implementation

**Ngày rà soát:** 28/10/2026  
**Branch:** `feature/HAV-05-property-api` (`HEAD` = `89993a71d85adc541bc63fa3b4494b0be503cd5d`)

Nhãn trong tài liệu:

- **Đã xác minh**: thấy trực tiếp trong source/test tại HEAD nêu trên.
- **Đề xuất**: phương án để các owner review, chưa phải contract.
- **Chờ xác nhận**: quyết định của owner tương ứng cần có trước implementation phụ thuộc.

## 1. Kết luận nhất quán tài liệu

Hai tài liệu hiện có **nhất quán về trạng thái và phạm vi chính**: domain/service đã có; public detail theo ID đã có; personal HTTP API và admin HTTP API chưa có; `GET /api/listings` cần Person 3; auth/admin policy chưa được xác minh; chưa có thay đổi implementation.

Không phát hiện mâu thuẫn làm sai trạng thái endpoint. Có một khác biệt về độ chi tiết, không phải mâu thuẫn: `PROPERTY_API_AUDIT.md` nói service kiểm tra ownership; `PROPERTY_API_IMPLEMENTATION_PLAN.md` làm rõ `GetOwnedAsync` hiện tải theo ID rồi so OwnerId ở Application, còn repository chưa có owner list query. Chi tiết thứ hai là đúng theo source và nên được giữ trong thiết kế list. Owner detail/update/submit cũng phải trả cùng hành vi missing/mismatch đã chốt.

**Đã xác minh trong source:** `Program.cs` gọi `UseAuthorization()` nhưng không thấy đăng ký scheme/Identity, `UseAuthentication()` hoặc admin policy; controller hiện có là public detail; repository có get-by-ID/public-ID, add/save nhưng chưa list-by-owner; integration test public dùng stub. Không tìm thấy Search controller/query hoặc auth test fixture trong phần source/tests đã rà soát.

Không có endpoint mới có thể mở ngay theo yêu cầu bảo mật: personal routes chờ auth contract; moderation chờ admin authorization; public list chờ Search owner/contract. Giữ nguyên public detail.

## 2. Quyết định còn thiếu — Person 1 (Identity/authentication/authorization)

### 1A. Scheme và principal cho API

- **Câu hỏi cần xác nhận:** API dùng scheme mặc định nào (cookie, bearer hoặc scheme khác)? Scheme/principal được đăng ký bởi module nào và Web chỉ cần tích hợp gì?
- **Vì sao chặn:** `[Authorize]` không bảo vệ đúng nếu chưa có authenticate/challenge scheme hoạt động; không thể viết integration test chứng minh anonymous nhận 401.
- **Phương án cân nhắc:** dùng scheme đã được ứng dụng/Identity đăng ký; nếu nhiều scheme thì gắn policy scheme cụ thể; không tạo scheme mới trong Property.
- **Ảnh hưởng:** `Program.cs`/controller metadata; `WebApplicationFactory` auth test fixture; không nên đặt auth logic trong Application service.
- **Điều kiện chốt:** Person 1 chỉ rõ scheme/policy dùng được trong runtime, module sở hữu DI/pipeline, và có xác nhận request anonymous bị challenge như mong đợi.

### 1B. Claim user ID và chuẩn hóa

- **Câu hỏi cần xác nhận:** Claim nào là user ID (`NameIdentifier`, `sub` hay claim riêng)? Claim có bảo đảm duy nhất, không rỗng và ổn định không? Có quy tắc casing/normalization không?
- **Vì sao chặn:** `Listing.OwnerId` là string; controller phải lấy đúng ID từ principal để gọi service, không thể suy đoán claim hoặc nhận OwnerId từ body/query.
- **Phương án cân nhắc:** chuẩn hóa claim về một claim type thống nhất ở Identity; hoặc cung cấp helper/service đã được Identity sở hữu. Property chỉ đọc hợp đồng đã thống nhất.
- **Ảnh hưởng:** controller/user-ID accessor, unit/integration tests claims, có thể DTO không nhận OwnerId; service hiện nhận `ownerId` string nên chưa cần đổi nếu hợp đồng khớp.
- **Điều kiện chốt:** tên claim/cách truy xuất, format/casing, hành vi khi thiếu/không hợp lệ được ghi trong contract hoặc code Identity và auth fixture tạo đúng claim đó.

### 1C. Principal đã xác thực nhưng thiếu user ID

- **Câu hỏi cần xác nhận:** Nếu authentication thành công nhưng principal không có claim ID hợp lệ thì API trả gì và ghi log ở đâu?
- **Vì sao chặn:** không được biến thiếu claim thành empty owner ID, dùng ID do client gửi, hoặc trả 404 giả cho lỗi cấu hình danh tính.
- **Phương án cân nhắc:** từ chối với 401/403; hoặc coi là lỗi principal/configuration và trả lỗi server có log. Chọn theo semantics Identity hiện có.
- **Ảnh hưởng:** controller/accessor, exception/error mapping, integration test principal thiếu claim.
- **Điều kiện chốt:** status/response được thống nhất và có test cho authenticated principal thiếu claim.

### 1D. Quyền admin và moderation

- **Câu hỏi cần xác nhận:** Tên role/policy admin chính xác là gì? Ai đăng ký? Role claim mapping nào được dùng? Approve/reject/hide có cùng một policy không?
- **Vì sao chặn:** service moderation không tự xác thực người gọi; endpoint không có policy thật là quyền moderation công khai.
- **Phương án cân nhắc:** dùng policy admin hiện hữu; hoặc Identity cung cấp policy chuyên biệt moderation. Không tự thêm role/policy trong Property.
- **Ảnh hưởng:** admin controller attributes, `Program.cs` chỉ nếu cần gắn policy đã đăng ký, test role mapping và admin/non-admin.
- **Điều kiện chốt:** tên policy và semantics được đăng ký trong Identity/app composition; Person 1 xác nhận admin claim được nhận diện; test admin/regular/anonymous dùng cùng contract.

### 1E. API user dùng policy nào

- **Câu hỏi cần xác nhận:** Personal routes chỉ yêu cầu authenticated hay yêu cầu policy riêng (ví dụ account active/verified)?
- **Vì sao chặn:** quyết định này xác định ai được tạo/sửa/gửi duyệt và có thể ảnh hưởng response challenge/forbid.
- **Phương án cân nhắc:** `[Authorize]` mặc định nếu mọi authenticated account đủ điều kiện; policy user-specific nếu Identity có điều kiện bổ sung.
- **Ảnh hưởng:** controller metadata, integration test danh tính hợp lệ/không đủ điều kiện.
- **Điều kiện chốt:** policy hoặc xác nhận dùng default authenticated policy và trạng thái account được phép gọi API.

## 3. Quyết định còn thiếu — Person 3 (Backend Search)

### 3A. Ownership và route của public list

- **Câu hỏi cần xác nhận:** `GET /api/listings` thuộc Search hoàn toàn chứ? Dùng `PublicListingsController` hay Search controller riêng? Có yêu cầu endpoint list trước khi Search hoàn tất không?
- **Vì sao chặn:** tránh route/controller trùng và hai implementation public query khác nhau.
- **Phương án cân nhắc:** Search sở hữu route; Property chỉ cung cấp/duy trì detail theo ID; hoặc Search cung cấp abstraction dùng chung nếu có yêu cầu cụ thể.
- **Ảnh hưởng:** controller, Application query/service, repository và OpenAPI ownership.
- **Điều kiện chốt:** Person 3 xác nhận owner, route, controller/query abstraction và phần việc cụ thể của Person 2.

### 3B. Search filter/sort semantics

- **Câu hỏi cần xác nhận:** Các filter nào được duyệt (property type, purpose, min/max price/area, location)? Sort keys/default và secondary sort ổn định là gì? Filter không hợp lệ xử lý ra sao?
- **Vì sao chặn:** ảnh hưởng query shape, response contract, index/test; tự đặt semantics có thể làm lệch Search/MVC UI.
- **Phương án cân nhắc:** chỉ dùng bộ lọc MVP đã chốt; whitelist sort keys; không nhận raw sort expression.
- **Ảnh hưởng:** Search query DTO/service/repository, SQL, response metadata, tests; không triển khai từ Property nếu Search sở hữu.
- **Điều kiện chốt:** bảng filter/sort gồm kiểu dữ liệu, null/empty semantics, defaults và lỗi đã được Person 3 chia sẻ/review.

### 3C. Pagination và kết quả public

- **Câu hỏi cần xác nhận:** Offset (`page/pageSize`) hay cursor? Default/max page size? Có total count không? Public list luôn chỉ gồm Approved?
- **Vì sao chặn:** quyết định abstraction/repository, thứ tự ổn định, metadata và tải DB; public visibility là điều kiện bảo mật dữ liệu.
- **Phương án cân nhắc:** dùng pagination abstraction chung nếu đã tồn tại; nếu không, Search đề xuất contract nhỏ có limit và stable order. Không tự thiết kế song song trong Property.
- **Ảnh hưởng:** response DTO, query/repository, SQL và integration test database.
- **Điều kiện chốt:** mẫu request/response, biên pagination, semantics out-of-range, total-count và status filter được ghi thành contract; Search xác nhận Approved-only.

## 4. Quyết định còn thiếu — Person 5 (Testing/CI/persistence)

### 5A. Authentication fixture cho integration tests

- **Câu hỏi cần xác nhận:** Có `WebApplicationFactory`/test auth handler dùng chung không? Ai sở hữu? Cách cấu hình user ID claim và admin role/policy như thế nào?
- **Vì sao chặn:** tests phải xác minh 401/403 và owner isolation thật; không được tạo test giả hoặc đăng ký auth bypass ở production.
- **Phương án cân nhắc:** fixture test-only thay scheme mặc định; hoặc host test đăng ký handler tương thích với policy Identity. Claims/roles phải theo Person 1.
- **Ảnh hưởng:** integration test fixture, `MyListingsApiTests`, `AdminListingsApiTests`, service registrations chỉ trong test host.
- **Điều kiện chốt:** fixture có ví dụ anonymous/user A/user B/admin/non-admin và chạy trong test host mà không ảnh hưởng production registration.

### 5B. Persistence test provider

- **Câu hỏi cần xác nhận:** Owner filter/pagination cần kiểm thử với SQL Server thật/container hay provider nào được CI hỗ trợ? Có fixture/service/cleanup sẵn không?
- **Vì sao chặn:** unit/stub test không xác minh EF query; provider khác SQL Server có thể có khác biệt về dịch/ordering.
- **Phương án cân nhắc:** SQL Server test container/CI service theo thiết lập chung; hoặc nếu chỉ dùng provider khác, ghi rõ giới hạn và giữ test repository phù hợp.
- **Ảnh hưởng:** repository tests/fixture, CI workflow/service config thuộc quyền Person 5, database cleanup; migration chỉ khi schema cần thật sự đổi.
- **Điều kiện chốt:** provider, connection lifecycle, data isolation/cleanup và CI availability được xác nhận; không dùng database production.

### 5C. Test ownership và tiêu chí nghiệm thu

- **Câu hỏi cần xác nhận:** Person 2 hay Person 5 sở hữu các integration test Property? CI chạy test project/lệnh nào? Có convention test report/coverage cần tuân theo không?
- **Vì sao chặn:** tránh sửa trùng fixture/test do nhiều người sở hữu và tránh đánh giá bằng test chưa chạy trong CI.
- **Phương án cân nhắc:** Person 2 thêm tests cạnh endpoint; Person 5 sở hữu fixture/CI và review persistence cases; hoặc Person 5 chịu toàn bộ integration tests theo phân công.
- **Ảnh hưởng:** test file locations, workflow và báo cáo nghiệm thu; không thay đổi business implementation.
- **Điều kiện chốt:** người sở hữu từng test/fixture, lệnh CI cần pass và coverage acceptance được ghi trong task/PR contract.

## 5. Quy tắc HTTP/API cần chốt trước hoặc trong contract review

Các tài liệu kế hoạch trước đưa ra vài response dưới dạng đề xuất. Chúng **chưa được xem là quyết định**:

- **Đề xuất 404 cho owner mismatch** để không tiết lộ listing tồn tại; cần xác nhận đây là convention chung thay vì 403.
- **Đề xuất 409 cho state transition không hợp lệ**, nhưng hiện domain ném `InvalidOperationException` và `Program.cs` không có API exception mapping riêng nhìn thấy trong source. Cần quyết định mapping có phạm vi đúng, tránh biến lỗi bất ngờ thành 409 hoặc 500 không rõ nghĩa.
- **Đề xuất 400 cho validation**; cần xác nhận cách phân biệt input/model validation với exception domain/application.
- Tạo đề xuất `201 Created` + Location; update/read `200`; transition `200` hay `204` còn mở.
- Owner DTO có thể cần Status/RejectionReason để người dùng quản lý tin, nhưng fields cụ thể, quyền xem rejection reason và timestamp phải được chốt.

**Ảnh hưởng:** controller response mapping, DTO, OpenAPI metadata và HTTP integration tests. **Tiêu chí chốt:** status/body/headers cho từng success/error case được ghi trong API contract, được Person 1 review phần auth semantics, Person 2 review domain semantics và Person 5 xác nhận có thể kiểm thử.

## 6. Thứ tự phụ thuộc để Person 2 bắt đầu an toàn

1. **Person 1 xác nhận 1A–1C và 1E** (scheme/pipeline, user-ID claim, thiếu claim, quyền user). Chưa cần chờ admin policy để bắt đầu personal API, nhưng không được expose endpoint trước khi auth có hiệu lực.
2. **Person 5 xác nhận 5A và 5C** (test auth fixture/ownership); đồng thời thống nhất với Person 1 rằng fixture phản ánh cùng claim/policy.
3. **Person 2 chốt HTTP/error mapping và owner DTO** theo mục 5; không đổi domain transition.
4. **Person 2 có thể triển khai personal API**: create, owner list (sau khi chốt paging), owner detail, update, submit. Owner list cần 3B/3C chỉ nếu dùng chung Search pagination; nếu chưa có convention chung, route cá nhân vẫn cần contract pagination riêng được Person 1/2/5 chấp thuận, không được tự suy diễn từ Search.
5. **Person 5 xác nhận 5B** trước khi viết/chạy persistence tests phụ thuộc provider; implementation EF query có thể làm khi contract owner/paging đã rõ, nhưng acceptance query chưa xong tới khi provider test chạy.
6. **Person 1 xác nhận 1D**, rồi Person 5 5A admin claims, trước khi Person 2 tạo admin moderation controller.
7. **Person 3 xác nhận 3A–3C** trước bất kỳ thay đổi nào tới `GET /api/listings`; Search list có thể triển khai độc lập bởi Search và không chặn personal endpoints ngoài pagination convention nếu các đội chọn contract dùng chung.
8. Chạy build/unit/integration tests và cập nhật evidence sau implementation. Không đánh dấu hoàn thành trước khi các test bảo mật và HTTP thực tế pass.

## 7. Điều kiện tối thiểu để Person 2 bắt đầu code

- **Personal routes:** 1A, 1B, 1C, 1E được trả lời; user ID accessor/claim contract đã được source hoặc owner Identity xác nhận; 5A có đường test authenticated/anonymous; status code cho validation, ownership mismatch và transition lỗi được thống nhất. Owner list cần page/sort/DTO contract trước khi thêm repository method.
- **Admin routes:** tất cả điều kiện personal auth cần thiết cộng 1D policy/role đã đăng ký, semantics 403 và fixture chứng minh anonymous/non-admin/admin.
- **Public list/search:** 3A–3C được Person 3 chốt; Person 5 xác nhận cách test query/pagination. Không bắt đầu endpoint trong Property trước xác nhận ownership.
- **Public detail hiện hữu:** không phụ thuộc các quyết định trên; chỉ sửa nếu phát hiện lỗi cụ thể và có test chứng minh.

## 8. Git và thực hiện lượt rà soát

Trước khi tạo tài liệu này, branch là `feature/HAV-05-property-api`, HEAD `89993a7`; hai tài liệu đầu vào ở trạng thái untracked, không có thay đổi staged/unstaged khác. Sau lượt này chỉ thêm tài liệu `DEPENDENCY_DECISIONS.md`.

Không sửa source, tests, cấu hình auth, migrations hoặc hai tài liệu đầu vào. Không commit, push hoặc tạo PR.
