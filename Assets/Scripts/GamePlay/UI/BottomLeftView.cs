using GamePlay.PlayerDataHandle;
using TMPro;
using UnityEngine;

namespace GamePlay.UI
{
    public class BottomLeftView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI goldLabel;
        [SerializeField] TextMeshProUGUI diamondLabel;
        [SerializeField] TextMeshProUGUI powerLabel;

        private PlayerDataManager playerDataManager;

        void Start()
        {
            playerDataManager = PlayerDataManager.Instance;
            if (playerDataManager == null) return;
            playerDataManager.OnCurrencyChanged += RefreshCurrency;
            playerDataManager.OnPowerChanged += RefreshPower;
            RefreshCurrency();
            RefreshPower(playerDataManager.TotalPower);
        }

        void OnEnable()
        {
            if (playerDataManager == null) return;
            RefreshCurrency();
            RefreshPower(playerDataManager.TotalPower);
        }

        void OnDisable()
        {
            if (playerDataManager == null) return;
            playerDataManager.OnCurrencyChanged -= RefreshCurrency;
            playerDataManager.OnPowerChanged -= RefreshPower;
            playerDataManager = null;
        }

        private void RefreshCurrency()
        {
            SetGold(playerDataManager.PlayerData.gameProps.gold);
            SetDiamond(playerDataManager.PlayerData.gameProps.diamonds);
        }

        private void RefreshPower(int totalPower)
        {
            SetPower(totalPower);
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
