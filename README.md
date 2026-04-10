# FASHIONWEB – HỆ THỐNG QUẢN LÝ BÁN HÀNG THỜI TRANG TRÊN NỀN WEB

## Chương 1: Tổng quan dự án
### 1.1. Thông tin đề tài
- **Tên đề tài:** Xây dựng hệ thống web quản lý và vận hành cửa hàng thời trang.
- **Loại đề tài:** Đồ án môn học Lập trình Web.
- **Định hướng:** Thiết kế hệ thống theo kiến trúc nhiều lớp, tách biệt rõ phần nghiệp vụ, dữ liệu và giao diện quản trị.

### 1.2. Tóm tắt học thuật (Abstract)
Đề tài xây dựng một hệ thống web phục vụ quản lý sản phẩm, danh mục, đơn hàng, giỏ hàng và người dùng trong bối cảnh thương mại điện tử thời trang. Hệ thống backend được phát triển bằng ASP.NET Core 8 theo mô hình phân lớp (Domain – Application – Infrastructure – API), sử dụng SQL Server làm cơ sở dữ liệu chính và Redis cho dữ liệu truy cập nhanh. Phần frontend quản trị được phát triển bằng Angular 21, giao tiếp với backend qua RESTful API có xác thực JWT. Hệ thống hỗ trợ tự động migration, seed dữ liệu mẫu, tài liệu API Swagger và quy trình CI kiểm thử/build tự động.

### 1.3. Mục tiêu nghiên cứu và triển khai
1. Xây dựng API backend có khả năng mở rộng, dễ bảo trì theo Clean Architecture.
2. Triển khai cơ chế xác thực/ủy quyền người dùng bằng JWT và Role-based Authorization.
3. Phát triển giao diện quản trị hỗ trợ các nghiệp vụ cốt lõi của cửa hàng thời trang.
4. Chuẩn hóa quy trình phát triển với kiểm thử, build và CI.
5. Đảm bảo hệ thống có thể chạy cục bộ và qua Docker Compose.

### 1.4. Phạm vi chức năng
#### 1.4.1. Nhóm chức năng người dùng
- Đăng ký, đăng nhập, làm mới token.
- Quản lý thông tin cá nhân, đổi mật khẩu.
- Quản lý địa chỉ giao hàng.
- Duyệt sản phẩm, danh mục, biến thể.
- Quản lý giỏ hàng.
- Tạo đơn hàng, xem trước đơn hàng, theo dõi/cancel đơn hàng.

#### 1.4.2. Nhóm chức năng quản trị
- Quản lý danh mục sản phẩm.
- Quản lý sản phẩm và biến thể sản phẩm.
- Quản lý đơn hàng và cập nhật trạng thái đơn hàng.
- Theo dõi số liệu tổng quan dashboard.

## Chương 2: Cơ sở lý thuyết
### 2.1. Kiến trúc hệ thống
Hệ thống được tổ chức theo 4 tầng chính:

1. **FashionShop.Domain**  
   Chứa thực thể miền nghiệp vụ (Product, Category, Order, Cart, Address, Identity...).

2. **FashionShop.Application**  
   Chứa service nghiệp vụ, DTO, interface, validation và specification.

3. **FashionShop.Infrastructure**  
   Cài đặt truy cập dữ liệu (EF Core), repository, cache Redis, seeding dữ liệu, cấu hình hạ tầng.

4. **FashionShop.API**  
   Cung cấp REST API, middleware, cấu hình JWT, CORS, Swagger, static files.

Phần giao diện quản trị tách riêng:
- **fashion_web_admin_ui**: Angular 21 + CoreUI + PrimeNG.

### 2.2. Công nghệ sử dụng
#### Backend
- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8 + SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- Redis (StackExchange.Redis)
- FluentValidation, MediatR, Ardalis.Specification
- Swagger / OpenAPI

#### Frontend (Admin UI)
- Angular 21
- CoreUI Angular
- PrimeNG
- NSwag (sinh client API từ Swagger)

#### DevOps
- Docker, Docker Compose
- GitHub Actions CI (restore/build/test, docker build & push trên nhánh `master`)

### 2.3. Cấu trúc thư mục chính
```text
FashionWeb/
├── src/
│   ├── FashionShop.API/
│   ├── FashionShop.Application/
│   ├── FashionShop.Domain/
│   └── FashionShop.Infrastructure/
├── tests/
│   └── FashionShop.Application.UnitTests/
├── fashion_web_admin_ui/
├── docker-compose.yml
└── FashionWeb.sln
```

