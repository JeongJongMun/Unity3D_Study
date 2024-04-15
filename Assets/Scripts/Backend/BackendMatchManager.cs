using BackEnd;
using BackEnd.Tcp;
using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.SceneManagement;
using Protocol;


/* [ partial class ��ġ �Ŵ��� ]
 * - BackendMatchManager.cs
 * - BackendMatch.cs
 * - BackendInGame.cs
 * 
 * [ BackendMatchManager.cs ]
 * ��ġ�Ŵ����� �ʿ��� ���� ����
 * ��ġ����ŷ �ڵ鷯 ����
 * �ΰ��� �ڵ鷯 ����
 * ��Ī ���� ����� BackendMatch.cs�� ����
 * �ΰ��� ���� ����� BackendInGame.cs�� ����
 */
public partial class BackendMatchManager : MonoBehaviour
{
    public class ServerInfo
    {
        public string host;
        public ushort port;
        public string roomToken;
    }
    // �ֿܼ��� ������ ��Ī ī�� ����
    public class MatchInfo
    {
        public string title;                // ��Ī ��
        public string inDate;               // ��Ī inDate (UUID)
        public MatchType matchType;         // ��ġ Ÿ��
        public MatchModeType matchModeType; // ��ġ ��� Ÿ��
        public string headCount;            // ��Ī �ο�
        public bool isSandBoxEnable;        // ����ڽ� ��� (AI��Ī)
    }
    public List<MatchInfo> matchInfos { get; private set; } = new List<MatchInfo>();  // �ֿܼ��� ������ ��Ī ī����� ����Ʈ
    public List<SessionId> sessionIdList { get; private set; }  // ��ġ�� �������� �������� ���� ���
    public Dictionary<SessionId, MatchUserGameRecord> gameRecords { get; private set; } = null;  // ��ġ�� �������� �������� ��Ī ���
    private static BackendMatchManager instance = null;
    private string inGameRoomToken = string.Empty;  // ���� �� ��ū (�ΰ��� ���� ��ū)
    public SessionId hostSession { get; private set; }  // ȣ��Ʈ ����
    private ServerInfo roomInfo = null;             // ���� �� ����
    [field: SerializeField]
    public bool isConnectMatchServer { get; private set; } = false;
    [SerializeField]
    private bool isConnectInGameServer = false;
    [SerializeField]
    private bool isJoinGameRoom = false;
    public bool isReconnectProcess { get; private set; } = false;
    public bool isSandBoxGame { get; private set; } = false;

