using UnityEngine;

namespace _01.Script.Agents.Interface
{
    public interface IagnetMover
    {
        void AddForceToAgent(Vector2 force);
        void StopImmediately(bool isX, bool isY);
        void SetMovement(Vector2 dir);
    }
}