using Sipebi.Malindo.Entry.Parser;
using System.Xml;
using System.Configuration;
using Octokit;

namespace Sipebi.Malindo.Entry.Parser.Console {
	internal class Program {
		static List<string> acceptedArgs = new List<string> {
			"check", "download", "parse", "serialize", "deserialize"
		};
		static List<string> compulsoryAppSettings = new List<string> {
			"GithubOwner", "GithubRepositoryName", "GithubClientApp", "GithubPersonalAccessToken", "GithubFilePattern"
		};
		static SipebiMalindoDictionary? dict;
		static void Main(string[] args) {
			if (args == null || args.Length == 0) {
				System.Console.WriteLine("No valid argument detected");
				return;
			}

			string arg = args[0].Trim().ToLower();
			string defaultErrorMessage = $"Argument [{arg}] is not among the accepted arguments [{string.Join(", ", acceptedArgs)}]";

			if (!acceptedArgs.Contains(arg)) {
				System.Console.WriteLine(defaultErrorMessage);
				return;
			}

			//List of strings which cannot be empty or null
			if (compulsoryAppSettings.Any(x =>
				string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Get(x)?.Trim()))) {
				System.Console.WriteLine($"One or more configuration settings [{string.Join(", ", compulsoryAppSettings)}] " +
					"are empty or null");
				return;
			}

			string? owner = ConfigurationManager.AppSettings.Get("GithubOwner")?.Trim();
			string? repositoryName = ConfigurationManager.AppSettings.Get("GithubRepositoryName")?.Trim();
			string? githubApp = ConfigurationManager.AppSettings.Get("GithubClientApp")?.Trim();
			string? personalAccessToken = ConfigurationManager.AppSettings.Get("GithubPersonalAccessToken")?.Trim();
			string? folderPath = ConfigurationManager.AppSettings.Get("GithubFolderPath")?.Trim();
			string? filePattern = ConfigurationManager.AppSettings.Get("GithubFilePattern")?.Trim();

			string? latestDownloadFileName = ConfigurationManager.AppSettings.Get("LatestDownloadFileName")?.Trim();

			dict = new SipebiMalindoDictionary();
			dict.RepoFound += Dict_RepoFound; //to print out event results, if necessary

			Tuple<GitHubClient, RepositoryContent?> githubSet;
			string error;
			string xmlFilepath = Path.Combine("data", "malindo.xml");

			try {
				switch (arg) {
					case "check":
					case "download":
						//The message will be taken cared of by this simple checking
						githubSet = dict.GetGithub(owner, repositoryName, githubApp, personalAccessToken, folderPath, filePattern);
						if (arg == "check") {
							break; //If it is just checking, we will return here
						}
						if (githubSet.Item2 == null) {
							break; //If there is no valid file, we have nothing to process, we will also return here. The error message would also have been printed earlier
						}
						//If it is a download, we will continue by downloading
						bool processResult = dict.DownloadGithub(githubSet, owner, repositoryName, out error);
						if (!processResult) System.Console.WriteLine(error);
						else {
							Directory.CreateDirectory("download");
							string downloadPath = Path.Combine("download", githubSet.Item2.Name);
							System.Console.WriteLine($"File successfully downloaded to [{downloadPath}]");
							System.Console.WriteLine($"URL source: [{githubSet.Item2.Url}]");
							System.Console.WriteLine($"Path: [{githubSet.Item2.Path}]");
						}
						break;

					case "parse": //For now, just combine parse with serialize
					case "serialize":
						var filepaths = Directory.GetFiles("download", filePattern);
						if (!filepaths.Any()) {
							System.Console.WriteLine("No available file in the download folder");
							return;
						}
						string filepath = filepaths[0];
						if (string.IsNullOrWhiteSpace(latestDownloadFileName))
							System.Console.WriteLine("No latest download file name provided. Default mecanism will be used");
						else if (!filepaths.Any(x => Path.GetFileName(x) == latestDownloadFileName)) {
							System.Console.WriteLine($"No file in the [download] folder matching the latest download file name [{latestDownloadFileName}]. Default mechanism will be used");
						} else {
							System.Console.WriteLine($"File [{latestDownloadFileName}] found in the [download] folder");
							filepath = filepaths.FirstOrDefault(x => Path.GetFileName(x) == latestDownloadFileName);
						}
						dict.Entries = dict.ReadTsv(filepath, out error);
						if (!string.IsNullOrWhiteSpace(error))
							System.Console.WriteLine($"Parsing error: {error}");
						if (arg == "parse") return;
						//If it is serializing, add one more step, that is to serialize the resulting file
						dict.SerializeToXml(xmlFilepath);
						System.Console.WriteLine($"File [{xmlFilepath}] is successfully serialized");
						break;

					case "deserialize":
						if (!File.Exists(xmlFilepath)) {
							System.Console.WriteLine($"Valie [{xmlFilepath}] file is not found");
							return;
						}
						dict.DeserializeFromXml(xmlFilepath);
						System.Console.WriteLine($"File [{xmlFilepath}] is successfully deserialized");
						break;

					default:
						System.Console.WriteLine(defaultErrorMessage);
						return;
				}
			} catch (Exception ex) {
				System.Console.WriteLine($"Command [{arg}] error: {ex.Message}");
			}
		}
	
		private static void Dict_RepoFound(object? sender, string e) {
			System.Console.WriteLine(e);
		}
	}
}