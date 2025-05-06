using UnityEngine;
using System.Collections;
public abstract class EnemyBase : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int maxEnergy = 50;
    protected int currentHealth;
    protected int currentEnergy;
    protected bool isInvulnerable = false;

    [Header("Feedback")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    private Color originalColor;
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isInvulnerable || currentHealth <= 0)
            return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        StartCoroutine(FlashSprite());

        if (currentHealth == 0)
            Die();
    }

    public virtual void UseEnergy(int amount)
    {
        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    public virtual void RestoreEnergy(int amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
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

    public int GetCurrentHealth() => currentHealth;
    //public int G;
}