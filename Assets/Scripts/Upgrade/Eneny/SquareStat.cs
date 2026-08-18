using System;
using UnityEngine;

public class SquareStat : MonoBehaviour
{
    [SerializeField] private int hp = 0;
    [SerializeField] private int damage = 0;
    [SerializeField] private CurrentSquareData squareData;

    private void Start()
    {
        hp = squareData.upgradePrice;
        damage = squareData.sellCoin;
    }
}
