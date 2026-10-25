# Quy trình phát triển HAVEN

Tài liệu này mô tả luồng làm việc thực tế của nhóm từ lúc nhận nhiệm vụ đến khi tích hợp thay đổi. Các quy ước chi tiết về Git và đóng góp được trình bày trong [CONTRIBUTING.md](../CONTRIBUTING.md); tài liệu này tập trung vào thứ tự thực hiện, điểm kiểm soát và trách nhiệm.

## 1. Luồng công việc tổng thể

```text
Yêu cầu / Issue
      |
      v
Làm rõ phạm vi và tiêu chí chấp nhận
      |
      v
Tạo nhánh từ develop
      |
      v
Phân tích mã hiện có -> triển khai thay đổi nhỏ
      |
      v
Build + test + tự review diff
      |
      v
Push nhánh + mở Pull Request vào develop
      |
      v
CI + review + xử lý góp ý
      |
      v
Merge khi đủ điều kiện
      |
      v
Đồng bộ develop và xác nhận trạng thái sau merge
      |
      v
Khi nhóm chấp thuận phát hành: PR develop -> main
```

## 2. Giai đoạn A — Tiếp nhận và làm rõ nhiệm vụ

Trước khi viết code, người nhận nhiệm vụ cần xác định:

- **Mục tiêu:** vấn đề nào cần giải quyết?
- **Phạm vi:** file, tầng hoặc hành vi nào có thể thay đổi?
- **Ngoài phạm vi:** thành phần nào phải được giữ nguyên?
- **Tiêu chí chấp nhận:** kết quả cụ thể nào chứng minh nhiệm vụ hoàn thành?
- **Tác động:** có ảnh hưởng đến database, API, cấu hình, bảo mật hoặc tương thích không?
- **Kiểm thử:** cần chạy test nào và cần kiểm tra thủ công điều gì?

Nếu yêu cầu chưa rõ hoặc có khả năng làm thay đổi kiến trúc/hợp đồng hiện có, cần thống nhất với người phụ trách trước khi triển khai. Không tự suy đoán hành vi nghiệp vụ chưa được đặc tả.

## 3. Giai đoạn B — Chuẩn bị môi trường và nhánh

Từ thư mục repository:

```powershell
git status
git switch develop
git pull origin develop
git switch -c feature/<ten-nhiem-vu>
```

Thay `feature/<ten-nhiem-vu>` bằng tên nhánh phù hợp. Với sửa lỗi dùng `fix/`, tài liệu dùng `docs/`, tái cấu trúc dùng `refactor/`. Xem [CONTRIBUTING.md](../CONTRIBUTING.md) để biết quy ước đầy đủ.

Trước khi bắt đầu, xác nhận:
- Working tree sạch hoặc mọi thay đổi đang có đã được hiểu rõ.
- Đang ở nhánh công việc, không phải `main` hoặc `develop`.
- Nhánh được tạo từ phiên bản `develop` mới nhất.
- Máy có SDK và dịch vụ cần thiết theo README.

## 4. Giai đoạn C — Phân tích và triển khai

1. Đọc các file liên quan, project reference và test hiện có trước khi sửa.
2. Tìm nơi chịu trách nhiệm cho hành vi cần thay đổi; không sửa lan sang tầng khác nếu không có lý do.
3. Ưu tiên thay đổi nhỏ, có mục đích rõ ràng và phù hợp kiến trúc hiện có.
4. Bổ sung hoặc cập nhật test cho hành vi mới và trường hợp lỗi quan trọng.
5. Cập nhật tài liệu/cấu hình khi thay đổi làm chúng không còn chính xác.
6. Không commit secret, file `.env`, dữ liệu cá nhân, output build hoặc log chứa thông tin nhạy cảm.

Nếu phát hiện lỗi ngoài phạm vi, ghi nhận thành Issue riêng thay vì gộp tùy tiện vào Pull Request hiện tại, trừ khi lỗi đó ngăn cản việc hoàn thành nhiệm vụ.

## 5. Giai đoạn D — Kiểm thử và tự review

Từ thư mục gốc, chạy các lệnh sau theo thứ tự:

```powershell
dotnet restore HAVEN.slnx
dotnet build HAVEN.slnx --configuration Release --no-restore
dotnet test HAVEN.slnx --configuration Release --no-build
git diff --check
git status
git diff
```

Không ghi rằng test đã đạt nếu chưa thực sự chạy và xem kết quả. Nếu một lệnh thất bại:
- Đọc lỗi đầu tiên có tính quyết định trong log.
- Xác định lỗi do code, môi trường, dependency hay cấu hình.
- Sửa nguyên nhân trên nhánh công việc.
- Chạy lại các kiểm tra bị ảnh hưởng và bộ kiểm tra cần thiết.

Trước khi push, xác nhận diff không có file ngoài phạm vi, thay đổi vô tình, mã debug hoặc thông tin bí mật.

## 6. Giai đoạn E — Commit và mở Pull Request

Dùng staging có chọn lọc và kiểm tra nội dung staged:

```powershell
git add <duong-dan-file>
git diff --cached
git commit -m "feat: <mo-ta-ngan>"
git push -u origin <ten-nhanh>
```

