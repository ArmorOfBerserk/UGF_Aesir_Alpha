using System.Collections;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private RectTransform redBar;
    [SerializeField] private RectTransform blackBar;
    [SerializeField] private float blackBarSpeed = 5f;

    [SerializeField] private GameObject DeathScreen;

    int currentHealth = 10;
    public int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); UpdateHealthBar(); }
    }

    readonly int maxHealth = 10;
    readonly float offset = 32f;

    void Start()
    {
        CurrentHealth = maxHealth;
    }

    public void UpdateHealthBar()
    {
        StopAllCoroutines();
        redBar.localPosition = new Vector3(offset * currentHealth, redBar.localPosition.y, redBar.localPosition.z);
        StartCoroutine(AnimateHealthBar());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator AnimateHealthBar()
    {
        Vector3 targetPosition = new Vector3(offset * currentHealth, blackBar.localPosition.y, blackBar.localPosition.z);
        while (Vector3.Distance(blackBar.localPosition, targetPosition) > 0.01f)
        {
            blackBar.localPosition = Vector3.Lerp(blackBar.localPosition, targetPosition, Time.deltaTime * blackBarSpeed);
            yield return null;
        }
        blackBar.localPosition = targetPosition;
    }

    //TEST
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            currentHealth = Mathf.Clamp(currentHealth - 1, 0, maxHealth);
            UpdateHealthBar();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
            UpdateHealthBar();
        }
    }

    private void Die()
    {
        Time.timeScale = 0;
        DeathScreen.SetActive(true);
    }
}
