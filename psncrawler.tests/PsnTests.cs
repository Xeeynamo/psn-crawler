using psncrawler.Playstation;
using Xunit;

namespace psncrawler.tests
{
    public class PsnTests
    {
        [Theory]
        [InlineData("CUSA00000", "http://tmdb.env.dl.playstation.net/tmdb2/CUSA00000_00_E2F7429044996DEDCA0AF0D69F7E352B3A6090B9/CUSA00000_00.json")]
        public void GetTmdbTest(string titleId, string expectedUrl) =>
            Assert.Contains(expectedUrl, Psn.GetTmdbUrl(new Title(titleId), "env"));
            
        [Theory]
        [InlineData("CUSA00000", "http://gs-sec.ww.env.dl.playstation.net/plo/env/CUSA00000/997fbd6ca3d7b5301253cb4f5b1903ca9dc7f4e1de3c005bd4bb366ef5d99a91/CUSA00000-ver.xml")]
        public void GetUpdateTest(string titleId, string expectedUrl) =>
            Assert.Contains(expectedUrl, Psn.GetUpdateUrl(new Title(titleId), "env"));
    }
}
