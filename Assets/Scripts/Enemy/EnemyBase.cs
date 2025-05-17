using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Classe base astratta per tutti i nemici, gestisce salute, invulnerabilità (VINCENZO)
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;
    protected bool isInvulnerable = false;

    [Header("Physics")]
    [SerializeField] protected float enemyWeight  = 5f;
    protected Vector3 knockbackVelocity;
    [SerializeField] protected float knockbackDecay = 5f;

    [Header("Feedback")]
    [SerializeField] private float invulnerabilityDuration = 1f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // metodo da ovveridare in copycat 
    public virtual void TakeDamage(float damage)
    {
        Debug.Log($"[EnemyBase] TakeDamage invocato con {damage}");

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
        Debug.Log($"[EnemyBase] FLASH START su {name}");
        isInvulnerable = true;

        // prendi tutti i renderer
        var rends = GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[EnemyBase] trovati {rends.Length} renderer per {name}");

        foreach (var r in rends)
            Debug.Log($"[EnemyBase] renderer: {r.gameObject.name} ({r.GetType().Name})");

        
        int flashes = 4;
        float step = invulnerabilityDuration / (flashes * 2);

        for (int i = 0; i < flashes; i++)
        {
            foreach (var r in rends) r.enabled = false;
            yield return new WaitForSeconds(step);
            foreach (var r in rends) r.enabled = true;
            yield return new WaitForSeconds(step);
        }

        isInvulnerable = false;
        Debug.Log($"[EnemyBase] FLASH END su {name}");
    }


    public float GetCurrentHealth() => currentHealth;
    
    public float GetWeight() => enemyWeight;
}