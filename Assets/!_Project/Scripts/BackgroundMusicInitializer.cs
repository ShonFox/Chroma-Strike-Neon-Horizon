using UnityEngine;

public class BackgroundMusicInitializer : MonoBehaviour
{
    [SerializeField] private GameObject backgroundMusicPrefab;

    private void Awake()
    {
        if (GameObject.Find("BackgroundMusic") == null)
        {
            GameObject instance = Instantiate(backgroundMusicPrefab);
            instance.name = "BackgroundMusic";
            DontDestroyOnLoad(instance);
        }
    }
}
