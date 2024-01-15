using Kezyma.Core.Utilities.DataFile;
using Kezyma.MorrowindSharedCode.Exports;
using Kezyma.MorrowindSharedCode.Voice;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kezyma.MorrowindLineExtractor.Services
{
    public class GeneratorConfig
    {
        public GeneratorConfig() { }
        public string API_Key { get; set; }
    }

    public class GeneratorBody
    {
        public string text { get; set; }
    }

    internal class LineGenerator
    {
        internal LineGenerator() :this(false)
        {

        }

        internal LineGenerator(bool existingOnly, bool organise = false)
        {
            ExistingOnly = existingOnly;
            Organise = organise;
        }

        private bool ExistingOnly { get; set; }
        private bool Organise { get; set; }

        public const string VoiceListUrl = "https://api.elevenlabs.io/v1/voices";
        public static string VoiceGenerateUrl(string voiceId) => $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        public void GenerateLines(string fileName)
        {
            var csvPath = Path.GetFullPath("Csv");
            var filePath = Path.Combine(csvPath, fileName);

            var gPath = Path.GetFullPath("Generated");
            if (!Directory.Exists(gPath)) Directory.CreateDirectory(gPath);
            var fPath = Path.Combine(gPath, fileName);
            if (!Directory.Exists(fPath)) Directory.CreateDirectory(fPath);
            var sounds = Path.Combine(fPath, "Sound");
            if (!Directory.Exists(sounds)) Directory.CreateDirectory(sounds);
            var vo = Path.Combine(sounds, "Vo");
            if (!Directory.Exists(vo)) Directory.CreateDirectory(vo);
            var aiv = Path.Combine(vo, "AIV");
            if (!Directory.Exists(aiv)) Directory.CreateDirectory(aiv);

            var configPath = Path.GetFullPath("GeneratorConfig.json");
            if (!File.Exists(configPath))
            {
                var cfg = new GeneratorConfig();
                File.WriteAllText(configPath, JsonConvert.SerializeObject(cfg, Formatting.Indented));
            }

            var config = JsonConvert.DeserializeObject<GeneratorConfig>(File.ReadAllText(configPath));
            if (!string.IsNullOrWhiteSpace(config.API_Key) || ExistingOnly)
            {
                var vList = new List<VoiceListItem>();
                if (!ExistingOnly)
                {
                    Console.WriteLine("Loading list of existing voices.");
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("xi-api-key", config.API_Key);
                        var data = client.GetAsync(VoiceListUrl).Result?.Content?.ReadAsStringAsync().Result;
                        vList = JsonConvert.DeserializeObject<Voices>(data).voices.ToList();
                    }
                    Console.WriteLine("List of voices loaded.");
                }

                Console.WriteLine($"Loading csv {fileName}");
                var exportItems = new CsvCollection<DialogueLine>(filePath);
                var invalidChars = new[] { "[", "]", "*", "%" };
                var cleanLines = exportItems.Where(x =>
                !string.IsNullOrWhiteSpace(x.DialogueText) &&
                //!string.IsNullOrWhiteSpace(x.Race) &&
                //!string.IsNullOrWhiteSpace(x.Gender) &&
                !x.Done &&
                !x.Conflict).ToList();
                var raceGroups = cleanLines.GroupBy(x => string.IsNullOrWhiteSpace(x.Race) ? "Creature" : x.Race);
                var done = 0;
                var total = cleanLines.Count;
                foreach (var raceGroup in raceGroups)
                {
                    var genderGroups = raceGroup.GroupBy(x => x.Gender);
                    foreach (var genderGroup in genderGroups)
                    {
                        if (Organise)
                        {
                            var oPath = Path.GetFullPath("Generated");
                            if (!Directory.Exists(oPath)) Directory.CreateDirectory(oPath);
                            oPath = Path.Combine(oPath, "Organised");
                            if (!Directory.Exists(fPath)) Directory.CreateDirectory(oPath);
                            oPath = Path.Combine(oPath, "Sound");
                            if (!Directory.Exists(oPath)) Directory.CreateDirectory(oPath);
                            oPath = Path.Combine(oPath, "Vo");
                            if (!Directory.Exists(oPath)) Directory.CreateDirectory(oPath);
                            oPath = Path.Combine(oPath, "AIV");
                            if (!Directory.Exists(oPath)) Directory.CreateDirectory(oPath);

                            var dialogueGroups = genderGroup.GroupBy(x => x.DialogueText.Trim()).ToArray();
                            foreach (var uniqueDialogue in dialogueGroups)
                            {
                                var itemDict = new Dictionary<string, DialogueLine>();
                                var targetPaths = new List<string>();
                                foreach (var dialogueItem in uniqueDialogue)
                                {
                                    var basePath = aiv;
                                    var mvPath = oPath;
                                    var gpath = Path.Combine(basePath, string.IsNullOrWhiteSpace(dialogueItem.Race) ? "Creature" : dialogueItem.Race);
                                    mvPath = Path.Combine(mvPath, string.IsNullOrWhiteSpace(dialogueItem.Race) ? "Creature" : dialogueItem.Race);
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!Directory.Exists(mvPath)) Directory.CreateDirectory(mvPath);
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.Gender))
                                    {
                                        gpath = Path.Combine(gpath, dialogueItem.Gender[0].ToString());
                                        mvPath = Path.Combine(mvPath, dialogueItem.Gender[0].ToString());
                                    }
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!Directory.Exists(mvPath)) Directory.CreateDirectory(mvPath);
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.SpeakerId))
                                    {
                                        gpath = Path.Combine(gpath, dialogueItem.SpeakerId);
                                        mvPath = Path.Combine(mvPath, dialogueItem.SpeakerId);
                                    }
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!Directory.Exists(mvPath)) Directory.CreateDirectory(mvPath);
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.FactionId))
                                    {
                                        var factionId = dialogueItem.FactionId;
                                        gpath = Path.Combine(gpath, factionId);
                                        mvPath = Path.Combine(mvPath, factionId);
                                    }
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!Directory.Exists(mvPath)) Directory.CreateDirectory(mvPath);
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.FactionRank))
                                    {
                                        gpath = Path.Combine(gpath, dialogueItem.FactionRank);
                                        mvPath = Path.Combine(mvPath, dialogueItem.FactionRank);
                                    }
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!Directory.Exists(mvPath)) Directory.CreateDirectory(mvPath);
                                    var fpath = Path.Combine(gpath, $"{dialogueItem.InfoId.Replace("_", "")}.mp3");
                                    mvPath = Path.Combine(mvPath, $"{dialogueItem.InfoId.Replace("_", "")}.mp3");
                                    itemDict.Add(fpath, dialogueItem);
                                    done++;
                                    if (done % 100 == 0)
                                    {
                                        Console.WriteLine($"{done} / {total}");
                                    }

                                    targetPaths.Add(mvPath);
                                }
                                GenerateLines(itemDict, null, uniqueDialogue.Key, null);
                                var firstFile = itemDict.Keys.FirstOrDefault();
                                if (itemDict.Any(x => x.Value.Done))
                                {

                                }
                                if (!string.IsNullOrWhiteSpace(firstFile) && File.Exists(firstFile))
                                {
                                    foreach (var t in targetPaths)
                                    {
                                        File.Copy(firstFile, t, true);
                                    }
                                    foreach (var key in itemDict.Keys)
                                    {
                                        File.Delete(key);
                                    }
                                }
                            }

                        }
                        if (ExistingOnly)
                        {
                            var dialogueGroups = genderGroup.GroupBy(x => x.DialogueText.Trim()).ToArray();
                            foreach (var uniqueDialogue in dialogueGroups)
                            {
                                var itemDict = new Dictionary<string, DialogueLine>();
                                foreach (var dialogueItem in uniqueDialogue)
                                {
                                    var basePath = aiv;
                                    var gpath = Path.Combine(basePath, string.IsNullOrWhiteSpace(dialogueItem.Race) ? "Creature" : dialogueItem.Race);
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.Gender))
                                    {
                                        gpath = Path.Combine(gpath, dialogueItem.Gender[0].ToString());
                                    }
                                    if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.SpeakerId))
                                    {
                                        gpath = Path.Combine(gpath, dialogueItem.SpeakerId);
                                        if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    }
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.FactionId))
                                    {
                                        var factionId = dialogueItem.FactionId;
                                        gpath = Path.Combine(gpath, factionId);
                                        if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    }
                                    if (!string.IsNullOrWhiteSpace(dialogueItem.FactionRank))
                                    {
                                        gpath = Path.Combine(gpath, dialogueItem.FactionRank);
                                        if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                    }
                                    var fpath = Path.Combine(gpath, $"{dialogueItem.InfoId.Replace("_", "")}.mp3");
                                    itemDict.Add(fpath, dialogueItem);
                                    done++;
                                    if (done % 100 == 0)
                                    {
                                        Console.WriteLine($"{done} / {total}");
                                    }
                                }

                                GenerateLines(itemDict, null, uniqueDialogue.Key, null);
                            }
                        }
                        else
                        {
                            var voiceString = $"{raceGroup.Key} - {genderGroup.Key}";
                            var voiceItem = vList.FirstOrDefault(x => x.name == voiceString);
                            if (voiceItem == null)
                            {
                                Console.WriteLine($"Voice not found {voiceString}");
                                continue;
                            }

                            var dialogueGroups = genderGroup.GroupBy(x => x.DialogueText.Trim()).ToArray();
                            foreach (var uniqueDialogue in dialogueGroups)
                            {
                                if (!invalidChars.Any(x => uniqueDialogue.Key.Contains(x)))
                                {
                                    var itemDict = new Dictionary<string, DialogueLine>();
                                    foreach (var dialogueItem in uniqueDialogue)
                                    {
                                        var basePath = aiv;
                                        var gpath = Path.Combine(basePath, string.IsNullOrWhiteSpace(dialogueItem.Race) ? "Creature" : dialogueItem.Race);
                                        if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                        if (!string.IsNullOrWhiteSpace(dialogueItem.Gender))
                                        {
                                            gpath = Path.Combine(gpath, dialogueItem.Gender[0].ToString());
                                        }
                                        if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                        if (!string.IsNullOrWhiteSpace(dialogueItem.SpeakerId))
                                        {
                                            gpath = Path.Combine(gpath, dialogueItem.SpeakerId);
                                            if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                        }
                                        if (!string.IsNullOrWhiteSpace(dialogueItem.FactionId))
                                        {
                                            var factionId = dialogueItem.FactionId;
                                            gpath = Path.Combine(gpath, factionId);
                                            if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                        }
                                        if (!string.IsNullOrWhiteSpace(dialogueItem.FactionRank))
                                        {
                                            gpath = Path.Combine(gpath, dialogueItem.FactionRank);
                                            if (!Directory.Exists(gpath)) Directory.CreateDirectory(gpath);
                                        }
                                        var fpath = Path.Combine(gpath, $"{dialogueItem.InfoId.Replace("_", "")}.mp3");
                                        itemDict.Add(fpath, dialogueItem);

                                        done++;
                                        if (done % 100 == 0)
                                        {
                                            Console.WriteLine($"{done} / {total}");
                                        }
                                    }

                                    GenerateLines(itemDict, voiceItem.voice_id, uniqueDialogue.Key, config.API_Key);
                                    exportItems.Save();
                                }
                            }
                        }
                    }
                }

                exportItems.Save();
            }
        }

        private void GenerateLines(Dictionary<string, DialogueLine> items, string voice, string dialogue, string apiKey)
        {
            if (items.Any(i => File.Exists(i.Key)))
            {
                var existing = items.Where(x => File.Exists(x.Key)).ToList();
                foreach (var existingItem in existing)
                {
                    existingItem.Value.Done = true;
                }
                var missing = items.Where(x => !File.Exists(x.Key)).ToList();
                foreach (var missingItem in missing)
                {
                    File.Copy(existing.First().Key, missingItem.Key);
                    missingItem.Value.Done = true;
                }
            }
            else if (!ExistingOnly)
            {
                using (var client = new HttpClient())
                {
                    Console.WriteLine($"Generating ({items.First().Value.Race} - {items.First().Value.Gender}): {dialogue}");
                    client.DefaultRequestHeaders.Add("xi-api-key", apiKey);
                    var post = client.PostAsync(VoiceGenerateUrl(voice), new StringContent(JsonConvert.SerializeObject(new GeneratorBody { text = dialogue }), Encoding.UTF8, "application/json"));
                    var res = post.Result;
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var fileStream = res.Content.ReadAsStreamAsync().Result;

                        var first = items.First();
                        using (var fs = File.Create(first.Key))
                        {
                            fileStream.CopyTo(fs);
                            fs.Close();
                        }
                        first.Value.Done = true;

                        foreach (var item in items.Skip(1))
                        {
                            File.Copy(first.Key, item.Key);
                            item.Value.Done = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("API Limit exhausted.");
                        Console.ReadLine();
                        return;
                    }
                }
            }
        }
    }
}
