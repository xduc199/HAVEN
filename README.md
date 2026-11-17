# HAVEN 🏡

> **Home Awaits, Vision Empowers Navigation.**  
> Nền tảng khám phá và tìm kiếm bất động sản theo nhu cầu người dùng.

HAVEN là dự án web hướng tới việc hỗ trợ người dùng khám phá và tìm kiếm bất động sản theo nhu cầu. Backend hiện có xác thực bằng cookie/Identity, quản lý listing theo chủ sở hữu, tìm kiếm công khai, favorites, so sánh và các API kiểm duyệt/catalog quản trị. Các phạm vi nghiệp vụ chưa được xác định được ghi rõ bên dưới thay vì tự đặt quy tắc.

## 📌 Trạng thái dự án

- **Loại dự án:** Ứng dụng web ASP.NET Core MVC.
- **Framework:** .NET 10.
- **Cơ sở dữ liệu dự kiến sử dụng trong môi trường phát triển:** SQL Server 2022 chạy bằng Docker.
- **ORM:** Entity Framework Core.
- **Kiến trúc solution hiện tại:** Web, Application, Domain, Infrastructure và các project kiểm thử.
- **CI:** GitHub Actions tự động restore, build và chạy test khi push hoặc tạo Pull Request vào `main` / `develop`.

> Public, User và Admin workflows được triển khai bằng MVC/Razor và nối trực tiếp với Application services. Home/search/detail chỉ hiển thị listing đã Approved. Chủ tin có thể quản lý ảnh và video MP4/WebM; địa điểm mới dùng mã tỉnh/xã chính thức, còn địa chỉ cũ được giữ nguyên nếu chưa ánh xạ chắc chắn.

## 🧰 Công nghệ

- C# / ASP.NET Core MVC
- .NET SDK 10.0.401 (được khai báo trong `global.json`)
- Entity Framework Core 10
- Microsoft SQL Server 2022
- Docker Compose
- xUnit
- GitHub Actions

## 🖥️ Frontend

Frontend sử dụng ASP.NET Core MVC và Razor Views, không tạo SPA và không gọi HTTP loopback đến chính ứng dụng. Mặc định giao diện dùng `vi-VN`; ngày, số và trạng thái được hiển thị theo tiếng Việt. Khu quản trị có tổng quan, báo cáo, quản lý người dùng, kiểm duyệt tin, loại bất động sản, nhãn, tư vấn và yêu cầu quyền riêng tư.

- [API Contract](docs/frontend/API_CONTRACT.md) — route, request/response, auth và role hiện có.
- [Frontend Architecture](docs/frontend/FRONTEND_ARCHITECTURE.md) — phân lớp MVC và quy tắc tích hợp.
- [Implementation Plan](docs/frontend/FRONTEND_IMPLEMENTATION_PLAN.md) — trạng thái, tiêu chí và phụ thuộc từng phase.
- [Frontend Test Report](docs/frontend/FRONTEND_TEST_REPORT.md) — kiểm tra đã chạy và các giới hạn hiện tại.

Các nhóm route chính: Public (`/`, `/properties`, `/properties/{id}`, `/compare`, `/Privacy`, `/Terms`, `/PostingRules`, `/Complaints`, `/CookiePolicy`, `/About`); Account (`/account/register`, `/account/login`, `/account/profile`, `/account/change-password`, `/Account/Privacy`); User (`/profile/listings`, `/favorites`, `/consultations`); Admin (`/Admin/Dashboard`, `/Admin/Listings`, `/Admin/Users`, `/Admin/PropertyTypes`, `/Admin/Tags`, `/Admin/Consultations`, `/Admin/Reports`, `/Admin/PrivacyRequests`). `/Admin/Users` hỗ trợ tìm kiếm và khóa/mở khóa tài khoản User; không cho khóa tài khoản Admin hoặc tài khoản đang đăng nhập. `Home/MuaBan` và `Home/ChoThue` chuyển hướng sang tìm kiếm có lọc; `/Property` chuyển hướng sang `/properties`.

## 🗂️ Cấu trúc repository

