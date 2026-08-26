#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace GameModule.UI
{
    [CreateAssetMenu(fileName = "Health View Model", menuName = "Agent/UI/Health View Model")]
    public class HealthModelSO : ScriptableObject
    {
        public float currentHelath;
        public float maxHelath;
        
        [HideInInspector] public float normalizedHealth;

        public void SyncHealth()
        {
            normalizedHealth = maxHelath > 0 ? currentHelath / maxHelath : 0f;
        }

        public void SetHealth(float current, float max)
        {
            currentHelath = current;
            maxHelath = max;
            SyncHealth();
        }
        
        private void OnValidate() => SyncHealth();

        public static HealthModelSO CreateInstanceFromOriginal(HealthModelSO original)
        {
            HealthModelSO healthModel =  CreateInstance<HealthModelSO>();
            healthModel.SetHealth(original.currentHelath, original.maxHelath);
            return healthModel;
        }
        
        
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RegisterConverters()
        {
            var healthGroup = new ConverterGroup("Health bar converter group");
            healthGroup.AddConverter((ref float normalized) => new StyleColor(Color.Lerp(Color.red,Color.green,normalized)));
            
            
            
            ConverterGroups.RegisterConverterGroup(healthGroup);
            
            var withGroup = new ConverterGroup("Float to with converter group");
            withGroup.AddConverter((ref float normalized) => 
                new StyleLength(new Length(normalized * 100f, LengthUnit.Percent)));
            ConverterGroups.RegisterConverterGroup(withGroup);
        }
    }
}
