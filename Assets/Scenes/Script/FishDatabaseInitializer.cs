using Unity.VisualScripting;
using UnityEngine;
public class FishDatabaseInitializer : MonoBehaviour
{
    // รายชื่อปลาทั้ง 20 ชนิดที่ชื่อตรงกับระบบ Fishing และ Inventory
    public static readonly string[] FishNames = new string[]
    {
       "Moonscale",        // 1. ปลามูนเกล็ด
       "Mossfin",          // 2. ปลามอสเขียว
       "Glass Eye",        // 3. ปลาตาแก้ว
       "Shadow Spine",     // 4. ปลาหนามเงา
       "Azure Moonfish",   // 5. ปลาจันทร์คราม
       "Moonfang",         // 6. ปลาฟันจันทร์
       "Branchfin",        // 7. ปลากิ่งไม้
       "Silvermist",       // 8. ปลาหมอกเงิน
       "Bloodeye",         // 9. ปลาตาแดง
       "Blackbone",        // 10. ปลากระดูกดำ
       "Phantomfin",       // 11. ปลาผีเสื้อเงา
       "Root Eater",       // 12. ปลากาฝาก
       "Eclipse Fish",     // 13. ปลาคราส
       "Thorn King",       // 14. ปลาราชันหนาม
       "Mist Dragonfish",  // 15. ปลามังกรหมอก
       "Lantern Maw",      // 16. ปลาตะเกียง
       "Darkroot Fish",    // 17. ปลารากมืด
       "Blood Moonfish",   // 18. ปลาจันทร์โลหิต
       "Hundred Eye Fish", // 19. ปลาร้อยตา
       "Abyssal Fish"      // 20. อสูรแห่งบ่อน้ำ
    };
}
