using System;
using System.Collections.Generic;
using DevLib.ModuleSystem;
using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class FireBallProjectile : MonoBehaviour ,IProjectile
    {
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstaclelayers;
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
            //길이가 0에 가까운 걸 normalize하면 위험. (내부적으로 컷 되긴 함)
            _direction = velocity.sqrMagnitude > Mathf.Epsilon ? velocity.normalized : Vector2.right;
            _lifeTimer = lifeTime;
            _isLaunched = true;
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
        
        //나중을 위해서, 풀링하게 되면 여길 바꾸면 된다.

        private void DestroyProjectile(bool spawnEffect)
        {
            _isLaunched = false;
            if (spawnEffect && hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isLaunched) return;
            if (_owner != null && collision.transform.IsChildOf(_owner.transform)) return;
            if (IsInLayerMask(collision.gameObject.layer, obstaclelayers))
            {
                DestroyProjectile(true);
                return;
            }

            if (!IsInLayerMask(collision.gameObject.layer, targetLayers)) return;
            if (!collision.TryGetComponent(out IDamageable damageable)) return;
            
            if (!_hitTargets.Add(collision.gameObject.GetInstanceID())) return;
            
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

        private static bool IsInLayerMask(int layer, LayerMask layerMask)
            => (layerMask.value & (1 << layer)) != 0;
    }
}