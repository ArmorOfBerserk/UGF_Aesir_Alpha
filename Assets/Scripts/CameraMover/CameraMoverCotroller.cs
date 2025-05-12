using UnityEngine;

public class CameraMoverController : MonoBehaviour
{
    [Header("Punto di partenza e arrivo")]
    public Transform puntoA; // da
    public Transform puntoB; // a

    [Header("Addizionali (riproduzione di un suono)")]
    public AudioClip moveSound_puntoA;
    public AudioClip moveSound_puntoB;

    [Header("Durata del movimento in secondi")]
    public float duration = 2f;

    // internals
    private float elapsedTime; // tempo impiegato per tenere traccia della posizione della camera
    private bool isMoving;
    private bool suonatoAlready;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void BeginMove()
    {
        if (puntoA == null || puntoB == null) return;
        if(moveSound_puntoA == null || moveSound_puntoB == null) return;

        transform.position = puntoA.position;
        elapsedTime = 0f; // camera ferma al puntoA
        suonatoAlready = false;
        isMoving = true;
        audioSource.PlayOneShot(moveSound_puntoA);
    }

    void Update()
    {
        if (!isMoving) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration); // se t è 1 allora elapsedTime è arrivato a duration quindi il movimento è completo 
        transform.position = Vector3.Lerp(puntoA.position, puntoB.position, t);

        if (t >= 1f && !suonatoAlready)
        {
            audioSource.PlayOneShot(moveSound_puntoB);
            suonatoAlready = true;
            isMoving = false;
        }
    }
}