using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Launcher
{
	internal static class Program
	{
		private static readonly string GameId = typeof(Program).Assembly.GetName().Name;
		private const string IniPath = @".\launcher.ini";
		private const string Application = "scummvm";
		//private const string Application = "residualvm";
		private const string ExePath = Application + ".exe";

		// ReSharper disable once InconsistentNaming
		private const int ERROR_FILE_NOT_FOUND = 0x2;

		private static async Task Main()
		{
			new Process
			{
				StartInfo =
				{
					FileName = GetFileName(),
					Arguments = await GetArguments(),
				}
			}.Start();
		}

		private static string GetFileName()
		{
			if (File.Exists(ExePath))
				return ExePath;

			var exePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + $@"\{Application}\" + ExePath;
			if (File.Exists(exePath))
				return exePath;

			exePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + $@"\{Application}\" + ExePath;
			if (File.Exists(exePath))
				return exePath;

			exePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"\{Application}\" + ExePath;
			if (File.Exists(exePath))
				return exePath;

			Environment.ExitCode = ERROR_FILE_NOT_FOUND;
			throw new FileNotFoundException("File not found", ExePath);
		}

		private static async Task<string> GetArguments()
		{
			var arguments = await ReadIniFile();
			SetGamePath(arguments);
			arguments.Add($"{GameId}");
			return string.Join(" ", arguments);
		}

		private static void SetGamePath(IList<string> arguments)
		{
			if (arguments.Any(x => x.StartsWith("--path=")))
				return;

			if (!CheckAndAddDirectory(arguments, $"{GameId}"))
				CheckAndAddDirectory(arguments, "game");
		}

		private static bool CheckAndAddDirectory(IList<string> arguments, string directory)
		{
			if (!IsValidDirectory(directory))
				return false;

			arguments.Add($@"--path=.\{directory}");
			return true;
		}

		private static bool IsValidDirectory(string directory)
		{
			var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), directory);
			return Directory.Exists(directoryPath) &&
				(Directory.GetFiles(directoryPath).Any() || Directory.GetDirectories(directoryPath).Any());
		}

		private static async Task<IList<string>> ReadIniFile()
		{
			var arguments = new List<string>();

			if (!File.Exists(IniPath))
				return arguments;

			using (var reader = File.OpenText(IniPath))
			{
				string line;
				while ((line = await reader.ReadLineAsync()) != null)
				{
					line = line.Trim();
					if (!string.IsNullOrWhiteSpace(line) && line[0] != ';')
						arguments.Add("--" + line);
				}
			}
			return arguments;
		}
	}
}
