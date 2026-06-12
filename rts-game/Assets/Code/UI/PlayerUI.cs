using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] 
    private TMPro.TextMeshProUGUI messageText;
    
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI lumberText;
    public TextMeshProUGUI supplyText;

    public int myPlayerId;

    public void UpdateFromState(StateMsg state)
    {
        if (state == null || state.players == null) return;

        foreach (var p in state.players)
        {
            if (p.playerId != myPlayerId) continue;

            if (goldText != null)
                goldText.text = p.gold.ToString();

            if (lumberText != null)
                lumberText.text = p.lumber.ToString();

            if (supplyText != null)
                supplyText.text = $"{p.usedSupply} / {p.maxSupply}";

            return;
        }
    }
    
    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        Debug.LogWarning(message);
    }
    
}