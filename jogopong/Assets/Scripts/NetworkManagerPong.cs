using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

public class NetworkManagerPong : MonoBehaviour
{
   public enum Mode { Host, Client }
    public Mode mode = Mode.Host;

    [Header("Rede")]
    public string remoteIp = "127.0.0.1"; // para o cliente, o IP do Host
    public int port = 7777;
    public float sendInterval = 0.03f;

    [Header("Referências")]
    public Transform paddleHost;       // paddle do host
    public Transform paddleClient;     // paddle do cliente
    public Transform ball;             // bola
    public GameManager gameManager;    // gerencia pontuação

    [Header("Configuração bola")]
    public float velocidadeBola = 6f;

    private UdpClient udp;
    private IPEndPoint remoteEP;
    private ConcurrentQueue<(string, IPEndPoint)> recvQueue = new ConcurrentQueue<(string, IPEndPoint)>();
    private IPEndPoint lastClient;

    private float lastSend = 0f;

    private Vector2 ballPos;
    private Vector2 ballVel;

    [Serializable]
    public struct InputMsg
    {
        public float paddleY;
    }

    [Serializable]
    public struct StateMsg
    {
        public float hostPaddleY;
        public float clientPaddleY;
        public float ballX;
        public float ballY;
        public int scoreHost;
        public int scoreClient;
    }

    void Start()
    {
        if (mode == Mode.Host)
        {
            udp = new UdpClient(port);
            remoteEP = new IPEndPoint(IPAddress.Any, 0);
            Task.Run(() => ReceiveLoop());

            // Inicializa bola
            ballPos = ball.position;
            ballVel = new Vector2(velocidadeBola, UnityEngine.Random.Range(-velocidadeBola, velocidadeBola));
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
        while (true)
        {
            try
            {
                var result = await udp.ReceiveAsync();
                string msg = Encoding.UTF8.GetString(result.Buffer);
                recvQueue.Enqueue((msg, result.RemoteEndPoint));
            }
            catch (Exception e)
            {
                Debug.Log("Erro ReceiveLoop: " + e.Message);
            }
        }
    }

    void Update()
    {
        // === PROCESSA MENSAGENS RECEBIDAS ===
        while (recvQueue.TryDequeue(out var entry))
        {
            string msg = entry.Item1;
            IPEndPoint sender = entry.Item2;

            if (mode == Mode.Host)
            {
                // Host recebe input do cliente
                var input = JsonUtility.FromJson<InputMsg>(msg);
                Vector3 p = paddleClient.position;
                p.y = input.paddleY;
                paddleClient.position = p;
                lastClient = sender;
            }
            else
            {
                // Cliente recebe estado do Host
                var state = JsonUtility.FromJson<StateMsg>(msg);

                // Atualiza paddles
                Vector3 hostP = paddleHost.position;
                hostP.y = state.hostPaddleY;
                paddleHost.position = Vector3.Lerp(paddleHost.position, hostP, 0.7f);

                Vector3 myP = paddleClient.position;
                myP.y = state.clientPaddleY;
                paddleClient.position = Vector3.Lerp(paddleClient.position, myP, 0.7f);

                // Atualiza bola
                ball.position = Vector3.Lerp(ball.position, new Vector3(state.ballX, state.ballY, 0), 0.8f);

                // Atualiza pontuação
                gameManager.SetPontuacao(state.scoreHost, state.scoreClient);
            }
        }

        // === LÓGICA DE JOGO E ENVIO ===
        if (mode == Mode.Host)
        {
            // Host movimenta paddle dele
            float v = Input.GetAxis("Vertical");
            Vector3 pHost = paddleHost.position;
            pHost.y += v * Time.deltaTime * 7f;
            paddleHost.position = pHost;

            // Atualiza física da bola
            ballPos += ballVel * Time.deltaTime;

            // Limites da tela (ajuste conforme cena)
            if (ballPos.y > 4.5f || ballPos.y < -4.5f) ballVel.y = -ballVel.y;

            // Colisões simples com paddles
            if (Mathf.Abs(ballPos.x - paddleHost.position.x) < 0.5f &&
                Mathf.Abs(ballPos.y - paddleHost.position.y) < 1f && ballVel.x < 0)
                ballVel.x = -ballVel.x;

            if (Mathf.Abs(ballPos.x - paddleClient.position.x) < 0.5f &&
                Mathf.Abs(ballPos.y - paddleClient.position.y) < 1f && ballVel.x > 0)
                ballVel.x = -ballVel.x;

            // Gols
            if (ballPos.x < -9f)
            {
                gameManager.AumentarPontuacaoDoJogador2();
                ResetBall(1);
            }
            else if (ballPos.x > 9f)
            {
                gameManager.AumentarPontuacaoDoJogador1();
                ResetBall(-1);
            }

            // Atualiza posição da bola visualmente
            ball.position = new Vector3(ballPos.x, ballPos.y, 0);

            // Envia estado para cliente
            if (lastClient != null && Time.time - lastSend > sendInterval)
            {
                var state = new StateMsg
                {
                    hostPaddleY = paddleHost.position.y,
                    clientPaddleY = paddleClient.position.y,
                    ballX = ballPos.x,
                    ballY = ballPos.y,
                    scoreHost = gameManager.pontuacaoDoJogador1,
                    scoreClient = gameManager.pontuacaoDoJogador2
                };

                string json = JsonUtility.ToJson(state);
                byte[] data = Encoding.UTF8.GetBytes(json);
                udp.SendAsync(data, data.Length, lastClient);
                lastSend = Time.time;
            }
        }
        else
        {
            // Cliente movimenta paddle dele
            float v = Input.GetAxis("Vertical");
            Vector3 pClient = paddleClient.position;
            pClient.y += v * Time.deltaTime * 7f;
            paddleClient.position = pClient;

            // Envia posição do paddle para o host
            if (Time.time - lastSend > sendInterval)
            {
                var input = new InputMsg { paddleY = paddleClient.position.y };
                string json = JsonUtility.ToJson(input);
                byte[] data = Encoding.UTF8.GetBytes(json);
                udp.SendAsync(data, data.Length, remoteEP);
                lastSend = Time.time;
            }
        }
    }

    void ResetBall(int dir)
    {
        ballPos = Vector2.zero;
        ballVel = new Vector2(velocidadeBola * dir, UnityEngine.Random.Range(-velocidadeBola, velocidadeBola));
    }

    void OnApplicationQuit()
    {
        try { udp?.Close(); } catch { }
    }
}

