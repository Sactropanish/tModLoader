using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.IO;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ModLoader.UI
{
	internal class UIModBrowser : UIState
	{
		public UIList modList;
		public UIList modListAll;
		public UIModDownloadItem selectedItem;
		public UITextPanel<string> uITextPanel;
		UIInputTextField filterTextBox;
		private List<UICycleImage> _categoryButtons = new List<UICycleImage>();
		public bool loaded = false;
		public SortModes sortMode = SortModes.RecentlyUpdated;
		public UpdateFilter updateFilterMode = UpdateFilter.Available;
		public SearchFilter searchFilterMode = SearchFilter.Name;
		internal string filter;
		private bool updateAvailable;
		private string updateText;
		private string updateURL;
		public bool aModUpdated = false;
		public bool aNewModDownloaded = false;

		public override void OnInitialize()
		{
			UIElement uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;
			UIPanel uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIPanel.PaddingTop = 0f;
			uIElement.Append(uIPanel);
			modListAll = new UIList();
			modList = new UIList();
			modList.Width.Set(-25f, 1f);
			modList.Height.Set(-50f, 1f);
			modList.Top.Set(50f, 0f);
			modList.ListPadding = 5f;
			uIPanel.Append(modList);
			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-50f, 1f);
			uIScrollbar.Top.Set(50f, 0f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			modList.SetScrollbar(uIScrollbar);
			uITextPanel = new UITextPanel<string>("Mod Browser", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			UITextPanel<string> button = new UITextPanel<string>("Reload List", 1f, false);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(25f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-65f, 0f);
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(ReloadList);
			uIElement.Append(button);
			UITextPanel<string> button3 = new UITextPanel<string>("Back", 1f, false);
			button3.Width.Set(-10f, 0.5f);
			button3.Height.Set(25f, 0f);
			button3.VAlign = 1f;
			button3.Top.Set(-20f, 0f);
			button3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button3.OnClick += new UIElement.MouseEvent(BackClick);
			uIElement.Append(button3);
			base.Append(uIElement);
			UIElement uIElement2 = new UIElement();
			uIElement2.Width.Set(0f, 1f);
			uIElement2.Height.Set(32f, 0f);
			uIElement2.Top.Set(10f, 0f);
			Texture2D texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));
			UICycleImage uIToggleImage;
			for (int j = 0; j < 2; j++)
			{
				if (j == 0)
				{
					uIToggleImage = new UICycleImage(texture, 5, 32, 32, 0, 0);
					uIToggleImage.setCurrentState((int)sortMode);
					uIToggleImage.OnClick += (a, b) => Interface.modBrowser.sortMode = sortMode.Next();
					uIToggleImage.OnClick += new UIElement.MouseEvent(this.SortList);
				}
				else
				{
					uIToggleImage = new UICycleImage(texture, 3, 32, 32, 34, 0);
					uIToggleImage.setCurrentState((int)updateFilterMode);
					uIToggleImage.OnClick += (a, b) => Interface.modBrowser.updateFilterMode = updateFilterMode.Next();
					uIToggleImage.OnClick += new UIElement.MouseEvent(this.SortList);
				}
				uIToggleImage.Left.Set((float)(j * 36 + 8), 0f);
				_categoryButtons.Add(uIToggleImage);
				uIElement2.Append(uIToggleImage);
			}
			filterTextBox = new UIInputTextField("Type to search");
			filterTextBox.Top.Set(5, 0f);
			filterTextBox.Left.Set(-150, 1f);
			filterTextBox.OnTextChange += new UIInputTextField.EventHandler(SortList);
			uIElement2.Append(filterTextBox);
			UICycleImage SearchFilterToggle = new UICycleImage(texture, 2, 32, 32, 68, 0);
			SearchFilterToggle.setCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) => Interface.modBrowser.searchFilterMode = searchFilterMode.Next();
			SearchFilterToggle.OnClick += new UIElement.MouseEvent(this.SortList);
			SearchFilterToggle.Left.Set(545f, 0f);
			_categoryButtons.Add(SearchFilterToggle);
			uIElement2.Append(SearchFilterToggle);
			uIPanel.Append(uIElement2);
		}

		private void SortList(object sender, EventArgs e)
		{
			SortList();
		}

		private void SortList(UIMouseEvent evt, UIElement listeningElement)
		{
			SortList();
		}

		private void SortList()
		{
			filter = filterTextBox.currentString;
			modList.Clear();
			foreach (UIModDownloadItem item in modListAll._items.Where(item => item.PassFilters()))
			{
				modList.Add(item);
			}
			modList.UpdateOrder();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			for (int i = 0; i < this._categoryButtons.Count; i++)
			{
				if (this._categoryButtons[i].IsMouseHovering)
				{
					string text;
					switch (i)
					{
						case 0:
							text = Interface.modBrowser.sortMode.ToFriendlyString();
							break;
						case 1:
							text = Interface.modBrowser.updateFilterMode.ToFriendlyString();
							break;
						case 2:
							text = Interface.modBrowser.searchFilterMode.ToFriendlyString();
							break;
						default:
							text = "None";
							break;
					}
					float x = Main.fontMouseText.MeasureString(text).X;
					Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
					if (vector.Y > (float)(Main.screenHeight - 30))
					{
						vector.Y = (float)(Main.screenHeight - 30);
					}
					if (vector.X > (float)Main.screenWidth - x)
					{
						vector.X = (float)(Main.screenWidth - 460);
					}
					Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
					return;
				}
			}
			if (updateAvailable)
			{
				updateAvailable = false;
				Interface.updateMessage.SetMessage(updateText);
				Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
				Interface.updateMessage.SetURL(updateURL);
				Main.menuMode = Interface.updateMessageID;
			}
		}

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
			if (Interface.modBrowser.aModUpdated)
			{
				Interface.infoMessage.SetMessage("You have updated a mod. Remember to reload mods for it to take effect.");
				Interface.infoMessage.SetGotoMenu(0);
				Main.menuMode = Interface.infoMessageID;
			}
			else if (Interface.modBrowser.aNewModDownloaded)
			{
				Interface.infoMessage.SetMessage("Your recently downloaded mods are currently disabled. Remember to enable and reload if you intend to use them.");
				Interface.infoMessage.SetGotoMenu(0);
				Main.menuMode = Interface.infoMessageID;
			}
			Interface.modBrowser.aModUpdated = false;
			Interface.modBrowser.aNewModDownloaded = false;
		}

		private static void ReloadList(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Interface.modBrowser.loaded = false;
			Main.menuMode = Interface.modBrowserID;
		}

		public override void OnActivate()
		{
			Main.clrInput();
			if (!loaded)
			{
				uITextPanel.SetText("Mod Browser", 0.8f, true);
				modListAll.Clear();
				try
				{
					ServicePointManager.Expect100Continue = false;
					string url = "http://javid.ddns.net/tModLoader/listmods.php";
					var values = new NameValueCollection
					{
						{ "modloaderversion", ModLoader.versionedName },
					};
					using (WebClient client = new WebClient())
					{
						ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
						client.UploadValuesCompleted += new UploadValuesCompletedEventHandler(UploadComplete);
						client.UploadValuesAsync(new Uri(url), "POST", values);
					}
				}
				catch (WebException e)
				{
					if (e.Status == WebExceptionStatus.Timeout)
					{
						uITextPanel.SetText("Mod Browser OFFLINE (Busy)", 0.8f, true);
						return;
					}
					if (e.Status == WebExceptionStatus.ProtocolError)
					{
						var resp = (HttpWebResponse)e.Response;
						if (resp.StatusCode == HttpStatusCode.NotFound)
						{
							uITextPanel.SetText("Mod Browser OFFLINE (404)", 0.8f, true);
							return;
						}
						uITextPanel.SetText("Mod Browser OFFLINE..", 0.8f, true);
						return;
					}
				}
				catch (Exception e)
				{
					ErrorLogger.LogModBrowserException(e);
					return;
				}
			}
		}

		public async void UploadComplete(Object sender, UploadValuesCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				if (e.Cancelled)
				{
				}
				else
				{
					HttpStatusCode httpStatusCode = GetHttpStatusCode(e.Error);
					if (httpStatusCode == HttpStatusCode.ServiceUnavailable)
					{
						uITextPanel.SetText("Mod Browser OFFLINE (Busy)", 0.8f, true);
					}
					else
					{
						uITextPanel.SetText("Mod Browser OFFLINE (Unknown)", 0.8f, true);
					}
				}
			}
			else if (!e.Cancelled)
			{
				XmlDocument xmlDoc = new XmlDocument();
				byte[] result = e.Result;
				xmlDoc.LoadXml(Encoding.UTF8.GetString(result, 0, result.Length));
				List<BuildProperties> modBuildProperties = await Task.Run(() =>
					ModLoader.FindMods().Select(BuildProperties.ReadModFile).ToList());
				PopulateFromXML(modBuildProperties, xmlDoc);
				loaded = true;
			}
		}

		private void PopulateFromXML(List<BuildProperties> modBuildProperties, XmlDocument xmlDoc)
		{
			try
			{
				foreach (XmlNode xmlNode in xmlDoc.DocumentElement)
				{
					if (xmlNode.Name.Equals("update"))
					{
						updateAvailable = true;
						updateText = xmlNode.SelectSingleNode("message").InnerText;
						updateURL = xmlNode.SelectSingleNode("url").InnerText;
					}
					else if (xmlNode.Name.Equals("modlist"))
					{
						foreach (XmlNode xmlNode2 in xmlNode)
						{
							string displayname = xmlNode2.SelectSingleNode("displayname").InnerText;
							string name = xmlNode2.SelectSingleNode("name").InnerText;
							string version = xmlNode2.SelectSingleNode("version").InnerText;
							string author = xmlNode2.SelectSingleNode("author").InnerText;
							string description = xmlNode2.SelectSingleNode("description").InnerText;
							string homepage = xmlNode2.SelectSingleNode("homepage").InnerText;
							string download = xmlNode2.SelectSingleNode("download").InnerText;
							string timeStamp = xmlNode2.SelectSingleNode("updateTimeStamp").InnerText;
							int downloads;
							Int32.TryParse(xmlNode2.SelectSingleNode("downloads").InnerText, out downloads);
							bool exists = false;
							bool update = false;
							foreach (BuildProperties bp in modBuildProperties)
							{
								if (bp.displayName.Equals(displayname))
								{
									exists = true;
									if (!bp.version.Equals(new Version(version.Substring(1))))
									{
										update = true;
									}
								}
							}
							UIModDownloadItem modItem = new UIModDownloadItem(displayname, name, version, author, description, homepage, download, downloads, timeStamp, update, exists);
							modListAll.Add(modItem);
						}
						SortList(null, null);
					}
				}
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
		}

		public XmlDocument GetDataFromUrl(string url)
		{
			XmlDocument urlData = new XmlDocument();
			HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
			rq.Timeout = 5000;
			HttpWebResponse response = rq.GetResponse() as HttpWebResponse;
			using (Stream responseStream = response.GetResponseStream())
			{
				XmlTextReader reader = new XmlTextReader(responseStream);
				urlData.Load(reader);
			}
			return urlData;
		}

		HttpStatusCode GetHttpStatusCode(System.Exception err)
		{
			if (err is WebException)
			{
				WebException we = (WebException)err;
				if (we.Response is HttpWebResponse)
				{
					HttpWebResponse response = (HttpWebResponse)we.Response;
					return response.StatusCode;
				}
			}
			return 0;
		}
	}

	public static class SortModesExtensions
	{
		public static SortModes Next(this SortModes sortmode)
		{
			switch (sortmode)
			{
				case SortModes.DisplayNameAtoZ:
					return SortModes.DisplayNameZtoA;
				case SortModes.DisplayNameZtoA:
					return SortModes.DownloadsDescending;
				case SortModes.DownloadsDescending:
					return SortModes.DownloadsAscending;
				case SortModes.DownloadsAscending:
					return SortModes.RecentlyUpdated;
				case SortModes.RecentlyUpdated:
					return SortModes.DisplayNameAtoZ;
			}
			return SortModes.DisplayNameAtoZ;
		}

		public static string ToFriendlyString(this SortModes sortmode)
		{
			switch (sortmode)
			{
				case SortModes.DisplayNameAtoZ:
					return "Sort mod names alphabetically";
				case SortModes.DisplayNameZtoA:
					return "Sort mod names reverse-alphabetically";
				case SortModes.DownloadsDescending:
					return "Sort by downloads descending";
				case SortModes.DownloadsAscending:
					return "Sort by downloads ascending";
				case SortModes.RecentlyUpdated:
					return "Sort by recently updated";
			}
			return "Unknown Sort";
		}
	}

	public static class UpdateFilterModesExtensions
	{
		public static UpdateFilter Next(this UpdateFilter updateFilterMode)
		{
			switch (updateFilterMode)
			{
				case UpdateFilter.All:
					return UpdateFilter.Available;
				case UpdateFilter.Available:
					return UpdateFilter.UpdateOnly;
				case UpdateFilter.UpdateOnly:
					return UpdateFilter.All;
			}
			return UpdateFilter.All;
		}

		public static string ToFriendlyString(this UpdateFilter updateFilterMode)
		{
			switch (updateFilterMode)
			{
				case UpdateFilter.All:
					return "Show all mods";
				case UpdateFilter.Available:
					return "Show mods not installed and updates";
				case UpdateFilter.UpdateOnly:
					return "Show only updates";
			}
			return "Unknown Sort";
		}
	}

	public static class SearchFilterModesExtensions
	{
		public static SearchFilter Next(this SearchFilter searchFilterMode)
		{
			switch (searchFilterMode)
			{
				case SearchFilter.Name:
					return SearchFilter.Author;
				case SearchFilter.Author:
					return SearchFilter.Name;
			}
			return SearchFilter.Name;
		}

		public static string ToFriendlyString(this SearchFilter searchFilterMode)
		{
			switch (searchFilterMode)
			{
				case SearchFilter.Name:
					return "Search by Mod name";
				case SearchFilter.Author:
					return "Search by Author name";
			}
			return "Unknown Sort";
		}
	}

	public enum SortModes
	{
		DisplayNameAtoZ,
		DisplayNameZtoA,
		DownloadsDescending,
		DownloadsAscending,
		RecentlyUpdated,
	}

	public enum UpdateFilter
	{
		All,
		Available,
		UpdateOnly,
	}

	public enum SearchFilter
	{
		Name,
		Author,
	}
}
