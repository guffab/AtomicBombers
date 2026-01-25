using UnityEngine;

public class GameMusicPlayer : MonoBehaviour
{
    public static GameMusicPlayer Instance {get; private set;}
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.GetComponent<AudioSource>().mute = !Setup.MusicOn;
            Destroy(this.gameObject);
        }
        else 
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        GetComponent<AudioSource>().mute = !Setup.MusicOn;
    }
}
