using System;
using UnityEngine;

namespace UtilityClasses
{
    public static class GameTime
    {
        public static float gameSpeedMultiplier { get { return _gameSpeedMultiplier; } }
        private static float _gameSpeedMultiplier = 1f;
        public static float fixedDeltaTime { get { return Time.fixedDeltaTime * gameSpeedMultiplier; } }
        public static float deltaTime { get { return Time.deltaTime * gameSpeedMultiplier; } }
        public static void IncreaseGameSpeed(int steps)
        {
            _gameSpeedMultiplier = Math.Min(5, _gameSpeedMultiplier + (float)steps);
        }
        public static void DecreaseGameSpeed(int steps)
        {
            _gameSpeedMultiplier = Math.Max(1, _gameSpeedMultiplier - (float)steps);
        }
    }
}