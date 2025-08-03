using System;
using UnityEngine;
public class App : MonoBehaviour
{
    void Awake()
    {
        var diContainer = ServiceContainer.Instance;

        // Singletons

        // Scoped
        diContainer.RegisterAsScoped<ITestService, TestService>();
        diContainer.RegisterAsSingleton<ILoggerService, LoggerService>();
    }
}