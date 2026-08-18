using DevLib.ModuleSystem;
using UnityEngine;

namespace CombatSystem
{
    public struct DamageData
    {
        public float DamageAmount;
        public bool IsCritical;
        public ModuleOwner Dealer;
        public Vector2 DirectedKBForce; //방향성이 있는 넉백 힘.
    }
}