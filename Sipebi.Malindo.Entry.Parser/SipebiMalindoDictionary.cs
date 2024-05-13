using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Text.RegularExpressions;
using Octokit;
using System.Reflection.Metadata;

namespace Sipebi.Malindo.Entry.Parser {
	public class SipebiMalindoDictionary {
		public List<SipebiMalindoEntry> Entries { get; set; } = new List<SipebiMalindoEntry>();

		public Tuple<GitHubClient, RepositoryContent?> GetGithub(string owner, string repositoryName, string githubApp, string personalAccessToken, string folderPath, string filePattern = @"^malindo.*\.tsv$") {

			String pattern = Path.Combine(folderPath, filePattern);
			DateTime lastCheckDate = DateTime.Now.AddDays(-1); // Check for files modified in the last 1 day (note: may not be used)

			// GitHub client authentication (replace with your personal access token)
			var github = new GitHubClient(new ProductHeaderValue(githubApp));
			github.Credentials = new Credentials(personalAccessToken);

			// Get repository contents
			var contents = github.Repository.Content.GetAllContents(owner, repositoryName).Result;

			// Find the first file that matches the pattern and has been modified within the last 1 day
			var fileToProcess = contents
					.FirstOrDefault(content => content.Type == Octokit.ContentType.File &&
																	Regex.IsMatch(content.Name, pattern));

			// Return success/error message
			RepoFound?.Invoke(this, fileToProcess == null ? 
				$"No file found matching the criteria." : $"File found: {fileToProcess.Name}");

			// Return the github client and repository content
			return new Tuple<GitHubClient, RepositoryContent?>(github, fileToProcess);
		}

		public event EventHandler<string>? RepoFound;
		public bool DownloadGithub(string owner, string repositoryName, string githubApp, string personalAccessToken, string folderPath, string filePattern, out string error) {
			Tuple<GitHubClient, RepositoryContent?> githubSet = GetGithub(owner, repositoryName, githubApp, personalAccessToken, folderPath, filePattern);
			return DownloadGithub(githubSet, owner, repositoryName, out error);
		}

		public bool DownloadGithub(Tuple<GitHubClient, RepositoryContent?> githubSet, string owner, string repositoryName, out string error) {
			error = string.Empty;
			try {
				GitHubClient github = githubSet.Item1;
				RepositoryContent? content = githubSet.Item2;
				if (content == null) return false;
				var fileContent = github.Repository.Content.GetRawContent(owner, repositoryName, content.Path).Result;
				var downloadPath = Path.Combine("download", content.Name);
				File.WriteAllBytes(downloadPath, fileContent);
				return true;
			} catch (Exception ex) {
				error = ex.ToString();
				return false;
			}
		}

		public List<SipebiMalindoEntry>? ReadTsv(string filePath, out string error) {
			error = string.Empty;
			List<SipebiMalindoEntry> dataObjects = new List<SipebiMalindoEntry>();

			try {
				using (TextFieldParser parser = new TextFieldParser(filePath)) {
					parser.SetDelimiters("\t"); // Set tab as the delimiter
					parser.TextFieldType = FieldType.Delimited;

					while (!parser.EndOfData) {
						string[]? parts = parser.ReadFields();
						if (parts != null && parts.Length >= 2) {
							SipebiMalindoEntry obj = new SipebiMalindoEntry {
								ID = parts[0],
								Akar = parts[1],
								BentukLahir = parts[2],
								Awalan = parts[3],
								Akhiran = parts[4],
								Apitan = parts[5],
								Penggandaan = parts[6],
								Sumber = parts[7],
								Dasar = parts[8],
								Lema = parts[9],
							};
							dataObjects.Add(obj);
						} else {
							error = $"Not enough parts in the TSV file";
							return null;
						}
					}
				}
			} catch (Exception ex) {
				error = "Error reading TSV file: " + ex.Message;
				return null;
			}
			Entries = dataObjects;
			return dataObjects;
		}

		public string SerializeToXmlString() {
			XmlSerializer serializer = new XmlSerializer(typeof(SipebiMalindoDictionary));

			// Create a StringBuilder to store the serialized XML
			StringBuilder stringBuilder = new StringBuilder();

			// Create an XmlWriter to write the serialized XML to the StringBuilder
			using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder)) {
				serializer.Serialize(xmlWriter, this);
			}
			return stringBuilder.ToString();
		}

		public void SerializeToXml(string filePath) {
			XmlSerializer serializer = new XmlSerializer(typeof(SipebiMalindoDictionary));

			// Create a FileStream to write the serialized XML to the specified file path
			using (FileStream stream = new FileStream(filePath, System.IO.FileMode.Create)) {
				serializer.Serialize(stream, this);
			}
		}

		public SipebiMalindoDictionary DeserializeFromXmlString(string xmlContent) {
			XmlSerializer serializer = new XmlSerializer(typeof(SipebiMalindoDictionary));

			using (StringReader reader = new StringReader(xmlContent)) {
				return (SipebiMalindoDictionary)serializer.Deserialize(reader);
			}
		}
		public SipebiMalindoDictionary DeserializeFromXml(string filePath) {
			XmlSerializer serializer = new XmlSerializer(typeof(SipebiMalindoDictionary));

			using (FileStream fileStream = new FileStream(filePath, System.IO.FileMode.Open)) {
				return (SipebiMalindoDictionary)serializer.Deserialize(fileStream);
			}
		}
	}
}
