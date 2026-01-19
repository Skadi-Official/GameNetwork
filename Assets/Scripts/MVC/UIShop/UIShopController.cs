using UnityEngine;

public class UIShopController : MonoBehaviour
{
    private const int ItemPrice = 50;
    private UIShopView m_View;
    
    private void Start()
    {
        m_View = GetComponent<UIShopView>();
        m_View.Refresh(ItemPrice, UIUserModel.instance.Coin);
        m_View.RefreshSellBtn(UIUserModel.instance.InventoryItemCount);
        m_View.BtnBuy.onClick.AddListener(OnBtnBuyClick);
        m_View.BtnSell.onClick.AddListener(OnBtnSoldClick);
        UIUserModel.instance.AddObserver(OnCoinChanged);
    }
    private void OnDestroy()
    {
        UIUserModel.instance.RemoveObserver(OnCoinChanged);
    }
    private void OnCoinChanged(UIUserModel.EPropChangeType type)
    {
        if (type == UIUserModel.EPropChangeType.Coin)
        {
            m_View.Refresh(ItemPrice,UIUserModel.instance.Coin);
            m_View.RefreshSellBtn(UIUserModel.instance.InventoryItemCount);
        }
    }
    private void OnBtnBuyClick()
    {
        UIUserModel.instance.RequestBuyItem(ItemPrice);
    }

    private void OnBtnSoldClick()
    {
        UIUserModel.instance.RequestSoldItem(ItemPrice);
    }
}