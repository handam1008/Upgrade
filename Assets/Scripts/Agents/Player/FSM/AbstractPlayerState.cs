using DevLib.FsmSystem.Runtime;
using UnityEngine;

namespace Agents.Player.FSM
{
    public abstract class AbstractPlayerState : AbstractState
    {
        protected PlayerController _player;
        
        protected const float MOVE_THRESHOLD = 0.01f;
        
        protected AbstractPlayerState(GameObject owner, StateSO stateData) : base(owner, stateData)
        {
            _player = owner.GetComponent<PlayerController>();
            Debug.Assert(_player != null, "PlayerController가 null 입니다 플레이어 상태는 반드시 플레이어의 자식이여야 합니다");
            
        }

        public override void Enter()
        {
            _player.Renderer.RenderClipNotPlaying(_stateSO.animHash.HashValue);
        }
    }
}