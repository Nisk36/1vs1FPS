using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    //SpawnPoint格納
    [SerializeField]
    private Transform[] spawnPoints;

    //生成するプレイヤーオブジェクト
    [SerializeField]
    private GameObject playerPrefab;
    //生成したプレイヤーオブジェクト
    private GameObject player;

    public void Initilize()
    {
        //スポーンオブジェクト非表示
        foreach(Transform spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }
    //ネットワークオブジェクトとしてプレイヤーを生成する
    public GameObject SpawnPlayer(bool isMasterClient)
    {
        // Masterなら0, そうでないなら1にスポーン
        int spawnIndex = isMasterClient ? 0 : 1;
        Transform spawnPoint = spawnPoints[spawnIndex];
        
        //NetworkObjectとしてplayerを生成
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

        return player;
    }

    public void Relocate(bool isMasterClient, GameObject player)
    {
        int spawnIndex = isMasterClient ? 0 : 1;
        player.transform.position = spawnPoints[spawnIndex].position;
        player.transform.rotation = spawnPoints[spawnIndex].rotation;
    }
}
