using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Anexe no botao da sala final.
/// Ao pressionar:
///   1. Toca audio 1 + parede comeca a subir
///   2. Apos atrasoAudio2, toca audio 2
///   3. Apos atrasoCanvas segundos do botao, mostra canvas de vitoria
/// </summary>
public class SalaFinal : MonoBehaviour
{
    [Header("Parede")]
    [SerializeField] private Transform parede;
    [SerializeField] private float posYInicial      = 0f;
    [SerializeField] private float posYFinal        = 5f;
    [SerializeField] private float velocidadeSubida = 1f;

    [Header("Audios")]
    [SerializeField] private AudioClip audio1;
    [SerializeField] private AudioClip audio2;
    [SerializeField] private float atrasoAudio2 = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float volumeAudio  = 1f;

    [Header("Mensagem de vitoria")]
    [SerializeField] private GameObject canvasVitoria;
    [Tooltip("Segundos apos apertar o botao para o canvas aparecer.")]
    [SerializeField] private float atrasoCanvas = 10f;

    private AudioSource audioSource;
    private bool        iniciou = false;
    private bool        subindo = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume       = volumeAudio;

        if (canvasVitoria != null)
            canvasVitoria.SetActive(false);

        if (parede != null)
        {
            Vector3 pos = parede.position;
            pos.y = posYInicial;
            parede.position = pos;
        }

        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(_ => Pressionado());
    }

    void Update()
    {
        if (!subindo || parede == null) return;

        Vector3 pos = parede.position;

        if (pos.y < posYFinal)
        {
            pos.y += velocidadeSubida * Time.deltaTime;
            pos.y  = Mathf.Min(pos.y, posYFinal);
            parede.position = pos;
        }
        else
        {
            subindo = false;
        }
    }

    void Pressionado()
    {
        if (iniciou) return;
        iniciou = true;
        Debug.Log("[SalaFinal] Botao pressionado!");
        StartCoroutine(SequenciaFinal());
    }

    IEnumerator SequenciaFinal()
    {
        // Audio 1 + parede sobem juntos
        if (audio1 != null)
            audioSource.PlayOneShot(audio1, volumeAudio);

        subindo = true;

        // Audio 2 após atraso
        yield return new WaitForSeconds(atrasoAudio2);

        if (audio2 != null)
            audioSource.PlayOneShot(audio2, volumeAudio);

        // Canvas aparece atrasoCanvas segundos após o botão
        // Já passou atrasoAudio2 segundos, então espera o restante
        float tempoRestante = atrasoCanvas - atrasoAudio2;
        if (tempoRestante > 0f)
            yield return new WaitForSeconds(tempoRestante);

        if (canvasVitoria != null)
        {
            canvasVitoria.SetActive(true);
            Debug.Log("[SalaFinal] Canvas de vitoria exibido!");
        }
    }
}