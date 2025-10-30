using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

public class NetworkManagerQuad : MonoBehaviour
{
    public enum Mode { Host, Client }
    public Mode mode = Mode.Host;

    [Header("Rede")]
    public string remoteIp = "10.57.10.8";
    public int port = 7777;
    public float sendInterval = 0.03f;

    [Header("Referências de Jogo")]
    public Transform paddle1;  // Host esquerda (Jogo A)
    public Transform paddle2;  // Cliente direita (Jogo A)
    public Transform paddle3;  // Host esquerda (Jogo B)
    public Transform paddle4;  // Cliente direita (Jogo B)
    public Transform ballA;    // Bola da primeira partida
    public Transform ballB;    // Bola da segunda partida
    public GameManager gameManager;

    [Header("Configuração da bola")]
    public float velocidadeBola = 6f;

    private UdpClient udp;
    private IPEndPoint remoteEP;
    private ConcurrentQueue<(string, IPEndPoint)> recvQueue = new ConcurrentQueue<(string, IPEndPoint)>();
    private IPEndPoint[] clients = new IPEndPoint[3];
    private float lastSend;
    private Vector2 ballAPos, ballAVel;
    private Vector2 ballBPos, ballBVel;

    [Serializable]
    public struct InputMsg
    {
        public int paddleIndex;
        public float paddleY;
    }

    [Serializable]
    public struct StateMsg
    {
        public float[] paddlesY;
        public float ballAX;
        public float ballAY;
        public float ballBX;
        public float ballBY;
        public int scoreA1;
        public int scoreA2;
        public int scoreB1;
        public int scoreB2;
    }

    void Start()
    {
        if (mode == Mode.Host)
        {
            udp = new UdpClient(port);
            remoteEP = new IPEndPoint(IPAddress.Any, 0);
            Task.Run(() => ReceiveLoop());
            ballAPos = ballA.position;
            ballBPos = ballB.position;
            ballAVel = new Vector2(velocidadeBola, UnityEngine.Random.Range(-velocidadeBola, velocidadeBola));
            ballBVel = new Vector2(-velocidadeBola, UnityEngine.Random.Range(-velocidadeBola, velocidadeBola));
        }
        else
        {
            udp = new UdpClient();
            remoteEP = new IPEndPoint(IPAddress.Parse(remoteIp), port);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            Task.Run(() => ReceiveLoop());
        }
    }

    async Task ReceiveLoop()
    {
        while (udp != null)
        {
            try
            {
                var result = await udp.ReceiveAsync();
                string msg = Encoding.UTF8.GetString(result.Buffer);
                recvQueue.Enqueue((msg, result.RemoteEndPoint));
            }
            catch { break; }
        }
    }

    void Update()
    {
        // se for host, processa física e envia estado
        // se for cliente, envia input e recebe estado
        // (implementação omitida para simplificar esta base)
    }

    void OnApplicationQuit()
    {
        udp?.Close();
        udp = null;
    }
}
