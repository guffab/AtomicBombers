using System.Collections;
using UnityEngine;

public class BombRestoreProxy : MonoBehaviour
{
    internal Player player;

    void Start()
    {
        StartCoroutine(ExecuteAfterWait(.5f));
    }

    IEnumerator ExecuteAfterWait(float duration)
    {
        yield return new WaitForSeconds(duration);
        player.availableBombs++;
    }
}
