using System.Collections.Generic;

namespace TPCA.Archipelago
{
    internal class ArchipelagoGameState
    {
        public bool IsValid;

        public string Uri;
        public string PlayerName;
        public string Password;
        public string Seed;

        public int ItemIndex { get; set; }

        public SlotData SlotData;

        public List<long> CheckedLocations { get; set; } = [];

        public ArchipelagoGameState(string uri, string slotName, string password)
        {
            Uri = uri;
            PlayerName = slotName;
            Password = password;
        }

        public void UpdateConnection(string uri, string slotName, string password)
        {
            Uri = uri;
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
