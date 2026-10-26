# HAVEN — Bộ tài liệu phân công công việc cho nhóm 5 người

## 1. Mục đích

Bộ tài liệu này chia công việc phát triển HAVEN thành 5 phạm vi trách nhiệm, giúp từng thành viên biết rõ:
- Mục tiêu và trách nhiệm chính.
- Các đầu việc cần thực hiện.
- Đầu vào, đầu ra và tiêu chí nghiệm thu.
- Quy tắc phối hợp, Git, Pull Request (PR) và kiểm thử.
- Những điều không được tự ý quyết định khi chưa thống nhất.

**Lưu ý:** Đây là kế hoạch phân công đề xuất dựa trên bối cảnh dự án HAVEN đã được mô tả trong cuộc trao đổi, không phải kết quả kiểm toán đầy đủ mã nguồn hiện tại. Trước khi triển khai, nhóm phải đối chiếu từng đầu việc với repository thực tế để tránh làm trùng hoặc giả định chức năng đã tồn tại.

## 2. Phân công tổng quan

| Thành viên | Vai trò | Phạm vi sở hữu chính | Kết quả quan trọng |
|---|---|---|---|
| Người 1 | Tech Lead / Backend Core | Phân tích yêu cầu, kiến trúc, Domain, hợp đồng Application, review tích hợp | Quyết định kiến trúc được ghi lại, hợp đồng dùng chung, PR được review |
| Người 2 | Backend Property | Nghiệp vụ bất động sản và luồng tạo/cập nhật/chi tiết/trạng thái tin đăng nếu nằm trong MVP | Use case và kiểm thử nghiệp vụ property |
| Người 3 | Backend Search | Tìm kiếm, lọc, sắp xếp, phân trang và tối ưu truy vấn | Use case tìm kiếm và bộ kiểm thử truy vấn |
| Người 4 | Frontend MVC | Razor Views, layout, trang chủ, danh sách, chi tiết và trải nghiệm responsive | Giao diện chạy được, trạng thái lỗi/rỗng/loading phù hợp |
| Người 5 | Database / QA / Platform | EF Core mapping, migration, test tích hợp, CI, cấu hình môi trường và tài liệu chạy | Cơ sở dữ liệu có thể tái tạo, kiểm thử và CI đáng tin cậy |

Vai trò không đồng nghĩa với việc một người được phép thay đổi tùy ý toàn bộ hệ thống. Những phần dùng chung phải được phối hợp và review.

## 3. Trình tự triển khai đề xuất

### Giai đoạn 0 — Xác nhận hiện trạng
1. Đọc `README.md`, `docs/ARCHITECTURE.md`, `docs/WORKFLOW.md` và các project trong solution.
2. Chạy `git status`, xác nhận branch và commit.
3. Chạy build/test hiện có, ghi lại kết quả thực tế.
4. Đánh dấu từng chức năng: **Đã có và xác minh**, **Có một phần**, **Chưa có**, **Chưa xác minh**.
5. Chỉ tạo task mới sau khi đã đối chiếu với code hiện hữu.

### Giai đoạn 1 — Chốt phạm vi MVP
Nhóm cần quyết định bằng văn bản:
- MVP chỉ tìm kiếm/xem tin hay cho phép tạo và quản lý tin đăng?
- Có cần đăng nhập/phân quyền trong MVP không?
- Các trường bắt buộc của bất động sản là gì?
- Bộ lọc và tiêu chí sắp xếp nào thực sự cần cho bản đầu?
- Cơ sở dữ liệu và cách chạy local được chốt ra sao?

Không tự suy diễn các quyết định còn bỏ ngỏ. Nếu chưa thống nhất, ghi thành câu hỏi chặn (blocking question) và giao Tech Lead điều phối.

### Giai đoạn 2 — Thiết lập nền tảng
- Chốt entity, DTO/contract và quy tắc validation.
- Chốt DbContext, mapping, migration và dữ liệu kiểm thử.
- Chốt route/controller/action và ViewModel cần cho giao diện.
- Chốt quy ước lỗi, logging và cách kiểm thử.
- Mọi thay đổi hợp đồng phải được thông báo cho các thành viên liên quan trước khi merge.

### Giai đoạn 3 — Phát triển song song
- Người 2 xây nghiệp vụ property.
- Người 3 xây truy vấn search dựa trên contract đã thống nhất.
- Người 4 xây giao diện dựa trên ViewModel/route đã thống nhất; có thể dùng dữ liệu giả tạm thời nhưng phải đánh dấu rõ.
- Người 5 xây persistence, migration và test hạ tầng theo các entity đã chốt.
- Người 1 điều phối contract, review và giải quyết xung đột kiến trúc.

### Giai đoạn 4 — Tích hợp và nghiệm thu
1. Tích hợp trên branch `develop`.
2. Chạy build và toàn bộ test liên quan.
3. Kiểm tra migration trên database phát triển sạch hoặc môi trường kiểm thử được phép.
4. Kiểm tra các luồng người dùng theo acceptance criteria.
5. Cập nhật README và tài liệu vận hành.
6. Chỉ đánh dấu hoàn tất khi có bằng chứng kiểm thử.

## 4. Quy tắc Git và Pull Request

