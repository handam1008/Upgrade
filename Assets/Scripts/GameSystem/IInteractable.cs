using UnityEngine;

namespace GameSystem
{
    public interface IInteractable
    {
        public void Interact();
        public string Prompt { get;  }
        public bool CanInteract { get; }
    } 
}