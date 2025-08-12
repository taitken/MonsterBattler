
using System.Threading;
using System.Threading.Tasks;
using Game.Application.Interfaces;
using UnityEngine;


public sealed class FadeController : IFadeController
{
    public Task FadeOut(float duration = 0.25f, CancellationToken ct = default)
    {
        // Implement fade out logic here
        Debug.Log($"Fading out over {duration} seconds.");
        return Task.CompletedTask; // Replace with actual fade logic
    }
    public Task FadeIn(float duration = 0.25f, CancellationToken ct = default)
    {
        // Implement fade out logic here
        Debug.Log($"Fading in over {duration} seconds.");
        return Task.CompletedTask; // Replace with actual fade logic
    }
}