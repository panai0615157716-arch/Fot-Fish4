using UnityEngine;
using TMPro; // ต้องมีบรรทัดนี้เพื่อใช้งาน TextMeshPro
public class MoneyManager : MonoBehaviour
{
    // สร้างเป็น Singleton เพื่อให้สคริปต์อื่นเรียกใช้ได้ง่ายๆ โดยไม่ต้อง FindObject
    public static MoneyManager instance;
    [Header("Economy Settings")]
    public int currentMoney = 0; // จำนวนเงินเริ่มต้น
    [Header("UI References")]
    public TextMeshProUGUI moneyText; // ลาก UI Text ที่จะแสดงเงินมาใส่ช่องนี้
    void Awake()
    {
        // ตั้งค่าตัวแปร instance ให้ชี้มาที่สคริปต์นี้
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        UpdateMoneyUI(); // อัปเดตตัวเลขบนหน้าจอตอนเริ่มเกม
    }
    // ฟังก์ชันสำหรับเพิ่มเงิน (เช่น ตอนเก็บเหรียญหรือจบด่าน)
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }
    // ฟังก์ชันสำหรับหักเงิน (เช่น ตอนซื้อของ)
    // ใช้แบบ bool เพื่อส่งค่ากลับไปบอกว่า "เงินพอจ่ายไหม?"
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            return true; // จ่ายเงินสำเร็จ
        }
        else
        {
            Debug.Log("เงินไม่พอ!");
            return false; // เงินไม่พอ จ่ายไม่สำเร็จ
        }
    }
    // ฟังก์ชันสำหรับรีเฟรชข้อความบน UI
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            // สามารถเติมคำนำหน้าได้ เช่น moneyText.text = "Money: " + currentMoney;
            moneyText.text = currentMoney.ToString();
        }
    }
}