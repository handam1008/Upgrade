using System;
using System.Collections.Generic;
using System.Linq;
using Test32.FeedBack;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GoldManager : MonoBehaviour
{
    public int Coin = 100;
    
    [SerializeField] private Tutorial1251 tutorial;
    
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI squaresText;
    [SerializeField] private TextMeshProUGUI upgradePercentText;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private TextMeshProUGUI sellPriceText;
    
    [SerializeField] private AbsractSquare[] _data;
    [SerializeField] private GameObject[] squarePrefabs;
    [SerializeField] private Transform sqaureSpawnPoint;
    
    [SerializeField] private FeedSquare successFeedback;
    [SerializeField] private FeedSquare FailFeedBack;
    private int currentData = 0;
    
    private List<GameObject> squares = new List<GameObject>();
    
    public delegate void onMyDelegate();
    public onMyDelegate myDelegate;

    private void Start()
    {
        myDelegate += ChangeUI;
        

        for (int i = 0; i < 9; i++)
        {
           GameObject Clone = Instantiate(squarePrefabs[i], sqaureSpawnPoint);
            squares.Add(Clone);
            Debug.Log(squarePrefabs + "소환");
        }
        myDelegate?.Invoke();
        
    }

    private void Update()
    {
        if (currentData == 1)
        {
            if (tutorial.one) return;
            
            tutorial.MoveNext();
            tutorial.one = true;
        }
        else if (currentData == 3)
        {
            if (tutorial.two) return;
            
            tutorial.MoveNext();
            tutorial.two = true;
        }
        
    }

    public void AddCoin()
    {
        Coin += 100;
    }
    

    public void Buy()
    {
        if (Coin < _data[currentData].upgradePrice) return;
        
        int i = Random.Range(0, 100); // 95퍼센트면 100개중에 95개

        if (i <= _data[currentData].upgradePercent)
        {
            GameObject square = squares[currentData];
            square.SetActive(false);
            successFeedback.PlayAllFeedBacks();
            
            
            
             if (currentData == 6)
            {
                AddCoin();
            }
            currentData++;
        }
        else
        {
            FailFeedBack.PlayAllFeedBacks();
            
        }
        
        Coin -= _data[currentData].upgradePrice;
        myDelegate?.Invoke();
    }

    public void Sell()
    {
        Coin += _data[currentData].sellCoin;
        for (int i = 0; i < currentData; i++)
        {
            squares[i].SetActive(true);
        }

        currentData = 0;
        myDelegate?.Invoke();
    }

    private void ChangeUI()
    {
        squaresText.SetText(_data[currentData].squareName);
        coinText.SetText("Coin: " + Coin);
        upgradePriceText.SetText("Upgrade Price : " + _data[currentData].upgradePrice);
        upgradePercentText.SetText(_data[currentData].upgradePercent + "%");
        sellPriceText.SetText("Sell Price " + _data[currentData].sellCoin);
    }
}
