# 🚀 F-- (F Minus Minus) Programming Language

[![.NET Build](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-build.yml/badge.svg?branch=main)](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-build.yml)
[![.NET Test](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-test.yml)
[![codecov](https://codecov.io/gh/realmg51-cpu/F---Programming-Language/branch/main/graph/badge.svg)](https://codecov.io/gh/realmg51-cpu/F---Programming-Language)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/realmg51-cpu/F---Programming-Language)](https://github.com/realmg51-cpu/F---Programming-Language/releases)
[![GitHub stars](https://img.shields.io/github/stars/realmg51-cpu/F---Programming-Language?style=social)](https://github.com/realmg51-cpu/F---Programming-Language/stargazers)

---

## 📋 Table of Contents
- [🌟 Introduction](#-introduction)
- [✨ Features](#-features)
- [🎮 Quick Example](#-quick-example)
- [🚀 Installation](#-installation)
- [📖 Documentation](#-documentation)
- [🧪 Testing](#-testing)
- [📊 Code Coverage](#-code-coverage)
- [📁 Project Structure](#-project-structure)
- [🤝 Contributing](#-contributing)
- [📜 License](#-license)
- [👨‍💻 Author](#-author)

---

## 🌟 Introduction

**F--** (F Minus Minus) - *"The backward step of humanity, but forward step in creativity!"*

Created by a **13-year-old developer**, F-- is a unique programming language with a special philosophy:

> *"Just code, the compiler handles the rest!"*

The project is built entirely with **.NET**, proving that age is not a barrier to creating interesting technology products.

---

## ✨ Features

| Feature | Description | Example |
|---------|-------------|---------|
| 🖥️ **Super Import** | Import the entire computer, no hardware worries! | `import computer` |
| 🔤 **String Interpolation** | Embed variables directly in strings | `$"Hello {name}!"` |
| 🧠 **Memory Management** | Automatic memory checking | `memory.memoryleft` |
| 📁 **Intuitive File I/O** | File operations made simple | `at "file.txt" { ... }` |
| 🚨 **FMM Error Codes** | Professional F Minus Minus error system | `fmm001: syntax error` |

---

## 🎮 Quick Example

Here's a complete F-- program:

```f--
import computer
start()
{
    // Variable declaration
    name = "F--"
    version = 2.0
    
    // Print with interpolation
    println($"Hello from {name} v{version}!")
    println($"Memory left: {memory.memoryleft} MB")
    
    // File operations
    io.cfile("hello"(path "txt"))
    at "hello.txt"
    {
        io.println("Hello file!")
        io.println($"Created by F-- v{version}")
        io.save()
    }
    
    println("✅ File created!")
    
    return(0)  // Success
    end()      // End program
}
🚀 Installation

System Requirements

· .NET SDK 8.0 or higher
· Git (to clone repository)

Option 1: Download from Release

1. Go to Releases
2. Download the latest Fminusminus-{version}.zip file
3. Extract the archive
4. Open terminal/command prompt in the extracted folder

Windows:

```cmd
build(Windows).cmd
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--
```

Linux / macOS:

```bash
chmod +x build\(Mac\,Linux\).sh
./build\(Mac\,Linux\).sh
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--
```

Option 2: Clone from GitHub

```bash
git clone https://github.com/realmg51-cpu/F---Programming-Language.git
cd F---Programming-Language

# Build
dotnet build

# Run F-- program
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--

# View AST tree
dotnet run --project Compiler/compiler.csproj -- ast examples/hello.f--
```

Option 3: Install via NuGet (Coming Soon)

```bash
# Install as global tool
dotnet tool install --global Fminusminus --version 2.0.0

# Run directly from terminal
fminus run hello.f--
```

---

📖 Documentation

Basic Syntax

```f--
import computer           // Required first line
start()                   // Entry point
{
    // Your code here
    print("No newline")   // Print without newline
    println("With newline") // Print with newline
    memory.memoryleft      // Check memory
    return(0)              // Return code
    end()                  // Required end statement
}
```

File I/O Operations

```f--
// Create a file
io.cfile("filename"(path "txt"))

// Write to file
at "filename.txt"
{
    io.println("Content line 1")
    io.print("No newline here")
    io.println("New line")
}

// Save file
io.save()
// or save to specific path
io.save("C:/myfolder/")
```

Memory Management

```f--
memory.memoryleft    // Check available memory
memory.memoryused    // Check used memory
memory.memorytotal   // Check total memory
```

---

🧪 Testing

Run the test suite to ensure everything works:

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific test
dotnet test --filter "FullyQualifiedName~LexerTests"
```

---

📊 Code Coverage

https://codecov.io/gh/realmg51-cpu/F---Programming-Language/branch/main/graph/badge.svg

We use Codecov to monitor code quality. Current coverage: ~85%

Level Coverage Status
🔴 Poor < 60% Needs urgent testing
🟡 Average 60-75% Acceptable
🟢 Good 75-85% Stable
💚 Excellent 85% High quality

---
