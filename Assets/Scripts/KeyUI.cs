using UnityEngine;
using UnityEngine.UI; // Required to manipulate UI elements

public class KeyUIUpdater : MonoBehaviour
{
    [Header("Target Player")]
    // We reference your base Player class so it works for both Player1 and Player2 scripts
    public Players.Player targetPlayer; 

    [Header("Arrow Backgrounds (UI Images)")]
    public Image upArrowBg;
    public Image downArrowBg;
    public Image leftArrowBg;
    public Image rightArrowBg;

    [Header("Color States")]
    public Color availableColor = Color.white;
    public Color unavailableColor = Color.black;

    void Update()
    {
        // Safety check: if player is not assigned or destroyed, do nothing
        if (targetPlayer == null) return;

        // --- UPDATE THE UI COLORS ---
        // This uses a ternary operator (condition ? trueResult : falseResult)
        // If haveUp is true, it turns white. If false, it turns black.
        
        if (upArrowBg != null)
            upArrowBg.color = targetPlayer.haveUp ? availableColor : unavailableColor;

        if (downArrowBg != null)
            downArrowBg.color = targetPlayer.haveDown ? availableColor : unavailableColor;

        if (leftArrowBg != null)
            leftArrowBg.color = targetPlayer.haveLeft ? availableColor : unavailableColor;

        if (rightArrowBg != null)
            rightArrowBg.color = targetPlayer.haveRight ? availableColor : unavailableColor;
    }
}