using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController : MonoBehaviour
{
    [SerializeField] private GameObject energyCover;
    [SerializeField] private Sprite[] coverSprites;

    private RectTransform coverTransform;
    private Image coverImage;

    [SerializeField] private float offset = 50f;

    private Vector3 initialPosition;
    private int currentEnergy = 5;
    private int imageIndex = 0;

    void Start()
    {
        coverTransform = energyCover.GetComponent<RectTransform>();
        coverImage = energyCover.GetComponent<Image>();

        initialPosition = coverTransform.localPosition;
        coverTransform.localPosition = new Vector3(initialPosition.x + offset * currentEnergy, initialPosition.y, initialPosition.z);

        StartCoroutine(ImageUpdate());
    }

    IEnumerator ImageUpdate()
    {
        while (true)
        {
            coverImage.sprite = coverSprites[imageIndex];
            imageIndex++;
            if (imageIndex >= coverSprites.Length)
            {
                imageIndex = 0;
            }
            yield return new WaitForSeconds(0.33f);
        }
    }

    public void UpdateEnergy(int energy)
    {
        currentEnergy = energy;
        coverTransform.localPosition = new Vector3(initialPosition.x + offset * currentEnergy, initialPosition.y, initialPosition.z);
    }


    //TEST
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            UpdateEnergy(currentEnergy - 1);
        } else if(Input.GetKeyDown(KeyCode.F2))
        {
            UpdateEnergy(currentEnergy + 1);
        }
    }
}
