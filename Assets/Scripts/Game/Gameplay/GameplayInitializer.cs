using Game.UI;
using Reflex.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class GameplayInitializer : MonoBehaviour, IInstaller
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private BattlePanel _battlePanel;
        [SerializeField] private QuestPanel _questPanel;
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private Camera _gameCamera;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(_gameController);
            containerBuilder.AddSingleton(_battlePanel);
            containerBuilder.AddSingleton(_questPanel);
            containerBuilder.AddSingleton(_loadingOverlay);
            containerBuilder.AddSingleton(_gameCamera);
        }
    }
}
