namespace HackF5.UnitySpy.HearthstoneLib
{
    using System;

    public class HearthstoneImage
    {
        private IAssemblyImage image;

        public HearthstoneImage(IAssemblyImage image)
        {
            this.image = image;
        }

        public dynamic this[string fullTypeName] { 
            get {
                return image[fullTypeName];
            }
        }

        public dynamic GetService(string name)
        {
            var dynamicServices = image?["HearthstoneServices"]["s_dynamicServices"];
            var staticServices = image?["HearthstoneServices"]["s_services"];
            var services = dynamicServices != null ? dynamicServices : staticServices;

            var serviceItems = services["m_services"]["_items"];

            int i = 0; ;
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
