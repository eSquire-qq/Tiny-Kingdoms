using UnityEngine;

public class TrainUI : MonoBehaviour
{
    public LobbyClient client;

    private int selectedBuildingId;

    public void SetSelectedBuilding(int id)
    {
        selectedBuildingId = id;
    }

    public void TrainSwordsman()
    {
        client.CmdTrainUnit(selectedBuildingId, "swordsman");
    }

    public void TrainArcher()
    {
        client.CmdTrainUnit(selectedBuildingId, "archer");
    }

    public void TrainWorker()
    {
        client.CmdTrainUnit(selectedBuildingId, "worker");
    }
}