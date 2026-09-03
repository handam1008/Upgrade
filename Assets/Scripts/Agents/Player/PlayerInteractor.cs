using DevLib.ModuleSystem;
using GameSystem;
using UnityEngine;

namespace Agents.Player
{
    public class PlayerInteractor : MonoModule
    {
        [SerializeField] private ContactFilter2D interactableFilter;
        [SerializeField] private float interactRadius = 2f;        
        [SerializeField] private int maxResults = 8;             

        Collider2D[] _results;
        private IInteractable _target;
        private PlayerController _player;
        
        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _results = new Collider2D[maxResults];
            _player = owner as PlayerController;
        }

        private void Start()
        {
            Debug.Log("구독완료");
            _player.playerInput.onInteractKeyPress += HandleInteract;
        }

        private void OnDestroy()
        {
            _player.playerInput.onInteractKeyPress -= HandleInteract;
        }

        private void HandleInteract()
        {
            if (_target == null) return;
            if (!_target.CanInteract) return;
            
            _target.Interact();
        }

        private GameObject GetClosestTarget(float radius)
        {
            int count = Physics2D.OverlapCircle(transform.position, radius, interactableFilter, _results);

            if (count == 0) return null;

            GameObject closest = null;
            float minSqrDistance = float.MaxValue;
            Vector2 origin = transform.position;

            for (int i = 0; i < count; i++)
            {
                float sqrDistance = ((Vector2)_results[i].transform.position - origin).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closest = _results[i].gameObject;
                }
            }

            return closest;
        }

        private void Update()
        {
            GameObject closest = GetClosestTarget(interactRadius);
            if (closest == null)
            {
                _target = null;
                return;
            }
            
            if (closest.TryGetComponent(out IInteractable interactable))
            {
                _target = interactable;
            }
            else
            {
                _target = null;
            }
        }
    }
}