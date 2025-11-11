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
    public string remoteIp = "127.0.0.1";
    public int port = 7777;
    public float sendInterval = 0.03f;

    [Header("Referências de Jogo")]
    public Transform paddle1;
    public Transform paddle2;
    public Transform paddle3;
    public Transform paddle4;
    public Transform ball;
    public GameManager gameManager;

    [Header("Configuração da bola")]
    public float velocidadeBola = 6f;

    [Header("Configuração local")]
    public int paddleIndex = 1;
    public float velocidadeDoJogador = 7f;

    private UdpClient udp;
    private IPEndPoint remoteEP;
    private ConcurrentQueue<(string, IPEndPoint)> recvQueue = new ConcurrentQueue<(string, IPEndPoint)>();
    private IPEndPoint[] clients = new IPEndPoint[3];
    private float lastSend;
    private Vector2 ballPos, ballVel;

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
        public float ballX;
        public float ballY;
        public int scoreA;
        public int scoreB;
    }

    void Start()
    {
        if (mode == Mode.Host)
        {
            udp = new UdpClient(port);
            remoteEP = new IPEndPoint(IPAddress.Any, 0);
            Task.Run(ReceiveLoop);

            ballPos = ball.position;
            ballVel = new Vector2(velocidadeBola, UnityEngine.Random.Range(-velocidadeBola, velocidadeBola));
        }
        else
        {
            udp = new UdpClient();
            remoteEP = new IPEndPoint(IPAddress.Parse(remoteIp), port);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            Task.Run(ReceiveLoop);
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
        ProcessReceivedMessages();
        ProcessLocalInput();

        if (mode == Mode.Host)
        {
            UpdateBallAndCollisions();
            SendStateToClients();
        }
    }

    private void ProcessReceivedMessages()
    {
        while (recvQueue.TryDequeue(out var entry))
        {
            string msg = entry.Item1;
            IPEndPoint sender = entry.Item2;

            if (mode == Mode.Host)
            {
                var input = JsonUtility.FromJson<InputMsg>(msg);
                Transform target = input.paddleIndex switch
                {
                    2 => paddle2,
                    3 => paddle3,
                    4 => paddle4,
                    _ => null
                };
                if (target != null)
                {
                    Vector3 p = target.position;
                    p.y = input.paddleY;
                    target.position = p;
                }

                for (int i = 0; i < clients.Length; i++)
                    if (clients[i] == null)
                    {
                        clients[i] = sender;
                        break;
                    }
            }
            else
            {
                var state = JsonUtility.FromJson<StateMsg>(msg);
                float[] py = state.paddlesY;
                paddle1.position = new Vector3(paddle1.position.x, py[0], 0);
                paddle2.position = new Vector3(paddle2.position.x, py[1], 0);
                paddle3.position = new Vector3(paddle3.position.x, py[2], 0);
                paddle4.position = new Vector3(paddle4.position.x, py[3], 0);
                ball.position = new Vector3(state.ballX, state.ballY, 0);
                gameManager.SetPontuacao(state.scoreA, state.scoreB);
            }
        }
    }

    private void ProcessLocalInput()
    {
        float movimento = Input.GetAxis("Vertical") * velocidadeDoJogador * Time.deltaTime;
        Transform minhaRaquete = paddleIndex switch
        {
            1 => paddle1,
            2 => paddle2,
            3 => paddle3,
            4 => paddle4,
            _ => null
        };
        if (minhaRaquete != null)
        {
            Vector3 pos = minhaRaquete.position;
            pos.y += movimento;
            pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
            minhaRaquete.position = pos;

            if (mode != Mode.Host)
            {
                InputMsg input = new InputMsg { paddleIndex = paddleIndex, paddleY = pos.y };
                string json = JsonUtility.ToJson(input);
                byte[] data = Encoding.UTF8.GetBytes(json);
                udp.SendAsync(data, data.Length, remoteEP);
            }
        }
    }

    private void UpdateBallAndCollisions()
    {
        ballPos += ballVel * Time.deltaTime;
        if (ballPos.y > 4.5f || ballPos.y < -4.5f) ballVel.y = -ballVel.y;
        CheckCollision(paddle1);
        CheckCollision(paddle2);
        CheckCollision(paddle3);
        CheckCollision(paddle4);

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
        ball.position = new Vector3(ballPos.x, ballPos.y, 0);
    }

    private void CheckCollision(Transform paddle)
    {
        if (Mathf.Abs(ballPos.x - paddle.position.x) < 0.5f &&
            Mathf.Abs(ballPos.y - paddle.position.y) < 1f)
        {
            ballVel.x = -ballVel.x;
        }
    }

    private void ResetBall(int dir)
    {
        ballPos = Vector2.zero;
        ballVel = new Vector2(velocidadeBola * dir, UnityEngine.Random.Range(-velocidadeBola, velocidadeBola));
    }

    private void SendStateToClients()
    {
        if (Time.time - lastSend > sendInterval)
        {
            var state = new StateMsg
            {
                paddlesY = new float[]
                {
                    paddle1.position.y,
                    paddle2.position.y,
                    paddle3.position.y,
                    paddle4.position.y
                },
                ballX = ballPos.x,
                ballY = ballPos.y,
                scoreA = gameManager.pontuacaoDoJogador1,
                scoreB = gameManager.pontuacaoDoJogador2
            };

            string json = JsonUtility.ToJson(state);
            byte[] data = Encoding.UTF8.GetBytes(json);
            foreach (var c in clients)
                if (c != null) udp.SendAsync(data, data.Length, c);
            lastSend = Time.time;
        }
    }

    void OnApplicationQuit()
    {
        udp?.Close();
        udp = null;
    }

}
