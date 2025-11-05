using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("UI do Placar")]
    public TextMeshProUGUI textoBranco;
    public TextMeshProUGUI textoPreto;

    public int JogadorScore { get; set; }
    public int InimigoScore { get; set; }

    public UnityEvent AtualizarPlacar;

    private void Awake()
    {
        if (AtualizarPlacar == null)
            AtualizarPlacar = new UnityEvent();

        AtualizarPlacar.AddListener(AtualizarUI);
    }

    private void AtualizarUI()
    {
        if (textoBranco != null)
            textoBranco.text = JogadorScore.ToString();

        if (textoPreto != null)
            textoPreto.text = InimigoScore.ToString();
    }
}
