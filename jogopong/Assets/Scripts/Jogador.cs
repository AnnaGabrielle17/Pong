using UnityEngine;

public class Jogador : MonoBehaviour
{
    public bool jogador1; // true = jogador 1, false = jogador 2
    public float velocidadeDoJogador = 5f;

    [Header("Limites Verticais")]
    public float yMinimo = -4f;
    public float yMaximo = 4f;

    private float inputVertical = 0f;

    void Update()
    {
        // captura input do teclado apenas para o jogador local
        if (jogador1)
            inputVertical = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        else
            inputVertical = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);

        // move o jogador localmente
        Vector2 pos = transform.position;
        pos.y += inputVertical * velocidadeDoJogador * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, yMinimo, yMaximo);
        transform.position = pos;
    }

    // métodos para controle de rede (servidor ou cliente)
    public void SetY(float y)
    {
        Vector2 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    public void SetYImmediate(float y)
    {
        Vector2 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    // opcional: retorna a posição Y atual (para enviar ao servidor)
    public float GetY()
    {
        return transform.position.y;
    }
}

