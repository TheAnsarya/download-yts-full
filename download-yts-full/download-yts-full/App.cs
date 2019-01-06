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
		public string BaseFolder = null;
		public string LogFileName = @"run\{now}\log.txt";
		public string ErrorFileName = @"run\{now}\error.txt";
		public string WorkingFolder = @"run\{now}\";

		static MD5 md5 = MD5.Create();
		
		public App(string baseFolder) {
			this.BaseFolder = baseFolder;

			// Working folder - base for the single run files
			this.WorkingFolder = Path.Combine(this.BaseFolder, this.WorkingFolder);
			this.WorkingFolder = WorkingFolder.Replace("{now}", this.NowString);
			mkdir(WorkingFolder);
		}

		public string GetInWorking(string path) {
			string newpath = Path.Combine(this.WorkingFolder, path);
			mkdir(path);
			return newpath;
		}

		public StreamWriter _logFile;
		public StreamWriter LogFile {
			get {
				if (_logFile == null) {
					this.LogFileName = Path.Combine(this.BaseFolder, this.LogFileName);
					this.LogFileName = LogFileName.Replace("{now}", this.NowString);
					mkdir(LogFileName);
					_logFile = File.CreateText(String.Format(LogFileName, Now()));
				}
				return _logFile;
			}
		}

		public StreamWriter _errorFile;
		public StreamWriter ErrorFile {
			get {
				if (_errorFile == null) {
					this.ErrorFileName = Path.Combine(this.BaseFolder, this.ErrorFileName);
					this.ErrorFileName = ErrorFileName.Replace("{now}", this.NowString);
					mkdir(ErrorFileName);
					_errorFile = File.CreateText(String.Format(ErrorFileName, Now()));
				}
				return _errorFile;
			}
		}

		static Stopwatch _clock;
		static Stopwatch Clock {
			get {
				if (_clock == null) {
					_clock = new Stopwatch();
					_clock.Start();
				}
				return _clock;
			}
		}

		static Random _rng;
		static Random RNG {
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
		
		static string ClockStamp() {
			TimeSpan delta = Clock.Elapsed;
			string stamp = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", delta.Hours, delta.Minutes, delta.Seconds, delta.Milliseconds / 10);
			return stamp;
		}

		public string NowString = Now();

		static string Now() {
			return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff");
		}

		static string LogStamp() {
			return Now() + " " + ClockStamp() + " --- ";
		}

		public void Log(string message) {
			message = LogStamp() + message;
			Console.WriteLine(message);
			LogFile.WriteLine(message);
			LogFile.Flush();
		}

		public void Error(string message) {
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

		public void End() {
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
