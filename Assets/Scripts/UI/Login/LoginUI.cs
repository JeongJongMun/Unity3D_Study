using TMPro;
using UnityEngine;

/* LoginUI.cs
 * �α��� UI ����
 * �α���, ȸ������ ��ư ������Ʈ
 * �α���, ȸ������ �Է� �ʵ�
 */
public class LoginUI : MonoBehaviour
{
    private static LoginUI instance;
    public GameObject input_id, input_pw, input_nickname;
    public GameObject btns_login, btns_signup, btn_play;
    public GameObject object_popup, object_progress, object_loading;
    private TMP_Text text_popup, text_progress;
    private TMP_InputField[] inputs = new TMP_InputField[3];
    private const int ID_INDEX = 0, PW_INDEX = 1, NICKNAME_INDEX = 2;
    void Awake()
    {
        instance = this;
    }

    public static LoginUI Instance()
    {
        if (instance == null)
        {
            Debug.LogError("LoginUI �ν��Ͻ��� �������� �ʽ��ϴ�.");
            return null;
        }
        return instance;
    }
    void Start()
    {
        input_id.SetActive(true);
        input_pw.SetActive(true);
        input_nickname.SetActive(false);
        btns_login.SetActive(true);
        btns_signup.SetActive(false);
        btn_play.SetActive(false);
        object_popup.SetActive(false);
        object_loading.SetActive(false);
        object_progress.SetActive(false);

        inputs[ID_INDEX] = input_id.GetComponent<TMP_InputField>();
        inputs[PW_INDEX] = input_pw.GetComponent<TMP_InputField>();
        inputs[NICKNAME_INDEX] = input_nickname.GetComponent<TMP_InputField>();
        text_progress = object_progress.GetComponent<TMP_Text>();
        text_popup = object_popup.GetComponentsInChildren<TMP_Text>()[0];
    }
    void CleanInputField()
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i].text = "";
        }
    }
    public void Play()
    {
        if (inputs[NICKNAME_INDEX].text.Length < 2)
        {
            ShowPopUp("�г����� 2���� �̻��̾�� �մϴ�.");
            return;
        }
        if (BackendServerManager.Instance().UpdateNickname(inputs[NICKNAME_INDEX].text))
        {
            BackendMatchManager.Instance().CreateMatchRoom();
            input_nickname.SetActive(false);
            btn_play.SetActive(false);
            object_loading.SetActive(true);
            object_progress.SetActive(true);
        }
        else
        {
            CleanInputField();
            ShowPopUp("�г��� ���� ����");
        }
    }
    public void Login()
    { 
        bool isSuccess = BackendServerManager.Instance().CustomLogin(inputs[ID_INDEX].text, inputs[PW_INDEX].text);
        if (isSuccess)
        {
            ShowPopUp("�α��� ����");
            input_id.SetActive(false);
            input_pw.SetActive(false);
            btns_login.SetActive(false);
            input_nickname.SetActive(true);
            btn_play.SetActive(true);
            BackendMatchManager.Instance().GetMatchList();
            BackendMatchManager.Instance().Invoke("JoinMatchMakingServer", 1.0f);
        }
        else
        {
            ShowPopUp("�α��� ����");
        }
    }
    public void SignUp()
    {
        bool isSuccess = BackendServerManager.Instance().CustomSignUp(inputs[ID_INDEX].text, inputs[PW_INDEX].text);
        if (isSuccess)
        {
            ShowPopUp("ȸ������ ����");
            ToggleLoginSignin();
        }
        else
        {
            ShowPopUp("ȸ������ ����");
        }
    }
    public void ToggleLoginSignin()
    {
        CleanInputField();
        btns_login.SetActive(!btns_login.activeSelf);
        btns_signup.SetActive(!btns_signup.activeSelf);
    }
    public void ShowPopUp(string text)
    {
        object_popup.SetActive(true);
        text_popup.text = text;
    }
    public void SetProgressText(string text)
    {
        text_progress.text = text;
    }
}
