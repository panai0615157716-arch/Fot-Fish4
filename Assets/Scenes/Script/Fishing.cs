using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

public class Fish : MonoBehaviour
{

    public enum State { Idle, WaitingForBite, Running, Result}
    [Header("References")]
    public RectTransform Area;
    public RectTransform Mark;
    public RectTransform Zone;
    public TMP_Text Text; 
    public InputActionReference stopAction; 

    [Header("Gameplay")]
    public float speed = 1.5f;
    public Vector2 biteWaitRange = new Vector2(0.75f, 1.75f);
    public bool randomizeZone = true;
    public Vector2 zoneSizeRange = new Vector2(0.18f, 0.32f);
    public Vector2 zoneCenterCalmp = new Vector2(0.15f, 0.85f); 
    public bool autoStartOnEnable = true;

    [Header("Events")]
    public UnityEvent onCatch;
    public UnityEvent onMiss;
    private State state = State.Idle;
    private float t;
    private int dir = 1;
    private float biteTimer;
    private void OnEnable()
    {
        if (stopAction != null && stopAction.action != null)
        {
            stopAction.action.performed += OnStopPerformed;

            stopAction.action.Enable();
        }
        if (autoStartOnEnable) StartFishing();
    }

    private void OnDisable()
    {
        if (stopAction != null && stopAction.action != null)
        {
            stopAction.action.performed -= OnStopPerformed;
            stopAction.action.Disable();
        }

    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingForBite:
                biteTimer -= Time.deltaTime;
                if (biteTimer <= 0f)
                {
                    state = State.Running;
                    if (Text) Text.text = "";
                }
                break;

            case State.Running:
                UpdateMarker();
                break;

        }

    }

    private void OnStopPerformed(InputAction.CallbackContext ctx)
    {
        if (state == State.Running)
            Evaluate();
    }

    public void StartFishing()
    {
        if (!ValidateRefs()) return;
        if (Text) Text.text = "";
        if (randomizeZone) RandomizeZone();
        t = Random.Range(0.05f, 0.95f);
        dir = Random.value < 0.5f ? 1 : -1;
        ApplyMarkerPosition();
        biteTimer = Random.Range(biteWaitRange.x, biteWaitRange.y);
        state = State.WaitingForBite;

    }

    public void CancelFishing()
    {
        state = State.Idle;
        if (Text) Text.text = "";
    }

    private void UpdateMarker()
    {
        t += dir * speed * Time.deltaTime;
        if (t >= 1f) { t = 1f; dir = -1; }
        else if (t < 0f) { t = 0f; dir = 1; }
        ApplyMarkerPosition();

    }

    private void ApplyMarkerPosition()
    {
        if (!Area || !Mark) return;
        float y = Mathf.Lerp(GetTrackBottom(), GetTrackTop(), t);
        var pos = Mark.anchoredPosition;
        pos.y = y;
        Mark.anchoredPosition = pos;
    }

    private void Evaluate()
    {
        state = State.Result;
        bool success = IsMarkerInsideZone();
        if (success)
        {
            if (Text) Text.text = "Fish Caught";
            onCatch?.Invoke();
        }
        else
        {
            if (Text) Text.text = "The fish got away!";
            onMiss?.Invoke();
        }

    }

    private bool IsMarkerInsideZone()
    {
        if (!Mark || !Zone) return false;
        float markerY = Mark.anchoredPosition.y;
        float zoneHalf = Zone.rect.height * 0.5f;
        float zoneCenter = Zone.anchoredPosition.y;
        float zoneMin = zoneCenter - zoneHalf;
        float zoneMax = zoneCenter + zoneHalf;
        return markerY >= zoneMin && markerY <= zoneMax;

    }

    private void RandomizeZone()
    {
        if (!Area || !Zone) return;
        float trackH = Area.rect.height;
        float zoneFrac = Random.Range(zoneSizeRange.x, zoneSizeRange.y);
        float zoneH = Mathf.Clamp(zoneFrac, 0.05f, 0.9f) * trackH;
        float minCenter = Mathf.Lerp(GetTrackBottom(), GetTrackTop(), zoneCenterCalmp.x);
        float maxCenter = Mathf.Lerp(GetTrackBottom(), GetTrackTop(), zoneCenterCalmp.y);
        float centerY = Random.Range(minCenter, maxCenter);
        var size = Zone.sizeDelta; 
        size.y = zoneH; 
        Zone.sizeDelta = size;
        var pos = Zone.anchoredPosition;
        pos.y = Mathf.Clamp(centerY, GetTrackBottom() + zoneH * 0.5f, GetTrackTop() - zoneH * 0.5f);
        Zone.anchoredPosition = pos;

    }

    private float GetTrackBottom() => -Area.rect.height * 0.5f;
    private float GetTrackTop() => Area.rect.height * 0.5f;
    private bool ValidateRefs()

    {
        if (!Area || !Mark || !Zone)
        {
            Debug.LogError("[FishingMinigame_Input] Missing reference. Assign Area, Mark and Zone.");
            return false;
        }
        return true;
    }

    public void PressStop() => OnStopPerformed(default);
    public void Retry() => StartFishing();

}
 