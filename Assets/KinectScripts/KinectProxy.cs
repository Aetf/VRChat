using KinectCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using BodyData = LibRawFrameData.KinectData;

public static class KinectDataExtension
{
    public static void SetJointPosition(this BodyData self, int jointIndex, ref Vector4 Position)
    {
        Position.Set(self.joints[jointIndex, 0],
                           self.joints[jointIndex, 1],
                           self.joints[jointIndex, 2],
                           0);
    }

    public static void SetJointTrackingState(this BodyData self, int jointIndex, ref NuiSkeletonPositionTrackingState State)
    {
        State = (NuiSkeletonPositionTrackingState)self.trackingState[jointIndex];
    }
};

public class RawFrameData
{
    public List<BodyData> bodies;
}

public class KinectProxy : MonoBehaviour
{

    public int LimitQueueSize = 0;

    public string serverip = "35.2.58.93";

    public int serverport = 5005;

    public int port = 5005;


    public void Initialize()
    {
        if (kinectConnected) return;

        DontDestroyOnLoad(gameObject);
    
        Debug.Log("KinectProxy powered up.");

        ConnectToKinectSource();

        frameQueue.Clear();
        client = new UdpClient(port);
        client.Client.Blocking = false;
    }

    public void Shutdown()
    {
        Debug.Log("KinectProxy shutting down.");
        client.Close();
        if (tcpclient != null)
        {
            tcpclient.Close();
        }
        frameQueue.Clear();
    }

    public void OnFrameArrived(RawFrameData data)
    {
        //InternalLog("Raw frame  data arrived");
        if (LimitQueueSize > 0 && frameQueue.Count >= LimitQueueSize)
        {
            InternalLog("raw frame data queue exceded max length, dropping earliest frame data!!");
            while (frameQueue.Count >= LimitQueueSize)
            {
                frameQueue.Dequeue();
            }
        }
        frameQueue.Enqueue(data);
    }

    public RawFrameData GetLatestFrame()
    {
        Debug.Log("Queue size " + frameQueue.Count + " before retrive");
        if (frameQueue.Count > 0)
            return frameQueue.Dequeue();
        return null;
    }

    private Queue<RawFrameData> frameQueue = new Queue<RawFrameData>();

    #region Kinect Source Connection
    bool kinectConnected = false;
    bool sceneLoaded = false;
    Socket tcpclient = null;
    private void ConnectToKinectSource()
    {
        InternalLog("Try connecting to kinect source...");
        try
        {
            Socket tcpclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpclient.BeginConnect(serverip, serverport,
                new AsyncCallback(KinectSourceConnected), tcpclient);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void KinectSourceConnected(IAsyncResult ar)
    {
        try
        {
            Socket tcpclient = (Socket)ar.AsyncState;
            tcpclient.EndConnect(ar);
            kinectConnected = true;

            InternalLog("Kinect source connected");

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write((Int32)1);
                    bw.Write(port);
                }
                var data = ms.ToArray();
                tcpclient.BeginSend(data, 0, data.Length, 0,
                                    new AsyncCallback(TcpSendCallback), tcpclient);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void TcpSendCallback(IAsyncResult ar)
    {
        try
        {
            Socket tcpclient = (Socket)ar.AsyncState;
            tcpclient.EndSend(ar);
            InternalLog("Tcp send finished");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void waitToLoadMainScene()
    {
        if (kinectConnected && !sceneLoaded)
        {
            sceneLoaded = true;
            Debug.Log("Log main scene!!");
            SceneManager.LoadScene("main");
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        KinectManager.Instance.ResetAvatarControllers();
    }
    #endregion

    #region UDP Client
    private UdpClient client = null;
    private BinaryFormatter formatter = new BinaryFormatter();

    private void PollData()
    {
        Debug.Log("KinectProxy polling");
        // Declare the hashtable reference.
        RawFrameData frame = new RawFrameData();

        try
        {
            // receive bytes
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref anyIP);
            using (var ms = new MemoryStream(data))
            {
                // deserialize.
                frame.bodies = (List<BodyData>)formatter.Deserialize(ms);
                OnFrameArrived(frame);
            }
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock)
            {
                throw;
            }
        }
        catch (SerializationException e)
        {
            Debug.LogError("Failed to deserialize. Reason: " + e.Message);
        }
        catch (Exception err)
        {
            Debug.LogError(err.ToString());
        }
    }
    #endregion

    #region Singleton
    private static KinectProxy instance = null;
    public static KinectProxy Instance
    {
        get {
            if (!instance)
            {
                instance = GameObject.FindObjectOfType(typeof(KinectProxy)) as KinectProxy;
                // Automatically create the EventCenter if none was found upon start of the game.
                if (!instance)
                    instance = (new GameObject("ProxyKinect")).AddComponent<KinectProxy>();
            }
            return instance;
        }
    }

    // Should be called in MonoBehaviour.Awake
    private void EnforceSingleton()
    {
        if (Instance != this)
        {
            InternalLog("Duplicate EventCenter, destroying the new one.");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Unity events handler
    void Start()
    {
        EnforceSingleton();
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        waitToLoadMainScene();
        if (kinectConnected)
            PollData();
    }

    public void OnApplicationQuit()
    {
        Shutdown();
        instance = null;
    }
    #endregion

    private void InternalLog(string msg)
    {
        Debug.Log(string.Format("KinectProxy\t{0}\t\t\t{1}", System.DateTime.Now.ToString("hh:mm:ss.fff"), msg));
    }
}
