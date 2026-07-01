using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    void Update()
    {
#if UNITY_EDITOR // 只在编辑器下生效
        
        // 按 F1 
        if (Input.GetKeyDown(KeyCode.F1)) {
        }

        // 按 F2 
        if (Input.GetKeyDown(KeyCode.F2)) {
        }

        // 按 F5 
        if (Input.GetKeyDown(KeyCode.F5)) {
        }

        // 按 F12 
        if (Input.GetKeyDown(KeyCode.F12)) {
        }

#endif
    }
}