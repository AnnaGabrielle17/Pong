using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int pontuacaoDoJogador1;
    public int pontuacaoDoJogador2;

    void Start()
    {
        // Opcional: inicializações
    }

    void Update()
    {
        // Remova se não for usar
    }

    public void AumentarPontuacaoDoJogador1()
    {
        pontuacaoDoJogador1 += 1;
        Debug.Log("Pontuação Jogador 1: " + pontuacaoDoJogador1);
    }

    public void AumentarPontuacaoDoJogador2()
    {
        pontuacaoDoJogador2 += 1;
        Debug.Log("Pontuação Jogador 2: " + pontuacaoDoJogador2);
    }
}