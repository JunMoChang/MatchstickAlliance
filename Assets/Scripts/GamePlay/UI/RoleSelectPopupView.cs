using System;
using System.Collections.Generic;
using AssetLoad;
using GamePlay.GameModel.Level;
using GamePlay.Role.RoleData;
using GamePlay.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class RoleSelectPopupView : MonoBehaviour
    {
        [SerializeField] Button backgroundDismiss;
        [SerializeField] Button cancelButton;
        [SerializeField] Button ensureButton;
        [SerializeField] TextMeshProUGUI hintLabel;
        [SerializeField] private GameObject heroCardPrefab;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private Transform heroCardContainer;

        private const int DefaultMaxSelectCount = 2;
        private int maxSelectCount = DefaultMaxSelectCount;
        private readonly List<RoleRegistry.RoleEntry> selectedHeroes = new(DefaultMaxSelectCount);

        private Image[] cardImages;
        private bool cardsGenerated;

        private bool isPvPMode;
        private Action<RoleRegistry.RoleEntry> onPvPRoleSelected;
        private Action onPvPCancelled;

        void OnEnable()
        {
            if (backgroundDismiss) backgroundDismiss.onClick.AddListener(Hide);
            if (cancelButton) cancelButton.onClick.AddListener(Hide);
            if (ensureButton) ensureButton.onClick.AddListener(OnEnsureClicked);
        }

        void OnDisable()
        {
            if (backgroundDismiss) backgroundDismiss.onClick.RemoveListener(Hide);
            if (cancelButton) cancelButton.onClick.RemoveListener(Hide);
            if (ensureButton) ensureButton.onClick.RemoveListener(OnEnsureClicked);
        }

        public void Show()
        {
            if (GameDataManager.RoleRegistry == null)
            {
                Debug.LogError("RoleRegistry 尚未加载");
                return;
            }

            if (!cardsGenerated) GenerateCards();

            isPvPMode = false;
            maxSelectCount = DefaultMaxSelectCount;
            onPvPRoleSelected = null;
            onPvPCancelled = null;
            selectedHeroes.Clear();

            RoleRegistry.RoleEntry[] entries = GameDataManager.RoleRegistry.entries;
            for (int i = 0; i < cardImages.Length && i < entries.Length; i++)
            {
                cardImages[i].gameObject.SetActive(true);
                cardImages[i].sprite = entries[i].template.unSelectedIcon;
            }

            gameObject.SetActive(true);
            UpdateHint();
        }

        /// <summary>
        /// PvP 大厅选角入口,展示玩家拥有的角色，确认后回调
        /// </summary>
        public void ShowForPvP(List<RoleRegistry.RoleEntry> ownedRoles, Action<RoleRegistry.RoleEntry> onSelected, Action onCancelled = null)
        {
            if (GameDataManager.RoleRegistry == null)
            {
                Debug.LogError("RoleRegistry 尚未加载");
                return;
            }

            if (!cardsGenerated) GenerateCards();

            isPvPMode = true;
            maxSelectCount = 1;
            onPvPRoleSelected = onSelected;
            onPvPCancelled = onCancelled;
            selectedHeroes.Clear();

            RoleRegistry.RoleEntry[] allEntries = GameDataManager.RoleRegistry.entries;
            for (int i = 0; i < cardImages.Length && i < allEntries.Length; i++)
            {
                bool owned = ownedRoles.Exists(r => r.roleName == allEntries[i].roleName);
                cardImages[i].gameObject.SetActive(owned);
                if (owned) cardImages[i].sprite = allEntries[i].template.unSelectedIcon;
            }

            gameObject.SetActive(true);
            UpdateHint();
        }

        private void Hide()
        {
            if (isPvPMode)
            {
                onPvPCancelled?.Invoke();
                isPvPMode = false;
            }
            gameObject.SetActive(false);
        }

        private void GenerateCards()
        {
            if (GameDataManager.RoleRegistry == null) return;

            RoleRegistry.RoleEntry[] entries = GameDataManager.RoleRegistry.entries;
            cardImages = new Image[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                GameObject card = Instantiate(heroCardPrefab, heroCardContainer);
                
                cardImages[i] = card.GetComponent<Image>();
                cardImages[i].sprite = entries[i].template.unSelectedIcon;
                
                TextMeshProUGUI cardName = card.GetComponentInChildren<TextMeshProUGUI>();
                cardName.text = entries[i].roleName.ToString();
                
                Transform starContent = card.transform.GetChild(1).transform;
                int childCount = starContent.childCount;
                while(childCount-- > 0) Destroy(starContent.GetChild(0).gameObject);
                
                for (int j = 0; j < entries[i].template.defaultStar; j++)
                {
                    Instantiate(starPrefab, starContent);
                }
                
                Button btn = card.GetComponent<Button>();
                if (btn)
                {
                    int index = i;
                    btn.onClick.AddListener(() => OnHeroCardClicked(index));
                }
            }

            cardsGenerated = true;
        }

        private void OnHeroCardClicked(int index)
        {
            if (GameDataManager.RoleRegistry == null) return;

            RoleRegistry.RoleEntry[] entries = GameDataManager.RoleRegistry.entries;
            
            bool isUnSelected = entries[index].template.selectedIcon != cardImages[index].sprite;
            if (isUnSelected)
            {
                if (selectedHeroes.Count >= maxSelectCount)
                {
                    Debug.Log("上场英雄已满！");
                    return;
                }

                selectedHeroes.Add(entries[index]);
                cardImages[index].sprite = entries[index].template.selectedIcon;
            }
            else
            {
                selectedHeroes.Remove(entries[index]);
                cardImages[index].sprite = entries[index].template.unSelectedIcon;
            }

            UpdateHint();
        }

        private void UpdateHint()
        {
            if (hintLabel) hintLabel.text = $"人数限制:{selectedHeroes.Count}/{maxSelectCount}";
        }

        private void OnEnsureClicked()
        {
            if (selectedHeroes.Count == 0)
            {
                Debug.Log("至少选择一个上场的英雄！");
                return;
            }

            if (isPvPMode)
            {
                onPvPRoleSelected?.Invoke(selectedHeroes[0]);
                isPvPMode = false;
                gameObject.SetActive(false);
                return;
            }

            LevelContext.SelectedHeroes = selectedHeroes;
            SceneLoader.Instance.LoadLevel(LevelContext.CurrentChapter, LevelContext.CurrentLevel);
        }
    }
}
