// ReSharper disable StringLiteralTypo
namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    using System;

    internal static class BattlegroundsInfoReader
    {
        public static IBattlegroundsInfo ReadBattlegroundsInfo(HearthstoneImage image)
        {
            var netCacheValues = image.GetService("NetCache")?["m_netCache"]?["valueSlots"];
            if (netCacheValues == null)
            {
                return null;
            }

            var battlegroundsInfo = new BattlegroundsInfo();
            foreach (var netCache in netCacheValues)
            {
                if (netCache?.TypeDefinition.Name != "NetCacheBaconRatingInfo")
                {
                    continue;
                }
                battlegroundsInfo.Rating = netCache["<Rating>k__BackingField"] ?? -1;
                battlegroundsInfo.PreviousRating = netCache["<PreviousBaconRatingInfo>k__BackingField"]?["<Rating>k__BackingField"] ?? -1;
            }

            return battlegroundsInfo;
        }
    }
}