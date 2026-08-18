using System;
using Agents;
using Agents.Player.FSM;
using DevLib.FsmSystem.Runtime;
using GameSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : AbstractAgent
{
   [field:SerializeField] public PlayerInputSO playerInput { get; private set; }
   [SerializeField] private StateListSO playerStateList;
   private StateMachine _stateMachine;


   protected override void InitializeModules()
   {
      base.InitializeModules();

      //에디터 창이 포커스를 잃어도 게임 루프가 멈추지 않게 한다.
      //(꺼져 있으면 콘솔을 보는 동안 Update가 멈춰 입력이 반영되지 않는다)
      Application.runInBackground = true;

      playerInput.SetEnable();
      _stateMachine = new StateMachine(gameObject, playerStateList.states);

   }

   private void Start()
   {
      ChangeState(PlayerState.IDLE);
   }
   
   public void ChangeState(PlayerState newState)=> _stateMachine?.ChangeState((int)newState);
   

   private void OnDestroy()
   {
      playerInput.SetDisable();
   }
   

   private void Update()
   {
      _stateMachine?.UpdateMachine();
      
   }
}
