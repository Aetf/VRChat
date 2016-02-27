using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ChatManagerCore))]
public class FixedIP : MonoBehaviour {

    public string IP = "127.0.0.1";
    public int port = 6005;
    public bool reverse = false;

	// Use this for initialization
	void Start () {
        var man = GetComponent<ChatManagerCore>();
        man.SetupFriendAt(IP, port, reverse);
	}
}
