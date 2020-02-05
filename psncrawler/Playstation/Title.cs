using System;
using System.Collections.Generic;
using System.Linq;

namespace psncrawler.Playstation
{
    public enum TitleCategory
    {
        Unknown,
        PS3_BD,
        PS3_BD_2,
        PS4,
        Digital,
        PS3MultiRegion,
        PSVita,
        PS4_BD,
        PSLegacy,
        PSLegacy_2,
        Umd,
        Umd_2,
        PSVitaCard,
        PSVitaCard_2,
        PS3Extra,
        PS3Extra_2,
        PS12_EU,
        PS12_JP,
        PS12_US,
    }

    public enum TitleRegion
    {
        Unknown,
        Japan,
        Usa,
        Europe,
        Asia,
        Internal,
    }

    public class Title
    {
        private static readonly Dictionary<TitleCategory, string> CategoryMap = new Dictionary<TitleCategory, string>
        {
            [TitleCategory.PS3_BD] = "BC",
            [TitleCategory.PS3_BD_2] = "BL",
            [TitleCategory.PS4] = "CU",
            [TitleCategory.Digital] = "NP",
            [TitleCategory.PS3MultiRegion] = "MR",
            [TitleCategory.PSVita] = "PC",
            [TitleCategory.PS4_BD] = "PL",
            [TitleCategory.PSLegacy] = "SC",
            [TitleCategory.PSLegacy_2] = "SL",
            [TitleCategory.Umd] = "UC",
            [TitleCategory.Umd_2] = "UL",
            [TitleCategory.PSVitaCard] = "VC",
            [TitleCategory.PSVitaCard_2] = "VL",
            [TitleCategory.PS3Extra] = "XC",
            [TitleCategory.PS3Extra_2] = "XL",
            [TitleCategory.PS12_EU] = "PE",
            [TitleCategory.PS12_JP] = "PT",
            [TitleCategory.PS12_US] = "PU",
        };

        private static readonly Dictionary<TitleRegion, char> RegionMap = new Dictionary<TitleRegion, char>
        {
            [TitleRegion.Japan] = 'J',
            [TitleRegion.Usa] = 'U',
            [TitleRegion.Europe] = 'E',
            [TitleRegion.Asia] = 'H',
            [TitleRegion.Internal] = 'I',
        };

        private static readonly Dictionary<string, TitleCategory> CategoryReversedMap = CategoryMap
            .ToDictionary(x => x.Value, x => x.Key);

        private static readonly Dictionary<char, TitleRegion> RegionReversedMap = RegionMap
            .ToDictionary(x => x.Value, x => x.Key);

        public Title(string titleId)
        {
            Category = CategoryReversedMap[titleId.Substring(0, 2)];
            //Region = RegionReversedMap[titleId[3]];
            Id = titleId.Substring(4, 5);
        }

        public TitleCategory Category { get; }
        public TitleRegion Region { get; }
        public string Id { get; }

        public override string ToString()
        {
            switch (Category)
            {
                case TitleCategory.PS4:
                    return $"{CategoryMap[Category]}SA{Id}";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}