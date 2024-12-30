using Managers;
using Photon.Pun;
using Player;
using UnityEngine;

namespace Gameplay
{
    public class CoinOnTriggerEnter : MonoBehaviourPun
    {
        private Coin _coin => gameObject.GetComponent<Coin>();
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerMovement>() != null)
            {
                other.GetComponent<PlayerScoreComponent>().AddScore(5);
                Debug.Log($"Coin collected with ID: {_coin.Id}");
                var coinSpawnManager = gameObject.GetComponentInParent<CoinSpawnManager>();
                // Удаляем монету через RPC
                coinSpawnManager.photonView.RPC(nameof(CoinSpawnManager.RPC_RemoveCoin), RpcTarget.All, _coin.Id);
            }
        }
    }
}