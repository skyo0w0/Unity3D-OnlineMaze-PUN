using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Installers
{
    public class GameInstaller : MonoInstaller
    {
        // Допустим, вы хотите ссылки на префабы
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private Launcher _launcherPrefab;
        [SerializeField] private DynamicHoleManager _holeManager;
        [SerializeField] private CinemachineVirtualCamera _mainCamera;
        [SerializeField] private GameObject _canvasPrefab;

        public override void InstallBindings()
        {

            // 3) Регистрируем Launcher
            Container.Bind<Launcher>()
                .FromComponentInNewPrefab(_launcherPrefab)
                .AsSingle()
                .NonLazy();
            
            // 2) Регистрируем GameManager
            Container.Bind<GameManager>()
                .FromComponentInNewPrefab(_gameManagerPrefab)
                .AsSingle()
                .NonLazy();
            
            Container.Bind<CinemachineVirtualCamera>().FromInstance(_mainCamera).AsSingle().NonLazy();
            
            
            var canvasGameObject = Instantiate(_canvasPrefab);
            canvasGameObject.GetComponent<Canvas>().worldCamera = Camera.main;

            var scoreDisplay = canvasGameObject.GetComponentInChildren<ScoreDisplayComponent>();
            if (scoreDisplay != null)
            {
                Container.Bind<ScoreDisplayComponent>()
                    .FromInstance(scoreDisplay)
                    .AsSingle()
                    .NonLazy();
            }
            else
            {
                Debug.LogError("ScoreDisplayComponent not found on Canvas prefab!");
            }
            
            
            
            
        }
    }
}