using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "chase to target", story: "[Enemy] chase to [TargetGO]", category: "Action/Navigation", id: "012eda585da0910bf6b5d2b521ade7b7")]
    public partial class ChaseToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGO;

        private AbstractEnemy _enemy;
        private NavModule _nav;
        
        protected override Status OnStart()
        {
            if (Enemy.Value == null || TargetGO.Value == null || Enemy.Value.Nav == null)
                return Status.Failure;

            _enemy = Enemy.Value;
            _nav = Enemy.Value.Nav;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            GameObject target = TargetGO.Value;
            if (_enemy == null || target == null) 
                return Status.Failure;

            Vector3 toTarget = target.transform.position - _enemy.transform.position;
            toTarget.z = 0;

            //정지거리 안이면 이동 멈추기
            float stopDistance = _enemy.EnemyData.StopDistance;
            if (toTarget.sqrMagnitude <= stopDistance * stopDistance)
            {
                _nav.Stop();
                return Status.Success;
            }
            
            _nav.SetDestination(target.transform.position);
            return Status.Running;
        }

        protected override void OnEnd()
        {
            _nav?.Stop();
        }
    }
}

