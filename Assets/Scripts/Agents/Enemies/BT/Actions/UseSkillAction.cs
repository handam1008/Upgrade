using System;
using SkillSystem;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Use skill", story: "[Enemy] use [Skill] to [TargetGO]", category: "Action/Combat", id: "47e7bba179ec17e4bab4e929d8783d87")]
    public partial class UseSkillAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<SkillDataSO> Skill;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGO;

        private ISkillModule _skillModule;
        private bool _skillComplete;
        
        protected override Status OnStart()
        {
            if (Enemy.Value == null || Skill.Value == null || Enemy.Value.SkillModule == null)
                return Status.Failure;

            _skillModule = Enemy.Value.SkillModule;
            if (_skillModule == null) return Status.Failure;

            int skillId = Skill.Value.skillIdHash;
            if (!_skillModule.CanUseSkill(skillId, TargetGO.Value))
                return Status.Failure;

            _skillComplete = false;
            _skillModule.OnSkillEnd -= HandleSkillEnd;
            _skillModule.OnSkillEnd += HandleSkillEnd;
            _skillModule.UseSkill(skillId, TargetGO.Value);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            return _skillComplete ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            if (_skillModule != null)
            {
                _skillModule.OnSkillEnd -= HandleSkillEnd;
                _skillModule.CurrentSkill?.CleanUpSkillData();
            }
        }

        private void HandleSkillEnd(int skillId)
        {
            if (skillId == Skill.Value.skillIdHash)
                _skillComplete = true;
        }
    }
}

