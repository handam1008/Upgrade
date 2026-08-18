using UnityEngine;

[CreateAssetMenu(fileName = "CurrentSquareData", menuName = "SO/Square/CurrentSquareData")]
public class CurrentSquareData : ScriptableObject
{
    public GameObject SquarePrefab;
    public int upgradePrice;
    public int sellCoin;
}
