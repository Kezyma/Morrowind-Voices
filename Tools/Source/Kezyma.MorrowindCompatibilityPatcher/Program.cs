using Kezyma.MorrowindSharedCode.Exports;
using Kezyma.MorrowindSharedCode.Helpers;

namespace Kezyma.MorrowindCompatibilityPatcher
{
    internal class Program
    {
        private static string[] GameFiles = new[]
        {
            "Morrowind.esm",
            "Tribunal.esm",
            "Bloodmoon.esm",
            "adamantiumarmor.esp",
            "AreaEffectArrows.esp",
            "bcsounds.esp",
            "EBQ_Artifact.esp",
            "entertainers.esp",
            "LeFemmArmor.esp",
            "master_index.esp",
            "Siege at Firemoth.esp",
            //"Patch for Purists.esm"
        };

        static bool Compare(string a, string b)
        {
            return Clean(a) == Clean(b);
        }

        static string Clean(string a)
        {
            return a.Trim().ToLower().Replace("!", "").Replace(".", "").Replace(",", "").Replace("*", "");
        }

        static void Main(string[] args)
        {
            var patchFile = args[0].Replace("\"", "");
            var patchOutput = args[1].Replace("\"", "");
            var soundPath = Path.Combine(Path.GetFullPath("Data Files"), "Sound");
            if (!Directory.Exists(soundPath)) soundPath = Path.GetFullPath("Sound");
            if (Directory.Exists(soundPath))
            {
                // Get all required paths.
                var aivPath = Path.Combine(soundPath, "Vo", "AIV");
                var dataFilesPath = Directory.GetParent(soundPath).FullName;
                var patchPath = Path.Combine(dataFilesPath, patchFile);
                var patchOutputPath = dataFilesPath;
                if (!string.IsNullOrWhiteSpace(patchOutput)) patchOutputPath = Path.Combine(dataFilesPath, patchOutput);

                if (File.Exists(patchPath))
                {
                    var espEsmFiles = Directory.GetFiles(dataFilesPath).Where(x => x.EndsWith("esp") || x.EndsWith("esm")).ToList();
                    var baseEspEsmFiles = espEsmFiles.Where(x => GameFiles.Contains(Path.GetFileName(x))).ToList();
                    if (patchFile.ToLower() != "Patch for Purists.esm".ToLower())
                    {
                        var pfp = espEsmFiles.FirstOrDefault(x => Path.GetFileName(x) == "Patch for Purists.esm");
                        if (pfp != null) baseEspEsmFiles.Add(pfp);
                    }

                    var tes3conv = ExportHelper.GetTes3Conv();
                    var jsonPath = Path.Combine(dataFilesPath, "json_cache_temp");
                    Directory.CreateDirectory(jsonPath);

                    // Export the json for the game files.
                    var factions = new Dictionary<string, Root>();
                    var npcs = new List<Root>();
                    var dialogue = new List<Root>();
                    foreach (var file in baseEspEsmFiles)
                    {
                        var jsonFile = ExportHelper.ExportEspJson(tes3conv, file, jsonPath);
                        npcs.AddRange(ExportHelper.ExtractJsonNPCs(jsonFile));
                        dialogue.AddRange(ExportHelper.ExtractJsonDialogue(jsonFile));
                        var facs = ExportHelper.ExtractJsonFactions(jsonFile);
                        foreach (var f in facs) factions[f.id] = f;
                    }

                    var races = npcs.Select(x => x.race).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

                    // Get data from patch file.
                    var patchJson = ExportHelper.ExportEspJson(tes3conv, patchPath, jsonPath);
                    var patchDialogue = ExportHelper.ExtractJsonDialogue(patchJson);

                    // Remove any dialogue with the same info_id from files to patch.
                    var changedDialogue = patchDialogue.Where(x =>
                            dialogue.Any(y =>
                                y.info_id != x.info_id &&
                                y.speaker_id == x.speaker_id &&
                                !string.IsNullOrWhiteSpace(y.text) &&
                                !string.IsNullOrWhiteSpace(x.text) &&
                                Compare(y.text, x.text))
                            ).ToList();

                    // Loop through dialogue to patch.
                    foreach (var d in changedDialogue)
                    {
                        var existingDialogue = dialogue.FirstOrDefault(y =>
                                y.info_id != d.info_id &&
                                y.speaker_id == d.speaker_id &&
                                !string.IsNullOrWhiteSpace(y.text) &&
                                !string.IsNullOrWhiteSpace(d.text) &&
                                Compare(y.text, d.text));

                        if (d.data != null && d.data.dialogue_type != "Journal" && d.data.dialogue_type != "Voice")
                        {
                            var pathsToCheck = new List<string>
                            {
                                Path.Combine(aivPath, "creature", $"{existingDialogue.info_id}.mp3")
                            };
                            if (!string.IsNullOrWhiteSpace(d.speaker_id))
                            {
                                pathsToCheck.Add(Path.Combine(aivPath, "creature", existingDialogue.speaker_id, $"{existingDialogue.info_id}.mp3"));

                                var speaker = npcs.FirstOrDefault(x => x.id == existingDialogue.speaker_id);
                                if (speaker != null)
                                {
                                    var race = speaker.race;
                                    var faction = speaker.faction;
                                    if (speaker.npc_flags.HasValue)
                                    {
                                        var flags = (NPC_Flag)speaker.npc_flags;
                                        var gender = "m";
                                        if (flags.HasFlag(NPC_Flag.Female)) gender = "f";

                                        pathsToCheck.Add(Path.Combine(aivPath, race, gender, $"{existingDialogue.info_id}.mp3"));
                                        pathsToCheck.Add(Path.Combine(aivPath, race, gender, d.speaker_id, $"{existingDialogue.info_id}.mp3"));

                                        if (!string.IsNullOrWhiteSpace(faction))
                                        {
                                            for (int i = 0; i <= 9; i++)
                                            {
                                                pathsToCheck.Add(Path.Combine(aivPath, race, gender, faction, i.ToString(), $"{existingDialogue.info_id}.mp3"));
                                                pathsToCheck.Add(Path.Combine(aivPath, race, gender, d.speaker_id, faction, i.ToString(), $"{existingDialogue.info_id}.mp3"));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var race in races)
                                {
                                    pathsToCheck.Add(Path.Combine(aivPath, race, "m", $"{existingDialogue.info_id}.mp3"));
                                    pathsToCheck.Add(Path.Combine(aivPath, race, "f", $"{existingDialogue.info_id}.mp3"));
                                    foreach (var npc in npcs.Where(x => x.race == race))
                                    {
                                        var flags = (NPC_Flag)npc.npc_flags;
                                        var gender = "m";
                                        if (flags.HasFlag(NPC_Flag.Female)) gender = "f";
                                        pathsToCheck.Add(Path.Combine(aivPath, race, gender, npc.id, $"{existingDialogue.info_id}.mp3"));

                                        if (!string.IsNullOrWhiteSpace(npc.faction))
                                        {
                                            for (int i = 0; i <= 9; i++)
                                            {
                                                pathsToCheck.Add(Path.Combine(aivPath, race, gender, npc.faction, i.ToString(), $"{existingDialogue.info_id}.mp3"));
                                                pathsToCheck.Add(Path.Combine(aivPath, race, gender, npc.id, npc.faction, i.ToString(), $"{existingDialogue.info_id}.mp3"));
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var f in pathsToCheck)
                            {
                                if (File.Exists(f))
                                {
                                    var aivDestPath = Path.Combine(patchOutputPath, "Sound", "Vo", "AIV");
                                    if (!Directory.Exists(aivDestPath)) Directory.CreateDirectory(aivDestPath);

                                    var newPath = f.Replace(aivPath, aivDestPath);
                                    var dirName = Directory.GetParent(newPath).FullName;
                                    if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                                    newPath = Path.Combine(dirName, $"{d.info_id}.mp3");
                                    File.Copy(f, newPath, true);
                                }
                            }
                        }
                    }

                    File.Delete(tes3conv);
                    Directory.Delete(jsonPath, true);
                }
                else
                {
                    Console.WriteLine($"Could not find file {patchFile} inside 'Data Files'.");
                }
            }
            else
            {
                Console.WriteLine("Data Files directory not found. Ensure this exe is placed inside the 'Morrowind' game folder.");
            }
            Console.WriteLine("Patch Complete.");
            Console.ReadLine();
        }
    }
}