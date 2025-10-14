using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int pontuacaoDoJogador1;
    public int pontuacaoDoJogador2;
    public TextMeshProUGUI textoDePontuacao;
    
public void SetPontuacao(int p1, int p2)
{
    pontuacaoDoJogador1 = p1;
    pontuacaoDoJogador2 = p2;
    AtualizarTextoDePontuacao();
} // <-- tipo correto para UI

    void Start()
    {
        AtualizarTextoDePontuacao(); // mostra 0x0 no início
    }

    public void AumentarPontuacaoDoJogador1()
    {
        pontuacaoDoJogador1 += 1;
        Debug.Log("Pontuação Jogador 1: " + pontuacaoDoJogador1);
        AtualizarTextoDePontuacao();
    }

    public void AumentarPontuacaoDoJogador2()
    {
        pontuacaoDoJogador2 += 1;
        Debug.Log("Pontuação Jogador 2: " + pontuacaoDoJogador2);
        AtualizarTextoDePontuacao();
    }

    public void AtualizarTextoDePontuacao()
    {
        textoDePontuacao.text = $"{pontuacaoDoJogador1} x {pontuacaoDoJogador2}";
    }
}