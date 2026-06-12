using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UnityTcpClient : MonoBehaviour
{
    [Header("Server")]
    public string host = "127.0.0.1";
    public int port = 7777;

    private TcpClient _client;
    private StreamReader _reader;
    private StreamWriter _writer;

    private Thread _recvThread;
    private volatile bool _running;

    private readonly ConcurrentQueue<string> _incoming = new ConcurrentQueue<string>();

    void Start()
    {
        Connect();
    }

    void Update()
    {
        while (_incoming.TryDequeue(out var line))
        {
        }
        if (Input.GetKeyDown(KeyCode.C))
            SendLine("{\"type\":\"create_lobby\"}");

        if (Input.GetKeyDown(KeyCode.R))
            SendLine("{\"type\":\"set_ready\",\"ready\":true}");
    }

    public void Connect()
    {
        try
        {
            _client = new TcpClient();
            _client.NoDelay = true;
            _client.Connect(host, port);

            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            _running = true;
            _recvThread = new Thread(ReceiveLoop) { IsBackground = true };
            _recvThread.Start();
            
        }
        catch (Exception e)
        {
        }
    }

    public void SendLine(string json)
    {
        try
        {
            if (_writer == null) return;
            _writer.WriteLine(json); // один JSON = один рядок
        }
        catch (Exception e)
        {
            Debug.LogError("Send failed: " + e);
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            while (_running && _client != null && _client.Connected)
            {
                var line = _reader.ReadLine();
                if (line == null) break;
                _incoming.Enqueue(line);
            }
        }
        catch (Exception e)
        {
            _incoming.Enqueue("{\"type\":\"_client_error\",\"msg\":\"" + e.Message + "\"}");
        }
    }

    void OnDestroy()
    {
        _running = false;

        try { _client?.Close(); } catch { }
        try { _recvThread?.Join(200); } catch { }
    }
}