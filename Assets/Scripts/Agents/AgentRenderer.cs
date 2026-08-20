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
        //Side 클립 하나로 좌우를 다 쓰는 리그에서만 켠다. 플레이어처럼 left/right 클립이 따로 있으면 꺼둬야 이중 반전이 안 생긴다.
        [SerializeField] private bool flipSideSprite;

        //정면/후면 이동에서 x가 미세하게 흔들려도 좌우가 뒤집히지 않도록 하는 데드존.
        private const float HorizontalDeadZone = 0.01f;

        public Animator Animator { get; private set; }
        private SpriteRenderer _spriteRenderer;

        //인터페이스(IAnimateRenderer)로 접근해도 이 값이 쓰인다.
        //예전엔 명시적 구현이 따로 있어서 항상 (0,0)이 반환됐고, 그래서 대쉬가 아래로만 나갔다.
        public Vector2 FacingDirection { get; private set; }

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            Animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
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

            //위아래로만 움직일 땐 직전 좌우를 그대로 유지해야 해서, 데드존을 넘을 때만 갱신한다.
            if (flipSideSprite && Mathf.Abs(direction.x) > HorizontalDeadZone)
                _spriteRenderer.flipX = direction.x < 0f;
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