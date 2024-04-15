using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private static bool isCreate = false;

    #region Actions-Events
    public static event Action InGame = delegate { };
    private IEnumerator InGameUpdateCoroutine;

    public enum GameState { Login, MatchLobby, Ready, Start, InGame, Over, Result, Reconnect };
    private GameState gameState;
    #endregion


    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("GameManager �ν��Ͻ��� �������� �ʽ��ϴ�.");
            return null;
        }
        return instance;
    }
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        // 60������ ����
        Application.targetFrameRate = 60;
        // ������ ������� ����
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        InGameUpdateCoroutine = InGameUpdate();

        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        if (isCreate)
        {
            DestroyImmediate(gameObject, true);
            return;
        }
        gameState = GameState.Login;
        isCreate = true;
    }
    IEnumerator InGameUpdate()
    {
        while (true)
        {
            if (gameState != GameState.InGame)
            {
                StopCoroutine(InGameUpdateCoroutine);
                yield return null;
            }
            InGame();
            yield return new WaitForSeconds(.1f); //1�� ����
        }
    }
    public void ChangeState(GameState state, Action<bool> func = null)
    {
        gameState = state;
        switch (gameState)
        {
            case GameState.InGame:
                // �ڷ�ƾ ����
                StartCoroutine(InGameUpdateCoroutine);
                break;
            default:
                Debug.Log("�˼����� ������Ʈ�Դϴ�. Ȯ�����ּ���.");
                break;
        }
    }
}
