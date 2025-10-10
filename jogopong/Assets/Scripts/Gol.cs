using UnityEngine;

public class Gol : MonoBehaviour
{
    public bool golDoJogador1;
    private GameManager gameManager;

    void Start()
    {
        // Encontra e guarda referência ao GameManager uma vez
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("GameManager não encontrado na cena. Verifique se o GameManager está presente.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager == null) return; 
       
        if (golDoJogador1)
        {
            gameManager.AumentarPontuacaoDoJogador2();
            Debug.Log("Gol do jogador 2 marcando contra o gol do jogador 1");
            other.gameObject.transform.position = Vector2.zero;
        }
        else
        {
            gameManager.AumentarPontuacaoDoJogador1();
            Debug.Log("Gol do jogador 1 marcando contra o gol do jogador 2");
            other.gameObject.transform.position = Vector2.zero;
        }
    }
}