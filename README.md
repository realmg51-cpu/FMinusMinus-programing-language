# 🚀 F-- (F Minus Minus) Programming Language

[![.NET Build](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-build.yml/badge.svg?branch=main)](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-build.yml)
[![.NET Test](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-test.yaml/badge.svg)](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-test.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/realmg51-cpu/F---Programming-Language?style=social)](https://github.com/realmg51-cpu/F---Programming-Language/stargazers)

## 🌟 Giới thiệu

**F--** (F Minus Minus) - *"The backward step of humanity, but forward step in creativity!"*

Được sáng tạo bởi một **lập trình viên 13 tuổi**, F-- là ngôn ngữ lập trình mang triết lý độc đáo:

> *"Cứ code đi, compiler lo phần còn lại!"*

Dự án được xây dựng hoàn toàn bằng **.NET**, minh chứng cho việc tuổi tác không phải là rào cản để tạo ra những sản phẩm công nghệ thú vị.

## ✨ Tính năng nổi bật

| Tính năng | Mô tả | Ví dụ |
|-----------|-------|-------|
| 🖥️ **Import siêu cấp** | Import cả computer, không cần quan tâm phần cứng! | `import computer` |
| 🔤 **String Interpolation** | Chèn biến trực tiếp vào chuỗi | `$"Hello {name}!"` |
| 🧠 **Quản lý bộ nhớ** | Kiểm tra bộ nhớ tự động | `memory.memoryleft` |
| 📁 **File I/O trực quan** | Thao tác file dễ như ăn kẹo | `at "file.txt" { ... }` |
| 🚨 **Tự động báo lỗi FMM** | Hệ thống mã lỗi F Minus Minus chuyên nghiệp | `fmm001: syntax error` |

## 🎮 Ví dụ nhanh

Đây là chương trình F-- hoàn chỉnh:

```f--
import computer

start()
{
    // Khai báo biến
    name = "F--"
    version = 1.4
    
    // In ra màn hình với interpolation
    println($"Xin chào từ {name} v{version}!")
    println($"Memory còn: {memory.memoryleft} MB")
    
    // Thao tác với file
    io.cfile("hello"(path "txt"))
    at "hello.txt"
    {
        io.println("Hello file!")
        io.println($"Created by F-- v{version}")
        io.save()
    }
    
    println("✅ File đã được tạo!")
    
    return(0)  // Thành công
    end()      // Kết thúc chương trình
}
```
🚀 Cài đặt & Sử dụng

Yêu cầu hệ thống

- .NET SDK 8.0 trở lên
- Git (để clone repository)

1. Clone repository

```bash
git clone https://github.com/realmg51-cpu/F---Programming-Language.git
cd F---Programming-Language
```

2. Build project

```bash
dotnet build
```

3. Chạy chương trình F-- đầu tiên

```bash
# Tạo file examples/hello.f-- với nội dung ở trên
# Sau đó chạy:
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--
```

4. Xem cây cú pháp (AST)

```bash
dotnet run --project Compiler/compiler.csproj -- ast examples/hello.f--
```

5. Chạy unit tests

```bash
dotnet test
```

📦 Cài đặt qua NuGet (Sắp có)

Package F-- sẽ sớm có mặt trên NuGet.org:

```bash
dotnet add package FSharpMinus --version 1.0.0
```

Sau đó bạn có thể dùng F-- như một global tool:

```bash
fminus run hello.f--
```
