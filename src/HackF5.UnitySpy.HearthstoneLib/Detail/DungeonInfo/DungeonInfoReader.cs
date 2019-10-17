namespace HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail;
    using JetBrains.Annotations;

    internal static class DungeonInfoReader
    {
        public static IFullDungeonInfo GetFullDungeonInfo([NotNull] HearthstoneImage image)
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

            return new FullDungeonInfo
            {
                DungeonRun = BuildDungeonInfo(image, (int)DungeonSaveKey.DUNGEON_RUN, savesMap),
                MonsterHunt = BuildDungeonInfo(image, (int)DungeonSaveKey.MONSTER_HUNT, savesMap),
                RumbleRun = BuildDungeonInfo(image, (int)DungeonSaveKey.RUMBLE_RUN, savesMap),
                DalaranHeist = BuildDungeonInfo(image, (int)DungeonSaveKey.DALARAN_HEIST, savesMap),
                DalaranHeistHeroic = BuildDungeonInfo(image, (int)DungeonSaveKey.DALANRA_HEIST_HEROIC, savesMap),
                TombsOfTerror = BuildDungeonInfo(image, (int)DungeonSaveKey.TOMBS_OF_TERROR, savesMap),
                TombsOfTerrorHeroic = BuildDungeonInfo(image, (int)DungeonSaveKey.TOMBS_OF_TERROR_HEROIC, savesMap),
            };
        }

        private static IDungeonInfo BuildDungeonInfo(HearthstoneImage image, int key, dynamic savesMap)
        {
            var index = GetKeyIndex(savesMap, key);
            if (index == -1)
            {
                return null;
            }

            var runSaveMap = savesMap["valueSlots"][index];
            //var values = runSaveMap["valueSlots"];
            //for (var i = 0; i < values.Length; i++)
            //{
            //    Console.Write("" + i + " ->");
            //    var value = values[i]["_IntValue"];
            //    var size = value["_size"];
            //    var items = value["_items"];
            //    for (var j = 0; j < size; j++)
            //    {
            //        Console.Write(" " + items[j] + ", ");
            //    }
            //    Console.WriteLine();
            //}

            var runFromMemory = new DungeonInfo
            {
                DeckCards = ExtractValues(runSaveMap, (int)DungeonFieldKey.DECK_LIST),
                LootOptionBundles = new List<List<int>>()
                {
                    ExtractValues(runSaveMap, (int)DungeonFieldKey.LOOT_OPTION_1),
                    ExtractValues(runSaveMap, (int)DungeonFieldKey.LOOT_OPTION_2),
                    ExtractValues(runSaveMap, (int)DungeonFieldKey.LOOT_OPTION_3),
                },
                ChosenLoot = ExtractValue(runSaveMap, (int)DungeonFieldKey.CHOSEN_LOOT),
                TreasureOption = ExtractValues(runSaveMap, (int)DungeonFieldKey.TREASURE_OPTION),
                ChosenTreasure = ExtractValue(runSaveMap, (int)DungeonFieldKey.CHOSEN_TREASURE),
                RunActive = ExtractValue(runSaveMap, (int)DungeonFieldKey.RUN_ACTIVE),
                SelectedDeck = ExtractValue(runSaveMap, (int)DungeonFieldKey.SELECTED_DECK),
                StartingTreasure = ExtractValue(runSaveMap, (int)DungeonFieldKey.SELECTED_DECK),
            };
            return EnrichDeck(image, runFromMemory);
        }

        private static IDungeonInfo EnrichDeck(HearthstoneImage image, DungeonInfo runFromMemory)
        {
            return new DungeonInfo
            {
                DeckCards = runFromMemory.DeckCards,
                LootOptionBundles = runFromMemory.LootOptionBundles,
                ChosenLoot = runFromMemory.ChosenLoot,
                TreasureOption = runFromMemory.TreasureOption,
                ChosenTreasure = runFromMemory.ChosenTreasure,
                RunActive = runFromMemory.RunActive,
                SelectedDeck = runFromMemory.SelectedDeck,
                StartingTreasure = runFromMemory.StartingTreasure,
                DeckList = BuildRealDeckList(image, runFromMemory),
            };
        }

        private static IReadOnlyList<int> BuildRealDeckList(HearthstoneImage image, DungeonInfo runFromMemory)
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
                    for (int i = 1; i < chosenBundle.Count; i++)
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
                    for (int i = 0; i < starterDecks.Length; i++)
                    {
                        var deckId = starterDecks[i]["m_ID"];
                        if (deckId == runFromMemory.SelectedDeck)
                        {
                            var topCardId = starterDecks[i]["m_topCardId"];
                            var cardDbf = GetDeckCardDbf(image, topCardId);
                            while (cardDbf != null)
                            {
                                deckList.Add(cardDbf["m_cardId"]);
                                var next = cardDbf["m_nextCardId"];
                                cardDbf = next == 0 ? null : GetDeckCardDbf(image, next);
                            }
                        }
                    }
                }
            }

            return deckList;
        }

        private static dynamic GetDeckCardDbf(HearthstoneImage image, int cardId)
        {
            var cards = image["GameDbf"]["DeckCard"]["m_records"]["_items"];
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i]["m_ID"] == cardId)
                {
                    return cards[i];
                }
            }

            return null;
        }

        private static int ExtractValue(dynamic dungeonMap, int key)
        {
            var keyIndex = GetKeyIndex(dungeonMap, key);
            if (keyIndex == -1)
            {
                return -1;
            }

            var value = dungeonMap["valueSlots"][keyIndex]["_IntValue"];
            var size = value["_size"];
            var items = value["_items"];
            List<int> result = new List<int>();
            for (int i = 0; i < size; i++)
            {
                var item = (int)items[i];
                result.Add(item);
            }

            return result.Count > 0 ? result[0] : -1;
        }

        private static IReadOnlyList<int> ExtractValues(dynamic dungeonMap, int key)
        {
            var keyIndex = GetKeyIndex(dungeonMap, key);
            if (keyIndex == -1)
            {
                return new List<int>();
            }

            var value = dungeonMap["valueSlots"][keyIndex]["_IntValue"];
            var size = value["_size"];
            var items = value["_items"];
            List<int> result = new List<int>();
            for (int i = 0; i < size; i++)
            {
                var item = (int)items[i];
                result.Add(item);
            }

            return result;
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
