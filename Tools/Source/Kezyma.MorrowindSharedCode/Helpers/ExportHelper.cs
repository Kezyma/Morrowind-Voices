using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;

namespace Kezyma.MorrowindSharedCode.Helpers
{
    public static class ExportHelper
    {
        public static string GetTes3Conv()
        {
            var tes3convpath = Path.GetFullPath("tes3conv.exe");
            if (!File.Exists(tes3convpath))
            {
                var dl = new HttpClient().GetAsync("https://github.com/Greatness7/tes3conv/releases/download/v0.0.10/tes3conv-windows-latest.zip");
                var bar = dl.Result.Content.ReadAsByteArrayAsync().Result;
                var zp = Path.GetFullPath("tes3conv.zip");
                var exl = Directory.GetParent(zp).FullName;
                File.WriteAllBytes(zp, bar);
                ZipFile.ExtractToDirectory(zp, exl);
                File.Delete(zp);
            }
            return tes3convpath;
        }

        public static string ExportEspJson(string tes3conv, string esp, string output)
        {
            var f = Path.GetFileNameWithoutExtension(esp);
            Console.WriteLine($"Extracting {esp}");
            var fN = $"{f}.json";
            var js = Path.Combine(output, fN);
            var cmd = $"\"{esp}\" \"{js}\"";
            var cmdsi = new ProcessStartInfo(tes3conv)
            {
                Arguments = cmd
            };
            var ex = Process.Start(cmdsi);
            ex.WaitForExit();
            return js;
        }

        public static List<Root> ExtractJsonDialogue(string json)
        {
            var text = new List<Root>();
            var fn = Path.GetFileNameWithoutExtension(json);
            Console.WriteLine($"Reading {json}");
            var jsonData = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(json));
            var npcData = jsonData.Where(x => x.type == "Info" && (x.data == null || (x.data.dialogue_type != "Journal" && x.data.dialogue_type != "Voice")));
            text.AddRange(npcData);
            return text;
        }

        public static List<Root> ExtractJsonNPCs(string json)
        {
            var npcs = new List<Root>();
            var fn = Path.GetFileNameWithoutExtension(json);
            Console.WriteLine($"Reading {json}");
            var jsonData = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(json));
            var npcData = jsonData.Where(x => x.type == "Npc" || x.type == "Creature");
            npcs.AddRange(npcData);
            return npcs;
        }

        public static List<Root> ExtractJsonFactions(string json)
        {
            var facs = new List<Root>();
            var fn = Path.GetFileNameWithoutExtension(json);
            Console.WriteLine($"Reading {json}");
            var jsonData = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(json));
            var npcData = jsonData.Where(x => x.type == "Faction");
            facs.AddRange(npcData);
            return facs;
        }

        public static List<Root> ExtractJsonCells(string json)
        {
            var facs = new List<Root>();
            var fn = Path.GetFileNameWithoutExtension(json);
            Console.WriteLine($"Reading {json}");
            var jsonData = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(json));
            var npcData = jsonData.Where(x => x.type == "Cell");
            facs.AddRange(npcData);
            return facs;
        }

        public static List<Root> ExtractJsonRegions(string json)
        {
            var facs = new List<Root>();
            var fn = Path.GetFileNameWithoutExtension(json);
            Console.WriteLine($"Reading {json}");
            var jsonData = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(json));
            var npcData = jsonData.Where(x => x.type == "Region");
            facs.AddRange(npcData);
            return facs;
        }
    }
}
