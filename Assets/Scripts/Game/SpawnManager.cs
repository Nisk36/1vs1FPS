using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    //SpawnPoint�i�[
    [SerializeField]
    private Transform[] spawnPoints;

    //��������v���C���[�I�u�W�F�N�g
    [SerializeField]
    private GameObject playerPrefab;
    //���������v���C���[�I�u�W�F�N�g
    private GameObject player;

    public void Initilize()
    {
        //�X�|�[���I�u�W�F�N�g��\��
        foreach(Transform spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }
    }
    //�l�b�g���[�N�I�u�W�F�N�g�Ƃ��ăv���C���[�𐶐�����
    public GameObject SpawnPlayer()
    {
        //�����_���ɃX�|�[���|�C���g��ϐ��Ɋi�[
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        //NetworkObject�Ƃ���player�𐶐�
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