    #region Host
    [SerializeField]
    private bool isHost = false;                    // ȣ��Ʈ ���� (�������� ������ SuperGamer ������ ������)
    private Queue<KeyMessage> localQueue = null;    // ȣ��Ʈ���� ���÷� ó���ϴ� ��Ŷ�� �׾Ƶδ� ť (����ó���ϴ� �����ʹ� ������ �߼� ����)
    #endregion

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
    }

    public static BackendMatchManager Instance()
    {
        if (instance == null)
        {
            return null;
        }
        return instance;
    }

    private void OnApplicationQuit()
    {
        if (isConnectMatchServer)
        {
            LeaveMatchMakingServer();
            Debug.Log("ApplicationQuit - LeaveMatchServer");
        }
    }

    private void Update()
    {
        if (isConnectInGameServer || isConnectMatchServer)
        {
            Backend.Match.Poll();

            // ȣ��Ʈ�� ��� ���� ť�� ����
            // ť�� �ִ� ��Ŷ�� ���ÿ��� ó��
            if (localQueue != null)
            {
                while (localQueue.Count > 0)
                {
                    var msg = localQueue.Dequeue();
                    WorldManager.instance.OnRecieveForLocal(msg);
                }
            }
        }
    }
    private void Start()
    {
        MatchMakingHandler();
        GameHandler();
        ExceptionHandler();
    }
    public bool IsHost()
    {
        return isHost;
    }
    public bool IsMySessionId(SessionId session)
    {
        return Backend.Match.GetMySessionId() == session;
    }
    public string GetNickNameBySessionId(SessionId session)
    {
        //return Backend.Match.GetNickNameBySessionId(session);
        return gameRecords[session].m_nickname;
    }

    public bool IsSessionListNull()
    {
        return sessionIdList == null || sessionIdList.Count == 0;
    }
    private bool SetHostSession()
    {
        // ȣ��Ʈ ���� ���ϱ�
        // �� Ŭ���̾�Ʈ�� ��� ���� (ȣ��Ʈ ���� ���ϴ� ������ ��� �����Ƿ� ������ Ŭ���̾�Ʈ�� ��� ������ ���������� ������� ����.)

        Debug.Log("ȣ��Ʈ ���� ���� ����");
        // ȣ��Ʈ ���� ���� (�� Ŭ���̾�Ʈ���� ���� ������ �ٸ� �� �ֱ� ������ ����)
        sessionIdList.Sort();
        isHost = false;
        // ���� ȣ��Ʈ ��������
        foreach (var record in gameRecords)
        {
            if (record.Value.m_isSuperGamer == true)
            {
                if (record.Value.m_sessionId.Equals(Backend.Match.GetMySessionId()))
                {
                    isHost = true;
                }
                hostSession = record.Value.m_sessionId;
                break;
            }
        }

        Debug.Log("ȣ��Ʈ ���� : " + isHost);

        // ȣ��Ʈ �����̸� ���ÿ��� ó���ϴ� ��Ŷ�� �����Ƿ� ���� ť�� �������ش�
        if (isHost)
        {
            localQueue = new Queue<KeyMessage>();
        }
        else
        {
            localQueue = null;
        }

        // ȣ��Ʈ �������� ������ ��ġ������ ���� ����
        LeaveMatchMakingServer();
        return true;
    }
    // ��Ī ���� ���� �̺�Ʈ �ڵ鷯
    private void MatchMakingHandler()
    {
        // ��Ī ���� ���� �̺�Ʈ
        Backend.Match.OnJoinMatchMakingServer += (JoinChannelEventArgs args) =>
        {
            Debug.Log("OnJoinMatchMakingServer : " + args.ErrInfo);
            ProcessAccessMatchMakingServer(args.ErrInfo);
        };

        // ��Ī ���� ���� ���� �̺�Ʈ
        Backend.Match.OnLeaveMatchMakingServer += (args) =>
        {
            Debug.Log("OnLeaveMatchMakingServer : " + args.ErrInfo);
            isConnectMatchServer = false;
        };

        // ���� ���� �̺�Ʈ
        Backend.Match.OnMatchMakingRoomCreate += (MatchMakingInteractionEventArgs args) =>
        {
            Debug.Log("OnMatchMakingRoomCreate : " + args.ErrInfo + " : " + args.Reason);
            if (args.ErrInfo == ErrorCode.Success)
            {
                RequestMatchMaking(0);
            }
            else
            {
                Backend.Match.CreateMatchRoom();
            }
        };

        // ��Ī ��û/����/���� �̺�Ʈ
        Backend.Match.OnMatchMakingResponse += (args) =>
        {
            Debug.Log("OnMatchMakingResponse : " + args.ErrInfo + " : " + args.Reason);
            ProcessMatchMakingResponse(args);
        };
        // ���濡 ���� ���� �޽���
        Backend.Match.OnMatchMakingRoomJoin += (args) =>
        {
            Debug.Log(string.Format("OnMatchMakingRoomJoin : {0} : {1}", args.ErrInfo, args.Reason));
        };
    }

    // �ΰ��� ���� ���� �̺�Ʈ �ڵ鷯
    private void GameHandler()
    {
        // �ΰ��� �������� �̺�Ʈ
        Backend.Match.OnSessionJoinInServer += (args) =>
        {
            Debug.Log("OnSessionJoinInServer : " + args.ErrInfo);
            if (args.ErrInfo != ErrorInfo.Success)
            {
                if (isReconnectProcess)
                {
                    if (args.ErrInfo.Reason.Equals("Reconnect Success"))
                    {
                        //������ ����
                        Debug.Log("������ ����");
                    }
                    else if (args.ErrInfo.Reason.Equals("Fail To Reconnect"))
                    {
                        Debug.Log("������ ����");
                        JoinMatchMakingServer();
                        isConnectInGameServer = false;
                    }
                }
                return;
            }
            if (isJoinGameRoom)
            {
                return;
            }
            if (inGameRoomToken == string.Empty)
            {
                Debug.LogError("�ΰ��� ���� ���� ���������� �� ��ū�� �����ϴ�.");
                return;
            }
            Debug.Log("�ΰ��� ���� ���� ����");
            isJoinGameRoom = true;
            AccessInGameRoom(inGameRoomToken);
        };

        // Ŭ���̾�Ʈ�� ���ӹ� ���ӿ� �������� �� ������ Ŭ���̾�Ʈ���Ը� ȣ��
        Backend.Match.OnSessionListInServer += (args) =>
        {
            // ���� ����Ʈ ȣ�� �� ���� ä���� ȣ���
            // ���� ���� ����(��)�� �������� �÷��̾�� �� ������ ���� �� �濡 ���� �ִ� �÷��̾��� ���� ������ ����ִ�.
            // ������ �ʰ� ���� �÷��̾���� ������ OnMatchInGameAccess ���� ���ŵ�
            Debug.Log("OnSessionListInServer : " + args.ErrInfo);
            ProcessMatchInGameSessionList(args);
            SceneManager.LoadScene("2. InGameJJM");
        };

        // Ŭ���̾�Ʈ�� ���ӹ� ���ӿ� �������� �� ��� �������� ȣ��
        Backend.Match.OnMatchInGameAccess += (args) =>
        {
            Debug.Log("OnMatchInGameAccess : " + args.ErrInfo);
            // ����(Ŭ���̾�Ʈ)�� �ΰ��� �뿡 ������ ������ ȣ��
            ProcessMatchInGameAccess(args);
        };

        // �������� ���� ���� ��Ŷ�� ������ ȣ��
        Backend.Match.OnMatchInGameStart += () =>
        {
            GameSetup();
        };
        
        // ���� ��� �̺�Ʈ
        Backend.Match.OnMatchResult += (args) =>
        {
            Debug.Log("���� ����� ���ε� ��� : " + string.Format("{0} : {1}", args.ErrInfo, args.Reason));
            // �������� ���� ��� ��Ŷ�� ������ ȣ��
            // ����(Ŭ���̾�Ʈ��) ������ ���� ������� ���������� ������Ʈ �Ǿ����� Ȯ��
        };

        Backend.Match.OnMatchRelay += (args) =>
        {
            // �� Ŭ���̾�Ʈ���� ������ ���� �ְ���� ��Ŷ��
            // ������ �ܼ� ��ε�ĳ���ø� ���� (�������� ��� ���굵 �������� ����)
            if (WorldManager.instance == null)
            {
                // ���� �Ŵ����� �������� ������ �ٷ� ����
                return;
            }

            WorldManager.instance.OnRecieve(args);
        };

        Backend.Match.OnMatchChat += (args) =>
        {
            // ä�ñ���� Ʃ�丮�� �������� �ʾҽ��ϴ�.
        };

        // �ΰ��� ���� ���� ���� �̺�Ʈ
        Backend.Match.OnLeaveInGameServer += (args) =>
        {
            Debug.Log("OnLeaveInGameServer : " + args.ErrInfo + " : " + args.Reason);
            if (args.Reason.Equals("Fail To Reconnect"))
            {
                JoinMatchMakingServer();
            }
            isConnectInGameServer = false;
        };

        // �ٸ� ������ ������ ���� �� ȣ��
        Backend.Match.OnSessionOnline += (args) =>
        {
            var nickName = Backend.Match.GetNickNameBySessionId(args.GameRecord.m_sessionId);
            Debug.Log(string.Format("[{0}] �¶��εǾ����ϴ�. - {1} : {2}", nickName, args.ErrInfo, args.Reason));
        };

        // �ٸ� ���� Ȥ�� �ڱ��ڽ��� ������ �������� �� ��� Ŭ���̾�Ʈ���� ȣ��
        Backend.Match.OnSessionOffline += (args) =>
        {
            Debug.Log(string.Format("[{0}] �������εǾ����ϴ�. - {1} : {2}", args.GameRecord.m_nickname, args.ErrInfo, args.Reason));
        };

        // ���۰��̸� ������ ����Ǿ��� �� ȣ��
        Backend.Match.OnChangeSuperGamer += (args) =>
        {
            Debug.Log(string.Format("���� ���� : {0} / �� ���� : {1}", args.OldSuperUserRecord.m_nickname, args.NewSuperUserRecord.m_nickname));
        };
    }
    private void ExceptionHandler()
    {
        // ���ܰ� �߻����� �� ȣ��
        Backend.Match.OnException += (e) =>
        {
            Debug.Log(e);
        };
    }
    public void GetMatchList()
    {
        // ��Ī ī�� ���� �ʱ�ȭ
        matchInfos.Clear();
        var callback = Backend.Match.GetMatchList();
        if (callback.IsSuccess() == false)
        {
            Debug.Log("��Īī�� ����Ʈ �ҷ����� ����\n" + callback);
            return;
        }

        foreach (JsonData row in callback.Rows())
        {
            MatchInfo matchInfo = new MatchInfo();
            matchInfo.title = row["matchTitle"]["S"].ToString();
            matchInfo.inDate = row["inDate"]["S"].ToString();
            matchInfo.headCount = row["matchHeadCount"]["N"].ToString();
            matchInfo.isSandBoxEnable = row["enable_sandbox"]["BOOL"].ToString().Equals("True") ? true : false;

            foreach (MatchType type in Enum.GetValues(typeof(MatchType)))
            {
                if (type.ToString().ToLower().Equals(row["matchType"]["S"].ToString().ToLower()))
                {
                    matchInfo.matchType = type;
                }
            }

            foreach (MatchModeType type in Enum.GetValues(typeof(MatchModeType)))
            {
                if (type.ToString().ToLower().Equals(row["matchModeType"]["S"].ToString().ToLower()))
                {
                    matchInfo.matchModeType = type;
                }
            }

            matchInfos.Add(matchInfo);
        }
        Debug.Log("��Īī�� ����Ʈ �ҷ����� ���� : " + matchInfos.Count);
    }
    public void SetHostSession(SessionId host)
    {
        hostSession = host;
    }
    public void AddMsgToLocalQueue(KeyMessage message)
    {
        // ���� ť�� �޽��� �߰�
        if (isHost == false || localQueue == null)
        {
            return;
        }

        localQueue.Enqueue(message);
    }
}