Mở Pull Request với:
- **Base:** `develop` cho công việc thường ngày.
- **Compare/head:** nhánh công việc hiện tại.
- Mô tả vấn đề, thay đổi, kiểm thử và rủi ro/tác động.
- Ảnh chụp nếu có thay đổi giao diện.
- Liên kết Issue liên quan nếu có.

Không đưa nhánh tính năng trực tiếp vào `main`. Luồng phát hành thông thường là Pull Request từ `develop` sang `main` sau khi nhóm xác nhận sẵn sàng.

## 7. Giai đoạn F — CI và review

Người tạo PR:
- Theo dõi các kiểm tra CI của workflow `.github/workflows/ci.yml`.
- Trả lời góp ý bằng giải thích hoặc cập nhật code.
- Push các commit sửa đổi lên cùng nhánh PR.
- Chạy lại kiểm thử cần thiết sau khi chỉnh sửa.
- Không tự đóng góp ý review mà chưa xử lý thực chất.

Người review:
- Đối chiếu thay đổi với yêu cầu và tiêu chí chấp nhận.
- Kiểm tra logic, trường hợp biên, xử lý lỗi, bảo mật và tác động đến tầng khác.
- Kiểm tra test có thực sự xác minh hành vi cần thiết.
- Xác nhận tài liệu/cấu hình được cập nhật khi cần.
- Nêu rõ vấn đề cần sửa và lý do; tránh yêu cầu thay đổi không liên quan phạm vi.

Chỉ merge khi CI đạt, review theo thỏa thuận nhóm hoàn tất và các vấn đề chặn merge đã được xử lý. Nếu repository chưa cưỡng chế quy tắc bảo vệ nhánh, các thành viên phải tự tuân thủ quy trình này.

## 8. Giai đoạn G — Sau khi merge

```powershell
git switch develop
git pull origin develop
git status
```

Xác nhận thay đổi đã xuất hiện trên `develop`. Chỉ xóa nhánh local sau khi chắc chắn PR đã merge:

```powershell
git branch -d <ten-nhanh>
```

Nếu sau merge phát hiện lỗi, tạo nhánh sửa lỗi mới từ `develop`, tái hiện lỗi, bổ sung test phù hợp và mở PR sửa lỗi; không âm thầm sửa trực tiếp trên `main`.

## 9. Trách nhiệm trong nhóm

| Vai trò | Trách nhiệm |
|---|---|
| Người nhận nhiệm vụ | Làm rõ phạm vi, tiêu chí chấp nhận, triển khai, kiểm thử và mô tả PR trung thực. |
| Người review | Đánh giá yêu cầu, chất lượng, rủi ro và khả năng bảo trì; nêu rõ các vấn đề cần xử lý. |
| Trưởng nhóm / người điều phối | Ưu tiên công việc, giải quyết phụ thuộc, thống nhất thay đổi kiến trúc và quyết định thời điểm phát hành. |
| Người merge / phát hành | Xác nhận CI và review theo quy trình của nhóm; đảm bảo merge đúng nhánh và thông báo thay đổi cần lưu ý. |

Một người có thể kiêm nhiều vai trò trong nhóm nhỏ, nhưng cần phân biệt rõ người viết thay đổi với việc tự kiểm tra chất lượng thay đổi đó.

## 10. Checklist hoàn thành nhiệm vụ

### Trước khi code
- [ ] Yêu cầu, phạm vi và tiêu chí chấp nhận đã rõ.
- [ ] Đã đọc mã nguồn và test liên quan.
- [ ] Đã cập nhật `develop` và tạo nhánh riêng.

### Trước khi mở PR
- [ ] Thay đổi đúng phạm vi, không sửa ngoài ý muốn.
- [ ] Build Release thành công.
- [ ] Test liên quan đã chạy và kết quả được ghi đúng.
- [ ] `git diff --check` không báo lỗi.
- [ ] Không có secret, file môi trường hoặc dữ liệu nhạy cảm.
- [ ] PR mô tả rõ thay đổi và cách kiểm thử.

### Trước khi merge
- [ ] CI đạt.
- [ ] Review đã hoàn tất theo thỏa thuận nhóm.
- [ ] Góp ý chặn merge đã được xử lý.
- [ ] Tài liệu và cấu hình liên quan đã cập nhật.

### Sau khi merge
- [ ] Đã pull `develop` và xác nhận trạng thái.
- [ ] Đã thông báo các thay đổi có ảnh hưởng đến thành viên khác.
- [ ] Nhánh công việc được dọn dẹp sau khi xác nhận merge.

## 11. Tài liệu liên quan

- [README.md](../README.md) — giới thiệu repository và hướng dẫn thiết lập.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — quy ước Git, commit và Pull Request.
- [Pull Request template](../.github/pull_request_template.md) — cấu trúc mô tả PR.
- [CI workflow](../.github/workflows/ci.yml) — các bước restore, build và test tự động.

> Lưu ý: Quy trình này quy định cách nhóm làm việc; nó không khẳng định các chức năng nghiệp vụ của HAVEN đã được triển khai hoàn chỉnh.
