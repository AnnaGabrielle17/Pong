using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PongCliente4 : MonoBehaviour
{
    [Header("Configurações de Conexão")]
    public string ipServidor = "127.0.0.1";
    public int portaServidor = 9050;
    public int idJogador = 1;

    [Header("Referências")]
    public Transform[] raquetes;
    public Rigidbody2D rbBola;
    public GameManager gm;

    private UdpClient udpClient;
    private bool rodando = true;

    void Start()
    {
        udpClient = new UdpClient(9051 + idJogador);
        udpClient.Connect(ipServidor, portaServidor);

        Debug.Log($"Cliente {idJogador} conectado ao servidor {ipServidor}:{portaServidor}");

        _ = ReceberAtualizacoesAsync();
    }

    void Update()
    {
        // Enviar comandos locais
        if (Input.GetKey(KeyCode.W))
            EnviarComando("UP");
        else if (Input.GetKey(KeyCode.S))
            EnviarComando("DOWN");
    }

    private void EnviarComando(string comando)
    {
        string msg = $"{idJogador};{comando}";
        byte[] data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length);
    }

    private async Task ReceberAtualizacoesAsync()
    {
        while (rodando)
        {
            try
            {
                var result = await udpClient.ReceiveAsync();
                string msg = Encoding.UTF8.GetString(result.Buffer);
                string[] partes = msg.Split(';');

                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    // Atualiza posições
                    for (int i = 0; i < 4; i++)
                    {
                        if (i < raquetes.Length)
                        {
                            Vector3 pos = raquetes[i].position;
                            pos.y = float.Parse(partes[i]);
                            raquetes[i].position = pos;
                        }
                    }

                    rbBola.position = new Vector2(float.Parse(partes[4]), float.Parse(partes[5]));
                    gm.JogadorScore = int.Parse(partes[6]);
                    gm.InimigoScore = int.Parse(partes[7]);
                });
            }
            catch (Exception e)
            {
                Debug.LogError("Erro ao receber atualização: " + e.Message);
            }

            await Task.Yield();
        }
    }

    private void OnApplicationQuit()
    {
        rodando = false;
        udpClient?.Close();
    }
}