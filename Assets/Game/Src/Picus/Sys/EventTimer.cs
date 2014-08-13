using System;
using System.Diagnostics;

namespace Picus.Sys
{
	/**
	 * 
	 */
	public class EventTimer
	{
		Stopwatch _stopWatch = new Stopwatch();
		private long _durationLeft;

		/**
		 * Setups timer for event started at @a startTime milliseconds and counting for @a duration milliseconds. 
		 * @param currentTime synchronizes event time 
		 */
		public EventTimer(long currentTime, long startTime, long duration)
		{
			long runtime = currentTime - startTime;
			_durationLeft = duration - runtime;
			_stopWatch.Start();
		}

		public void reset() { _stopWatch.Reset(); _stopWatch.Start(); }

		public bool timeOut() { return _stopWatch.ElapsedMilliseconds > _durationLeft; }
		public long timeLeft() { return _durationLeft - _stopWatch.ElapsedMilliseconds; }
		public string timeLeftFormatted() 
		{
			const long MINUTE = 60;
			const long HOUR = 60 * MINUTE;
			long secondsLeft = timeLeft() / 1000;
			long hours = secondsLeft / HOUR;
			long minutes = (secondsLeft % HOUR) / MINUTE;
			long seconds = (secondsLeft % HOUR) % MINUTE;
			return string.Format("{0:D}:{1:D2}:{2:D2}", hours, minutes, seconds);
		}
	}
}
