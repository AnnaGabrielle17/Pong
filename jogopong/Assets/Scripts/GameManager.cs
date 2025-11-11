using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int pontuacaoDoJogador1;
    public int pontuacaoDoJogador2;
    public TextMeshProUGUI textoDePontuacao;

    void Start()
    {
        AtualizarTextoDePontuacao();
    }

    public void SetPontuacao(int p1, int p2)
    {
        pontuacaoDoJogador1 = p1;
        pontuacaoDoJogador2 = p2;
        AtualizarTextoDePontuacao();
    }

    public void AumentarPontuacaoDoJogador1()
    {
        pontuacaoDoJogador1++;
        AtualizarTextoDePontuacao();
    }

    public void AumentarPontuacaoDoJogador2()
    {
        pontuacaoDoJogador2++;
        AtualizarTextoDePontuacao();
    }

    private void AtualizarTextoDePontuacao()
    {
        if (textoDePontuacao)
            textoDePontuacao.text = $"{pontuacaoDoJogador1} x {pontuacaoDoJogador2}";
    }
}

