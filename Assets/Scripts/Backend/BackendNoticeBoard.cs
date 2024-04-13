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

    public bool isMyPost;

    [System.Serializable]
    public struct PostData
    {
        public string nickname;
        public string title;
        public string content;
        public string inDate;
    }

    //�Խñ� ������ ������ ����ü ����Ʈ
    public List<PostData> postDataList = new List<PostData>();
    
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
            //{ "gamer_id", id },
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
            }
            else
            {
                //���ο� �Խñ� �����͸� �ӽ� ����Ʈ�� �߰�
                List<PostData> tempPostDataList = new List<PostData>();

                for (int i = 0; i < postDataJson.Count; i++)
                {
                    string nickname = postDataJson[i]["nickname"].ToString();
                    string title = postDataJson[i]["title"].ToString();
                    string content = postDataJson[i]["content"].ToString();
                    string inDate = postDataJson[i]["inDate"].ToString();
                    //string date = postDataJson[i]["date"].ToString(); 

                    // ���ο� �Խñ� �����͸� ����Ʈ�� �߰�
                    PostData postData = new PostData();
                    postData.nickname = nickname;
                    postData.title = title;
                    postData.content = content;
                    postData.inDate = inDate;
                    postDataList.Add(postData);
                    tempPostDataList.Add(postData);

                }
                //������ �Խñ� �����͸� ������ �ʰ� �ӽ� ����Ʈ�� �߰��� �Ŀ� �Ѳ����� ����
                postDataList.Clear();
                postDataList.AddRange(tempPostDataList);
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
        //// ������ ������ �Խñ� UI ��ҵ��� �����Ѵ�.
        foreach (Transform child in post_spawnPoint)
        {
            Destroy(child.gameObject);
        }

        //���ο� �Խñ� �����͸� �̿��Ͽ� UI ��� ����
        foreach (PostData postData in postDataList)
        {
            GameObject postObject = Instantiate(post_Prefab, post_spawnPoint);

            postObject.transform.Find("Text_Info/NickName").GetComponent<TextMeshProUGUI>().text = postData.nickname;
            postObject.transform.Find("Text_Info/Title").GetComponent<TextMeshProUGUI>().text = postData.title;
            postObject.transform.Find("Text_Info/Info").GetComponent<TextMeshProUGUI>().text = postData.content;

            if (!postObjectDic.ContainsKey(postData.inDate))
            {
                //��ųʸ��� �Խñ� �����Ϳ� �Խñ� UI ��Ҹ� �߰�
                postObjectDic.Add(postData.inDate, postObject);              
            }

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

        Button button = InGameUI.Instance().btn_delete.GetComponent<Button>();
        if(button != null)
        {
            button.onClick.AddListener(() => deletePost(postDataList[index].inDate));
        }
    }
    public void deletePost(string inDate)
    {
        // �Խñ� ���� ��û ������
        var bro = Backend.GameData.DeleteV2("notice_table", inDate, Backend.UserInDate);
        if (bro.IsSuccess())
        {
            Debug.Log("�Խñ� ���� ����");

            if(postObjectDic.ContainsKey(inDate))
            {
                //��ųʸ����� �Խñ� �����Ϳ� �Խñ� UI ��Ҹ� ����
                Destroy(postObjectDic[inDate]);
                postObjectDic.Remove(inDate);

                postDataList.RemoveAll(post => post.inDate == inDate);
                ////�Խñ� ����Ʈ ����
                GetPost();
                //�Խñ� �г� �ݱ�
                InGameUI.Instance().ClosePost();
                //�Խñ� ���� ���� �޽��� ���
                Debug.Log("�Խñ� ���� ����");
               
            }
        }
        else
        {
            Debug.LogError("�Խñ� ���� ���� : " + bro.GetErrorCode());
        }

    }
}
