using Xunit;
using Fminusminus.Utils.Package;
using System;
using System.IO;

namespace Fminusminus.Tests
{
    public class IOPackageTests : IDisposable
    {
        private readonly IOPackage _io;
        private readonly string _testFile = "test-io.txt";
        
        public IOPackageTests()
        {
            _io = new IOPackage();
            _io.Initialize();
            
            // Clean up any existing test file
            if (File.Exists(_testFile))
                File.Delete(_testFile);
        }

        public void Dispose()
        {
            // Clean up after tests
            if (File.Exists(_testFile))
                File.Delete(_testFile);
        }

        [Fact]
        public void TestCreateFile()
        {
            // Act
            _io.CallMethod("CreateFile", new object?[] { _testFile });
            
            // Assert
            Assert.True(File.Exists(_testFile));
        }

        [Fact]
        public void TestWriteAndReadFile()
        {
            // Arrange
            var content = new[] { "Line 1", "Line 2", "Line 3" };
            
            // Act - Write
            _io.CallMethod("CreateFile", new object?[] { _testFile });
            _io.CallMethod("BeginWrite", new object?[] { _testFile });
            
            foreach (var line in content)
            {
                _io.CallMethod("WriteLine", new object?[] { line });
            }
            
            _io.CallMethod("EndWrite", Array.Empty<object?>());
            
            // Assert - File exists
            Assert.True(File.Exists(_testFile));
            
            // Read and verify
            var lines = File.ReadAllLines(_testFile);
            Assert.Equal(content.Length, lines.Length);
            Assert.Equal(content[0], lines[0]);
            Assert.Equal(content[1], lines[1]);
            Assert.Equal(content[2], lines[2]);
        }

        [Fact]
        public void TestFileExists()
        {
            // Arrange
            _io.CallMethod("CreateFile", new object?[] { _testFile });
            
            // Act
            var exists = _io.CallMethod("FileExists", new object?[] { _testFile });
            var notExists = _io.CallMethod("FileExists", new object?[] { "nonexistent.txt" });
            
            // Assert
            Assert.True((bool)exists!);
            Assert.False((bool)notExists!);
        }

        [Fact]
        public void TestDeleteFile()
        {
            // Arrange
            _io.CallMethod("CreateFile", new object?[] { _testFile });
            Assert.True(File.Exists(_testFile));
            
            // Act
            _io.CallMethod("DeleteFile", new object?[] { _testFile });
            
            // Assert
            Assert.False(File.Exists(_testFile));
        }
    }
}
