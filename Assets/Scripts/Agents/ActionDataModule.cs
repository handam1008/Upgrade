using DevLib.ModuleSystem;
using UnityEngine;

namespace Agents
{
    public class ActionDataModule : MonoModule
    {
        public GameObject LastDealer { get; set; }
        public Vector2 HitPoint { get; set; }
        public Vector2 KnockbackForce { get; set; }
        public Vector2 HitNormal { get; set; }
        public bool IsLastHitCritical { get; set; }
        
        
    }
}