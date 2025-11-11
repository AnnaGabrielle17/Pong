using UnityEngine;

public class Bola : MonoBehaviour
{
    public float velocidadeDaBola = 6f;
    public Rigidbody2D oRigidbody2D;

    void Start()
    {
        MoverBola();
    }

    private void MoverBola()
    {
        // Define direção inicial aleatória
        float direcaoX = Random.value < 0.5f ? -1f : 1f;
        float direcaoY = Random.Range(-0.5f, 0.5f);
        oRigidbody2D.linearVelocity = new Vector2(direcaoX, direcaoY).normalized * velocidadeDaBola;
    }

}
