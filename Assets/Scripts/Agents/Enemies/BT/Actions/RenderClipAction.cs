using System;
using DevLib.HashDataSystem;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "render clip", story: "[Enemy] render [Clip] [IsRestart]", category: "Action/Animation", id: "32854e29519cfe11ee713411bd279f75")]
    public partial class RenderClipAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<HashDataSO> Clip;
        [SerializeReference] public BlackboardVariable<bool> IsRestart;

        protected override Status OnStart()
        {
            if(Enemy.Value == null || Enemy.Value.Renderer == null || Clip.Value == null)
                return Status.Failure;
            
            if(IsRestart.Value)
                Enemy.Value.Renderer.RenderClip(Clip.Value.HashValue);
            else
                Enemy.Value.Renderer.RenderClipNotPlaying(Clip.Value.HashValue);
            
            return Status.Success;
        }
        
    }
}

