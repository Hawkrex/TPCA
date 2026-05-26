using System.Collections.Generic;
using TPCA.Archipelago;

namespace TPCA
{
    internal class ArchipelagoSaveDatas
    {
        public string ModName = PluginInfo.PLUGIN_NAME;

        public string Guid { get; set; }

        public string Host { get; set; }

        public string PlayerName { get; set; }

        public string Password { get; set; }

        public Dictionary<string, ApItemInfo> Locations { get; set; }
    }
}
