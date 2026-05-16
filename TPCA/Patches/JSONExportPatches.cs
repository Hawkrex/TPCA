using HarmonyLib;
using Newtonsoft.Json;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(JSONExport))]
    internal static class JSONExportPatches
    {
        public static Dictionary<string, ArchipelagoSaveDatas> ArchipelagoInfosByNames = new();

        /// <summary>
        /// Executes before the method
        /// Called when creating a new game
        /// Modify the game settings to skip the intro landing
        /// </summary>
        /// <param name="gameSettings">Game settings</param>
        [HarmonyPatch(nameof(JSONExport.CreateNewSaveFile))]
        [HarmonyPrefix]
        public static void CreateNewSaveFile_Prefix(JsonableGameState gameSettings)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            gameSettings.hasPlayedIntro = true;
        }

        /// <summary>
        /// Executes after the method
        /// Called when creating a new game
        /// Save Archipelago informations to the end of the game save file
        /// </summary>
        /// <param name="saveFileName">Save filename</param>
        [HarmonyPatch(nameof(JSONExport.CreateNewSaveFile))]
        [HarmonyPostfix]
        public static void CreateNewSaveFile_Postfix(string saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            SaveArchipelagoSaveDatas(saveFileName);
        }

        /// <summary>
        /// Executes after the method
        /// Called when game is saved
        /// Save Archipelago informations to the end of the game save file
        /// </summary>
        /// <param name="_saveFileName">Save filename</param>
        [HarmonyPatch(nameof(JSONExport.SaveToJson))]
        [HarmonyPostfix]
        public static void SaveToJson_Postfix(string _saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            SaveArchipelagoSaveDatas(_saveFileName);
        }

        /// <summary>
        /// Saves the Archipelago informations at the end of the save file
        /// </summary>
        /// <param name="saveFileName">Save filename</param>
        private static void SaveArchipelagoSaveDatas(string saveFileName)
        {
            Plugin.Log.LogDebug($"{nameof(SaveArchipelagoSaveDatas)} => Save file <{saveFileName}>");
            string infosJson = string.Empty;
            try
            {
                infosJson = JsonConvert.SerializeObject(Plugin.State.SaveDatas);
            }
            catch (Exception e)
            {
                Plugin.Log.LogFatal($"{nameof(SaveArchipelagoSaveDatas)} => Exception while trying to serialize Archipelago save datas : <{e}>");
            }

            Plugin.Log.LogDebug($"{nameof(SaveArchipelagoSaveDatas)} => Datas to save <{infosJson}>");

            string filePath = GetFullSaveFilePath(saveFileName);

            try
            {
                File.AppendAllText(filePath, "\r\n");
                File.AppendAllText(filePath, infosJson);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"{nameof(SaveArchipelagoSaveDatas)} => Error saving archipelago datas into save <{saveFileName}> : {ex}");
            }
        }

        /// <summary>
        /// Executes after the method
        /// Called when save is loaded
        /// Load Archipelago informations at the end of the game save file
        /// </summary>
        /// <param name="_saveFileName">Save filename</param>
        [HarmonyPatch(nameof(JSONExport.LoadFromJson))]
        [HarmonyPostfix]
        public static void LoadFromJson_Postfix(string _saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                return;
            }

            if (ArchipelagoInfosByNames.ContainsKey(_saveFileName))
            {
                Plugin.Log.LogDebug($"{nameof(LoadFromJson_Postfix)} => Infos already loaded for save <{_saveFileName}>");
                return;
            }

            string filePath = GetFullSaveFilePath(_saveFileName);
            string lastLine = File.ReadLines(filePath).Last();

            if (lastLine != "@") // Normal game save end with a line containing only a "@"
            {
                var infos = JsonConvert.DeserializeObject<ArchipelagoSaveDatas>(lastLine);
                ArchipelagoInfosByNames[_saveFileName] = infos;
            }
        }

        /// <summary>
        /// Get full save file path
        /// </summary>
        /// <param name="saveFileName">Save filename</param>
        /// <returns>The full save file path</returns>
        private static string GetFullSaveFilePath(string saveFileName)
        {
            return string.Format("{0}/{1}.json", Application.persistentDataPath, saveFileName);
        }
    }
}
