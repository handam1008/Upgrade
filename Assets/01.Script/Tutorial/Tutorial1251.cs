using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorial1251 : MonoBehaviour
{
    private IEnumerator tutorial;
    [SerializeField] private GoldManager _gold;
    [SerializeField] TextMeshProUGUI Quest;
    public bool one = false;
    public bool two = false;
    
    private void Start()
    {
        tutorial = Tutorial();
        tutorial.MoveNext();
    }

    public IEnumerator Tutorial()
    {
        Quest.SetText("화난 네모를 만드시오");
        _gold.AddCoin();
        yield return null;
        Quest.SetText("미친 네모를 만드시오");
        _gold.AddCoin();
        yield return null;
        Quest.SetText("역병 네모를 만드시오 (반복 퀘스트)");
        _gold.AddCoin();
    }

    public void MoveNext()
    {
        tutorial.MoveNext();
    }
}
