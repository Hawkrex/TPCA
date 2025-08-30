namespace TPCA_Debug.JsonModels
{
    internal class ArchipelagoItem
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public string Classification { get; private set; }

        public ArchipelagoItem(string name, int id, string classification)
        {
            Name = name;
            Id = id;
            Classification = classification;
        }
    }
}
