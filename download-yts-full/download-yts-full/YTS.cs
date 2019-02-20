using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace download_yts_full {
	class YTS {
		static string  StartURL = @"https://yts.am/browse-movies?page=1";
		static string ListURL = @"https://yts.am/browse-movies?page={0}";

		// Filenames
		public string StartFileName = @"start.txt";
		public string ListFileName = @"list\list-{0}.txt";
		public string PageFileName = @"page\page-{0}-{1}.txt";
		public string TorrentFileName = @"torrent-in\{0}-{1}-{2}-{3}.torrent";
		public string TorrentFileNameSecond = @"torrent\{0} [{1}][{2}].torrent";
		
		static Regex listRegex = new Regex(@"<a href=""(.+)"" class=""browse-movie-title"">(.+)</a>\s+<div class=""browse-movie-year"">(.+)</div>");
		static Regex lastRegex = new Regex(@"<a href=""/browse-movies\?page=(\d+)"">Last &raquo;</a>");
		static Regex pageRegex = new Regex(@"<em class=""pull-left"">Available in: &nbsp;</em>\s+(?:<a href=""(.+)"" rel=""nofollow"" title="".+"">(.+)</a>\s*)+");
		static Regex filenameRegex = new Regex(@"[\*\.""\/\?\\:;|=,]");
		static int PerPage = 20;

		public App app = null;
		public Download dl = new Download();

		static void Log(string message) {
			App.Log(message);
		}

		static void Error(string message) {
			App.Error(message);
		}
		public YTS() {
			Initialize();
		}

		private void Initialize() {
			this.StartFileName = App.GetInWorking(this.StartFileName);
			this.ListFileName = App.GetInWorking(this.ListFileName);
			this.PageFileName = App.GetInWorking(this.PageFileName);
			this.TorrentFileName = App.GetInWorking(this.TorrentFileName);
			this.TorrentFileNameSecond = App.GetInWorking(this.TorrentFileNameSecond);
		}

		public class ListPage {
			public string URL = null;

			public string Filename = null;

			public ListPage(string filename) {
				this.Filename = filename;
			}
			public ListPage(string filename, string url) {

				this.Filename = filename;
			}


			public string Contents {
				get {
					return File.ReadAllText(this.Filename);
				}
			}

			private MatchCollection _matches = null;
			public MatchCollection Matches {
				get {
					if (this._matches == null) {
						this._matches = listRegex.Matches(this.Contents);
					}

					return this._matches;
				}
			}

			public void Refresh() {
				this._matches = null;
			}
		}
	}
}
