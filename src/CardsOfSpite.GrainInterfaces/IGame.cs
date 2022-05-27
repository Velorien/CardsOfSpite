﻿using CardsOfSpite.Models;
using Orleans;

namespace CardsOfSpite.GrainInterfaces;
public interface IGame : IGrainWithGuidKey
{
    Task<bool> Initialize(GameSettings settings);
    Task<bool> Join(string playerId, string name);
    Task Leave(string playerId);
    Task<bool> SelectCards(string playerId, List<Guid> cardIds);
    Task SelectWinner(string playerId);
    Task DiscardHand(string playerId);
}
