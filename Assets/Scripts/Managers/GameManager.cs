using System.Collections;
using Cinemachine;
using Managers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Zenject;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab; // Префаб игрока
    [Inject] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private MazeGenerator _mazeGenerator;
    private DynamicHoleManager _holeManager;
    private CoinSpawnManager _coinSpawnManager;
    private DiContainer _container;
    
    [Inject]
    public void Construct(DiContainer container)
    {
        _container = container;
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom: We have joined a room!");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("We are MasterClient -> instantiate MazeGenerator");
            GameObject mazeObj = PhotonNetwork.Instantiate("MazeGenerator", Vector3.zero, Quaternion.identity);
            _mazeGenerator = mazeObj.GetComponent<MazeGenerator>();
            _mazeGenerator.GenerateMaze(); 
            _container.Bind<MazeGenerator>().FromInstance(_mazeGenerator).AsSingle();
            
            GameObject hlMnj = PhotonNetwork.Instantiate("HoleManager", Vector3.zero, Quaternion.identity);
            _holeManager = hlMnj.GetComponent<DynamicHoleManager>();
            _container.Bind<DynamicHoleManager>().FromInstance(_holeManager).AsSingle();
            _container.Inject(_holeManager);
            
            GameObject coinSpawn = PhotonNetwork.Instantiate("CoinManager", Vector3.zero, Quaternion.identity);
            _container.Inject(coinSpawn.GetComponent<CoinSpawnManager>());
        }
        else
        {
            StartCoroutine(WaitForDependenciesAndSpawn());
        }
        
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPlayer();
        }
    }
    
    private IEnumerator WaitForDependenciesAndSpawn()
    {
        while (_mazeGenerator == null)
        {
            _mazeGenerator = FindObjectOfType<MazeGenerator>();
            if (_mazeGenerator != null)
            {
                RegisterMazeGenerator(_mazeGenerator);
                break;
            }

            yield return null;
        }
        
        while (_holeManager == null)
        {
            _holeManager = FindObjectOfType<DynamicHoleManager>();
            if (_holeManager != null)
            {
                RegisterHoleManager(_holeManager);
                break;
            }

            yield return null;
        }
        
        _container.Inject(_holeManager);
        
        SpawnPlayer();
    }
    
    private void RegisterMazeGenerator(MazeGenerator mazeGenerator)
    {
        if (!_container.HasBinding<MazeGenerator>())
        {
            _container.Bind<MazeGenerator>().FromInstance(mazeGenerator).AsSingle().NonLazy();
        }
    }

    private void RegisterHoleManager(DynamicHoleManager holeManager)
    {
        if (!_container.HasBinding<DynamicHoleManager>())
        {
            _container.Bind<DynamicHoleManager>().FromInstance(holeManager).AsSingle().NonLazy();
        }
    }

    private GameObject CreateCameraConfiner()
    {
        var size = _mazeGenerator.GetSize();
        GameObject cameraConfiler = new GameObject("CameraConfiler");
        Instantiate(cameraConfiler, Vector3.zero, Quaternion.identity);
        cameraConfiler.AddComponent<BoxCollider>();
        var boxCollider = cameraConfiler.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(size.x, 10, size.y);
        boxCollider.center = new Vector3(size.x/2, 5, size.y/2);
        
        return cameraConfiler;
    }

    private void SetupCamera(GameObject player, int playerIndex)
    {
        float[] angles = { 45f, 315f, 135f, 225f };
        _cinemachineVirtualCamera.Follow = player.transform;
        Vector3 currentEulerAngles = _cinemachineVirtualCamera.transform.rotation.eulerAngles;
        currentEulerAngles.y = angles[playerIndex];
        _cinemachineVirtualCamera.transform.rotation = Quaternion.Euler(currentEulerAngles); 
        //_cinemachineVirtualCamera.GetComponent<CinemachineConfiner>().m_BoundingVolume = 
        //   CreateCameraConfiner().GetComponent<BoxCollider>();
    }
    
    public void SpawnPlayer()
    {
        Debug.Log((PhotonNetwork.LocalPlayer.ActorNumber - 1));

        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        
        Debug.Log(_mazeGenerator != null);
        
        Vector3 spawnPosition = _mazeGenerator.PlayerSpawnPoints[playerIndex];
        
        var player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        
        var playerScore = player.GetComponent<PlayerScoreComponent>();
        
        _container.Bind<PlayerScoreComponent>().FromInstance(playerScore);
        
        _container.Inject(playerScore);
        
        SetupCamera(player,playerIndex);
    }
    

}