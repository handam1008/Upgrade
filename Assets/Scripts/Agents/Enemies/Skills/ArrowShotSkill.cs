using CombatSystem;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
  
    public class ArrowShotSkill : TelegraphedAttackSkill
    {
        [Header("화살")]
        [SerializeField] private ArrowProjectile arrowPrefab;
        [SerializeField] private float arrowSpeed = 12f;
        
        [SerializeField] private float _shotEndTime = 0.5f;
        [SerializeField] private float _shotDuration = 0.5f;
        
        private IAnimatorTrigger _trigger;

        protected override void OnInitialize()
        {
            _trigger = _skillModule.Owner.GetModule<IAnimatorTrigger>();    
        }


        protected override int TelegraphLineCount => 1;

        protected override void DrawTelegraph()
        {
            if (!_telegraph) return;
            
            Vector3 origin = transform.position;
            Vector3 tip = (AimDirection * SkillData.maxRange);
            
            _telegraph.SetLine(0, origin, origin + tip);
        }

        protected override void BeginExecute()
        {
            _shotEndTime = Time.time + _shotDuration;
            if (arrowPrefab == null) return;

            _trigger.OnAnimationEnd -= HandleFireArrow;
            _trigger.OnAnimationEnd += HandleFireArrow;
        }

        private void HandleFireArrow()
        {
            if(arrowPrefab == null) return;
            
            ArrowProjectile arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            arrow.Launch(_skillModule.Owner, SkillData, AimDirection * arrowSpeed, _skillModule.GetBaseDamage(SkillData));
        }

        protected override void OnCleanUp()
        {
            _trigger.OnAnimationEnd -= HandleFireArrow;
        }

        protected override void OnExecuteUpdate()
        {

            if (Time.time >= _shotEndTime) 
                StopSkill();
        }
    }
}
