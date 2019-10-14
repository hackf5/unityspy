

namespace HackF5.UnitySpy.HearthstoneLib
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;

    public class MindVision
    {
        private IAssemblyImage image;

        public MindVision()
        {
            var process = Process.GetProcessesByName("Hearthstone").FirstOrDefault();
            this.image = AssemblyImageFactory.Create(process.Id);
        }

        public List<CollectionCard> GetCollection()
        {
            List<CollectionCard> collectionCards = new List<CollectionCard>();
            var collectibleCards = image["CollectionManager"]["s_instance"]?["m_collectibleCards"];
            if (collectibleCards != null)
            {
                var items = collectibleCards["_items"];
                int size = collectibleCards["_size"];
                for (var i = 0; i < size; i++)
                {
                    string cardId = items[i]["m_EntityDef"]["m_cardIdInternal"];
                    if (string.IsNullOrEmpty(cardId))
                    {
                        continue;
                    }
                    int count = items[i]["<OwnedCount>k__BackingField"];
                    int premium = items[i]["m_PremiumType"];
                    var card = collectionCards.Where(existingCard => existingCard.CardId == cardId).FirstOrDefault();
                    if (card == null)
                    {
                        card = new CollectionCard
                        {
                            CardId = cardId
                        };
                        collectionCards.Add(card);
                    }
                    if (premium == 1)
                    {
                        card.PremiumCount = count;
                    } 
                    else
                    {
                        card.Count = count;
                    }
                }
            }
            return collectionCards;
        }
    }
}
