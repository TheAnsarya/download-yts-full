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
		static string BaseFolder = @"c:\working\yts_base\";
		
		static YTS yts = null;

		//static string DiagnosticNow = @"2019-01-01_11-41-23-1358";


		static int StartOnPage = 1;
		
		static void Main(string[] args) {
			App.Initalize(Basefolder);
			Log("START");
			YTS = new YTS();

			DownloadYTS();
			//DownloadTypes();
		}

		static void DownloadYTS() {
			WebClient web = new WebClient();

			string startName = String.Format(StartFileName, Now());
			Log("start file - " + startName);
			web.DownloadFile(StartURL, startName);
			Wait();

			string text = File.ReadAllText(startName);
			MatchCollection startMatches = lastRegex.Matches(text);
			Log("start match count - " + startMatches.Count);

			if (startMatches.Count == 0) {
				Error("No last link");
			} else {
				int lastPage = int.Parse(startMatches[0].Groups[1].Value);
				Log("last number - " + lastPage);

				string lastName = String.Format(ListFileName, lastPage);
				Log("last file - " + lastName);
				web.DownloadFile(StartURL, lastName);
				Wait();

				text = File.ReadAllText(lastName);
				MatchCollection lastMatches = lastRegex.Matches(text);
				Log("last match count - " + lastMatches.Count);

				if (lastMatches.Count == 0) {
					Error("No last matches");
				} else {
					int total = ((lastPage - 1) * PerPage) + lastMatches.Count;
					Log("total - " + total);
					int current = total - ((StartOnPage - 1) * PerPage);
					Log("start with - " + current);

					for (int i = StartOnPage; i < lastPage; i++) {// 2; i++) {
						string listName = String.Format(ListFileName, i);
						string listUrl = String.Format(ListURL, i);
						Log("list file - " + listName);
						web.DownloadFile(listUrl, listName);
						Wait();

						text = File.ReadAllText(listName);
						MatchCollection listMatches = listRegex.Matches(text);
						Log("list match count - " + listMatches.Count);

						if (listMatches.Count == 0) {
							Error("No list matches");
						} else {
							for (int j = 0; j < listMatches.Count; j++) {
								bool go = true;
								string pageUrl = listMatches[j].Groups[1].Value;
								Log("page url - " + pageUrl);

								string name = filenameRegex.Replace(listMatches[j].Groups[2].Value, "-");
								string year = listMatches[j].Groups[3].Value;
								string title = name + " (" + year + ")";

								string pageName = String.Format(PageFileName, current, title);
								Log("page name - " + pageName);
								try {
									web.DownloadFile(pageUrl, pageName);
								} catch (Exception ex) {
									Error("bad fetch page - " + pageName + " - " + pageUrl);
									go = false;
								}

								if (go) {
									Wait();

									text = File.ReadAllText(pageName);
									MatchCollection pageMatches = pageRegex.Matches(text);
									Log("page match count - " + pageMatches.Count);

									if (pageMatches.Count == 0) {
										Error("No page matches - " + pageName + " - " + pageUrl);
									} else {
										foreach (Match m in pageMatches) {
											for (int uu = 0; uu < m.Groups[1].Captures.Count; uu++) {
												string torrentUrl = m.Groups[1].Captures[uu].Value;

												string torrentRType = m.Groups[2].Captures[uu].Value;
												string torrentName = String.Format(TorrentFileName, i, current, title, torrentRType);

												Log("torrent file - " + torrentName + " - " + torrentUrl);

												try {
													web.DownloadFile(torrentUrl, torrentName);
												} catch (Exception ex) {
													Error("bad fetch torrent - " + torrentName + " - " + torrentUrl + " - " + ex.ToString());
												}

												try {
													string hash = FileToMD5(torrentName);
													string otherName = String.Format(TorrentFileNameSecond, title, torrentRType, hash);

													File.Move(torrentName, otherName);
												} catch (Exception ex) {
													Error("error changing md5 name - " + torrentName + " - " + torrentUrl + " - " + ex.ToString());
												}



												Wait();
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

			End();
		}


		static void DownloadTypes() {

			string dir = Path.GetDirectoryName(PageFileName.Replace("{now}", NowString));
			FixFileNames();
			Directories();
			Log("START");
			Dictionary<string, int> counts = new Dictionary<string, int>();

			var pageNames = Directory.EnumerateFiles(dir);
			Log("page count - " + pageNames.Count());
			foreach (var pageName in pageNames) {
				Log("page - " + pageName);
				string content = File.ReadAllText(pageName);
				MatchCollection pageMatches = pageRegex.Matches(content);
				Log("page match count - " + pageMatches.Count);

				if (pageMatches.Count == 0) {
					Error("No page matches - " + pageName);
				} else {
					//for (pageMatches[0].Groups) {
					string title = "";
					//}
				}

			}








			End();
		}

	}
}

