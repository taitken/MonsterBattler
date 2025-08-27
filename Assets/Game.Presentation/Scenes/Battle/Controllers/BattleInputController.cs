using Game.Application.Interfaces;
using Game.Core;
using UnityEngine;

namespace Game.Presentation.Scenes.Battle.Controllers
{
    public class BattleInputController : MonoBehaviour
    {
        private IPauseService _pauseService;
        
        void Start()
        {
            _pauseService = ServiceLocator.Get<IPauseService>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _pauseService.TogglePause();
            }
        }
    }
}