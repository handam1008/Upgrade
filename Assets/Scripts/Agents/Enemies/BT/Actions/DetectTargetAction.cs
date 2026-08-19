using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Detect target", story: "[Enemy] detect [TargetGO]", category: "Action/Combat", id: "12f0a829ceea97d6c39590c6c6487234")]
    public partial class DetectTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGO;

        protected override Status OnStart()
        {
            AbstractEnemy enemy = Enemy.Value;
            if(enemy == null || enemy.Sensor == null || enemy.EnemyData == null)
                return Status.Failure;

            //만약 장애물도 감지할거면 함수만 변경해주면 된다.
            bool found = enemy.Sensor.TryDetectTarget(enemy.EnemyData.DetectRadius, out GameObject target);

            if (found)
            {
                TargetGO.Value = target;
                return Status.Success;
            }
            return Status.Failure;
        }

    }
}

