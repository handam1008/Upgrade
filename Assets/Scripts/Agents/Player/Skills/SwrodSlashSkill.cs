using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using SkillSystem;
using UnityEngine;

namespace Agents.Player.Skills
{
    public class SwrodSlashSkill : AbstractSkill
    {
        [SerializeField] private SoundClipSO slashSound;

        [Tooltip("애니메이션 종료 이벤트가 없을 때 강제로 끝내는 시간 (상태에 갇히는 것 방지)")]
        [SerializeField] private float fallbackDuration = 0.4f;

        private IAnimateRenderer _renderer;
        private IAnimatorTrigger _trigger;
        private float _endTime;

        public override void InitalizeSkill(ISkillModule skillModule)
        {
            base.InitalizeSkill(skillModule);
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            _trigger = skillModule.Owner.GetModule<IAnimatorTrigger>();
            Debug.Assert(_renderer != null , "Renderer is null");
            Debug.Assert(_trigger != null, "Trigger is null");
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            return IsUsing == false;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _trigger.OnAnimationEnd += HandleAnimationEnd;
            _renderer.RenderClip(SkillData.defaultAnimation.HashValue);
            ServiceLocator.Get<IAudioService>()?.PlaySfx(slashSound);
            _endTime = Time.time + fallbackDuration;
        }

        // 클립에 종료 이벤트가 없어도 스킬 상태에 갇히지 않게 시간으로도 끝낸다
        public override void OnUpdateSkill()
        {
            if (Time.time >= _endTime)
                StopSkill();
        }

        private void HandleAnimationEnd()
        {
            StopSkill();
        }

        public override void CleanUpSkillData()
        {
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
            base.CleanUpSkillData();
        }
    }
}