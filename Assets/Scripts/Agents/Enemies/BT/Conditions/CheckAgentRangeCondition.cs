using System;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Check Agent Range", story: "[Enemy] to [TargetGO] [Operator] [Range]", category: "Conditions", id: "2293f280f51e4335ec6376cef1231a8a")]
    public partial class CheckAgentRangeCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGO;
        [Comparison(comparisonType: ComparisonType.All)]
        [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
        [SerializeReference] public BlackboardVariable<RangeField> Range;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || TargetGO.Value == null || Enemy.Value.EnemyData == null) return false;
            
            float threshold = Enemy.Value.EnemyData.GetFieldValue(Range.Value);
            float distance = Vector2.Distance(Enemy.Value.transform.position, TargetGO.Value.transform.position);

            return Operator.Value switch
            {
                ConditionOperator.Equal => Mathf.Approximately(distance, threshold),
                ConditionOperator.NotEqual => !Mathf.Approximately(distance, threshold),
                ConditionOperator.Greater => distance > threshold,
                ConditionOperator.Lower => distance < threshold,
                ConditionOperator.GreaterOrEqual => distance >= threshold,
                ConditionOperator.LowerOrEqual => distance <= threshold,
                _ => false
            };
        }

    }
}
