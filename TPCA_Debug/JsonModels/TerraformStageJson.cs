using SpaceCraft;

namespace TPCA_Debug.JsonModels
{
    internal class TerraformStageJson
    {
        public string Id { get; private set; }

        public double StartValue { get; private set; }

        public string UnitType { get; private set; }

        public TerraformStageJson(TerraformStage terraformStage)
        {
            if (terraformStage == null)
                return;

            Id = terraformStage.GetTerraId();
            StartValue = terraformStage.GetStageStartValue();
            UnitType = terraformStage.GetWorldUnitType().ToString();
        }
    }
}
