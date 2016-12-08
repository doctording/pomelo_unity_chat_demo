using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;
using System.Threading;

public class LoginGUI : MonoBehaviour
{
    public static string userName = "";
    public static string channel = "";
    public static JsonObject users = null;

    public static PomeloClient pomeloClient = null;

    public Texture2D pomelo;
    public GUISkin pomeloSkin;
    public GUIStyle pomeloStyle;

    protected bool _bNeedLoadScene = false;

    void Start()
    {
        pomelo = (Texture2D)Resources.Load("pomelo");
        pomeloStyle.normal.textColor = Color.black;
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
            Application.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
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
        //string host = "120.27.163.31";
        string host = "127.0.0.1";
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

    void OnGUI()
    {
        GUI.skin = pomeloSkin;
        GUI.color = Color.yellow;
        GUI.enabled = true;
        GUI.Label(new Rect(160, 50, pomelo.width, pomelo.height), pomelo);

        GUI.Label(new Rect(75, 350, 50, 20), "name:", pomeloStyle);
        userName = GUI.TextField(new Rect(125, 350, 90, 20), userName);
        GUI.Label(new Rect(225, 350, 55, 20), "channel:", pomeloStyle);
        channel = GUI.TextField(new Rect(280, 350, 100, 20), channel);

        if (GUI.Button(new Rect(410, 350, 70, 20), "OK"))
        {
            if (pomeloClient == null)
            {
                Login();
            }
        }
    }

}