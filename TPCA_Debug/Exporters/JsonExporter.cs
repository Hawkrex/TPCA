using Newtonsoft.Json;
using SpaceCraft;
using System.Collections.Generic;
using System.Linq;
using TPCA_Debug.JsonModels;

namespace TPCA_Debug.Exporters
{
    internal static class JsonExporter
    {
        public static void ExportGroupsDatas(List<Group> groups)
        {
            string json = JsonConvert.SerializeObject(groups.Select(x => new GroupDataJson(x.GetGroupData())).ToArray(), Formatting.Indented);

            //write string to file
            System.IO.File.WriteAllText(@"./Exports/Groups.json", json);
        }
    }
}
