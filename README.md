# 🚀 F-- (F Minus Minus) Programming Language

<div align="center">

[![.NET Build](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-build.yml)
[![.NET Test](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/realmg51-cpu/F---Programming-Language/actions/workflows/dotnet-test.yml)
[![codecov](https://codecov.io/gh/realmg51-cpu/F---Programming-Language/branch/main/graph/badge.svg)](https://codecov.io/gh/realmg51-cpu/F---Programming-Language)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/realmg51-cpu/F---Programming-Language)](https://github.com/realmg51-cpu/F---Programming-Language/releases)
[![GitHub stars](https://img.shields.io/github/stars/realmg51-cpu/F---Programming-Language?style=social)](https://github.com/realmg51-cpu/F---Programming-Language/stargazers)
[![GitHub contributors](https://img.shields.io/github/contributors/realmg51-cpu/F---Programming-Language)](https://github.com/realmg51-cpu/F---Programming-Language/graphs/contributors)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/realmg51-cpu/F---Programming-Language/pulls)
[![Made with ❤️ by 13-year-old](https://img.shields.io/badge/Made%20with-%E2%9D%A4%EF%B8%8F%20by%2013--year--old-ff69b4.svg)](https://github.com/realmg51-cpu)


### *"The backward step of humanity, but forward step in creativity!"*

[🌟 Introduction](#-introduction) •
[✨ Features](#-features) •
[🎮 Quick Example](#-quick-example) •
[🚀 Installation](#-installation) •
[📖 Documentation](#-documentation) •
[🧪 Testing](#-testing) •
[🤝 Contributing](#-contributing) •
[📜 License](#-license)

</div>

---

## 🌟 Introduction

**F--** (F Minus Minus) is not just another programming language – it's a **passion project** by a **13-year-old developer** who believes that age is just a number when it comes to creativity!

> *"Just code, the compiler handles the rest!"* 

Built entirely with **.NET**, F-- aims to make programming fun, intuitive, and accessible to everyone. Whether you're a beginner or an experienced developer, F--'s unique philosophy will bring a smile to your face while you code!

### 🎯 **Why F--?**
- **For beginners**: Simple syntax, clear error messages
- **For fun**: Playful design, quirky features
- **For learning**: Open source, well-documented, community-driven

---

## ✨ Features

<div align="center">

| Feature | Description | Example | Status |
|---------|-------------|---------|--------|
| 🖥️ **Super Import** | Import the entire computer, no hardware worries! | `import computer` | ✅ Stable |
| 🔤 **String Interpolation** | Embed variables directly in strings | `$"Hello {name}!"` | ✅ Stable |
| 🧠 **Memory Management** | Automatic memory checking | `memory.memoryleft` | ✅ Stable |
| 📁 **Intuitive File I/O** | File operations made simple | `at "file.txt" { ... }` | ✅ Stable |
| 🚨 **FMM Error Codes** | Professional F Minus Minus error system | `fmm001: syntax error` | ✅ Stable |
| 🔄 **Loops & Conditions** | Control flow made easy | `if (x > 5) { ... }` | 🚧 In Progress |
| 📦 **Package Manager** | Share your F-- code with the world | `import "mylib.f--"` | 🚧 Coming Soon |

</div>

---

## 🎮 Quick Example

Here's a complete F-- program that demonstrates the language's power and simplicity:

```
import computer

start()
{
    // Variable declaration
    name = "F--"
    version = alpha0
   
    luckyNumber = 42
    
    // Print with interpolation
    println($"Hello from {name} v{version}!")
    println($"Memory left: {memory.memoryleft} MB")
    
    // Simple calculation
    result = luckyNumber * 2
    println($"The answer to everything multiplied by 2 is: {result}")
    
    // File operations
    io.cfile("hello"(path "txt"))
    at "hello.txt"
    {
        io.println("Hello file!")
        io.println($"Created by F-- v{version}")
        io.println($"Today's lucky number: {luckyNumber}")
        io.save()
    }
    
    println("✅ File created successfully!")
    
    return(0)  // Success
    end()      // End program
}
```

**Output:**
```
Hello from F-- alpha0
Memory left: 7823 MB
The answer to everything multiplied by 2 is: 84
✅ File created successfully!
```

---

## 🚀 Installation

### 📋 **System Requirements**
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [Git](https://git-scm.com/) (to clone repository)
- **OS**: Windows, Linux, macOS (any platform that supports .NET!)

### 📦 **Option 1: Download from Release (Easiest)**

1. Go to the [Releases](https://github.com/realmg51-cpu/F---Programming-Language/releases) page
2. Download the latest `Fminusminus-{version}.zip`
3. Extract the archive
4. Open terminal/command prompt in the extracted folder

**Windows:**
```cmd
build(Windows).cmd
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--
```

**Linux / macOS:**
```bash
chmod +x build\(Mac\,Linux\).sh
./build\(Mac\,Linux\).sh
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--
```

### 🔧 **Option 2: Clone from GitHub (For Contributors)**

```bash
git clone https://github.com/realmg51-cpu/F---Programming-Language.git
cd F---Programming-Language

# Build the project
dotnet build

# Run your first F-- program
dotnet run --project Compiler/compiler.csproj -- run examples/hello.f--

# View the Abstract Syntax Tree (AST)
dotnet run --project Compiler/compiler.csproj -- ast examples/hello.f--
```

### 📦 **Option 3: Install via NuGet (Coming Soon)**

```bash
# Install as global tool
dotnet tool install --global Fminusminus --version 2.0.0

# Run directly from terminal
fminus run hello.f--
```

### 🐳 **Option 4: Use Docker (For the Adventurous)**

```bash
# Build Docker image
docker build -t fminusminus .

# Run F-- program
docker run --rm -v $(pwd):/app fminusminus run /app/examples/hello.f--
```

---

## 📖 Documentation

### 📚 **Basic Syntax**

| Construct | Syntax | Example |
|-----------|--------|---------|
| **Import** | `import computer` | `import computer` |
| **Entry Point** | `start() { ... }` | `start() { println("Hello!") }` |
| **Variables** | `name = value` | `age = 15` |
| **Print** | `print("text")` | `print("No newline")` |
| **Print Line** | `println("text")` | `println("With newline")` |
| **String Interpolation** | `$"text {var}"` | `$"Hello {name}!"` |
| **Return** | `return(value)` | `return(0)` |
| **End Program** | `end()` | `end()` |

### 📁 **File I/O Operations**

``f--
// Create a file with extension
io.cfile("myfile"(path "txt"))

// Write to file
at "myfile.txt"
{
    io.println("Line 1")
    io.print("No newline here")
    io.println("Line 2 with newline")
}

// Save file
io.save()
// or save to specific path
io.save("C:/myfolder/")
``

### 🧠 **Memory Management**

``f--
memory.memoryleft    // Check available memory
memory.memoryused    // Check used memory
memory.memorytotal   // Check total memory

println($"Free memory: {memory.memoryleft} MB")
println($"Used memory: {memory.memoryused} MB")
println($"Total memory: {memory.memorytotal} MB")
``

### 🚨 **Error Handling**

F-- uses a professional error code system (FMM - F Minus Minus):

| Error Code | Description | Example |
|------------|-------------|---------|
| `fmm001` | Syntax error | Missing semicolon |
| `fmm002` | Undefined variable | Using variable before declaration |
| `fmm003` | Type mismatch | String vs number |
| `fmm004` | File not found | Cannot open file |
| `fmm005` | Memory overflow | Not enough memory |

---

## 🧪 Testing

F-- comes with a comprehensive test suite to ensure everything works as expected:

```bash
# Run all tests
dotnet test

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific test category
dotnet test --filter "Category=Lexer"
dotnet test --filter "Category=Parser"
dotnet test --filter "Category=CodeGen"

# Generate test report
dotnet test --logger "trx;LogFileName=test_results.trx"
```

---

## 📊 Code Coverage

<div align="center">

[![codecov](https://codecov.io/gh/realmg51-cpu/F---Programming-Language/branch/main/graph/badge.svg)](https://codecov.io/gh/realmg51-cpu/F---Programming-Language)

**Current coverage: 0%???** (We're working on it! 😅)

| Coverage Level | Status | Meaning |
|----------------|--------|---------|
| 🔴 < 60% | Poor | Needs urgent testing |
| 🟡 60-75% | Average | Acceptable |
| 🟢 75-85% | Good | Stable |
| 💚 > 85% | Excellent | High quality |

</div>

---

## 🤝 Contributing

### 🎉 **We welcome all contributors!**

Whether you're a seasoned developer or just starting out, your contributions are valuable!

### 📋 **How to Contribute**

1. **🐛 Report bugs**: Create a new [Issue](https://github.com/realmg51-cpu/F---Programming-Language/issues)
2. **💡 Suggest features**: Share ideas through [Issues](https://github.com/realmg51-cpu/F---Programming-Language/issues)
3. **🔧 Submit Pull Request**: Fork the repo and create a [Pull Request](https://github.com/realmg51-cpu/F---Programming-Language/pulls)

### 👨‍💻 **Development Setup**

```bash
# Fork and clone your fork
git clone https://github.com/YOUR_USERNAME/F---Programming-Language.git
cd F---Programming-Language

# Add upstream remote
git remote add upstream https://github.com/realmg51-cpu/F---Programming-Language.git

# Create a feature branch
git checkout -b feature/amazing-feature

# Make your changes and test
dotnet test

# Commit with clear message
git commit -m "Add: amazing feature that does X"

# Push to your fork
git push origin feature/amazing-feature

# Open a Pull Request on GitHub
```

### 📋 **Contribution Guidelines**

- Write clear commit messages
- Add tests for new features
- Update documentation
- Be respectful and kind
- Have fun! 🎉

---

## 📜 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2026 RealMG

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

---

## 👨‍💻 Author

<div align="center">

### **RealMG** - 13-year-old developer

[![GitHub](https://img.shields.io/badge/GitHub-@realmg51--cpu-181717?style=for-the-badge&logo=github)](https://github.com/realmg51-cpu)
[![Email](https://img.shields.io/badge/Email-realmg51%40gmail.com-D14836?style=for-the-badge&logo=gmail)](mailto:realmg51@gmail.com)

**"Passion has no age, creativity has no limits!"**

</div>

---

## 🌟 **Support the Project**

If you find F-- interesting or useful, please consider:

- ⭐ **Star** this repository
- 🔄 **Fork** and develop further
- 📢 **Share** with your friends
- 💬 **Join** the conversation in Issues
- 🤝 **Contribute** with code or ideas

---

## 🎉 **Acknowledgments**

- 🙏 Thanks to the amazing **.NET community**
- 💡 Inspired by the desire to learn and create
- 👥 Special thanks to [@chaunguyen12477-cmyk](https://github.com/chaunguyen12477-cmyk) for being the first contributor!
- 🌍 Everyone who stars, forks, or shares this project

---

## 📊 **Project Stats**

<div align="center">

| Metric | Value |
|--------|-------|
| 📅 Created | March 3, 2026 |
| 🔢 Commits | 86+ |
| 👥 Contributors | 2 |
| ⭐ Stars | Growing... |
| 🍴 Forks | Growing... |

</div>

---

## 🚧 **Roadmap**

- [x] Basic syntax and compiler
- [x] File I/O operations
- [x] Memory management
- [x] Error handling system
- [ ] **v1.0.0-alpha0** (March 8, 2026)
- [ ] Loops and conditions
- [ ] VS Code extension
- [ ] Package manager
- [ ] NuGet package
- [ ] **v1.0.0 stable**

---

<div align="center">

### **Made with ❤️ by a 13-year-old developer**

*Last updated: March 6, 2026*

---

<img src="https://raw.githubusercontent.com/realmg51-cpu/F---Programming-Language/main/docs/footer.png" width="400"/>

**Built with ☕ and curiosity**

</div>
