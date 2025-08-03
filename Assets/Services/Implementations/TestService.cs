using System;
using UnityEngine;

public class TestService : ITestService
{
    ILoggerService _loggerService;
    public TestService(ILoggerService loggerService)
    {
        _loggerService = loggerService;
        _loggerService.Log("Test Service Initialised");
    }

    public void Log()
    {
        _loggerService.Log("Test Service Log Method Called");
    }
}