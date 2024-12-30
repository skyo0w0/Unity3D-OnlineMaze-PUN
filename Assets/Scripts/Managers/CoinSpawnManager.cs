using System.Collections;
using System.Collections.Generic;
using Gameplay;
using Photon.Pun;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class CoinSpawnManager : MonoBehaviourPun
    {
        [Inject] private MazeGenerator _mazeGenerator;
        
        [SerializeField] private float _updateInterval = 5.0f;
    
        [SerializeField] private static int _maxCoinsOnMap = 8;
        
        [SerializeField] private GameObject _coinPrefab;

        private List<GameObject> _floors;
    

        private int _segments = 4;
    
        private int _floorsPerSegment;
        
        private GameObject[] _currentCoins = new GameObject[_maxCoinsOnMap];
        private List<Vector3> _coinPositions = new List<Vector3>();

        private System.Random _random = new System.Random();
        private Coroutine _routine;
    
        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Master Client");
                _routine = StartCoroutine(CoinUpdater());
                _floors = _mazeGenerator.GetFloors();
                _floorsPerSegment = _floors.Count / _segments;
            }
        }
    
        private void GenerateAndSyncCoins()
        {
            for (int i = 0; i < _currentCoins.Length; i++)
            {
                if (_currentCoins[i] == null)
                {
                    var coinSegment = i/(_maxCoinsOnMap/_segments);
                    var floorIndex = Random.Range(_floorsPerSegment*coinSegment,_floorsPerSegment * (coinSegment + 1) );
                
                    var floorRenderer = _floors[floorIndex].GetComponent<Renderer>();
                    Vector3 spawnPosition = floorRenderer != null ? floorRenderer.bounds.center : _floors[floorIndex].transform.position;

                    _coinPositions.Add(new Vector3(spawnPosition.x, spawnPosition.y + 1, spawnPosition.z));
                }
                else if (_currentCoins[i] != null)
                {
                    _coinPositions.Add(_currentCoins[i].transform.position);
                }
            }
            
            photonView.RPC(nameof(RPC_SyncCoins), RpcTarget.All, _coinPositions.ToArray());
        
            _coinPositions.Clear();
        }
    
        [PunRPC]
        private void RPC_SyncCoins(Vector3[] coinPositions)
        {

            for (int i = 0; i < _maxCoinsOnMap; i++)
            {
                if (_currentCoins[i] == null)
                {
                    GameObject coin = Instantiate(_coinPrefab, coinPositions[i], Quaternion.identity,transform);
                    var coinComponent = coin.GetComponent<Coin>();
                    if (coinComponent != null)
                    {
                        coinComponent.Id = i; 
                    }
                    
                    _currentCoins[i] = coin;
                }
            }
        
        }
        
        private IEnumerator CoinUpdater()
        {
            while (true)
            {
                yield return new WaitForSeconds(_updateInterval);
                GenerateAndSyncCoins();
            }
        }
    
        
        [PunRPC]
        public void RPC_RemoveCoin(int coinId)
        {
            if (coinId < 0 || coinId >= _currentCoins.Length) return;

            if (_currentCoins[coinId] != null)
            {
                // Удаляем монету из сцены
                Destroy(_currentCoins[coinId]);

                // Удаляем монету из списка
                _currentCoins[coinId] = null;
            }
        }
    }
}