```text
HAVEN/
├── .github/
│   └── workflows/
│       └── ci.yml
├── src/
│   ├── Haven.Web/                 # Điểm vào ứng dụng web, MVC, static assets
│   ├── Haven.Application/         # Tầng ứng dụng
│   ├── Haven.Domain/              # Tầng miền nghiệp vụ
│   └── Haven.Infrastructure/      # Tích hợp hạ tầng và EF Core
├── tests/
│   ├── Haven.UnitTests/
│   └── Haven.IntegrationTests/
├── docs/
│   └── frontend/                  # API contract, architecture, plan, test report
├── compose.yaml                   # SQL Server cho môi trường phát triển
├── global.json                    # Phiên bản .NET SDK
└── HAVEN.slnx                     # Solution
```

## ⚙️ Yêu cầu môi trường

Cài đặt các công cụ sau trên máy phát triển:

1. Git.
2. .NET SDK 10.0.401 hoặc phiên bản tương thích theo `global.json`.
3. Docker Desktop (đã khởi động Docker Engine).
4. Trình soạn thảo phù hợp, khuyến nghị Visual Studio hoặc Visual Studio Code.

Kiểm tra cài đặt:

```powershell
git --version
dotnet --version
docker --version
docker compose version
```

## 🚀 Cài đặt và chạy local

### 1. Clone repository

```powershell
git clone https://github.com/xduc199/HAVEN.git
cd HAVEN
```

### 2. Tạo file cấu hình môi trường

Tạo file `.env` từ file mẫu:

```powershell
Copy-Item .env.example .env
```

Mở `.env` và đặt giá trị mạnh, riêng cho máy phát triển của bạn:

```dotenv
MSSQL_SA_PASSWORD=ReplaceWithYourOwnStrongPassword123!
```

Đây chỉ là ví dụ định dạng. Hãy thay bằng mật khẩu riêng đáp ứng yêu cầu của SQL Server. **Không commit hoặc chia sẻ file `.env`**; Git đã được cấu hình bỏ qua file này. Chỉ commit `.env.example` với giá trị minh họa, không chứa bí mật.

### 3. Khởi động SQL Server

```powershell
docker compose up -d
docker compose ps
```

Dịch vụ SQL Server được ánh xạ tại `127.0.0.1:1433`. Dữ liệu phát triển được lưu trong Docker volume `haven_sql_data`.

Dừng container mà không xóa dữ liệu:

```powershell
docker compose down
```

Không dùng `docker compose down -v` trừ khi bạn chủ ý xóa volume dữ liệu phát triển.

### 4. Restore, build và test

Chạy từ thư mục gốc repository:

```powershell
dotnet restore HAVEN.slnx
dotnet build HAVEN.slnx --configuration Release --no-restore
dotnet test HAVEN.slnx --configuration Release --no-build
```

### 5. Tạo/cập nhật schema database

Sau khi đã cấu hình `HavenDb` trong user secrets hoặc biến môi trường, áp dụng các migration đã có:

```powershell
dotnet ef database update --project src/Haven.Infrastructure/Haven.Infrastructure.csproj --startup-project src/Haven.Web/Haven.Web.csproj
```

Lệnh này chỉ nên chạy trên database phát triển mà bạn chủ động chọn; không tự động áp dụng migration vào production.

Migration mới nhất `AddPolicyConsentAndPrivacyRequests` tạo lịch sử đồng ý chính sách và hàng đợi yêu cầu quyền riêng tư. Migration `AddListingVideosAndAdministrativeLocations` bổ sung metadata video, danh mục tỉnh/xã và hai mã địa điểm nullable cho tin đăng. Các cột văn bản Province/District hiện có không bị đổi hoặc backfill. Migration chỉ được tạo trong repository, chưa áp dụng vào bất kỳ database nào. Chỉ áp dụng lên database phát triển đã chọn bằng lệnh ở trên; ứng dụng không tự động chạy migration khi khởi động.

### Chính sách và quyền riêng tư

