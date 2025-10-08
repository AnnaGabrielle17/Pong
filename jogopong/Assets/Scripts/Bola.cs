using UnityEngine;

public class Bola : MonoBehaviour
{

    public float velocidadeDaBola;

    public Rigidbody2D oRigidbody2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoverBola();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void MoverBola()
    {
        oRigidbody2D.linearVelocity = new Vector2(velocidadeDaBola, velocidadeDaBola);
    }
}
