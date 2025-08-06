using System;
using Game.Core.Logger;
using UnityEngine;

namespace Game.Infrastructure.Services
{
    public class LoggerService : ILoggerService
    {
        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        public void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}