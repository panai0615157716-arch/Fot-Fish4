using UnityEngine;

public class JumpScareManager : MonoBehaviour
{
    public GameObject scareCanvas;
    public AudioSource scareAudio;
    public float duration = 1.5f;

    public void TriggerJumpScare()
    {
        if (scareCanvas) scareCanvas.SetActive(true);
        if (scareAudio) scareAudio.Play();

        CancelInvoke(nameof(HideScare));
        Invoke(nameof(HideScare), duration);
    }

    void HideScare()
    {
        if (scareCanvas) scareCanvas.SetActive(false);
    }
}