Các tài liệu công khai hiện ở phiên bản `1.0.0`, hiệu lực ngày `17/11/2026`: `/Privacy`, `/Terms`, `/PostingRules`, `/Complaints` và `/CookiePolicy`. Nội dung được tổ chức theo phiên bản; khi thay đổi, hãy thêm edition mới và giữ nguyên nội dung phiên bản đã công bố. Form đăng ký yêu cầu hai xác nhận riêng và lưu phiên bản, thời điểm UTC, hành động và nguồn ghi nhận. Người dùng xem lịch sử/yêu cầu tại `/Account/Privacy`; quản trị viên xử lý tại `/Admin/PrivacyRequests`.

Trước khi phát hành, điền tên đơn vị, email, điện thoại và địa chỉ thật trong `Haven:Legal:OperatorName`, `Haven:Legal:SupportEmail`, `Haven:Legal:SupportPhone`, `Haven:Legal:SupportAddress` qua cấu hình an toàn của môi trường. Mã nguồn để trống các trường này và giao diện đánh dấu chưa cấu hình; không dùng dữ liệu minh họa làm thông tin vận hành. Nội dung chính sách mô tả website đăng/tìm tin và gửi yêu cầu tư vấn theo source hiện tại; website chưa xử lý thanh toán hay ký kết. Chủ thể vận hành cần xác nhận phân loại, nghĩa vụ áp dụng và nhờ chuyên gia pháp lý rà soát trước khi công bố.

