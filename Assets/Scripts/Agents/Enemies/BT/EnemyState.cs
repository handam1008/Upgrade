using Unity.Behavior;

namespace Agents.Enemies.BT
{
    [BlackboardEnum]
    public enum EnemyState
    {
        IDLE = 0, 
        BATTLE = 1,
        RETURN_TO_ORIGIN = 2,
        HIT = 3,
        DEAD = 4,
        STUN = 5
    }
}