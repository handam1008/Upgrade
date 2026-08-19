using System;
using Agents.Enemies.BT;
using Agents.Enemies.BT.Events;
using DevLib.TileAstar;
using SkillSystem;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies
{
    [RequireComponent(typeof(PathAgent), typeof(BehaviorGraphAgent))]
    public class AbstractEnemy : AbstractAgent
    {
        [SerializeField] private bool isDebugMode;
        
        [field: SerializeField] public EnemyDataSO EnemyData {get; private set;}
        public BehaviorGraphAgent BtAgent { get; private set; }
        public NavModule Nav { get; private set; }
        public EnemySensor Sensor { get; private set; } //이것도 사실 인터페이스화 시켜야하는게 정석이다.
        public ISkillModule SkillModule { get; private set; }

        private StateChannel _stateChannel;
        
        
        protected override void InitializeModules()
        {
            base.InitializeModules();
            Nav = GetModule<NavModule>();
            BtAgent = GetComponent<BehaviorGraphAgent>();
            Sensor = GetModule<EnemySensor>();
            SkillModule = GetModule<ISkillModule>();
        }

        protected override void HandleHit()
        {
            if (IsDead) return;
            _stateChannel.SendEventMessage(EnemyState.HIT);
        }

        private void Start()
        {
            //Awake는 BT초기화가 이루어지는 곳이라. 이때 셋팅하면 셋팅이 될 수도 있고 안될수도 있게돼.
            BtAgent.SetVariableValue(BtVars.Enemy, this);
            BtAgent.SetVariableValue(BtVars.OriginPosition, transform.position);

            if (BtAgent.GetVariable(BtVars.StateChannel, out BlackboardVariable<StateChannel> stateChannel))
            {
                _stateChannel = stateChannel;
            }
            else
            {
                Debug.Log($"StateChannel 이 없습니댜ㅏ. : {gameObject}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!isDebugMode || EnemyData == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, EnemyData.AttackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, EnemyData.DetectRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, EnemyData.SignalLostRange);
        }
    }
}