Văn bản chính thức dùng để định hướng nội dung: [Luật Bảo vệ dữ liệu cá nhân 91/2025/QH15](https://vanban.chinhphu.vn/?docid=214590&pageid=27160) và [Nghị định 356/2025/NĐ-CP](https://vanban.chinhphu.vn/?classid=1&docid=216387&pageid=27160); [Luật Bảo vệ quyền lợi người tiêu dùng 19/2023/QH15](https://vanban.chinhphu.vn/?classid=1&docid=208363&orggroupid=1&pageid=27160); [Luật Thương mại điện tử 122/2025/QH15](https://vanban.chinhphu.vn/?clas-=&docid=216503&pageid=27160) và [Nghị định 248/2026/NĐ-CP](https://vanban.chinhphu.vn/?classid=1&docid=218747&pageid=27160&typegroupid=4); [Luật Giao dịch điện tử 20/2023/QH15](https://vanban.chinhphu.vn/?docid=208421&pageid=27160). Việc có trang chính sách không tự xác nhận HAVEN tuân thủ đầy đủ các nghĩa vụ pháp luật.

Ảnh và video nằm trong cùng kho local ngoài webroot, mặc định `src/Haven.Web/App_Data/ListingImages` (được Git bỏ qua). Có thể chọn đường dẫn bằng `Haven:Media:StoragePath`; khóa lưu trữ là UUID do server tạo. Tất cả máy chạy HAVEN cần cùng vùng media bền vững hoặc cơ chế đồng bộ tệp phù hợp.

Tin đăng hỗ trợ tối đa 10 ảnh (10 MB mỗi ảnh) và tối đa 3 video MP4/WebM (mặc định 100 MiB mỗi video). Giới hạn video đặt tại `Haven:ListingVideos:MaximumVideosPerListing` và `Haven:ListingVideos:MaximumFileLengthBytes`; giới hạn an toàn tối đa là 3 video và 100 MiB mỗi tệp. Upload kiểm tra extension, MIME, chữ ký container và kích thước; video công khai chỉ được phát khi tin Approved và hỗ trợ HTTP Range. Chủ tin thay đổi media của tin Approved sẽ đưa tin về Pending.

Danh mục địa giới nằm tại `src/Haven.Infrastructure/Data/Locations/vietnam-administrative-units-2025-07-01.json`: 34 tỉnh/thành, 3.321 xã/phường/đặc khu, hiện hành theo mã đang có hiệu lực ngày 17/11/2026. Nguồn dữ liệu là [Cục Thống kê, danh mục mã cấp xã](https://www.nso.gov.vn/wp-content/uploads/2025/06/Danh-sach-va-ma-so-cac-don-vi-hanh-chinh-cap-xa.pdf); cơ sở pháp lý là [Quyết định 19/2025/QĐ-TTg](https://vanban.chinhphu.vn/?classid=0&docid=214409&pageid=27160), hiệu lực từ 01/07/2025. Số lượng hiện hành 34 tỉnh và 3.321 xã/phường cũng được [nguồn thống kê nhà nước công bố năm 2026](https://www.nso.gov.vn/default/2026/01/bo-tai-chinh-di-dau-trong-chuyen-doi-so-kien-tao-chinh-phu-so-va-kinh-te-so/). [Thông tin chính thức tháng 11/2026](https://baochinhphu.vn/thong-qua-de-an-dieu-chinh-dia-gioi-don-vi-hanh-chinh-giua-thanh-pho-ha-noi-va-tinh-phu-tho-102261107172713294.htm) nêu đề án điều chỉnh ranh giới Hà Nội–Phú Thọ đang được trình Quốc hội, dự kiến hiệu lực 01/01/2027; danh mục hiện tại chưa áp dụng thay đổi tương lai này và cần được rà soát trước thời điểm hiệu lực. Sau khi migration được áp dụng, seeder idempotent nạp danh mục nếu database trống; seeder dừng nếu dữ liệu hiện có thiếu hoặc khác mã/quan hệ nguồn, không ghi đè. API đọc danh mục: `GET /api/locations/provinces` và `GET /api/locations/provinces/{provinceCode}/wards`.

Form tạo/sửa tin và trang chi tiết tin của chủ sở hữu hỗ trợ tải tối đa 10 ảnh, 10 MB mỗi ảnh (JPEG, PNG, WebP), preview, chọn ảnh đại diện và xóa ảnh. Admin có cùng thao tác tại chi tiết kiểm duyệt. Ảnh lưu với tên ngẫu nhiên ngoài webroot; trang công khai chỉ phân phối ảnh của tin Approved. Thay đổi ảnh của tin Approved do chủ sở hữu thực hiện sẽ đưa tin về Pending để kiểm duyệt lại.

Để cấp quyền quản trị ban đầu, triển khai với hai secret cấu hình:

```text
Haven__AdminBootstrap__Email
Haven__AdminBootstrap__Password
```

Tính năng bootstrap sẽ được bật khi cả hai giá trị được cấu hình; không đặt mật khẩu quản trị trong file cấu hình được commit. Không cấu hình hai biến này nếu không có nhu cầu tạo/cấp quyền Admin.

### API xác thực

| Method | Route | Yêu cầu | Kết quả |
|---|---|---|---|
| `GET` | `/api/auth/antiforgery` | Public | Trả `requestToken` và đặt cookie antiforgery; gửi token ở header `RequestVerificationToken` cho mọi request thay đổi dữ liệu |
| `POST` | `/api/auth/register` | `displayName`, `email`, `password`, `acceptedTerms`, `consentedPersonalDataProcessing`, `termsVersion`, `privacyVersion` | `201 Created`, tạo session cookie, lưu hai bản ghi chấp thuận và trả tài khoản/roles |
| `POST` | `/api/auth/login` | `email`, `password`, tùy chọn `rememberMe` | `200 OK` hoặc `401 Unauthorized` |
| `POST` | `/api/auth/logout` | Đã đăng nhập | `204 No Content` |
| `POST` | `/api/auth/change-password` | Đã đăng nhập, `currentPassword`, `newPassword` | `204 No Content` |
| `GET` | `/api/auth/me` | Đã đăng nhập | Tài khoản hiện tại và roles |
| `PUT` | `/api/auth/me` | Đã đăng nhập, `displayName` | Cập nhật tên hiển thị công khai |

Cookie xác thực là `HttpOnly`, `SameSite=Lax`; production luôn đặt `Secure`. Password mới cần tối thiểu 12 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt. Hiện chưa tích hợp email sender nên tài khoản đăng ký chưa yêu cầu xác nhận email.

### API tin đăng

| Method | Route | Yêu cầu | Ghi chú |
|---|---|---|---|
| `GET` | `/api/listings` | Public | Tìm kiếm listing Approved, lọc/sort/phân trang |
| `GET` | `/api/listings/{id}` | Public | Chỉ trả listing `Approved`, tên loại và display name (không trả email/OwnerId) |
| `GET` | `/api/listings/compare?ids=1&ids=2` | Public | So sánh 1–3 listing Approved, giữ thứ tự ID yêu cầu |
| `GET` | `/api/property-types` | Public | Chỉ trả các loại bất động sản đang hoạt động |
| `POST` | `/api/my/listings` | Đã đăng nhập | Tạo Draft; Owner lấy từ principal |
| `GET` | `/api/my/listings?page=1&pageSize=20` | Đã đăng nhập | Danh sách phân trang, chỉ gồm listing của tài khoản hiện tại |
| `GET` | `/api/my/listings/{id}` | Đã đăng nhập | Chỉ chủ sở hữu xem được; người khác nhận `404` |
| `PUT` | `/api/my/listings/{id}` | Đã đăng nhập | Cập nhật theo state machine Domain |
| `POST` | `/api/my/listings/{id}/submit` | Đã đăng nhập | Gửi duyệt; trạng thái sai trả `409` |
| `DELETE` | `/api/my/listings/{id}` | Đã đăng nhập | Chủ tin xóa cứng Draft/Rejected; trạng thái khác trả `409` |
| `GET` | `/api/favorites` | Đã đăng nhập | Danh sách favorites phân trang, chỉ chứa listing Approved |
| `POST` | `/api/favorites/{listingId}` | Đã đăng nhập | Thêm favorite; duplicate trả `409` |
| `DELETE` | `/api/favorites/{listingId}` | Đã đăng nhập | Xóa favorite của chính user |
| `GET` | `/api/admin/moderation/pending` | Admin | Danh sách tin chờ duyệt, phân trang |
| `GET` | `/api/admin/listings` | Admin | Tất cả tin đăng, có thể lọc status và phân trang |
| `GET` | `/api/admin/listings/{id}` | Admin | Chi tiết tin đăng |
| `POST` | `/api/admin/moderation/{id}/approve` | Admin | Duyệt tin Pending |
| `POST` | `/api/admin/moderation/{id}/reject` | Admin | Từ chối tin Pending; bắt buộc có lý do |
| `POST` | `/api/admin/moderation/{id}/hide` | Admin | Ẩn tin Approved |
| `GET` | `/api/admin/property-types` | Admin | Danh sách tất cả loại bất động sản |
| `POST` | `/api/admin/property-types` | Admin | Tạo loại bất động sản |
| `PUT` | `/api/admin/property-types/{id}` | Admin | Cập nhật tên, thứ tự và trạng thái hoạt động |
| `POST` | `/api/consultations` | Đã đăng nhập | Tạo yêu cầu tư vấn; ListingId nếu có phải là Approved |
| `GET` | `/api/consultations` | Đã đăng nhập | Chỉ xem yêu cầu do tài khoản hiện tại tạo |
| `GET` | `/api/consultations/{id}` | Đã đăng nhập | Yêu cầu thuộc user khác trả `404` |
| `GET` | `/api/admin/consultations` | Admin | Xem/tìm lọc yêu cầu tư vấn |
| `GET` | `/api/admin/consultations/{id}` | Admin | Xem chi tiết yêu cầu tư vấn |
| `PUT` | `/api/admin/consultations/{id}/status` | Admin | Cập nhật `New`, `InProgress`, `Completed` hoặc `Cancelled` |
| `GET` | `/api/admin/dashboard` | Admin | Tổng listing, Pending/Approved và consultation New/InProgress |

Request tạo/cập nhật gồm `propertyTypeId`, `title`, `description`, `purpose`, `priceAmount`, `areaSqm`, `province`, `district`, `addressLine` và tùy chọn `bedrooms`/`bathrooms`. API không nhận `ownerId`. Owner chỉ được xóa cứng listing Draft/Rejected; listing khác không thể bị owner xóa. Admin có thể Hide tin Approved. Không có archive state.

Các route quản trị yêu cầu role `Admin`; việc ẩn action ở client không thay thế authorization phía server. Loại bất động sản được lưu trữ bằng trạng thái active/inactive để listing hiện có vẫn giữ quan hệ; không có thao tác xóa cứng.

Search nhận các query `keyword`, `propertyTypeId`, `purpose`, `province`, `district`, `minimumPrice`, `maximumPrice`, `minimumAreaSqm`, `maximumAreaSqm`, `bedrooms`, `sort`, `page` và `pageSize`. Sort hỗ trợ `Newest`, `Oldest`, `PriceAscending`, `PriceDescending`, `AreaAscending`, `AreaDescending`; `pageSize` tối đa 100. Search áp dụng lọc, sort và pagination ở EF Core/database và luôn loại listing chưa `Approved`. MVC search còn nhận `provinceCode`/`wardCode` và giữ mã khi chuyển trang. Tin cũ không có mã tiếp tục khớp tên địa điểm tương ứng; API tin đăng giữ hợp đồng chuỗi Province/District hiện có.

Consultation yêu cầu họ tên, email và số điện thoại; ListingId và lời nhắn là tùy chọn. Requester chỉ xem dữ liệu do mình tạo; Admin xem và cập nhật trạng thái. Owner tin không được xem consultation. Thông tin liên hệ không xuất hiện trong API công khai và không có thao tác xóa tự động hoặc thủ công trong API MVP.

Toàn bộ `POST`, `PUT`, `DELETE` MVC/API đều yêu cầu antiforgery token. MVC forms tự render hidden token; JSON client cùng origin gọi `/api/auth/antiforgery`, giữ cookie trả về và gửi giá trị `requestToken` trong header `RequestVerificationToken`. Các automated integration tests dùng EF Core InMemory; chúng không chứng nhận SQL Server.

### 6. Chạy ứng dụng web

```powershell
dotnet run --project src/Haven.Web/Haven.Web.csproj
```

Mở địa chỉ HTTP/HTTPS mà terminal hiển thị. URL cụ thể phụ thuộc cấu hình chạy local của máy bạn.

> Lưu ý: cấu hình `HavenDb` trong `appsettings.Development.json` mặc định dùng SQL Server Express local. Hãy override bằng User Secrets/biến môi trường nếu dùng kết nối khác. Sau đó áp dụng migration bằng lệnh ở bước 5; ứng dụng không tự động thay đổi schema khi khởi động.

## 🌿 Quy trình nhánh

- `main`: nhánh ổn định.
- `develop`: nhánh tích hợp công việc.
- `feature/<ten-tinh-nang>`: phát triển tính năng.
- `fix/<mo-ta-loi>`: sửa lỗi thông thường.
- `hotfix/<mo-ta>`: xử lý lỗi khẩn cấp theo thống nhất của nhóm.

Không phát triển tính năng trực tiếp trên `main` hoặc `develop`. Tạo nhánh công việc từ `develop`, sau đó mở Pull Request về `develop`. Chỉ đưa phiên bản đã được nhóm xác nhận từ `develop` sang `main` qua Pull Request.

Xem hướng dẫn đầy đủ tại [CONTRIBUTING.md](CONTRIBUTING.md).

## ✅ CI — kiểm tra tự động

Workflow tại `.github/workflows/ci.yml` thực hiện:

1. Checkout source.
2. Cài .NET SDK 10.
3. Restore dependencies.
4. Build solution ở cấu hình Release.
5. Chạy test.

Trước khi yêu cầu review, hãy chạy các lệnh restore/build/test ở máy local và kiểm tra CI của Pull Request. Do repository hiện tại chưa có branch protection được GitHub cưỡng chế trên gói đang sử dụng, nhóm cần tuân thủ quy trình Pull Request bằng thao tác và review thực tế.

## 🔐 An toàn thông tin

- Không commit `.env`, mật khẩu, token, API key hoặc connection string chứa thông tin xác thực.
- Không đưa dữ liệu cá nhân hay dữ liệu thật của khách hàng vào repository hoặc môi trường phát triển dùng chung.
- Nếu bí mật bị commit hoặc chia sẻ, hãy thu hồi/thay mới bí mật đó; chỉ xóa file khỏi commit sau không làm bí mật đã lộ trở nên an toàn.
- Không xóa Docker volume nếu chưa xác nhận dữ liệu có thể bị xóa.

## 🤝 Đóng góp

Đọc [CONTRIBUTING.md](CONTRIBUTING.md) trước khi tạo nhánh, commit hoặc Pull Request.

## 📄 Giấy phép

Chưa khai báo giấy phép mã nguồn mở. Không mặc định rằng repository được cấp phép sử dụng, sửa đổi hoặc phân phối lại cho công chúng.
