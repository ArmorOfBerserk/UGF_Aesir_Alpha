using UnityEngine;

public class Attack : MonoBehaviour
{
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

            Vector3 knockDir = (enemy.transform.position - transform.position).normalized;
            float force    = damage * 3f / enemy.GetWeight();
            Debug.Log($"[Attack] Knockback force = {force}");
            enemy.ApplyKnockback(knockDir * force);

            Destroy(gameObject);
        }
    }
}