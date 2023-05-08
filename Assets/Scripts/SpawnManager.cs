using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    //SpawnPoint格納
    public Transform[] spawnPoints;


    //生成するプレイヤーオブジェクト
    public GameObject playerPrefab;
    //生成したプレイヤーオブジェクト
    private GameObject player;



    //スポーンまでのインターバル
    public float respawnInterval = 5.0f;

    private void Start()
    {
        //スポーンオブジェクト非表示
        foreach(Transform spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
    //ランダムにスポーンポイント選んで取得
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    //ネットワークオブジェクトとしてプレイヤーを生成する
    public void SpawnPlayer()
    {
        //ランダムにスポーンポイントを変数に格納
        Transform spawnPoint = GetSpawnPoint();

        //NetworkObjectとしてplaterを生成
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    //削除とリスポーン
    public void Die()
    {
        if(player != null)
        {
            //5秒後にリス
            Invoke("SpawnPlayer", 5f);
        }

        PhotonNetwork.Destroy(player);
    }
}
