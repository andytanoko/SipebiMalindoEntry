
using Octokit;
using SipebiMalindoEntry.lib;
class Program
{

    static void Main(string[] args)
    {
        SipebiMalindoEntry.lib.SipebiMalindoDictionary reader = new SipebiMalindoEntry.lib.SipebiMalindoDictionary();
        reader.processGithub("matbahasa", "MALINDO_Morph", "sipebiMalindo", "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", "");

        string xml = reader.SerializeListToXml();
        List<SipebiMalindoEntry.lib.SipebiMalindoEntry>  data2 = reader.DeserializeFromXml<List<SipebiMalindoEntry.lib.SipebiMalindoEntry>>(xml);
       // Console.WriteLine(reader.SerializeListToXml());
        foreach (var obj in data2)
        {
            Console.WriteLine($"id: {obj.id}, dasar: {obj.akar}");
            // Output more properties as needed
        }
    }
}
