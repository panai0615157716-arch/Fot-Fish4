using UnityEngine;

using UnityEngine.SceneManagement; // จำเป็นต้องใช้สำหรับการโหลด Scene

public class GameOverManager : MonoBehaviour

{

    [Header("UI References")]

    public GameObject gameOverPanel; // ลาก UI Panel หน้า Game Over มาใส่ช่องนี้

    void Start()

    {

        // ซ่อนหน้า Game Over ไว้ก่อนตอนเริ่มเกม และให้เวลาเดินปกติ

        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

    }

    // ฟังก์ชันนี้จะถูกเรียกเมื่อผู้เล่นเลือดหมด หรือตรงเงื่อนไขจบเกม

    public void TriggerGameOver()

    {

        gameOverPanel.SetActive(true); // แสดงหน้าต่าง Game Over

        Time.timeScale = 0f; // หยุดเวลาในเกม (พวกฟิสิกส์และการเคลื่อนไหวจะหยุดลง)

    }

    // ฟังก์ชันสำหรับผูกกับปุ่ม "Restart"

    public void RestartGame()

    {

        Time.timeScale = 1f; // คืนค่าเวลากลับมาเป็นปกติก่อนโหลด Scene

        // โหลด Scene ปัจจุบันใหม่

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    // ฟังก์ชันสำหรับผูกกับปุ่ม "Main Menu"

    public void GoToMainMenu()

    {

        Time.timeScale = 1f; // คืนค่าเวลากลับมาเป็นปกติ

        // โหลดหน้า Main Menu (แก้ "MainMenu" เป็นชื่อ Scene ของคุณ)

        SceneManager.LoadScene("MainMenu");

    }
    
    

        public void PlayGame()
        {

            SceneManager.LoadScene(1);


        }


        public void QuitGame()
        {
            Debug.Log("Quit Game!");
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    
}
