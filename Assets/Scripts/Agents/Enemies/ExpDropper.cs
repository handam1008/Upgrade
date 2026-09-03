using CombatSystem;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents.Enemies
{
    public class ExpDropper : MonoModule
    {
        [SerializeField] private GameObject expPrefab;
        [SerializeField] private int dropCount = 2;
        [SerializeField] private float scatterRadius = 0.5f;
        
        private HealthComponent _health;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _health = owner.GetModule<HealthComponent>();
        }

        private void Start()
        {
            _health.OnDead += HandleDead;
        }

        private void OnDestroy()
        {
            _health.OnDead -= HandleDead;
        }

        private void HandleDead()
        {
            for (int i = 0; i < dropCount; i++)
            {
               Vector3 pos = transform.position+Random.insideUnitSphere * scatterRadius;
               pos.z = 0;
                
                Instantiate(expPrefab, pos, Quaternion.identity);
            }
        }
    }
}