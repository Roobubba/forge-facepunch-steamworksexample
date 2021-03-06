﻿using System;
using BeardedManStudios.Forge.Logging;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ForgeSteamworksNETExample.Player
{
	public class NetworkedPlayer : PlayerBehavior
	{
		[Serializable]
		public class ToggleEvent : UnityEvent<bool>{}

		/// <summary>
		/// Event triggered when a SetupPlayer RPC is called on this object.
		/// This is the place where other scripts can hook into the RPC call handling
		/// </summary>
		public event System.Action<RpcArgs> SetupPlayerEvent;

		/// <summary>
		/// Custom event for other scripts to hook into that need to run code when the
		/// NetworkStart event on the Owner object is called
		/// </summary>
		public event System.Action NetworkStartEvent;

		/// <summary>
		/// Scripts that should be enabled for the local client
		/// </summary>
		[SerializeField]
		private ToggleEvent localScripts;

		[SerializeField]
		private InputManager inputManager;

		[SerializeField]
		private Rigidbody baseBody;

		private Animator baseAnimator;

		private SteamId steamId;

		private void Awake()
		{
			baseBody = GetComponent<Rigidbody>();
			baseAnimator = GetComponent<Animator>();
			inputManager = GetComponent<InputManager>();
		}

		private void FixedUpdate()
		{
			if (networkObject.IsOwner)
			{
				networkObject.position = baseBody.position;
				networkObject.rotation = baseBody.rotation;
				networkObject.moveAmount = inputManager.MoveAmount;
			}
			else
			{
				baseBody.position = networkObject.position;
				baseBody.rotation = networkObject.rotation;
				baseAnimator.SetFloat("vertical", networkObject.moveAmount);
			}
		}

		protected override void NetworkStart()
		{
			base.NetworkStart();

			// Enable all the scripts that are needed by the local client
			localScripts.Invoke(networkObject.IsOwner);

			if (networkObject.IsOwner)
			{
				if (NetworkStartEvent != null)
					NetworkStartEvent();
			}

			networkObject.rotation = Quaternion.identity;
			networkObject.rotationInterpolation.target = Quaternion.identity;
			networkObject.SnapInterpolations();

			if (NetworkManager.Instance.Networker is IServer)
			{
				// You can do server specific initialization here
			}
			else
			{
				NetworkManager.Instance.Networker.disconnected += OnDisconnect;
			}
		}

		private void OnDisconnect(NetWorker sender)
		{


			MainThreadManager.Run(() =>
			{
				foreach (var netObject in sender.NetworkObjectList)
				{
					if (netObject.Owner.IsHost)
					{
						BMSLog.Log("Server disconnected");
						// Go back to the multiplayer menu
						SceneManager.LoadScene(1);
					}
				}
			});

			if (NetworkManager.Instance == null)
				return;

			NetworkManager.Instance.Networker.disconnected -= OnDisconnect;
			NetworkManager.Instance.Disconnect();
		}

		/// <summary>
		/// Set the <see cref="SteamId"/> of this player
		/// </summary>
		/// <param name="steamId"></param>
		public void SetSteamId(ulong steamId)
		{
			this.steamId = (SteamId) steamId;
		}

		/// <summary>
		/// Handle the SetupPlayer RPC call
		/// </summary>
		/// <param name="args"></param>
		public override void SetupPlayer(RpcArgs args)
		{
			if (SetupPlayerEvent != null)
				SetupPlayerEvent(args);
		}
	}
}
