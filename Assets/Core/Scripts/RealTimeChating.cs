using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

using AS = UnityEngine.AudioSource;

[RequireComponent(typeof(AS))]
public class RealTimeChating : MonoBehaviour
{
	// udpclient object
	UdpClient client;

	// port number
	public int port = 5005;
    public int listenport = 6005;

    public string ip = "";

	// audio things
	int lastSample = 0;
	AudioClip c;
	int FREQUENCY = 44100;

	// Cache the AudioSource component
	AS audioSource = null;


    void Start()
    {
        audioSource.clip = AudioClip.Create("test", 1000, 2, FREQUENCY, false);
    }
	// start from unity3d
	void OnEnable()
	{
        Debug.Log("Real time chating enabled");
		if (audioSource == null) {
			audioSource = GetComponent<AS> ();
		}

		client = new UdpClient(port);
		client.Client.Blocking = false;

		c = Microphone.Start(null, true, 100, FREQUENCY);
		//while(Microphone.GetPosition(null) < 0) {} 
	}

	// Unity Update Function
	void Update()
	{
		// audio
		int pos = Microphone.GetPosition (null);
		int diff = pos - lastSample;
		if (diff > 0) {
			float[] samples = new float[diff * c.channels];
			c.GetData (samples, lastSample);
			byte[] ba = ToByteArray(samples, c.channels);
			SendData (ba);
		}
		lastSample = pos;

		// Debug.Log("Enter ReceiveKinect's Update");
		// Declare the hashtable reference.
		try
		{
			// receive bytes
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, listenport);
			byte[] data = client.Receive(ref anyIP);
			PlayAudio(data);
		}
		catch (SocketException e)
		{
			if (e.SocketErrorCode != SocketError.WouldBlock)
			{
				throw;
			}
		}
		catch (Exception err)
		{
			Debug.LogError(err.ToString());
		}
	}

	// Unity Application Quit Function
	void OnApplicationQuit()
	{
		client.Close();
	}

    void OnDisable()
    {
        client.Close();
    }

	// Stop reading UDP messages

	// receive thread function
	private void ReceiveData()
	{
	}

	private void SendData(byte[] data)
	{
		Debug.Log(data.Length);
		try
		{
            client.Send(data, data.Length, ip, port);
			Debug.Log("Send!");
		}
		catch (Exception e)
		{
			Debug.Log ("Failed to send data. Reason: " + e.Message);
		}
	}

	public void PlayAudio(byte[] ba) {
        int chan;
		float[] f = ToFloatArray(ba, out chan);
		audioSource.clip.SetData(f, 0);
		if (!audioSource.isPlaying) audioSource.Play();
	}

	public byte[] ToByteArray(float[] floatArray, int channels) {
        byte[] res = null;
        using (var ms = new MemoryStream())
        {
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(channels);
                bw.Write(floatArray.Length);
                foreach (var elem in floatArray)
                    bw.Write(elem);
            }
            res = ms.ToArray();
        }
        return res;
	}

	public float[] ToFloatArray(byte[] byteArray, out int channels) {
        float[] res = null;
        using (var ms = new MemoryStream(byteArray))
        {
            using (var br = new BinaryReader(ms))
            {
                channels = br.ReadInt32();
                int len = br.ReadInt32();
                res = new float[len];
                for (int i = 0; i!= len; i++)
                {
                    res[i] = br.ReadSingle();
                }
            }
        }
        return res;
	}
}