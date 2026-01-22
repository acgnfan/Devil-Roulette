// ########
// UPDATE: Separate Live/Blank
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
    
    // Generate bullets with SOME LIVE, SOME BLANK
    public (int totalBullets, int liveCount, int[] livePositions, int[] blankPositions) GenerateBullets(
        int roundNumber, float playerWinRate, bool playerWonLast)
    {
        // 1. Calculate TOTAL bullets to load (n ≤ 8)
        int totalBullets = CalculateTotalBullets(roundNumber, playerWinRate, playerWonLast);
        
        // 2. Calculate how many are LIVE (rest are blank)
        int liveCount = Mathf.RoundToInt(totalBullets * liveBulletRatio);
        liveCount = Mathf.Clamp(liveCount, 1, totalBullets - 1); // At least 1 live, 1 blank
        
        int blankCount = totalBullets - liveCount;
        
        Debug.Log($"Loading {totalBullets} bullets: {liveCount} LIVE, {blankCount} BLANK");
        
        // 3. Pick RANDOM positions for live bullets
        int[] livePositions = PickRandomPositions(liveCount, 8);
        
        // 4. Pick RANDOM positions for blank bullets (different from live)
        int[] blankPositions = PickRandomPositions(blankCount, 8, livePositions);
        
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
    
    // Pick random unique positions
    int[] PickRandomPositions(int count, int maxPositions, int[] excludePositions = null)
    {
        List<int> allPositions = new List<int>();
        for (int i = 0; i < maxPositions; i++) allPositions.Add(i);
        
        // Remove positions we can't use (like already taken by live shells)
        if (excludePositions != null)
        {
            foreach (int excluded in excludePositions)
            {
                allPositions.Remove(excluded);
            }
        }
        
        // Shuffle
        allPositions = Shuffle(allPositions);
        
        // Take first 'count' positions
        int[] result = new int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = allPositions[i];
        }
        
        return result;
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
}
