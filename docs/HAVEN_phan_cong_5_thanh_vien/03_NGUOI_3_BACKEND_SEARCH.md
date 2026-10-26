# Người 3 — Backend Search / Tìm kiếm và lọc bất động sản

## 1. Sứ mệnh của vai trò

Xây dựng truy vấn tìm kiếm bất động sản đáp ứng các tiêu chí đã thống nhất: lọc, sắp xếp, phân trang và trả về dữ liệu nhất quán cho giao diện. Ưu tiên đúng kết quả và khả năng kiểm thử trước khi tối ưu hiệu năng dựa trên số liệu.

Không tự thêm bộ lọc mới chỉ vì dễ triển khai; mọi tiêu chí phải xuất phát từ phạm vi MVP và dữ liệu thực tế.

## 2. Phạm vi sở hữu

- Search criteria/query model.
- Kết hợp các bộ lọc được duyệt.
- Sắp xếp theo các tùy chọn được duyệt và có thứ tự ổn định.
- Phân trang, tổng số kết quả nếu giao diện cần.
- Xử lý query rỗng, bộ lọc không hợp lệ và trang vượt giới hạn.
- Test logic search và integration test truy vấn khi cần.
- Phối hợp Người 5 về index/schema và Người 4 về UX tìm kiếm.

## 3. Kế hoạch công việc chi tiết

### A. Khảo sát hiện trạng
- [ ] Tìm controller/action hoặc endpoint search hiện có.
- [ ] Tìm service/query handler/repository tương ứng.
- [ ] Kiểm tra entity và field có thể dùng để tìm kiếm.
- [ ] Kiểm tra dữ liệu mẫu, index và migration hiện có.
- [ ] Kiểm tra các test hiện có và cách khởi tạo database test.
- [ ] Ghi lại phần đã hoạt động, thiếu hoặc chưa xác minh trước khi sửa.

### B. Chốt yêu cầu tìm kiếm
Làm việc với Người 1 và Người 4 để xác nhận:
- [ ] Từ khóa tìm kiếm áp dụng cho trường nào?
- [ ] Bộ lọc loại bất động sản/mục đích giao dịch nào nằm trong MVP?
- [ ] Khoảng giá có cho phép chỉ nhập cận dưới hoặc cận trên không?
- [ ] Khoảng diện tích được xử lý ra sao?
- [ ] Vị trí là chuỗi địa chỉ, khu vực phân cấp hay cơ chế khác?
- [ ] Có bộ lọc trạng thái không? Trạng thái nào được hiển thị công khai?
- [ ] Các tùy chọn sort và giá trị mặc định là gì?
- [ ] Kích thước trang tối đa là bao nhiêu?
- [ ] Tổng số kết quả có cần trả về không?

Không đoán schema địa lý hoặc yêu cầu tìm kiếm toàn văn nếu chưa được xác nhận.

### C. Thiết kế contract
- [ ] Đề xuất query/filter model rõ ràng, có kiểu dữ liệu phù hợp.
- [ ] Chốt cách xử lý giá trị null/empty.
- [ ] Chốt validation cho page/pageSize và khoảng min/max.
- [ ] Chốt sort key qua danh sách cho phép; không nối trực tiếp chuỗi sort tùy ý vào SQL.
- [ ] Chốt cấu trúc response: items, pagination metadata và total nếu cần.
- [ ] Gửi contract cho Người 1 và Người 4 review trước khi triển khai giao diện phụ thuộc.

### D. Triển khai truy vấn
- [ ] Áp dụng filter theo điều kiện chỉ khi người dùng cung cấp.
- [ ] Bảo đảm nhiều filter kết hợp bằng logic AND/OR đúng như yêu cầu.
- [ ] Xác định hành vi khi min > max hoặc pageSize ngoài giới hạn.
- [ ] Áp dụng sort từ danh sách được cho phép.
- [ ] Bổ sung secondary sort ổn định khi cần để phân trang không nhảy tùy tiện.
- [ ] Phân trang ở database thay vì tải toàn bộ dữ liệu về bộ nhớ.
- [ ] Chỉ chọn field cần thiết nếu phù hợp với contract.
- [ ] Tránh truy vấn N+1 và include dữ liệu không cần thiết.
- [ ] Dùng async/cancellation theo convention.
- [ ] Không thực hiện tối ưu phức tạp khi chưa có dấu hiệu hoặc số liệu.

