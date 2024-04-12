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
    public Transform myPost_spwanPoint;
    public Transform post_spawnPoint;
    public GameObject post_Content;


    [System.Serializable]
    public struct PostData
    {
        public string nickname;
        public string title;
        public string content;
    }

    //�Խñ� ������ ������ ����ü ����Ʈ
    public List<PostData> postDataList = new List<PostData>();

    private static BackendNoticeBoard instance;

    public object QueryOperator { get; private set; }

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

    }

    public void WritePost()
    {
        string nickname = Backend.UserNickName;
        string date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int like = 0;
        string title = input_post[TITLE_INDEX].text;
        string content = input_post[CONTENT_INDEX].text;


        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
        {
            Debug.LogError("���� �Ǵ� ������ ����ֽ��ϴ�.");
            return;
        }
        Param param = new Param
        {
            { "nickname", nickname },
            { "date", date },
            { "like", like },
            { "title", title },
            { "content", content },
        };
        var bro = Backend.GameData.Insert("notice_table", param);
        if (bro.IsSuccess())
        {
            Debug.Log("�Խñ� �ۼ� ����");
            PostGet();
        }
        else
        {
            Debug.LogError("�Խñ� �ۼ� ����");
        }
    }

    public void PostGet()
    {
        Debug.Log("�Խñ� ��ȸ �Լ��� ȣ���մϴ�.");

        var bro = Backend.GameData.GetMyData("notice_table", new Where());
        if(bro.IsSuccess())
        {
            Debug.Log("�Խñ� ��ȸ�� �����߽��ϴ�. : " + bro);

            LitJson.JsonData postDataJson = bro.FlattenRows();

            if(postDataJson.Count <= 0)
            {
                Debug.LogWarning("�����Ͱ� �������� �ʽ��ϴ�.");
            }
            else
            {
                //������ �ҷ��� �Խñ� �����͸� ����.
                postDataList.Clear();

                for(int i = 0; i< postDataJson.Count; i++)
                {
                    string nickname = postDataJson[i]["nickname"].ToString();
                    string title = postDataJson[i]["title"].ToString();
                    string content = postDataJson[i]["content"].ToString();
                    //string date = postDataJson[i]["date"].ToString(); 

                    // ���ο� �Խñ� �����͸� ����Ʈ�� �߰�
                    PostData postData = new PostData();
                    postData.nickname = nickname;
                    postData.title = title;
                    postData.content = content;
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
        // ������ ������ �Խñ� UI ��ҵ��� �����Ѵ�.
        foreach (Transform child in post_spawnPoint)
        {
            Destroy(child.gameObject);
        }

        //���ο� �Խñ� �����͸� �̿��Ͽ� UI ��� ����
        foreach(PostData postData in postDataList)
        {
            GameObject postObject = Instantiate(post_Prefab, post_spawnPoint);

            postObject.transform.Find("Text_Info/NickName").GetComponent<TextMeshProUGUI>().text = postData.nickname;
            postObject.transform.Find("Text_Info/Title").GetComponent<TextMeshProUGUI>().text = postData.title;
            postObject.transform.Find("Text_Info/Info").GetComponent<TextMeshProUGUI>().text = postData.content;

            Button button = postObject.GetComponent<Button>();

            if (button != null)
            {
                int index = postDataList.IndexOf(postData);     //�ش� �Խñ��� �ε����� ����
                button.onClick.AddListener(() => ShowPost_Panel(index));
            }
        }
    }

    //�Խñ� �󼼺��� 
    public void ShowPost_Panel(int index)
    {
        InGameUI.Instance().ShowPost();

        TextMeshProUGUI show_title = post_Content.transform.Find("InputFieldGroup/Title_Panel/Title").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI show_content = post_Content.transform.Find("InputFieldGroup/Content_Panel/Content").GetComponent<TextMeshProUGUI>();

        show_title.text = postDataList[index].title;
        show_content.text = postDataList[index].content;

    }

    //public void MyPostShow()
    //{
       
    //}

    //public void DeletePost()
    //{

    //    //Backend.PlayerData.DeleteMyData("notice_table", )
    //}
}
