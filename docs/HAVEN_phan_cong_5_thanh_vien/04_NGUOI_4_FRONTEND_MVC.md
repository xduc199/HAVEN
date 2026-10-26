# Người 4 — Frontend MVC / Razor Views và trải nghiệm người dùng

## 1. Sứ mệnh của vai trò

Xây dựng giao diện MVC rõ ràng, nhất quán, responsive và kết nối đúng với controller/use case của HAVEN. Giao diện phải thể hiện chính xác trạng thái dữ liệu, lỗi validation, kết quả rỗng và lỗi hệ thống; không chỉ đẹp ở trạng thái dữ liệu mẫu.

Phạm vi trang cụ thể phụ thuộc MVP và cấu trúc Web hiện tại. Trước khi tạo View hoặc Controller mới, kiểm tra xem trang/luồng đó đã tồn tại chưa.

## 2. Phạm vi sở hữu

- Layout dùng chung, navigation và các thành phần tái sử dụng phù hợp.
- Trang chủ nếu nằm trong MVP.
- Trang danh sách/tìm kiếm bất động sản.
- Trang chi tiết bất động sản.
- Form tạo/sửa tin nếu MVP có chức năng này.
- Responsive layout và khả năng sử dụng cơ bản.
- ViewModel, validation display và thông báo lỗi ở lớp Web.
- Kiểm tra luồng giao diện với backend đã tích hợp.

## 3. Kế hoạch công việc chi tiết

### A. Khảo sát trước khi code
- [ ] Đọc cấu trúc `Haven.Web`, routes, controllers, Views và static assets.
- [ ] Xác định layout, CSS framework/thư viện hiện có.
- [ ] Kiểm tra convention ViewModel và cách truyền dữ liệu hiện tại.
- [ ] Xác định trang nào đã có và mức độ hoàn thiện.
- [ ] Kiểm tra các lỗi build/route hiện hữu.
- [ ] Không thay framework hoặc viết lại CSS toàn bộ nếu chưa có lý do và phê duyệt.

### B. Chốt luồng và wireframe
Cùng Người 1 thống nhất:
- [ ] Trang/luồng thuộc MVP.
- [ ] Thành phần hiển thị trong card bất động sản.
- [ ] Thông tin bắt buộc trên trang chi tiết.
- [ ] Bộ lọc, sort và pagination có trên giao diện.
- [ ] Hành vi khi chưa có kết quả.
- [ ] Hành vi khi backend lỗi hoặc bản ghi không tồn tại.
- [ ] Có form tạo/sửa tin hay không.
- [ ] Hỗ trợ màn hình điện thoại, tablet và desktop ở mức nào.

### C. Layout và thành phần chung
- [ ] Hoàn thiện layout hiện có thay vì tạo layout trùng.
- [ ] Navigation có trạng thái và đường dẫn hợp lý.
- [ ] Có vùng hiển thị validation/error/success nhất quán.
- [ ] Các nút và liên kết có tên dễ hiểu, trạng thái focus nhìn thấy được.
- [ ] Kiểm tra label cho input và cấu trúc heading.
- [ ] Tránh đưa logic nghiệp vụ vào Razor View.
- [ ] Tách CSS/JS theo quy ước dự án; không nhúng script tùy tiện nếu codebase có cấu trúc assets.

### D. Trang tìm kiếm/danh sách
- [ ] Tạo hoặc hoàn thiện form filter theo contract đã chốt.
- [ ] Giữ giá trị filter sau khi submit nếu cần.
- [ ] Hiển thị loading chỉ khi kiến trúc UI thực sự có trạng thái bất đồng bộ.
- [ ] Hiển thị rõ trường hợp không có kết quả.
- [ ] Hiển thị thông tin phân trang và giữ filter khi chuyển trang.
- [ ] Không tạo tên query parameter khác với backend contract.
- [ ] Encode nội dung hiển thị theo cơ chế an toàn mặc định của Razor.
- [ ] Không hiển thị field thiếu dữ liệu bằng cách gây vỡ layout.

