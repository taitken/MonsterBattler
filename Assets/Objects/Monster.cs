using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Monster : MonoObject
{
    private ITestService testService;
    void Awake()
    {
        Debug.Log("Monster awake");
        testService = Inject<ITestService>();
    }

    void Start()
    {
        testService.log();
    }
}
