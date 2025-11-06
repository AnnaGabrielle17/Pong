using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PongServidor3 : MonoBehaviour
{
    public Transform[] raquetes; // 4 raquetes
    public Transform bola;
    private UdpClient udpServer;
    private IPEndPoint[] clientes = new IPEndPoint[3]; // até 3 clientes
    private bool rodando = true;

    private async void Start()
    {
        udpServer = new UdpClient(7777);
        Debug.Log("Servidor iniciado na porta 7777...");

        _ = Task.Run(ReceberMensagens);
        _ = Task.Run(EnviarAtualizacoes);
    }

    private async Task ReceberMensagens()
    {
        while (rodando)
        {
            var result = await udpServer.ReceiveAsync();
            string msg = Encoding.UTF8.GetString(result.Buffer);
            string[] partes = msg.Split(';');

            int idJogador = int.Parse(partes[0]);
            string comando = partes[1];

            // Registra o cliente
            if (clientes[idJogador - 1] == null)
                clientes[idJogador - 1] = result.RemoteEndPoint;

            // Movimenta a raquete correspondente
            float velocidade = 5f;
            if (comando == "UP")
                raquetes[idJogador - 1].Translate(Vector3.up * velocidade * Time.deltaTime);
            else if (comando == "DOWN")
                raquetes[idJogador - 1].Translate(Vector3.down * velocidade * Time.deltaTime);
        }
    }

    private async Task EnviarAtualizacoes()
    {
        while (rodando)
        {
            await Task.Delay(33); // ~30 FPS

            // Monta o pacote com as posições
            string dados = "";
            foreach (var r in raquetes)
                dados += $"{r.position.x},{r.position.y};";

            dados += $"{bola.position.x},{bola.position.y}";

            byte[] buffer = Encoding.UTF8.GetBytes(dados);

            // Envia para todos os clientes conectados
            foreach (var cliente in clientes)
            {
                if (cliente != null)
                    await udpServer.SendAsync(buffer, buffer.Length, cliente);
            }
        }
    }

    private void OnApplicationQuit()
    {
        rodando = false;
        udpServer?.Dispose();
    }
}


