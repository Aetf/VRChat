using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.IO;

public static class Helper
{
    public static GameObject FindInChildren(this GameObject go, string name)
    {
        foreach (var x in go.GetComponentsInChildren<Transform>())
            if (x.gameObject.name == name)
                return x.gameObject;
        return null;
    }
}

[RequireComponent(typeof(ChatDiscover))]
public class ChatManagerCore : MonoBehaviour {

    public string serverip = "35.2.58.93";
    public int serverport = 5005;

    ChatDiscover discover = null;
    public RealTimeChating realtimechating = null;
    public GameObject friendObject = null;
    public GameObject myObject = null;
	// Use this for initialization
	void Start () {
        var discover = GetComponent<ChatDiscover>();
        var cardboardCamera = GameObject.FindGameObjectWithTag("CardboardCam");

        if (discover == null)
        {
            Debug.LogError("Can't find ChatDiscover");
        }
        if (realtimechating == null)
        {
            Debug.LogError("Can't find RealTimeChating");
        }
        if (friendObject == null)
        {
            Debug.LogError("Can't find friend");
        }

        if (friendObject != null)
            //friendObject.SetActive(false);

        //discover.GetComponent<ChatDiscover>().broadcastData = "" + realtimechating.listenport;
        //discover.GetComponent<ChatDiscover>().ClientModeTimeoutSec = 15;
        //discover.beginDiscover = true;

        if (cardboardCamera != null && friendObject != null)
        {
            var head = myObject.FindInChildren("Head_end");
            if (head == null)
            {
                Debug.Log("Can't find a head!");
                return;
            }
            
            var offset = head.transform.position - myObject.transform.position;

            cardboardCamera.transform.position = offset;
            cardboardCamera.transform.SetParent(myObject.transform);
        }

        KinectManager.Instance.Player1Avatars.Clear();
        KinectManager.Instance.Player2Avatars.Clear();
        //KinectManager.Instance.Player1Avatars.Add(GameObject.FindGameObjectWithTag("Player"));
        //KinectManager.Instance.Player1Avatars.Add(GameObject.FindGameObjectWithTag("Friend"));
        KinectManager.Instance.ResetAvatarControllers();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetupFriendAt(string ipaddr, int port, bool reverse = false)
    {
        if (discover != null)
            discover.enabled = false;

        Debug.Log("Setting up peer at " + ipaddr + ":" + port);
        // setup voice
        realtimechating.ip = ipaddr;
        realtimechating.port = port;
        realtimechating.enabled = true;

        // setup character
        if (friendObject == null)
        {
            Debug.LogError("Error: can't find character for the second player");
            return;
        }
        friendObject.SetActive(true);
        var kinectManager = KinectManager.Instance;
        kinectManager.TwoUsers = true;
        kinectManager.Player1Avatars.Clear();
        kinectManager.Player2Avatars.Clear();
        if (reverse)
        {
            kinectManager.Player1Avatars.Add(GameObject.FindGameObjectWithTag("Player"));
            kinectManager.Player2Avatars.Add(friendObject);
        } else
        {
            kinectManager.Player2Avatars.Add(GameObject.FindGameObjectWithTag("Player"));
            kinectManager.Player1Avatars.Add(friendObject);
        }
        kinectManager.ResetAvatarControllers();
    }

    private bool requestFriendIpFinished = false;
    private string friendIp;
    private int friendTcpPort;
    private void RequestFriendIp()
    {
        Debug.Log("Requesting friend endpoint");
        try
        {
            Socket tcpclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpclient.BeginConnect(serverip, serverport,
                new AsyncCallback(RequestFriendIpCallback1), tcpclient);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void RequestFriendIpCallback1(IAsyncResult ar)
    {
        var tcpclient = (Socket)ar.AsyncState;
        try
        {
            tcpclient.EndConnect(ar);
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(2);
                }
                var data = ms.ToArray();
                tcpclient.BeginSend(data, 0, data.Length, 0,
                    new AsyncCallback(RequestFriendIpCallback2), tcpclient);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private byte[] buffer = new byte[1024];
    private void RequestFriendIpCallback2(IAsyncResult ar)
    {
        var tcpclient = (Socket)ar.AsyncState;
        try
        {
            tcpclient.EndSend(ar);
            tcpclient.BeginReceive(buffer, 0, buffer.Length, 0,
                   new AsyncCallback(RequestFriendIpCallback3), tcpclient);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void RequestFriendIpCallback3(IAsyncResult ar)
    {
        var tcpclient = (Socket)ar.AsyncState;
        try
        {
            int read = tcpclient.EndReceive(ar);
            using (var ms = new MemoryStream(buffer, 0, read))
            {
                using (var br = new BinaryReader(ms))
                {
                    string friendip = br.ReadString();
                    int friendport = br.ReadInt32();
                    requestFriendIpFinished = true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
