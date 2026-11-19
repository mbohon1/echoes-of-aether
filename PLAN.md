# Kế hoạch dự án: Echoes of Aether

## 1. Mục tiêu
Xây dựng prototype góc nhìn thứ ba trên Unity. Nhân vật di chuyển bằng `CharacterController`, camera dùng Cinemachine. Mã nguồn được công khai trên GitHub để người khác xem xét.

## 2. Quyết định kiến trúc (đã chốt)
| Hạng mục | Lựa chọn | Lý do |
|---|---|---|
| Di chuyển | `CharacterController.Move` | Điều khiển chính xác, dễ dự đoán. Không phải tinh chỉnh vật lý như khi dùng Rigidbody |
| Camera | Cinemachine 3.x (Third Person Follow + Rotate With Follow Target) | Có sẵn follow, damping và xử lý va chạm camera. Không cần tự viết hệ camera |
| Input | Input System, tạo action bằng code | Không phụ thuộc file `.inputactions`, repo gọn hơn |
| Phiên bản | Unity 6 LTS, Input System 1.11+, Cinemachine 3.1+ | Bộ phiên bản LTS hiện đại, API thống nhất |

## 3. Phân chia trách nhiệm
- `PlayerInputReader`: chỉ đọc input.
- `PlayerMovement`: xử lý di chuyển, xoay hướng, nhảy, trọng lực và kiểm tra tiếp đất.
- `PlayerCameraInput`: xoay `CameraTarget`. Cinemachine đảm nhận phần còn lại của camera.

## 4. Lộ trình

### Milestone 1: Khung điều khiển (repo hiện tại)
- [x] Chốt kiến trúc
- [x] `PlayerInputReader.cs`
- [x] `PlayerMovement.cs`: di chuyển theo hướng camera, đi/chạy, nhảy, coyote time, jump buffer, trọng lực
- [x] `PlayerCameraInput.cs`: yaw/pitch, giới hạn góc dọc, hỗ trợ chuột và tay cầm
- [x] README hướng dẫn cài đặt và dựng scene, `.gitignore` cho Unity
- [ ] Mở trong Unity 6, sửa lỗi compile nếu có
- [ ] Dựng test scene và chạy hết checklist thủ công trong README

### Milestone 2: Hoàn thiện cảm giác điều khiển
- [ ] Trượt dốc khi dốc vượt `slopeLimit`
- [ ] Kiểm tra tiếp đất bổ sung bằng SphereCast
- [ ] Animator: idle, walk, run, jump, fall
- [ ] Hỗ trợ đổi phím (rebinding)

### Milestone 3: Nội dung khám phá
- [ ] Màn chơi mẫu (greybox)
- [ ] Đối tượng tương tác cơ bản
- [ ] Hệ thống lưu checkpoint

## 5. Quy trình GitHub
1. Phát triển trên `main` cho đến khi có milestone ổn định. Sau đó dùng nhánh `feature/*` và tạo Pull Request.
2. Commit nhỏ, đặt message theo dạng `feat:`, `fix:`, `docs:`.
3. Không commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`. Các thư mục này đã có trong `.gitignore`.
4. Khi đã thêm Unity project đầy đủ, commit cả `Packages/manifest.json` và `ProjectSettings/`.

## 6. Trạng thái trung thực
- Code là **bản nháp**, chưa được compile hay chạy thử trong Unity.
- README không tuyên bố tính năng nào là đã kiểm chứng.
