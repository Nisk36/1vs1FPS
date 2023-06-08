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
    public GameObject SpawnPlayer()
    {
        //ランダムにスポーンポイントを変数に格納
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        //NetworkObjectとしてplayerを生成
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("SpawnPlayer");

        return player;
    }

    public void Relocate(int actor, GameObject player)
    {
        player.transform.position = spawnPoints[actor - 1].position;
        player.transform.rotation = spawnPoints[actor - 1].rotation;
    }
}
