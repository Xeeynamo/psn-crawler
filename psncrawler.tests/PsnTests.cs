using System;
using Xunit;

namespace psncrawler.tests
{
    public class PsnTests
    {
        [Theory]
        [InlineData("CUSA12025_00", "something")]
        public void Test1(string titleId, string expectedUrl)
        {
            var url = Psn.GetUrl(titleId);
            Assert.Equal(expectedUrl, url);
        }
    }
}
