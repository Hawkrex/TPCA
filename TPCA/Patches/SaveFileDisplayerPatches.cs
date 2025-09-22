using HarmonyLib;
using SpaceCraft;
using TMPro;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(SaveFileDisplayer))]
    internal class SaveFileDisplayerPatches
    {
        private static TextMeshProUGUI text;

        [HarmonyPatch(nameof(SaveFileDisplayer.SetData))]
        [HarmonyPrefix]
        public static bool SetData_Prefix(string fileName, TextMeshProUGUI ___gameModeText)
        {
            if (JSONExportPatches.ArchipelagoInfosByNames == null || !JSONExportPatches.ArchipelagoInfosByNames.ContainsKey(fileName))
            {
                return true;
            }

            SaveFileDisplayer_Start(___gameModeText);

            var archipelagoInfos = JSONExportPatches.ArchipelagoInfosByNames[fileName];
            text.text = $"AP  {archipelagoInfos.Guid}";

            text.GetComponent<RectTransform>().localPosition += new Vector3(10, 10);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 20);

            return true;
        }

        private static void SaveFileDisplayer_Start(TextMeshProUGUI ___gameModeText)
        {
            var archipelagoId = new GameObject("ArchipelagoId");
            archipelagoId.transform.SetParent(___gameModeText.transform, false);

            text = archipelagoId.AddComponent<TextMeshProUGUI>();
            text.font = ___gameModeText.font;
            text.fontSize = ___gameModeText.fontSize;
            text.overflowMode = TextOverflowModes.Ellipsis;

            archipelagoId.SetActive(true);
        }
    }
}
