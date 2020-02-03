using System;
using Xunit;

namespace psncrawler.tests
{
    public class TitleTests
    {
        [Theory]
        [InlineData("BCXX00000_00", TitleCategory.PS3_BD)]
        [InlineData("BLXX00000_00", TitleCategory.PS3_BD_2)]
        [InlineData("CUXX00000_00", TitleCategory.PS4)]
        [InlineData("NPXX00000_00", TitleCategory.Digital)]
        [InlineData("MRXX00000_00", TitleCategory.PS3MultiRegion)]
        [InlineData("PCXX00000_00", TitleCategory.PSVita)]
        [InlineData("PLXX00000_00", TitleCategory.PS4_BD)]
        [InlineData("SCXX00000_00", TitleCategory.PSLegacy)]
        [InlineData("SLXX00000_00", TitleCategory.PSLegacy_2)]
        [InlineData("UCXX00000_00", TitleCategory.Umd)]
        [InlineData("ULXX00000_00", TitleCategory.Umd_2)]
        [InlineData("VCXX00000_00", TitleCategory.PSVitaCard)]
        [InlineData("VLXX00000_00", TitleCategory.PSVitaCard_2)]
        [InlineData("XCXX00000_00", TitleCategory.PS3Extra)]
        [InlineData("XLXX00000_00", TitleCategory.PS3Extra_2)]
        [InlineData("PEXX00000_00", TitleCategory.PS12_EU)]
        [InlineData("PTXX00000_00", TitleCategory.PS12_JP)]
        [InlineData("PUXX00000_00", TitleCategory.PS12_US)]
        public void Test1(string titleId, TitleCategory expectedCategory)
        {
            var title = new Title(titleId);
            Assert.Equal(expectedCategory, title.Category);
        }
    }
}
