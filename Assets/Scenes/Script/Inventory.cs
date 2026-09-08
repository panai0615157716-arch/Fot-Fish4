using UnityEngine;
using UnityEngine.SceneManagement;

public class inventory : MonoBehaviour
{
    // ฟังก์ชันนี้เอาไว้ผูกกับปุ่มกระเป๋า
    public void OpenInventoryScene()
    {

        SceneManager.LoadScene(3);
    }
}