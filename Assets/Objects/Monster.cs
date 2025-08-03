using UnityEngine;

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
        testService.Log();
    }
}
