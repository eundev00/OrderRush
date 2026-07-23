using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using OrderRush.Models;
using UniRx;
using UnityEngine;

public class AccountService : IAccountService, IDisposable
{
    private readonly ILocalStorageService _storage;
    private readonly IGameDataService _gameDataService;
    private readonly ISubscriber<PaymentEvent> _paymentSubscriber;
    private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    private readonly CompositeDisposable _disposables = new();

    public Account Account { get; private set; } = new();

    public AccountService(
        ILocalStorageService storage,
        IGameDataService gameDataService,
        ISubscriber<PaymentEvent> paymentSubscriber,
        ISubscriber<DayEndedEvent> dayEndedSubscriber)
    {
        _storage = storage;
        _gameDataService = gameDataService;
        _paymentSubscriber = paymentSubscriber;
        _dayEndedSubscriber = dayEndedSubscriber;
    }

    public void Initialize()
    {
        Account.OwnedRecipeIDs.AddRange(_gameDataService.GetDefaultRecipeIDs());
        Load();

        _dayEndedSubscriber
            .Subscribe(OnDayEnded)
            .AddTo(_disposables);
    }

    private void OnDayEnded(DayEndedEvent evt)
    {
        SetCurrentProgress(evt.NextDay);

        if (evt.IsCompleted)
        {
            AddCoins(evt.EarnedCoins);
        }
    }

    public void AddCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError($"Cannot add negative coins: {amount}");
            return;
        }
        Account.Coins.Value += amount;
        Save();
    }

    public void SpendCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError($"Cannot spend negative coins: {amount}");
            return;
        }
        if (Account.Coins.Value < amount)
        {
            Debug.LogError($"Not enough coins. Have: {Account.Coins.Value}, Need: {amount}");
            return;
        }
        Account.Coins.Value -= amount;
        Save();
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount < 0 || Account.Coins.Value < amount)
            return false;
        Account.Coins.Value -= amount;
        Save();
        return true;
    }

    public void AddOwnedRecipe(int recipeID)
    {
        if (Account.OwnedRecipeIDs.Contains(recipeID))
            return;
        Account.OwnedRecipeIDs.Add(recipeID);
        Save();
    }

    public int GetRandomOwnedRecipeID()
    {
        if (Account.OwnedRecipeIDs.Count == 0)
            return -1;
        return Account.OwnedRecipeIDs[UnityEngine.Random.Range(0, Account.OwnedRecipeIDs.Count)];
    }

    public void SetCurrentProgress(int day)
    {
        Account.CurrentDay = day;
        Save();
    }

    public void AddPurchasedCard(int cardID)
    {
        if (!Account.PurchasedCardIDs.Contains(cardID))
        {
            Account.PurchasedCardIDs.Add(cardID);
            Save();
        }
    }

    public List<int> GetPurchasedCardIDs()
    {
        return Account.PurchasedCardIDs;
    }

    public void ResetAll()
    {
        _storage.DeleteAll();

        Account.Coins.Value = 0;
        Account.CurrentDay = 1;
        Account.CurrentRun = 1;
        Account.OwnedRecipeIDs.Clear();
        Account.PurchasedCardIDs.Clear();

        Account.OwnedRecipeIDs.AddRange(_gameDataService.GetDefaultRecipeIDs());
    }

    private void Save()
    {
        _storage.SaveInt(LocalStorageKeys.AccountCoins, Account.Coins.Value);

        string recipeIDs = string.Join(",", Account.OwnedRecipeIDs);
        _storage.SaveString(LocalStorageKeys.AccountOwnedRecipes, recipeIDs);

        string cardIDs = string.Join(",", Account.PurchasedCardIDs);
        _storage.SaveString(LocalStorageKeys.PurchasedCardIDs, cardIDs);

        _storage.SaveInt(LocalStorageKeys.CurrentRun, Account.CurrentRun);
        _storage.SaveInt(LocalStorageKeys.CurrentDay, Account.CurrentDay);
    }

    private void Load()
    {
        Account.Coins.Value = _storage.LoadInt(LocalStorageKeys.AccountCoins, 0);

        string recipeIDs = _storage.LoadString(LocalStorageKeys.AccountOwnedRecipes, "");
        if (!string.IsNullOrEmpty(recipeIDs))
        {
            foreach (var id in recipeIDs.Split(',').Select(int.Parse))
                if (!Account.OwnedRecipeIDs.Contains(id))
                    Account.OwnedRecipeIDs.Add(id);
        }

        string cardIDs = _storage.LoadString(LocalStorageKeys.PurchasedCardIDs, "");
        if (!string.IsNullOrEmpty(cardIDs))
        {
            Account.PurchasedCardIDs = cardIDs.Split(',')
                .Select(int.Parse)
                .ToList();
        }

        Account.CurrentRun = _storage.LoadInt(LocalStorageKeys.CurrentRun, 1);
        Account.CurrentDay = _storage.LoadInt(LocalStorageKeys.CurrentDay, 1);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
