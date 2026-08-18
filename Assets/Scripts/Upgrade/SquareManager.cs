using System;
using UnityEngine;

public class SquareManager : MonoBehaviour
{
    [SerializeField] private CurrentSquareData squareData;
    [SerializeField] private Transform squareSpawnPoint;
    [SerializeField] private SquareStat squareStat;

     private void Awake()
     {
     }

     private void Start()
     {
       GameObject square = Instantiate(squareData.SquarePrefab, squareSpawnPoint.position, Quaternion.identity);
     }
}
