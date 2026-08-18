using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Test32.FeedBack
{
    public class TextFeedBack: AbstractFeedBack12312
    {
        [SerializeField] private TextMeshPro _text;

        public override void CreateFeedBack()
        {
            TextMeshPro text = Instantiate(_text);
            StartCoroutine(DeleteText(text));
        }

        public IEnumerator DeleteText(TextMeshPro text)
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(text.gameObject);
        }



        public override void StopFeedBack()
        {
            throw new System.NotImplementedException();
        }
    }
}