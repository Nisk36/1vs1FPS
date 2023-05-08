using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    //SpawnPoint�i�[
    public Transform[] spawnPoints;


    //��������v���C���[�I�u�W�F�N�g
    public GameObject playerPrefab;
    //���������v���C���[�I�u�W�F�N�g
    private GameObject player;



    //�X�|�[���܂ł̃C���^�[�o��
    public float respawnInterval = 5.0f;

    private void Start()
    {
        //�X�|�[���I�u�W�F�N�g��\��
        foreach(Transform spawnPoint in spawnPoints)
        {
            spawnPoint.gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
    //�����_���ɃX�|�[���|�C���g�I��Ŏ擾
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    //�l�b�g���[�N�I�u�W�F�N�g�Ƃ��ăv���C���[�𐶐�����
    public void SpawnPlayer()
    {
        //�����_���ɃX�|�[���|�C���g��ϐ��Ɋi�[
        Transform spawnPoint = GetSpawnPoint();

        //NetworkObject�Ƃ���plater�𐶐�
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    //�폜�ƃ��X�|�[��
    public void Die()
    {
        if(player != null)
        {
            //5�b��Ƀ��X
            Invoke("SpawnPlayer", 5f);
        }

        PhotonNetwork.Destroy(player);
    }
}
