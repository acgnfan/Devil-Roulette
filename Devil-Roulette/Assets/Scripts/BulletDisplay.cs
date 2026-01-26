// ################### 
// UPDATE: work with the new ChamberShell class
// ################### 

using UnityEngine;
using UnityEngine.UI;

public class BulletDisplay : MonoBehaviour
{
    [Header("References")]
    public GameState gameState;
    
    [Header("UI Elements")]
    public Text bulletCountText;
    public Text chamberStatusText;
    public Text currentChamberText;
    public GameObject[] chamberIndicators; // 8 UI elements for chambers
    
    void Update()
    {
        if (gameState == null) return;
        
        // Update bullet count
        int remaining = gameState.GetRemainingBullets();
        int total = gameState.totalBulletsLoaded;
        bulletCountText.text = $"Bullets: {remaining}/{total}";
        bulletCountText.color = remaining <= 2 ? Color.red : Color.white;
        
        // Update chamber status
        if (gameState.NeedToReload())
            chamberStatusText.text = "🔴 EMPTY - Need Reload";
        else if (remaining <= 2)
            chamberStatusText.text = "🟡 LOW AMMO";
        else
            chamberStatusText.text = "🟢 READY";
        
        // Update current chamber info
        if (gameState.currentChamberIndex < gameState.chamberShells.Count)
        {
            var currentShell = gameState.chamberShells[gameState.currentChamberIndex];
            string shellType = currentShell.type switch
            {
                GameState.ShellType.Live => "LIVE",
                GameState.ShellType.Blank => "BLANK",
                _ => "EMPTY"
            };
            
            string firedStatus = currentShell.fired ? " (FIRED)" : "";
            currentChamberText.text = $"Current: Chamber {gameState.currentChamberIndex}\n{shellType}{firedStatus}";
        }
        
        // Update chamber indicators (optional)
        if (chamberIndicators != null && chamberIndicators.Length >= 8)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i >= gameState.chamberShells.Count)
                {
                    chamberIndicators[i].GetComponent<Image>().color = Color.black;
                    continue;
                }
                
                var shell = gameState.chamberShells[i];
                
                // Choose color based on shell type and status
                if (shell.fired)
                {
                    // Already fired - gray
                    chamberIndicators[i].GetComponent<Image>().color = Color.gray;
                }
                else if (shell.type == GameState.ShellType.Live)
                {
                    // Live bullet - red
                    chamberIndicators[i].GetComponent<Image>().color = Color.red;
                }
                else if (shell.type == GameState.ShellType.Blank)
                {
                    // Blank bullet - yellow
                    chamberIndicators[i].GetComponent<Image>().color = Color.yellow;
                }
                else
                {
                    // Empty chamber - black
                    chamberIndicators[i].GetComponent<Image>().color = Color.black;
                }
                
                // Highlight current chamber with a border or different alpha
                if (i == gameState.currentChamberIndex)
                {
                    // Make current chamber brighter or add outline
                    var color = chamberIndicators[i].GetComponent<Image>().color;
                    color.a = 1.0f; // Full opacity
                    chamberIndicators[i].GetComponent<Image>().color = color;
                    
                    // // Optional: Add outline component
                    // if (chamberIndicators[i].GetComponent<Outline>() == null)
                    // {
                    //     var outline = chamberIndicators[i].AddComponent<Outline>();
                    //     outline.effectColor = Color.white;
                    //     outline.effectDistance = new Vector2(2, 2);
                    // }
                }
                else
                {
                    // Dim other chambers slightly
                    var color = chamberIndicators[i].GetComponent<Image>().color;
                    color.a = 0.7f; // Slightly transparent
                    chamberIndicators[i].GetComponent<Image>().color = color;
                    
                    // Remove outline if exists
                    var outline = chamberIndicators[i].GetComponent<Outline>();
                    if (outline != null) Destroy(outline);
                }
            }
        }
    }
    
    // Optional: Add labels to chambers
    void AddChamberLabels()
    {
        if (chamberIndicators == null) return;
        
        for (int i = 0; i < chamberIndicators.Length; i++)
        {
            // Create or find label text
            Text label = chamberIndicators[i].GetComponentInChildren<Text>();
            if (label == null)
            {
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(chamberIndicators[i].transform);
                label = labelObj.AddComponent<Text>();
                label.rectTransform.anchoredPosition = Vector2.zero;
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = 12;
                label.color = Color.white;
            }
            
            label.text = $"Ch{i}";
        }
    }
}