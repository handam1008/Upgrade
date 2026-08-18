using DevLib.HashDataSystem;
using UnityEngine;

namespace SkillSystem
{
    public enum SkillType
    {
        Physical, Magic, NonDamage
    }

    public enum DirectionType
    {
        Body, Pointer
    }

    public enum SkillCategory
    {
        BasicAttack, Active, Dash
    }
    [CreateAssetMenu(fileName = "Skill data", menuName = "Agent/Skill data", order = 0)]
    public class SkillDataSO : ScriptableObject
    {
        public int skillIdHash;
        public string skillName;
        public Sprite icon;
        public SkillCategory category = SkillCategory.Active;
        public SkillType type = SkillType.Physical;
        public DirectionType directionType = DirectionType.Body;
        public float maxRange;
        public HashDataSO defaultAnimation;
        public float damageMultiplier = 1f;
        public float baseSkillDamage = 3f;
        public float kbForce = 0;
        public float cooldownTime = 0.5f;

        private void OnValidate()
        {
            skillIdHash = Animator.StringToHash(skillName);
        }
    }
}