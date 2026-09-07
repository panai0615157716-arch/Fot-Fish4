using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("ฐานข้อมูลปลาทั้งหมดในเกม")]
    public List<ItemData> allFishDatabase = new List<ItemData>();

    [Header("ข้อมูลในกระเป๋า (นับจำนวน)")]
    public Dictionary<ItemData, int> inventoryItems = new Dictionary<ItemData, int>();

    [Header("UI References")]
    public GameObject inventoryUIPanel;
    public Transform inventoryGrid;
    public GameObject itemSlotPrefab;

    // 1. เพิ่มตัวแปรสำหรับรับค่า Icon กระเป๋า
    [Header("Button References")]
    public GameObject bagIcon;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (inventoryUIPanel != null) inventoryUIPanel.SetActive(false);
        // สั่งวาด UI ตั้งแต่เริ่มเกม เพื่อให้โชว์เงาปลารอไว้เลย
        UpdateInventoryUI();
    }

    public void ToggleInventory()
    {
        bool isActive = inventoryUIPanel.activeSelf;
        inventoryUIPanel.SetActive(!isActive);

        // 2. ซ่อน/แสดง icon กระเป๋า 
        if (bagIcon != null)
        {
            bagIcon.SetActive(isActive);
        }
    }

    public void AddItem(ItemData newItem)
    {
        if (inventoryItems.ContainsKey(newItem))
        {
            inventoryItems[newItem]++;
        }
        else
        {
            inventoryItems.Add(newItem, 1);
        }
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        // วนลูปจากฐานข้อมูลปลา 'ทั้งหมด' ที่มีในเกม
        foreach (ItemData fish in allFishDatabase)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, inventoryGrid);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();

            if (slotScript != null)
            {
                // เช็กว่าในกระเป๋าเรา มีปลาชนิดนี้อยู่จริงไหม และมีจำนวนมากกว่า 0 หรือเปล่า
                bool hasFish = inventoryItems.ContainsKey(fish) && inventoryItems[fish] > 0;

                // ดึงจำนวนปลาออกมาส่งให้ UI (ถ้าไม่มีให้เป็น 0)
                int amount = hasFish ? inventoryItems[fish] : 0;

                // ส่งข้อมูลไปให้ Slot พร้อมสถานะ hasFish (ปลดล็อกแล้วหรือยัง)
                slotScript.SetupSlot(fish.itemIcon, amount, hasFish);
            }
        }
    }
}