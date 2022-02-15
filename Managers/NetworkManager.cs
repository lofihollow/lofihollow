using System;
using System.Collections.Generic;
using System.Linq;
using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.Minigames.Mining;
using LofiHollow.EntityData;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.DataTypes;
using Steamworks;

namespace LofiHollow.Managers {
	public class NetworkManager {
		public CSteamID ownID;
		public bool isHost = false;
		public bool FoundLobby = false;


		protected Callback<LobbyCreated_t> Callback_lobbyCreated;
		protected Callback<LobbyMatchList_t> Callback_lobbyList;
		protected Callback<LobbyEnter_t> Callback_lobbyEnter;
		protected Callback<LobbyDataUpdate_t> Callback_lobbyInfo;
		protected Callback<LobbyChatMsg_t> Callback_lobbyChatMsg;
		protected Callback<LobbyChatUpdate_t> Callback_lobbyChatUpdate;

		public ulong currentLobby;
		List<CSteamID> lobbyIDs;

		public void Start() {
			lobbyIDs = new();
			Callback_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
			Callback_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
			Callback_lobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyMessage);
			Callback_lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnChatUpdate);
		}

		public void SteamLobby() {
			SteamAPICall_t try_toHost = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 8);
			 
        }

		public void JoinSteamLobby(string roomcode) {
			SteamMatchmaking.AddRequestLobbyListStringFilter("roomCode", roomcode, ELobbyComparison.k_ELobbyComparisonEqual);
			SteamAPICall_t try_getList = SteamMatchmaking.RequestLobbyList();
			SteamAPICall_t try_joinLobby = SteamMatchmaking.JoinLobby(SteamMatchmaking.GetLobbyByIndex(0));
        }

		public void OnLobbyCreated(LobbyCreated_t result) {
			if (result.m_eResult == EResult.k_EResultOK) {
				string lobbyCode = GetRoomCode();
				SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "roomCode", lobbyCode);

				GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible = false;
				GameLoop.UIManager.Map.MapWindow.IsVisible = true;
				GameLoop.UIManager.Map.MessageLog.IsVisible = true;
				GameLoop.UIManager.Sidebar.BattleLog.IsVisible = true;
				GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = true;
				GameLoop.UIManager.selectedMenu = "None";

				isHost = true;

				GameLoop.UIManager.AddMsg(new ColoredString("Created a lobby with code " + lobbyCode, Color.Green, Color.Black));
			}
        }

		public void OnGetLobbiesList(LobbyMatchList_t result) {
			for (int i = 0; i < result.m_nLobbiesMatching; i++) {
				CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
				lobbyIDs.Add(lobbyID);
				SteamMatchmaking.RequestLobbyData(lobbyID); 
            }
        }

		public void OnLobbyEntered(LobbyEnter_t result) {
			if (result.m_EChatRoomEnterResponse == 1) {
				if (SteamMatchmaking.GetLobbyOwner((CSteamID) currentLobby) != ownID) {
					GameLoop.UIManager.MainMenu.joinError = "Joining lobby...";

					currentLobby = result.m_ulSteamIDLobby;

					NetMsg register = new("registerPlayer", GameLoop.World.Player.ToByteArray());
					BroadcastMsg(register);

					NetMsg sendJoin = new("sendJoinData");
					BroadcastMsg(sendJoin);

					GameLoop.UIManager.clientAndConnected = false;
					FoundLobby = true;

					NetMsg requestEnts = new("requestEntities");
					requestEnts.SetMap(GameLoop.World.Player.MapPos);
					byte[] req = requestEnts.ToByteArray();
					Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);
					if (map != null)
						map.Entities.Clear();
					SteamMatchmaking.SendLobbyChatMsg((CSteamID)currentLobby, req, req.Length);
				} else {
					GameLoop.UIManager.MainMenu.joinError = "Failed to join lobby.";
				}
			}
		}

		public void OnChatUpdate(LobbyChatUpdate_t result) {
			if (result.m_rgfChatMemberStateChange == 2) {
				if (GameLoop.World.otherPlayers.ContainsKey((CSteamID) result.m_ulSteamIDUserChanged)) {
					GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[(CSteamID)result.m_ulSteamIDUserChanged].Name + " disconnected.", Color.Orange, Color.Black));
					GameLoop.World.otherPlayers.Remove((CSteamID)result.m_ulSteamIDUserChanged);

					NetMsg discon = new("disconnectedPlayer");
					discon.senderID = (CSteamID) result.m_ulSteamIDUserChanged;

					BroadcastMsg(discon);

					Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);
					if (map != null) {
						GameLoop.UIManager.Map.SyncMapEntities(map);
					}
				}
			}
        }


		public void OnLobbyMessage(LobbyChatMsg_t result) {
			if (result.m_ulSteamIDLobby != currentLobby)
				return;
			if (result.m_eChatEntryType != 1)
				return;
			if ((CSteamID)result.m_ulSteamIDUser == SteamUser.GetSteamID())
				return;

			CSteamID sender;
			byte[] Data = new byte[32768];
			EChatEntryType ChatEntryType;
			int ret = SteamMatchmaking.GetLobbyChatEntry((CSteamID)result.m_ulSteamIDLobby, (int)result.m_iChatID, out sender, Data, Data.Length, out ChatEntryType);

			if (Data.Length > 0) {
				NetMsg msg = Data.FromByteArray<NetMsg>();

				if (msg.recipient != CSteamID.Nil && msg.recipient != ownID)
					return;

				switch (msg.ident) {
					case "createPlayer":
						Player newPlayer = msg.data.FromByteArray<Player>();
						newPlayer.UsePixelPositioning = true;
						if (!GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							GameLoop.World.otherPlayers.Add(msg.senderID, newPlayer);
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
						}
						break;
					case "hostFlags":
						if (msg.MiscString1 == "farm")
							HostOwnsFarm = msg.Flag;
						break;
					case "hostMap":
						Map sentMap = msg.data.FromByteArray<Map>();

						if (msg.MiscString1 == "farm") {
							if (GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0))) {
								GameLoop.World.maps[new Point3D(-1, 0, 0)] = sentMap;
							} else {
								GameLoop.World.maps.Add(new Point3D(-1, 0, 0), sentMap);
							}

							if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0))
								GameLoop.UIManager.Map.UpdateVision();
						}

						break;
					case "usedPermit":
						string username = "Someone";
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID))
							username = GameLoop.World.otherPlayers[msg.senderID].Name;

						if (msg.MiscString1 == "farm") {
							if (isHost && username != GameLoop.World.Player.Name) {
								GameLoop.UIManager.AddMsg(new ColoredString(username + " unlocked the farm for you!", Color.Cyan, Color.Black));
								GameLoop.World.Player.OwnsFarm = true;
							} else if (!isHost && username != GameLoop.World.Player.Name) {
								GameLoop.UIManager.AddMsg(new ColoredString(username + " unlocked the farm!", Color.Cyan, Color.Black));
								HostOwnsFarm = true;
							}
						}

						break;
					case "registerPlayer":
						Player player = msg.data.FromByteArray<Player>();
						if (!GameLoop.World.otherPlayers.ContainsKey(sender)) {
							GameLoop.World.otherPlayers.Add(sender, player);

							Map regmap = Helper.ResolveMap(GameLoop.World.Player.MapPos);
							if (regmap != null) {
								GameLoop.UIManager.Map.SyncMapEntities(regmap);
							}
						}

						GameLoop.UIManager.AddMsg(new ColoredString(player.Name + " connected!", Color.Orange, Color.Black));
						break;
					case "requestEntities":
						Map map = Helper.ResolveMap(msg.GetMapPos());

						if (map != null) {
							if (!GameLoop.World.Player.VisitedMaps.Contains(msg.GetMapPos())) {
								map.PopulateMonsters(msg.GetMapPos());
								GameLoop.World.Player.VisitedMaps.Add(msg.GetMapPos());
							}

							foreach (Entity ent in map.Entities.Items) {
								NetMsg entity = new("");
								if (ent is MonsterWrapper monEntity) {
									entity.ident = "spawnMonster";
									entity.data = monEntity.monster.ToByteArray();
									entity.SetFullPos(ent.Position, msg.GetMapPos());
								}

								if (ent is ItemWrapper itemEntity) {
									entity.ident = "spawnItem";
									entity.data = itemEntity.item.ToByteArray();
									entity.SetFullPos(ent.Position, msg.GetMapPos());
								}

								if (entity.ident != "") {
									BroadcastMsg(entity);
								}
							}
						}
						break;
					case "goToGame":
						ownID = SteamUser.GetSteamID();
						GameLoop.UIManager.clientAndConnected = true;
						GameLoop.World.DoneInitializing = true;

						GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible = false;
						GameLoop.UIManager.Map.MapWindow.IsVisible = true;
						GameLoop.UIManager.Map.MessageLog.IsVisible = true;
						GameLoop.UIManager.Sidebar.BattleLog.IsVisible = true;
						GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = true;
						GameLoop.UIManager.selectedMenu = "None";
						break;
					case "time":
						GameLoop.World.Player.Clock = msg.data.FromByteArray<TimeManager>();
						break;
					case "moveNPC":
						if (GameLoop.World.npcLibrary.ContainsKey(msg.MiscString1)) {
							GameLoop.World.npcLibrary[msg.MiscString1].MoveTo(msg.GetPos(), msg.GetMapPos());
						}
						break;
					case "moveMonster":
						CommandManager.MoveMonster(msg.MiscString1, msg.GetMapPos(), msg.GetPos());
						break;
					case "damageMonster":
						CommandManager.DamageMonster(msg.MiscString1, msg.GetMapPos(), msg.MiscInt, msg.MiscString2, msg.MiscString3);
						break;
					case "damagePlayer":
						CommandManager.DamagePlayer(msg.senderID, msg.MiscInt, msg.MiscString1, msg.MiscString2);
						break;
					case "updateTile":
						Map tileMap = Helper.ResolveMap(msg.GetMapPos());

						if (tileMap != null) {
							Tile tile = msg.data.FromByteArray<Tile>();

							tileMap.SetTile(msg.GetPos(), tile);
							tileMap.GetTile(msg.GetPos()).UpdateAppearance();
							if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(msg.GetPos().ToCoord())) {
								tileMap.GetTile(msg.GetPos()).Unshade();
							} else {
								tileMap.GetTile(msg.GetPos()).Shade();
							}
						}
						break;
					case "updateMine":
						MineTile mineTile = msg.data.FromByteArray<MineTile>();

						if (msg.MiscString1 == "Mountain") {
							if (GameLoop.UIManager.Minigames.MineManager.MountainMine != null && GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.ContainsKey(msg.MiscInt))
								GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[msg.MiscInt].SetTile(msg.GetPos(), mineTile);
						}

						if (msg.MiscString1 == "Lake") {
							if (GameLoop.UIManager.Minigames.MineManager.LakeMine != null && GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.ContainsKey(msg.MiscInt))
								GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[msg.MiscInt].SetTile(msg.GetPos(), mineTile);
						}

						break;
					case "spawnItem":
						Item spawnItem = msg.data.FromByteArray<Item>();
						ItemWrapper wrap = new(spawnItem);
						wrap.Position = msg.GetPos();
						wrap.MapPos = msg.GetMapPos();
						CommandManager.SpawnItem(wrap);
						break;
					case "mineItem":
						Item mineItem = msg.data.FromByteArray<Item>();
						ItemWrapper mineWrap = new(mineItem);
						mineWrap.Position = msg.GetPos();

						if (msg.MiscString1 == "Mountain") {
							GameLoop.UIManager.Minigames.MineManager.MountainMine.SpawnItem(mineWrap, msg.MiscInt);
						}

						if (msg.MiscString1 == "Lake") {
							GameLoop.UIManager.Minigames.MineManager.LakeMine.SpawnItem(mineWrap, msg.MiscInt);
						}
						break;
					case "updateSkill":
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							if (GameLoop.World.otherPlayers[msg.senderID].Skills.ContainsKey(msg.MiscString1)) {
								GameLoop.World.otherPlayers[msg.senderID].Skills[msg.MiscString1].Level = msg.MiscInt;
							}
						}
						break;
					case "spawnMonster":
						CommandManager.SpawnMonster(msg.MiscString1, msg.GetMapPos(), msg.GetPos());
						break;
					case "disconnectedPlayer":
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[msg.senderID].Name + " disconnected.", Color.Orange, Color.Black));
							GameLoop.World.otherPlayers.Remove(msg.senderID);
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
						}
						break;
					case "destroyItem":
						Item destroyItem = msg.data.FromByteArray<Item>();
						ItemWrapper destroyWrap = new(destroyItem);
						destroyWrap.Position = msg.GetPos();
						destroyWrap.MapPos = msg.GetMapPos();
						CommandManager.DestroyItem(destroyWrap);
						break;
					case "mineDestroy":
						Item mineDestroy = msg.data.FromByteArray<Item>();
						ItemWrapper mineDestroyWrap = new(mineDestroy);
						mineDestroyWrap.Position = msg.GetPos();

						if (msg.MiscString1 == "Mountain") {
							GameLoop.UIManager.Minigames.MineManager.MountainMine.DestroyItem(mineDestroyWrap, msg.MiscInt);
						}

						if (msg.MiscString1 == "Lake") {
							GameLoop.UIManager.Minigames.MineManager.LakeMine.DestroyItem(mineDestroyWrap, msg.MiscInt);
						}
						break;
					case "movePlayer":
						GameLoop.World.otherPlayers[msg.senderID].MoveTo(msg.GetPos(), msg.GetMapPos());
						GameLoop.UIManager.Map.UpdateVision();
						break;
					case "updatePlayerMine":
						GameLoop.World.otherPlayers[msg.senderID].Position = msg.GetPos();
						GameLoop.World.otherPlayers[msg.senderID].MineLocation = msg.MiscString1;
						GameLoop.World.otherPlayers[msg.senderID].MineDepth = msg.MiscInt;
						break;
					case "updateChest":
						Container chestContents = msg.data.FromByteArray<Container>();

						if (!GameLoop.World.maps.ContainsKey(msg.GetMapPos()))
							GameLoop.World.LoadMapAt(msg.GetMapPos());

						if (GameLoop.World.maps[msg.GetMapPos()].GetTile(msg.GetPos()).Container != null)
							GameLoop.World.maps[msg.GetMapPos()].GetTile(msg.GetPos()).Container = chestContents;

						if (GameLoop.World.Player.MapPos == msg.GetMapPos() && msg.GetPos() == GameLoop.UIManager.Container.ContainerPosition) {
							GameLoop.UIManager.Container.CurrentContainer = GameLoop.World.maps[msg.GetMapPos()].GetTile(msg.GetPos()).Container;
						}
						break;
					case "fullMine":
						if (!isHost) {
							MineLevel sentMine = msg.data.FromByteArray<MineLevel>();

							if (msg.MiscString1 == "Mountain") {
								if (GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.ContainsKey(msg.MiscInt)) {
									GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[msg.MiscInt] = sentMine;
								} else {
									GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.Add(msg.MiscInt, sentMine);
								}
								GameLoop.UIManager.Minigames.ToggleMinigame("Mining");
								GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[GameLoop.World.Player.MineDepth].MapFOV);
							} else if (msg.MiscString1 == "Lake") {
								if (GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.ContainsKey(msg.MiscInt)) {
									GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[msg.MiscInt] = sentMine;
								} else {
									GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.Add(msg.MiscInt, sentMine);
								}
								GameLoop.UIManager.Minigames.ToggleMinigame("Mining");
								GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[GameLoop.World.Player.MineDepth].MapFOV);
							}
						}

						break;
					case "newDay":
						if (msg.Flag)
							GameLoop.UIManager.AddMsg(new ColoredString("You passed out!", Color.Red, Color.Black));
						else
							GameLoop.UIManager.AddMsg(new ColoredString("You sleep through the night.", Color.Lime, Color.Black));

						GameLoop.World.Player.Sleeping = false;
						GameLoop.World.Player.CurrentHP = GameLoop.World.Player.MaxHP;

						foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
							kv.Value.CurrentHP = kv.Value.MaxHP;
							kv.Value.Sleeping = false;
						}

						if (GameLoop.World.Player.NoonbreezeApt != null) {
							GameLoop.World.Player.NoonbreezeApt.DaysLeft--;

							if (GameLoop.World.Player.NoonbreezeApt.DaysLeft == 0) {
								GameLoop.UIManager.AddMsg("This is your last day of rent, pay for another month!");
							}

							if (GameLoop.World.Player.NoonbreezeApt.DaysLeft < 0) {
								GameLoop.World.Player.NoonbreezeApt = null;
							}
						}

						GameLoop.World.SavePlayer();
						break;
					case "sleep":
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							GameLoop.World.otherPlayers[msg.senderID].Sleeping = msg.Flag;

							int sleepCount = 0;
							int totalPlayers = 1;
							if (GameLoop.World.Player.Sleeping)
								sleepCount++;

							foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
								totalPlayers++;
								if (kv.Value.Sleeping)
									sleepCount++;
							}

							if (msg.Flag) {
								GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[msg.senderID].Name + " went to sleep. (" + sleepCount + "/" + totalPlayers + ")", Color.Lime, Color.Black));
							} else {
								GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[msg.senderID].Name + " decided not to sleep. (" + sleepCount + "/" + totalPlayers + ")", Color.Lime, Color.Black));
							}
						}
						break;
					case "apartment":
						if (msg.MiscString1 == "Noonbreeze")
							if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID))
								GameLoop.World.otherPlayers[msg.senderID].NoonbreezeApt = msg.data.FromByteArray<Apartment>();
						break;
					case "requestMine":
						if (isHost) {
							NetMsg fullMine = new("fullMine");
							fullMine.MiscString1 = msg.MiscString1;
							fullMine.MiscInt = msg.MiscInt;

							if (msg.MiscString1 == "Mountain") {
								fullMine.data = GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[msg.MiscInt].ToByteArray();
							} else if (msg.MiscString1 == "Lake") {
								fullMine.data = GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[msg.MiscInt].ToByteArray();
							}

							BroadcastMsg(fullMine);
						}
						break;
					case "fullMap":
						if (!isHost) {
							Map fullmap = msg.data.FromByteArray<Map>();

							if (!GameLoop.World.maps.ContainsKey(msg.GetMapPos()))
								GameLoop.World.maps.Add(msg.GetMapPos(), fullmap);
							else
								GameLoop.World.maps[msg.GetMapPos()] = fullmap;

							if (msg.GetMapPos() == GameLoop.World.Player.MapPos) {
								GameLoop.UIManager.Map.LoadMap(GameLoop.World.Player.MapPos);
								NetMsg reqEnts = new("requestEntities");
								reqEnts.SetMap(GameLoop.World.Player.MapPos);
								GameLoop.SendMessageIfNeeded(reqEnts, false, false, 0);

								Map clearmap = Helper.ResolveMap(GameLoop.World.Player.MapPos);
								if (clearmap != null) {
									clearmap.Entities.Clear();
								}
							}
						} else {
							NetMsg reply = new("fullMap");

							Map sendmap = Helper.ResolveMap(msg.GetMapPos());
							if (sendmap != null) {
								reply.data = sendmap.ToByteArray();
								reply.SetMap(msg.GetMapPos());

								BroadcastMsg(reply);
							}
						}
						break;
					case "startCombat":
						NetMsg combatReply = new("fullPlayer", GameLoop.World.Player.ToByteArray());
						BroadcastMsg(combatReply);

						GameLoop.UIManager.Combat.Toggle();
						GameLoop.UIManager.Combat.Current = msg.data.FromByteArray<Combat>();
						break;
					case "fullPlayer":
						if (GameLoop.World.otherPlayers.ContainsKey(sender))
							GameLoop.World.otherPlayers[sender] = msg.data.FromByteArray<Player>();
						else
							GameLoop.World.otherPlayers.Add(sender, msg.data.FromByteArray<Player>());
						break;
					case "sendJoinData":
						foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
							NetMsg createPlayer = new("createPlayer", kv.Value.ToByteArray());
							createPlayer.senderID = kv.Key;
							createPlayer.recipient = sender;
							BroadcastMsg(createPlayer);
						}

						NetMsg yourID = new("goToGame");
						yourID.recipient = sender;
						BroadcastMsg(yourID);

						NetMsg createHost = new("createPlayer", GameLoop.World.Player.ToByteArray());
						createHost.senderID = ownID;
						createHost.recipient = sender;
						BroadcastMsg(createHost);

						NetMsg farmFlag = new("hostFlags");
						farmFlag.MiscString1 = "farm";
						farmFlag.Flag = GameLoop.World.Player.OwnsFarm;
						farmFlag.recipient = sender;
						BroadcastMsg(farmFlag);

						if (GameLoop.World.Player.OwnsFarm) {
							if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
								GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

							Map normalFarm = GameLoop.World.UnchangedMap(new Point3D(-1, 0, 0));

							for (int i = 0; i < GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles.Length; i++) {
								if (GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i] != normalFarm.Tiles[i]) {
									NetMsg updateTile = new("updateTile", GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i].ToByteArray());
									updateTile.SetFullPos(new Point(i % GameLoop.MapWidth, i / GameLoop.MapWidth), new Point3D(-1, 0, 0));
									GameLoop.SendMessageIfNeeded(updateTile, false, false);
								}
							}

							//	string hostFarm = "hostMap;farm;" + JsonConvert.SerializeObject(GameLoop.World.maps[new Point3D(-1, 0, 0)], Formatting.Indented);
							//	lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(hostFarm));
						}

						foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) {
							NetMsg moveNPC = new("moveNPC");
							moveNPC.MiscString1 = kv.Key;
							moveNPC.SetFullPos(kv.Value.Position, kv.Value.MapPos);
							moveNPC.recipient = sender;
							BroadcastMsg(moveNPC);
						}

						Map currentMap = Helper.ResolveMap(GameLoop.World.Player.MapPos);

						if (currentMap != null) {
							GameLoop.UIManager.Map.SyncMapEntities(currentMap);
						}
						break;
					default:
						GameLoop.UIManager.AddMsg(msg.ident);
						break;
				}
			}
		}




		public bool HostOwnsFarm = false;

		public NetworkManager(bool second = false) {
			Start();
		}

		public void BroadcastMsg(NetMsg msg) { 
			byte[] message = msg.ToByteArray();
			SteamMatchmaking.SendLobbyChatMsg((CSteamID)currentLobby, message, message.Length);
		}

		public static string GetRoomCode() {
			string[] letters = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z".Split(",");

			return letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)]
				+ letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)];
		}
	}
}
