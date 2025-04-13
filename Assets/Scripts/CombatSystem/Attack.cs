using UnityEngine;

public class Attack : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 0.5f;
    private float damage = 10f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Distrugge l'oggetto dopo 'lifetime'
        Destroy(gameObject, lifetime);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Spostamento dell'attacco
        transform.position += transform.right * speed * Time.deltaTime;
    }

    public void SetDamage(float attackPower)
    {
        damage = attackPower;

        // Se vuoi un gradiente di colore tra bianco (attacco debole) e rosso (attacco forte):
        if (spriteRenderer != null)
        {
            // Normalizzo un po' per far vedere bene il rosso su potenze alte:
            float t = attackPower / 200f; 
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, t);
        }

        // Se invece vuoi un colore BINARIO (bianco fisso = attacco normale, rosso fisso = caricato),
        // puoi fare ad esempio:
        /*
        if (attackPower <= 10f)
            spriteRenderer.color = Color.white; // attacco base
        else
            spriteRenderer.color = Color.red;   // attacco caricato
        */
    }


    // Sarebbe preferibile evitare di distruggere i particellari per avere delle buone prestazioni
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Implementa il danno al nemico
            Debug.Log("Colpito nemico con danno: " + damage);
            Destroy(gameObject);
        }
    }
}
