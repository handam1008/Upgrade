using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Test32.FeedBack
{
    public class FeedSquare : MonoBehaviour
    {
        private List<AbstractFeedBack12312> _feedBack12312;

        private void Awake()
        {
            _feedBack12312 = GetComponents<AbstractFeedBack12312>().ToList();
        }

        public void PlayAllFeedBacks()
        {
            _feedBack12312.ForEach(x => x.CreateFeedBack());
        }

        public void StopAllFeedBacks()
        {
            _feedBack12312.ForEach(x => x.StopFeedBack());
        }
    }
}