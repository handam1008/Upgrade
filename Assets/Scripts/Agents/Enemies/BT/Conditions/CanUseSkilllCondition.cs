using System;
using SkillSystem;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Can use skilll", story: "[Enemy] can use [Skill] to [TargetGO]", category: "Conditions", id: "ad7b40701b10a1233190708aeae229e8")]
    public partial class CanUseSkilllCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<SkillDataSO> Skill;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGO;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Skill.Value == null || Enemy.Value.SkillModule == null)
                return false;
            return Enemy.Value.SkillModule.CanUseSkill(Skill.Value.skillIdHash, TargetGO.Value);
        }
    }
}
