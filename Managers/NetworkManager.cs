using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.Minigames.Mining;
using Newtonsoft.Json;
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

		private void ProcessMessage(long lobbyId, long userId, byte channelId, byte[] data) {
			if (lobbyId == lobbyID && userId != ownID) { 
				string msg = Encoding.UTF8.GetString(data);

				string[] splitMsg = msg.Split(";");
				switch (splitMsg[0]) {
					case "createPlayer":
						long id = long.Parse(splitMsg[1]);
						Player newPlayer = JsonConvert.DeserializeObject<Player>(splitMsg[2]);
						if (!GameLoop.World.otherPlayers.ContainsKey(id)) {
							GameLoop.World.otherPlayers.Add(id, newPlayer);
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
						}
						break;
					case "hostFlags":
						bool farm = bool.Parse(splitMsg[1]); 
						HostOwnsFarm = farm;
						break;
					case "hostMap":
						string map = splitMsg[1];
						Map sentMap = JsonConvert.DeserializeObject<Map>(splitMsg[2]);

						if (map == "farm") {
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
						string permit = splitMsg[1];
						string username = splitMsg[2];

						if (permit == "farm") {
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
						Player player = JsonConvert.DeserializeObject<Player>(splitMsg[2]);
						if (!GameLoop.World.otherPlayers.ContainsKey(userId)) {
							GameLoop.World.otherPlayers.Add(userId, player);
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
						}

						GameLoop.UIManager.AddMsg(new ColoredString(player.Name + " connected!", Color.Orange, Color.Black));
						break;
					case "requestEntities":
						int requestX = Int32.Parse(splitMsg[1]);
						int requestY = Int32.Parse(splitMsg[2]);
						int requestZ = Int32.Parse(splitMsg[3]);

						Point3D requestPos = new(requestX, requestY, requestZ);

						if (!GameLoop.World.maps.ContainsKey(requestPos))
							GameLoop.World.LoadMapAt(requestPos);

						if (GameLoop.World.maps.ContainsKey(requestPos)) {
							if (!GameLoop.World.Player.VisitedMaps.Contains(requestPos)) {
								GameLoop.World.maps[requestPos].PopulateMonsters(requestPos);
								GameLoop.World.Player.VisitedMaps.Add(requestPos);
                            }

							foreach (Entity ent in GameLoop.World.maps[requestPos].Entities.Items) {
								string entityMsg = "";
								if (ent is Monster monEntity) {
									entityMsg = "spawnMonster;" + JsonConvert.SerializeObject(monEntity, Formatting.Indented);
								}

								if (ent is ItemWrapper itemEntity) {
									entityMsg = "spawnItem;" + JsonConvert.SerializeObject(itemEntity, Formatting.Indented);
								}

								if (entityMsg != "") { 
									lobbyManager.SendNetworkMessage(lobbyID, userId, 0, Encoding.UTF8.GetBytes(entityMsg));
								}
							}
						}
						break;
					case "yourID":
						ownID = long.Parse(splitMsg[1]);
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
						GameLoop.World.Player.Clock = JsonConvert.DeserializeObject<TimeManager>(splitMsg[1]);
						break;
					case "moveNPC":
						int NPCid = Int32.Parse(splitMsg[1]);
						int posX = Int32.Parse(splitMsg[2]);
						int posY = Int32.Parse(splitMsg[3]);
						int mapX = Int32.Parse(splitMsg[4]);
						int mapY = Int32.Parse(splitMsg[5]);
						int mapZ = Int32.Parse(splitMsg[6]);

						if (GameLoop.World.npcLibrary.ContainsKey(NPCid)) {
							GameLoop.World.npcLibrary[NPCid].MoveTo(new Point(posX, posY), new Point3D(mapX, mapY, mapZ));
                        }
						break;
					case "moveMonster":
						string monMoveID = splitMsg[1];
						int monMoveX = Int32.Parse(splitMsg[2]);
						int monMoveY = Int32.Parse(splitMsg[3]);
						int monMapX = Int32.Parse(splitMsg[4]);
						int monMapY = Int32.Parse(splitMsg[5]);
						int monMapZ = Int32.Parse(splitMsg[6]);
						 
						Point3D monMoveMap = new(monMapX, monMapY, monMapZ);
						Point monNewPos = new(monMoveX, monMoveY);

						CommandManager.MoveMonster(monMoveID, monMoveMap, monNewPos);
						break;
					case "damageMonster":
						string hitId = splitMsg[1];
						int monX = Int32.Parse(splitMsg[2]);
						int monY = Int32.Parse(splitMsg[3]);
						int monZ = Int32.Parse(splitMsg[4]);
						int dmgDealt = Int32.Parse(splitMsg[5]);
						string hitString = splitMsg[6];
						string hitColor = splitMsg[7];

						CommandManager.DamageMonster(hitId, new Point3D(monX, monY, monZ), dmgDealt, hitString, hitColor);
						break;
					case "damagePlayer":
						long playerID = long.Parse(splitMsg[1]); 
						int dmgTaken = Int32.Parse(splitMsg[2]);
						string playerHitString = splitMsg[3];
						string playerHitColor = splitMsg[4];

						CommandManager.DamagePlayer(playerID, dmgTaken, playerHitString, playerHitColor);
						break;
					case "updateTile":
						int tilePosX = Int32.Parse(splitMsg[1]);
						int tilePosY = Int32.Parse(splitMsg[2]);
						int tileMapX = Int32.Parse(splitMsg[3]);
						int tileMapY = Int32.Parse(splitMsg[4]);
						int tileMapZ = Int32.Parse(splitMsg[5]);
						TileBase tile = JsonConvert.DeserializeObject<TileBase>(splitMsg[6]);

						Point3D tileMapPos = new(tileMapX, tileMapY, tileMapZ);
						Point tilePos = new(tilePosX, tilePosY);

						if (!GameLoop.World.maps.ContainsKey(tileMapPos))
							GameLoop.World.LoadMapAt(tileMapPos);

						GameLoop.World.maps[tileMapPos].SetTile(tilePos, tile);
						GameLoop.World.maps[tileMapPos].GetTile(tilePos).UpdateAppearance();
						if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(new GoRogue.Coord(tilePos.X, tilePos.Y))) {
							GameLoop.World.maps[tileMapPos].GetTile(tilePos).Unshade();
						} else {
							GameLoop.World.maps[tileMapPos].GetTile(tilePos).Shade(); 
						} 
						break;
					case "updateMine":
						string Loc = splitMsg[1];
						int Depth = Int32.Parse(splitMsg[2]);
						int mineTilePosX = Int32.Parse(splitMsg[3]);
						int mineTilePosY = Int32.Parse(splitMsg[4]);
						MineTile mineTile = JsonConvert.DeserializeObject<MineTile>(splitMsg[5]);
						 
						Point mineTilePos = new(mineTilePosX, mineTilePosY);
						
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
						Item spawnItem = JsonConvert.DeserializeObject<Item>(splitMsg[1]);
						ItemWrapper wrap = new(spawnItem);
						CommandManager.SpawnItem(wrap);
						break;
					case "updateSkill":
						long skillID = long.Parse(splitMsg[1]); 
						string skillName = splitMsg[2];
						int skillLevel = Int32.Parse(splitMsg[3]);

						if (GameLoop.World.otherPlayers.ContainsKey(skillID)) {
							if (GameLoop.World.otherPlayers[skillID].Skills.ContainsKey(skillName)) {
								GameLoop.World.otherPlayers[skillID].Skills[skillName].Level = skillLevel;
                            }
                        }

						break;
					case "spawnMonster":
						Monster spawnMonster = JsonConvert.DeserializeObject<Monster>(splitMsg[1]);
						CommandManager.SpawnMonster(spawnMonster);
						break;
					case "disconnectedPlayer":
						long disconnectID = long.Parse(splitMsg[1]);
						if (GameLoop.World.otherPlayers.ContainsKey(disconnectID)) {
							GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[disconnectID].Name + " disconnected.", Color.Orange, Color.Black));
							GameLoop.World.otherPlayers.Remove(disconnectID);
							GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
						}
						break;
					case "destroyItem":
						Item destroyItem = JsonConvert.DeserializeObject<Item>(splitMsg[1]);
						ItemWrapper destroyWrap = new(destroyItem);
						CommandManager.DestroyItem(destroyWrap);
						break;
					case "movePlayer":
						long moveID = long.Parse(splitMsg[1]);
						int x = Int32.Parse(splitMsg[2]);
						int y = Int32.Parse(splitMsg[3]);
						int mx = Int32.Parse(splitMsg[4]);
						int my = Int32.Parse(splitMsg[5]);
						int mz = Int32.Parse(splitMsg[6]);
						GameLoop.World.otherPlayers[moveID].MoveTo(new Point(x, y), new Point3D(mx, my, mz));

						GameLoop.UIManager.Map.UpdateVision(); 
						break;
					case "updatePlayerMine":
						long mineID = long.Parse(splitMsg[1]);
						int mineX = Int32.Parse(splitMsg[2]);
						int mineY = Int32.Parse(splitMsg[3]);
						string mineLoc = splitMsg[4];
						int mineDepth = Int32.Parse(splitMsg[5]);
						GameLoop.World.otherPlayers[mineID].Position = new Point(mineX, mineY);
						GameLoop.World.otherPlayers[mineID].MineLocation = mineLoc;
						GameLoop.World.otherPlayers[mineID].MineDepth = mineDepth;
						break;
					case "updateChest":
						int chestX = Int32.Parse(splitMsg[1]);
						int chestY = Int32.Parse(splitMsg[2]);
						int chestMX = Int32.Parse(splitMsg[3]);
						int chestMY = Int32.Parse(splitMsg[4]);
						int chestMZ = Int32.Parse(splitMsg[5]);

						Point3D chestMap = new Point3D(chestMX, chestMY, chestMZ);
						Point chestPos = new Point(chestX, chestY);

						Container chestContents = JsonConvert.DeserializeObject<Container>(splitMsg[6]);

						if (!GameLoop.World.maps.ContainsKey(chestMap))
							GameLoop.World.LoadMapAt(chestMap);

						if (GameLoop.World.maps[chestMap].GetTile(chestPos).Container != null)
							GameLoop.World.maps[chestMap].GetTile(chestPos).Container = chestContents;

						if (GameLoop.World.Player.MapPos == chestMap && chestPos == GameLoop.UIManager.Container.ContainerPosition) {
							GameLoop.UIManager.Container.CurrentContainer = GameLoop.World.maps[chestMap].GetTile(chestPos).Container; 
						}
						break;
					case "requestMine":
						if (isHost) {
							string mineName = splitMsg[1];
							int mineLevel = Int32.Parse(splitMsg[2]);

							string mineSend = "fullMine;" + mineName + ";" + mineLevel + ";";

							if (mineName == "Mountain") {
								mineSend += JsonConvert.SerializeObject(GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[mineLevel], Formatting.Indented);
							} else if (mineName == "Lake") {
								mineSend += JsonConvert.SerializeObject(GameLoop.UIManager.Minigames.MineManager.LakeMine, Formatting.Indented);
							}

							lobbyManager.SendNetworkMessage(lobbyID, userId, 0, Encoding.UTF8.GetBytes(mineSend));
						}
						break;
					case "fullMine":
						if (!isHost) {
							string mineName = splitMsg[1];
							int mineLevel = Int32.Parse(splitMsg[2]);

							MineLevel sentMine = JsonConvert.DeserializeObject<MineLevel>(splitMsg[3]);

							if (mineName == "Mountain") {
								if (GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.ContainsKey(mineLevel)) {
									GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[mineLevel] = sentMine; 
								} else {
									GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels.Add(mineLevel, sentMine);
								}
								GameLoop.UIManager.Minigames.ToggleMinigame();
								GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(GameLoop.UIManager.Minigames.MineManager.MountainMine.Levels[GameLoop.World.Player.MineDepth].MapFOV);
							} else if (mineName == "Lake") {
								if (GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.ContainsKey(mineLevel)) {
									GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[mineLevel] = sentMine;
								} else {
									GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels.Add(mineLevel, sentMine);
								}
								GameLoop.UIManager.Minigames.ToggleMinigame();
								GameLoop.UIManager.Minigames.MineManager.MiningFOV = new GoRogue.FOV(GameLoop.UIManager.Minigames.MineManager.LakeMine.Levels[GameLoop.World.Player.MineDepth].MapFOV);
							}
						}

						break;
					case "newDay":
						bool passedOut = Boolean.Parse(splitMsg[1]);
						if (passedOut)
							GameLoop.UIManager.AddMsg(new ColoredString("You passed out!", Color.Red, Color.Black));
						else
							GameLoop.UIManager.AddMsg(new ColoredString("You sleep through the night.", Color.Lime, Color.Black));

						GameLoop.World.Player.Sleeping = false;
						GameLoop.World.Player.CurrentHP = GameLoop.World.Player.MaxHP;

						foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
							kv.Value.CurrentHP = kv.Value.MaxHP;
							kv.Value.Sleeping = false;
                        }

						GameLoop.World.SavePlayer();
						break;
					case "sleep":
						long sleepID = long.Parse(splitMsg[1]);
						bool sleeping = Boolean.Parse(splitMsg[2]);

						if (GameLoop.World.otherPlayers.ContainsKey(sleepID)) {
							GameLoop.World.otherPlayers[sleepID].Sleeping = sleeping;

							int sleepCount = 0;
							int totalPlayers = 1;
							if (GameLoop.World.Player.Sleeping)
								sleepCount++;

							foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
								totalPlayers++;
								if (kv.Value.Sleeping)
									sleepCount++;
							}

							if (sleeping) {
								GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[sleepID].Name + " went to sleep. (" + sleepCount + "/" + totalPlayers + ")", Color.Lime, Color.Black));
							} else {
								GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[sleepID].Name + " decided not to sleep. (" + sleepCount + "/" + totalPlayers + ")", Color.Lime, Color.Black));
							}
						}
						break;
					default:
						GameLoop.UIManager.AddMsg(msg);
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
					GameLoop.UIManager.AddMsg(new ColoredString(GameLoop.World.otherPlayers[userId].Name + " disconnected.", Color.Orange, Color.Black));
					GameLoop.World.otherPlayers.Remove(userId);

					string disconnect = "disconnectedPlayer;" + userId;
					BroadcastMsg(disconnect);

					GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
                }
			}
		}

        private void MemberConnected(long lobbyId, long userId) {
			if (lobbyId == lobbyID) {
				foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
					string json = JsonConvert.SerializeObject(kv.Value, Formatting.Indented);
					string msg = "createPlayer;" + kv.Key + ";" + json;
					lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(msg));
				}

				string connectedID = "yourID;" + userId;
				lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(connectedID));
				 
				string ownjson = JsonConvert.SerializeObject(GameLoop.World.Player, Formatting.Indented);
				string ownmsg = "createPlayer;" + ownID + ";" + ownjson;
				lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(ownmsg));

				string hostFlags = "hostFlags;" + GameLoop.World.Player.OwnsFarm;
				lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(hostFlags));

				if (GameLoop.World.Player.OwnsFarm) {
					if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
						GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

					Map normalFarm = GameLoop.World.UnchangedMap(new Point3D(-1, 0, 0));

					for (int i = 0; i < GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles.Length; i++) {
						if (GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i] != normalFarm.Tiles[i]) {
							string json = JsonConvert.SerializeObject(GameLoop.World.maps[new Point3D(-1, 0, 0)].Tiles[i], Formatting.Indented);
							GameLoop.SendMessageIfNeeded(new string[] { "updateTile", (i % GameLoop.MapWidth).ToString(), (i / GameLoop.MapWidth).ToString(), "-1;0;0", json }, false, false);
						}
                    }

				//	string hostFarm = "hostMap;farm;" + JsonConvert.SerializeObject(GameLoop.World.maps[new Point3D(-1, 0, 0)], Formatting.Indented);
				//	lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(hostFarm));
				}

				foreach (KeyValuePair<int, NPC> kv in GameLoop.World.npcLibrary) {
					string msg = "moveNPC;" + kv.Value.npcID + ";" + kv.Value.Position.X + ";" + kv.Value.Position.Y + ";" + kv.Value.MapPos.X + ";" + kv.Value.MapPos.Y + ";" + kv.Value.MapPos.Z;
					lobbyManager.SendNetworkMessage(lobbyId, userId, 0, Encoding.UTF8.GetBytes(msg));
				}

				GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[GameLoop.World.Player.MapPos]);
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
								 
								string jsonString = JsonConvert.SerializeObject(GameLoop.World.Player, Formatting.Indented);
								BroadcastMsg("registerPlayer;" + ";" + jsonString);

								GameLoop.UIManager.clientAndConnected = false;
								FoundLobby = true;

								string initialRequest = "requestEntities;" + GameLoop.World.Player.MapPos.X + ";" + GameLoop.World.Player.MapPos.Y + ";" + GameLoop.World.Player.MapPos.Z;
								GameLoop.World.maps[GameLoop.World.Player.MapPos].Entities.Clear();
								lobbyManager.SendNetworkMessage(lobby.Id, lobby.OwnerId, 0, Encoding.UTF8.GetBytes(initialRequest));
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
