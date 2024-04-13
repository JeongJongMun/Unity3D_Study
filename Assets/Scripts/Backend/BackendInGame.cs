using BackEnd;
using UnityEngine;

/* [ BackendInGame.cs ]
 * �ΰ��ӿ� �ʿ��� ������
 * �ΰ��Ӽ��� ���ӷ� �����ϱ� (�ΰ��� ���� ������ BackendMatch.cs�� ����)
 * �ΰ��� ���� ���� ����
 * ���� ����
 */
public partial class BackendMatchManager : MonoBehaviour
{
    // ���� �α�
    private string FAIL_ACCESS_INGAME = "�ΰ��� ���� ���� : {0} - {1}";
    private string SUCCESS_ACCESS_INGAME = "���� �ΰ��� ���� ���� : {0}";
    private string NUM_INGAME_SESSION = "�ΰ��� �� ���� ���� : {0}";

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
}
