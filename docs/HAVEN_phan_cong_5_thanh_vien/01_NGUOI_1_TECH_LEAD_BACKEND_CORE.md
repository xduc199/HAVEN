# Người 1 — Tech Lead / Backend Core

## 1. Sứ mệnh của vai trò

Chịu trách nhiệm giữ cho nhóm phát triển HAVEN theo một kiến trúc nhất quán, có thể tích hợp và kiểm thử. Người 1 điều phối yêu cầu, Domain, contract giữa các tầng, review thay đổi có ảnh hưởng rộng và hỗ trợ thành viên gỡ vướng.

**Không có nghĩa là:** Người 1 tự làm mọi phần backend hoặc trở thành người duy nhất hiểu hệ thống. Mục tiêu là tạo nền tảng rõ ràng để bốn thành viên còn lại làm việc độc lập trong phạm vi đã thống nhất.

## 2. Trách nhiệm chính

1. Kiểm kê hiện trạng repository và phân biệt code đã xác minh với tài liệu/kế hoạch.
2. Làm rõ phạm vi MVP, use case, actor và quy tắc nghiệp vụ.
3. Giữ ranh giới kiến trúc Domain / Application / Infrastructure / Web theo cấu trúc thực tế của solution.
4. Chốt Domain model và contract dùng chung sau khi tham khảo Người 2, 3 và 5.
5. Quản lý quyết định kiến trúc, rủi ro và phụ thuộc liên nhóm.
6. Review PR có thay đổi contract, kiến trúc, bảo mật hoặc luồng tích hợp.
7. Điều phối tích hợp, demo và nghiệm thu MVP.

## 3. Kế hoạch công việc chi tiết

### A. Kiểm kê ban đầu
- [ ] Xác nhận branch, commit và trạng thái working tree.
- [ ] Đọc README, tài liệu kiến trúc, workflow và CI.
- [ ] Xác định các project, reference giữa project và entry point.
- [ ] Chạy build/test hiện có; ghi lại lệnh, kết quả, lỗi và môi trường.
- [ ] Kiểm tra controller, service/use case, entity, DbContext, migration và View hiện có.
- [ ] Lập bảng trạng thái từng chức năng: đã xác minh / một phần / chưa có / chưa xác minh.
- [ ] Không yêu cầu viết lại thành phần đang hoạt động nếu chưa có lý do kỹ thuật được chứng minh.

**Đầu ra:** `docs/BASELINE_AUDIT.md` hoặc tài liệu tương đương; danh sách vấn đề và task đã xác minh.

### B. Chốt yêu cầu MVP
- [ ] Xác định nhóm người dùng và luồng chính.
- [ ] Chốt MVP có cho phép tạo/quản lý tin đăng hay chỉ tìm kiếm/xem.
- [ ] Chốt yêu cầu đăng nhập/phân quyền; nếu ngoài phạm vi thì ghi rõ.
- [ ] Chốt các trường thông tin bất động sản bắt buộc và tùy chọn.
- [ ] Chốt bộ lọc tìm kiếm, sắp xếp và phân trang cần thiết.
- [ ] Chốt các trạng thái tin đăng và quy tắc chuyển trạng thái nếu có.
- [ ] Viết acceptance criteria có thể kiểm thử.
- [ ] Đưa câu hỏi chưa thống nhất vào danh sách blocking questions, không tự quyết thay nhóm.

**Đầu ra:** tài liệu MVP, user flow, danh sách use case và acceptance criteria.

### C. Domain và Application contract
- [ ] Phối hợp Người 2 xác định các quy tắc property.
- [ ] Phối hợp Người 3 xác định tiêu chí search và ngữ nghĩa lọc.
- [ ] Phối hợp Người 5 xác định nhu cầu persistence.
- [ ] Chỉ đưa logic nghiệp vụ vào Domain/Application phù hợp kiến trúc hiện hành.
- [ ] Chốt request/response DTO hoặc contract tương đương.
- [ ] Chốt cách biểu diễn kết quả rỗng, lỗi validation, not found và lỗi hệ thống.
- [ ] Chốt quy ước bất đồng bộ, cancellation token và naming theo codebase.
- [ ] Viết unit test cho quy tắc cốt lõi do Người 1 sở hữu.

