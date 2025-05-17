using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 3f;   // Regola la forza base
    [SerializeField] private float maxKnockbackForce = 10f;    // Massimo limite

    public float speed = 10f;
    public float lifetime = 0.5f;
    private float damage = 10f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        Destroy(gameObject, lifetime);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    public void SetDamage(float attackPower)
    {
        damage = attackPower;

        if (spriteRenderer != null)
        {
            float t = attackPower / 200f;
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, t);
        }

        /*
        if (attackPower <= 10f)
            spriteRenderer.color = Color.white; // attacco base
        else
            spriteRenderer.color = Color.red;   // attacco caricato
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        // evita self‐damage
        if (other.GetComponent<PlayerCombatStats>() != null) return;

        var enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            Vector3 knockDir = transform.right;
            knockDir.y = 0f;
            knockDir.Normalize(); // direzione locale X del proiettile
            float rawForce = damage * knockbackMultiplier / enemy.GetWeight();
            float clampedForce = Mathf.Clamp(rawForce, 0f, maxKnockbackForce);
            Debug.Log($"[Attack] Knockback force = {clampedForce}");
            enemy.ApplyKnockback(knockDir * clampedForce);

            Destroy(gameObject);
        }
    }
    
    public float GetDamage() => damage;
}