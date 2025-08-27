using Game.Presentation.Scenes.Battle.Controllers;
using UnityEngine;

public class BattleSceneUIController : MonoBehaviour
{

    [SerializeField] private RewardsWindow _rewardsWindow;
    void Awake()
    {
        var pauseUIController = gameObject.AddComponent<BattlePauseUIController>();
        pauseUIController.Initialize(_rewardsWindow);
    }

    void Update()
    {

    }
}
