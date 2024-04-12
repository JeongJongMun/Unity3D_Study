using BackEnd;
using BackEnd.Tcp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackendMatch : MonoBehaviour
{
    /* 1. ��Ī ���� ����
     * 2. ���� ����
     * 3. ��Ī ��û
     * 4. ��Ī ����
     */
    void JoinMatchServer()
    {
        ErrorInfo errorInfo;
        bool isSuccess = Backend.Match.JoinMatchMakingServer(out errorInfo);
        //if (errorInfo != null)
        //{
        //    Debug.LogError("��Ī ���� ���� ����");
        //    return;
        //}
        if (isSuccess)
        {
            Debug.Log("��Ī ���� ���� ����");
        }
        else
        {
            Debug.LogError("��Ī ���� ���� ����");
        }
        Backend.Match.OnJoinMatchMakingServer = (JoinChannelEventArgs args) =>
        {
            // TODO
        };
    }

    void LeaveMatchserver()
    {
        Backend.Match.LeaveMatchMakingServer();
        Backend.Match.OnLeaveMatchMakingServer = (LeaveChannelEventArgs args) =>
        {
            Debug.Log("��Ī ���� ���� ����");
        };
    }
}
