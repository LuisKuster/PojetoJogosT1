using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BotaoPressionado : MonoBehaviour
{
    public int numBotao;
    public PortaCorrerController portaCorrer; // arrasta o Hanger aqui no Inspector

    private int contadorBotao2 = 0;

    void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(x => Pressionei());
    }

    public void Pressionei()
    {
        print("PRESS " + numBotao);

        if (numBotao == 2)
        {
            contadorBotao2++;
            print("Botao 2 apertado: " + contadorBotao2 + " vezes");

            if (contadorBotao2 >= 3)
            {
                portaCorrer.Destravar();
            }
        }
    }
}