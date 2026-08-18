using DevLib.ModuleSystem;
using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public interface IProjectile
    {
        public void Launch(ModuleOwner owner, SkillDataSO skillData, Vector2 velocity, float damage,
            float scaleMultiplier = 1f);
        
    }
}