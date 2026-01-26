// using UnityEngine;
// using UnityEngine.UI;

// [System.Serializable]
// public class ToolButtonSlot
// {
//     public RectTransform slotTransform;  // Where button appears
//     public ToolSystem.ToolType assignedTool; // Which tool goes here
//     public Button buttonInstance;        // Created button (null if no tool)
//     public bool isOccupied = false;      // Is this slot in use?
// }

// public class ToolButtonManager : MonoBehaviour
// {
//     [Header("Button Slots")]
//     public ToolButtonSlot[] buttonSlots; // Define these in Inspector
    
//     [Header("Button Prefab")]
//     public GameObject toolButtonPrefab;
    
//     [Header("References")]
//     public GameState gameState;
//     public GameFlowController gameFlow;
    
//     // Update button visibility based on player's tools
//     public void UpdateToolButtons()
//     {
//         // First, hide/clear all slots
//         ClearAllSlots();
        
//         // For each tool the player has, find a matching slot
//         foreach (var tool in gameState.playerTools)
//         {
//             // Find a slot assigned to this tool type
//             ToolButtonSlot slot = FindSlotForTool(tool.type);
            
//             if (slot != null && !slot.isOccupied)
//             {
//                 CreateButtonInSlot(slot, tool.type);
//             }
//             else
//             {
//                 // No assigned slot for this tool - find any empty slot
//                 slot = FindEmptySlot();
//                 if (slot != null)
//                 {
//                     CreateButtonInSlot(slot, tool.type);
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"No available slot for {tool.name}");
//                 }
//             }
//         }
//     }
    
//     ToolButtonSlot FindSlotForTool(ToolSystem.ToolType toolType)
//     {
//         foreach (var slot in buttonSlots)
//         {
//             if (slot.assignedTool == toolType && !slot.isOccupied)
//                 return slot;
//         }
//         return null;
//     }
    
//     ToolButtonSlot FindEmptySlot()
//     {
//         foreach (var slot in buttonSlots)
//         {
//             if (!slot.isOccupied)
//                 return slot;
//         }
//         return null;
//     }
    
//     void CreateButtonInSlot(ToolButtonSlot slot, ToolSystem.ToolType toolType)
//     {
//         if (slot.slotTransform == null || toolButtonPrefab == null) return;
        
//         // Create button
//         GameObject buttonObj = Instantiate(toolButtonPrefab, slot.slotTransform);
//         Button button = buttonObj.GetComponent<Button>();
        
//         // Set position to slot
//         RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
//         buttonRect.anchoredPosition = Vector2.zero;
//         buttonRect.sizeDelta = slot.slotTransform.sizeDelta;
        
//         // Set button text
//         Text buttonText = buttonObj.GetComponentInChildren<Text>();
//         if (buttonText != null)
//         {
//             buttonText.text = GetToolShortName(toolType);
//         }
        
//         // Set button color
//         Image buttonImage = buttonObj.GetComponent<Image>();
//         if (buttonImage != null)
//         {
//             buttonImage.color = GetToolColor(toolType);
//         }
        
//         // Add click listener
//         button.onClick.AddListener(() => gameFlow.OnToolButtonClicked(toolType));
        
//         // Store reference
//         slot.buttonInstance = button;
//         slot.isOccupied = true;
//     }
    
//     void ClearAllSlots()
//     {
//         foreach (var slot in buttonSlots)
//         {
//             if (slot.buttonInstance != null)
//             {
//                 Destroy(slot.buttonInstance.gameObject);
//                 slot.buttonInstance = null;
//             }
//             slot.isOccupied = false;
//         }
//     }
    
//     string GetToolShortName(ToolSystem.ToolType toolType)
//     {
//         return toolType switch
//         {
//             ToolSystem.ToolType.BurnerPhone => "Phone",
//             ToolSystem.ToolType.MagnifyingGlass => "Glass",
//             ToolSystem.ToolType.Beer => "Beer",
//             ToolSystem.ToolType.Pills => "Pills",
//             ToolSystem.ToolType.HandSaw => "Saw",
//             ToolSystem.ToolType.Adrenaline => "Adren",
//             _ => "???"
//         };
//     }
    
//     Color GetToolColor(ToolSystem.ToolType toolType)
//     {
//         return toolType switch
//         {
//             ToolSystem.ToolType.BurnerPhone => new Color(0.2f, 0.8f, 0.2f),
//             ToolSystem.ToolType.MagnifyingGlass => new Color(0.8f, 0.8f, 0.2f),
//             ToolSystem.ToolType.Beer => new Color(0.9f, 0.6f, 0.2f),
//             ToolSystem.ToolType.Pills => new Color(0.8f, 0.2f, 0.2f),
//             ToolSystem.ToolType.HandSaw => new Color(0.5f, 0.5f, 0.5f),
//             ToolSystem.ToolType.Adrenaline => new Color(0.6f, 0.2f, 0.8f),
//             _ => Color.white
//         };
//     }
// }

