using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ChatDiscover : NetworkDiscovery {

    public float ClientModeTimeoutSec = 2;
    public bool beginDiscover = false;

    bool started = false;
    void Start()
    {
        Initialize();
        useNetworkManager = false;
        showGUI = false;
        timestamp = 0;
        started = true;
    }

    // Use this for initialization
    float timestamp;
	void StartChatDiscover () {
        if (!started) return;

        Debug.Log("ChatDiscover enabled");
        timestamp = Time.deltaTime;
        StartAsClient();
	}

    void OnDisabled()
    {
        Debug.Log("ChatDiscover disabled");
        if (running)
            StopBroadcast();
    }
	
	// Update is called once per frame
	void Update () {
        timestamp += Time.deltaTime;

        if (beginDiscover)
        {
            beginDiscover = false;
            StartChatDiscover();
        }
	    
        if (timestamp >= ClientModeTimeoutSec && isClient && running)
        {
            Debug.Log("Discover client mode timed out, switch to server mode");
            StopBroadcast();
            //Debug.Log("recevied broadcasts: " + broadcastsReceived.ToString());
            StartAsServer();
        }
	}

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        Debug.Log("Got broadcast from " + fromAddress);
        StopBroadcast();
        var core = FindObjectOfType<ChatManagerCore>();
        core.SetupFriendAt(fromAddress, int.Parse(data));
    }
}
