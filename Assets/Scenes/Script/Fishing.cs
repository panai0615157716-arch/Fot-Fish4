using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct FishType
{
    public string name;
    [Header("Random Catch Goals")]
    public int minCatchCount;
    public int maxCatchCount;
    [HideInInspector]
    public int requiredCatchCount;
    [Header("Difficulty")]
    public float targetProgress;
    public float clickPower;
    public float decayRate;
    public float mashTimeLimit;
    [Header("Visual")]
    public GameObject fishVisual;
}

[System.Serializable]
public struct FishingRod
{
    public string rodName;
    public int unlockCost;
    public bool isUnlocked;

    [Header("Rod Stats Modifiers (1.0 = ปกติ)")]
    public float zoneSizeMultiplier;    // ทำให้โซนกว้างขึ้น (Phase 1)
    public float markerSpeedMultiplier; // ทำให้ตัววิ่งช้าลง (Phase 1)
    public float mashPowerMultiplier;   // เพิ่มแรงคลิก (Phase 2)
}

public class Fishing : MonoBehaviour
{
    public enum State { Idle, WaitingForBite, Running, MashingPhase, Result }

    [Header("Gameplay (Overall Goal)")]
    public float delayBetweenCatches = 2.0f;
    public TMP_Text fishCounterText;

    [Header("Rod System & Points")]
    public int currentPoints = 0;
    public int pointsPerCatch = 10;
    public TMP_Text pointsText;
    public TMP_Text rodInfoText;
    public FishingRod[] availableRods;
    private int equippedRodIndex = 0;

    [Header("References (Phase 1)")]
    public RectTransform Area;
    public RectTransform Mark;
    public RectTransform Zone;
    public TMP_Text Text;
    public InputActionReference stopAction;

    [Header("References (Phase 2)")]
    public Slider mashProgressBar;
    public TMP_Text mashText;

    [Header("Gameplay (Reflex - Phase 1)")]
    public Vector2 speedRange = new Vector2(1.0f, 3.5f);
    public Vector2 speedChangeInterval = new Vector2(0.3f, 1.2f);
    public Vector2 biteWaitRange = new Vector2(0.75F, 1.75F);
    public bool randomizeZone = true;
    public Vector2 zoneSizeRange = new Vector2(0.18F, 0.32F);
    public Vector2 zoneCenterCalmp = new Vector2(0.15F, 0.85F);
    public bool autoStartOnEnable = false;
    private float currentSpeed;
    private float speedTimer;

    [Header("Gameplay (Mashing - Phase 2)")]
    public bool progressiveTension = true;
    public FishType[] fishTypes;
    private FishType currentFish;
    private float currentMashProgress;
    private float currentMashTimer;
    private Dictionary<string, int> fishCaughtTracker = new Dictionary<string, int>();

    [Header("Events")]
    public UnityEvent onCatch;
    public UnityEvent onMiss;
    public UnityEvent<string> onCatchNamed;
    public UnityEvent onAllFishCaught;
    public UnityEvent onRodUpgraded;

    private State state = State.Idle;
    private float t;
    private int dir = 1;
    private float biteTimer;
    private float resultTimer;
    private bool clickedThisFrame = false;

    private void OnEnable()
    {
        if (stopAction != null && stopAction.action != null)
        {
            stopAction.action.performed += OnStopPerformed;
            stopAction.action.Enable();
        }

        UpdatePointsUI();
        UpdateRodUI();

        if (autoStartOnEnable) StartFishingSession();
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
        clickedThisFrame = false;
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
            case State.MashingPhase:
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    AddMashProgress();
                }

                float currentDecay = currentFish.decayRate;
                if (progressiveTension && currentFish.targetProgress > 0)
                {
                    float tensionMultiplier = (currentMashProgress / currentFish.targetProgress) * 0.8f;
                    currentDecay += (currentFish.decayRate * tensionMultiplier);
                }
                currentMashProgress -= currentDecay * Time.deltaTime;
                currentMashProgress = Mathf.Clamp(currentMashProgress, 0f, currentFish.targetProgress);

