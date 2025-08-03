using System;
using UnityEngine;
public class App : MonoBehaviour
{
    void Awake()
    {
        var diContainer = ServiceContainer.Instance;

        // Singletons

        // Scoped
        diContainer.RegisterAsTransient<ITestService, TestService>();
    }
}