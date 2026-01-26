


// ########
// UPDATE: Consecutive bullet placement
// ######## 

using UnityEngine;
using System.Collections.Generic;

public class BulletSystem : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] int minTotalBullets = 3;
    [SerializeField] int maxTotalBullets = 6;
    [SerializeField] float liveBulletRatio = 0.6f; // 60% of bullets are live
    
    [Header("Difficulty")]
    [SerializeField] bool adaptiveDifficulty = true;
    
    // Generate bullets with SOME LIVE, SOME BLANK - CONSECUTIVE placement
    public (int totalBullets, int liveCount, int[] livePositions, int[] blankPositions) GenerateBullets(
        int roundNumber, float playerWinRate, bool playerWonLast)
    {
        // 1. Calculate TOTAL bullets to load (n ≤ 8)
        int totalBullets = CalculateTotalBullets(roundNumber, playerWinRate, playerWonLast);
        
        // 2. Calculate how many are LIVE (rest are blank)
        int liveCount = Mathf.RoundToInt(totalBullets * liveBulletRatio);
        liveCount = Mathf.Clamp(liveCount, 1, totalBullets - 1); // At least 1 live, 1 blank
        
        int blankCount = totalBullets - liveCount;
        
        Debug.Log($"Loading {totalBullets} bullets: {liveCount} LIVE, {blankCount} BLANK (consecutive placement)");
        
        // 3. Pick CONSECUTIVE positions starting from chamber 0
        // All bullets (live + blank) occupy positions 0 through (totalBullets-1)
        int[] allBulletPositions = new int[totalBullets];
        for (int i = 0; i < totalBullets; i++)
        {
            allBulletPositions[i] = i;
        }
        
        // 4. Shuffle the bullet types within the consecutive positions
        List<int> shuffledIndices = new List<int>(allBulletPositions);
        shuffledIndices = Shuffle(shuffledIndices);
        
        // 5. Assign live bullets to random positions within the consecutive range
        int[] livePositions = new int[liveCount];
        for (int i = 0; i < liveCount; i++)
        {
            livePositions[i] = shuffledIndices[i];
        }
        
        // 6. Assign blank bullets to remaining positions
        int[] blankPositions = new int[blankCount];
        for (int i = 0; i < blankCount; i++)
        {
            blankPositions[i] = shuffledIndices[liveCount + i];
        }
        
        // Sort for clarity
        System.Array.Sort(livePositions);
        System.Array.Sort(blankPositions);
        
        return (totalBullets, liveCount, livePositions, blankPositions);
    }
    
    int CalculateTotalBullets(int roundNumber, float playerWinRate, bool playerWonLast)
    {
        // Base: random between min and max
        int bullets = Random.Range(minTotalBullets, maxTotalBullets + 1);
        
        // Adjust based on round
        bullets += Mathf.FloorToInt(roundNumber / 2f);
        
        if (adaptiveDifficulty)
        {
            if (playerWinRate > 0.5f) bullets++;
            if (playerWinRate < 0.3f) bullets--;
        }
        
        // Keep between minTotalBullets and 8
        return Mathf.Clamp(bullets, minTotalBullets, 8);
    }
    
    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }

    // 
    // 
    // 
    void CreateChamberVisualization(int totalBullets, int[] livePositions, int[] blankPositions)
    {
        // Create array to track bullet types
        string[] bulletTypes = new string[totalBullets];
        
        // Initialize all as empty
        for (int i = 0; i < totalBullets; i++)
            bulletTypes[i] = "?";
        
        // Mark live bullets
        foreach (int pos in livePositions)
        {
            if (pos < totalBullets) bulletTypes[pos] = "L";
        }
        
        // Mark blank bullets
        foreach (int pos in blankPositions)
        {
            if (pos < totalBullets) bulletTypes[pos] = "B";
        }
        
        // Build simple visual
        string positionLine = "Position: ";
        string typeLine = "Type:     ";
        
        for (int i = 0; i < totalBullets; i++)
        {
            positionLine += $"{i} ";
            typeLine += $"{bulletTypes[i]} ";
        }
        
        Debug.Log($"\n🔫 CHAMBER LAYOUT:");
        Debug.Log(positionLine);
        Debug.Log(typeLine);
        Debug.Log($"Total: {totalBullets} bullets ({livePositions.Length} live, {blankPositions.Length} blank)");
    }
}
