using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.ModuleSystem;
using UnityEngine;

namespace SkillSystem
{
    public abstract class AbstractSkillModule : MonoModule, ISkillModule
    {
        public event Action<int> OnSkillEnd;

        protected Dictionary<int, ISkill> _skillDict;
        
        public ISkill CurrentSkill { get; private set; }

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _skillDict = GetComponentsInChildren<ISkill>()
                .ToDictionary(s => s.SkillData.skillIdHash);
            
            foreach(ISkill skill in _skillDict.Values)
                skill.InitalizeSkill(this);
        }


        public ModuleOwner owner { get; }

        public bool CanUseSkill(int skillId, GameObject target = null)
        {
            if (_skillDict.TryGetValue(skillId, out ISkill skill))
            {
                return skill.CanUseSkill(target);
            }

            return false;
        }

       
        public void UseSkill(int skillId, GameObject target = null)
        {
            if (_skillDict.TryGetValue(skillId, out ISkill skill))
            {
                if (CurrentSkill is { IsUsing: true })  
                {
                    ISkill oldSkill = CurrentSkill;
                    CurrentSkill = null;
                    oldSkill.OnSkillEnd -= HandleSkillEnd;
                    oldSkill.StopSkill();
                }
                
                CurrentSkill = skill;
                CurrentSkill.OnSkillEnd += HandleSkillEnd;
                CurrentSkill.UseSkill(target);
            }
        }
        
        private void HandleSkillEnd(ISkill endSkill)
        {
            endSkill.OnSkillEnd -= HandleSkillEnd; 
            int skillId = endSkill.SkillData.skillIdHash;
            if(endSkill == CurrentSkill)
                CurrentSkill = null;
            OnSkillEnd?.Invoke(skillId);
        }
        
        public abstract float GetBaseDamage(SkillDataSO skillData);
    }
}