using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class Room : MonoBehaviour
{
    public Text buttonText;
    private RoomInfo info;

    public void RegisterRoomDetails(RoomInfo info)//ƒ‹[ƒ€î•ñŠi”[
    {
        this.info = info;

        buttonText.text = this.info.Name;
    }

    public void OpenRoom()
    {
        PhotonManager.instance.JoinRoom(info);
    }
}
