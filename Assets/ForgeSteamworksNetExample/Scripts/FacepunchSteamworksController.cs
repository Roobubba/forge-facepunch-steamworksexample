using System;
using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine.SceneManagement;

public class FacepunchSteamworksController : MonoBehaviour
{
	private static Dictionary<ulong, SteamId> allFriends = new Dictionary<ulong, SteamId>();
	private static BaseFacepunchP2P networker;

	private void Awake()
	{
		DontDestroyOnLoad(this);
	}

	private void Start()
	{
		try
		{
			SteamClient.Init(480);
			Invoke("LoadMenu", 1.0f);
		}
		catch (Exception e)
		{
			BeardedManStudios.Forge.Logging.BMSLog.LogException("Steam Client could not be initialized");
			throw e;
		}
	}

	private void LoadMenu()
	{
		SceneManager.LoadScene(1);
	}

	public static void SetNetworker(BaseFacepunchP2P networker)
	{
		FacepunchSteamworksController.networker = networker;
	}

	public static SteamId GetFriend(ulong steamId)
	{
		var friend = default(SteamId);
		if (!allFriends.TryGetValue(steamId, out friend))
		{
			if (steamId == SteamClient.SteamId.Value)
			{
				friend = SteamClient.SteamId;
				allFriends.Add(steamId, friend);
			}

			foreach (var steamFriend in SteamFriends.GetFriends())
			{
				var temp2 = default(SteamId);
				if (!allFriends.TryGetValue(steamFriend.Id.Value, out temp2))
				{
					allFriends.Add(steamFriend.Id.Value, steamFriend.Id);
				}
				if (steamFriend.Id.Value == steamId)
				{
					return steamFriend.Id;
				}
			}
			if (networker != null)
			{
				var lobby = networker.Lobby;
				foreach (var person in lobby.Members)
				{
					var temp = default(SteamId);
					if (!allFriends.TryGetValue(person.Id.Value, out temp))
					{
						allFriends.Add(person.Id.Value, person.Id);
					}
					if (person.Id.Value == steamId)
					{
						return person.Id;
					}
				}
			}
		}
		return friend;
	}

	private void OnApplicationQuit()
	{
		SteamClient.Shutdown();
	}

	private void OnDestroy()
	{
		SteamClient.Shutdown();
	}
}
