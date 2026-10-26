# Người 5 — Database / QA / Platform

## 1. Sứ mệnh của vai trò

Đảm bảo HAVEN có thể được khởi tạo, lưu trữ dữ liệu, kiểm thử và tích hợp một cách tái lập. Người 5 chịu trách nhiệm chính về cấu hình persistence/EF Core, migration, test tích hợp, CI và tài liệu chạy ứng dụng.

Phải kiểm tra code thực tế trước khi kết luận `DbContext`, migration, SQL Server, Docker hay CI đã sẵn sàng. Tài liệu này không khẳng định các thành phần đó đã hoàn thiện.

## 2. Phạm vi sở hữu

- Khảo sát DbContext, entity configuration và provider hiện có.
- Mapping, khóa, constraint và index dựa trên yêu cầu đã chốt.
- Migration có thể tái tạo và an toàn trong môi trường phát triển/kiểm thử.
- Cấu hình môi trường qua biến môi trường/secrets; không commit secret.
- Test tích hợp persistence/use case.
- CI build/test và độ ổn định của test.
- Hướng dẫn chạy local, kiểm thử và xử lý lỗi phổ biến.
- Phối hợp các thành viên khi có thay đổi schema hoặc yêu cầu test.

## 3. Kế hoạch công việc chi tiết

### A. Kiểm tra nền tảng hiện có
- [ ] Kiểm tra package EF Core và database provider.
- [ ] Xác định DbContext, entity configuration và dependency injection.
- [ ] Kiểm tra thư mục migration và lịch sử migration.
- [ ] Kiểm tra cách cấu hình connection string.
- [ ] Kiểm tra `.env.example` và tài liệu thiết lập; không mở hoặc ghi secret thật vào báo cáo.
- [ ] Kiểm tra CI workflow, lệnh build/test và target framework.
- [ ] Chạy test hiện có và ghi rõ môi trường.
- [ ] Xác nhận database thực tế mà dự án hỗ trợ; không tự đổi provider.

**Đầu ra:** baseline database/CI, danh sách thiếu sót và đề xuất theo mức độ ưu tiên.

### B. Schema và EF Core
- [ ] Thu thập entity và quy tắc từ Người 1/2/3 trước khi sửa schema.
- [ ] Xác nhận kiểu dữ liệu, nullable, độ dài và precision.
- [ ] Thiết lập khóa và quan hệ theo Domain model đã thống nhất.
- [ ] Thêm constraint giúp bảo vệ tính toàn vẹn dữ liệu khi phù hợp.
- [ ] Trao đổi Người 3 về index phục vụ query thực tế.
- [ ] Tạo migration sau khi schema được review.
- [ ] Kiểm tra migration trên database phát triển/kiểm thử sạch được phép sử dụng.
- [ ] Xác minh cập nhật schema và khởi tạo lại môi trường có thể tái lập.
- [ ] Không sửa migration đã được dùng chung một cách âm thầm; nếu cần chỉnh, thông báo cả nhóm.

### C. Cấu hình môi trường và bảo mật
- [ ] Cấu hình đọc connection string từ nguồn phù hợp của ứng dụng.
- [ ] Cập nhật `.env.example` chỉ bằng tên biến và giá trị placeholder.
- [ ] Kiểm tra `.gitignore` loại trừ file secret/local.
- [ ] Không commit password, token, certificate private key hoặc dữ liệu thật.
- [ ] Ghi rõ dependency cần cài và lệnh khởi động.
- [ ] Tách cấu hình Development/Test/Production theo convention hiện có.
- [ ] Không chạy migration tự động trong môi trường production nếu chưa có quyết định vận hành rõ ràng.

### D. Integration test và test data
- [ ] Xác định test nào là unit test và test nào thực sự cần database.
- [ ] Chọn chiến lược test database phù hợp với provider thực tế.
- [ ] Dữ liệu test phải được tạo rõ ràng, độc lập và dọn dẹp an toàn.
- [ ] Test truy vấn persistence, ràng buộc và các use case quan trọng.
- [ ] Kiểm tra test không phụ thuộc thứ tự chạy.
- [ ] Không dùng database production hoặc dữ liệu người dùng thật.
- [ ] Ghi rõ các test bị skip và lý do; không coi test bị skip là đã xác minh.

