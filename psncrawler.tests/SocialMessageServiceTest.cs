using System.Collections.Generic;
using Xunit;

namespace psncrawler.tests
{
    public class SocialMessageServiceTest
    {
        [Fact]
        public void TestMessageWhenDefaultLanguageIsProvided()
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>
                {
                    new Playstation.TmdbName() { lang = "some language", name = "wrong name" },
                    new Playstation.TmdbName() { lang = null, name = "correct name" },
                    new Playstation.TmdbName() { lang = "another language", name = "another wrong name" },
                }
            });
            Assert.Equal("The game correct name has been added to the PSN!", message);
        }

        [Fact]
        public void TestMessageWhenNoDefaultLanguageIsProvided()
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>
                {
                    new Playstation.TmdbName() { lang = "some language", name = "right name" },
                    new Playstation.TmdbName() { lang = "another language", name = "another wrong name" },
                }
            });
            Assert.Equal("The game right name has been added to the PSN!", message);
        }

        [Fact]
        public void FailsWhenGameNameListIsEmpty()
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>()
            });
            Assert.Null(message);
        }

        [Fact]
        public void FailsWhenNoGameNameIsFound()
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = null
            });
            Assert.Null(message);
        }

        [Theory]
        [InlineData("EP_CUSA0000", "to the european PSN")]
        [InlineData("UP_CUSA0000", "to the american PSN")]
        [InlineData("JP_CUSA0000", "to the japanese PSN")]
        [InlineData("HP_CUSA0000", "to the asian PSN")]
        [InlineData("IP_CUSA0000", "to the internal PSN")]
        [InlineData("XP_CUSA0000", "to the 'X' PSN")]
        [InlineData(null, "to the PSN")]
        public void TestRegionInMessage(string contentId, string expectedMessage)
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>
                {
                    new Playstation.TmdbName() { name = "ciccio" },
                },
                contentId = contentId
            });

            Assert.Contains(expectedMessage, message);
        }

        [Theory]
        [InlineData("CUSA12345", "CUSA12345")]
        [InlineData("CUSA12345_00", "CUSA12345")]
        public void TestApplicationIdInMessage(string npTitleId, string expectedMessage)
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>
                {
                    new Playstation.TmdbName() { name = "ciccio" }
                },
                npTitleId = npTitleId
            });

            Assert.Equal($"The game ciccio with id {expectedMessage} has been added to the PSN!", message);
        }

        [Theory]
        [InlineData("ps4", "the PS4 PSN")]
        [InlineData("PS4", "the PS4 PSN")]
        [InlineData("SomeConsole", "the SOMECONSOLE PSN")]
        [InlineData("", "the PSN")]
        [InlineData(null, "the PSN")]
        public void TestMessageWithConsole(string console, string expected)
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>
                {
                    new Playstation.TmdbName() { name = "ciccio" }
                },
                console = console
            });

            Assert.Contains(expected, message);
        }

        [Fact]
        public void TestMessageWithEverythingInside()
        {
            var message = SocialMessageService.GetNewGameMessage(new Playstation.Tmdb
            {
                names = new List<Playstation.TmdbName>
                {
                    new Playstation.TmdbName() { name = "ciccio" },
                },
                npTitleId = "CUSA12345",
                contentId = "EP7777_CUSA12345_00-0123456789ABCDEF",
                console = "PS4"
            });

            Assert.Equal("The game ciccio with id CUSA12345 has been added to the PS4 european PSN!", message);
        }
    }
}
