using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class Launcher : MonoBehaviourPunCallbacks
{
    private const string RoomName = "MazeRoom";

    [Inject]
    private void Initialize()
    {
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        PhotonNetwork.JoinOrCreateRoom(RoomName, new Photon.Realtime.RoomOptions { MaxPlayers = 4 }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + RoomName);
        
    }
}