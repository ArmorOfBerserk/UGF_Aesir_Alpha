using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GestoreDialoghi : MonoBehaviour
{
    public TextAsset fileDialoghiJSON;   
    public TMP_Text testoUI;            
    public RectTransform sfondoSinistra; // Sfondo verso sinistra se sono io a parlare (ID = 0)
    public RectTransform sfondoDestra;   // Sfondo verso a destra se parla un NPC (ID = 1)

    private Dictionary<int, string> dialoghiDict;

    void Start()
    {
        CaricaDialoghi();
    }

    void CaricaDialoghi()
    {
        Dialoghi datiDialoghi = JsonUtility.FromJson<Dialoghi>(fileDialoghiJSON.text); // parser json
        dialoghiDict = new Dictionary<int, string>();

        foreach (Dialogo d in datiDialoghi.dialoghi)
        {
            dialoghiDict[d.id] = d.testo;
        }
    }

    public void MostraTesto(int id)
    {
        if (dialoghiDict.ContainsKey(id))
        {
            testoUI.text = dialoghiDict[id];

            // Imposta lo sfondo a seconda dell'id
            if (id == 0)
            {
                sfondoSinistra.gameObject.SetActive(true);
                sfondoDestra.gameObject.SetActive(false);
            }
            else if (id == 1) 
            {
                sfondoSinistra.gameObject.SetActive(false);
                sfondoDestra.gameObject.SetActive(true);
            }
        }
        else
        {
            //
        }
    }
}