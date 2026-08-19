using System;
using DevLib.ModuleSystem;
using UnityEngine;

namespace SkillSystem
{
    public interface ISkillModule
    {
        event Action<int> OnSkillEnd;
        ModuleOwner Owner { get; }  
        ISkill CurrentSkill { get; }
        
        bool CanUseSkill(int skillId, GameObject target = null);
        void UseSkill(int skillId, GameObject target = null);
        float GetBaseDamage(SkillDataSO skillData);
    }
}