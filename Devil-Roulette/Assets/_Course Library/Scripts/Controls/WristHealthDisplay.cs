using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WristHealthDisplay : MonoBehaviour
{
    [Header("数据源绑定")]
    public GameState gameState; // <--- 1. 这里用来拖入你的 GameState 脚本
    
    [Header("UI 配置")]
    public GameObject iconPrefab;       
    public Transform playerIconContainer; 
    public Transform enemyIconContainer;
    
    // 我们假设最大血量是固定的，或者你可以从 GameState 获取
    public int maxPlayerHealth = 5; 
    public int maxDealerHealth = 5;

    private List<GameObject> playerIcons = new List<GameObject>();
    private List<GameObject> enemyIcons = new List<GameObject>();

    // 用来记录上一帧的血量，用于对比是否发生变化
    private int lastKnownPlayerHP = -1;
    private int lastKnownDealerHP = -1;

    private void Start()
    {
        // 初始化生成所有图标
        GenerateIcons(playerIconContainer, playerIcons, maxPlayerHealth);
        GenerateIcons(enemyIconContainer, enemyIcons, maxDealerHealth);

        // 如果没有绑定 GameState，报个错提醒自己
        if (gameState == null)
        {
            Debug.LogError("请在 Inspector 中将挂载 GameState 的物体拖入 WristHealthDisplay 的 GameState 插槽！");
        }
    }

    private void Update()
    {
        // <--- 2. 在这里实时监听变化
        if (gameState != null)
        {
            CheckAndRefreshUI();
        }
    }

    // 检查数据是否改变，只有改变了才刷新 UI (节省性能)
    private void CheckAndRefreshUI()
    {
        // --- 检查玩家血量 ---
        if (gameState.playerHP != lastKnownPlayerHP)
        {
            UpdatePlayerHealth(gameState.playerHP);
            lastKnownPlayerHP = gameState.playerHP; // 更新记录
        }

        // --- 检查电脑/庄家血量 ---
        if (gameState.dealerHP != lastKnownDealerHP)
        {
            UpdateEnemyHealth(gameState.dealerHP);
            lastKnownDealerHP = gameState.dealerHP; // 更新记录
        }
    }

    // --- 下面是之前的逻辑，不用动 ---

    private void GenerateIcons(Transform container, List<GameObject> list, int count)
    {
        foreach (Transform child in container) Destroy(child.gameObject);
        list.Clear();
        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(iconPrefab, container);
            icon.SetActive(true);
            list.Add(icon);
        }
    }

    public void UpdatePlayerHealth(int currentHealth)
    {
        UpdateIconState(playerIcons, currentHealth);
    }

    public void UpdateEnemyHealth(int currentHealth)
    {
        UpdateIconState(enemyIcons, currentHealth);
    }

    private void UpdateIconState(List<GameObject> icons, int currentVal)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetActive(i < currentVal);
        }
    }
}