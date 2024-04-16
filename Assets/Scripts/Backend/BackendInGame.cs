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

        if (IsHost() == true)
        {
            Debug.Log("3�� �� �ΰ��� �� ��ȯ �޽��� �۽�");
            Invoke("SendChangeGameScene", 3f);
        }
    }

    // �������� ���� ���� ��Ŷ�� ������ �� ȣ��
    // ��� ������ ���� �뿡 ���� �� "�ֿܼ��� ������ �ð�" �Ŀ� ���� ���� ��Ŷ�� �������� �´�
    private void GameSetup()
    {
        Debug.Log("���� ���� �޽��� ����. ���� ���� ����");
        // ���� ���� �޽����� ���� ������ ���� ���·� ����
        if (GameManager.Instance().GetGameState() != GameManager.GameState.Ready)
        {
            isHost = false;
            isSetHost = false;
            OnGameReady();
        }
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
        if (!sessionIdList.Contains(args.GameRecord.m_sessionId))
        {
            Debug.Log(string.Format(string.Format("���ο� �ΰ��� ���� ���� [{0}] : {1}", args.GameRecord.m_sessionId, args.GameRecord.m_nickname)));
            // ���� ����, ���� ��� ���� ����
            sessionIdList.Add(record.m_sessionId);
            gameRecords.Add(record.m_sessionId, record);

            Debug.Log(string.Format(NUM_INGAME_SESSION, sessionIdList.Count));
        }               
    }

    // �ΰ��� �� ����
    private void AccessInGameRoom(string roomToken)
    {
        LoginUI.Instance().SetProgressText("���� �� ���� ��");

        Backend.Match.JoinGameRoom(roomToken);
    }
    // �ΰ��� ���� ���� ����
    public void LeaveInGameServer()
    {
        isConnectInGameServer = false;
        Backend.Match.LeaveGameServer();
    }

    // ������ ������ ��Ŷ ����
    // ���������� �� ��Ŷ�� �޾� ��� Ŭ���̾�Ʈ(��Ŷ ���� Ŭ���̾�Ʈ ����)�� ��ε�ĳ���� ���ش�.
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Backend.Match.SendDataToInGameRoom(byteArray);
    }

    private void SendChangeGameScene()
    {
        Debug.Log("�ΰ��� �� ��ȯ �޽��� �۽�");
        SendDataToInGame(new Protocol.LoadGameSceneMessage());
    }
    public bool PrevGameMessage(byte[] BinaryUserData)
    {
        Protocol.Message msg = DataParser.ReadJsonData<Protocol.Message>(BinaryUserData);
        if (msg == null)
        {
            return false;
        }

        // ���� ���� ���� �۾� ��Ŷ �˻� 
        switch (msg.type)
        {
            case Protocol.Type.LoadGameScene:
                GameManager.Instance().ChangeState(GameManager.GameState.Start);
                return true;
        }
        return false;
    }
}