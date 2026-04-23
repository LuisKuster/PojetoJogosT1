using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotaoPressionado : MonoBehaviour
{
    public int numBotao;
    public PortaCorrerController portaCorrer;

    [Header("Puzzle Botao 2")]
    [Tooltip("Outline da porta de correr — some quando a porta comeca a mover.")]
    public Outline outlinePorta;

    [Tooltip("Largura do outline quando o puzzle e resolvido.")]
    public float outlineWidth = 5f;

    [Tooltip("Som de vitoria tocado ao resolver o puzzle.")]
    public AudioClip somVitoria;

    [Range(0f, 1f)]
    public float volumeSom = 1f;

    private int         contadorBotao2  = 0;
    private bool        puzzleResolvido = false;
    private AudioSource audioSource;

    // Posicao inicial da porta para detectar movimento
    private Vector3 posicaoInicialPorta;

    void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(x => Pressionei());

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;

        // Garante outline desativado no inicio
        if (outlinePorta != null)
            outlinePorta.OutlineWidth = 0f;

        // Guarda posicao inicial da porta
        if (portaCorrer != null)
            posicaoInicialPorta = portaCorrer.transform.position;
    }

    void Update()
    {
        // Remove outline quando a porta comecar a se mover
        if (puzzleResolvido && outlinePorta != null && outlinePorta.OutlineWidth > 0f)
        {
            if (portaCorrer != null)
            {
                float distancia = Vector3.Distance(
                    portaCorrer.transform.position,
                    posicaoInicialPorta
                );

                if (distancia > 0.05f)
                {
                    outlinePorta.OutlineWidth = 0f;
                    Debug.Log("[BotaoPressionado] Porta moveu — outline removido.");
                }
            }
        }
    }

    public void Pressionei()
    {
        print("PRESS " + numBotao);

        if (numBotao == 2)
        {
            contadorBotao2++;
            print("Botao 2 apertado: " + contadorBotao2 + " vezes");

            if (contadorBotao2 >= 3 && !puzzleResolvido)
            {
                puzzleResolvido = true;

                // Toca som de vitoria
                if (audioSource != null && somVitoria != null)
                    audioSource.PlayOneShot(somVitoria, volumeSom);

                // Ativa outline na porta
                if (outlinePorta != null)
                    outlinePorta.OutlineWidth = outlineWidth;

                // Destrava a porta
                portaCorrer.Destravar();

                Debug.Log("[BotaoPressionado] Puzzle resolvido! Porta liberada.");
            }
        }
    }
}