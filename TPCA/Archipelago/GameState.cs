using System.Collections.Generic;
using TPCA.Core;

namespace TPCA.Archipelago
{
    internal class GameState
    {
        public string Host;
        public string PlayerName;
        public string Password;
        public string Seed;

        public ArchipelagoSaveDatas SaveDatas { get; set; } = new ArchipelagoSaveDatas();

        public SlotData SlotData;

        public List<long> CheckedLocations { get; set; } = [];

        public Dictionary<string, ApItemInfo> ItemByLocations { get; set; }

        public GameState(string host, string slotName, string password)
        {
            Host = host;
            PlayerName = slotName;
            Password = password;
        }

        public bool SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
        {
            SlotData = new SlotData(roomSlotData);
            Seed = roomSeed;
            return GameManager.ConnectSave();
        }
    }
}
