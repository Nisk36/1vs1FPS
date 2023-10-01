using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;//部屋の作成に必要


public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance;

    public GameObject loadingPanel;
    public Text loadingText;

    public GameObject buttons;

    public GameObject createRoomPanel;
    public Text createdRoomName;

    public GameObject roomPanel;
    public Text roomName;
    public GameObject startButton;

    public GameObject errorPanel;
    public Text errorText;

    public GameObject roomListPanel;

    public Room originalRoomButton;

    public GameObject roomButtonContent;

    Dictionary<string, RoomInfo> roomsDic = new Dictionary<string, RoomInfo>();
    private List<Room> allRoomButtons = new List<Room>();

    public Text playerNameText;
    private List<Text> allPlayerNames = new List<Text>();

    public GameObject playerNameContent;

    public GameObject nameInputPanel;
    public Text placeholderText;
    public InputField nameInput;
    private bool setName;


    public string playLevel;

    [SerializeField] public GameObject explainPanel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CloseMenuUI();

        loadingPanel.SetActive(true);
        loadingText.text = "サーバーに接続中...";

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void Update()
    {
        //Debug.Log(PhotonNetwork.IsConnected);
    }


    public void CloseMenuUI()
    {
        buttons.SetActive(false);

        loadingPanel.SetActive(false);

        createRoomPanel.SetActive(false);

        roomPanel.SetActive(false);

        errorPanel.SetActive(false);

        roomListPanel.SetActive(false);

        nameInputPanel.SetActive(false);
    }

    public void DisplayLobbyUI()
    {
        CloseMenuUI();
        buttons.SetActive(true);
    }

    //マスターサーバーに接続されたときに呼ばれる関数
    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターに接続");
        //ロビーに接続
        PhotonNetwork.JoinLobby();
        loadingText.text = "ロビーに接続中";

        //Master Clientと同じレベルをロード
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    //ロビー接続時に呼ばれる関数
    public override void OnJoinedLobby()
    {
        DisplayLobbyUI();
        //roomDic初期化
        roomsDic.Clear();

        PhotonNetwork.NickName = "Player" + Random.RandomRange(0, 1000).ToString();

        ConfirmationName();
    }

    public void DisplayCreateRoomUI()
    {
        CloseMenuUI();
        Debug.Log("CreateRoom");
        createRoomPanel.SetActive(true);
    }

    //ルーム作成ボタン用の関数
    public void CreateRoomButton()
    {
        if (!string.IsNullOrEmpty(createdRoomName.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;//1vs1の2人にする

            PhotonNetwork.CreateRoom(createdRoomName.text, options);
            Debug.Log(createdRoomName.text);

            CloseMenuUI();

            loadingPanel.SetActive(true);
            loadingText.text = "ルーム作成中...";
        }
    }

    //ルーム参加時に呼ばれる関数
    public override void OnJoinedRoom()
    {
        Debug.Log("joined room");

        CloseMenuUI();
        roomPanel.SetActive(true);
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        roomName.text = PhotonNetwork.CurrentRoom.Name;//参加しているルームの名前取得

        GetAllPlayers();

        //ルームマスターかどうか判定してボタン表示
        CheckRoomMaster();

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

        CloseMenuUI();
        loadingText.text = "ルーム退出中...";
        loadingPanel.SetActive(true);
    }

    //ルームから離れた時に呼ばれる関数
    public override void OnLeftRoom()
    {
        DisplayLobbyUI();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenuUI();
        errorText.text = "Failed to create room." + message;

        errorPanel.SetActive(true);
    }

    public void FindRoom()
    {
        CloseMenuUI();
        roomListPanel.SetActive(true);

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//ルームリストに更新があった時に呼ばれる関数
    {
        //ルームボタンUI初期化
        RoomUIInitialize();
        UpdateRoomDic(roomList);
    }

    public void UpdateRoomDic(List<RoomInfo> roomList)
    {
        for(int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            if (info.RemovedFromList)//満室とか消えたらTureが返ってくる
            {
                roomsDic.Remove(info.Name);
            }
            else
            {
                roomsDic[info.Name] = info;
            }
        }

        RoomsDicDisplay(roomsDic);
    }

    public void RoomsDicDisplay(Dictionary<string,RoomInfo> roomsDic)
    {
        foreach(var roomInfo in roomsDic)
        {
            //ボタン生成
            Room newButton = Instantiate(originalRoomButton);
            //生成したボタンにルーム情報を設定
            newButton.RegisterRoomDetails(roomInfo.Value);
            //親の設定
            newButton.transform.SetParent(roomButtonContent.transform);

            allRoomButtons.Add(newButton);
        }
    }

    public void RoomUIInitialize()
    {
        foreach(Room roomButton in allRoomButtons)
        {
            Destroy(roomButton.gameObject);
        }
        //リストの初期化
        allRoomButtons.Clear();
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);

        CloseMenuUI();
        loadingText.text = "ルーム参加中";
        loadingPanel.SetActive(true);
    }

    public void GetAllPlayers()
    {
        InitializePlayerList();

        DisplayPlayers();
    }

    public void InitializePlayerList()
    {
        foreach(var playerName in allPlayerNames)
        {
            Destroy(playerName.gameObject);
        }

        allPlayerNames.Clear();
    }

    public void DisplayPlayers()
    {
        //ルームに参加している人数分UI作成
        foreach(var players in PhotonNetwork.PlayerList)
        {
            //UI生成
            GeneratePlayerText(players);
        }
    }

    public void GeneratePlayerText(Player players)
    {
        Text newPlayerText = Instantiate(playerNameText);
        newPlayerText.text = players.NickName;
        newPlayerText.transform.SetParent(playerNameContent.transform);

        allPlayerNames.Add(newPlayerText);
    }


    public void ConfirmationName()
    {
        if (!setName)//起動時false
        {
            CloseMenuUI();
            nameInputPanel.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))//過去のプレイで保存済みだったら(playerNameはPlayerPrefsのKey)
            {
                placeholderText.text = PlayerPrefs.GetString("playerName");
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else//ルームに入って抜けた時とかに再度入力防ぐ
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    //名前登録
    public void SetName()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            //ユーザ名登録
            PhotonNetwork.NickName = nameInput.text;

            //保存
            PlayerPrefs.SetString("playerName", nameInput.text);

            DisplayLobbyUI();

            setName = true;
        }
    }

    //プレイヤーがルームに入った時(override)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GeneratePlayerText(newPlayer);
    }

    //プレイヤーがルームを離れるか、非アクティブになった時(overide)
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetAllPlayers();
    }

    public void CheckRoomMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    //マスターが切り替わった時(override)
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    //遷移関数
    public void PlayGame()
    {
        //PhotonNetwork.LoadLevel(playLevel);
        //Playerが2人いないと入れないように
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            PhotonNetwork.LoadLevel(playLevel);
        }
    }

    //操作説明
    public void ShowExplainPanel()
    {
        explainPanel.SetActive(true);
    }

    public void CloseExplainPanel()
    {
        explainPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
        Application.Quit();//ゲームプレイ終了
#endif

    }
}