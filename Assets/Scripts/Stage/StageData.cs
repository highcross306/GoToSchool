using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StageData",
    menuName = "Game/Stage Data"
)]
public class StageData : ScriptableObject
{
    public int timeLimitSeconds;
    public int initialBudget;
    public NodeData[] nodes;
    public RouteData[] routes;
}