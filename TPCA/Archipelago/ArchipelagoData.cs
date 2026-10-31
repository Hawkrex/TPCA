using System.Collections.Generic;

namespace TPCA.Archipelago
{
    internal class ArchipelagoData
    {
        public string ModName = PluginInfo.PLUGIN_NAME;

        public string Guid { get; set; } // RoomInfo -> seed_name

        public string Host { get; set; } = "localhost:38281";

        public string PlayerName { get; set; } = "Player1";

        public string Password { get; set; }

        public SlotData SlotData { get; set; }

        public Dictionary<string, ApItemInfo> ItemByLocations { get; set; }
    }
}
