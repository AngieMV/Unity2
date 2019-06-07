using Photon.Pun;
using UnityEngine;

public class NetDataSyncer : MonoBehaviourPun, IPunObservable
{

    // [ContextMenu("Localize")]
    // private void LocalizeMe()
    // {
    //     if(photonView.IsMine == false)
    //     {
    //         Destroy(GetComponent<PlayerController>());
    //         Destroy(GetComponent<Rigidbody>());
    //     }
    // }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting) // Locally Controller, we send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else // Someone else is controlling, we receive data
        {
            transform.position = (Vector3) stream.ReceiveNext();
            transform.rotation = (Quaternion) stream.ReceiveNext();
        }
    }

    [SerializeField]
    private string _WhatToSay;

    [ContextMenu("Send Text")]
    private void CallRPC()
    {
        photonView.RPC("RPC_SaySomething", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_SaySomething(string something, PhotonMessageInfo info)
    {
        print($"User [{info.Sender.NickName}]: {something}");
    }


}
