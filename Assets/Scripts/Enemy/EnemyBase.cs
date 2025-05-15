using UnityEngine;
using System.Collections;

// Classe base astratta per tutti i nemici, gestisce salute, invulnerabilità (VINCENZO)
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]
    protected float maxHealth = 100f;
    protected float currentHealth;
    protected bool isInvulnerable = false;

    [Header("Physics")]
    [SerializeField] protected float enemyWeight  = 5f;
    protected Vector3 knockbackVelocity;
    [SerializeField] protected float knockbackDecay = 5f;

    [Header("Feedback")]
    [SerializeField]
    private float invulnerabilityDuration = 0.5f;

    [SerializeField]
    private SpriteRenderer spriteRenderer;   // Renderer per il flash visivo

    [SerializeField]
    private Color flashColor = Color.red;     // colore di flash quando subisce danno
    private Color originalColor;               // colore originale da ripristinare


    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color; // salva colore iniziale
    }


    // metodo da ovveridare in copycat 
    public virtual void TakeDamage(float damage)
    {
        // se è già invulnerabile o morto, ignora il danno
        if (isInvulnerable || currentHealth <= 0f)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0f);

        StartCoroutine(FlashSprite());

        if (currentHealth == 0f)
            Die();
    }

    public void ApplyKnockback(Vector3 velocity)
    {
        knockbackVelocity = velocity;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    private IEnumerator FlashSprite()
    {
        isInvulnerable = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(invulnerabilityDuration);

            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerable = false;
    }

    public float GetCurrentHealth() => currentHealth;
    
    public float GetWeight() => enemyWeight;
}