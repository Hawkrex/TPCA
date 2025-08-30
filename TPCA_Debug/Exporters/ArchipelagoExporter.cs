using Newtonsoft.Json;
using SpaceCraft;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPCA_Debug.JsonModels;
using UnityEngine.UIElements;

namespace TPCA_Debug.Exporters
{
    internal static class ArchipelagoExporter
    {
        private static int nextArchipelagoItemId = 1;

        internal static void ExportGroupsDatas(List<Group> allGroups)
        {
            ExportItemTable(allGroups);
        }

        private static void ExportItemTable(List<Group> allGroups)
        {
            var archipelagoItems = new List<ArchipelagoItem>
            {
                new ArchipelagoItem("NoItem", nextArchipelagoItemId, "filler")
            };
            archipelagoItems.AddRange(allGroups
                .Select(x => x.GetGroupData())
                .Where(x => !x.unlockInPlanets.Any() && (x.unlockingValue != 0 || x.terraformStageUnlock != null)) // Prime and blueprint tree item only
                .Select(CreateArchipelagoItem));

            string json = JsonConvert.SerializeObject(archipelagoItems, Formatting.Indented);
            
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("ITEMS_JSON = ");
            stringBuilder.Append(json);

            //write string to file
            System.IO.File.WriteAllText(@"./Exports/Items.py", stringBuilder.ToString());
        }

        private static ArchipelagoItem CreateArchipelagoItem(GroupData groupData)
        {
            nextArchipelagoItemId++;
            string classification = "progression";


            return new ArchipelagoItem(groupData.id, nextArchipelagoItemId, classification);
        }
    }
}