## Chương 3: Phân tích và thiết kế hệ thống
### 3.1. Thiết kế API và định tuyến chính
- **Authentication:** `/api/auth/*`
- **User APIs:** `/api/products`, `/api/categories`, `/api/variants`, `/api/carts`, `/api/orders`, `/api/users`, `/api/addresses`
- **Admin APIs:** `/api/admin/products`, `/api/admin/categories`, `/api/admin/variants`, `/api/admin/orders`, `/api/admin/dashboard`

Tài liệu API ở chế độ Development:
- `https://localhost:7239/swagger`
- `http://localhost:5259/swagger`

### 3.2. Cơ sở dữ liệu và dữ liệu mẫu
- Ứng dụng tự động chạy **EF Core Migration** khi khởi động API.
- Seeder tự động tạo dữ liệu mẫu (roles, users, catalog, cart, order...).
- Tài khoản mẫu:
  - **Admin:** `admin` / `Admin@123`
  - **User:** `user` / `User@123`

> Khuyến nghị: đổi toàn bộ mật khẩu mẫu khi triển khai thực tế.

### 3.3. Yêu cầu môi trường
- .NET SDK 8.0+
- Node.js theo yêu cầu Angular (`^20.19.0 || ^22.12.0 || ^24.0.0`)
- npm >= 10
- SQL Server
- Redis
- (Tùy chọn) Docker & Docker Compose

### 3.4. Hướng dẫn chạy dự án
#### 3.4.1. Chạy backend API (local)
```bash
cd /home/runner/work/FashionWeb/FashionWeb
dotnet restore FashionWeb.sln
dotnet build FashionWeb.sln -c Release --no-restore
dotnet run --project src/FashionShop.API/FashionShop.API.csproj
```

Biến cấu hình quan trọng (có thể set qua môi trường):
- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__Redis`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `ASPNETCORE_URLS` (ví dụ `http://+:7239`)

#### 3.4.2. Chạy Admin UI (Angular)
```bash
cd /home/runner/work/FashionWeb/FashionWeb/fashion_web_admin_ui
npm install
npm start
```

Lưu ý:
- URL API đang cấu hình trong `src/environments/environment.ts` là `https://localhost:7239`.
- API hiện CORS mặc định cho origin: `https://localhost:4200`.

#### 3.4.3. Chạy bằng Docker Compose
```bash
cd /home/runner/work/FashionWeb/FashionWeb
docker compose up -d
```

Stack gồm:
- `fashionweb-api` (cổng `7239`)
- `fashionshop-sql` (cổng `1433`)
- `fashionshop-redis` (cổng `6379`)

## Chương 4: Đánh giá và tổng kết
### 4.1. Kiểm thử và chất lượng
#### 4.1.1. Build & test solution
```bash
cd /home/runner/work/FashionWeb/FashionWeb
dotnet restore FashionWeb.sln
dotnet build FashionWeb.sln -c Release --no-restore
dotnet test FashionWeb.sln -c Release --no-build --verbosity normal
```

#### 4.1.2. CI Pipeline
Workflow CI tự động:
- Trigger khi `push`/`pull_request` vào `master`.
- Chạy `restore`, `build`, `test` cho .NET 8.
- Job docker push chỉ chạy khi `push` vào `master`.

### 4.2. Sinh lại API client cho Angular (NSwag)
Khi backend API thay đổi, sinh lại client:
```bash
cd /home/runner/work/FashionWeb/FashionWeb/fashion_web_admin_ui
npm install
npm run nswag-admin
```

File sinh ra:
- `src/app/api/admin-api.service.generated.ts`

### 4.3. Đánh giá kết quả đạt được
- Hoàn thiện một hệ thống web có kiến trúc rõ ràng, tách lớp tốt.
- Tích hợp đầy đủ chức năng nghiệp vụ cốt lõi của một cửa hàng thời trang.
- Có khả năng demo thực tế với dữ liệu mẫu, Swagger và dashboard quản trị.
- Có nền tảng mở rộng cho thanh toán online, theo dõi tồn kho, tối ưu hiệu năng.

### 4.4. Hướng phát triển
1. Bổ sung giao diện người mua (customer storefront) riêng.
2. Tích hợp cổng thanh toán trực tuyến và xử lý trạng thái giao dịch.
3. Triển khai logging/monitoring tập trung và audit trail.
4. Mở rộng kiểm thử tích hợp/e2e và kiểm thử bảo mật.
5. Tối ưu CORS, secret management và hardening cấu hình production.

### 4.5. Giấy phép và ghi nhận
- Cần bổ sung thông tin license chính thức của nhóm đề tài trước khi phát hành công khai.
- Một phần giao diện quản trị sử dụng nền tảng CoreUI Angular template (MIT).
