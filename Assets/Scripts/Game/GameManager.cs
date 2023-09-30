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


//リアルタイムAPIのイベントコールバック。サーバーからのイベントと、OpRaiseEventを介してクライアントから送信されたイベントをカバー
//ゲームのシーケンス進行を担当
public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public List<PlayerInfo> playerList = new List<PlayerInfo>();//プレイヤー情報を扱うクラスのリスト
    
    public enum EventCodes : byte//自作イベント：byteは扱うデータ(0 ～ 255)
    {
        NewPlayer,//新しいプレイヤー情報をマスターに送る
        ListPlayers,//全員にプレイヤー情報を共有
        UpdateStat,//キルデス数の更新
        ShareNum,//MasterのRoundNumやDicisionNumを共有
    }

    /// <summary>
    /// ゲームの状態
    /// </summary>
    public enum GameState
    {
        Waiting,
        Playing,
        RoundEnding,
        Ending
    }
    public GameState state;//状態を格納

    private List<PlayerInformation> playerInfoList = new List<PlayerInformation>();

    //クリアパネルを表示している時間
    public float waitAfterEnding = 5.0f;
    
    //現在のラウンド数
    private int roundNumber;
    //ラウンドスタート待ちの秒数
    [SerializeField]
    private float startWaitSeconds = 3.0f;
    //ラウンド終了のときの秒数
    [SerializeField]
    private float endWaitSeconds = 3.0f;
    //WaitForSecondsキャッシュ用変数
    private WaitForSeconds startWait;
    private WaitForSeconds endWait;

    private PlayerInfo gameWinner;
    private PlayerInfo roundWinner;
    
    //playerPrefab
    private GameObject playerObj;
    private PlayerController player;
    
    //勝敗分け用乱数 (0.5未満ならMasterが最初(roundNumberが奇数)逃げれば勝ち, 0.5以上なら負けスタート)
    private float decisionNum = 0.0f;

    //スポーン管理用Manager
    [SerializeField] 
    private SpawnManager spawnManager = null;
    //ui管理用Manager
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
        //ネットワーク接続されていない場合
        if (!PhotonNetwork.IsConnected)
        {
            //タイトルに戻る
            SceneManager.LoadScene(0);
        }
        else //繋がっている場合
        {
            //スポーン
            Assert.IsNotNull(spawnManager);
            Assert.IsNotNull(timer);
            Assert.IsNotNull(uiManager);
            //マスターにユーザー情報を発信する
            NewPlayerGet(PhotonNetwork.NickName);
            //WaitSecondsはキャッシュして使う
            startWait = new WaitForSeconds(startWaitSeconds);
            endWait = new WaitForSeconds(endWaitSeconds);
            //状態を待機中に設定する
            state = GameState.Waiting;
            //タイマー初期化
            timer.Initilize(uiManager.TimerText);
            //スポーン
            spawnManager.Initilize();
            playerObj = spawnManager.SpawnPlayer();
            player = playerObj.GetComponent<PlayerController>();
            spawnManager.Relocate(PhotonNetwork.LocalPlayer.ActorNumber, playerObj);
            //変数初期化
            roundNumber = 1;
            decisionNum = UnityEngine.Random.value;
            Debug.Log(decisionNum);
        }
    }

    private void FixedUpdate()
    {
        Assert.IsNotNull(uiManager);
        //UIの名前セット
        SetPlayerName();
    }

    private void Update()
    {
        //Debug.Log(state);
        switch (state)
        {
            case GameState.Waiting: // ラウンド開始前
                if (timer == null) return;
                //roundNumber更新
                roundNumber = 1;
                for (int i = 0; i < playerList.Count; i++)
                {
                    roundNumber += playerList[i].kills;
                }
                //roundNumber表示
                uiManager.SetRoundNumber(roundNumber);

                //dicisionNum同期
                ShareNum(decisionNum);
                Debug.Log(decisionNum);
                //プレイヤーのラウンド目標文表示
                if (decisionNum <= 0.5f) //roundNumが奇数だったらMasterが勝利
                {
                    if (roundNumber % 2 == 1) //奇数の時
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
                    else //roundNumが偶数の時
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
                else//roundNumが偶数だったらMasterが勝利
                {
                    if (roundNumber % 2 == 1) //奇数の時
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
                    else //roundNumが偶数の時
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
                // ゲームプレイ中
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
                        // PlayerListのActornumber格納
                        for (int i = 0; i < playerList.Count; i++)
                        {   
                            idList.Add(playerList[i].actor);
                        }
                        // 対戦相手のActorId取得
                        int enemyActor = -1;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (idList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
                            {
                                enemyActor = idList[i];
                                Debug.Log(enemyActor);
                            }
                        }
                        //スコア処理
                        ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);//自分死亡時のイベント呼び出し
                        if (enemyActor != -1)
                        {
                            ScoreGet(enemyActor, 0, 1);   
                        }
                    }
                    if (timer.IsTimeUp)
                    {
                        state = GameState.RoundEnding;
                        var idList = new List<int>();
                        // PlayerListのActornumber格納
                        for (int i = 0; i < playerList.Count; i++)
                        {   
                            idList.Add(playerList[i].actor);
                        }
                        // 対戦相手のActorId取得
                        int enemyActorId = -1;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (idList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
                            {
                                enemyActorId = idList[i];
                                Debug.Log(enemyActorId);
                            }
                        }
                        //スコア処理(MasterClientにやらせる)
                        if (!PhotonNetwork.IsMasterClient) return;
                        if (decisionNum <= 0.5f)//roundNumが奇数だったらMasterが勝利
                        {
                            if (roundNumber % 2 == 1)//奇数の時
                            {
                                ScoreGet(enemyActorId, 1, 1); //相手死亡時のイベント呼び出し
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 0, 1); //自分勝利時
                            }
                            else
                            {
                                ScoreGet(enemyActorId, 0, 1); //相手勝利時のイベント呼び出し
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1); //自分敗北時
                            }
                        }
                        else//roundNumが偶数だったらMasterが勝利
                        {
                            if (roundNumber % 2 == 1)//奇数の時
                            {
                                ScoreGet(enemyActorId, 0, 1); //相手勝利時のイベント呼び出し
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1); //自分敗北時
                            }
                            else
                            {
                                ScoreGet(enemyActorId, 1, 1); //相手死亡時のイベント呼び出し
                                ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 0, 1); //自分勝利時
                            }
                        }
                    }
                }
                break;
            case GameState.RoundEnding:
                //タイマー初期化
                if (timer != null)
                {
                    timer.Initilize(uiManager.TimerText);   
                }
                //リスポーン
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
                //state変更
                state = GameState.Waiting;
                break;
        }
    }


    //イベント発生時に呼び出される
    public void OnEvent(EventData photonEvent)
    {

        if (photonEvent.Code < 200)//200以上はphotonが独自のイベントを使っているため200以下のみに処理をする
        {
            EventCodes eventCode = (EventCodes)photonEvent.Code;//今回のイベントコードを格納（型変換）
            object[] data = (object[])photonEvent.CustomData;//インデクサーとCustomDataKeyを介して、イベントのカスタムデータにアクセスします

            switch (eventCode)
            {
                case EventCodes.NewPlayer://発生したイベントがNewPlayerなら
                    NewPlayerSet(data);//マスターに新規ユーザー情報処理させる
                    break;

                case EventCodes.ListPlayers:
                    ListPlayersSet(data);//ユーザー情報を共有
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
    /// コンポーネントがオンになると呼ばれる
    /// </summary>
    public override void OnEnable()
    {
        //実装されているコールバック・インターフェースのコールバック用オブジェクトを登録します。
        PhotonNetwork.AddCallbackTarget(this);//追加する
    }
    
    /// <summary>
    /// コンポーネントがオフになると呼ばれる
    /// </summary>
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);//削除する
    }

    /// <summary>
    ///  新規ユーザーがネットワーク経由でマスターに自分の情報を送る
    /// </summary>
    public void NewPlayerGet(string name)//イベントを発生させる関数
    {
        object[] info = new object[4];//データ格納配列を作成
        info[0] = name;//名前
        info[1] = PhotonNetwork.LocalPlayer.ActorNumber;//ユーザー管理番号
        info[2] = 0;//キル
        info[3] = 0;//デス


        // RaiseEventでカスタムイベントを発生：データを送る
        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer,//発生させるイベント
            info,//送るもの（プレイヤーデータ）
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },//ルームマスターだけに送信される設定
            new SendOptions { Reliability = true }//信頼性の設定：信頼できるのでプレイヤーに送信される
        );

    }


    /// <summary>
    ///送られてきた新プレイヤーの情報をリストに格納する
    /// </summary>
    public void NewPlayerSet(object[] data)//マスターが行う処理　イベント発生時に行う処理
    {
        Debug.Log("NewPlayerSet");
        PlayerInfo player = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);//ネットワークからプレイヤー情報を取得

        playerList.Add(player);//リストに追加

        ListPlayersGet();//マスターが取得したプレイヤー情報を他のプレイヤーに共有
    }
    
    /// <summary>
    /// 取得したプレイヤー情報をルーム内の全プレイヤーに送信する
    /// </summary>
    public void ListPlayersGet()//マスターが行う処理　イベントを発生させる関数
    {
        object[] info = new object[playerList.Count + 1];//ゲームの状況＆プレイヤー情報格納配列作成

        info[0] = state;//最初にはゲームの状況を入れる

        for (int i = 0; i < playerList.Count; i++) //参加ユーザーの数分ループ
        {
            object[] temp = new object[4];//一時的格納する配列

            temp[0] = playerList[i].name;
            temp[1] = playerList[i].actor;
            temp[2] = playerList[i].kills;
            temp[3] = playerList[i].deaths;

            info[i + 1] = temp;//プレイヤー情報を格納している配列に格納する。0にはゲームの状態が入っているため＋１する。
        }


        //RaiseEventでカスタムイベントを発生：データを送る
        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers,////発生させるイベント
            info,//送るもの（プレイヤーデータ）
            new RaiseEventOptions { Receivers = ReceiverGroup.All },//全員に送信するイベント設定
            new SendOptions { Reliability = true }//信頼性の設定：信頼できるのでプレイヤーに送信される
        );
    }
    
    /// <summary>
    /// ListPlayersGetで新しくプレイヤー情報が送られてきたので、リストに格納する
    /// </summary>
    /// <param name="data">data = {0 : ゲーム状態, 1 : </param>
    public void ListPlayersSet(object[] data)//イベントが発生したら呼ばれる関数　全プレイヤーで呼ばれる
    {
        playerList.Clear();//既に持っているプレイヤーのリストを初期化

        state = (GameState)data[0];//ゲーム状態を変数に格納

        var alreadyAddedActor = new List<int>();

        for (int i = 1; i < data.Length; i++)//1にする 0はゲーム状態なので1から始める
        {
            object[] info = (object[])data[i];//

            PlayerInfo player = new PlayerInfo(
                (string)info[0],//名前
                (int)info[1],//管理番号
                (int)info[2],//キル
                (int)info[3]//デス
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
                playerList.Add(player);//リストに追加   
            }
        }
        //ゲームの状態判定
        StateCheck();
    }
    
    /// <summary>
    /// キル数やデス数を取得する関数(プレイヤー識別数、キルかデスを数値で判定、加算する数値)
    /// </summary>
    public void ScoreGet(int actor, int stat, int amount)
    {
        object[] package = new object[] { actor, stat, amount };

        //データを送るイベント
        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStat,//発生させるイベント
            package,//送るもの（プレイヤーのキルデスデータ）
            new RaiseEventOptions { Receivers = ReceiverGroup.All },//全員に送信するイベント設定
            new SendOptions { Reliability = true }//信頼性の設定：信頼できるのでプレイヤーに送信される
        );
    }

    /// <summary>
    /// マスターにdicisionNumを共有する関数
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
    /// dicisionNumを取得する関数
    /// </summary>
    public void NumSet(object[] data)
    {
        float sendDicisionNum = (float)data[0];
        decisionNum = sendDicisionNum;
    }

    /// <summary>
    /// 受け取ったデータからリストにキルデス情報を追加
    /// </summary>
    public void ScoreSet(object[] data)
    {
        int actor = (int)data[0];//識別数
        int stat = (int)data[1];//キルなのかデスなのか
        int amount = (int)data[2];//加算する数値


        for (int i = 0; i < playerList.Count; i++)//プレイヤーの数分ループ
        {
            if (playerList[i].actor == actor)//情報を取得したプレイヤーと数値が合致したとき
            {
                switch (stat)
                {
                    case 0://キル
                        playerList[i].kills += amount;

                        break;

                    case 1://デス
                        playerList[i].deaths += amount;

                        break;
                }
                break;//処理はできたのでこれ以降for文を回さないためにブレイクする
            }
        }

        //クリア条件満たしたか確認
        TargetScoreCheck();

    }


    //更新しつつスコアボードを開く
    public void ShowScoreboard()
    {
        //スコア開く
        uiManager.ChangeScoreUI();

        //現在表示されているスコアを削除
        foreach (PlayerInformation info in playerInfoList)
        {
            Destroy(info.gameObject);
        }

        playerInfoList.Clear();
        
        //参加ユーザー分ループ
        foreach(PlayerInfo player in playerList)
        {
            //スコア表示UI作成して格納
            PlayerInformation newPlayerDisplay = Instantiate(uiManager.info, uiManager.info.transform.parent);

            //UIにプレイヤ―情報を反映
            newPlayerDisplay.SetPlayerDetailes(player.name, player.kills, player.deaths);

            //表示
            newPlayerDisplay.gameObject.SetActive(true);

            //リストに格納
            playerInfoList.Add(newPlayerDisplay);
        }

        
    }

    //ゲームクリア条件を達成したか確認
    public void TargetScoreCheck()
    {
        bool clear = false;

        //クリア条件
        foreach (PlayerInfo player in playerList)
        {
            //キル数判定
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
            //UI変更
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
                //ゲームの状態を変更
                state = GameState.Ending;

                //ゲームプレイ状態を共有
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
            //ゲーム終了関数
            EndGame();
        }
    }
    //ゲーム終了関数
    public void EndGame()
    {
        //ネットワークオブジェクト削除
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        //終了パネルを表示
        uiManager.OpenEndPanel();

        //スコアパネルを表示
        ShowScoreboard();

        //カーソルの表示
        Cursor.lockState = CursorLockMode.None;

        //終了後の処理
        Invoke("ProcessingAfterCompletion", waitAfterEnding);
    }
    //終了後の処理
    private void ProcessingAfterCompletion()
    {
        //シーンの同期設定解除
        PhotonNetwork.AutomaticallySyncScene = false;

        //ルームを抜ける
        PhotonNetwork.LeaveRoom();
    }
    //ルーム抜けた時に呼ばれる
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }
    
    public void SetPlayerName()
    {
        //Actorが1のPlayerをPlayer1, 2のPlayerをPlayer2とする.
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
public class PlayerInfo//プレイヤー情報を管理するクラス
{
    public string name;//名前
    public int actor, kills, deaths;//番号、キル、デス
    public PlayerController player;

    //情報を格納
    public PlayerInfo(string _name, int _actor, int _kills, int _death)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _death;
    }
}