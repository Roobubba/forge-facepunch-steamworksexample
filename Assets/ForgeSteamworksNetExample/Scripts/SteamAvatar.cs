using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace ForgeSteamworksNETExample
{
	public enum AvatarSize
	{
		Small,
		Medium,
		Large
	}

	public class SteamAvatar : MonoBehaviour
	{
		/// <summary>
		/// The Steam avatar image if any
		/// </summary>
		public RawImage avatarImage;

		/// <summary>
		/// The local user's steam name
		/// </summary>
		public TMP_Text personaName;

		[SerializeField]
		private Texture2D fallbackImage;

		private void Awake()
		{
			if (avatarImage == null)
			{
				avatarImage = GetComponentInChildren<RawImage>();
			}
		}

		public void LoadTextureFromImage(Steamworks.Data.Image img)
		{
			var texture = new Texture2D((int)img.Width, (int)img.Height);

			for (int x = 0; x < img.Width; x++)
				for (int y = 0; y < img.Height; y++)
				{
					var p = img.GetPixel(x, y);

					texture.SetPixel(x, (int)img.Height - y, new Color32(p.r, p.g, p.b, p.a));
				}

			texture.Apply();

			ApplyTexture(texture);
		}

		public virtual void ApplyTexture(Texture2D texture)
		{
			var rawImage = avatarImage;
			if (rawImage != null)
			{
				rawImage.texture = texture;
			}
		}

		/// <summary>
		/// Initialize the steam avatar display.
		/// </summary>
		/// <param name="steamId">The <see cref="SteamId"/> of the user who's information to get</param>
		/// <param name="size">The <see cref="AvatarSize"/> of the image to get</param>
		public void InitializeAvatar(SteamId steamId, string name, AvatarSize size = AvatarSize.Medium)
		{
			personaName.text = name;
			GetSteamImageAsync(steamId, size);
		}

		private async void GetSteamImageAsync(SteamId steamId, AvatarSize size)
		{
			await GetSteamImage(steamId, size);
		}

		private async Task GetSteamImage(SteamId steamId, AvatarSize size)
		{
			Steamworks.Data.Image? image;
			switch (size)
			{
				case AvatarSize.Small:
					image = await SteamFriends.GetSmallAvatarAsync(steamId);
					break;
				case AvatarSize.Medium:
					image = await SteamFriends.GetMediumAvatarAsync(steamId);
					break;
				case AvatarSize.Large:
					image = await SteamFriends.GetLargeAvatarAsync(steamId);
					break;
				default:
					throw new ArgumentException("Unknown Steam Avatar size!");
			}
			if (image.HasValue)
			{
				LoadTextureFromImage(image.Value);
			}
		}
	}
}
