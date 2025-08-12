
using System;
using Game.Core;
using UnityEngine;

namespace Game.Presentation.Helpers
{

    /// <summary>
    /// Represents a command entry for UI buttons, containing the command asset, topic, dispatch type, and priority.
    /// </summary>
    [Serializable]
    public struct CommandEntry
    {
        public ScriptableObject commandAsset; // must implement ICommandAsset
        public string topic;                  // optional
        public Dispatch dispatch;             // Immediate / NextFrame / Queued (your enum)
        public int priority;                  // used if Queued
    }
}