using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;
using System.Threading;
using UnityEngine.UI;

public class LoginGUI : MonoBehaviour
{
    public static string userName = "";
    public static string channel = "";
    public static JsonObject users = null;

    public static PomeloClient pomeloClient = null;

    protected bool _bNeedLoadScene = false;

    private Button btn_login;
    private InputField infield_username;
    private InputField infield_channelid;

    void Start()
    {
        // 找到各个控件
        infield_username = GameObject.FindGameObjectWithTag("username").GetComponent<InputField>();
        infield_channelid = GameObject.FindGameObjectWithTag("channel").GetComponent<InputField>();
        btn_login = GameObject.FindGameObjectWithTag("btn_login").GetComponent<Button>();

        // 添加按钮的事件监听方法
        btn_login.onClick.AddListener(Login);
    }

    //When quit, release resource
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (pomeloClient != null)
            {
                pomeloClient.disconnect();
            }
            Application.Quit();
        }

        if(_bNeedLoadScene)
        {
            // 场景切换
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    //When quit, release resource
    void OnApplicationQuit()
    {
        if (pomeloClient != null)
        {
            pomeloClient.disconnect();
        }
    }

    //Login the chat application and new PomeloClient.
    void Login()
    {
        userName = infield_username.text; // 获取输入框中的信息
        channel = infield_channelid.text;

        if (userName == "" || channel == "")
            return;

        string host = "127.0.0.1"; // game-server的host和port
        int port = 3014;

        pomeloClient = new PomeloClient();

        //listen on network state changed event
        pomeloClient.NetWorkStateChangedEvent += (state) =>
        {
            Debug.logger.Log("CurrentState is:" + state);
        };
        
        // 连接gate 得到connect的host和port
        pomeloClient.initClient(host, port, () =>
        {
            //The user data is the handshake user params
            JsonObject user = new JsonObject();
            //user["uid"] = userName;
            pomeloClient.connect(user, data =>
            {
                //process handshake call back data
                JsonObject msg = new JsonObject();
                msg["uid"] = userName;
                pomeloClient.request("gate.gateHandler.queryEntry", msg, OnQuery);
            });
        });
    }

    void OnQuery(JsonObject result)
    {
        if (Convert.ToInt32(result["code"]) == 200)
        {
            pomeloClient.disconnect();

            string host = (string)result["host"];
            int port = Convert.ToInt32(result["port"]);

            pomeloClient = new PomeloClient();

            pomeloClient.initClient(host, port, () =>
            {
                //The user data is the handshake user params
                JsonObject user = new JsonObject();
                pomeloClient.connect(user, data =>
                {
                    Entry();
                });
            });
        }
    }

    // 根据得到的connect 请求进入场景，服务端返回该用户频道的所有用户，广播add消息
    void Entry()
    {
        JsonObject userMessage = new JsonObject();
        userMessage.Add("username", userName);
        userMessage.Add("rid", channel);
        if (pomeloClient != null)
        {
            pomeloClient.request("connector.entryHandler.enter", userMessage, (data) =>
            {
                users = data;
                _bNeedLoadScene = true;
            });
        }
    }
}