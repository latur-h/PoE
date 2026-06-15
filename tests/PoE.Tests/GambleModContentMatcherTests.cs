using PoE.dlls.Gamble;
using Xunit;

namespace PoE.Tests
{
    public class GambleModContentMatcherTests
    {
        [Fact]
        public void Hash_in_rule_matches_rolled_percent_value()
        {
            Assert.True(GambleModContentMatcher.IsContentMatch(
                "#% increased Physical Damage",
                "65% increased Physical Damage"));
        }

        [Fact]
        public void Hash_matches_after_range_normalization()
        {
            Assert.True(GambleModContentMatcher.IsContentMatch(
                "#% more Monster Life",
                "44(40-49)% more Monster Life"));
        }

        [Fact]
        public void Normalize_keeps_highest_non_range_value()
        {
            string normalized = GambleModContentMatcher.NormalizeItemModContent("29(25-30)% more Monster Life");
            Assert.Equal("29% more Monster Life", normalized);
        }

        [Fact]
        public void Normalize_handles_negative_range_values()
        {
            string normalized = GambleModContentMatcher.NormalizeItemModContent("-9(-12--9)% to all maximum Resistances");
            Assert.Equal("-9% to all maximum Resistances", normalized);
        }

        [Fact]
        public void Percent_stays_literal_in_pattern()
        {
            Assert.True(GambleModContentMatcher.IsContentMatch("#%", "18% of Physical Damage"));
        }
    }
}
