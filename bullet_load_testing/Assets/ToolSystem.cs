using UnityEngine;
using System.Collections.Generic;

public class ToolSystem : MonoBehaviour
{

    public static ToolSystem Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    [Header("Tool Settings")]
    [SerializeField] int maxToolsPerRound = 8;
    
    // Tool definitions
    public enum ToolType 
    { 
        None,
        BurnerPhone,    // Reveals random shell polarity
        MagnifyingGlass, // Inspect current chamber
        Beer,           // Eject current shell to skip
        Pills,          // 40% heal, 60% damage
        HandSaw,        // Next shot deals 2x damage
        Adrenaline      // Steal and use dealer's tool
    }

    
    [System.Serializable]
    public class Tool
    {
        public ToolType type;
        public string name;
        public string description;
        public Sprite icon; // For UI
        
        public Tool(ToolType toolType)
        {
            type = toolType;
            switch (toolType)
            {
                case ToolType.BurnerPhone:
                    name = "Burner Phone";
                    description = "Reveals polarity of a random shell";
                    break;
                case ToolType.MagnifyingGlass:
                    name = "Magnifying Glass";
                    description = "Inspect current chamber";
                    break;
                case ToolType.Beer:
                    name = "Beer";
                    description = "Ejects current shell to skip chamber";
                    break;
                case ToolType.Pills:
                    name = "Painkillers";
                    description = "40% Heal 1 HP / 60% Lose 1 HP";
                    break;
                case ToolType.HandSaw:
                    name = "Hand Saw";
                    description = "Next shot deals Double Damage (2 HP)";
                    break;
                case ToolType.Adrenaline:
                    name = "Adrenaline Shot";
                    description = "Steal and use an item from the Dealer";
                    break;
            }
        }
    }
    
    // Get tools for specific round
    public List<Tool> GetToolsForRound(int roundNumber, bool isPlayer)
    {
        List<Tool> tools = new List<Tool>();
        
        switch (roundNumber)
        {
            case 1:
                // Round 1: No tools
                break;
                
            case 2:
                // Round 2: 2 random tools
                tools = GetRandomTools(2);
                break;
                
            case 3:
                // Round 3: 4 random tools
                tools = GetRandomTools(4);
                break;
        }
        
        Debug.Log($"Round {roundNumber}: Giving {tools.Count} tools to {(isPlayer ? "Player" : "Dealer")}");
        return tools;
    }
    
    // Get random unique tools
    public List<Tool> GetRandomTools(int count)
    {
        List<Tool> allToolTypes = new List<Tool>
        {
            new Tool(ToolType.BurnerPhone),
            new Tool(ToolType.MagnifyingGlass),
            new Tool(ToolType.Beer),
            new Tool(ToolType.Pills),
            new Tool(ToolType.HandSaw),
            new Tool(ToolType.Adrenaline)
        };
        
        // Shuffle and take 'count' tools
        allToolTypes = Shuffle(allToolTypes);
        
        List<Tool> selected = new List<Tool>();
        for (int i = 0; i < Mathf.Min(count, allToolTypes.Count); i++)
        {
            selected.Add(allToolTypes[i]);
        }
        
        return selected;
    }
    
    // Shuffle list
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
    
    // Check if player can use tool (has it in inventory)
    public bool CanUseTool(List<Tool> inventory, ToolType toolType)
    {
        foreach (Tool tool in inventory)
        {
            if (tool.type == toolType)
                return true;
        }
        return false;
    }
    
    // Remove tool from inventory (when used)
    public void RemoveTool(List<Tool> inventory, ToolType toolType)
    {
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].type == toolType)
            {
                inventory.RemoveAt(i);
                break; // Remove only one instance
            }
        }
    }
}

