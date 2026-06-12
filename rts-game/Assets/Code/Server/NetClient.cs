using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetClient : MonoBehaviour
{
    [Header("Connection")]
    public string host = "127.0.0.1";
    public int port = 7777;

    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _recvThread;
    private volatile bool _running;

    private readonly StringBuilder _recvBuffer = new StringBuilder(4096);
    
    public event Action OnConnected;
    public event Action<string> OnDisconnected;
    public event Action<string> OnLine;

    public bool IsConnected => _client != null && _client.Connected;

    public void Connect()
    {
        if (IsConnected) return;

        try
        {
            _client = new TcpClient();
            _client.NoDelay = true;
            _client.Connect(host, port);

            _stream = _client.GetStream();
            _running = true;

            _recvThread = new Thread(RecvLoop) { IsBackground = true };
            _recvThread.Start();

            Debug.Log($"Connected to {host}:{port}");
            OnConnected?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Connect failed: " + e.Message);
            Cleanup("connect_failed");
        }
    }

    public void Disconnect(string reason = "client_disconnect")
    {
        Cleanup(reason);
    }

    public void SendLine(string json)
    {
        if (!IsConnected) return;
        
        string line = json + "\n";
        byte[] bytes = Encoding.UTF8.GetBytes(line);

        try
        {
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }
        catch (Exception e)
        {
            Cleanup("send_failed");
        }
    }

    private void RecvLoop()
    {
        byte[] buf = new byte[4096];

        try
        {
            while (_running && IsConnected)
            {
                int read = _stream.Read(buf, 0, buf.Length);
                if (read <= 0) break;

                string chunk = Encoding.UTF8.GetString(buf, 0, read);

                lock (_recvBuffer)
                {
                    _recvBuffer.Append(chunk);
                    
                    while (true)
                    {
                        int idx = _recvBuffer.ToString().IndexOf('\n');
                        if (idx < 0) break;

                        string line = _recvBuffer.ToString(0, idx).Trim();
                        _recvBuffer.Remove(0, idx + 1);

                        if (!string.IsNullOrEmpty(line))
                        {
                            MainThreadDispatcher.Enqueue(() => {
                                OnLine?.Invoke(line);
                            });
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            MainThreadDispatcher.Enqueue(() => Debug.LogWarning("Recv loop stopped: " + e.Message));
        }

        MainThreadDispatcher.Enqueue(() => Cleanup("remote_closed"));
    }

    private void Cleanup(string reason)
    {
        if (!_running && _client == null) return;

        _running = false;

        try { _stream?.Close(); } catch { }
        try { _client?.Close(); } catch { }

        _stream = null;
        _client = null;

        OnDisconnected?.Invoke(reason);
        Debug.Log("Disconnected: " + reason);
    }

    private void OnDestroy()
    {
        Cleanup("destroy");
    }
}