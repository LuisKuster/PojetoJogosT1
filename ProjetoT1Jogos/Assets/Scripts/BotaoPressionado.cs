using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotaoPressionado : MonoBehaviour
{
    public int numBotao;
    public PortaCorrerController portaCorrer;

    [Header("Puzzle Botao 2")]
    public Outline outlinePorta;
    public float outlineWidth = 5f;
    public AudioClip somVitoria;

    [Range(0f, 1f)]
    public float volumeSom = 1f;

    private int         contadorBotao2  = 0;
    private bool        puzzleResolvido = false;
    private AudioSource audioSource;
    private Vector3     posicaoInicialPorta;

    void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(x => Pressionei());

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;

        if (outlinePorta != null)
            outlinePorta.OutlineWidth = 0f;

        if (portaCorrer != null)
            posicaoInicialPorta = portaCorrer.transform.position;
    }

    void Update()
    {
        if (puzzleResolvido && outlinePorta != null && outlinePorta.OutlineWidth > 0f && portaCorrer != null)
        {
            float distancia = Vector3.Distance(portaCorrer.transform.position, posicaoInicialPorta);
            if (distancia > 0.05f)
                outlinePorta.OutlineWidth = 0f;
        }
    }

    public void Pressionei()
    {
        if (numBotao == 2)
        {
            contadorBotao2++;

            if (contadorBotao2 >= 3 && !puzzleResolvido)
            {
                puzzleResolvido = true;

                if (audioSource != null && somVitoria != null)
                    audioSource.PlayOneShot(somVitoria, volumeSom);

                if (outlinePorta != null)
                    outlinePorta.OutlineWidth = outlineWidth;

                portaCorrer.Destravar();
            }
        }
    }
}