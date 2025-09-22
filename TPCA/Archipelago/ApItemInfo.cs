using Archipelago.MultiClient.Net.Enums;

namespace TPCA.Archipelago
{
    internal class ApItemInfo
    {
        public string Name { get; set; }
        public ItemFlags Flags { get; set; }
        public string PlayerName { get; set; }
        public int Index { get; set; }
        public bool IsLocal { get; set; }
        public bool IsTpcItem { get; set; }
    }
}