**Đầu ra:** contract được review và ví dụ request/response; ghi chú quyết định kiến trúc nếu cần.

### D. Review và tích hợp
- [ ] Review PR theo checklist chung.
- [ ] Kiểm tra dependency direction; tránh để Domain phụ thuộc Web/Infrastructure.
- [ ] Kiểm tra thay đổi contract có thông báo đến các bên liên quan.
- [ ] Tách yêu cầu refactor lớn khỏi PR chức năng khi có thể.
- [ ] Hỗ trợ giải quyết merge conflict bằng cách phối hợp, không ghi đè công việc của thành viên khác.
- [ ] Điều phối chạy test/build tích hợp trên `develop`.
- [ ] Duy trì danh sách lỗi blocker và người chịu trách nhiệm.
- [ ] Demo luồng đầu-cuối theo acceptance criteria.

## 4. Phân phối thời gian gợi ý

- 25%: yêu cầu và kiểm kê hiện trạng.
- 25%: Domain/contract và quyết định kiến trúc.
- 25%: review, phối hợp, gỡ blocker.
- 25%: tích hợp, kiểm thử và nghiệm thu.

Điều chỉnh theo tình hình; Tech Lead cần dành thời gian cho phối hợp thay vì nhận quá nhiều task code độc lập.

## 5. Phụ thuộc và phối hợp

| Làm việc với | Cần thống nhất |
|---|---|
| Người 2 | Domain property, validation, use case và trạng thái tin |
| Người 3 | Search criteria, sort, pagination, semantics của bộ lọc |
| Người 4 | Routes, ViewModel, trạng thái UI, validation/error display |
| Người 5 | Persistence contract, entity mapping, migration, integration test |
| Cả nhóm | Phạm vi MVP, tiêu chí nghiệm thu, cách chạy và quy tắc Git |

## 6. Tiêu chí nghiệm thu công việc của Người 1

- Có baseline audit dựa trên repository thực tế, nêu lệnh và bằng chứng.
- Các quyết định MVP quan trọng được ghi lại và nhóm xác nhận.
- Contract được các thành viên bị ảnh hưởng review trước khi triển khai sâu.
- PR được review theo rủi ro; không merge chỉ vì “chạy được trên máy người viết”.
- Có acceptance checklist cho luồng MVP.
- Build/test tích hợp được báo cáo trung thực, gồm cả lỗi còn tồn tại.
- Không có quyết định kiến trúc lớn chỉ nằm trong tin nhắn cá nhân.

## 7. Ranh giới không tự ý vượt qua

- Không tự thay schema khi chưa phối hợp Người 5.
- Không đổi API/DTO mà không báo Người 2, 3 và 4.
- Không thêm công nghệ mới chỉ vì sở thích cá nhân.
- Không viết lại toàn bộ kiến trúc khi chưa có bằng chứng về vấn đề.
- Không tuyên bố tính năng hoàn tất nếu chưa có acceptance test phù hợp.

## 8. Checklist review PR

- [ ] PR có một mục đích chính và liên kết task.
- [ ] Không có file hoặc refactor ngoài phạm vi không giải thích được.
- [ ] Code tuân theo kiến trúc và naming hiện hành.
- [ ] Validation, lỗi và edge case được xử lý.
- [ ] Có test hoặc giải thích rõ vì sao không thể test.
- [ ] Không có secret, dữ liệu nhạy cảm hoặc log không an toàn.
- [ ] README/docs/schema/contract được cập nhật khi cần.
- [ ] CI/test đạt hoặc lỗi được mô tả rõ.
- [ ] Ảnh hưởng đến các thành viên khác đã được thông báo.

## 9. Báo cáo cuối ngày

- Việc đã hoàn thành và bằng chứng.
- Quyết định vừa chốt.
- PR đang chờ review.
- Blocker, người cần hỗ trợ và thời điểm cần quyết định.
- Rủi ro đối với phạm vi hoặc tiến độ MVP.
