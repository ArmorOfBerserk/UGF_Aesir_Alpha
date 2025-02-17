using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public Transform target; //Con target intendiamo quello che sarà il player

    public Vector3 offset;//Posizione iniziale telecamera

    public bool useOffesetValue; //Per scegliere se usare o meno gli offsetValue

    public float rotateSpeed; //velocità con cui ruota la telecamera

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!useOffesetValue){ //Se la spunta è su false quindi non c'è l'if è valido e andiamo a creare un offset automatico, altrimenti andiamo a usare l'offest di default che applichiamo manualmente

            offset = target.position - transform.position; //La camera partirà si sposterà dall'inziale 0 0 0 per mettersi in una posizione vicina al personaggio in qualsiasi punto esso si trovi
        
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //Prende la posizione X del mouse e ruotare il target
        float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
        target.Rotate(0, horizontal, 0); //Usiamo in realtà la X del mouse per andare a ruotare sull'asse delle Y il target così che vada a girare su se stesso

        float vertical= Input.GetAxis("Mouse Y") * rotateSpeed;
        target.Rotate(-vertical, 0,0); //Usiamo in realtà la X del mouse per andare a ruotare sull'asse delle X il target così che vada a guardare verso l'alto o il basso



        //Muoviamo la camera in base all'attuale rotazione del target e l'offset originale
        float desiredYAngle= target.eulerAngles.y;
        float desiredXAngle= target.eulerAngles.x;

        Quaternion rotation = Quaternion.Euler(desiredXAngle, desiredYAngle,0);
        transform.position=target.position - (rotation*offset); //la camerà aggiorna la sua posizione seguendo il personaggio da una certa distanza

        //transform.position= target.position - offset; 

        transform.LookAt(target);  // Qui andiamo a definire chi è l'obiettivo della camera che può essere modificato da unity
    }
}
