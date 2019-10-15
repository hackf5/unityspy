namespace HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib.Detail;
    using JetBrains.Annotations;

    internal static class DungeonInfoReader {

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
                DungeonRun = BuildDungeonInfo((int)DungeonSaveKey.DUNGEON_RUN, savesMap),
                MonsterHunt = BuildDungeonInfo((int)DungeonSaveKey.MONSTER_HUNT, savesMap),
                RumbleRun = BuildDungeonInfo((int)DungeonSaveKey.RUMBLE_RUN, savesMap),
                DalaranHeist = BuildDungeonInfo((int)DungeonSaveKey.DALARAN_HEIST, savesMap),
                DalaranHeistHeroic = BuildDungeonInfo((int)DungeonSaveKey.DALANRA_HEIST_HEROIC, savesMap),
                TombsOfTerror = BuildDungeonInfo((int)DungeonSaveKey.TOMBS_OF_TERROR, savesMap),
                TombsOfTerrorHeroic = BuildDungeonInfo((int)DungeonSaveKey.TOMBS_OF_TERROR_HEROIC, savesMap),
            };
        }

        private static DungeonInfo BuildDungeonInfo(int key, dynamic savesMap)
        {
            var index = GetKeyIndex(savesMap, key);
            if (index == -1)
            {
                return null;
            }
            return new DungeonInfo
            {
                DeckList = ExtractValues(savesMap["valueSlots"][index], (int)DungeonFieldKey.DECK_LIST),
            };
        }

        private static IReadOnlyList<int> ExtractValues(dynamic dungeonMap, int key)
        {
            var keyIndex = GetKeyIndex(dungeonMap, key);
            var value = dungeonMap["valueSlots"][keyIndex]["_IntValue"];
            var size = value["_size"];
            var items = value["_items"];
            List<int> result = new List<int>();
            for (int i = 0; i < size; i++)
            {
                var cardDbfId = (int)items[i];
                result.Add(cardDbfId);
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
