using TMPro;
using UnityEngine;

namespace Test32.FeedBack
{
    public class TextFeedBack : AbstractFeedBack12312
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _lifeTime = 0.5f;

        public override void CreateFeedBack()
        {
            if (_text == null) return;

            TextMeshPro text = Instantiate(_text, transform.position, Quaternion.identity);
            Destroy(text.gameObject, _lifeTime);
        }
    }
}
