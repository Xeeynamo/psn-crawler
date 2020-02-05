using System;
using psncrawler.Playstation;
using Xunit;

namespace psncrawler.tests
{
    public class TitleTests
    {
        [Theory]
        [InlineData("BCUX00000", TitleCategory.PS3_BD)]
        [InlineData("BLUX00000", TitleCategory.PS3_BD_2)]
        [InlineData("CUUX00000", TitleCategory.PS4)]
        [InlineData("NPUX00000", TitleCategory.Digital)]
        [InlineData("MRUX00000", TitleCategory.PS3MultiRegion)]
        [InlineData("PCUX00000", TitleCategory.PSVita)]
        [InlineData("PLUX00000", TitleCategory.PS4_BD)]
        [InlineData("SCUX00000", TitleCategory.PSLegacy)]
        [InlineData("SLUX00000", TitleCategory.PSLegacy_2)]
        [InlineData("UCUX00000", TitleCategory.Umd)]
        [InlineData("ULUX00000", TitleCategory.Umd_2)]
        [InlineData("VCUX00000", TitleCategory.PSVitaCard)]
        [InlineData("VLUX00000", TitleCategory.PSVitaCard_2)]
        [InlineData("XCUX00000", TitleCategory.PS3Extra)]
        [InlineData("XLUX00000", TitleCategory.PS3Extra_2)]
        [InlineData("PEUX00000", TitleCategory.PS12_EU)]
        [InlineData("PTUX00000", TitleCategory.PS12_JP)]
        [InlineData("PUUX00000", TitleCategory.PS12_US)]
        public void CategoryTest(string titleId, TitleCategory expectedCategory) =>
            Assert.Equal(expectedCategory, new Title(titleId).Category);

        [Theory]
        [InlineData("SLES12345", "12345")]
        [InlineData("CUSAZZZZZ", "ZZZZZ")]
        public void IdTest(string titleId, string expectedId) =>
            Assert.Equal(expectedId, new Title(titleId).Id);

        [Theory]
        [InlineData("SLES12345")]
        [InlineData("CUSAZZZZZ")]
        public void ToStringTest(string titleId) =>
            Assert.Equal(titleId, new Title(titleId).ToString());
    }
}
