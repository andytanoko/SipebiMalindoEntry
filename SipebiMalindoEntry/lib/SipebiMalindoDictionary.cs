using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net;
using System.Linq;
using Octokit;
using System.Text.RegularExpressions;


namespace SipebiMalindoEntry.lib
{using System.Text.RegularExpressions;

    public class SipebiMalindoDictionary
    {
        private List<SipebiMalindoEntry> sipebiMalindoEntrieList = new List<SipebiMalindoEntry>();

        public void processGithub(string owner, string repositoryName,string githubApp, string personalAccessToken, string folderPath ,string filePattern=@"^malindo.*\.tsv$")
        {
            
            String pattern = folderPath + filePattern;
            DateTime lastCheckDate = DateTime.Now.AddDays(-1); // Check for files modified in the last 1 day

            // GitHub client authentication (replace with your personal access token)
            var github = new GitHubClient(new Octokit.ProductHeaderValue(githubApp));
            github.Credentials = new Credentials(personalAccessToken);

            // Get repository contents
            var contents = github.Repository.Content.GetAllContents(owner, repositoryName).Result;

            // Find the first file that matches the pattern and has been modified within the last 1 day
            var fileToProcess = contents
                .FirstOrDefault(content => content.Type == Octokit.ContentType.File &&
                                        Regex.IsMatch(content.Name, pattern));

            if (fileToProcess != null)
            {
                Console.WriteLine($"File found: {fileToProcess.Name} ");
                // Call your processing method here
                // ProcessFile(fileToProcess.DownloadUrl);
                var fileContent = github.Repository.Content.GetRawContent(owner, repositoryName, fileToProcess.Path).Result;
                File.WriteAllBytes(fileToProcess.Name, fileContent);
                ReadTsv(fileToProcess.Name);
            }
            else
            {
                Console.WriteLine("No file found matching the criteria.");
            }
        }

        public List<SipebiMalindoEntry> ReadTsv(string filePath)
        {
            List<SipebiMalindoEntry> dataObjects = new List<SipebiMalindoEntry>();

            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.SetDelimiters("\t"); // Set tab as the delimiter
                    parser.TextFieldType = FieldType.Delimited;

                    while (!parser.EndOfData)
                    {
                        string[] parts = parser.ReadFields();
                        if (parts.Length >= 2)
                        {
                            SipebiMalindoEntry obj = new SipebiMalindoEntry
                            {
                                id = parts[0],
                                akar = parts[1],
                                bentukLahir = parts[2],
                                awalan = parts[3],
                                akhiran = parts[4],
                                apitan = parts[5],
                                penggandaan = parts[6],
                                sumber  = parts[7],
                                dasar = parts[8],
                                lema = parts[9],
                            };
                            dataObjects.Add(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading TSV file: " + ex.Message);
            }
            sipebiMalindoEntrieList = dataObjects;
            return dataObjects;
        }

        public string SerializeListToXml()
        {
            List<SipebiMalindoEntry> list = this.sipebiMalindoEntrieList;
            XmlSerializer serializer = new XmlSerializer(typeof(List<SipebiMalindoEntry>));

            // Create a StringBuilder to store the serialized XML
            StringBuilder stringBuilder = new StringBuilder();

            // Create an XmlWriter to write the serialized XML to the StringBuilder
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
            {
                serializer.Serialize(xmlWriter, list);
            }
            return stringBuilder.ToString();
        }

        public void SerializeToXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<SipebiMalindoEntry>));

            // Create a FileStream to write the serialized XML to the specified file path
            using (FileStream stream = new FileStream(filePath, System.IO.FileMode.Create))
            {
                serializer.Serialize(stream, this.sipebiMalindoEntrieList);
            }
        }

        public T DeserializeFromXml<T>(string xmlContent)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StringReader reader = new StringReader(xmlContent))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        public T DeserializeFromXmlFile<T>(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream fileStream = new FileStream(filePath, System.IO.FileMode.Open))
            {
                return (T)serializer.Deserialize(fileStream);
            }
        }
    }


    [Serializable]
    public class SipebiMalindoEntry
    {

        //ID [TAB] Akar [TAB] Bentuk lahir [TAB] Awalan/proklitik [TAB] Akhiran/enklitik
        //[TAB] Apitan [TAB] Penggandaan [TAB] Sumber [TAB] Dasar [TAB] Lema


        public string id{ get; set; }
        public string akar { get; set; }

        public string bentukLahir { get; set; }
        public string awalan { get; set; }

        public string akhiran{ get; set; }

        public string apitan { get; set; }

        public string penggandaan { get; set; }

        public string sumber { get; set; }
        public string dasar{ get; set; }
        public string lema { get; set; }

    }
}

