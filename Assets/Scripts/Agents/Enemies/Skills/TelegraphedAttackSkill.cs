using CombatSystem;
using SkillSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    public abstract class TelegraphedAttackSkill : AbstractSkill
    {
        protected enum Phase { None, Telegraph, Execute }

        [Header("경고 표시")]
        [SerializeField] private float telegraphTime = 0.6f;

        protected ITopDownMover _mover;
        protected IAnimateRenderer _renderer;
        protected TelegraphLines _telegraph;
        private NavModule _nav;

        private float _telegraphEndTime;

        protected Phase CurrentPhase { get; private set; }
        protected Transform Target { get; private set; }

        protected Vector2 AimDirection { get; private set; }

        public override bool CanInterrupt => CurrentPhase == Phase.Telegraph;

        #region 하위 클래스

        protected abstract int TelegraphLineCount { get; }

        protected abstract void DrawTelegraph();

        protected abstract void BeginExecute();

        protected abstract void OnExecuteUpdate();

        protected virtual void OnInitialize() { }

        protected virtual void OnCleanUp() { }

        #endregion

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _mover = skillModule.Owner.GetModule<ITopDownMover>();
            _renderer = skillModule.Owner.GetModule<IAnimateRenderer>();
            _nav = skillModule.Owner.GetModule<NavModule>();
            _telegraph = GetComponentInChildren<TelegraphLines>();
            OnInitialize();
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            if (target == null) return false;
            if (IsUsing || NormalizedCoolTime > 0f) return false;

            return Vector2.Distance(transform.position, target.transform.position) <= SkillData.maxRange;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);

            Target = target.transform;
            AimAtTarget();

            
            _nav?.Stop();
            _mover.StopImmediately();

            CurrentPhase = Phase.Telegraph;
            _telegraphEndTime = Time.time + telegraphTime;

            _telegraph?.Show(TelegraphLineCount);
            DrawTelegraph();
        }

        public override void OnUpdateSkill()
        {
            switch (CurrentPhase)
            {
                case Phase.Telegraph:
                    AimAtTarget();
                    if (Time.time >= _telegraphEndTime)
                    {
                        EnterExecute();
                        return;
                    }
                    DrawTelegraph();
                    break;

                case Phase.Execute:
                    OnExecuteUpdate();
                    break;
            }
        }

     
        private void AimAtTarget()
        {
            if (Target == null) return;

            Vector2 toTarget = (Vector2)Target.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;

            AimDirection = toTarget.normalized;
            _renderer.SetMovementDirection(AimDirection);
        }

     
        private void EnterExecute()
        {
            _telegraph?.Hide();
            CurrentPhase = Phase.Execute;

            if (SkillData.defaultAnimation != null)
                _renderer.RenderClip(SkillData.defaultAnimation.HashValue);

            BeginExecute();
        }

        public override void CleanUpSkillData()
        {
            _telegraph?.Hide();
            OnCleanUp(); 

            CurrentPhase = Phase.None;
            Target = null;
            base.CleanUpSkillData(); 
        }
    }
}
