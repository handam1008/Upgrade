using SkillSystem;
using UnityEngine;

namespace Agents.Enemies
{
    public class EnemySkillModule : AbstractSkillModule
    {
        public override float GetBaseDamage(SkillDataSO skillDataSo)
        {
            return skillDataSo.baseSkillDamage;
        }
    }
}