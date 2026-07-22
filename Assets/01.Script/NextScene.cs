using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class NextScene : MonoBehaviour
{
    [SerializeField] private CurrentSquareData squareData;
    [SerializeField] private GoldManager gold;
    
    public void FightScene()
    {
        squareData.upgradePrice = gold._data[gold.CurrentData].upgradePrice;
        squareData.sellCoin = gold._data[gold.CurrentData].sellCoin ;
        squareData.SquarePrefab = gold._data[gold.CurrentData].squarePrefab;
        SceneManager.LoadScene("FIghtScene");
        Debug.Log("현재 네모 체력 " + squareData.upgradePrice);
        Debug.Log("현재 네모 데미지 " +  squareData.sellCoin);
        Debug.Log("현재 네모 프리팹 " + squareData.SquarePrefab);
    }
    
    
}
