using Xunit;
using Fminusminus.Utils.Package;
using System;

namespace Fminusminus.Tests
{
    public class MemoryPackageTests
    {
        private readonly MemoryPackage _memory;
        
        public MemoryPackageTests()
        {
            _memory = new MemoryPackage();
            _memory.Initialize();
        }

        [Fact]
        public void TestMemoryInfo()
        {
            // Act
            var total = _memory.CallMethod("GetTotal", Array.Empty<object?>());
            var used = _memory.CallMethod("GetUsed", Array.Empty<object?>());
            var free = _memory.CallMethod("GetFree", Array.Empty<object?>());
            
            // Assert
            Assert.True((long)total! > 0);
            Assert.True((long)used! >= 0);
            Assert.True((long)free! >= 0);
            Assert.Equal((long)total, (long)used + (long)free);
        }

        [Fact]
        public void TestAllocateAndFree()
        {
            // Arrange
            var beforeUsed = (long)_memory.CallMethod("GetUsed", Array.Empty<object?>())!;
            
            // Act - Allocate
            _memory.CallMethod("Allocate", new object?[] { 100 });
            var afterAlloc = (long)_memory.CallMethod("GetUsed", Array.Empty<object?>())!;
            
            // Assert
            Assert.Equal(beforeUsed + 100, afterAlloc);
            
            // Act - Free
            _memory.CallMethod("Free", new object?[] { 50 });
            var afterFree = (long)_memory.CallMethod("GetUsed", Array.Empty<object?>())!;
            
            // Assert
            Assert.Equal(afterAlloc - 50, afterFree);
        }

        [Fact]
        public void TestGC()
        {
            // Arrange
            _memory.CallMethod("Allocate", new object?[] { 200 });
            var afterAlloc = (long)_memory.CallMethod("GetUsed", Array.Empty<object?>())!;
            Assert.True(afterAlloc > 256); // Base is 256
            
            // Act
            _memory.CallMethod("GC", Array.Empty<object?>());
            var afterGC = (long)_memory.CallMethod("GetUsed", Array.Empty<object?>())!;
            
            // Assert - GC resets to base (256)
            Assert.Equal(256, afterGC);
        }
    }
}
