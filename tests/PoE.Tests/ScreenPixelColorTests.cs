using PoE.dlls.InteropServices;
using System.Drawing;
using Xunit;

namespace PoE.Tests;

public sealed class ScreenPixelColorTests
{
    [Theory]
    [InlineData(0x00000000u, 0, 0, 0)]
    [InlineData(0x00F9D799u, 0x99, 0xD7, 0xF9)]
    public void FromGdiPixel_unpacks_bgr(uint pixel, int red, int green, int blue)
    {
        Color color = ScreenPixelColor.FromGdiPixel(pixel);
        Assert.Equal(red, color.R);
        Assert.Equal(green, color.G);
        Assert.Equal(blue, color.B);
    }
}
