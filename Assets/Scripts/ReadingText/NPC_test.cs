using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class NPCDialogoTrigger : MonoBehaviour
{
    public GameObject dialogoCanvas;
    public TextAsset fileDialoghiJSON;
    public int idDialogo = 1;
    public string npcID = "";
    public Button frecciaAvanti;
    public Button frecciaIndietro;

    private TMP_Text testoDialogo;
    private List<string> pagine = new List<string>();
    private int paginaCorrente = 0;
    private int maxCaratteri = 120;

    void Start()
    {
        testoDialogo = dialogoCanvas.GetComponentInChildren<TMP_Text>();
        dialogoCanvas.SetActive(false);

        frecciaAvanti.onClick.AddListener(PaginaSuccessiva);
        frecciaIndietro.onClick.AddListener(PaginaPrecedente);

        frecciaAvanti.gameObject.SetActive(false);
        frecciaIndietro.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //string testo = CaricaTestoPerID(idDialogo);
            string testo = CaricaTesto(idDialogo, npcID);
            DividiInPagine(testo);

            paginaCorrente = 0;
            MostraPagina(paginaCorrente);

            dialogoCanvas.SetActive(true);
            AggiornaFrecce();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogoCanvas.SetActive(false);
            frecciaAvanti.gameObject.SetActive(false);
            frecciaIndietro.gameObject.SetActive(false);
        }
    }

    /*
    string CaricaTestoPerID(int id)
    {
        if (fileDialoghiJSON == null) return "[Nessun file JSON caricato]";

        Dialoghi dati = JsonUtility.FromJson<Dialoghi>(fileDialoghiJSON.text);

        foreach (Dialogo d in dati.dialoghi)
        {
            if (d.id == id)
                return d.testo;
        }

        return "[Dialogo non trovato]";
    }*/
    
    private string CaricaTesto(int id, string npcName)
    {
        if (fileDialoghiJSON == null) return "[Nessun file JSON caricato]";

        Dialoghi dati = JsonUtility.FromJson<Dialoghi>(fileDialoghiJSON.text);

        foreach (Dialogo d in dati.dialoghi)
        {
            if (d.id == id && d.npc.ToLower() == npcName.ToLower())
                return d.testo;
        }

        return "[Dialogo non trovato]";
    }

    void DividiInPagine(string testo)
    {
        pagine.Clear();
        for (int i = 0; i < testo.Length; i += maxCaratteri)
        {
            int len = Mathf.Min(maxCaratteri, testo.Length - i);
            pagine.Add(testo.Substring(i, len));
        }
    }

    void MostraPagina(int index)
    {
        if (testoDialogo != null && index >= 0 && index < pagine.Count)
        {
            testoDialogo.text = pagine[index];
        }
    }

    void PaginaSuccessiva()
    {
        if (paginaCorrente < pagine.Count - 1)
        {
            paginaCorrente++;
            MostraPagina(paginaCorrente);
            AggiornaFrecce();
        }
    }

    void PaginaPrecedente()
    {
        if (paginaCorrente > 0)
        {
            paginaCorrente--;
            MostraPagina(paginaCorrente);
            AggiornaFrecce();
        }
    }

    void AggiornaFrecce()
    {
        frecciaIndietro.gameObject.SetActive(paginaCorrente > 0);
        frecciaAvanti.gameObject.SetActive(paginaCorrente < pagine.Count - 1);
    }
}
