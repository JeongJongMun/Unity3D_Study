using BackEnd;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;   

public class BackendNoticeBoard : MonoBehaviour
{
    public TMP_InputField[] input_post;
    const byte TITLE_INDEX = 0;
    const byte CONTENT_INDEX = 1;

    public GameObject post_Prefab;
    public Transform post_spawnPoint;
    public GameObject post_Content;
    public bool isMyPost = false;

    [System.Serializable]
    public struct PostData
    {
        public string title;
        public string content;
        public string inDate;
        public string id;
    }

    //�Խñ� ������ ������ ����ü ����Ʈ
    public List<PostData> postDataList = new List<PostData>();
    
    //�Խñ� ������ ������ ��ųʸ�
    public Dictionary<string, GameObject> postObjectDic = new Dictionary<string, GameObject>();

    private static BackendNoticeBoard instance;

    void Awake()
    {
        instance = this;
    }

    public static BackendNoticeBoard Instance()
    {
        if (instance == null)
        {
            Debug.LogError("BackendNoticeBoard �ν��Ͻ��� �������� �ʽ��ϴ�.");
            return null;
        }
        return instance;
    }

    void Start()
    {
        AddPostList();
    }

    public void WritePost()
    {
        string id = BackendServerManager.Instance().GetId();
        string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int like = 0;
        string title = input_post[TITLE_INDEX].text;
        string content = input_post[CONTENT_INDEX].text;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
        {
            Debug.LogError("���� �Ǵ� ������ ����ֽ��ϴ�.");
            InGameUI.Instance().ShowPopUp("���� �Ǵ� ������ ����ֽ��ϴ�.");
            return;
        }
        Param param = new Param
        {
            { "date", date },
            { "like", like },
            { "title", title },
            { "content", content },
            { "id", id }
        };
        var bro = Backend.GameData.Insert("notice_table", param);
        if (bro.IsSuccess())
        {
            Debug.Log("�Խñ� �ۼ� ����");
            GetPost();
        }
        else
        {
            Debug.LogError("�Խñ� �ۼ� ����");
        }

        InGameUI.Instance().TogglePanelWrite();
    }


    public void GetPost()
    {
        Debug.Log("�Խñ� ��ȸ �Լ��� ȣ���մϴ�.");

        BackendReturnObject bro;
        if(isMyPost)
        {
            bro = Backend.GameData.GetMyData("notice_table", new Where());
        }
        else
        {
            bro = Backend.GameData.Get("notice_table", new Where());

        }
        if (bro.IsSuccess())
        {
            Debug.Log("�Խñ� ��ȸ�� �����߽��ϴ�. : " + bro);

            LitJson.JsonData postDataJson = bro.FlattenRows();

            if (postDataJson.Count <= 0)
            {
                Debug.LogWarning("�����Ͱ� �������� �ʽ��ϴ�.");
                InGameUI.Instance().ShowPopUp("�ۼ��� �Խñ��� �����ϴ�.");
            }
            else
            {
                // ������ �Խñ� �����͸� ��� ����
                postDataList.Clear();

                for (int i = 0; i < postDataJson.Count; i++)
                {
                    string title = postDataJson[i]["title"].ToString();
                    string content = postDataJson[i]["content"].ToString();
                    string inDate = postDataJson[i]["inDate"].ToString();
                    string id = postDataJson[i]["id"].ToString();

                    // �Խñ� �����͸� ������ ����ü�� �����Ͽ� ����Ʈ�� �߰�
                    PostData postData = new PostData();
                    postData.title = title;
                    postData.content = content;
                    postData.inDate = inDate;
                    postData.id = id;
                    postDataList.Add(postData);

                }
                AddPostList();
            }
        }
        else
        {
            Debug.LogError("�Խñ� ��ȸ ���� : " + bro.GetErrorCode());
        }

    }

    private void AddPostList()
    {
        //������ �Խñ� UI ��Ҹ� ��� ����
        foreach (GameObject postObject in postObjectDic.Values)
        {
            Destroy(postObject);
        }

        postObjectDic.Clear();

        //���ο� �Խñ� �����͸� �̿��Ͽ� UI ��� ����
        foreach (PostData postData in postDataList)
        {
            GameObject postObject = Instantiate(post_Prefab, post_spawnPoint);

            //LoginUI���� ����ϴ� ID�� �����ͼ� �Խñۿ� ǥ��
            string loginId = BackendServerManager.Instance().GetId();

            postObject.transform.Find("Text_Info/ID").GetComponent<TextMeshProUGUI>().text = postData.id;
            postObject.transform.Find("Text_Info/Title").GetComponent<TextMeshProUGUI>().text = postData.title;
            postObject.transform.Find("Text_Info/Content").GetComponent<TextMeshProUGUI>().text = postData.content;

            // �ߺ� Ű ����
            if (!postObjectDic.ContainsKey(postData.inDate))
            {
                //��ųʸ��� �Խñ� �����Ϳ� �Խñ� UI ��Ҹ� �߰�
                postObjectDic.Add(postData.inDate, postObject);
            }

            Button button = postObject.GetComponent<Button>();
            if (button != null)
            {
                int index = postDataList.IndexOf(postData);     //�ش� �Խñ��� �ε����� ����
                button.onClick.AddListener(() => ShowPostPanel(postData.inDate));
            }

            postObject.transform.Find("DeleteBtn").gameObject.SetActive(isMyPost);
            Button delete_btn = postObject.transform.Find("DeleteBtn").GetComponent<Button>();
            if (delete_btn != null)
            {
                delete_btn.onClick.AddListener(() => DeletePost(postData.inDate));
            }

        }

    }

    //�Խñ� �󼼺��� 
    public void ShowPostPanel(string inDate)
    {
        //�ε����� ��ȿ���� Ȯ��
        if (!postObjectDic.ContainsKey(inDate))
        {
            Debug.LogError("�ش� inDate�� �Խñ��� �������� �ʽ��ϴ�.");
            return;
        }

        InGameUI.Instance().TogglePost();
        
        foreach (PostData postData in postDataList)
        {
            //�ش� �ε����� �Խñ� �����͸� �̿��Ͽ� �󼼺��� UI�� ������ ǥ��
            TextMeshProUGUI show_title = post_Content.transform.Find("InputFieldGroup/Title_Panel/Title").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI show_content = post_Content.transform.Find("InputFieldGroup/Content_Panel/Content").GetComponent<TextMeshProUGUI>();

            if (postData.inDate == inDate)
            {
                show_title.text = postData.title;
                show_content.text = postData.content;
            }
        }
    }
    public void DeletePost(string inDate)
    {
        // �Խñ� ���� ��û ������
        var bro = Backend.GameData.DeleteV2("notice_table", inDate, Backend.UserInDate);
        if (bro.IsSuccess())
        {
            Destroy(postObjectDic[inDate].gameObject);
            postObjectDic.Remove(inDate);

            //����Ʈ������ ����
            postDataList.RemoveAll(post => post.inDate == inDate);
            AddPostList();
            
            Debug.Log("�Խñ� ���� ����");
        }
        else
        {
            Debug.LogError("�Խñ� ���� ���� : " + bro.GetErrorCode());
        }

    }
}
