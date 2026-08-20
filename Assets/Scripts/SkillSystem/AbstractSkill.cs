using System;
using UnityEngine;

namespace SkillSystem
{
    public abstract class AbstractSkill : MonoBehaviour, ISkill
    {
        public event Action<ISkill> OnSkillEnd;
        [field: SerializeField] public SkillDataSO SkillData { get; set; }
        
        protected float _lastUsedTime = float.NegativeInfinity;
        protected ISkillModule _skillModule;

        public float NormalizedCoolTime
        {
            get
            {
                if (SkillData == null || SkillData.cooldownTime <= 0) return 0f;
                return Mathf.Clamp01(1f - (Time.time - _lastUsedTime) / SkillData.cooldownTime);
            }
        }
        
        public bool IsUsing { get; private set; }
        public virtual bool CanInterrupt => false;

        public virtual void InitializeSkill(ISkillModule skillModule)
        {
            _skillModule = skillModule;
            
        }

        public abstract bool CanUseSkill(GameObject target = null);
        
        public virtual void UseSkill(GameObject target = null)
        {
            IsUsing = true;
        }

        public virtual void OnUpdateSkill() { }
        public virtual void ReleaseInput() {}

        public void StopSkill()
        {
            CleanUpSkillData();
        }

        public virtual void CleanUpSkillData()
        {
            _lastUsedTime = Time.time;
            IsUsing = false;
            OnSkillEnd?.Invoke(this);
        }
        
        

    }
}