using GamePlay.PlayerDataHandle;
using TMPro;
using UnityEngine;

namespace GamePlay.UI
{
    public class TopBarView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI goldLabel;
        [SerializeField] TextMeshProUGUI diamondLabel;
        [SerializeField] TextMeshProUGUI powerLabel;

        private PlayerDataManager playerDataManager;

        void OnEnable()
        {
            playerDataManager = PlayerDataManager.Instance;
            playerDataManager.OnCurrencyChanged += RefreshCurrency;
            RefreshCurrency();
        }

        void OnDisable()
        {
            playerDataManager.OnCurrencyChanged -= RefreshCurrency;
        }

        private void RefreshCurrency()
        {
            SetGold(playerDataManager.PlayerData.gameProps.gold);
            SetDiamond(playerDataManager.PlayerData.gameProps.diamonds);
        }

        private void SetGold(int value)
        {
            if (goldLabel) goldLabel.text = value.ToString("N0");
        }

        private void SetDiamond(int value)
        {
            if (diamondLabel) diamondLabel.text = value.ToString("N0");
        }

        private void SetPower(int value)
        {
            if (powerLabel) powerLabel.text = value.ToString("N0");
        }
    }
}
