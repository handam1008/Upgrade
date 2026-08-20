using UnityEngine;

namespace Test
{
    public class AudoDeactivator : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}