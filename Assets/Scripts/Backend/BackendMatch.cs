using BackEnd;
using BackEnd.Tcp;
using UnityEngine;

/* [ BackendMatch.cs ]
 * ��Ī ���� ��� ����
 * ��Ī ���� ����
 * ��Ī ���� ������
 * ��Ī ��û�ϱ�
 * ��Ī ��û ����ϱ�
 */
public partial class BackendMatchManager : MonoBehaviour
{
    // ����� �α�
    private string NOTCONNECT_MATCHSERVER = "��ġ ������ ����Ǿ� ���� �ʽ��ϴ�.";
    private string RECONNECT_MATCHSERVER = "��ġ ������ ������ �õ��մϴ�.";
    private string FAIL_CONNECT_MATCHSERVER = "��ġ ���� ���� ���� : {0}";
    private string SUCCESS_CONNECT_MATCHSERVER = "��ġ ���� ���� ����";
    private string SUCCESS_MATCHMAKE = "��Ī ���� : {0}";
    private string SUCCESS_REGIST_MATCHMAKE = "��Ī ��⿭�� ���";
    private string FAIL_REGIST_MATCHMAKE = "��Ī ���� : {0}";
    private string CANCEL_MATCHMAKE = "��Ī ��û ��� : {0}";
    private string INVAILD_MATCHTYPE = "�߸��� ��ġ Ÿ���Դϴ�.";
    private string INVALID_MODETYPE = "�߸��� ��� Ÿ���Դϴ�.";
    private string INVALID_OPERATION = "�߸��� ��û�Դϴ�\n{0}";
    private string EXCEPTION_OCCUR = "���� �߻� : {0}\n�ٽ� ��Ī�� �õ��մϴ�.";


    
    // ��Ī ���� ���� ��û
    public void JoinMatchMakingServer()
    {
        if (isConnectMatchServer)
        {
            return;
        }

        ErrorInfo errorInfo;
        isConnectMatchServer = true;
        if (!Backend.Match.JoinMatchMakingServer(out errorInfo))
        {
            Debug.LogError("��Ī ���� ���� ����");
        }
    }

    // ��Ī ���� ���� ����
    public void LeaveMatchMakingServer()
    {
        Debug.Log("��ġ ���� ���� ����");
        isConnectMatchServer = false;
        Backend.Match.LeaveMatchMakingServer();
    }

    // ��Ī ���� ���� �� ����
    public bool CreateMatchRoom()
    {
        SelectUI.Instance().SetProgressText("��Ī ���� ���� ��");

        if (!isConnectMatchServer)
        {
            Debug.LogError(NOTCONNECT_MATCHSERVER);
            Debug.Log(RECONNECT_MATCHSERVER);
            JoinMatchMakingServer();
            return false;
        }
        Backend.Match.CreateMatchRoom();
        return true;
    }

    // ��Ī ��� �� ������
    public void LeaveMatchRoom()
    {
        Debug.Log("��Ī ���� ������");
        Backend.Match.LeaveMatchRoom();
    }
    // ��Ī ��û�ϱ�
    public void RequestMatchMaking(int index)
    {
        SelectUI.Instance().SetProgressText("��Ī ��û��");

        // ��û ������ ����Ǿ� ���� ������ ��Ī ���� ����
        if (!isConnectMatchServer)
        {
            Debug.Log(NOTCONNECT_MATCHSERVER);
            Debug.Log(RECONNECT_MATCHSERVER);
            JoinMatchMakingServer();
            return;
        }
        // ���� �ʱ�ȭ
        isConnectInGameServer = false;

        Backend.Match.RequestMatchMaking(matchInfos[index].matchType, matchInfos[index].matchModeType, matchInfos[index].inDate);
        if (isConnectInGameServer)
        {
            Backend.Match.LeaveGameServer(); //�ΰ��� ���� ���ӵǾ� ���� ��츦 ����� �ΰ��� ���� ���� ȣ��
        }

        //nowMatchType = matchInfos[index].matchType;
        //nowModeType = matchInfos[index].matchModeType;
        //numOfClient = int.Parse(matchInfos[index].headCount);
    }
    // ��Ī ���� ����
    public void CancelMatchMaking()
    {
        Backend.Match.CancelMatchMaking();
    }

