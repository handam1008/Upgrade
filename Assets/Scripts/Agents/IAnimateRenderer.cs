using UnityEngine;

namespace Agents
{
    public interface IAnimateRenderer
    {
        Animator Animator { get; }
        Vector2 FacingDirection { get; }
        void SetMovementDirection(Vector2 direction);
        void RenderClip(int clipHash);
        void RenderClipNotPlaying(int clipHash);
    }
}