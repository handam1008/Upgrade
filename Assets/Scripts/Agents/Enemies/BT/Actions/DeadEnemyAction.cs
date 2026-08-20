using Agents.Enemies;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DeadEnemy", story: "[Enemy] disable collider.", category: "Action", id: "49a7d6924f6b757b0f81f911e227e51f")]
public partial class DeadEnemyAction : Action
{
    [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;

    protected override Status OnStart()
    {
        Enemy.Value.GetComponent<Collider2D>().enabled = false;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

