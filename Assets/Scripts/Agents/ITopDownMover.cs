using System;
using UnityEngine;

namespace Agents
{
    public interface ITopDownMover
    {
        event Action<Vector2> OnMovementChange;
        float MoveSpeed { get; }
        void SetMovementSpeed(float speed);
        void SetMovement(Vector2 movement);
        void StopImmediately();
    }
}