### E. Trang chi tiết
- [ ] Hiển thị các trường được thống nhất trong MVP.
- [ ] Xử lý ID không hợp lệ/not found theo convention của dự án.
- [ ] Có đường quay lại danh sách hoặc tiếp tục tìm kiếm.
- [ ] Không giả định ảnh hoặc dữ liệu luôn tồn tại.
- [ ] Không hard-code nội dung giả vào luồng production.
- [ ] Kiểm tra layout trên màn hình hẹp.

### F. Form tạo/sửa (chỉ nếu nằm trong MVP)
- [ ] Dùng ViewModel đầu vào phù hợp, không bind trực tiếp entity persistence khi không phù hợp.
- [ ] Hiển thị validation phía server; validation phía client chỉ bổ trợ.
- [ ] Giữ lại giá trị hợp lệ khi submit lỗi.
- [ ] Hiển thị lỗi theo field và lỗi tổng quát.
- [ ] Chống submit lặp nếu có yêu cầu.
- [ ] Kiểm tra quyền truy cập nếu luồng có xác thực/phân quyền.
- [ ] Không cho rằng ẩn nút trên UI là đủ để bảo vệ endpoint.

### G. Kiểm thử giao diện
- [ ] Trang render khi có dữ liệu.
- [ ] Trang render khi danh sách rỗng.
- [ ] Trang xử lý not found và lỗi backend.
- [ ] Filter/sort/pagination giữ đúng giá trị.
- [ ] Form lỗi và thành công hiển thị đúng nếu có form.
- [ ] Kiểm tra desktop và mobile ở kích thước đại diện.
- [ ] Kiểm tra route/link không trỏ tới action không tồn tại.
- [ ] Kiểm tra accessibility cơ bản: label, keyboard focus, heading và contrast đủ đọc.

## 4. Phối hợp

| Người | Cần trao đổi |
|---|---|
| Người 1 | Luồng MVP, routes, ViewModel và tiêu chí nghiệm thu |
| Người 2 | Field chi tiết, validation message, not found và form nghiệp vụ |
| Người 3 | Search query, filter, sort, pagination và response metadata |
| Người 5 | Cách chạy ứng dụng, cấu hình môi trường và dữ liệu test |

## 5. Đầu ra bắt buộc

1. Các View/Controller/ViewModel cần thiết theo phạm vi MVP.
2. Giao diện responsive theo convention hiện có.
3. Trạng thái loading/rỗng/lỗi/validation phù hợp với từng luồng.
4. Hướng dẫn test thủ công hoặc ảnh minh chứng trong PR khi hữu ích.
5. PR nhỏ, mô tả các trang và luồng đã kiểm tra.

## 6. Tiêu chí nghiệm thu

- Giao diện dùng đúng route và contract backend.
- Không có dữ liệu giả bị nhầm là dữ liệu thật.
- Các trạng thái rỗng, lỗi và validation không làm người dùng bối rối.
- Form giữ dữ liệu và hiển thị lỗi phù hợp khi submit thất bại.
- Giao diện không vỡ ở màn hình hẹp phổ biến.
- Không đưa logic nghiệp vụ quan trọng vào View.
- Các nội dung người dùng nhập được hiển thị an toàn.
- Các luồng chính đã được kiểm tra thủ công hoặc tự động và ghi lại kết quả.

## 7. Ranh giới cần lưu ý

- Không tự đổi contract API/query parameter.
- Không thêm thư viện lớn khi chưa trao đổi Người 1.
- Không coi dữ liệu mock là hoàn tất tích hợp.
- Không dựa vào validation client-side làm lớp bảo vệ duy nhất.
- Không thêm luồng tạo/sửa/xóa nếu chưa được xác nhận là thuộc MVP.
- Không sửa schema hoặc migration.

## 8. Checklist trước PR

- [ ] Đã kiểm tra cấu trúc View/Controller hiện tại.
- [ ] ViewModel và route khớp contract.
- [ ] Trạng thái dữ liệu rỗng/lỗi/not found đã xử lý.
- [ ] Form có validation server-side nếu có.
- [ ] Đã kiểm tra mobile/desktop.
- [ ] Không có nội dung giả hoặc link chết.
- [ ] Không có script/style không cần thiết.
- [ ] Có hướng dẫn kiểm tra thủ công và kết quả.
