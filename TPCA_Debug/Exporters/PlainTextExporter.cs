using SpaceCraft;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPCA_Debug.Exporters
{
    internal static class PlainTextExporter
    {
        internal static void ExportGroupsDataNames(List<Group> allGroups)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in allGroups.Select(x => x.GetGroupData()).Where(x => !x.unlockInPlanets.Any() && (x.unlockingValue != 0 || x.terraformStageUnlock != null)))
            {
                stringBuilder.AppendLine($"\"{item.id}\",");
            }

            //write string to file
            System.IO.File.WriteAllText(@"./Exports/Items.txt", stringBuilder.ToString());
        }
    }
}
