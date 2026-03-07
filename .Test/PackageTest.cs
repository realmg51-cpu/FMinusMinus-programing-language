using Xunit;
using Fminusminus;
using Fminusminus.Utils.Package;
using System;
using System.IO;

namespace Fminusminus.Tests
{
    public class PackageTests
    {
        [Fact]
        public void TestPackageManager_LoadComputerPackage()
        {
            // Arrange
            var pm = PackageManager.Instance;
            pm.ClearPackages();
            
            // Act
            var computer = pm.ImportPackage("computer");
            
            // Assert
            Assert.NotNull(computer);
            Assert.Equal("computer", computer.Name);
            Assert.Equal("2.0.0", computer.Version);
            Assert.True(computer.HasMethod("PrintLn"));
            Assert.True(computer.HasMethod("SetVar"));
            Assert.True(computer.HasMethod("GetInfo"));
        }

        [Fact]
        public void TestPackageManager_LoadMultiplePackages()
        {
            // Arrange
            var pm = PackageManager.Instance;
            pm.ClearPackages();
            
            // Act
            var computer = pm.ImportPackage("computer");
            var io = pm.ImportPackage("io");
            var memory = pm.ImportPackage("memory");
            var dict = pm.ImportPackage("dictionary");
            
            // Assert
            Assert.NotNull(computer);
            Assert.NotNull(io);
            Assert.NotNull(memory);
            Assert.NotNull(dict);
            
            Assert.Equal(4, pm.GetLoadedPackages().Count());
        }

        [Fact]
        public void TestPackageManager_PackageNotFound()
        {
            // Arrange
            var pm = PackageManager.Instance;
            pm.ClearPackages();
            
            // Act
            var notFound = pm.ImportPackage("nonexistent");
            
            // Assert
            Assert.Null(notFound);
        }
    }
}
