using Poss.Win.Automation.Common.Keys.Enums;
using Poss.Win.Automation.Common.Structs;
using Poss.Win.Automation.Input;
using Xunit;

namespace PoE.Tests;

public class KeyStrokePrecheckTests
{
    [Theory]
    [InlineData("0", VirtualKey.D0, 0x30)]
    [InlineData("1", VirtualKey.D1, 0x31)]
    [InlineData("2", VirtualKey.D2, 0x32)]
    [InlineData("9", VirtualKey.D9, 0x39)]
    public void TryParseKey_digit_strings_map_to_D_keys(string key, VirtualKey expected, ushort expectedVk)
    {
        Assert.True(KeyStroke.TryParseKey(key, out VirtualKey vk));
        Assert.Equal(expected, vk);
        Assert.Equal(expectedVk, (ushort)vk);
        Assert.NotEqual(VirtualKey.LButton, vk);
    }

    [Theory]
    [InlineData("0", VirtualKey.D0, 0x30)]
    [InlineData("1", VirtualKey.D1, 0x31)]
    [InlineData("9", VirtualKey.D9, 0x39)]
    public void TryGetVirtualKeyCode_digit_strings_map_to_top_row_vk(string key, VirtualKey expected, ushort expectedVk)
    {
        Assert.True(KeyStroke.TryGetVirtualKeyCode(key, out ushort vkCode));
        Assert.Equal(expectedVk, vkCode);
        Assert.Equal(expected, (VirtualKey)vkCode);
        Assert.NotEqual((ushort)VirtualKey.LButton, vkCode);
    }

    [Theory]
    [InlineData("D1", VirtualKey.D1)]
    [InlineData("d1", VirtualKey.D1)]
    public void TryParseKey_D_prefix_still_works(string key, VirtualKey expected)
    {
        Assert.True(KeyStroke.TryParseKey(key, out VirtualKey vk));
        Assert.Equal(expected, vk);
    }

    [Theory]
    [InlineData("1 Down")]
    [InlineData("1 down")]
    public void InputSimulator_TryParse_digit_down_resolves_to_D_key_not_LButton(string input)
    {
        Assert.True(InputSimulator.TryParse(input, out KeyStroke stroke));
        Assert.Equal(VirtualKey.D1, stroke.Key);
        Assert.Equal(KeyAction.Down, stroke.Action);
        Assert.NotEqual(VirtualKey.LButton, stroke.Key);
    }

    [Theory]
    [InlineData("1 Down")]
    [InlineData("1")]
    public void KeyStroke_TryParse_digit_input_resolves_to_D_key_not_LButton(string input)
    {
        Assert.True(KeyStroke.TryParse(input, out KeyStroke stroke));
        Assert.Equal(VirtualKey.D1, stroke.Key);
        Assert.NotEqual(VirtualKey.LButton, stroke.Key);
    }

    [Fact]
    public void Digit_one_must_not_parse_as_LButton_via_any_precheck_api()
    {
        Assert.True(KeyStroke.TryParseKey("1", out VirtualKey fromTryParseKey));
        Assert.True(KeyStroke.TryGetVirtualKeyCode("1", out ushort fromVkCode));
        Assert.True(InputSimulator.TryParse("1 Down", out KeyStroke fromInputSimulator));

        Assert.Equal(VirtualKey.D1, fromTryParseKey);
        Assert.Equal(0x31, fromVkCode);
        Assert.Equal(VirtualKey.D1, fromInputSimulator.Key);

        Assert.NotEqual(VirtualKey.LButton, fromTryParseKey);
        Assert.NotEqual((ushort)VirtualKey.LButton, fromVkCode);
        Assert.NotEqual(VirtualKey.LButton, fromInputSimulator.Key);
    }
}
