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

       
        public GameObject IsTargetInRadius(float radius)
        {
            int count = Physics2D.OverlapCircle(transform.position, radius, filter, ColliderResults);
            return count > 0 ? ColliderResults.First().gameObject : null;
        }

       
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

        
        public int GetAllTargetsInRadius(float radius)
        {
            return Physics2D.OverlapCircle(transform.position, radius, filter, ColliderResults);
        }

      
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