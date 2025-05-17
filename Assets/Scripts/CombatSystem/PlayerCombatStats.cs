using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityDuration = 1f;

    [Header("Energy")]
    [SerializeField] private float maxEnergy = 50f;
    [SerializeField] private float energyRechargeRate = 10f;
    private float currentHealth;
    private float currentEnergy;
    private bool isInvulnerable = false;
    private void Awake()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
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

        var allRends = GetComponentsInChildren<Renderer>(true);
        var rends = new List<Renderer>();
        foreach (var r in allRends)
            if (r.gameObject.name != "adventurer-idle-00") // escludo adventurer-idle altrimenti ritorna visibile anche lui 
                rends.Add(r);

        int flashes = 4;
        float step = invulnerabilityDuration / (flashes * 2);

        for (int i = 0; i < flashes; i++)
        {
            // disattivo il render
            foreach (var r in rends) r.enabled = false;
            yield return new WaitForSeconds(step);

            // attivo il render 
            foreach (var r in rends) r.enabled = true;
            yield return new WaitForSeconds(step);
        }

        isInvulnerable = false;
    }
}