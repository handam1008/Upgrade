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
        protected virtual void HandleDead()
        {
            IsDead = true;
        }
    }
}