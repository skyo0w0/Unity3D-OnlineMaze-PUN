using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using Photon.Pun;
using Zenject;

public class DynamicHoleManager : MonoBehaviourPun
{
    [Inject] private DiContainer _container;
    [Inject] private MazeGenerator _mazeGenerator;
    
    [SerializeField] private float _updateInterval = 5.0f;

    // Вероятность появления ямы в клетке (0..1)
    [Range(0f, 1f)]
    [SerializeField] private float _holeChance = 0.1f;
    
    [SerializeField] private GameObject _holePrefab;
    
    private List<GameObject> _currentHoles = new List<GameObject>();
    private List<GameObject> _inactiveFloors = new List<GameObject>();

    private System.Random _random = new System.Random();
    private Coroutine _routine;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client");
            _routine = StartCoroutine(HoleUpdater());
        }
    }
    
    private IEnumerator HoleUpdater()
    {
        while (true)
        {
            yield return new WaitForSeconds(_updateInterval);
            GenerateAndSyncHoles();
        }
    }


    private void GenerateAndSyncHoles()
    {
        List<GameObject> floors = _mazeGenerator.GetFloors();
        Debug.Log(floors.Count);
        int totalFloors = floors.Count;

        int[] holeData = new int[totalFloors];

        for (int i = 0; i < totalFloors; i++)
        {
            // С вероятностью _holeChance делаем яму
            if (_random.NextDouble() < _holeChance)
            {
                holeData[i] = 1;
            }
            else
            {
                holeData[i] = 0;
            }
        }
        
        photonView.RPC(nameof(RPC_SyncHoles), RpcTarget.All, holeData);
    }


    [PunRPC]
    private void RPC_SyncHoles(int[] holeData)
    {

        foreach (var holeGO in _currentHoles)
        {
            if (holeGO != null) Destroy(holeGO);
        }
        // Вернуть неактивные полы
        foreach (var floor in _inactiveFloors)
        {
            floor.gameObject.SetActive(true);
        }
        _currentHoles.Clear();
        _inactiveFloors.Clear();
        List<GameObject> floors = _mazeGenerator.GetFloors();

        Debug.Log(floors.Count);
        Debug.Log(holeData.Length);


        for (int i = 0; i < holeData.Length; i++)
        {

            if (holeData[i] == 1)
            {
                GameObject floorObj = floors[i];
                if (floorObj == null) continue;
                
                
                // Позиция пола
                Vector3 pos = new Vector3(floorObj.transform.position.x, 
                    floorObj.transform.position.y - 1f, 
                    floorObj.transform.position.z);


                floorObj.gameObject.SetActive(false);
                GameObject holeObj = Instantiate(_holePrefab, pos , Quaternion.identity, transform);
                _container.Inject(holeObj.GetComponent<HoleTriggerComponent>());

                _currentHoles.Add(holeObj);
                _inactiveFloors.Add(floorObj);
            }
        }
    }
}

