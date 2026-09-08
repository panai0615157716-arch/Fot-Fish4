using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PondController : MonoBehaviour
{
    [Header("ลากรูปทุ่นมาใส่ตรงนี้")]
    public GameObject floatIcon;

    [Header("ลากตัวปุ่ม (Button) มาใส่ตรงนี้เพื่อซ่อน")]
    public GameObject buttonObject;

    void Start()
    {
        if (floatIcon != null)
        {
            floatIcon.SetActive(false);
        }
    }

    public void OnPondClicked()
    {
        // 1. ซ่อนปุ่มทันทีที่คลิก (ปุ่มจะหายไปเดี๋ยวนั้นเลย)
        if (buttonObject != null)
        {
            buttonObject.SetActive(false);
        }

        // 2. แสดงรูปทุ่น
        if (floatIcon != null)
        {
            floatIcon.SetActive(true);
        }

        // 3. เริ่มรันเวลาหน่วงก่อนเปลี่ยนซีน
        StartCoroutine(PondClickRoutine());
    }

    private IEnumerator PondClickRoutine()
    {

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(2);
    }
}