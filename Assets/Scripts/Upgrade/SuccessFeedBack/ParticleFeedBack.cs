using UnityEngine;

namespace Test32.FeedBack
{
    public class ParticleFeedBack : AbstractFeedBack12312
    {
        [SerializeField] private GameObject _particlePrefab;
        [SerializeField] private float _lifeTime = 2f;

        public override void CreateFeedBack()
        {
            if (_particlePrefab == null) return;

            GameObject go = Instantiate(_particlePrefab, transform.position, Quaternion.identity);
            if (go.TryGetComponent(out ParticleSystem ps)) ps.Play();

            Destroy(go, _lifeTime);
        }
    }
}
