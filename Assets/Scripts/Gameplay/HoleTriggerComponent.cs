using Photon.Pun;
using Player;
using UnityEngine;
using Zenject;

namespace Gameplay
{
    public class HoleTriggerComponent : MonoBehaviour
    {
    
        [Inject] private MazeGenerator _mazeGenerator;
    
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Enter");

            if (other.gameObject.GetComponent<PlayerMovement>())
            {
                PhotonView photonView = other.gameObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                
                    Vector3 spawnPosition = _mazeGenerator.PlayerSpawnPoints[playerIndex];
                
                    photonView.RPC("RespawnAtPosition", RpcTarget.All, spawnPosition);
                }
            }
        }
    }
}
