using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
     public string serverIP = "10.57.1.137";
    public int serverPort = 9050;
    public int playerId = 2;

    UdpClient udp;
    IPEndPoint serverEP;
    Thread receiveThread;
    ConcurrentQueue<string> incoming = new ConcurrentQueue<string>();

    // local references
    public Jogador jogadorLocal; // a raquete controlada localmente
    public Jogador jogadorRemoto; // a raquete do adversário (aplicada a partir de rede)
    public Bola bolaVisual; // visual da bola (após receber do servidor)
    public GameManager gameManager;

    // interpolation
    private float remoteYTarget;
    private float remoteYCurrent;
    public float interpSpeed = 15f;

    void Start()
    {
        serverEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        udp = new UdpClient();
        StartReceiveThread();

        // send HELLO to register
        Send($"HELLO|{playerId}");

        remoteYCurrent = jogadorRemoto.transform.position.y;
        remoteYTarget = remoteYCurrent;
    }

    void StartReceiveThread()
    {
        receiveThread = new Thread(() =>
        {
            var ep = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    var data = udp.Receive(ref ep);
                    var msg = Encoding.UTF8.GetString(data);
                    incoming.Enqueue(msg);
                }
                catch (Exception e)
                {
                    Debug.LogError("Client receive error: " + e);
                    break;
                }
            }
        })
        { IsBackground = true };
        receiveThread.Start();
    }

    void Update()
    {
        // read incoming
        while (incoming.TryDequeue(out var msg))
            ProcessMessage(msg);

        // send local paddle Y each frame (or throttle if desired)
        float y = jogadorLocal.transform.position.y;
        Send($"P|{playerId}|{y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        // smooth remote paddle
        remoteYCurrent = Mathf.Lerp(remoteYCurrent, remoteYTarget, Time.deltaTime * interpSpeed);
        var p = jogadorRemoto.transform.position;
        jogadorRemoto.SetYImmediate(remoteYCurrent);

        // apply inputs local via Jogador script as usual (Jogador handles input)
    }

    private void ProcessMessage(string msg)
    {
        var fields = msg.Split('|');
        var type = fields[0];
        if (type == "B")
        {
            // B|x|y|vx|vy
            if (fields.Length >= 5)
            {
                if (float.TryParse(fields[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bx) &&
                    float.TryParse(fields[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float by) &&
                    float.TryParse(fields[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float vx) &&
                    float.TryParse(fields[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float vy))
                {
                    bolaVisual.SetNetworkState(new Vector2(bx, by), new Vector2(vx, vy));
                }
            }
        }
        else if (type == "P")
        {
            // P|playerId|y
            int pid = int.Parse(fields[1]);
            if (float.TryParse(fields[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y))
            {
                if (pid != playerId)
                {
                    // remote paddle
                    remoteYTarget = y;
                }
                else
                {
                    // if server authoritative, we could reconcile local to server
                    // optional: correct local position to server value
                }
            }
        }
        else if (type == "S")
        {
            // S|p1|p2
            int p1 = int.Parse(fields[1]);
            int p2 = int.Parse(fields[2]);
            gameManager.SetPontuacao(p1, p2);
        }
    }

    private void Send(string msg)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(msg);
            udp.Send(data, data.Length, serverEP);
        }
        catch (Exception e)
        {
            Debug.LogError("Client send error: " + e);
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            receiveThread?.Abort();
            udp?.Close();
        }
        catch { }
    }
}