### Nhánh
- `main`: phiên bản ổn định, không commit trực tiếp.
- `develop`: nhánh tích hợp của nhóm.
- `feature/HAV-xx-ten-ngan`: nhánh cho một task.
- `fix/HAV-xx-ten-ngan`: sửa lỗi gắn với task/issue.

Tên nhánh là quy ước đề xuất; nếu repository đang dùng quy ước khác thì thống nhất theo hiện trạng thay vì tự ý đổi.

### Quy trình
1. Đồng bộ `develop`.
2. Tạo branch cho task.
3. Thực hiện một phạm vi thay đổi nhỏ, có thể review.
4. Chạy test phù hợp và ghi kết quả.
5. Tạo PR về `develop`, mô tả mục đích, phạm vi, cách kiểm tra và rủi ro.
6. Chờ ít nhất một người khác review; thay đổi kiến trúc, database hoặc security cần Tech Lead review.
7. Sửa theo review, chờ CI xanh rồi mới merge theo chính sách nhóm.

### Không làm
- Không commit secret, connection string thật, token hoặc dữ liệu cá nhân.
- Không sửa file ngoài phạm vi task nếu chưa trao đổi.
- Không tự tạo migration cạnh tranh hoặc đổi schema mà không báo Người 5 và Người 1.
- Không thay đổi public contract mà không thông báo Người 2, Người 3 và Người 4 nếu có liên quan.
- Không tuyên bố test/build thành công nếu chưa chạy hoặc chưa có log xác minh.
- Không đưa refactor lớn không liên quan vào PR chức năng.

## 5. Definition of Ready (DoR)

Một task chỉ nên bắt đầu khi:
- Mục tiêu và hành vi mong muốn được mô tả.
- Phạm vi file/module dự kiến đã rõ ở mức hợp lý.
- Đầu vào/đầu ra và tiêu chí nghiệm thu xác định được.
- Các phụ thuộc hoặc câu hỏi chặn đã được giải quyết.
- Có cách kiểm tra kết quả.

## 6. Definition of Done (DoD)

Task được coi là hoàn tất khi:
- Code phù hợp kiến trúc hiện hành.
- Validation và xử lý lỗi cần thiết đã được triển khai.
- Có unit test hoặc integration test phù hợp; nếu không thể viết test, phải nêu lý do.
- Build/test liên quan đã chạy và kết quả được ghi lại.
- Không đưa secret vào repository.
- Tài liệu/API/ViewModel/schema liên quan được cập nhật.
- PR đã được review và CI đạt yêu cầu.
- Không còn TODO quan trọng bị che giấu; việc chưa làm phải được ghi thành task riêng.

## 7. Nhịp làm việc nhóm

- Daily ngắn: hôm qua đã làm gì, hôm nay làm gì, đang bị chặn ở đâu.
- Hai lần mỗi tuần hoặc theo nhu cầu: review contract/kiến trúc.
- Trước khi merge: kiểm tra PR, test và ảnh hưởng đến các thành viên khác.
- Cuối mỗi giai đoạn: demo luồng chạy thực tế, không chỉ báo cáo số lượng commit.

Mẫu cập nhật task:
- **Đã làm:**
- **Đang làm:**
- **Bị chặn bởi:**
- **File/module ảnh hưởng:**
- **Cách kiểm thử và kết quả:**
- **Cần ai review/quyết định:**

## 8. Các task điều phối gợi ý

| ID | Task | Owner chính | Người phối hợp | Điều kiện hoàn tất |
|---|---|---|---|---|
| HAV-01 | Kiểm kê hiện trạng repository | Người 1 | Cả nhóm | Bảng hiện trạng có bằng chứng |
| HAV-02 | Chốt phạm vi MVP và user flow | Người 1 | Cả nhóm | Quyết định được ghi trong tài liệu |
| HAV-03 | Chốt Domain và contract | Người 1 | Người 2, 3, 5 | Contract được review |
| HAV-04 | Chốt schema/persistence | Người 5 | Người 1, 2, 3 | Mapping/migration được kiểm tra |
| HAV-05 | Use case property | Người 2 | Người 1, 5 | Test nghiệp vụ đạt |
| HAV-06 | Use case search | Người 3 | Người 1, 5 | Bộ lọc/phân trang được test |
| HAV-07 | Giao diện MVC | Người 4 | Người 1, 2, 3 | Luồng UI gọi đúng contract |
| HAV-08 | Test tích hợp và CI | Người 5 | Cả nhóm | CI xanh, hướng dẫn tái tạo được |
| HAV-09 | Tích hợp MVP | Người 1 | Cả nhóm | Acceptance checklist đạt |
| HAV-10 | Cập nhật tài liệu bàn giao | Người 5 | Cả nhóm | Người mới có thể chạy theo hướng dẫn |

## 9. Cách dùng bộ file

- Mỗi thành viên đọc `README_HUONG_DAN_CHUNG.md` và file riêng của mình.
- Người 1 quản lý quyết định và phụ thuộc giữa các task.
- Mỗi task phải được đưa vào issue/board của nhóm; file Markdown là tài liệu giao việc, không thay thế trạng thái issue thực tế.
- Khi phát hiện code hiện tại khác với kế hoạch, ghi lại bằng chứng và điều chỉnh task trước khi sửa code.
