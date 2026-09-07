using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI amountText;


    public void SetupSlot(Sprite icon, int amount, bool isUnlocked)
    {
        iconImage.sprite = icon;

        if (isUnlocked)
        {
            // ถ้าปลดล็อกแล้ว (เคยตกได้) ให้แสดงสีปกติ
            iconImage.color = Color.white;

            if (amount > 1)
            {
                amountText.text = amount.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
        else
        {
            // ถ้ายังไม่ปลดล็อก ให้เปลี่ยนรูปเป็นสีดำสนิท (ทำเป็นเงา)
            iconImage.color = Color.black;
            amountText.gameObject.SetActive(false); // ซ่อนตัวเลขทิ้ง
        }
    }
}