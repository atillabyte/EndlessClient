﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Data.BLL;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	public class CharacterStateTestGame : Game
	{
		private enum DisplayState
		{
			Standing,
			SitChair,
			SitFloor,
			Attack1,
			Attack2,
			Walk1,
			Walk2,
			Walk3,
			SpellCast,
			AttackingAnimation,
			WalkingAnimation,
			SpellCastAnimation
		}

		private static readonly List<DisplayState> _allDisplayStates;

		static CharacterStateTestGame()
		{
			_allDisplayStates = ((DisplayState[]) Enum.GetValues(typeof (DisplayState))).ToList();
		}

		private readonly EOGame _baseGame;
		private readonly IDataFile<ItemRecord> _itemFile;
		private readonly GraphicsDeviceManager _graphicsDeviceManager;

		private ICharacterRenderProperties _baseProperties;
		private readonly List<ICharacterRenderer> _renderersForDifferentStates;

		private XNALabel _howToLabel;

		private KeyboardState _previousState, _currentState;

		private DateTime _lastWalk, _lastAttack, _lastSpell;

		public CharacterStateTestGame(EOGame baseGame, IDataFile<ItemRecord> itemFile)
		{
			_baseGame = baseGame;
			_itemFile = itemFile;

			_graphicsDeviceManager = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 640,
				PreferredBackBufferHeight = 480,
				IsFullScreen = false
			};

			_renderersForDifferentStates = new List<ICharacterRenderer>(12);
		}

		protected override void Initialize()
		{
			IsMouseVisible = false;

			_baseProperties = new CharacterRenderProperties();
			foreach (var displayState in _allDisplayStates)
			{
				var props = GetRenderPropertiesForState(displayState);
				
				var characterRenderer = new CharacterRenderer(_baseGame, props);
				characterRenderer.SetAbsoluteScreenPosition(640 / 4 * ((int)displayState % 4),
															480 / 3 * ((int)displayState / 4));

				_renderersForDifferentStates.Add(characterRenderer);
			}

			_renderersForDifferentStates.ForEach(Components.Add);

			_currentState = _previousState = Keyboard.GetState();
			_lastWalk = _lastAttack = _lastSpell = DateTime.Now;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_howToLabel = new XNALabel(new Rectangle(0, 458, 640, 22), Constants.FontSize08)
			{
				ForeColor = Color.Black,
				BackColor = Color.White,
				AutoSize = false,
				TextAlign = LabelAlignment.MiddleCenter,
				Text = "1: Change Gender | 2: Change HairStyle | 3: Change Hat | 4: Change Armor | 5: Change Boots | 6: Change Weapon | 7: Change Shield | 8: Change Direction | Esc: Close"
			};

			RefreshDisplayedCharacters();

			base.LoadContent();
		}

		//standing = 0
		//sitting: chair floor
		//attacking: 0 1 2   +animated (0-1-2-0)
		//walking:   0 1 2 3 +animated (0-1-2-3-0)
		//spellcast: 0 1     +animated (0-1-0)

		protected override void Update(GameTime gameTime)
		{
			_currentState = Keyboard.GetState();

			var update = false;
			if (KeyPressed(Keys.D1))
			{
				_baseProperties = _baseProperties.WithGender((byte) ((_baseProperties.Gender + 1)%2));
				update = true;
			}
			else if (KeyPressed(Keys.D2))
			{
				_baseProperties = _baseProperties.WithHairStyle((byte) ((_baseProperties.HairStyle + 1)%21));
				update = true;
			}
			else if (KeyPressed(Keys.D3))
			{
				_baseProperties = _baseProperties.WithHatGraphic(GetNextItemGraphicMatching(ItemType.Hat, _baseProperties.HatGraphic));
				update = true;
			}
			else if (KeyPressed(Keys.D4))
			{
				_baseProperties = _baseProperties.WithArmorGraphic(GetNextItemGraphicMatching(ItemType.Armor, _baseProperties.ArmorGraphic));
				update = true;
			}
			else if (KeyPressed(Keys.D5))
			{
				_baseProperties = _baseProperties.WithBootsGraphic(GetNextItemGraphicMatching(ItemType.Boots, _baseProperties.BootsGraphic));
				update = true;
			}
			else if (KeyPressed(Keys.D6))
			{
				_baseProperties = _baseProperties.WithWeaponGraphic(GetNextItemGraphicMatching(ItemType.Weapon, _baseProperties.WeaponGraphic));
				update = true;
			}
			else if (KeyPressed(Keys.D7))
			{
				_baseProperties = _baseProperties.WithShieldGraphic(GetNextItemGraphicMatching(ItemType.Shield, _baseProperties.ShieldGraphic));
				update = true;
			}
			else if (KeyPressed(Keys.D8))
			{
				_baseProperties = _baseProperties.WithDirection((EODirection)(((int)_baseProperties.Direction + 1) % 4));
				update = true;
			}

			if(update)
				RefreshDisplayedCharacters();

			_previousState = _currentState;

			var now = DateTime.Now;
			if ((now - _lastWalk).TotalMilliseconds > 100)
			{
				var rend = _renderersForDifferentStates[(int) DisplayState.WalkingAnimation];
				rend.RenderProperties = rend.RenderProperties.WithNextWalkFrame();
				_lastWalk = now;
			}

			if ((now - _lastAttack).TotalMilliseconds > 285)
			{
				var rend = _renderersForDifferentStates[(int)DisplayState.AttackingAnimation];
				rend.RenderProperties = rend.RenderProperties.WithNextAttackFrame();
				_lastAttack = now;
			}

			if ((now - _lastSpell).TotalMilliseconds > 280)
			{
				var rend = _renderersForDifferentStates[(int)DisplayState.SpellCastAnimation];
				rend.RenderProperties = rend.RenderProperties.WithNextSpellCastFrame();
				_lastSpell = now;
			}

			base.Update(gameTime);
		}

		private ICharacterRenderProperties GetRenderPropertiesForState(DisplayState displayState)
		{
			switch (displayState)
			{
				case DisplayState.Standing:
					return _baseProperties;
				case DisplayState.SitChair:
					return _baseProperties.WithSitState(SitState.Chair);
				case DisplayState.SitFloor:
					return _baseProperties.WithSitState(SitState.Floor);
				case DisplayState.Attack1:
					return _baseProperties.WithNextAttackFrame();
				case DisplayState.Attack2:
					return _baseProperties.WithNextAttackFrame().WithNextAttackFrame();
				case DisplayState.Walk1:
					return _baseProperties.WithNextWalkFrame();
				case DisplayState.Walk2:
					return _baseProperties.WithNextWalkFrame();
				case DisplayState.Walk3:
					return _baseProperties.WithNextWalkFrame();
				case DisplayState.SpellCast:
					return _baseProperties.WithNextSpellCastFrame();
				//create a clone of the properties for animation
				case DisplayState.WalkingAnimation:
				case DisplayState.SpellCastAnimation:
				case DisplayState.AttackingAnimation:
					return (ICharacterRenderProperties)_baseProperties.Clone();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void RefreshDisplayedCharacters()
		{
			foreach (var displayState in _allDisplayStates)
			{
				var props = GetRenderPropertiesForState(displayState);
				_renderersForDifferentStates[(int) displayState].RenderProperties = props;
			}
		}

		private bool KeyPressed(Keys key)
		{
			return _previousState.IsKeyDown(key) && _currentState.IsKeyUp(key);
		}

		private short GetNextItemGraphicMatching(ItemType type, short currentGraphic)
		{
			var matchingItems = _itemFile.Data.Where(x => x.Type == type).OrderBy(x => x.ID).ToList();
			var ndx = matchingItems.FindIndex(x => x.DollGraphic == currentGraphic);
			return (short) matchingItems[(ndx + 1)%matchingItems.Count].DollGraphic;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_renderersForDifferentStates.ForEach(x => x.Dispose());

				_howToLabel.Close();

				_graphicsDeviceManager.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
