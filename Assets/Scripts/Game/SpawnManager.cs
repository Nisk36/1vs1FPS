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
    public GameObject SpawnPlayer(bool isMasterClient)
    {
        // Master�Ȃ�0, �����łȂ��Ȃ�1�ɃX�|�[��
        int spawnIndex = isMasterClient ? 0 : 1;
        Transform spawnPoint = spawnPoints[spawnIndex];
        
        //NetworkObject�Ƃ���player�𐶐�
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
