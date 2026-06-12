using TMPro;
using UnityEngine;



public class TopBarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI lumberText;
    [SerializeField] private TextMeshProUGUI supplyText;
    [SerializeField] private TopBarUI topBarUI;
    
    private int myPlayerId;

    public void SetPlayerId(int id)
    {
        myPlayerId = id;
    }

    public void UpdateFromState(StateMsg state)
    {
        if (state == null || state.players == null) return; 

        foreach (var p in state.players)
        {
            if (p.playerId != myPlayerId) continue;

            goldText.text = $"Gold: {p.gold}";
            lumberText.text = $"Lumber: {p.lumber}";
            supplyText.text = $"Supply: {p.usedSupply} / {p.maxSupply}";
            return;
        }
    }
}