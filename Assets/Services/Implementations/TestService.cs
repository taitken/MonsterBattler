using System;
using UnityEngine;

public class TestService : ITestService
{
    public TestService()
    {
        Debug.Log("Test Service Initialised");
    }
    public void log()
    {
        Console.WriteLine("test");
    }
}