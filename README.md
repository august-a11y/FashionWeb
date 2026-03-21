# FashionWeb

## Sau khi chạy xong workflow CI thì làm gì?

Workflow CI tại `.github/workflows/ci.yml` gồm:

- `Build & Test (.NET 8)` cho cả `pull_request` và `push` vào `master`
- `Build & Push Docker image` chỉ chạy khi `push` vào `master` (sau khi build/test thành công)

Sau khi CI chạy xong, làm theo checklist sau:

1. **Kiểm tra trạng thái workflow**
   - Tất cả job phải màu xanh (`success`).
2. **Nếu là Pull Request**
   - Khi `Build & Test` pass: tiếp tục review code và merge theo quy trình.
3. **Nếu là Push vào `master`**
   - Xác nhận thêm job `Build & Push Docker image` pass.
   - Ảnh Docker sẽ được push với tag:
     - `${GITHUB_SHA}`
     - `latest`
     - image name: `vyle1008/fashionweb-api`
4. **Nếu job fail**
   - Mở log của job lỗi, sửa lỗi tương ứng, rồi push lại để CI chạy lại.
