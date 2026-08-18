using Unity.VisualScripting;
using UnityEngine;

namespace Test32.FeedBack
{
    public class ParticleFeedBack: AbstractFeedBack12312
    {
        [SerializeField] private GameObject _particlePrefab;
        public override void CreateFeedBack()
        {
            GameObject go = Instantiate(_particlePrefab);
            ParticleSystem  ps = go.GetComponent<ParticleSystem>();
            ps.Play();
        }

        public override void StopFeedBack()
        {
            throw new System.NotImplementedException();
        }

        
    }
}