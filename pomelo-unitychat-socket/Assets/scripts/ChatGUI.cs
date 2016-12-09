using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;
using UnityEngine.UI;

public class ChatGUI : MonoBehaviour
{
    // 直接从login 获取users和PomeloClient
    private string userName = LoginGUI.userName;
    private PomeloClient pclient = LoginGUI.pomeloClient;


    private Vector3 chatScrollPosition;
    private Vector3 userScrollPosition;

    private string playerName;

    public static string inputField = "";   // 输入的聊天内容

    private ArrayList chatRecords = null;   // 记录数据的list
    private ArrayList userList = null; 

    public bool debug = true;

    public ScrollRect scroll_msg;
    public ScrollRect scroll;

    public InputField inField_msg;
    public Button btn;

    void Start()
    {
        Application.runInBackground = true;
        /* ----------------------------------------- */
        chatRecords = new ArrayList();
        userList = new ArrayList();

        InitUserWindow();

        pclient.on("onAdd", (data) =>
        {
            RefreshUserWindow("add", data);
        });

        pclient.on("onLeave", (data) =>
        {
            RefreshUserWindow("leave", data);
        });

        pclient.on("onChat", (data) =>
        {
            addMessage(data);
        });


        // 添加按钮的事件监听方法
        btn.onClick.AddListener(msgSend);
    }

    void msgSend()
    {
        inputField = inField_msg.text; // 获取输入框中的信息
        inField_msg.text = "";
        if (inputField == null || inputField == "")
            return;
        sendMessage(); 
    }

    //When quit, release resource
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) || Input.GetKey("escape"))
        {
            if (pclient != null)
            {
                pclient.disconnect();
            }
            Application.Quit();
        }

        // userlist 
        foreach (Transform child in scroll.transform.FindChild("Viewport").FindChild("Content").transform)
        {
            //Text it = child.gameObject.GetComponent<Text>();
            Destroy(child.gameObject);
        }
        foreach (string userName in userList)
        {
            GameObject t2 = new GameObject();
            t2.AddComponent<Text>();
            t2.GetComponent<Text>().text = userName;
            t2.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            RectTransform rectTransform2 = t2.GetComponent<RectTransform>();
            rectTransform2.localPosition = new Vector3(0, 0, 0);
            t2.transform.parent = scroll.transform.FindChild("Viewport").FindChild("Content");
        }

        // msglist
        foreach (Transform child in  scroll_msg.transform.FindChild("Viewport").FindChild("Content").transform)
        {
            Destroy(child.gameObject);
        } 
        foreach (ChatRecord cr in chatRecords)
        {
            string s = cr.name + ": " + cr.dialog;
            GameObject t3 = new GameObject();
            t3.AddComponent<Text>();
            t3.GetComponent<Text>().text = s;
            t3.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            RectTransform rectTransform2 = t3.GetComponent<RectTransform>();
            rectTransform2.localPosition = new Vector3(0, 0, 0);
            t3.transform.parent = scroll_msg.transform.FindChild("Viewport").FindChild("Content");
        }
    }

    //When quit, release resource
    void OnApplicationQuit()
    {
        if (pclient != null)
        {
            pclient.disconnect();
        }
    }

    void HitEnter()
    {
        sendMessage();
        chatScrollPosition.y = 10000000;
    }

    // Init userList and userWindow
    void InitUserWindow()
    {
        JsonObject jsonObject = LoginGUI.users;
        System.Object users = null;
        if (jsonObject.TryGetValue("users", out users))
        {
            string u = users.ToString();
            string[] initUsers = u.Substring(1, u.Length - 2).Split(new Char[] { ',' });
            int length = initUsers.Length;
            for (int i = 0; i < length; i++)
            {
                string s = initUsers[i];
                userList.Add(s.Substring(1, s.Length - 2));
            }
        }
    }

    //Update the userlist.
    void RefreshUserWindow(string flag, JsonObject msg)
    {
        System.Object user = null;
        if (msg.TryGetValue("user", out user))
        {
            if (flag == "add")
            {
                this.userList.Add(user.ToString());
            }
            else if (flag == "leave")
            {
                this.userList.Remove(user.ToString());
            }
        }
    }

    //Add message to chat window.
    void addMessage(JsonObject messge)
    {
        System.Object msg = null, fromName = null, targetName = null;
        if (messge.TryGetValue("msg", out msg) && messge.TryGetValue("from", out fromName) &&
            messge.TryGetValue("target", out targetName))
        {
            chatRecords.Add(new ChatRecord(fromName.ToString(), msg.ToString()));
        }
    }

    void sendMessage()
    {
        string reg = "^@.*?:";
        if (System.Text.RegularExpressions.Regex.IsMatch(inputField, reg))
        {
            solo();
        }
        else
        {
            chat("*", inputField);
            inputField = "";
        }
    }

    //Chat with someone only.
    void solo()
    {
        int userL = inputField.IndexOf(":");
        if (userL > 1)
        {
            string name = inputField.Substring(1, userL - 1);
            for (int i = 0; i < userList.Count; i++)
            {
                if (name == userList[i].ToString())
                {
                    string con = inputField.Substring(userL + 1, inputField.Length - userL - 1);
                    chat(name, con);
                    inputField = "@" + name + ":";
                    return;
                }
            }
        }
        chat("*", inputField);
        inputField = "";
    }

    void chat(string target, string content)
    {
        JsonObject message = new JsonObject();
        message.Add("rid", LoginGUI.channel);
        message.Add("content", content);
        message.Add("from", LoginGUI.userName);
        message.Add("target", target);
        pclient.request("chat.chatHandler.send", message, (data) =>
        {
            if (target != "*" && target != LoginGUI.userName)
            {
                chatRecords.Add(new ChatRecord(LoginGUI.userName, content));
            }
        });
    }

}