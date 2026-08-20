using DevLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public abstract class AbstractDamageCaster : MonoBehaviour
    {
        [SerializeField] protected int maxHitCount = 5;
        [SerializeField] protected ContactFilter2D contactFilter;

        protected Collider2D[] _hitResult;
        
        public ModuleOwner Owner { get; private set; }
        
        public void InitCaster(ModuleOwner owner)
        {
            Owner = owner;
            _hitResult = new Collider2D[maxHitCount];
        }

        public abstract bool CastDamage(float damage, Vector2 direction, float kbForce);

    }
}