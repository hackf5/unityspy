

namespace HackF5.UnitySpy.HearthstoneLib
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Collection;

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
            return new CollectionReader(image).GetCollection();
        }


    }
}
