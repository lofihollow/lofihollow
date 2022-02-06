using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.Minigames.Mining;
using LofiHollow.EntityData;
using SadConsole;
using SadRogue.Primitives;

namespace LofiHollow.Managers {
	public class NetworkManager {
		public Discord.Discord discord;
		public LobbyManager lobbyManager;
		public UserManager userManager;
		public long lobbyID;
		public long ownID;
		public bool isHost = false;
		public bool FoundLobby = false;


		public bool HostOwnsFarm = false;

		public NetworkManager(bool second = false) {
			if (second) {
				System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "1");
				discord = new Discord.Discord(579827348665532425, (UInt64)Discord.CreateFlags.Default);
			} else {
				System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "0");
				discord = new Discord.Discord(579827348665532425, (UInt64)Discord.CreateFlags.Default);
			}
			//discord = new Discord.Discord(579827348665532425, (UInt64)Discord.CreateFlags.Default);

		}

		public void BroadcastMsg(string msg) {
			if (lobbyManager != null) {
				for (int i = 0; i < lobbyManager.MemberCount(lobbyID); i++) {
					var userID = lobbyManager.GetMemberUserId(lobbyID, i);
					lobbyManager.SendNetworkMessage(lobbyID, userID, 0, Encoding.UTF8.GetBytes(msg));
				}
			}
		}

		public void BroadcastMsg(NetMsg msg) {
			if (lobbyManager != null) {
				for (int i = 0; i < lobbyManager.MemberCount(lobbyID); i++) {
					var userID = lobbyManager.GetMemberUserId(lobbyID, i);
					lobbyManager.SendNetworkMessage(lobbyID, userID, 0, msg.ToByteArray());
				}
			}
		}

		private void ProcessMessage(long lobbyId, long userId, byte channelId, byte[] data) {
			if (lobbyId == lobbyID && userId != ownID) {
				NetMsg msg = data.FromByteArray<NetMsg>();

				switch (msg.msgID) {
					case "createPlayer":
						long id = msg.senderID;
						Player newPlayer = msg.data.FromByteArray<Player>();
						if (!GameLoop.World.otherPlayers.ContainsKey(id)) {
							GameLoop.World.otherPlayers.Add(id, new PlayerWrapper(newPlayer));
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
						}
						break;
					case "hostFlags":
						bool farm = msg.data.FromByteArray<bool>();
						HostOwnsFarm = farm;
						break;
					case "hostMap":
						string map = msg.MiscString;
						Map sentMap = msg.data.FromByteArray<Map>();

						if (map == "farm") {
							if (GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0))) {
								GameLoop.World.maps[new Point3D(-1, 0, 0)] = sentMap;
							} else {
								GameLoop.World.maps.Add(new Point3D(-1, 0, 0), sentMap);
							}

							if (GameLoop.World.Player.player.MapPos == new Point3D(-1, 0, 0))
								GameLoop.UIManager.Map.UpdateVision();
						}

						break;
					case "usedPermit":
						string permit = msg.MiscString;
						string username = "";
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID))
							username = GameLoop.World.otherPlayers[msg.senderID].player.Name;

						if (permit == "farm") {
							if (isHost && username != GameLoop.World.Player.player.Name) {
								GameLoop.UIManager.AddMsg(new ColoredString(username + " unlocked the farm for you!", Color.Cyan, Color.Black));
								GameLoop.World.Player.player.OwnsFarm = true;
							} else if (!isHost && username != GameLoop.World.Player.player.Name) {
								GameLoop.UIManager.AddMsg(new ColoredString(username + " unlocked the farm!", Color.Cyan, Color.Black));
								HostOwnsFarm = true;
							}
						}

						break;
					case "registerPlayer":
						Player player = msg.data.FromByteArray<Player>();
						if (!GameLoop.World.otherPlayers.ContainsKey(userId)) {
							GameLoop.World.otherPlayers.Add(userId, new PlayerWrapper(player));
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
						}

						GameLoop.UIManager.AddMsg(new ColoredString(player.Name + " connected!", Color.Orange, Color.Black));
						break;
					case "requestEntities":
						int requestX = msg.mX;
						int requestY = msg.mY;
						int requestZ = msg.mZ;

						Point3D requestPos = new(requestX, requestY, requestZ);

						if (!GameLoop.World.maps.ContainsKey(requestPos))
							GameLoop.World.LoadMapAt(requestPos);

						if (GameLoop.World.maps.ContainsKey(requestPos)) {
							if (!GameLoop.World.Player.player.VisitedMaps.Contains(requestPos)) {
								GameLoop.World.maps[requestPos].PopulateMonsters(requestPos);
								GameLoop.World.Player.player.VisitedMaps.Add(requestPos);
							}

							foreach (Entity ent in GameLoop.World.maps[requestPos].Entities.Items) {
								NetMsg reqMsg = null;
								if (ent is MonsterWrapper monEntity) {
									reqMsg = new("spawnMonster", monEntity.ToByteArray());
								}

								if (ent is ItemWrapper itemEntity) {
									reqMsg = new("spawnItem", itemEntity.item.ToByteArray());
								}

								if (reqMsg != null) {
									lobbyManager.SendNetworkMessage(lobbyID, userId, 0, reqMsg.ToByteArray());
								}
							}
						}
						break;
					case "yourID":
						ownID = msg.data.FromByteArray<long>();
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
						GameLoop.World.Player.player.Clock = msg.data.FromByteArray<TimeManager>();
						break;
					case "moveNPC":
						string NPCid = msg.MiscString;
						int posX = msg.X;
						int posY = msg.Y;
						int mapX = msg.mX;
						int mapY = msg.mY;
						int mapZ = msg.mZ;

						if (GameLoop.World.npcLibrary.ContainsKey(NPCid)) {
							GameLoop.World.npcLibrary[NPCid].npc.MoveTo(new Point(posX, posY), new Point3D(mapX, mapY, mapZ));
						}
						break;
					case "moveMonster":
						Point3D monMoveMap = new(msg.mX, msg.mY, msg.mZ);
						Point monNewPos = new(msg.X, msg.Y);

						CommandManager.MoveMonster(msg.MiscString, monMoveMap, monNewPos);
						break;
					case "damageMonster":
						CommandManager.DamageMonster(msg.MiscString, new Point3D(msg.mX, msg.mY, msg.mZ), msg.MiscInt, msg.MiscString1, msg.MiscString2);
						break;
					case "damagePlayer":
						CommandManager.DamagePlayer(msg.senderID, msg.MiscInt, msg.MiscString, msg.MiscString1);
						break;
					case "updateTile":
						Tile tile = msg.data.FromByteArray<Tile>();

						Point3D tileMapPos = new(msg.mX, msg.mY, msg.mZ);
						Point tilePos = new(msg.X, msg.Y);

						if (!GameLoop.World.maps.ContainsKey(tileMapPos))
							GameLoop.World.LoadMapAt(tileMapPos);

						GameLoop.World.maps[tileMapPos].SetTile(tilePos, tile);
						GameLoop.World.maps[tileMapPos].GetTile(tilePos).UpdateAppearance();
						if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(tilePos.ToCoord())) {
							GameLoop.World.maps[tileMapPos].GetTile(tilePos).Unshade();
						} else {
							GameLoop.World.maps[tileMapPos].GetTile(tilePos).Shade();
						}
						break;
					case "updateMine":
						string Loc = msg.MiscString1;
						int Depth = msg.MiscInt;
						MineTile mineTile = msg.data.FromByteArray<MineTile>();
						Point mineTilePos = new(msg.X, msg.Y);

						if (Loc == "Mountain") {
							if (GameLoop.UIManager.Minigames.MineManager.MountainMine != null && GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.ContainsKey(Depth))
								GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[Depth].SetTile(mineTilePos, mineTile);
						}

						if (Loc == "Lake") {
							if (GameLoop.UIManager.Minigames.MineManager.LakeMine != null && GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.ContainsKey(Depth))
								GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[Depth].SetTile(mineTilePos, mineTile);
						}


						break;
					case "spawnItem":
						Item spawnItem = msg.data.FromByteArray<Item>();
						ItemWrapper wrap = new(spawnItem);
						CommandManager.SpawnItem(wrap);
						break;
					case "updateSkill":
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							if (GameLoop.World.otherPlayers[msg.senderID].player.Skills.ContainsKey(msg.MiscString)) {
								GameLoop.World.otherPlayers[msg.senderID].player.Skills[msg.MiscString].Level = msg.MiscInt;
							}
						}

						break;
					case "spawnMonster":
						CommandManager.SpawnMonster(msg.MiscString, new Point3D(msg.mX, msg.mY, msg.mZ), new Point(msg.X, msg.Y));
						break;
					case "disconnectedPlayer":
						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[msg.senderID].player.Name + " disconnected.", Color.Orange, Color.Black));
							GameLoop.World.otherPlayers.Remove(msg.senderID);
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
						}
						break;
					case "destroyItem":
						Item destroyItem = msg.data.FromByteArray<Item>();
						ItemWrapper destroyWrap = new(destroyItem);
						CommandManager.DestroyItem(destroyWrap);
						break;
					case "movePlayer":
						GameLoop.World.otherPlayers[msg.senderID].player.MoveTo(new Point(msg.X, msg.Y), new Point3D(msg.mX, msg.mY, msg.mZ));
						GameLoop.UIManager.Map.UpdateVision();
						break;
					case "updatePlayerMine":
						GameLoop.World.otherPlayers[msg.senderID].player.Position = new Point(msg.X, msg.Y);
						GameLoop.World.otherPlayers[msg.senderID].player.MineLocation = msg.MiscString;
						GameLoop.World.otherPlayers[msg.senderID].player.MineDepth = msg.MiscInt;
						break;
					case "updateChest":

						Point3D chestMap = new Point3D(msg.mX, msg.mY, msg.mZ);
						Point chestPos = new Point(msg.X, msg.Y);

						Container chestContents = msg.data.FromByteArray<Container>();

						if (!GameLoop.World.maps.ContainsKey(chestMap))
							GameLoop.World.LoadMapAt(chestMap);

						if (GameLoop.World.maps[chestMap].GetTile(chestPos).Container != null)
							GameLoop.World.maps[chestMap].GetTile(chestPos).Container = chestContents;

						if (GameLoop.World.Player.player.MapPos == chestMap && chestPos == GameLoop.UIManager.Container.ContainerPosition) {
							GameLoop.UIManager.Container.CurrentContainer = GameLoop.World.maps[chestMap].GetTile(chestPos).Container;
						}
						break;
					case "requestMine":
						if (isHost) {
							string mineName = msg.MiscString;
							int mineLevel = msg.MiscInt;

							NetMsg mineMsg = new("fullMine", null);
							mineMsg.MiscString = mineName;
							mineMsg.MiscInt = mineLevel;

							if (mineName == "Mountain") {
								mineMsg.data = GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[mineLevel].ToByteArray();
							} else if (mineName == "Lake") {
								mineMsg.data = GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[mineLevel].ToByteArray();
							}

							lobbyManager.SendNetworkMessage(lobbyID, userId, 0, mineMsg.ToByteArray());
						}
						break;
					case "fullMine":
						if (!isHost) {
							string mineName = msg.MiscString;
							int mineLevel = msg.MiscInt;

							MineLevel sentMine = msg.data.FromByteArray<MineLevel>();

							if (mineName == "Mountain") {
								if (GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.ContainsKey(mineLevel)) {
									GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[mineLevel] = sentMine;
								} else {
									GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.Add(mineLevel, sentMine);
								}
								GameLoop.UIManager.Minigames.ToggleMinigame("Mining");
								GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[GameLoop.World.Player.player.MineDepth].MapFOV);
							} else if (mineName == "Lake") {
								if (GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.ContainsKey(mineLevel)) {
									GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[mineLevel] = sentMine;
								} else {
									GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.Add(mineLevel, sentMine);
								}
								GameLoop.UIManager.Minigames.ToggleMinigame("Mining");
								GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[GameLoop.World.Player.player.MineDepth].MapFOV);
							}
						}

						break;
					case "newDay":
						bool passedOut = msg.data.FromByteArray<bool>();
						if (passedOut)
							GameLoop.UIManager.AddMsg(new ColoredString("You passed out!", Color.Red, Color.Black));
						else
							GameLoop.UIManager.AddMsg(new ColoredString("You sleep through the night.", Color.Lime, Color.Black));

						GameLoop.World.Player.player.Sleeping = false;
						GameLoop.World.Player.player.CurrentHP = GameLoop.World.Player.player.MaxHP;

						foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
							kv.Value.player.CurrentHP = kv.Value.player.MaxHP;
							kv.Value.player.Sleeping = false;
						}

						GameLoop.World.SavePlayer();
						break;
					case "sleep":
						bool sleeping = msg.data.FromByteArray<bool>();

						if (GameLoop.World.otherPlayers.ContainsKey(msg.senderID)) {
							GameLoop.World.otherPlayers[msg.senderID].player.Sleeping = sleeping;

							int sleepCount = 0;
							int totalPlayers = 1;
							if (GameLoop.World.Player.player.Sleeping)
								sleepCount++;

							foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
								totalPlayers++;
								if (kv.Value.player.Sleeping)
									sleepCount++;
							}

							if (sleeping) {
								GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[msg.senderID].player.Name + " went to sleep. (" + sleepCount + "/" + totalPlayers + ")", Color.Lime, Color.Black));
							} else {
								GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[msg.senderID].player.Name + " decided not to sleep. (" + sleepCount + "/" + totalPlayers + ")", Color.Lime, Color.Black));
							}
						}
						break;
					default:
						GameLoop.UIManager.AddMsg(msg.msgID);
						break;
				}
			}
		}


		public void CreateLobby() {
			lobbyManager = discord.GetLobbyManager();

			var transaction = lobbyManager.GetLobbyCreateTransaction();
			transaction.SetCapacity(4);
			transaction.SetType(Discord.LobbyType.Public);


			string lobbyCode = GetRoomCode();
			transaction.SetMetadata("RoomCode", lobbyCode);

			GameLoop.UIManager.MainMenu.joinError = "Creating lobby...";


			lobbyManager.CreateLobby(transaction, (Discord.Result result, ref Discord.Lobby lobby) => {
				if (result == Discord.Result.Ok) {
					lobbyManager.ConnectNetwork(lobby.Id);
					lobbyID = lobby.Id;

					lobby.Secret = "123";

					lobbyManager.OpenNetworkChannel(lobbyID, 0, true);
					lobbyManager.OpenNetworkChannel(lobbyID, 1, false);

					lobbyManager.OnNetworkMessage += ProcessMessage;
					lobbyManager.OnMemberConnect += MemberConnected;
					lobbyManager.OnMemberDisconnect += MemberDisconnected;

					isHost = true;
					ownID = lobbyManager.GetLobby(lobbyID).OwnerId;

					GameLoop.UIManager.MainMenu.MainMenuWindow.IsVisible = false;
					GameLoop.UIManager.Map.MapWindow.IsVisible = true;
					GameLoop.UIManager.Map.MessageLog.IsVisible = true;
					GameLoop.UIManager.Sidebar.BattleLog.IsVisible = true;
					GameLoop.UIManager.Sidebar.SidebarWindow.IsVisible = true;
					GameLoop.UIManager.selectedMenu = "None";

					GameLoop.UIManager.AddMsg(new ColoredString("Created a lobby with code " + lobbyCode, Color.Green, Color.Black));
				}
			});
		}

		private void MemberDisconnected(long lobbyId, long userId) {
			if (lobbyId == lobbyID) {
				if (GameLoop.World.otherPlayers.ContainsKey(userId)) {
					GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[userId].player.Name + " disconnected.", Color.Orange, Color.Black));
					GameLoop.World.otherPlayers.Remove(userId);

					NetMsg discon = new("disconnectedPlayer", null);
					discon.senderID = userId;

					BroadcastMsg(discon);

					GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
				}
			}
		}

		private void MemberConnected(long lobbyId, long userId) {
			if (lobbyId == lobbyID) {
				foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
					NetMsg player = new("createPlayer", kv.Value.ToByteArray());
					player.senderID = kv.Key;
					lobbyManager.SendNetworkMessage(lobbyId, userId, 0, player.ToByteArray());
				}

				NetMsg yourID = new("yourID", userId.ToByteArray());
				lobbyManager.SendNetworkMessage(lobbyId, userId, 0, yourID.ToByteArray());

				NetMsg ownPlayer = new("createPlayer", GameLoop.World.Player.player.ToByteArray());
				ownPlayer.senderID = ownID;
				lobbyManager.SendNetworkMessage(lobbyId, userId, 0, ownPlayer.ToByteArray());

				NetMsg hostFlags = new("hostFlags", GameLoop.World.Player.player.OwnsFarm.ToByteArray());
				lobbyManager.SendNetworkMessage(lobbyId, userId, 0, hostFlags.ToByteArray());

				if (GameLoop.World.Player.player.OwnsFarm) {
					if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
						GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

					Map normalFarm = GameLoop.World.UnchangedMap(new Point3D(-1, 0, 0));

					for (int i = 0; i < GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles.Length; i++) {
						if (GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i] != normalFarm.Tiles[i]) {
							NetMsg tileUpdate = new("updateTile", GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i].ToByteArray());
							tileUpdate.X = i % GameLoop.MapWidth;
							tileUpdate.Y = i / GameLoop.MapWidth;
							tileUpdate.mX = -1;
							tileUpdate.mY = 0;
							tileUpdate.mZ = 0;
							GameLoop.SendMessageIfNeeded(tileUpdate, false, false);
						}
					}

					//	string hostFarm = "hostMap;farm;" + JsonConvert.SerializeObject(GameLoop.World.maps[new Point3D(-1, 0, 0)], Formatting.Indented);
					//	lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(hostFarm));
				}

				foreach (KeyValuePair<string, NPCWrapper> kv in GameLoop.World.npcLibrary) {
					NetMsg npc = new("moveNPC", null);
					npc.MiscString = kv.Key;
					npc.SetPosition(kv.Value.npc.Position);
					npc.SetMapPos(kv.Value.npc.MapPos);
					lobbyManager.SendNetworkMessage(lobbyId, userId, 0, npc.ToByteArray());
				}

				GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.player.MapPos]);
			}
		}

		public void InitNetworking(Int64 lobbyId) {
			lobbyManager = discord.GetLobbyManager();
			lobbyManager.ConnectNetwork(lobbyId);

			lobbyManager.OpenNetworkChannel(lobbyId, 0, true);
			lobbyManager.OpenNetworkChannel(lobbyId, 1, false);
			lobbyID = lobbyId;
		}

		public void SearchLobbiesAndJoin(string code) {
			lobbyManager = discord.GetLobbyManager();

			var query = lobbyManager.GetSearchQuery();
			query.Filter("metadata.RoomCode", LobbySearchComparison.Equal, LobbySearchCast.String, code);

			lobbyManager.Search(query, (result) => {
				if (result == Discord.Result.Ok) {
					var count = lobbyManager.LobbyCount();

					GameLoop.UIManager.MainMenu.joinError = "No Lobby Found";

					if (count == 1) {
						GameLoop.UIManager.MainMenu.joinError = "Connecting...";

						long connectID = lobbyManager.GetLobbyId(0);
						Discord.Lobby lobby = lobbyManager.GetLobby(connectID);

						lobbyManager.ConnectLobby(connectID, lobby.Secret, (Discord.Result result, ref Discord.Lobby lobby) => {
							if (result == Result.Ok) {
								InitNetworking(lobby.Id);
								lobbyManager.OnNetworkMessage += ProcessMessage;

								lobbyID = lobby.Id;

								NetMsg msg = new("registerPlayer", GameLoop.World.Player.player.ToByteArray());
								BroadcastMsg(msg);

								GameLoop.UIManager.clientAndConnected = false;
								FoundLobby = true;

								GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Entities.Clear();

								NetMsg initRequest = new("requestEntities", null);
								initRequest.mX = GameLoop.World.Player.player.MapPos.X;
								initRequest.mY = GameLoop.World.Player.player.MapPos.Y;
								initRequest.mZ = GameLoop.World.Player.player.MapPos.Z;
								lobbyManager.SendNetworkMessage(lobby.Id, lobby.OwnerId, 0, initRequest.ToByteArray());
							}
						});
					}
				}
			});
		}




		public static string GetRoomCode() {
			string[] letters = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z".Split(",");

			return letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)]
				+ letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)] + letters[GameLoop.rand.Next(letters.Length)];
		}
	}
}
