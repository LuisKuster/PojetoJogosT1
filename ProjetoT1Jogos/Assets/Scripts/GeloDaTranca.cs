using System.Collections;
using UnityEngine;

public class GeloDaTranca : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PortaLock portaLock;

    [Header("Configuracoes")]
    [SerializeField] private float volumeParaDerretar = 1f;
    [SerializeField] private float tempoSumico        = 0.5f;

    [Header("Som")]
    [SerializeField] private AudioClip somDerretendo;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;
    private float volumeRecebido = 0f;
    private bool  jaDerreteu     = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;
    }

    public bool ReceberLiquido(float volume)
    {
        if (jaDerreteu) return true;

        volumeRecebido += volume;

        if (volumeRecebido >= volumeParaDerretar)
        {
            jaDerreteu = true;
            StartCoroutine(Derreter());
            return true;
        }

        return false;
    }

    IEnumerator Derreter()
    {
        if (audioSource != null && somDerretendo != null)
            audioSource.PlayOneShot(somDerretendo, volumeSom);

        Vector3 escalaInicial = transform.localScale;
        float   tempo         = 0f;

        while (tempo < tempoSumico)
        {
            tempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, tempo / tempoSumico);
            yield return null;
        }

        if (portaLock != null)
            portaLock.LiberarPorta();

        Destroy(gameObject);
    }
}