using UnityEngine;

public class GameMusicPlayer : MonoBehaviour
{
    public static GameMusicPlayer Instance {get; private set;}
    
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else 
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
