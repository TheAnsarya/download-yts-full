using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace download_yts_full {
	class Download {
		// Milliseconds to wait before rechecking when there are no available connections in the pool
		// 100 ms (0.1 seconds, a tenth of a second)
		const int DEFAULT_WAIT_TIME = 100;

		// Maximum number of times to try to find a connection
		// 10 minutes
		const int DEFAULT_MAX_ATTEMPTS = 6000;

		// Default number of connections to use in the pool
		const int DEFAULT_MAX_CONNECTIONS = 16;

		// Number of connections to use in the pool
		private int _maxConnections;
		public int MaxConnections {
			get {
				return this._maxConnections;
			}
		}

		// Web Connection Pool
		private List<WC> Pool = new List<WC>();

		// Lock object for thread safety
		private object _lock = new object();
		
		// Actual number of connections
		public int Count {
			get {
				return this.Pool.Count;
			}
		}

		// Index of the last connection object used
		private int _last = -1;
		public int Last {
			get {
				return this._last;
			}
			set {
				this._last = value;
			}
		}

		// Available connections
		public int Available {
			get {
				return this.Pool.Select(n => !n.InUse).Count();
			}
		}

		public Download() : this(DEFAULT_MAX_CONNECTIONS) { }

		public Download(int maxConnections) {
			this._maxConnections = maxConnections;

			for (int i = 0; i <maxConnections; i++) {
				this.Pool.Add(new WC());
			}
		}

		// Gets the next avilable connection
		// Should be thread safe
		// Note that with the lock, if something goes wrong nothing can download for upwards of ten minutes
		public WC GetNext() {
			lock (_lock) {
				for (int i = 0; i < DEFAULT_MAX_ATTEMPTS; i++) {
					for (int j = this.Last + 1; j < this.Count; j++) {
						WC current = this.Pool[j];
						if (!current.InUse) {
							current.InUse = true;
							this.Last = j;
							return current;
						}
					}

					for (int j = 0; j < this.Last + 1; j++) {
						WC current = this.Pool[j];
						if (!current.InUse) {
							current.InUse = true;
							this.Last = j;
							return current;
						}
					}

					App.Wait(DEFAULT_WAIT_TIME);
				}
			}

			throw new Exception("Maximum number of attempts, " + DEFAULT_MAX_ATTEMPTS + ", made to find an available web connection with no result.");
		}

		// Save url to file
		public void SaveTo(string url, string path) {
			WC wc = null;
			try {
				wc = GetNext();
				wc.web.DownloadFile(url, path);
			} finally {
				try {
					wc.InUse = false;
				} catch { }
			}
		}
		
		public class WC {
			public bool InUse = false;
			public WebClient web = new WebClient();

			public bool Use() {
				if (this.InUse) {
					return false;
				}

				this.InUse = true;

				return true;
			}

			public void Done() {
				this.InUse = false;
			}
		}
	}
}
