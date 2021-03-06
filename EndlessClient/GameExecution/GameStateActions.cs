﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.ControlSets;
using EndlessClient.Network;
using Microsoft.Xna.Framework;

namespace EndlessClient.GameExecution
{
    public class GameStateActions : IGameStateActions
    {
        private readonly IGameStateRepository _gameStateRepository;
        private readonly IControlSetRepository _controlSetRepository;
        private readonly IControlSetFactory _controlSetFactory;
        private readonly IEndlessGameProvider _endlessGameProvider;

        public GameStateActions(IGameStateRepository gameStateRepository,
                                IControlSetRepository controlSetRepository,
                                IControlSetFactory controlSetFactory,
                                IEndlessGameProvider endlessGameProvider)
        {
            _gameStateRepository = gameStateRepository;
            _controlSetRepository = controlSetRepository;
            _controlSetFactory = controlSetFactory;
            _endlessGameProvider = endlessGameProvider;
        }

        public void ChangeToState(GameStates newState)
        {
            if (newState == _gameStateRepository.CurrentState)
                return;

            var currentSet = _controlSetRepository.CurrentControlSet;
            var nextSet = _controlSetFactory.CreateControlsForState(newState, currentSet);

            RemoveOldComponents(currentSet, nextSet);
            AddNewComponents(nextSet);

            _gameStateRepository.CurrentState = newState;
            _controlSetRepository.CurrentControlSet = nextSet;
        }

        public void RefreshCurrentState()
        {
            var currentSet = _controlSetRepository.CurrentControlSet;
            var emptySet = new EmptyControlSet();

            RemoveOldComponents(currentSet, emptySet);
            var refreshedSet = _controlSetFactory.CreateControlsForState(currentSet.GameState, emptySet);
            AddNewComponents(refreshedSet);
            _controlSetRepository.CurrentControlSet = refreshedSet;
        }

        public void ExitGame()
        {
            Game.Exit();
        }

        private void AddNewComponents(IControlSet nextSet)
        {
            foreach (var component in nextSet.AllComponents.Except(nextSet.XNAControlComponents))
                if (!Game.Components.Contains(component))
                    Game.Components.Add(component);
        }

        private void RemoveOldComponents(IControlSet currentSet, IControlSet nextSet)
        {
            var componentsToRemove = FindUnusedComponents(currentSet, nextSet);
            var disposableComponents = componentsToRemove
                .Where(x => !(x is PacketHandlerGameComponent))
                .OfType<IDisposable>()
                .ToList();

            foreach (var component in disposableComponents)
                component.Dispose();
            foreach (var component in componentsToRemove.Where(Game.Components.Contains))
                Game.Components.Remove(component);
        }

        private List<IGameComponent> FindUnusedComponents(IControlSet current, IControlSet next)
        {
            return current.AllComponents
                .Where(component => !next.AllComponents.Contains(component))
                .ToList();
        }

        private IEndlessGame Game => _endlessGameProvider.Game;
    }
}
