using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace download_yts_full {
	class App {
		public static string BaseFolder = null;
		public static string LogFileName = @"run\{now}\log.txt";
		public static string ErrorFileName = @"run\{now}\error.txt";
		public static string WorkingFolder = @"run\{now}\";
		public static MD5 md5 = MD5.Create();
		
		public static void Initialize(string baseFolder) {
			BaseFolder = baseFolder;

			// Working folder - base for the single run files
			WorkingFolder = Path.Combine(BaseFolder, WorkingFolder);
			WorkingFolder = WorkingFolder.Replace("{now}", NowString);
			mkdir(WorkingFolder);
		}

		public static string GetInWorking(string path) {
			string newpath = Path.Combine(WorkingFolder, path);
			mkdir(path);
			return newpath;
		}

		public static StreamWriter _logFile;
		public static StreamWriter LogFile {
			get {
				if (_logFile == null) {
					LogFileName = Path.Combine(BaseFolder, LogFileName);
					LogFileName = LogFileName.Replace("{now}", NowString);
					mkdir(LogFileName);
					_logFile = File.CreateText(String.Format(LogFileName, Now()));
				}
				return _logFile;
			}
		}

		private static StreamWriter _errorFile;
		public static StreamWriter ErrorFile {
			get {
				if (_errorFile == null) {
					ErrorFileName = Path.Combine(BaseFolder, ErrorFileName);
					ErrorFileName = ErrorFileName.Replace("{now}", NowString);
					mkdir(ErrorFileName);
					_errorFile = File.CreateText(String.Format(ErrorFileName, Now()));
				}
				return _errorFile;
			}
		}

		private static Stopwatch _clock;
		public static Stopwatch Clock {
			get {
				if (_clock == null) {
					_clock = new Stopwatch();
					_clock.Start();
				}
				return _clock;
			}
		}

		private static Random _rng;
		public static Random RNG {
			get {
				if (_rng == null) {
					_rng = new Random();
				}
				return _rng;
			}
		}

		public static void mkdir(string path) {
			string dirName = Path.GetDirectoryName(path);
			if (!Directory.Exists(dirName)) {
				Directory.CreateDirectory(dirName);
			}
		}

		public static string FileToMD5(string filename) {
			FileStream fs = File.Open(filename, FileMode.Open);
			byte[] crc = md5.ComputeHash(fs);
			StringBuilder sb = new StringBuilder();
			for (int k = 0; k < crc.Length; k++) {
				sb.Append(crc[k].ToString("x2"));
			}
			string hash = sb.ToString();
			fs.Close();

			return hash;
		}
		
		public static string ClockStamp() {
			TimeSpan delta = Clock.Elapsed;
			string stamp = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", delta.Hours, delta.Minutes, delta.Seconds, delta.Milliseconds / 10);
			return stamp;
		}

		public static string NowString = Now();

		public static string Now() {
			return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
		}

		public static string LogStamp() {
			return Now() + " " + ClockStamp() + " --- ";
		}

		public static void Log(string message) {
			message = LogStamp() + message;
			Console.WriteLine(message);
			LogFile.WriteLine(message);
			LogFile.Flush();
		}

		public static void Error(string message) {
			Log(message);
			message = LogStamp() + message;
			ErrorFile.WriteLine(message);
			ErrorFile.Flush();
		}

		public static void Wait() {
			//int time = 500 + RNG.Next(1000);
			//Wait(time);
			Wait(1);
		}

		public static void Wait(int time) {
			Thread.Sleep(time);
		}

		public static void End() {
			Clock.Stop();
			Log("END");

			ErrorFile.Flush();
			ErrorFile.Close();

			LogFile.Flush();
			LogFile.Close();

			Console.ReadKey();
		}
	}
}
