using UnityEngine;
using System.Collections;

public class PlayerCombatStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private float maxHealth = 100f;               
    [SerializeField]
    private float invulnerabilityDuration = 0.5f;
    [SerializeField]
    private SpriteRenderer spriteRenderer;      
    [SerializeField]
    private Color flashColor = Color.red;      
    
    [Header("Energy")]
    [SerializeField]
    private float maxEnergy = 50f;             
    [SerializeField]
    private float energyRechargeRate = 10f;    
    private float currentHealth;  
    private float currentEnergy;  
    private bool isInvulnerable = false;
    private Color originalColor;       

    void Awake()
    {
        currentHealth = maxHealth; 
        currentEnergy = maxEnergy;  
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color; 
    }

    void Update()
    {
        if (currentEnergy < maxEnergy)
            currentEnergy = Mathf.Min(
                currentEnergy + energyRechargeRate * Time.deltaTime,
                maxEnergy);
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || currentHealth <= 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        StartCoroutine(FlashSprite());

        Debug.Log($"[PlayerStats] Danno ricevuto: {damage}, Vita rimanente: {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    public bool UseEnergy(float amount)
    {
        if (currentEnergy < amount)
            return false; 

        currentEnergy -= amount;
        Debug.Log($"[PlayerStats] Energia spesa: {amount}, Energia rimanente: {currentEnergy}");
        return true;
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        Debug.Log($"[PlayerStats] Energia ripristinata: {amount}, Energia ora: {currentEnergy}");
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetCurrentEnergy() => currentEnergy;

    private void Die()
    {
        Debug.Log("[PlayerStats] Il player è morto!");
        // implementare respawn o game over
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
}