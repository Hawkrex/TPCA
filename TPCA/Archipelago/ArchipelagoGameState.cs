using System.Collections.Generic;

namespace TPCA.Archipelago
{
    internal class ArchipelagoGameState
    {
        public bool IsValid;

        public string Host;
        public string PlayerName;
        public string Password;
        public string Seed;

        public int ItemIndex { get; set; }

        public ArchipelagoSaveInfos SaveInfos { get; set; }

        public SlotData SlotData;

        public List<long> CheckedLocations { get; set; } = [];

        public Dictionary<string, ApItemInfo> ItemByLocations { get; set; }

        public ArchipelagoGameState(string host, string slotName, string password)
        {
            Host = host;
            PlayerName = slotName;
            Password = password;
        }

        public void UpdateConnection(string host, string slotName, string password)
        {
            Host = host;
            PlayerName = slotName;
            Password = password;
        }

        public void ClearConnection()
        {
            IsValid = false;

            Seed = string.Empty;
            SlotData = null;
        }

        public bool SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
        {
            SlotData = new SlotData(roomSlotData);
            Seed = roomSeed;
            return GameManager.ConnectSave();
        }

        internal void ClearSave()
        {
            IsValid = false;

            ItemIndex = 0;
        }
    }
}
