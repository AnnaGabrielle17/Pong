using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float velocidadeDoJogador;

    public bool jogador1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(jogador1 == true)
        {
           MoverJogador1();
        }
        else
        {
            MoverJogador2();
        }
    }
    private void MoverJogador1()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
           transform.Translate(Vector2.up * velocidadeDoJogador * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            transform.Translate(Vector2.down * velocidadeDoJogador * Time.deltaTime);
        }
        
    }

    private void MoverJogador2()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Translate(Vector2.up * velocidadeDoJogador * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.Translate(Vector2.down * velocidadeDoJogador * Time.deltaTime);
        }
    }
}
