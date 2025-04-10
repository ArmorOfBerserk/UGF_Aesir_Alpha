using UnityEngine;

public class TEST_SAVES_LOADER : MonoBehaviour
{
    void Start()
    {
        GameData data = SaveSystem.LoadGameData();
        if (data != null)
        {
            transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
        }
    }
}
