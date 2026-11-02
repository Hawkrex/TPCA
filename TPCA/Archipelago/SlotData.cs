using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using TPCA.Archipelago.Enums;

namespace TPCA.Archipelago
{
    // Informations sent by the server about the game
    internal class SlotData
    {
        [JsonProperty("Goal")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Goal Goal { get; set; }
        public bool DeathLink { get; set; }

        public SlotData()
        {
            // Used for Json deserialization
        }

        public SlotData(IReadOnlyDictionary<string, object> slotData)
        {
            Goal = ParseEnum<Goal>(slotData["goal"]);

            DeathLink = ParseBool(slotData["death_link"]);
        }

        private bool ParseBool(object inputValue, bool defaultValue = false)
        {
            if (inputValue.GetType() != typeof(long))
            {
                return defaultValue;
            }

            return Convert.ToInt64(inputValue) == 1;
        }

        private T ParseEnum<T>(object inputValue, T defaultValue = default) where T : Enum
        {
            if (inputValue.GetType() == typeof(long))
            {
                return (T)Enum.ToObject(typeof(T), inputValue);
            }

            return defaultValue;
        }
    }
}
