using UnityEngine;
using UnityEngine.EventSystems; // 必须引用：用于处理指针事件
using TMPro; // 如果你使用的是 TextMeshPro
using UnityEngine.UI; // 如果你使用的是旧版 Text

public class ButtonHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("设置")]
    public TextMeshProUGUI textComponent; // 如果是旧版Text，改为 public Text textComponent;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private void Start()
    {
        // 游戏开始时确保颜色是普通颜色
        if (textComponent != null)
        {
            textComponent.color = normalColor;
        }
    }

    // 当射线（或鼠标）进入按钮区域时调用
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textComponent != null)
        {
            textComponent.color = hoverColor;
        }
    }

    // 当射线（或鼠标）离开按钮区域时调用
    public void OnPointerExit(PointerEventData eventData)
    {
        if (textComponent != null)
        {
            textComponent.color = normalColor;
        }
    }
}