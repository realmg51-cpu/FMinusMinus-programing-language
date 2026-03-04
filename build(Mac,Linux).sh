#!/bin/bash
echo "🔨 Building F-- Compiler..."
dotnet restore
dotnet build -c Release
echo "✅ Build complete!"
echo "📦 Run: dotnet run -- Compiler/Program.cs examples/hello.f--"
