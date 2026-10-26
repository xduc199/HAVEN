# Người 2 — Backend Property / Nghiệp vụ bất động sản

## 1. Sứ mệnh của vai trò

Triển khai các use case nghiệp vụ xoay quanh dữ liệu bất động sản/tin đăng trong phạm vi MVP đã được nhóm chốt. Tập trung vào tính đúng đắn của quy tắc nghiệp vụ, validation, kết quả trả về và khả năng kiểm thử.

Phạm vi chính xác phải dựa trên code hiện có và quyết định MVP. Nếu MVP chỉ có tìm kiếm/xem tin, không tự mở rộng sang tạo/sửa/xóa tin khi chưa được duyệt.

## 2. Phạm vi sở hữu

- Use case đọc chi tiết bất động sản.
- Use case tạo/cập nhật tin đăng nếu được đưa vào MVP.
- Kiểm tra dữ liệu đầu vào và quy tắc nghiệp vụ.
- Xử lý trường hợp không tìm thấy bản ghi hoặc dữ liệu không hợp lệ.
- Unit test cho logic do Người 2 triển khai.
- Phối hợp với Người 5 về persistence và Người 4 về dữ liệu hiển thị.

## 3. Kế hoạch công việc

### A. Khảo sát code trước khi sửa
- [ ] Tìm entity/model property hiện có.
- [ ] Tìm controller, service/use case, repository hoặc abstraction đang sử dụng.
- [ ] Xác định luồng dữ liệu từ HTTP request đến persistence.
- [ ] Kiểm tra validation, DTO, ViewModel và test hiện có.
- [ ] Ghi rõ phần đã có, thiếu hoặc chưa xác minh.
- [ ] Trao đổi Người 1 trước khi thêm entity hoặc đổi contract dùng chung.

### B. Làm rõ quy tắc nghiệp vụ
Cùng Tech Lead xác nhận các câu hỏi sau; không tự điền giá trị nếu chưa có quyết định:
- [ ] Những trường nào bắt buộc?
- [ ] Giá được lưu và hiển thị theo quy ước nào? Có đơn vị/tiền tệ không?
- [ ] Diện tích, địa chỉ, loại hình và mục đích giao dịch có quy tắc gì?
- [ ] Những giá trị nào phải giới hạn hoặc từ chối?
- [ ] Tin đăng có trạng thái không? Ai được chuyển trạng thái?
- [ ] Có cần kiểm tra quyền sở hữu tin đăng không?
- [ ] Xóa là xóa vật lý hay đổi trạng thái? Nếu chưa chốt, không tự triển khai.
- [ ] Có yêu cầu ảnh đại diện/danh sách ảnh không? Nếu ngoài MVP, ghi rõ.

**Đầu ra:** danh sách rule có thể kiểm thử, thống nhất với Người 1.

### C. Triển khai use case
- [ ] Tạo hoặc hoàn thiện request/response theo contract đã thống nhất.
- [ ] Tách validation đầu vào khỏi các quy tắc nghiệp vụ khi phù hợp kiến trúc.
- [ ] Triển khai đọc chi tiết, bao gồm hành vi khi không tìm thấy.
- [ ] Triển khai tạo/cập nhật/trạng thái chỉ khi có trong MVP.
- [ ] Dùng abstraction persistence hiện có; không tạo thêm tầng chỉ để “đúng mẫu”.
- [ ] Xử lý async/cancellation theo quy ước dự án.
- [ ] Không để entity persistence lọt ra ngoài nếu kiến trúc dùng DTO.
- [ ] Trả lỗi nhất quán theo convention hiện hành.
- [ ] Không đưa thông tin nội bộ hoặc exception chi tiết nhạy cảm ra response.

### D. Kiểm thử
- [ ] Dữ liệu hợp lệ được chấp nhận.
- [ ] Thiếu trường bắt buộc bị từ chối đúng cách.
- [ ] Giá trị biên và dữ liệu sai định dạng được kiểm tra.
- [ ] Bản ghi không tồn tại trả về hành vi đã thống nhất.
- [ ] Trạng thái không hợp lệ không làm thay đổi dữ liệu.
- [ ] Kiểm tra quy tắc quyền nếu tính năng yêu cầu xác thực/phân quyền.
- [ ] Test không phụ thuộc thứ tự chạy hoặc dữ liệu máy cá nhân.
- [ ] Có integration test nếu cần xác minh luồng persistence thực.

### E. Tích hợp với giao diện và database
- [ ] Cung cấp ví dụ request/response cho Người 4.
- [ ] Thông báo các field nullable/required và format.
- [ ] Phối hợp Người 5 về mapping, ràng buộc và migration.
- [ ] Báo ngay khi contract thay đổi.
- [ ] Kiểm tra trang chi tiết xử lý not found và lỗi backend.

## 4. Đầu ra bắt buộc

1. Use case property theo phạm vi MVP.
2. Validation và cách xử lý lỗi rõ ràng.
3. Unit test cho quy tắc nghiệp vụ.
4. Tài liệu ngắn về contract hoặc ví dụ payload nếu giao diện/API cần.
5. PR nhỏ, có task ID, kết quả test và ghi chú migration/contract nếu liên quan.

## 5. Tiêu chí nghiệm thu

- Hành vi phù hợp rule đã được Tech Lead xác nhận.
- Dữ liệu không hợp lệ không âm thầm được lưu.
- Trường hợp not found và lỗi validation có hành vi nhất quán.
- Không có logic nghiệp vụ cốt lõi bị đặt tùy tiện trong Controller/View.
- Test bao phủ happy path và các edge case quan trọng.
- Không làm hỏng contract của Search hoặc giao diện.
- Build/test liên quan chạy được và có bằng chứng.
- Tài liệu được cập nhật nếu hành vi công khai thay đổi.

## 6. Phối hợp và phụ thuộc

| Người | Cần trao đổi |
|---|---|
| Người 1 | Domain, quy tắc nghiệp vụ, contract, phạm vi MVP |
| Người 3 | Các field/enum được dùng làm bộ lọc và ngữ nghĩa trạng thái |
| Người 4 | Field cần hiển thị, validation message, hành vi trang chi tiết |
| Người 5 | Schema, khóa, index, constraint, migration và test database |

## 7. Không thực hiện khi chưa được duyệt

- Không tự thêm chức năng đăng nhập, quản lý ảnh hoặc quy trình duyệt tin nếu ngoài MVP.
- Không đổi tên field public hoặc enum dùng chung mà không thông báo.
- Không tự sửa migration do Người 5 quản lý.
- Không bỏ validation chỉ để giao diện “chạy được”.
- Không dùng dữ liệu giả như thể đó là dữ liệu production.
- Không xử lý lỗi bằng cách nuốt exception hoặc trả thành công giả.

## 8. Checklist trước khi tạo PR

- [ ] Mô tả rõ use case.
- [ ] Đã đọc code hiện tại và tránh tạo logic trùng.
- [ ] Validation có test.
- [ ] Các nhánh lỗi quan trọng có test.
- [ ] Contract/field được thông báo cho Người 3/4/5 khi liên quan.
- [ ] Build/test đã chạy.
- [ ] Không chứa secret hoặc dữ liệu thật nhạy cảm.
- [ ] PR có hướng dẫn kiểm tra thủ công nếu test tự động chưa bao phủ UI/API.
