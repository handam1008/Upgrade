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
    
    [SerializeField] private Transform sqaureSpawnPoint;
    
    [SerializeField] private FeedSquare successFeedback;
    [SerializeField] private FeedSquare FailFeedBack;
    
    public int CurrentData { get; private set; }= 0;
    [field: SerializeField] public List<AbsractSquare> _data { get; private set; }
    [field: SerializeField] public GameObject[] squarePrefabs;
    
    
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
        if (CurrentData == 1)
        {
            if (tutorial.one) return;
            
            tutorial.MoveNext();
            tutorial.one = true;
        }
        else if (CurrentData == 3)
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
        if (_data.Count-1 <= CurrentData) return;
        
        if (Coin < _data[CurrentData].upgradePrice) return;
        
        int i = Random.Range(0, 100); // 95퍼센트면 100개중에 95개

        if (i <= _data[CurrentData].upgradePercent)
        {
            GameObject square = squares[CurrentData];
            square.SetActive(false);
            successFeedback.PlayAllFeedBacks();
            
            
            
             if (CurrentData == 6)
            {
                AddCoin();
            }
            CurrentData++;
        }
        else
        {
            FailFeedBack.PlayAllFeedBacks();
            
        }
        
        Coin -= _data[CurrentData].upgradePrice;
        myDelegate?.Invoke();
    }

    public void Sell()
    {
        Coin += _data[CurrentData].sellCoin;
        for (int i = 0; i < CurrentData; i++)
        {
            squares[i].SetActive(true);
        }

        CurrentData = 0;
        myDelegate?.Invoke();
    }

    private void ChangeUI()
    {
        squaresText.SetText(_data[CurrentData].squareName);
        coinText.SetText("Coin: " + Coin);
        upgradePriceText.SetText("Upgrade Price : " + _data[CurrentData].upgradePrice);
        upgradePercentText.SetText(_data[CurrentData].upgradePercent + "%");
        sellPriceText.SetText("Sell Price " + _data[CurrentData].sellCoin);
    }
}
