using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Assertions;


//���A���^�C��API�̃C�x���g�R�[���o�b�N�B�T�[�o�[����̃C�x���g�ƁAOpRaiseEvent����ăN���C�A���g���瑗�M���ꂽ�C�x���g���J�o�[
//�Q�[���̃V�[�P���X�i�s��S��
public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public List<PlayerInfo> playerList = new List<PlayerInfo>();//�v���C���[���������N���X�̃��X�g
    
    public enum EventCodes : byte//����C�x���g�Fbyte�͈����f�[�^(0 �` 255)
    {
        NewPlayer,//�V�����v���C���[�����}�X�^�[�ɑ���
        ListPlayers,//�S���Ƀv���C���[�������L
        UpdateStat,//�L���f�X���̍X�V
        ShareNum,//Master��RoundNum��DicisionNum�����L
    }

    /// <summary>
    /// �Q�[���̏��
    /// </summary>
    public enum GameState
    {
        Waiting,
        Playing,
        RoundEnding,
        Ending
    }
    public GameState state;//��Ԃ��i�[

    private List<PlayerInformation> playerInfoList = new List<PlayerInformation>();

    //�N���A�p�l����\�����Ă��鎞��
    public float waitAfterEnding = 5.0f;
    
    //���݂̃��E���h��
    private int roundNumber;
    //���E���h�X�^�[�g�҂��̕b��
    [SerializeField]
    private float startWaitSeconds = 3.0f;
    //���E���h�I���̂Ƃ��̕b��
    [SerializeField]
    private float endWaitSeconds = 3.0f;
    //WaitForSeconds�L���b�V���p�ϐ�
    private WaitForSeconds startWait;
    private WaitForSeconds endWait;

    private PlayerInfo gameWinner;
    private PlayerInfo roundWinner;
    
    //playerPrefab
    private GameObject playerObj;
    private PlayerController player;
    
    //���s�����p���� (0.5�����Ȃ�Master���ŏ�(roundNumber���)������Ώ���, 0.5�ȏ�Ȃ畉���X�^�[�g)
    private float decisionNum = 0.0f;

    //�X�|�[���Ǘ��pManager
    [SerializeField] 
    private SpawnManager spawnManager = null;
    //ui�Ǘ��pManager
    [SerializeField]
    private UIManager uiManager = null;

    [SerializeField] 
    private Transform bulletImpactParent = null;
    
    //Timer
    [SerializeField]
    private Timer timer;

    private void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }

    private void Start()
    {
        //�l�b�g���[�N�ڑ�����Ă��Ȃ��ꍇ
        if (!PhotonNetwork.IsConnected)
        {
            //�^�C�g���ɖ߂�
            SceneManager.LoadScene(0);
        }
        else //�q�����Ă���ꍇ
        {
            //�X�|�[��
            Assert.IsNotNull(spawnManager);
            Assert.IsNotNull(timer);
            Assert.IsNotNull(uiManager);
            //�}�X�^�[�Ƀ��[�U�[���𔭐M����
            NewPlayerGet(PhotonNetwork.NickName);
            //WaitSeconds�̓L���b�V�����Ďg��
            startWait = new WaitForSeconds(startWaitSeconds);
            endWait = new WaitForSeconds(endWaitSeconds);
            //��Ԃ�ҋ@���ɐݒ肷��
            state = GameState.Waiting;
            //�^�C�}�[������
            timer.Initilize(uiManager.TimerText);
            //�X�|�[��
            spawnManager.Initilize();
            playerObj = spawnManager.SpawnPlayer();
            player = playerObj.GetComponent<PlayerController>();
            spawnManager.Relocate(PhotonNetwork.LocalPlayer.ActorNumber, playerObj);
            //�ϐ�������
            roundNumber = 1;
            decisionNum = UnityEngine.Random.value;
            Debug.Log(decisionNum);
        }
    }

    private void FixedUpdate()
    {
        Assert.IsNotNull(uiManager);
        //UI�̖��O�Z�b�g
        SetPlayerName();
    }

    private void Update()
    {
        //Debug.Log(state);
        switch (state)
        {
            case GameState.Waiting: // ���E���h�J�n�O
                if (timer == null) return;
                //roundNumber�X�V
                roundNumber = 1;
                for (int i = 0; i < playerList.Count; i++)
                {
                    roundNumber += playerList[i].kills;
                }
                //roundNumber�\��
                uiManager.SetRoundNumber(roundNumber);

                //dicisionNum����
                ShareNum(decisionNum);
                Debug.Log(decisionNum);
                //�v���C���[�̃��E���h�ڕW���\��
                if (decisionNum <= 0.5f) //roundNum�����������Master������
                {
                    if (roundNumber % 2 == 1) //��̎�
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            uiManager.SetActiveExplainCanEscape(true);
                            uiManager.SetActiveExplainCannotEscape(false);
                        }
                        else
                        {
                            uiManager.SetActiveExplainCannotEscape(true);
                            uiManager.SetActiveExplainCanEscape(false);
                        }
                    }
                    else //roundNum�������̎�
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            uiManager.SetActiveExplainCannotEscape(true);
                            uiManager.SetActiveExplainCanEscape(false);
                        }
                        else
                        {
                            uiManager.SetActiveExplainCanEscape(true);
                            uiManager.SetActiveExplainCannotEscape(false);
                        }
                    }
                }
                else//roundNum��������������Master������
                {
                    if (roundNumber % 2 == 1) //��̎�
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            uiManager.SetActiveExplainCannotEscape(true);
                            uiManager.SetActiveExplainCanEscape(false);
                        }
                        else
                        {
                            uiManager.SetActiveExplainCanEscape(true);
                            uiManager.SetActiveExplainCannotEscape(false);
                        }
                    }
                    else //roundNum�������̎�
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            uiManager.SetActiveExplainCanEscape(true);
                            uiManager.SetActiveExplainCannotEscape(false);
                        }
                        else
                        {
                            uiManager.SetActiveExplainCannotEscape(true);
                            uiManager.SetActiveExplainCanEscape(false);
                        }
                    }
                }
                if (timer.UpdateCountDown(uiManager.CountDownText))
                {
                    state = GameState.Playing;
                    if (player != null)
                    {
                        player.WaittoPlay();
                        uiManager.SetActiveExplainCanEscape(false);
                        uiManager.SetActiveExplainCannotEscape(false);
                    }
                }
                break;
            case GameState.Playing:
                // �Q�[���v���C��
                if (timer != null)
                {
                    timer.UpdateTimer(uiManager.TimerText);   
                }

                if (player != null)
                {
                    if (player._PlayerState == PlayerController.PlayerState.Dead)
                    {
                        state = GameState.RoundEnding;
                        var idList = new List<int>();
                        // PlayerList��Actornumber�i�[
                        for (int i = 0; i < playerList.Count; i++)
                        {   
                            idList.Add(playerList[i].actor);
                        }
                        // �ΐ푊���ActorId�擾
                        int enemyActor = -1;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (idList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
                            {
                                enemyActor = idList[i];
                                Debug.Log(enemyActor);
                            }
                        }
                        //�X�R�A����
                        ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);//�������S���̃C�x���g�Ăяo��
                        if (enemyActor != -1)
                        {
                            ScoreGet(enemyActor, 0, 1);   
                        }
                    }
                    if (timer.IsTimeUp)
                    {
                        state = GameState.RoundEnding;
                        var idList = new List<int>();
                        // PlayerList��Actornumber�i�[
                        for (int i = 0; i < playerList.Count; i++)
                        {   
                            idList.Add(playerList[i].actor);
                        }
                        // �ΐ푊���ActorId�擾
                        int enemyActorId = -1;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (idList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
                            {
                                enemyActorId = idList[i];
                                Debug.Log(enemyActorId);
                            }
                        }
                        //�X�R�A����(MasterClient�ɂ�点��)
                        if (!PhotonNetwork.IsMasterClient) return;
                        if (decisionNum <= 0.5f)//roundNum�����������Master������
                        {
                            if (roundNumber % 2 == 1)//��̎�
                            {
                                ScoreGet(enemyActorId, 1, 1); //���莀�S���̃C�x���g�Ăяo��
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 0, 1); //����������
                            }
                            else
                            {
                                ScoreGet(enemyActorId, 0, 1); //���菟�����̃C�x���g�Ăяo��
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1); //�����s�k��
                            }
                        }
                        else//roundNum��������������Master������
                        {
                            if (roundNumber % 2 == 1)//��̎�
                            {
                                ScoreGet(enemyActorId, 0, 1); //���菟�����̃C�x���g�Ăяo��
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1); //�����s�k��
                            }
                            else
                            {
                                ScoreGet(enemyActorId, 1, 1); //���莀�S���̃C�x���g�Ăяo��
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 0, 1); //����������
                            }
                        }
                    }
                }
                break;
            case GameState.RoundEnding:
                //�^�C�}�[������
                if (timer != null)
                {
                    timer.Initilize(uiManager.TimerText);   
                }
                //���X�|�[��
                if (spawnManager != null && playerObj != null)
                {
                    spawnManager.Relocate(PhotonNetwork.LocalPlayer.ActorNumber, playerObj);   
                }
                foreach (Transform bulletImpact in bulletImpactParent)
                {
                    if (bulletImpact.gameObject != null)
                    {
                        Destroy(bulletImpact.gameObject);
                    }
                }
                
                player.Initailize();
                //state�ύX
                state = GameState.Waiting;
                break;
        }
    }


    //�C�x���g�������ɌĂяo�����
    public void OnEvent(EventData photonEvent)
    {

        if (photonEvent.Code < 200)//200�ȏ��photon���Ǝ��̃C�x���g���g���Ă��邽��200�ȉ��݂̂ɏ���������
        {
            EventCodes eventCode = (EventCodes)photonEvent.Code;//����̃C�x���g�R�[�h���i�[�i�^�ϊ��j
            object[] data = (object[])photonEvent.CustomData;//�C���f�N�T�[��CustomDataKey����āA�C�x���g�̃J�X�^���f�[�^�ɃA�N�Z�X���܂�

            switch (eventCode)
            {
                case EventCodes.NewPlayer://���������C�x���g��NewPlayer�Ȃ�
                    NewPlayerSet(data);//�}�X�^�[�ɐV�K���[�U�[��񏈗�������
                    break;

                case EventCodes.ListPlayers:
                    ListPlayersSet(data);//���[�U�[�������L
                    break;

                case EventCodes.UpdateStat:
                    ScoreSet(data);//
                    break;
                case EventCodes.ShareNum:
                    NumSet(data);
                    break;
            }
        }
    }

    /// <summary>
    /// �R���|�[�l���g���I���ɂȂ�ƌĂ΂��
    /// </summary>
    public override void OnEnable()
    {
        //��������Ă���R�[���o�b�N�E�C���^�[�t�F�[�X�̃R�[���o�b�N�p�I�u�W�F�N�g��o�^���܂��B
        PhotonNetwork.AddCallbackTarget(this);//�ǉ�����
    }
    
    /// <summary>
    /// �R���|�[�l���g���I�t�ɂȂ�ƌĂ΂��
    /// </summary>
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);//�폜����
    }

    /// <summary>
    ///  �V�K���[�U�[���l�b�g���[�N�o�R�Ń}�X�^�[�Ɏ����̏��𑗂�
    /// </summary>
    public void NewPlayerGet(string name)//�C�x���g�𔭐�������֐�
    {
        object[] info = new object[4];//�f�[�^�i�[�z����쐬
        info[0] = name;//���O
        info[1] = PhotonNetwork.LocalPlayer.ActorNumber;//���[�U�[�Ǘ��ԍ�
        info[2] = 0;//�L��
        info[3] = 0;//�f�X


        // RaiseEvent�ŃJ�X�^���C�x���g�𔭐��F�f�[�^�𑗂�
        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer,//����������C�x���g
            info,//������́i�v���C���[�f�[�^�j
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },//���[���}�X�^�[�����ɑ��M�����ݒ�
            new SendOptions { Reliability = true }//�M�����̐ݒ�F�M���ł���̂Ńv���C���[�ɑ��M�����
        );

    }


    /// <summary>
    ///�����Ă����V�v���C���[�̏������X�g�Ɋi�[����
    /// </summary>
    public void NewPlayerSet(object[] data)//�}�X�^�[���s�������@�C�x���g�������ɍs������
    {
        Debug.Log("NewPlayerSet");
        PlayerInfo player = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);//�l�b�g���[�N����v���C���[�����擾

        playerList.Add(player);//���X�g�ɒǉ�

        ListPlayersGet();//�}�X�^�[���擾�����v���C���[���𑼂̃v���C���[�ɋ��L
    }
    
    /// <summary>
    /// �擾�����v���C���[�������[�����̑S�v���C���[�ɑ��M����
    /// </summary>
    public void ListPlayersGet()//�}�X�^�[���s�������@�C�x���g�𔭐�������֐�
    {
        object[] info = new object[playerList.Count + 1];//�Q�[���̏󋵁��v���C���[���i�[�z��쐬

        info[0] = state;//�ŏ��ɂ̓Q�[���̏󋵂�����

        for (int i = 0; i < playerList.Count; i++) //�Q�����[�U�[�̐������[�v
        {
            object[] temp = new object[4];//�ꎞ�I�i�[����z��

            temp[0] = playerList[i].name;
            temp[1] = playerList[i].actor;
            temp[2] = playerList[i].kills;
            temp[3] = playerList[i].deaths;

            info[i + 1] = temp;//�v���C���[�����i�[���Ă���z��Ɋi�[����B0�ɂ̓Q�[���̏�Ԃ������Ă��邽�߁{�P����B
        }


        //RaiseEvent�ŃJ�X�^���C�x���g�𔭐��F�f�[�^�𑗂�
        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers,////����������C�x���g
            info,//������́i�v���C���[�f�[�^�j
            new RaiseEventOptions { Receivers = ReceiverGroup.All },//�S���ɑ��M����C�x���g�ݒ�
            new SendOptions { Reliability = true }//�M�����̐ݒ�F�M���ł���̂Ńv���C���[�ɑ��M�����
        );
    }
    
    /// <summary>
    /// ListPlayersGet�ŐV�����v���C���[��񂪑����Ă����̂ŁA���X�g�Ɋi�[����
    /// </summary>
    /// <param name="data">data = {0 : �Q�[�����, 1 : </param>
    public void ListPlayersSet(object[] data)//�C�x���g������������Ă΂��֐��@�S�v���C���[�ŌĂ΂��
    {
        playerList.Clear();//���Ɏ����Ă���v���C���[�̃��X�g��������

        state = (GameState)data[0];//�Q�[����Ԃ�ϐ��Ɋi�[

        var alreadyAddedActor = new List<int>();

        for (int i = 1; i < data.Length; i++)//1�ɂ��� 0�̓Q�[����ԂȂ̂�1����n�߂�
        {
            object[] info = (object[])data[i];//

            PlayerInfo player = new PlayerInfo(
                (string)info[0],//���O
                (int)info[1],//�Ǘ��ԍ�
                (int)info[2],//�L��
                (int)info[3]//�f�X
            );

            bool isAlreadyAdded = false;
            for (int j = 0; j < alreadyAddedActor.Count; j++)
            {
                if (player.actor == alreadyAddedActor[i])
                {
                    isAlreadyAdded = true;
                }
            }

            if (!isAlreadyAdded)
            {
                playerList.Add(player);//���X�g�ɒǉ�   
            }
        }
        //�Q�[���̏�Ԕ���
        StateCheck();
    }
    
    /// <summary>
    /// �L������f�X�����擾����֐�(�v���C���[���ʐ��A�L�����f�X�𐔒l�Ŕ���A���Z���鐔�l)
    /// </summary>
    public void ScoreGet(int actor, int stat, int amount)
    {
        object[] package = new object[] { actor, stat, amount };

        //�f�[�^�𑗂�C�x���g
        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStat,//����������C�x���g
            package,//������́i�v���C���[�̃L���f�X�f�[�^�j
            new RaiseEventOptions { Receivers = ReceiverGroup.All },//�S���ɑ��M����C�x���g�ݒ�
            new SendOptions { Reliability = true }//�M�����̐ݒ�F�M���ł���̂Ńv���C���[�ɑ��M�����
        );
    }

    /// <summary>
    /// �}�X�^�[��dicisionNum�����L����֐�
    /// </summary>
    public void ShareNum(float dicisionNum)
    {
        object[] package = new object[] { dicisionNum };

        PhotonNetwork.RaiseEvent((byte)EventCodes.ShareNum,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    /// <summary>
    /// dicisionNum���擾����֐�
    /// </summary>
    public void NumSet(object[] data)
    {
        float sendDicisionNum = (float)data[0];
        decisionNum = sendDicisionNum;
    }

    /// <summary>
    /// �󂯎�����f�[�^���烊�X�g�ɃL���f�X����ǉ�
    /// </summary>
    public void ScoreSet(object[] data)
    {
        int actor = (int)data[0];//���ʐ�
        int stat = (int)data[1];//�L���Ȃ̂��f�X�Ȃ̂�
        int amount = (int)data[2];//���Z���鐔�l


        for (int i = 0; i < playerList.Count; i++)//�v���C���[�̐������[�v
        {
            if (playerList[i].actor == actor)//�����擾�����v���C���[�Ɛ��l�����v�����Ƃ�
            {
                switch (stat)
                {
                    case 0://�L��
                        playerList[i].kills += amount;

                        break;

                    case 1://�f�X
                        playerList[i].deaths += amount;

                        break;
                }
                break;//�����͂ł����̂ł���ȍ~for�����񂳂Ȃ����߂Ƀu���C�N����
            }
        }

        //�N���A���������������m�F
        TargetScoreCheck();

    }


    //�X�V���X�R�A�{�[�h���J��
    public void ShowScoreboard()
    {
        //�X�R�A�J��
        uiManager.ChangeScoreUI();

        //���ݕ\������Ă���X�R�A���폜
        foreach (PlayerInformation info in playerInfoList)
        {
            Destroy(info.gameObject);
        }

        playerInfoList.Clear();
        
        //�Q�����[�U�[�����[�v
        foreach(PlayerInfo player in playerList)
        {
            //�X�R�A�\��UI�쐬���Ċi�[
            PlayerInformation newPlayerDisplay = Instantiate(uiManager.info, uiManager.info.transform.parent);

            //UI�Ƀv���C���\���𔽉f
            newPlayerDisplay.SetPlayerDetailes(player.name, player.kills, player.deaths);

            //�\��
            newPlayerDisplay.gameObject.SetActive(true);

            //���X�g�Ɋi�[
            playerInfoList.Add(newPlayerDisplay);
        }

        
    }

    //�Q�[���N���A������B���������m�F
    public void TargetScoreCheck()
    {
        bool clear = false;

        //�N���A����
        foreach (PlayerInfo player in playerList)
        {
            //�L��������
            if (player.kills >= 3)
            {
                clear = true;
                break;
            }
        }

        int maxkillCount1 = 0;
        int maxkillCount2 = 0;

        foreach (PlayerInfo player in playerList)
        {
            //UI�ύX
            if (player.actor == 1)
            {
                if (maxkillCount1 <= player.kills)
                {
                    maxkillCount1 = player.kills;
                    uiManager.SetPlayer1Round(maxkillCount1);
                }
            }
            else if (player.actor == 2)
            {
                if (maxkillCount2 <= player.kills)
                {
                    maxkillCount2 = player.kills;
                    uiManager.SetPlayer2Round(maxkillCount2);
                }
            }
        }

        if (clear) {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                //�Q�[���̏�Ԃ�ύX
                state = GameState.Ending;

                //�Q�[���v���C��Ԃ����L
                ListPlayersGet();
            }
        }
        else
        {
            state = GameState.RoundEnding;
        }
    }

    public void StateCheck()
    {
        if(state == GameState.Ending)
        {
            //�Q�[���I���֐�
            EndGame();
        }
    }
    //�Q�[���I���֐�
    public void EndGame()
    {
        //�l�b�g���[�N�I�u�W�F�N�g�폜
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        //�I���p�l����\��
        uiManager.OpenEndPanel();

        //�X�R�A�p�l����\��
        ShowScoreboard();

        //�J�[�\���̕\��
        Cursor.lockState = CursorLockMode.None;

        //�I����̏���
        Invoke("ProcessingAfterCompletion", waitAfterEnding);
    }
    //�I����̏���
    private void ProcessingAfterCompletion()
    {
        //�V�[���̓����ݒ����
        PhotonNetwork.AutomaticallySyncScene = false;

        //���[���𔲂���
        PhotonNetwork.LeaveRoom();
    }
    //���[�����������ɌĂ΂��
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }
    
    public void SetPlayerName()
    {
        //Actor��1��Player��Player1, 2��Player��Player2�Ƃ���.
        foreach (PlayerInfo player in playerList)
        {
            if (player.actor == 1)
            {
                uiManager.SetPlayer1Text(player.name);
            }
            else if (player.actor == 2)
            {
                uiManager.SetPlayer2Text(player.name);
            }
        }
    }
}





    [System.Serializable]
public class PlayerInfo//�v���C���[�����Ǘ�����N���X
{
    public string name;//���O
    public int actor, kills, deaths;//�ԍ��A�L���A�f�X
    public PlayerController player;

    //�����i�[
    public PlayerInfo(string _name, int _actor, int _kills, int _death)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _death;
    }
}