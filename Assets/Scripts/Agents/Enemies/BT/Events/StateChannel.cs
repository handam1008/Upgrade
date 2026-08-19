using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Agents.Enemies.BT.Events
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/StateChannel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "StateChannel", message: "Change [State]", category: "Events", id: "f3f0f02c77225867e578cf1bb4138fd6")]
    public sealed partial class StateChannel : EventChannel<EnemyState> { }
}