### E. Kiểm thử
- [ ] Không có filter: trả về tập kết quả mặc định theo rule đã thống nhất.
- [ ] Mỗi filter riêng lẻ hoạt động đúng.
- [ ] Nhiều filter kết hợp cho kết quả đúng.
- [ ] Cận min/max được kiểm tra ở đúng biên.
- [ ] Sort tăng/giảm hoạt động đúng.
- [ ] Page đầu, page giữa, page vượt cuối được xử lý nhất quán.
- [ ] Query rỗng và chuỗi có khoảng trắng được xử lý đúng.
- [ ] PageSize âm/quá lớn và sort không hợp lệ được kiểm tra.
- [ ] Tập kết quả phân trang có thứ tự ổn định.
- [ ] Test dùng dữ liệu xác định, không phụ thuộc database local của một thành viên.

### F. Hiệu năng và quan sát
- [ ] Đo thời gian truy vấn bằng dữ liệu đại diện khi có thể.
- [ ] Trao đổi Người 5 về index dựa trên query thực tế.
- [ ] Kiểm tra SQL được tạo nếu truy vấn phức tạp.
- [ ] Ghi lại giả định và giới hạn hiệu năng.
- [ ] Không tuyên bố “đã tối ưu” nếu chưa đo hoặc chưa có bằng chứng.

## 4. Đầu ra bắt buộc

1. Search use case/query handler theo cấu trúc hiện có.
2. Contract tìm kiếm được review.
3. Unit/integration test cho filter, sort và pagination.
4. Ghi chú query/index nếu có tác động tới schema.
5. Ví dụ query và response để Người 4 tích hợp.
6. PR có hướng dẫn test và kết quả kiểm thử.

## 5. Tiêu chí nghiệm thu

- Kết quả đúng với tiêu chí tìm kiếm đã chốt.
- Các filter kết hợp chính xác.
- Phân trang có giới hạn và thứ tự ổn định.
- Sort chỉ cho phép tùy chọn đã định nghĩa.
- Truy vấn thực hiện ở database khi thích hợp, không tải toàn bộ dữ liệu không cần thiết.
- Test bao phủ các trường hợp biên.
- Không phá vỡ API/ViewModel đang được Người 4 sử dụng.
- Có bằng chứng build/test; mọi giới hạn còn lại được ghi rõ.

## 6. Phối hợp

| Người | Cần trao đổi |
|---|---|
| Người 1 | Phạm vi search, semantics filter, contract và quy tắc lỗi |
| Người 2 | Field, enum và trạng thái tin đăng có thể tìm kiếm |
| Người 4 | Form filter, sort option, pagination UI và trạng thái không có kết quả |
| Người 5 | Schema, index, migration, database test và hiệu năng truy vấn |

## 7. Những lỗi cần tránh

- Nối trực tiếp đầu vào người dùng vào SQL hoặc sort expression.
- Bỏ qua validation cho min/max và page size.
- Tải tất cả dữ liệu rồi mới lọc/phân trang trong bộ nhớ.
- Dùng thứ tự không ổn định làm kết quả phân trang thay đổi.
- Thêm index mà không trao đổi người sở hữu schema.
- Thay đổi ý nghĩa filter mà không cập nhật UI/docs/tests.
- Tối ưu dựa trên cảm giác thay vì đo đạc.

## 8. Checklist trước PR

- [ ] Contract được review.
- [ ] Filter/sort/pagination có test.
- [ ] Có test giá trị biên và dữ liệu không hợp lệ.
- [ ] Query không tải dữ liệu quá mức.
- [ ] Đã xem xét thứ tự ổn định.
- [ ] Người 4 biết contract và trạng thái lỗi/rỗng.
- [ ] Người 5 được báo nếu cần index/schema.
- [ ] Build/test liên quan chạy và kết quả được ghi lại.
