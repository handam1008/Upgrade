using Agents.Enemies;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Return to origin", story: "[Enemy] return to [OriginPosition] check [TargetGO]", category: "Action/Navigation", id: "256013ced7497085c67a58b35dcf496a")]
public partial class ReturnToOriginAction : Action
{
    [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
    [SerializeReference] public BlackboardVariable<Vector3> OriginPosition;
    [SerializeReference] public BlackboardVariable<GameObject> TargetGO;

    private const float DetectInterval = 0.2f;
    private const float ArriveDistance = 0.3f;

    
    private float _detectTimer;
    private Transform _targetTrm;
    private AbstractEnemy _enemy;
    private float _detectSqr;
    
    protected override Status OnStart()
    {
        if (Enemy.Value == null || TargetGO.Value == null || Enemy.Value.Nav == null)
            return Status.Failure;
        _enemy = Enemy.Value;
        
        _targetTrm = TargetGO.Value.transform;
        _detectTimer = DetectInterval;

        _detectSqr = _enemy.EnemyData.DetectRadius * _enemy.EnemyData.DetectRadius;
        _enemy.Nav.SetDestination(OriginPosition);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_enemy == null || _targetTrm == null)
            return Status.Failure;
        _detectTimer -= Time.deltaTime;
        if (_detectTimer <= 0f)
        {
            Vector3 delta = _targetTrm.position - _enemy.transform.position;
            if (delta.sqrMagnitude < _detectSqr)
            {
                _enemy.Nav.Stop();
                return Status.Failure;
            }
            _detectTimer = DetectInterval;
        }
        
        Vector3 toOrigin = OriginPosition.Value - _targetTrm.position;
        toOrigin.z = 0f;
        if (toOrigin.sqrMagnitude < ArriveDistance * ArriveDistance)
        {
            _enemy.Nav.Stop();
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        _enemy?.Nav?.Stop();
    }
}

