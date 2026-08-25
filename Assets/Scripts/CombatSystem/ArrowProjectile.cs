using System;
using System.Collections.Generic;
using Agents.Enemies.Skills;
using DevLib.ModuleSystem;
using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class ArrowProjectile : MonoBehaviour, IProjectile
    {
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask ObstacleLayer;
        [SerializeField] private int maxHitCount = 1;
        [SerializeField] private bool rotateToVelocity = true;
        [SerializeField] private GameObject hitEffectPrefab;
        
        private readonly HashSet<int> _hitTargets = new HashSet<int>();
        
        private Rigidbody2D _rigidbody;
        private ModuleOwner _owner;
        private SkillDataSO _skillData;
        private Vector2 _direction;
        private float _damage;
        private float _lifeTimer;
        private bool _isLaunched;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Launch(ModuleOwner owner, SkillDataSO skillData, Vector2 velocity, float damage, float scaleMultiplier = 1)
        {
            _owner = owner;
            _skillData = skillData;
            _damage = damage;
            _lifeTimer = lifeTime;
            _isLaunched = true;
            _direction = velocity.sqrMagnitude > Mathf.Epsilon ? velocity.normalized : Vector3.zero;
            _hitTargets.Clear();
            
            transform.localScale *= scaleMultiplier;
            
            if (rotateToVelocity)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            
            _rigidbody.gravityScale = 0f;
            _rigidbody.linearVelocity = velocity;
            
        }

        private void Update()
        {
            if (!_isLaunched) return;
            
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                DestroyProjectile(false);
            }
        }

        private void DestroyProjectile(bool spawnEffect)
        {
            _isLaunched = false;
            if(spawnEffect && hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isLaunched) return; // 쏘고있나?
            if (_owner != null && collision.transform.IsChildOf(_owner.transform)) return;
            
            if (IsInLayerMask(collision.gameObject.layer, ObstacleLayer))
            {
                DestroyProjectile(true);
                return;
            }
            
            if (!IsInLayerMask(collision.gameObject.layer, targetLayer)) return; // 레이어 구별
            if (!collision.TryGetComponent(out IDamageable damageable)) return; // IDamageable 구현했는지
            if (!_hitTargets.Add(collision.gameObject.GetInstanceID())) return; // 이미 맞은애 또 맞은애면
            
            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            DamageData damageData = new DamageData()
            {
                DamageAmount = _damage,
                Dealer = _owner,
                DirectedKBForce = _direction * (_skillData != null ? _skillData.kbForce : 0f)
            };
            damageable.ApplyDamage(damageData,hitPoint,_direction,-_direction);
            
            if (_hitTargets.Count >= maxHitCount)
                DestroyProjectile(true);
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
            => (mask.value & (1 << layer)) != 0;
    }
}