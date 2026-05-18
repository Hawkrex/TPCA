using HarmonyLib;
using SpaceCraft;
using TPCA.Archipelago.Enums;
using static SpaceCraft.MachineDeparturePlatform;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(MachineDeparturePlatform))]
    internal class MachineDeparturePlatformPatches
    {
        /// <summary>
        /// Executes beofre the method
        /// Called when player selects a departure on the departure platform's screen
        /// Send the completion flag to the server if the goal matches to the correct departure method
        /// </summary>
        /// <param name="endingType"></param>
        [HarmonyPatch(nameof(MachineDeparturePlatform.StartDepartureSequence))]
        [HarmonyPrefix]
        public static void StartDepartureSequence_Prefix(DepartureType endingType)
        {
            Plugin.Log.LogDebug($"{nameof(StartDepartureSequence_Prefix)} => Departure made with ending <{endingType}>");
            Plugin.Log.LogDebug($"{nameof(StartDepartureSequence_Prefix)} => Goal was <{Plugin.State.SlotData.Goal}>");

            switch (endingType)
            {
                case DepartureType.EndingSentinel:
                    if (Plugin.State.SlotData.Goal == Goal.PrimeSentinel)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingRiley:
                    if (Plugin.State.SlotData.Goal == Goal.PrimeSmugglers)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingWardens:
                    if (Plugin.State.SlotData.Goal == Goal.PrimeWardens)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingHumbleSentinel:
                    break;
                case DepartureType.EndingHumbleStarform:
                    break;
                case DepartureType.EndingHumbleSolo:
                    break;
                case DepartureType.EndingToxicitySolo:
                    break;
                case DepartureType.EndingToxicityEternastra:
                    break;
                case DepartureType.EndingToxicityDassetex:
                    break;
            }
        }
    }
}
