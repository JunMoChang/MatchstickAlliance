using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class NavBarView : MonoBehaviour
    {
        public event Action<int> OnClicked;

        [SerializeField] Button[] functionButtons;

        public void Initialize(FunctionButtonData[] buttonData)
        {
            int count = Mathf.Min(functionButtons.Length, buttonData.Length);
            for (int i = 0; i < count; i++)
            {
                int index = i;
                FunctionButtonData btnData = buttonData[i];
                Button btn = functionButtons[i];

                btn.onClick.AddListener(() => OnClicked?.Invoke(index));

                Image icon = btn.GetComponent<Image>();
                if (icon && btnData.icon) icon.sprite = btnData.icon;

                TextMeshProUGUI label = btn.GetComponent<TextMeshProUGUI>();
                if (label) label.text = btnData.nameLabel.ToString();

                TextMeshProUGUI badge = btn.transform.Find("Badge")?.GetComponent<TextMeshProUGUI>();
                if (badge)
                {
                    if (btnData.badge != 0)
                    {
                        badge.text = btnData.badge == -1 ? "!" : btnData.badge.ToString();
                        badge.gameObject.SetActive(true);
                    }
                    else
                    {
                        badge.gameObject.SetActive(false);
                    }
                }
            }
            
            for (int i = count; i < functionButtons.Length; i++)
                functionButtons[i].gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            foreach (Button btn in functionButtons)
            {
                if (btn) btn.onClick.RemoveAllListeners();
            }
        }
    }
}
