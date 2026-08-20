using UnityEngine;

namespace CombatSystem
{
    public class OverlapDamageCaster : AbstractDamageCaster
    {
        public enum CastType
        {
            Circle, Box
        }
        
        [SerializeField] private CastType castType;
        [SerializeField] private float radius; //원형 캐스트 전용
        [SerializeField] private Vector2 boxSize; //박스 캐스트 전용
        
        public void SetRadius(float value) => radius = value; 
        public void SetBoxSize(Vector2 value) => boxSize = value;
        
        public override bool CastDamage(float damage, Vector2 direction, float kbForce)
        {
            int cnt = castType switch
            {
                CastType.Circle => Physics2D.OverlapCircle(transform.position, radius, contactFilter, _hitResult),
                CastType.Box => Physics2D.OverlapBox(transform.position, boxSize, 0, contactFilter, _hitResult),
                _ => 0
            };

            for (int i = 0; i < cnt; i++)
            {
                if (_hitResult[i].TryGetComponent(out IDamageable damageable))
                {
                    Vector2 point= _hitResult[i].ClosestPoint(transform.position); //가장 가까운 지점을 찾아서 피격으로 해주고
                    Vector2 knockbackForce = direction.normalized * kbForce;

                    DamageData damageData = new DamageData
                    {
                        DamageAmount = damage,
                        Dealer = Owner,
                        DirectedKBForce = knockbackForce
                    };
                    
                    damageable.ApplyDamage(damageData, point, direction, -direction);
                }
            }

            return cnt > 0;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (castType == CastType.Circle)
                Gizmos.DrawWireSphere(transform.position, radius);
            else if(castType == CastType.Box)
                Gizmos.DrawWireCube(transform.position, boxSize);
        }
        #endif
    }
}