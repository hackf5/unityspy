namespace HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class DungeonInfoReader
    {
        public static IDungeonInfoCollection ReadCollection([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var savesMap = image["GameSaveDataManager"]?["s_instance"]?["m_gameSaveDataMapByKey"];
            if (savesMap == null)
            {
                return null;
            }

            var dictionary = new Dictionary<DungeonKey, IDungeonInfo>();
            foreach (DungeonKey key in Enum.GetValues(typeof(DungeonKey)))
            {
                dictionary.Add(
                    key,
                    DungeonInfoReader.BuildDungeonInfo(image, key, savesMap));
            }

            return new DungeonInfoCollection(dictionary);
        }

        private static IDungeonInfo BuildDungeonInfo(HearthstoneImage image, DungeonKey key, dynamic savesMap)
        {
            var index = DungeonInfoReader.GetKeyIndex(savesMap, (int)key);
            if (index == -1)
            {
                return null;
            }

            var dungeonMap = savesMap["valueSlots"][index];
            var dungeonInfo = new DungeonInfo
            {
                Key = key,
                DeckCards = DungeonInfoReader.ExtractValues(dungeonMap, (int)DungeonFieldKey.DeckList),
                LootOptionBundles = new List<List<int>>
                {
                    DungeonInfoReader.ExtractValues(dungeonMap, (int)DungeonFieldKey.LootOption1),
                    DungeonInfoReader.ExtractValues(dungeonMap, (int)DungeonFieldKey.LootOption2),
                    DungeonInfoReader.ExtractValues(dungeonMap, (int)DungeonFieldKey.LootOption3),
                },
                ChosenLoot = DungeonInfoReader.ExtractValue(dungeonMap, (int)DungeonFieldKey.ChosenLoot),
                TreasureOption = DungeonInfoReader.ExtractValues(dungeonMap, (int)DungeonFieldKey.TreasureOption),
                ChosenTreasure = DungeonInfoReader.ExtractValue(dungeonMap, (int)DungeonFieldKey.ChosenTreasure),
                RunActive = DungeonInfoReader.ExtractValue(dungeonMap, (int)DungeonFieldKey.RunActive),
                SelectedDeck = DungeonInfoReader.ExtractValue(dungeonMap, (int)DungeonFieldKey.SelectedDeck),
                StartingTreasure = DungeonInfoReader.ExtractValue(dungeonMap, (int)DungeonFieldKey.StartingTreasure),
            };
            dungeonInfo.DeckList = DungeonInfoReader.BuildRealDeckList(image, dungeonInfo);

            return dungeonInfo;
        }

        private static IReadOnlyList<int> BuildRealDeckList(HearthstoneImage image, IDungeonInfo runFromMemory)
        {
            var deckList = new List<int>();

            // The current run is in progress, which means the value held in the DeckCards
            // field is the aggregation of the cards picked in the previous steps
            // TODO: how to handle card changed / removed by Bob?
            if (runFromMemory.RunActive == 1)
            {
                deckList = runFromMemory.DeckCards.ToList();
                if (runFromMemory.ChosenLoot > 0)
                {
                    // index is 1-based
                    var chosenBundle = runFromMemory.LootOptionBundles[runFromMemory.ChosenLoot - 1];

                    // First card is the name of the bundle
                    for (var i = 1; i < chosenBundle.Count; i++)
                    {
                        deckList.Add(chosenBundle[i]);
                    }
                }

                if (runFromMemory.ChosenTreasure > 0)
                {
                    deckList.Add(runFromMemory.TreasureOption[runFromMemory.ChosenTreasure - 1]);
                }
            }
            else
            {
                if (runFromMemory.SelectedDeck > 0)
                {
                    deckList.Add(runFromMemory.StartingTreasure);
                    var dbf = image["GameDbf"];
                    var starterDecks = dbf["Deck"]["m_records"]["_items"];
                    for (var i = 0; i < starterDecks.Length; i++)
                    {
                        var deckId = starterDecks[i]["m_ID"];
                        if (deckId == runFromMemory.SelectedDeck)
                        {
                            var topCardId = starterDecks[i]["m_topCardId"];
                            var cardDbf = DungeonInfoReader.GetDeckCardDbf(image, topCardId);
                            while (cardDbf != null)
                            {
                                deckList.Add(cardDbf["m_cardId"]);
                                var next = cardDbf["m_nextCardId"];
                                cardDbf = next == 0 ? null : DungeonInfoReader.GetDeckCardDbf(image, next);
                            }
                        }
                    }
                }
            }

            // Some cards can be set to 0, when they are removed by Bob for instance
            return deckList.Where(id => id > 0).ToArray();
        }

        private static int ExtractValue(dynamic dungeonMap, int key)
        {
            var keyIndex = DungeonInfoReader.GetKeyIndex(dungeonMap, key);
            if (keyIndex == -1)
            {
                return -1;
            }

            var value = dungeonMap["valueSlots"][keyIndex]["_IntValue"];
            var size = value["_size"];
            var items = value["_items"];

            return size > 0 ? (int)items[0] : -1;
        }

        private static IReadOnlyList<int> ExtractValues(dynamic dungeonMap, int key)
        {
            var keyIndex = DungeonInfoReader.GetKeyIndex(dungeonMap, key);
            var result = new List<int>();
            if (keyIndex == -1)
            {
                return result;
            }

            var value = dungeonMap["valueSlots"][keyIndex]["_IntValue"];
            var size = value["_size"];
            var items = value["_items"];
            for (var i = 0; i < size; i++)
            {
                var item = (int)items[i];
                result.Add(item);
            }

            return result;
        }

        private static dynamic GetDeckCardDbf(HearthstoneImage image, int cardId)
        {
            var cards = image["GameDbf"]["DeckCard"]["m_records"]["_items"];
            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i]["m_ID"] == cardId)
                {
                    return cards[i];
                }
            }

            return null;
        }

        private static int GetKeyIndex(dynamic map, int key)
        {
            var keys = map["keySlots"];
            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i] == key)
                {
                    return i;
                }
            }

            return -1;
        }

    }
}
