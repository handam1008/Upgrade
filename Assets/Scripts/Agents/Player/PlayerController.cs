using Agents;
using Agents.Player.FSM;
using DevLib.FsmSystem.Runtime;
using GameSystem;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : AbstractAgent
{
   [field:SerializeField] public PlayerInputSO playerInput { get; private set; }
   [SerializeField] private StateListSO playerStateList;
   [SerializeField] private float shakeForce = 0.5f;
   
   private StateMachine _stateMachine;
   private CinemachineImpulseSource _impulseSource;

   protected override void InitializeModules()
   {
      base.InitializeModules();

      //에디터 창이 포커스를 잃어도 게임 루프가 멈추지 않게 한다.
      //(꺼져 있으면 콘솔을 보는 동안 Update가 멈춰 입력이 반영되지 않는다)
      Application.runInBackground = true;

      playerInput.SetEnable();
      _stateMachine = new StateMachine(gameObject, playerStateList.states);
      
      _impulseSource = GetComponent<CinemachineImpulseSource>();

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
      
      _impulseSource.GenerateImpulseWithVelocity(ActionData.HitNormal * shakeForce);
         
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
