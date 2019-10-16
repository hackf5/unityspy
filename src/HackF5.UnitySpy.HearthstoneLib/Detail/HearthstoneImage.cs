namespace HackF5.UnitySpy.HearthstoneLib.Detail
{
    internal class HearthstoneImage
    {
        private readonly IAssemblyImage image;

        public HearthstoneImage(IAssemblyImage image)
        {
            this.image = image;
        }

        public dynamic this[string fullTypeName] => this.image[fullTypeName];

        public dynamic GetService(string name)
        {
            var dynamicServices = this.image?["HearthstoneServices"]["s_dynamicServices"];
            var staticServices = this.image?["HearthstoneServices"]["s_services"];
            var services = dynamicServices ?? staticServices;

            if (services == null)
            {
                return null;
            }

            var serviceItems = services["m_services"]["_items"];

            var i = 0;
            foreach (var service in serviceItems)
            {
                if (service?["<ServiceTypeName>k__BackingField"] == name)
                {
                    var result = service["<Service>k__BackingField"];
                    return result;
                }

                i++;
            }

            return null;
        }
    }
}