using UnityEngine;

public class GameMenu : MonoBehaviour
{
    // 把这个方法绑定到按钮上
    public void QuitGame()
    {
        // 1. 在编辑器模式下，让它停止运行（方便调试）
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 2. 在打包出来的游戏中，执行真正的退出
            Application.Quit();
        #endif

        Debug.Log("Game quitting！"); // 在控制台打印一条消息确认代码运行了
    }
}