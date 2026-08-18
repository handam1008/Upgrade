using System;
using DevLib.HashDataSystem;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class AgentRenderer : MonoModule, IAnimateRenderer, IAnimatorTrigger, IAfterInitModule
    {
        [SerializeField] private HashDataSO moveXHash;
        [SerializeField] private HashDataSO moveYHash;

        public Animator Animator { get; private set; }

        //인터페이스(IAnimateRenderer)로 접근해도 이 값이 쓰인다.
        //예전엔 명시적 구현이 따로 있어서 항상 (0,0)이 반환됐고, 그래서 대쉬가 아래로만 나갔다.
        public Vector2 FacingDirection { get; private set; }

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            Animator = GetComponent<Animator>();
        }

        public void AfterInit()
        {
            if (Owner.GetModule<ITopDownMover>() is { } mover)
            {
                mover.OnMovementChange += SetMovementDirection;
            }
        }

        private void OnDestroy()
        {
            if (Owner.GetModule<ITopDownMover>() is { } mover)
            {
                mover.OnMovementChange -= SetMovementDirection;
            }
        }


        public void SetMovementDirection(UnityEngine.Vector2 direction)
        {
            if (Mathf.Approximately(direction.magnitude, 0f))
                return;
            
            Animator.SetFloat(moveXHash.HashValue, direction.x);
            Animator.SetFloat(moveYHash.HashValue, direction.y);
            FacingDirection = direction;
        }

        public void RenderClip(int clipHash)
        {
           Animator.Play(clipHash, 0, 0f);
        }

        public void RenderClipNotPlaying(int clipHash)
        {
            if(Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != clipHash)
                RenderClip(clipHash);
        }

        #region Trigger logic
        
        public event Action OnAnimationEnd;
        public event Action OnDamageCast;
        public event Action OnFootStep;
        
        private void FootstepTrigger() => OnFootStep?.Invoke();
        private void AnimationEndTrigger() => OnAnimationEnd?.Invoke();
        private void DamageCastTrigger() => OnDamageCast?.Invoke();

        

        #endregion

        
       
    }
}