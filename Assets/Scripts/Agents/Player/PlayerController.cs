using Agents.Player.FSM;
using DevLib.FsmSystem.Runtime;
using GameSystem;
using UnityEngine;

namespace Agents.Player
{
   [RequireComponent(typeof(Rigidbody2D))]
   public class PlayerController : AbstractAgent
   {
      [field:SerializeField] public PlayerInputSO playerInput { get; private set; }
      [SerializeField] private StateListSO playerStateList;
      
      public override bool IsGuard => _stateMachine?.CurrentState is AbstractPlayerState { IsInvulnerable: true };
   
      private StateMachine _stateMachine;

      protected override void InitializeModules()
      {
         base.InitializeModules();
         Application.runInBackground = true;

         playerInput.SetEnable();
         _stateMachine = new StateMachine(gameObject, playerStateList.states);
      

      }

      private void Start()
      {
         ChangeState(PlayerState.IDLE);
      }
   
      public void ChangeState(PlayerState newState)=> _stateMachine?.ChangeState((int)newState);

      protected override void HandleHit()
      {
         base.HandleHit();
         if (IsDead) return;
         if (IsGuard) return;
         ChangeState(PlayerState.HIT);
      }

      protected override void HandleDead()
      {
         base.HandleDead();
         ChangeState(PlayerState.DEAD);
      }


      private void OnDestroy()
      {
         playerInput.SetDisable();
      }
   

      private void Update()
      {
         _stateMachine?.UpdateMachine();
      
      
      }
   }
}