                if (mashProgressBar) mashProgressBar.value = currentMashProgress;

                if (currentMashProgress >= currentFish.targetProgress)
                {
                    EvaluateMashing(true);
                }
                else
                {
                    currentMashTimer -= Time.deltaTime;
                    if (currentMashTimer <= 0f || (currentMashTimer < currentFish.mashTimeLimit - 0.5f && currentMashProgress <= 0f))
                    {
                        EvaluateMashing(false);
                    }
                }
                break;
            case State.Result:
                resultTimer -= Time.deltaTime;
                if (resultTimer <= 0f)
                {
                    StartFishing();
                }
                break;
        }
    }

    private void OnStopPerformed(InputAction.CallbackContext ctx)
    {
        if (state == State.Running)
        {
            EvaluateReflex();
        }
        else if (state == State.MashingPhase)
        {
            AddMashProgress();
        }
    }

    private FishingRod GetCurrentRod()
    {
        if (availableRods == null || availableRods.Length == 0)
        {
            return new FishingRod { zoneSizeMultiplier = 1f, markerSpeedMultiplier = 1f, mashPowerMultiplier = 1f };
        }
        return availableRods[equippedRodIndex];
    }

    private void AddMashProgress()
    {
        if (clickedThisFrame) return;

        float rodBonus = GetCurrentRod().mashPowerMultiplier;
        currentMashProgress += (currentFish.clickPower * rodBonus);

        clickedThisFrame = true;
    }

    private void EvaluateReflex()
    {
        if (IsMarkerInsideZone())
        {
            StartMashingPhase();
        }
        else
        {
            state = State.Result;
            resultTimer = delayBetweenCatches;
            if (Area) Area.gameObject.SetActive(false);
            if (Mark) Mark.gameObject.SetActive(false);
            if (Zone) Zone.gameObject.SetActive(false);
            if (Text) { Text.gameObject.SetActive(true); Text.text = "Missed! Try again..."; }
            onMiss?.Invoke();
        }
    }

    private void StartMashingPhase()
    {
        state = State.MashingPhase;
        currentMashProgress = currentFish.targetProgress * 0.1f;
        currentMashTimer = currentFish.mashTimeLimit;
        clickedThisFrame = false;

        if (Area) Area.gameObject.SetActive(false);
        if (Mark) Mark.gameObject.SetActive(false);
        if (Zone) Zone.gameObject.SetActive(false);
        if (Text) { Text.gameObject.SetActive(true); Text.text = "FISH ON THE LINE!"; }
        if (mashText) { mashText.gameObject.SetActive(true); mashText.text = "KEEP CLICKING!"; }

        if (mashProgressBar)
        {
            mashProgressBar.gameObject.SetActive(true);
            mashProgressBar.minValue = 0;
            mashProgressBar.maxValue = currentFish.targetProgress;
            mashProgressBar.value = currentMashProgress;
        }
    }

    private void EvaluateMashing(bool mashSuccess)
    {
        if (mashSuccess)
        {
            currentPoints += pointsPerCatch;
            UpdatePointsUI();

            if (!fishCaughtTracker.ContainsKey(currentFish.name))
                fishCaughtTracker[currentFish.name] = 0;

            fishCaughtTracker[currentFish.name]++;
            UpdateFishCounterUI();

            if (mashText) mashText.gameObject.SetActive(false);
            if (mashProgressBar) mashProgressBar.gameObject.SetActive(false);

            if (currentFish.fishVisual)
            {
                currentFish.fishVisual.SetActive(true);
                Animator anim = currentFish.fishVisual.GetComponent<Animator>();
                if (anim != null) anim.SetTrigger("Catch");
            }

            onCatch?.Invoke();
            onCatchNamed?.Invoke(currentFish.name);

            if (IsAllFishGoalReached())
            {
                state = State.Idle;
                if (Text) { Text.gameObject.SetActive(true); Text.text = $"Caught a {currentFish.name}!\nAll Goals Reached!"; }
                onAllFishCaught?.Invoke();
            }
            else
            {
                state = State.Result;
                resultTimer = delayBetweenCatches;
                if (Text) { Text.gameObject.SetActive(true); Text.text = $"Caught a {currentFish.name}!\n+ {pointsPerCatch} Pts\nNext in {delayBetweenCatches}s..."; }
            }
        }
        else
        {
            state = State.Result;
            resultTimer = delayBetweenCatches;
            if (mashText) mashText.gameObject.SetActive(false);
            if (mashProgressBar) mashProgressBar.gameObject.SetActive(false);
            if (Text) { Text.gameObject.SetActive(true); Text.text = "The fish got away! Try again..."; }
            onMiss?.Invoke();
        }
    }

    public void UpgradeToNextRod()
    {
        if (availableRods == null || equippedRodIndex >= availableRods.Length - 1)
        {
            Debug.Log("ไม่มีเบ็ดให้อัปเกรดแล้ว");
            return;
        }

        int nextIndex = equippedRodIndex + 1;
        FishingRod nextRod = availableRods[nextIndex];

        if (!nextRod.isUnlocked)
        {
            if (currentPoints >= nextRod.unlockCost)
            {
                currentPoints -= nextRod.unlockCost;
                availableRods[nextIndex].isUnlocked = true;
                equippedRodIndex = nextIndex;

                UpdatePointsUI();
                UpdateRodUI();
                onRodUpgraded?.Invoke();
                Debug.Log($"ปลดล็อกเบ็ดใหม่สำเร็จ: {nextRod.rodName}");
            }
            else
            {
                Debug.Log("แต้มไม่พออัปเกรดเบ็ด!");
            }
        }
    }

    private void UpdatePointsUI()
    {
        // อัปเดต UI ให้เป็น "Score : [คะแนน]" ตรงตามภาพของคุณ
        if (pointsText != null) pointsText.text = $"Score : {currentPoints}";
    }

    private void UpdateRodUI()
    {
        if (rodInfoText != null && availableRods != null && availableRods.Length > 0)
        {
            rodInfoText.text = $"Rod: {availableRods[equippedRodIndex].rodName}";
        }
    }

    private bool IsAllFishGoalReached()
    {
        foreach (var fish in fishTypes)
        {
            int caught = fishCaughtTracker.ContainsKey(fish.name) ? fishCaughtTracker[fish.name] : 0;
            if (caught < fish.requiredCatchCount) return false;
        }
        return true;
    }

    public void StartFishingSession()
    {
        if (!ValidateRefs()) return;
        fishCaughtTracker.Clear();
        if (fishTypes != null)
        {
            for (int i = 0; i < fishTypes.Length; i++)
            {
                fishTypes[i].requiredCatchCount = Random.Range(fishTypes[i].minCatchCount, fishTypes[i].maxCatchCount + 1);
                fishCaughtTracker[fishTypes[i].name] = 0;
            }
        }

        if (availableRods != null && availableRods.Length > 0)
        {
            availableRods[0].isUnlocked = true;
        }

        UpdateFishCounterUI();
        StartFishing();
    }

    private void StartFishing()
    {
        currentFish = GetRandomFish();
        ResetUI();
        if (randomizeZone) RandomizeZone();
        t = Random.Range(0.05f, 0.95f);
        dir = Random.value < 0.5f ? 1 : -1;
        ApplyMarkerPosition();

        float rodSpeedMod = GetCurrentRod().markerSpeedMultiplier;
        currentSpeed = Random.Range(speedRange.x, speedRange.y) * rodSpeedMod;

        speedTimer = Random.Range(speedChangeInterval.x, speedChangeInterval.y);
        biteTimer = Random.Range(biteWaitRange.x, biteWaitRange.y);
        state = State.WaitingForBite;
    }

    private void UpdateFishCounterUI()
    {
        if (fishCounterText != null && fishTypes != null)
        {
            string status = "Goals:\n";
            foreach (var fish in fishTypes)
            {
                if (fish.requiredCatchCount <= 0) continue;
                int caught = fishCaughtTracker.ContainsKey(fish.name) ? fishCaughtTracker[fish.name] : 0;
                status += $"{fish.name}: {caught} / {fish.requiredCatchCount}\n";
            }
            fishCounterText.text = status;
        }
    }

    private FishType GetRandomFish()
    {
        if (fishTypes == null || fishTypes.Length == 0) return new FishType();
        List<FishType> availableFish = new List<FishType>();
        foreach (var fish in fishTypes)
        {
            int caught = fishCaughtTracker.ContainsKey(fish.name) ? fishCaughtTracker[fish.name] : 0;
            if (caught < fish.requiredCatchCount)
            {
                availableFish.Add(fish);
            }
        }
        if (availableFish.Count == 0) return fishTypes[Random.Range(0, fishTypes.Length)];
        return availableFish[Random.Range(0, availableFish.Count)];
    }

    private void ResetUI()
    {
        if (Text) { Text.gameObject.SetActive(true); Text.text = ""; }
        if (Area) Area.gameObject.SetActive(true);
        if (Mark) Mark.gameObject.SetActive(true);
        if (Zone) Zone.gameObject.SetActive(true);
        if (mashText) { mashText.text = ""; mashText.gameObject.SetActive(false); }
        if (mashProgressBar) { mashProgressBar.gameObject.SetActive(false); }
        if (fishTypes != null)
        {
            foreach (var fish in fishTypes)
            {
                if (fish.fishVisual) fish.fishVisual.SetActive(false);
            }
        }
    }

    private void HideAllUI()
    {
        if (Area) Area.gameObject.SetActive(false);
        if (Mark) Mark.gameObject.SetActive(false);
        if (Zone) Zone.gameObject.SetActive(false);
        if (Text) Text.gameObject.SetActive(false);
        if (mashProgressBar) mashProgressBar.gameObject.SetActive(false);
        if (mashText) mashText.gameObject.SetActive(false);
        if (fishTypes != null)
        {
            foreach (var fish in fishTypes)
            {
                if (fish.fishVisual) fish.fishVisual.SetActive(false);
            }
        }
    }

    public void CancelFishing()
    {
        state = State.Idle;
        HideAllUI();
    }

    private void UpdateMarker()
    {
        speedTimer -= Time.deltaTime;
        if (speedTimer <= 0f)
        {
            float rodSpeedMod = GetCurrentRod().markerSpeedMultiplier;
            currentSpeed = Random.Range(speedRange.x, speedRange.y) * rodSpeedMod;
            speedTimer = Random.Range(speedChangeInterval.x, speedChangeInterval.y);
        }
        t += dir * currentSpeed * Time.deltaTime;
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

        float rodZoneMod = GetCurrentRod().zoneSizeMultiplier;
        zoneFrac *= rodZoneMod;

        float zoneH = Mathf.Clamp(zoneFrac, 0.05f, 0.9f) * trackH;
        float minCenter = Mathf.Lerp(GetTrackBottom(), GetTrackTop(), zoneCenterCalmp.x);
        float maxCenter = Mathf.Lerp(GetTrackBottom(), GetTrackTop(), zoneCenterCalmp.y);
        float centerY = Random.Range(minCenter, maxCenter);

        var size = Zone.sizeDelta; size.y = zoneH; Zone.sizeDelta = size;
        var pos = Zone.anchoredPosition;
        pos.y = Mathf.Clamp(centerY, GetTrackBottom() + zoneH * 0.5f, GetTrackTop() - zoneH * 0.5f);
        Zone.anchoredPosition = pos;
    }

    private float GetTrackBottom() => -Area.rect.height * 0.5f;
    private float GetTrackTop() => Area.rect.height * 0.5f;

    private bool ValidateRefs()
    {
        if (!Area || !Mark || !Zone) return false;
        return true;
    }
}