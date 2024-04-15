using BackEnd;
using BackEnd.Tcp;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

/* [ BackendInGame.cs ]
 * �ΰ��ӿ� �ʿ��� ������
 * �ΰ��Ӽ��� ���ӷ� �����ϱ� (�ΰ��� ���� ������ BackendMatch.cs�� ����)
 * �ΰ��� ���� ���� ����
 * ���� ����
 */
public partial class BackendMatchManager : MonoBehaviour
{
    private bool isSetHost = false;                 // ȣ��Ʈ ���� �����ߴ��� ����
    // public Dictionary<SessionId, PlayerType> playerTypeList; // ���Ǻ� �÷��̾� Ÿ��

    // ���� �α�
    private string FAIL_ACCESS_INGAME = "�ΰ��� ���� ���� : {0} - {1}";
    private string SUCCESS_ACCESS_INGAME = "���� �ΰ��� ���� ���� : {0}";
    private string NUM_INGAME_SESSION = "�ΰ��� �� ���� ���� : {0}";
    // ���� ���� ������ �� ȣ���
    public void OnGameReady()
    {
        if (isSetHost == false)
        {
            // ȣ��Ʈ�� �������� ���� �����̸� ȣ��Ʈ ����
            isSetHost = SetHostSession();
        }
        Debug.Log("ȣ��Ʈ ���� �Ϸ�");
    }
    // �������� ���� ���� ��Ŷ�� ������ �� ȣ��
    // ��� ������ ���� �뿡 ���� �� "�ֿܼ��� ������ �ð�" �Ŀ� ���� ���� ��Ŷ�� �������� �´�
    private void GameSetup()
    {
        Debug.Log("���� ���� �޽��� ����. ���� ���� ����");
        // ���� ���� �޽����� ���� ������ ���� ���·� ����
        isHost = false;
        isSetHost = false;
        OnGameReady();
    }

    // ���� �뿡 ������ ���ǵ��� ����
    // Ŭ���̾�Ʈ�� ���ӹ� ���ӿ� �������� �� ������ Ŭ���̾�Ʈ���Ը� ȣ�� (������ O)
    private void ProcessMatchInGameSessionList(MatchInGameSessionListEventArgs args)
    {
        sessionIdList = new List<SessionId>();
        gameRecords = new Dictionary<SessionId, MatchUserGameRecord>();

        foreach (var record in args.GameRecords)
        {
            Debug.LogFormat("���� �ΰ��� ���� ���� [{0}] : {1}", record.m_sessionId, record.m_nickname);
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);
        }
        sessionIdList.Sort();
    }

    // Ŭ���̾�Ʈ�� ���ӹ� ���ӿ� �������� �� ��� �������� ȣ�� (������ �� X)
    private void ProcessMatchInGameAccess(MatchInGameSessionEventArgs args)
    {
        if (isReconnectProcess)
        {
            // ������ ���μ��� �� ���
            // �� �޽����� ���ŵ��� �ʰ�, ���� ���ŵǾ ������
            Debug.Log("������ ���μ��� ������... ������ ���μ��������� ProcessMatchInGameAccess �޽����� ���ŵ��� �ʽ��ϴ�.\n" + args.ErrInfo);
            return;
        }

        Debug.Log(string.Format(SUCCESS_ACCESS_INGAME, args.ErrInfo));

        if (args.ErrInfo != ErrorCode.Success)
        {
            // ���� �� ���� ����
            var errorLog = string.Format(FAIL_ACCESS_INGAME, args.ErrInfo, args.Reason);
            Debug.Log(errorLog);
            LeaveInGameServer();
            return;
        }

        // ���� �� ���� ����
        // ���ڰ��� ��� ������ Ŭ���̾�Ʈ(����)�� ����ID�� ��Ī ����� ����ִ�.
        // ���� ������ �����Ǿ� ����ֱ� ������ �̹� ������ �����̸� �ǳʶڴ�.

        var record = args.GameRecord;
        Debug.Log(string.Format(string.Format("���ο� �ΰ��� ���� ���� [{0}] : {1}", args.GameRecord.m_sessionId, args.GameRecord.m_nickname)));
        if (!sessionIdList.Contains(args.GameRecord.m_sessionId))
        {
            // ���� ����, ���� ��� ���� ����
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);

            Debug.Log(string.Format(NUM_INGAME_SESSION, sessionIdList.Count));
        }               
    }

    // �ΰ��� �� ����
    private void AccessInGameRoom(string roomToken)
    {
        SelectUI.Instance().SetProgressText("���� �� ���� ��");

        Backend.Match.JoinGameRoom(roomToken);
    }
    // �ΰ��� ���� ���� ����
    public void LeaveInGameServer()
    {
        isConnectInGameServer = false;
        Backend.Match.LeaveGameServer();
    }

    // ȣ��Ʈ���� ���� ���Ǹ���Ʈ�� ����
    public void SetPlayerSessionList(List<SessionId> sessions)
    {
        sessionIdList = sessions;
    }
    // ������ ������ ��Ŷ ����
    // ���������� �� ��Ŷ�� �޾� ��� Ŭ���̾�Ʈ(��Ŷ ���� Ŭ���̾�Ʈ ����)�� ��ε�ĳ���� ���ش�.
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Backend.Match.SendDataToInGameRoom(byteArray);
    }
        private void ProcessSessionOnline(SessionId sessionId, string nickName)
    {
        // ȣ��Ʈ�� �ƴϸ� �ƹ� �۾� ���� (ȣ��Ʈ�� ����)
        if (isHost)
        {
            // ������ �� Ŭ���̾�Ʈ�� �ΰ��� ���� �����ϱ� �� ���� �������� ���� �� nullptr ���ܰ� �߻��ϹǷ� ����
            // 2������ ��ٸ� �� ���� ���� �޽����� ����
            Invoke("SendGameSyncMessage", 2.0f);
        }
    }

    // Invoke�� �����
    private void SendGameSyncMessage()
    {
        // ���� ���� ��Ȳ (��ġ, hp ���...)
        var message = WorldManager.instance.GetNowGameState(hostSession);
        SendDataToInGame(message);
    }
}