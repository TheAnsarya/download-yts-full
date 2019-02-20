using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace download_yts_full {
	class LockedResourceObject<T> where T : new() {
		// Does something claim this resource?
		public bool InUse = false;

		// Create The resource
		private T _resource = new T();

		// Get The resource
		public T Get() {
			return _resource;
		}

		// Lock object for thread safety
		private object _lock = new object();

		// Mark as in use
		// Returns true if gotten, false if something has already claimed it
		// TODO: doesn't seem thread safe (is lock the best way?)
		public bool Use() {
			lock (_lock) {
				if (this.InUse) {
					return false;
				}

				this.InUse = true;

				return true;
			}
		}

		// Mark as available
		// Lock doesn't matter as this releases the resource, and single assignment is atomic
		// Call when done with this object
		public void Done() {
			this.InUse = false;
		}
	}
}
