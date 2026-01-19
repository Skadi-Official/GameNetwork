using UnityEngine;

public class UIUserInfoController : MonoBehaviour
{
    private UIUserInfoView m_View;
    private void Start()
    {
        m_View = GetComponent<UIUserInfoView>();
        var userModel = UIUserModel.instance;
        m_View.Refresh(userModel.Name, userModel.Coin);
        userModel.AddObserver(OnCoinChanged);
        userModel.AddObserver(OnChange);
    }
    private void OnDestroy()
    {
        UIUserModel.instance.RemoveObserver(OnCoinChanged);
        UIUserModel.instance.RemoveObserver(OnChange);
    }
    private void OnCoinChanged(UIUserModel.EPropChangeType type)
    {
        if (type == UIUserModel.EPropChangeType.Coin)
        {
            var userModel = UIUserModel.instance;
            m_View.Refresh(userModel.Name, userModel.Coin);
            
        }
    }

    private void OnChange(UIUserModel.EPropChangeType type)
    {
        if (type != UIUserModel.EPropChangeType.Gold)
        {
            Debug.Log(type);
        }
    }
}