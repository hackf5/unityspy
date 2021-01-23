namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Match;
    using JetBrains.Annotations;

    internal static class ActiveDeckReader
    {
        public static IDeck ReadActiveDeck([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var gameState = image["GameState"]["s_instance"];
            if (gameState == null)
            {
                return null;
            }

            var matchInfo = MatchInfoReader.ReadMatchInfo(image);
            switch (matchInfo.GameType)
            {
                case GameType.GT_ARENA: return GetArenaDeck(image);
                case GameType.GT_CASUAL: return GetCasualDeck(image);
                case GameType.GT_RANKED: return GetRankedDeck(image);
                case GameType.GT_VS_AI: return GetSoloDeck(image, matchInfo.MissionId);
                case GameType.GT_VS_FRIEND: return GetFriendlyDeck(image);
                default: return null;
            }
        }

        private static IDeck GetArenaDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetCasualDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetRankedDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetSoloDeck(HearthstoneImage image, int missionId)
        {
            Console.WriteLine("Getting solo deck for missionId: " + missionId);
            var deckList = GetSoloDeckList(image, missionId);
            return new Deck
            {
                DeckList = deckList,
            };
        }

        private static IReadOnlyList<int> GetSoloDeckList(HearthstoneImage image, int missionId)
        {
            var dungeonInfo = DungeonInfoReader.ReadCollection(image);
            switch (missionId)
            {
                case 2663:
                    return dungeonInfo?[DungeonKey.DungeonRun]?.DeckList;
                case 2706:
                case 2821:
                    return dungeonInfo?[DungeonKey.MonsterHunt]?.DeckList;
                case 2890:
                    return dungeonInfo?[DungeonKey.RumbleRun]?.DeckList;
                case 3005:
                case 3188:
                case 3189:
                case 3190:
                case 3191:
                case 3236:
                    return dungeonInfo?[DungeonKey.DalaranHeist]?.DeckList;
                case 3328:
                case 3329:
                case 3330:
                case 3331:
                case 3332:
                case 3359:
                    return dungeonInfo?[DungeonKey.DalaranHeistHeroic]?.DeckList;
                case 3428:
                case 3429:
                case 3430:
                case 3431:
                case 3432:
                case 3438:
                    return dungeonInfo?[DungeonKey.TombsOfTerror]?.DeckList;
                case 3433:
                case 3434:
                case 3435:
                case 3436:
                case 3437:
                case 3439:
                    return dungeonInfo?[DungeonKey.TombsOfTerrorHeroic]?.DeckList;
            }

            Console.WriteLine($"Unsupported scenario id: {missionId}.");
            return null;
        }

        private static IDeck GetFriendlyDeck(HearthstoneImage image)
        {
            return null;
        }
    }
}
