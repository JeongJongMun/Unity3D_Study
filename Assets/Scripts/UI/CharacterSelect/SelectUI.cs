using TMPro;
using UnityEngine;

public class SelectUI : MonoBehaviour
{
    private static SelectUI instance;
    public GameObject popup_object, select_object, loading_object;
    public TMP_InputField input_nickname;
    public TMP_Text topbar_progress;

    void Awake()
    {
        instance = this;
    }

    public static SelectUI Instance()
    {
        if (instance == null)
        {
            Debug.LogError("SelectUI �ν��Ͻ��� �������� �ʽ��ϴ�.");
            return null;
        }
        return instance;
    }

    void Start()
    {
        popup_object.SetActive(false);
        select_object.SetActive(false);
        loading_object.SetActive(false);
        BackendMatchManager.Instance().GetMatchList();
    }
    public void SelectBoyCharacter()
    {
        PlayerInfo.Instance.PlayerType = PlayerType.Boy;
        popup_object.SetActive(true);
        select_object.SetActive(true);
        SetProgressText("�г��� ����");
        BackendMatchManager.Instance().JoinMatchMakingServer();
    }
    public void SelectGirlCharacter()
    {
        PlayerInfo.Instance.PlayerType = PlayerType.Girl;
        popup_object.SetActive(true);
        select_object.SetActive(true);
        SetProgressText("�г��� ����");
        BackendMatchManager.Instance().JoinMatchMakingServer();
    }
    public void SetProgressText(string text)
    {
        topbar_progress.text = text;
    }
    public bool SetNickname()
    {
        PlayerInfo.Instance.Nickname = input_nickname.text;
        return BackendServerManager.Instance().UpdateNickname(PlayerInfo.Instance.Nickname);
    }
    public void Play()
    {
        if (SetNickname())
        {
            select_object.SetActive(false);
            loading_object.SetActive(true);
            BackendMatchManager.Instance().CreateMatchRoom();
        }
    }
}