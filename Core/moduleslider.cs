using UnityEngine;
using KSP.UI.Screens;
using System.Reflection;



namespace YourModName
{
    public class ModuleSlider : PartModule
    {
        [KSPField(isPersistant = true, guiActiveEditor = true, guiName = "Slider Control")]
        [UI_FloatRange(affectSymCounterparts = UI_Scene.Editor)]
        public float sliderValue = 50f;

        [KSPField] public string sliderName = "Generic Slider";
        [KSPField] public float defaultValue = 50f;
        [KSPField] public float minValue = 0f;
        [KSPField] public float maxValue = 100f;
        [KSPField] public float stepSize = 1f;
        [KSPField] public string targetModule = "";
        [KSPField] public string targetField = "";
        [KSPField] public string valueMultiplier = "1.0";
		
		[KSPField] public string additionalTargets = "";
		
        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "Current Value")]
        public string displayValue = "";

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            UI_FloatRange sliderControl = Fields["sliderValue"].uiControlEditor as UI_FloatRange;
            if (sliderControl != null)
            {
                sliderControl.minValue = minValue;
                sliderControl.maxValue = maxValue;
                sliderControl.stepIncrement = stepSize;
                sliderControl.onFieldChanged += OnSliderValueChanged;
            }
            Fields["sliderValue"].guiName = sliderName;
            if (HighLogic.LoadedSceneIsEditor) InitializeDefaultValue();
            UpdateDisplay();
            UpdateTargetField();
        }

        private void InitializeDefaultValue()
        {
            if (part != null && part.persistentId == 0) sliderValue = defaultValue;
        }

        private void OnSliderValueChanged(BaseField field, object oldValue)
        {
            UpdateDisplay();
            UpdateTargetField();
            if (HighLogic.LoadedSceneIsEditor) GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartTweaked, part);
        }

        private void UpdateDisplay()
        {
            displayValue = sliderValue.ToString("F2");
        }

        private void UpdateTargetField()
		{
			// Handle primary target
			UpdateSingleTarget(targetModule, targetField, valueMultiplier);
    
			// Handle additional targets
			if (!string.IsNullOrEmpty(additionalTargets))
			{
				string[] targets = additionalTargets.Split(';');
				foreach (string target in targets)
				{
					string[] parts = target.Split(',');
					if (parts.Length == 3)
					{
						UpdateSingleTarget(parts[0].Trim(), parts[1].Trim(), parts[2].Trim());
					}
				}
			}
		}

		private void UpdateSingleTarget(string moduleName, string fieldName, string multiplierStr)
		{
			if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(fieldName)) return;
    
			try 
			{
				float multiplier = float.Parse(multiplierStr);
				var modules = part.FindModulesImplementing<PartModule>();
				foreach (var module in modules)
				{
					if (module.moduleName == moduleName)
					{
						var target = module.Fields[fieldName];
						if (target != null)
						{
							target.SetValue(sliderValue * multiplier, module);
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("[ModuleSlider] Error updating " + moduleName + "." + fieldName + ": " + e.Message);
			}
		}

        public void OnDestroy()
        {
            UI_FloatRange sliderControl = Fields["sliderValue"].uiControlEditor as UI_FloatRange;
            if (sliderControl != null)
            {
                sliderControl.onFieldChanged -= OnSliderValueChanged;
            }
        }
    }
}