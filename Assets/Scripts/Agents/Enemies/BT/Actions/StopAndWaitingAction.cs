using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Stop and waiting", story: "[Enemy] stop and waiting [Sec]", category: "Action/Combat", id: "475237ea17a23617c4e426c41767c02b")]
    public partial class StopAndWaitingAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<float> Sec;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGO;

        private float _startTime;
        private EnemySensor _sensor;
        private EnemyDataSO _enemyData;
        protected override Status OnStart()
        {
            AbstractEnemy enemy = Enemy.Value;
            if (enemy == null || enemy.Nav == null || enemy.EnemyData == null)
                return Status.Failure;

            _startTime = Time.time;
            _sensor = enemy.Sensor;
            _enemyData = enemy.EnemyData;
            
            enemy.Nav.Stop();
            
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (TargetGO.Value == null)
            {
                if (_sensor == null)
                    return Status.Failure;
                if (_sensor.IsTargetInRadius(_enemyData.DetectRadius))
                    return Status.Failure;
            }
            else
            {
                Vector3 targetPos = TargetGO.Value.transform.position;
                Vector3 selfPos = Enemy.Value.transform.position;
                float distance = Vector2.Distance(targetPos, selfPos);
                if (distance < _enemyData.SignalLostRange)
                    return Status.Failure;
            }
            
            if (Time.time - _startTime > Sec.Value) return Status.Success;

            return Status.Running;
        }
    }
}

