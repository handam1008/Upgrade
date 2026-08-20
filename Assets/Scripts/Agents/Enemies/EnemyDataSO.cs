using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies
{
    [BlackboardEnum]
    public enum RangeField
    {
        StopDistance,
        Detect,
        Attack,
        SignalLost
    }
    
    [CreateAssetMenu(fileName = "Enemy data", menuName = "Agent/Enemy data", order = 25)]
    public class EnemyDataSO : ScriptableObject
    {
        [field: SerializeField] public float StopDistance { get; private set; } = 0.8f;
        [field: SerializeField] public float DetectRadius { get; private set; }
        [field: SerializeField] public float AttackRange { get; private set; }
        [field: SerializeField] public float SignalLostRange { get; private set; }

        public float GetFieldValue(RangeField field) => field switch
        {
            RangeField.StopDistance => StopDistance,
            RangeField.Detect => DetectRadius,
            RangeField.Attack => AttackRange,
            RangeField.SignalLost => SignalLostRange,
            _ => 0
        };
    }
}