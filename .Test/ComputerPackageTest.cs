using Xunit;
using Fminusminus.Utils.Package;
using System;
using System.IO;

namespace Fminusminus.Tests
{
    public class ComputerPackageTests
    {
        private readonly ComputerPackage _computer;
        
        public ComputerPackageTests()
        {
            _computer = new ComputerPackage();
            _computer.Initialize();
        }

        [Fact]
        public void TestPrintLn()
        {
            // Arrange
            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);
            
            try
            {
                // Act
                _computer.CallMethod("PrintLn", new object?[] { "Hello, F--!" });
                
                // Assert
                Assert.Contains("Hello, F--!", sw.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void TestVariableOperations()
        {
            // Act - Set variable
            _computer.CallMethod("SetVar", new object?[] { "name", "F--" });
            _computer.CallMethod("SetVar", new object?[] { "version", 2.0 });
            
            // Assert - Get variable
            var name = _computer.CallMethod("GetVar", new object?[] { "name" });
            var version = _computer.CallMethod("GetVar", new object?[] { "version" });
            
            Assert.Equal("F--", name);
            Assert.Equal(2.0, version);
            
            // Test HasVar
            var hasName = _computer.CallMethod("HasVar", new object?[] { "name" });
            var hasAge = _computer.CallMethod("HasVar", new object?[] { "age" });
            
            Assert.True((bool)hasName!);
            Assert.False((bool)hasAge!);
        }

        [Fact]
        public void TestTypeConversion()
        {
            // Act & Assert - ToInt
            var int1 = _computer.CallMethod("ToInt", new object?[] { "123" });
            var int2 = _computer.CallMethod("ToInt", new object?[] { 45.67 });
            var int3 = _computer.CallMethod("ToInt", new object?[] { "abc" });
            
            Assert.Equal(123, int1);
            Assert.Equal(45, int2);
            Assert.Equal(0, int3);
            
            // Act & Assert - ToBool
            var bool1 = _computer.CallMethod("ToBool", new object?[] { true });
            var bool2 = _computer.CallMethod("ToBool", new object?[] { 1 });
            var bool3 = _computer.CallMethod("ToBool", new object?[] { 0 });
            var bool4 = _computer.CallMethod("ToBool", new object?[] { "hello" });
            var bool5 = _computer.CallMethod("ToBool", new object?[] { "" });
            
            Assert.True((bool)bool1!);
            Assert.True((bool)bool2!);
            Assert.False((bool)bool3!);
            Assert.True((bool)bool4!);
            Assert.False((bool)bool5!);
        }

        [Fact]
        public void TestSystemInfo()
        {
            // Act
            var os = _computer.CallMethod("GetOS", Array.Empty<object?>());
            var machine = _computer.CallMethod("GetMachineName", Array.Empty<object?>());
            var cpu = _computer.CallMethod("GetProcessorCount", Array.Empty<object?>());
            var info = _computer.CallMethod("GetInfo", Array.Empty<object?>());
            
            // Assert
            Assert.NotNull(os);
            Assert.NotNull(machine);
            Assert.True((int)cpu! > 0);
            Assert.NotNull(info);
            Assert.Contains("SYSTEM INFORMATION", info!.ToString()!);
        }
    }
}
