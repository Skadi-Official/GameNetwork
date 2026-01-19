using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIShopView : MonoBehaviour
{
    public Button BtnBuy;
    public Text Label;
    [FormerlySerializedAs("BtnSold")] public Button BtnSell;
    public void Refresh(int price, int wallet)
    {
        Label.color = price > wallet ? Color.red : Color.green;
        Label.text = $"{price}/{wallet}";
    }

    public void RefreshSellBtn(int inventoryItemCount)
    {
        Debug.Log(inventoryItemCount);
        BtnSell.gameObject.SetActive(inventoryItemCount > 0);
    }
}