using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonGiveItem : MonoBehaviour
{
    [Header("ข้อมูลปลาที่จะได้รับเมื่อกดปุ่มนี้")]
    public ItemData itemToGive;

    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(GiveItem);
    }

    private void GiveItem()
    {
        if (itemToGive != null && InventoryManager.instance != null)
        {
            // สั่งเรียกใช้ฟังก์ชัน AddItem จากสมองหลัก (InventoryManager)
            InventoryManager.instance.AddItem(itemToGive);
            Debug.Log("ได้รับปลา: " + itemToGive.itemName);
        }
    }
}