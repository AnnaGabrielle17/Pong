using UnityEngine;

public class Bola : MonoBehaviour
{

    public float velocidadeDaBola;

    public Rigidbody2D oRigidbody2D;
    public Vector2 GetVelocity()
{
    return oRigidbody2D.linearVelocity;
}

public void SetNetworkState(Vector2 pos, Vector2 vel)
{
    // usado no cliente: posiciona a bola visualmente conforme o servidor
    transform.position = pos;
    oRigidbody2D.linearVelocity = vel;
}
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
