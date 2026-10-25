# Hướng dẫn đóng góp HAVEN

Tài liệu này thống nhất cách làm việc với Git và GitHub trong dự án HAVEN. Mục tiêu là giữ lịch sử rõ ràng, giảm xung đột và hạn chế đưa lỗi vào nhánh tích hợp.

## 1. Nguyên tắc chung

1. Không phát triển trực tiếp trên `main` hoặc `develop`.
2. Mỗi nhiệm vụ nên có phạm vi rõ ràng và nhánh riêng.
3. Đồng bộ `develop` trước khi bắt đầu công việc mới.
4. Chạy build và test trước khi mở Pull Request.
5. Không đưa bí mật, file `.env`, output build hoặc dữ liệu cá nhân vào Git.
6. Không tự ý sửa/xóa hệ thống hoặc kiến trúc ngoài phạm vi nhiệm vụ.
7. Nếu cần thay đổi kiến trúc, package, cấu trúc database hoặc hợp đồng API, hãy mô tả lý do và tác động trong Pull Request.

## 2. Chuẩn bị repository

Clone repository và chuyển sang nhánh tích hợp:

```powershell
git clone https://github.com/xduc199/HAVEN.git
cd HAVEN
git switch develop
git pull origin develop
```

Thiết lập môi trường local theo README.md. Không chia sẻ file `.env` hoặc mật khẩu giữa các thành viên.

## 3. Quy tắc đặt tên nhánh

Tạo nhánh từ `develop`:

```powershell
git switch develop
git pull origin develop
git switch -c feature/property-search
```

Quy ước:

| Loại | Mẫu | Ví dụ |
|---|---|---|
| Tính năng | `feature/<ten-ngan>` | `feature/property-search` |
| Sửa lỗi | `fix/<mo-ta-ngan>` | `fix/property-filter` |
| Tài liệu | `docs/<mo-ta-ngan>` | `docs/update-readme` |
| Tái cấu trúc | `refactor/<pham-vi>` | `refactor/application-services` |
| Khẩn cấp | `hotfix/<mo-ta-ngan>` | `hotfix/startup-error` |

Dùng chữ thường, không dấu, các từ cách nhau bằng dấu gạch ngang. Một nhánh nên tập trung vào một nhiệm vụ có thể review được.

## 4. Commit

Trước khi commit, kiểm tra:

```powershell
git status
git diff
git diff --check
```

Đưa vào staging các file có chủ đích, sau đó commit:

```powershell
git add <duong-dan-file>
git diff --cached
git commit -m "feat: add property search foundation"
```

Mẫu commit khuyến nghị:

- `feat: ...` — thêm tính năng.
- `fix: ...` — sửa lỗi.
- `docs: ...` — tài liệu.
- `test: ...` — kiểm thử.
- `refactor: ...` — tái cấu trúc không đổi hành vi dự kiến.
- `build: ...` — cấu hình build/dependency.
- `ci: ...` — pipeline tự động.
- `chore: ...` — công việc bảo trì.

Viết nội dung ngắn gọn, mô tả thay đổi thực tế. Không dùng các thông điệp mơ hồ như `update`, `fix bug` hoặc `final`.

## 5. Đồng bộ và đẩy nhánh

Trước khi gửi Pull Request, lấy thay đổi mới từ `develop`. Nếu nhánh có thay đổi độc lập, tích hợp chúng theo cách nhóm đã thống nhất; không force-push lên nhánh dùng chung.

```powershell
git fetch origin
git status
git push -u origin feature/property-search
```

Thay `feature/property-search` bằng tên nhánh thực tế. Nếu có xung đột, giải quyết và kiểm thử lại trước khi push.

## 6. Pull Request

- Pull Request của tính năng/sửa lỗi/tài liệu thông thường: nhánh công việc → `develop`.
- Phát hành phiên bản đã được nhóm xác nhận: `develop` → `main`.
- Không đưa trực tiếp một nhánh tính năng vào `main` nếu chưa có thống nhất riêng của nhóm.

Mỗi Pull Request cần nêu:
1. Vấn đề hoặc nhu cầu.
2. Thay đổi chính.
3. Cách kiểm thử và kết quả.
4. Ảnh chụp màn hình nếu thay đổi giao diện.
5. Tác động tới cấu hình, database, API, bảo mật hoặc khả năng tương thích.
6. Các việc chưa hoàn thành hoặc rủi ro còn lại.

Trước khi yêu cầu review:

```powershell
dotnet restore HAVEN.slnx
dotnet build HAVEN.slnx --configuration Release --no-restore
dotnet test HAVEN.slnx --configuration Release --no-build
```

Chỉ merge khi CI thành công, thay đổi đã được review theo thỏa thuận của nhóm và các cuộc thảo luận cần xử lý đã được giải quyết. Vì GitHub hiện chưa cưỡng chế branch protection trên repository riêng tư ở gói hiện tại, đây là quy trình nhóm cần thực hiện chủ động.

## 7. Checklist review

Người review nên xác nhận:

- [ ] Thay đổi đúng phạm vi và giải quyết yêu cầu.
- [ ] Không đưa secret, file môi trường hoặc dữ liệu nhạy cảm vào Git.
- [ ] Không thay đổi ngoài phạm vi mà không giải thích.
- [ ] Build và test có kết quả rõ ràng.
- [ ] Xử lý lỗi, validation và các trường hợp biên phù hợp.
- [ ] Cập nhật tài liệu/cấu hình khi cần.
- [ ] Không để lại mã debug hoặc TODO không được giải thích.

## 8. Sau khi Pull Request được merge

Đồng bộ nhánh tích hợp và xóa nhánh công việc đã hoàn thành nếu không còn cần thiết:

```powershell
git switch develop
git pull origin develop
git branch -d feature/property-search
```

Chỉ xóa nhánh local sau khi xác nhận công việc đã được merge. Không dùng `git push --force` trên `main`, `develop` hoặc nhánh của thành viên khác.

## 9. Khi có sự cố

- Build/test thất bại: đọc log, tái hiện lỗi và sửa trên nhánh công việc.
- Xung đột merge: phối hợp với người sở hữu thay đổi liên quan; không ghi đè thay đổi của họ.
- Lỡ đưa bí mật lên Git: thu hồi/thay mới bí mật ngay và báo người phụ trách; không chỉ xóa file rồi coi như đã an toàn.
- Cần thay đổi quy ước: cập nhật tài liệu và thông báo cho cả nhóm trước khi áp dụng.
