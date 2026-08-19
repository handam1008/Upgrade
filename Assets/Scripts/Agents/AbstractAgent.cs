using DevLib.ModuleSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Agents
{
    public abstract class AbstractAgent : ModuleOwner
    {
        public UnityEvent OnHit;

        public IAnimateRenderer Renderer { get; private set; }
        public ITopDownMover Mover { get; private set; }
        public bool IsDead { get; private set; }
        
        public Vector2 FacingDirection => Renderer?.FacingDirection ?? Vector2.zero;

        protected override void InitializeModules()
        {
            base.InitializeModules();
            Renderer = GetModule<IAnimateRenderer>();
            Mover = GetModule<ITopDownMover>();
            Debug.Assert(Mover != null, "Mover != null");
            Debug.Assert(Renderer != null, "Renderer != null");
            
        }

        protected override void AfterInitializeModules()
        {
            base.AfterInitializeModules();
            OnHit ??= new UnityEvent();
            OnHit.AddListener(HandleHit);
        }

        private void OnDestroy()
        {
            OnHit?.RemoveListener(HandleHit);
        }

        //피격 반응. 기본은 아무것도 하지 않고, 필요한 에이전트만 재정의한다.
        protected virtual void HandleHit() { }

        protected virtual void HandleDead()
        {
            IsDead = true;
        }
    }
}