### E. CI và chất lượng
- [ ] Xác nhận CI dùng SDK/target framework tương thích.
- [ ] Chạy restore, build và test trong pipeline.
- [ ] Kiểm tra path/case sensitivity để giảm lỗi giữa Windows và Linux.
- [ ] Không làm CI phụ thuộc secret cá nhân nếu có thể tránh.
- [ ] Giữ log đủ để chẩn đoán nhưng không lộ secret.
- [ ] Xác minh CI xanh trên đúng commit/PR, không dựa vào lần chạy cũ.
- [ ] Nếu thêm dịch vụ database vào CI, tài liệu hóa cách khởi động và dọn dẹp.
- [ ] Theo dõi flaky test và tạo task xử lý riêng.

### F. Tài liệu vận hành
- [ ] Cập nhật README với yêu cầu cài đặt và lệnh chạy.
- [ ] Mô tả cách cấu hình connection string bằng placeholder.
- [ ] Mô tả cách áp dụng migration theo môi trường được hỗ trợ.
- [ ] Mô tả lệnh chạy unit/integration test.
- [ ] Ghi lại lỗi phổ biến và cách xác minh nguyên nhân.
- [ ] Phân biệt rõ bước bắt buộc và bước tùy chọn.
- [ ] Đảm bảo người mới có thể làm theo trên máy sạch theo điều kiện đã nêu.

## 4. Đầu ra bắt buộc

1. Báo cáo hiện trạng database/CI.
2. Mapping/migration đã được review nếu cần thay đổi.
3. Test tích hợp phù hợp với rủi ro nghiệp vụ.
4. CI build/test có kết quả xác minh.
5. Hướng dẫn thiết lập môi trường và chạy test.
6. Ghi chú về giới hạn, giả định hoặc bước thủ công còn tồn tại.

## 5. Tiêu chí nghiệm thu

- Schema phản ánh entity và rule đã chốt.
- Migration có thể áp dụng trên môi trường kiểm thử sạch theo hướng dẫn.
- Không có secret được commit.
- Test persistence có thể tái lập và không dựa vào dữ liệu cá nhân.
- CI chạy đúng build/test cần thiết và kết quả gắn với commit hiện tại.
- Hướng dẫn setup đủ để thành viên khác làm theo.
- Index được đề xuất theo query/đo đạc, không thêm tùy tiện.
- Các thay đổi schema được thông báo cho Người 1, 2 và 3.

## 6. Phối hợp

| Người | Cần trao đổi |
|---|---|
| Người 1 | Domain model, quyết định kiến trúc, phạm vi database và acceptance |
| Người 2 | Field bắt buộc, constraint, quy tắc trạng thái và test nghiệp vụ |
| Người 3 | Query/filter/sort, index và dữ liệu test tìm kiếm |
| Người 4 | Hướng dẫn cấu hình, dữ liệu demo/test và cách chạy ứng dụng |

## 7. Rủi ro cần kiểm soát

- Migration không đồng bộ với code.
- Thay đổi schema không thông báo khiến nhánh thành viên khác bị lỗi.
- Dùng provider khác trong test làm kết quả khác database thật.
- Connection string/secret bị commit.
- CI xanh nhưng không chạy integration test cần thiết.
- Migration chỉ chạy trên database cũ của một máy.
- Dùng test data thật hoặc test không dọn dẹp.
- Tuyên bố đã hỗ trợ Docker/database provider khi chưa chạy thử.

## 8. Checklist trước PR

- [ ] Đã khảo sát cấu hình và migration hiện tại.
- [ ] Schema change có lý do và đã được review.
- [ ] Migration được tạo/kiểm tra đúng quy trình.
- [ ] Test chạy trên provider/chiến lược đã thống nhất.
- [ ] Không có secret.
- [ ] CI được xác minh trên commit hiện tại.
- [ ] Tài liệu setup/test được cập nhật.
- [ ] Các thành viên bị ảnh hưởng đã được thông báo.
