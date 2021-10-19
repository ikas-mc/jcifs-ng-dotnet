/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
namespace cifs_ng.lib.threading {
	public class Semaphore {
		protected AtomicInteger value;

		public Semaphore(int initialPermits) {
			value = new AtomicInteger(initialPermits);
		}

		public virtual void Release() {
			value.IncrementValueAndReturn();
		}


		private int nonfairTryAcquireShared(int acquires) {
			for (;;) {
				var available = value.Value;
				var remaining = available - acquires;
				if (remaining < 0 || value.CompareAndSet(available, remaining)) {
					return remaining;
				}
			}
		}

		private bool releaseShared(int releases) {
			for (;;) {
				var current = value.Value;
				var next = current + releases;
				if (next < current) {
					throw new Exception("Maximum permit count exceeded");
				}
				if (value.CompareAndSet(current, next)) {
					return true;
				}
			}
		}

		public int drainPermits() {
			for (;;) {
				var current = value.Value;
				if (current == 0 || value.CompareAndSet(current, 0)) {
					return current;
				}
			}
		}

		public bool tryAcquire() {
			try {
				return nonfairTryAcquireShared(1) >= 0;
			}
			catch (Exception) {
				return false;
			}
		}

		public bool tryAcquire(int s) {
			try {
				return nonfairTryAcquireShared(1) >= 0;
			}
			catch (Exception) {
				return false;
			}
		}

		//TODO 
		public bool tryAcquire(int s, TimeSpan span) {
			try {
				return nonfairTryAcquireShared(1) >= 0;
			}
			catch (Exception) {
				return false;
			}
		}

		public virtual void Acquire() {
			nonfairTryAcquireShared(1);
		}

		public virtual void Acquire(int s) {
			nonfairTryAcquireShared(s);
		}

		public virtual void Release(int n) {
			releaseShared(n);
		}

		public virtual int Permits() {
			return value.Value;
		}
	}
}