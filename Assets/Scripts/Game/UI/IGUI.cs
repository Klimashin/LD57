using Game.Gameplay;
using Reflex.Attributes;
using UnityEngine;

namespace Game.UI
{
    public class IGUI : MonoBehaviour
    {
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        
        private GameController _gameController;
        
        [Inject]
        private void Inject(GameController gameController)
        {
            _gameController = gameController;
        }

        private void Start()
        {
            _gameController.OnLose.AddListener(() => _losePanel.SetActive(true));
            _gameController.OnWin.AddListener(() => _winPanel.SetActive(true));
        }
    }
}
