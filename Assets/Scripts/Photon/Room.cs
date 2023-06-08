using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class Room : MonoBehaviour
{
    public Text buttonText;
    private RoomInfo info;

    public void RegisterRoomDetails(RoomInfo info)//ルーム情報格納
    {
        this.info = info;

        buttonText.text = this.info.Name;
    }

    public void OpenRoom()
    {
        PhotonManager.instance.JoinRoom(info);
    }
}
