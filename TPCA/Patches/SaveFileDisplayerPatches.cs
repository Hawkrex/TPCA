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

        /// <summary>
        /// Executes before the method
        /// Called when showing a save file in the SaveFileDisplayer
        /// Shows the GUID of the archipelago player
        /// </summary>
        /// <param name="fileName">Save file name</param>
        /// <param name="___gameModeText">Text component showing the game mode and the planet</param>
        /// <returns>true to execute the original method after</returns>
        [HarmonyPatch(nameof(SaveFileDisplayer.SetData))]
        [HarmonyPrefix]
        public static bool SetData_Prefix(string fileName, TextMeshProUGUI ___gameModeText)
        {
            if (JSONExportPatches.ArchipelagoInfosByNames == null || !JSONExportPatches.ArchipelagoInfosByNames.ContainsKey(fileName))
            {
                return true;
            }

            CreateArchipelagoIdTextComponent(___gameModeText);

            var archipelagoInfos = JSONExportPatches.ArchipelagoInfosByNames[fileName];
            text.text = $"AP  {archipelagoInfos.Guid}";

            text.GetComponent<RectTransform>().localPosition += new Vector3(10, 10);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 20);

            return true;
        }

        /// <summary>
        /// Create a new text component above the gameMode text component
        /// </summary>
        /// <param name="___gameModeText">Text component showing the game mode and the planet</param>
        private static void CreateArchipelagoIdTextComponent(TextMeshProUGUI ___gameModeText)
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
