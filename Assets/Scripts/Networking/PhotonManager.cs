using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string _NickName = "DUDE";

    private void Start()
    {
        PhotonNetwork.NickName = _NickName;
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        print("OnConnectedToMaster");

        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 20,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("GD54", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        print($"Room [{PhotonNetwork.CurrentRoom.Name}] Joined");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        print($"Player [{newPlayer.NickName}] Joined our room");
    }


}
