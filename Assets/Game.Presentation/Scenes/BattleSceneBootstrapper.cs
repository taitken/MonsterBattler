using Game.Core;
using Game.Core.Logger;
using UnityEngine;

public class BattleSceneBootstrapper : MonoBehaviour
{
    private ILoggerService _loggerService;
    void Awake()
    {
        _loggerService = ServiceLocator.Get<ILoggerService>();   
    }
    void Start()
    {
        _loggerService.Log("BattleSceneBootstrapper started");
        var battleController = new BattleController();
        battleController.StartBattle();
        _loggerService.Log("BattleSceneBootstrapper finished starting battle");
    }
}
