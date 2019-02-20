using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;

namespace download_yts_full {
	class Program {
		// Folder that contains all the working data files for the application
		static string BaseFolder = @"c:\working\yts_base\";
		
		// YTS parser and controller
		// Created after initializing the App class in Main()
		static YTS Yts = null;

		//static string DiagnosticNow = @"2019-01-01_11-41-23-1358";

		// Which YTS browse-movies to start on for scraping
		static int StartOnPage = 1;
		
		static void Main(string[] args) {
			// Init the App class for services and log the start
			App.Initialize(BaseFolder);
			App.Log("START");

			// Init YTS now that App is available
			Yts = new YTS();



			// What action?

			DownloadYTS();
			//DownloadTypes();






			App.End();
		}

		static void DownloadYTS() {
			WebClient web = new WebClient();

			string startName = String.Format(StartFileName, App.Now());
			App.Log("start file - " + startName);
			web.DownloadFile(StartURL, startName);
			App.Wait();

			string text = File.ReadAllText(startName);
			MatchCollection startMatches = lastRegex.Matches(text);
			App.Log("start match count - " + startMatches.Count);

			if (startMatches.Count == 0) {
				App.Error("No last link");
			} else {
				int lastPage = int.Parse(startMatches[0].Groups[1].Value);
				App.Log("last number - " + lastPage);

				string lastName = String.Format(ListFileName, lastPage);
				App.Log("last file - " + lastName);
				web.DownloadFile(StartURL, lastName);
				App.Wait();

				text = File.ReadAllText(lastName);
				MatchCollection lastMatches = lastRegex.Matches(text);
				App.Log("last match count - " + lastMatches.Count);

				if (lastMatches.Count == 0) {
					App.Error("No last matches");
				} else {
					int total = ((lastPage - 1) * PerPage) + lastMatches.Count;
					App.Log("total - " + total);
					int current = total - ((StartOnPage - 1) * PerPage);
					App.Log("start with - " + current);

					for (int i = StartOnPage; i < lastPage; i++) {// 2; i++) {
						string listName = String.Format(ListFileName, i);
						string listUrl = String.Format(ListURL, i);
						App.Log("list file - " + listName);
						web.DownloadFile(listUrl, listName);
						App.Wait();

						text = File.ReadAllText(listName);
						MatchCollection listMatches = listRegex.Matches(text);
						App.Log("list match count - " + listMatches.Count);

						if (listMatches.Count == 0) {
							App.Error("No list matches");
						} else {
							for (int j = 0; j < listMatches.Count; j++) {
								bool go = true;
								string pageUrl = listMatches[j].Groups[1].Value;
								App.Log("page url - " + pageUrl);

								string name = filenameRegex.Replace(listMatches[j].Groups[2].Value, "-");
								string year = listMatches[j].Groups[3].Value;
								string title = name + " (" + year + ")";

								string pageName = String.Format(PageFileName, current, title);
								App.Log("page name - " + pageName);
								try {
									web.DownloadFile(pageUrl, pageName);
								} catch (Exception ex) {
									App.Error("bad fetch page - " + pageName + " - " + pageUrl);
									go = false;
								}

								if (go) {
									App.Wait();

									text = File.ReadAllText(pageName);
									MatchCollection pageMatches = pageRegex.Matches(text);
									App.Log("page match count - " + pageMatches.Count);

									if (pageMatches.Count == 0) {
										App.Error("No page matches - " + pageName + " - " + pageUrl);
									} else {
										foreach (Match m in pageMatches) {
											for (int uu = 0; uu < m.Groups[1].Captures.Count; uu++) {
												string torrentUrl = m.Groups[1].Captures[uu].Value;

												string torrentRType = m.Groups[2].Captures[uu].Value;
												string torrentName = String.Format(TorrentFileName, i, current, title, torrentRType);

												App.Log("torrent file - " + torrentName + " - " + torrentUrl);

												try {
													web.DownloadFile(torrentUrl, torrentName);
												} catch (Exception ex) {
													App.Error("bad fetch torrent - " + torrentName + " - " + torrentUrl + " - " + ex.ToString());
												}

												try {
													string hash = App.FileToMD5(torrentName);
													string otherName = String.Format(TorrentFileNameSecond, title, torrentRType, hash);

													File.Move(torrentName, otherName);
												} catch (Exception ex) {
													App.Error("error changing md5 name - " + torrentName + " - " + torrentUrl + " - " + ex.ToString());
												}



												App.Wait();
											}
										}
									}
								}

								current--;
							}
						}
					}
				}
			}
		}

		/*
		static void DownloadTypes() {

			string dir = Path.GetDirectoryName(PageFileName.Replace("{now}", NowString));
			FixFileNames();
			Directories();
			App.Log("START");
			Dictionary<string, int> counts = new Dictionary<string, int>();

			var pageNames = Directory.EnumerateFiles(dir);
			App.Log("page count - " + pageNames.Count());
			foreach (var pageName in pageNames) {
				App.Log("page - " + pageName);
				string content = File.ReadAllText(pageName);
				MatchCollection pageMatches = pageRegex.Matches(content);
				App.Log("page match count - " + pageMatches.Count);

				if (pageMatches.Count == 0) {
					Error("No page matches - " + pageName);
				} else {
					//for (pageMatches[0].Groups) {
					string title = "";
					//}
				}

			}
		}
		*/
	}
}

