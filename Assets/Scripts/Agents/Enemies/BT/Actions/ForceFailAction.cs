using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Force fail", story: "Force fail [Option]", category: "Action/Test", id: "e69cbab33cc612e98b19ec65bf2d9465")]
    public partial class ForceFailAction : Action
    {
        [SerializeReference] public BlackboardVariable<bool> Option;

        protected override Status OnStart()
        {
            if(Option.Value)
                return Status.Failure;
            return Status.Success;
        }

    }

}

