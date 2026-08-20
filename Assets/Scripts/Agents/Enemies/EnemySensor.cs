using System.Linq;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents.Enemies
{
    public class EnemySensor : MonoModule
    {
        [SerializeField] private ContactFilter2D filter;
        [SerializeField] private int maxResults;
        [SerializeField] private LayerMask obstacleMask;
        
        public Collider2D[] ColliderResults { get; private set; }

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            ColliderResults = new Collider2D[maxResults];
        }

        /// <summary>
        /// 범위 내 첫번째 감지된 타겟을 반환하는 매서드. 없다면 null 리턴
        /// </summary>
        public GameObject IsTargetInRadius(float radius)
        {
            int count = Physics2D.OverlapCircle(transform.position, radius, filter, ColliderResults);
            return count > 0 ? ColliderResults.First().gameObject : null;
        }

        /// <summary>
        /// 지정된 범위내에 있는 적들중 가장 가까운 적을 반환
        /// </summary>
        public GameObject GetClosestTarget(float radius)
        {
            int count = Physics2D.OverlapCircle(transform.position, radius, filter, ColliderResults);

            if (count == 0) return null;

            GameObject closest = null;
            float minSqrDistance = float.MaxValue;
            Vector2 origin = transform.position;

            for (int i = 0; i < count; i++)
            {
                float sqrDistance = ((Vector2)ColliderResults[i].transform.position - origin).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closest = ColliderResults[i].gameObject;
                }
            }

            return closest;
        }

        /// <summary>
        /// 범위 내 모든 타겟을 감지하고 감지한 수를 반환한다.
        /// </summary>
        public int GetAllTargetsInRadius(float radius)
        {
            return Physics2D.OverlapCircle(transform.position, radius, filter, ColliderResults);
        }

        /// <summary>
        /// radius 내에서 가시 가능한 첫번째 타겟을 반환한다. 없으면 false;
        /// </summary>
        public bool TryDetectTarget(float radius, out GameObject target)
        {
            int count = GetAllTargetsInRadius(radius);

            for (int i = 0; i < count; i++)
            {
                GameObject candidate = ColliderResults[i].gameObject;

                if (IsTargetVisible(candidate))
                {
                    target = candidate;
                    return true;
                }
            }
            target = null;
            return false;
        }

        // 타겟까지 obstacle에 해당하는 장애물이 있는지 체크하는거
        private bool IsTargetVisible(GameObject target)
        {
            if (target == null) return false;
            Vector2 origin = transform.position;
            Vector2 targetPosition = target.transform.position;

            RaycastHit2D hit = Physics2D.Linecast(origin, targetPosition, obstacleMask);
            return !hit;
        }
    }
}