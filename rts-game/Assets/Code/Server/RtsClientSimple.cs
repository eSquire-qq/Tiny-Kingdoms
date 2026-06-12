using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class RtsClientSimple : MonoBehaviour
{
    public string host = "127.0.0.1";
    public int port = 7777;

    public Transform unit1Visual;

    TcpClient client;
    StreamReader reader;
    StreamWriter writer;
    Thread netThread;
    
    volatile float unitX;
    volatile float unitY;

    void Start()
    {
        Connect();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float x = world.x;
            float y = world.y;

            SendLine($"{{\"type\":\"cmd_move\",\"unitId\":1,\"x\":{x},\"y\":{y}}}");
        }
        if (unit1Visual != null)
            unit1Visual.position = new Vector3(unitX, unitY, unit1Visual.position.z);
    }

    void Connect()
    {
        client = new TcpClient();
        client.NoDelay = true;
        client.Connect(host, port);

        var stream = client.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);

        netThread = new Thread(NetLoop);
        netThread.IsBackground = true;
        netThread.Start();
    }

    void NetLoop()
    {
        try
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                HandleServerMessage(line);
            }
        }
        catch { /* connection lost */ }
    }

    void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"welcome\""))
        {
            Debug.Log("Connected: " + json);
            return;
        }
        
        if (json.Contains("\"type\":\"state\""))
        {
            unitX = ExtractFloat(json, "\"x\":");
            unitY = ExtractFloat(json, "\"y\":");
        }
    }

    void SendLine(string json)
    {
        try
        {
            writer.WriteLine(json);
            writer.Flush();
        }
        catch { }
    }

    float ExtractFloat(string json, string key)
    {
        int i = json.IndexOf(key, StringComparison.Ordinal);
        if (i < 0) return 0;
        i += key.Length;
        int end = i;
        while (end < json.Length && "0123456789.-".IndexOf(json[end]) >= 0) end++;
        return float.Parse(json.Substring(i, end - i), System.Globalization.CultureInfo.InvariantCulture);
    }

    void OnDestroy()
    {
        try { client?.Close(); } catch {}
        try { netThread?.Abort(); } catch {}
    }
}