    // ��Ī ���� ���ӿ� ���� ���ϰ�
    private void ProcessAccessMatchMakingServer(ErrorInfo errInfo)
    {
        if (errInfo != ErrorInfo.Success)
        {
            // ���� ����
            isConnectMatchServer = false;
        }

        if (!isConnectMatchServer)
        {
            var errorLog = string.Format(FAIL_CONNECT_MATCHSERVER, errInfo.ToString());
            // ���� ����
            Debug.Log(errorLog);
        }
        else
        {
            //���� ����
            Debug.Log(SUCCESS_CONNECT_MATCHSERVER);
        }
    }

    /*
     * ��Ī ��û�� ���� ���ϰ� (ȣ��Ǵ� ����)
     * ��Ī ��û �������� ��
     * ��Ī �������� ��
     * ��Ī ��û �������� ��
     */
    private void ProcessMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        string debugLog = string.Empty;
        bool isError = false;
        switch (args.ErrInfo)
        {
            case ErrorCode.Success:
                // ��Ī �������� ��
                SelectUI.Instance().SetProgressText("�ΰ��� ���� ���� ��");
                debugLog = string.Format(SUCCESS_MATCHMAKE, args.Reason);
                ProcessMatchSuccess(args);
                break;

            case ErrorCode.Match_InProgress:
                // ��Ī ��û �������� �� or ��Ī ���� �� ��Ī ��û�� �õ����� ��

                // ��Ī ��û �������� ��
                if (args.Reason == string.Empty)
                {
                    SelectUI.Instance().SetProgressText(SUCCESS_REGIST_MATCHMAKE);
                    debugLog = SUCCESS_REGIST_MATCHMAKE;
                }
                break;

            case ErrorCode.Match_MatchMakingCanceled:
                // ��Ī ��û�� ��ҵǾ��� ��
                debugLog = string.Format(CANCEL_MATCHMAKE, args.Reason);
                break;

            case ErrorCode.Match_InvalidMatchType:
                isError = true;
                // ��ġ Ÿ���� �߸� �������� ��
                debugLog = string.Format(FAIL_REGIST_MATCHMAKE, INVAILD_MATCHTYPE);
                break;

            case ErrorCode.Match_InvalidModeType:
                isError = true;
                // ��ġ ��带 �߸� �������� ��
                debugLog = string.Format(FAIL_REGIST_MATCHMAKE, INVALID_MODETYPE);
                break;

            case ErrorCode.InvalidOperation:
                isError = true;
                // �߸��� ��û�� �������� ��
                debugLog = string.Format(INVALID_OPERATION, args.Reason);
                break;

            case ErrorCode.Match_Making_InvalidRoom:
                isError = true;
                // �߸��� ��û�� �������� ��
                debugLog = string.Format(INVALID_OPERATION, args.Reason);
                break;

            case ErrorCode.Exception:
                isError = true;
                // ��Ī �ǰ�, �������� �� ������ �� ���� �߻� �� exception�� ���ϵ�
                // �� ��� �ٽ� ��Ī ��û�ؾ� ��
                debugLog = string.Format(EXCEPTION_OCCUR, args.Reason);
                break;
        }

        if (!debugLog.Equals(string.Empty))
        {
            Debug.Log(debugLog);
            if (isError == true)
            {
                //LobbyUI.GetInstance().SetErrorObject(debugLog);
            }
        }
    }

    // ��Ī ���� �� �ΰ��� ���� ����
    private void ProcessMatchSuccess(MatchMakingResponseEventArgs args)
    {

        ErrorInfo errorInfo;
        if (!Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
        {
            var debugLog = string.Format(FAIL_ACCESS_INGAME, errorInfo.ToString(), string.Empty);
            Debug.Log(debugLog);
        }
        // ���ڰ����� �ΰ��� ����ū�� �����صξ�� �Ѵ�.
        // �ΰ��� �������� �뿡 ������ �� �ʿ�
        // 1�� ���� ��� ������ �뿡 �������� ������ �ش� ���� �ı�ȴ�.
        isConnectInGameServer = true;
        isJoinGameRoom = false;
        isReconnectProcess = false;
        inGameRoomToken = args.RoomInfo.m_inGameRoomToken;
        isSandBoxGame = args.RoomInfo.m_enableSandbox;
    }
}
