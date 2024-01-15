using Kezyma.Core.Utilities.Collection;
using Kezyma.Core.Utilities.Extensions;
using Kezyma.MorrowindSharedCode.Exports;
using Kezyma.MorrowindSharedCode.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kezyma.MorrowindLineExtractor.Services
{
    public class DialogueLine
    {
        public bool Done { get; set; }
        public bool Conflict { get; set; }
        public string Source { get; set; }
        public string InfoId { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public string SpeakerId { get; set; }
        public string FactionId { get; set; }
        public string FactionRank { get; set; }
        public string DialogueText { get; set; }

        public bool Equals(DialogueLine line)
        {
            return
                InfoId == line.InfoId &&
                Race == line.Race &&
                Gender == line.Gender &&
                SpeakerId == line.SpeakerId &&
                FactionId == line.FactionId &&
                FactionRank == line.FactionRank &&
                DialogueText.Trim().ToLower() == line.DialogueText.Trim().ToLower();
        }
    }

    internal class LineExtractor
    {
        internal LineExtractor() :this(true) { }
        internal LineExtractor(bool extractGame = true)
        {
            ExtractGame = extractGame;
        }
        private bool ExtractGame { get; set; }

        private static bool Contains(string a, string b) => a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
        private static string Replace(string a, string b, string c) => a.Replace(b, c, StringComparison.InvariantCultureIgnoreCase);

        private static string ClearNewLines(string a) => a.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

        private Dictionary<string, List<Root>> _dialogue = new();
        private Dictionary<string, Root> _npcs = new();
        private Dictionary<string, Root> _factions = new();
        private Dictionary<string, Root> _regions = new();
        private Dictionary<string, Root> _cells = new();

        private ConcurrentQueue<DialogueLine> _brokenLines = new();
        private string ExtractNpcGender(Root npc) => npc.npc_flags.HasValue ? ((NPC_Flag)npc.npc_flags).HasFlag(NPC_Flag.Female) ? "Female" : "Male" : "";

        private ConcurrentQueue<DialogueLine> ExtractForSpeaker(Root dialogue, string gameFile, string speakerId)
        {
            var dialogueLines = new ConcurrentQueue<DialogueLine>();
            var speakerNpc = _npcs[speakerId];
            var dialogueText = ClearNewLines(dialogue.text);
            if (Contains(dialogueText, "%Name")) dialogueText = Replace(dialogueText, "%Name", speakerNpc.name);
            if (Contains(dialogueText, "%Race")) dialogueText = Replace(dialogueText, "%Race", speakerNpc.race);
            if (Contains(dialogueText, "%Class")) dialogueText = Replace(dialogueText, "%Class", speakerNpc.@class);
            if (Contains(dialogueText, "%Rank"))
            {
                var speakerFactionId = speakerNpc.faction;
                var speakerRankId = speakerNpc.data.rank;
                if (speakerFactionId != null && speakerRankId.HasValue && _factions.ContainsKey(speakerFactionId))
                {
                    var speakerFaction = _factions[speakerFactionId];
                    var speakerRank = speakerFaction.rank_names[speakerRankId.Value];
                    dialogueText = Replace(dialogueText, "%Rank", speakerRank);
                }
                else
                {
                    // Something is wrong with the ranks.
                }
            }
            if (Contains(dialogueText, "%Faction"))
            {
                var playerFactionId = dialogue.player_faction;
                var speakerFactionId = speakerNpc.faction;
                var dialogueFactionId = !string.IsNullOrWhiteSpace(playerFactionId) ? playerFactionId : speakerFactionId;
                if (!string.IsNullOrWhiteSpace(dialogueFactionId) && _factions.ContainsKey(dialogueFactionId))
                {
                    var dialogueFaction = _factions[dialogueFactionId];
                    dialogueText = Replace(dialogueText, "%Faction", dialogueFaction.name);
                }
                else
                {
                    // Something is wrong with the factions.
                }
            }
            if (Contains(dialogueText, "%PCRank") || Contains(dialogueText, "%NextPCRank"))
            {
                // Rank specific lines required.
                var playerFactionId = dialogue.player_faction;
                var speakerFactionId = speakerNpc.faction;
                var dialogueFactionId = !string.IsNullOrWhiteSpace(playerFactionId) ? playerFactionId : speakerFactionId;
                if (!string.IsNullOrWhiteSpace(dialogueFactionId))
                {
                    var dialogueFaction = _factions[dialogueFactionId];
                    if (dialogue.data.player_rank.HasValue && dialogue.data.player_rank.Value >= 0)
                    {
                        // Only needs a single line
                        var playerRank = dialogueFaction.rank_names[dialogue.data.player_rank.Value];
                        dialogueText = Replace(dialogueText, "%PCRank", playerRank);
                        if (dialogue.data.player_rank.Value < dialogueFaction.rank_names.Count - 1) dialogueText = Replace(dialogueText, "%NextPCRank", dialogueFaction.rank_names[dialogue.data.player_rank.Value + 1]);
                        dialogueLines.Enqueue(new DialogueLine
                        {
                            DialogueText = dialogueText,
                            Source = gameFile,
                            SpeakerId = speakerNpc.id,
                            Race = speakerNpc.race,
                            Gender = ExtractNpcGender(speakerNpc),
                            InfoId = $"_{dialogue.info_id}"
                        });
                    }
                    else
                    {
                        var maxRank = dialogueFaction.rank_names.Count - (Contains(dialogueText, "%NextPCRank") ? 1 : 0);
                        for (int i = 0; i < maxRank; i++)
                        {
                            var rankName = dialogueFaction.rank_names[i];
                            var rankText = Replace(dialogueText, "%PCRank", rankName);

                            if (Contains(rankText, "%NextPCRank"))
                            {
                                var nextRankName = dialogueFaction.rank_names[i + 1];
                                rankText = Replace(rankText, "%NextPCRank", nextRankName);
                            }

                            dialogueLines.Enqueue(new DialogueLine
                            {
                                DialogueText = rankText,
                                Source = gameFile,
                                SpeakerId = speakerNpc.id,
                                Race = speakerNpc.race,
                                Gender = ExtractNpcGender(speakerNpc),
                                InfoId = $"_{dialogue.info_id}",
                                FactionId = dialogueFaction.id,
                                FactionRank = i.ToString()
                            });
                        }
                    }

                }
                else
                {
                    _brokenLines.Enqueue(new DialogueLine
                    {
                        DialogueText = dialogueText,
                        Source = gameFile,
                        SpeakerId = speakerNpc.id,
                        Race = speakerNpc.race,
                        Gender = ExtractNpcGender(speakerNpc),
                        InfoId = $"_{dialogue.info_id}",
                        FactionId = dialogueFactionId,
                        FactionRank = ""
                    });
                    // Something is wrong with the factions.
                }
            }
            else
            {
                // Single line.
                dialogueLines.Enqueue(new DialogueLine
                {
                    DialogueText = dialogueText,
                    Source = gameFile,
                    SpeakerId = speakerNpc.id,
                    Race = speakerNpc.race,
                    Gender = speakerNpc.npc_flags.HasValue ? ((NPC_Flag)speakerNpc.npc_flags).HasFlag(NPC_Flag.Female) ? "Female" : "Male" : "",
                    InfoId = $"_{dialogue.info_id}",
                });
            }
            return dialogueLines;
        }
        private ConcurrentQueue<DialogueLine> ExtractForRaceGender(Root dialogue, string gameFile, string race, string gender, string[] factions)
        {
            var dialogueLines = new ConcurrentQueue<DialogueLine>();
            var dialogueText = ClearNewLines(dialogue.text);
            if (!Contains(dialogueText, "%Faction") && !Contains(dialogueText, "%PCRank") && !Contains(dialogueText, "%NextPCRank") || !string.IsNullOrWhiteSpace(dialogue.player_faction)) factions = new[] { "" };
            foreach (var faction in factions)
            {
                if (Contains(dialogueText, "%Faction"))
                {
                    var playerFactionId = dialogue.player_faction;
                    var speakerFactionId = faction;
                    var dialogueFactionId = !string.IsNullOrWhiteSpace(playerFactionId) ? playerFactionId : speakerFactionId;
                    if (!string.IsNullOrWhiteSpace(dialogueFactionId))
                    {
                        var dialogueFaction = _factions[dialogueFactionId];
                        dialogueText = Replace(dialogueText, "%Faction", dialogueFaction.name);
                    }
                    else
                    {
                        // Something is wrong with the factions.
                    }
                }
                if (Contains(dialogueText, "%PCRank") || Contains(dialogueText, "%NextPCRank"))
                {
                    // Rank specific lines required.
                    var playerFactionId = dialogue.player_faction;
                    var speakerFactionId = faction;
                    var dialogueFactionId = !string.IsNullOrWhiteSpace(playerFactionId) ? playerFactionId : speakerFactionId;
                    if (!string.IsNullOrWhiteSpace(dialogueFactionId))
                    {
                        var dialogueFaction = _factions[dialogueFactionId];
                        if (dialogue.data.player_rank.HasValue && dialogue.data.player_rank.Value >= 0)
                        {
                            // Only needs a single line
                            var playerRank = dialogueFaction.rank_names[dialogue.data.player_rank.Value];
                            dialogueText = Replace(dialogueText, "%PCRank", playerRank);
                            if (dialogue.data.player_rank.Value < dialogueFaction.rank_names.Count - 1) dialogueText = Replace(dialogueText, "%NextPCRank", dialogueFaction.rank_names[dialogue.data.player_rank.Value + 1]);
                            dialogueLines.Enqueue(new DialogueLine
                            {
                                DialogueText = dialogueText,
                                Source = gameFile,
                                Race = race,
                                Gender = gender,
                                InfoId = $"_{dialogue.info_id}",
                            });
                        }
                        else
                        {
                            var maxRank = dialogueFaction.rank_names.Count - (Contains(dialogueText, "%NextPCRank") ? 1 : 0);
                            for (int i = 0; i < maxRank; i++)
                            {
                                var rankName = dialogueFaction.rank_names[i];
                                var rankText = Replace(dialogueText, "%PCRank", rankName);

                                if (Contains(rankText, "%NextPCRank"))
                                {
                                    var nextRankName = dialogueFaction.rank_names[i + 1];
                                    rankText = Replace(rankText, "%NextPCRank", nextRankName);
                                }

                                dialogueLines.Enqueue(new DialogueLine
                                {
                                    DialogueText = rankText,
                                    Source = gameFile,
                                    Race = race,
                                    Gender = gender,
                                    InfoId = $"_{dialogue.info_id}",
                                    FactionId = dialogueFaction.id,
                                    FactionRank = i.ToString()
                                });
                            }
                        }

                    }
                    else
                    {
                        _brokenLines.Enqueue(new DialogueLine
                        {
                            DialogueText = dialogueText,
                            Source = gameFile,
                            Race = race,
                            Gender = gender,
                            InfoId = $"_{dialogue.info_id}",
                            FactionId = dialogueFactionId,
                            FactionRank = ""
                        });
                        // Something is wrong with the factions.
                    }
                }
                else
                {
                    // Single line.
                    dialogueLines.Enqueue(new DialogueLine
                    {
                        DialogueText = dialogueText,
                        Source = gameFile,
                        Race = race,
                        Gender = gender,
                        InfoId = $"_{dialogue.info_id}",
                    });
                }
            }
            return dialogueLines;
        }
        private ConcurrentQueue<DialogueLine> ExtractDialogue(string gameFile)
        {
            Console.WriteLine($"Extracting dialogue from {gameFile}");
            var dialogueLines = new ConcurrentQueue<DialogueLine>();
            var total = _dialogue[gameFile].Count;
            var done = 0;
            var log = 1;
            var time = new Stopwatch();
            time.Start();
            Parallel.ForEach(_dialogue[gameFile], dialogue =>
            {
                //foreach (var dialogue in _dialogue[gameFile])
                //{
                if (!string.IsNullOrWhiteSpace(dialogue.text))
                {
                    // Calculate all possible variations of each line and export.
                    if (!string.IsNullOrWhiteSpace(dialogue.speaker_id) && _npcs.ContainsKey(dialogue.speaker_id))
                    {
                        // This is dialogue specific to a single NPC.
                        var newLines = ExtractForSpeaker(dialogue, gameFile, dialogue.speaker_id);
                        foreach (var line in newLines) dialogueLines.Enqueue(line);
                        //dialogueLines.AddRange(newLines);
                    }
                    else
                    {
                        var validNpcs = _npcs.Where(x => x.Value.type == "Npc");

                        var dialogueFaction = dialogue.player_faction;
                        //if (!string.IsNullOrWhiteSpace(dialogueFaction)) validNpcs = validNpcs.Where(x => x.Value.faction == dialogueFaction);

                        var speakerFaction = dialogue.speaker_faction;
                        if (!string.IsNullOrWhiteSpace(speakerFaction)) validNpcs = validNpcs.Where(x => x.Value.faction == speakerFaction);

                        var speakerRace = dialogue.speaker_rank;
                        if (!string.IsNullOrWhiteSpace(speakerRace)) validNpcs = validNpcs.Where(x => x.Value.race == speakerRace);

                        var speakerGender = dialogue.data?.speaker_sex ?? "Any";
                        if (!string.IsNullOrWhiteSpace(speakerGender) && speakerGender != "Any") validNpcs = validNpcs.Where(x => speakerGender == ExtractNpcGender(x.Value));

                        if (Contains(dialogue.text, "%Faction") || Contains(dialogue.text, "%Rank")) validNpcs = validNpcs.Where(x => !string.IsNullOrWhiteSpace(x.Value.faction));
                        if (Contains(dialogue.text, "%Rank")) validNpcs = validNpcs.Where(x => _factions[x.Value.faction].rank_names.Count > 0);

                        if (dialogue.filters != null && dialogue.filters.Any(f => f.id == "NoLore" || f.id == "nolore"))
                        {
                            var nolore = dialogue.filters.First(f => f.id == "NoLore" || f.id == "nolore");
                            var noloreValue = nolore.value.Integer;
                            if (nolore.filter_comparison == "NotEqual")
                            {
                                if (noloreValue == 1) noloreValue = 0;
                                else noloreValue = 1;
                            }

                            validNpcs = validNpcs.Where(x => (x.Value.script == "nolore" || x.Value.script == "NoLore" ? 1 : 0) == noloreValue);
                        }

                        var dialogueCell = dialogue.speaker_cell;
                        if (!string.IsNullOrWhiteSpace(dialogueCell))
                        {
                            var cellParts = dialogueCell.Split(",").Select(y => y.Trim()).ToArray();
                            var validCells = _cells.Keys.Where(x => cellParts.All(p => x.Split(",").Select(y => y.Trim()).Any(s => s.StartsWith(p) || p == "Tel Fyr" && s.Contains(p)))).ToArray();
                            var validCellReferences = validCells.SelectMany(x => _cells[x].references.Select(r => r.id));
                            while (!validNpcs.Any(v => validCellReferences.Contains(v.Value.id)) && cellParts.Any())
                            {
                                var partsConcurrentQueue = cellParts.ToList();
                                partsConcurrentQueue.Reverse();
                                partsConcurrentQueue = partsConcurrentQueue.Skip(1).ToList();
                                partsConcurrentQueue.Reverse();
                                cellParts = partsConcurrentQueue.ToArray();

                                validCells = _cells.Keys.Where(x => cellParts.All(p => x.Split(",").Select(y => y.Trim()).Any(s => s.StartsWith(p) || p == "Tel Fyr" && s.Contains(p)))).ToArray();
                                validCellReferences = validCells.SelectMany(x => _cells[x].references.Select(r => r.id));
                            }

                            if (validCellReferences.Any()) validNpcs = validNpcs.Where(x => validCellReferences.Contains(x.Value.id));
                        }

                        if (dialogue.data != null)
                        {
                            var speakerRank = dialogue.data.speaker_rank;
                            if (speakerRank.HasValue && speakerRank.Value >= 0)
                                if (validNpcs.Any(x => x.Value.data.rank >= speakerRank.Value))
                                    validNpcs = validNpcs.Where(x => x.Value.data.rank >= speakerRank.Value);
                        }

                        var speakerClass = dialogue.speaker_class;
                        if (!string.IsNullOrWhiteSpace(speakerClass))
                            if (validNpcs.Any(v => v.Value.@class == speakerClass))
                                validNpcs = validNpcs.Where(x => x.Value.@class == speakerClass);

                        var speakerNpcs = validNpcs.ToArray();
                        if (speakerNpcs.Any())
                        {
                            if (Contains(dialogue.text, "%Name") || Contains(dialogue.text, "%Class") || Contains(dialogue.text, "%Faction") || Contains(dialogue.text, "%Rank"))
                            {
                                if (Contains(dialogue.text, "%Faction") || Contains(dialogue.text, "%Rank")) // Filter out factionless NPCs.
                                {
                                    validNpcs = validNpcs.Where(x => !string.IsNullOrWhiteSpace(x.Value.faction));
                                }

                                // Requires NPC specific lines.
                                foreach (var speaker in speakerNpcs)
                                    foreach (var line in ExtractForSpeaker(dialogue, gameFile, speaker.Value.id))
                                        dialogueLines.Enqueue(line);
                                //dialogueLines.AddRange();
                            }
                            else
                            {
                                // Can have generic voice lines.
                                var raceGenderPairs = validNpcs.Select(x => $"{x.Value.race}|{ExtractNpcGender(x.Value)}").Distinct().ToList();
                                var factionClusters = raceGenderPairs.ToDictionary(x => x, x => validNpcs.Where(v => $"{v.Value.race}|{ExtractNpcGender(v.Value)}" == x).Select(v => v.Value.faction).Distinct().ToArray());
                                foreach (var factionCluster in factionClusters)
                                {
                                    var r = factionCluster.Key.Split("|")[0];
                                    var g = factionCluster.Key.Split("|")[1];
                                    foreach (var line in ExtractForRaceGender(dialogue, gameFile, r, g, factionCluster.Value))
                                        dialogueLines.Enqueue(line);
                                    //dialogueLines.AddRange();
                                }
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(speakerRace) && !Contains(dialogue.text, "%Name") && !Contains(dialogue.text, "%Class") && !Contains(dialogue.text, "%Rank") &&
                            (!Contains(dialogue.text, "%Faction") || !string.IsNullOrWhiteSpace(speakerFaction) || !string.IsNullOrWhiteSpace(dialogueFaction)))
                        {
                            // Can still extract this line.
                            if (speakerGender == "Any" || speakerGender == "Male")
                                foreach (var line in ExtractForRaceGender(dialogue, gameFile, speakerRace, "Male", new[] { !string.IsNullOrWhiteSpace(dialogueFaction) ? dialogueFaction : speakerFaction }))
                                    dialogueLines.Enqueue(line);
                            //dialogueLines.AddRange();

                            if (speakerGender == "Any" || speakerGender == "Female")
                                foreach (var line in ExtractForRaceGender(dialogue, gameFile, speakerRace, "Male", new[] { !string.IsNullOrWhiteSpace(dialogueFaction) ? dialogueFaction : speakerFaction }))
                                    dialogueLines.Enqueue(line);
                            //dialogueLines.AddRange();
                        }
                        else
                        {
                            // Something is wrong.
                            _brokenLines.Enqueue(new DialogueLine
                            {
                                InfoId = $"_{dialogue.info_id}",
                                DialogueText = ClearNewLines(dialogue.text),
                                Source = gameFile,
                                FactionId = !string.IsNullOrWhiteSpace(dialogueFaction) ? dialogueFaction : speakerFaction,
                                FactionRank = dialogue.data?.player_rank?.ToString() ?? "",
                                Gender = speakerGender,
                                Race = speakerRace
                            });
                        }
                        // This dialogue may be shared between multiple NPCs.
                    }
                }
                //}
                if (time.Elapsed.TotalMinutes > log)
                {
                    log++;
                    Console.WriteLine($"{log}m - {done}/{total}");
                }
                done++;
            });
            return dialogueLines;
        }

        public void ExtractDialogue()
        {
            // Create required paths.
            var espPath = Path.GetFullPath("Esp");
            var jsonPath = Path.GetFullPath("Json");
            var csvPath = Path.GetFullPath("Csv");
            if (!Directory.Exists(csvPath)) Directory.CreateDirectory(csvPath);
            if (!Directory.Exists(espPath)) Directory.CreateDirectory(espPath);
            if (!Directory.Exists(jsonPath)) Directory.CreateDirectory(jsonPath);

            // Cleanup json files.
            foreach (var file in Directory.GetFiles(Path.GetFullPath("Json"))) File.Delete(file);

            // Get tes3conv.exe if it isn't already present.
            var tes3convpath = ExportHelper.GetTes3Conv();

            // Run tes3conv.exe to get json for all of the current esp/esm files
            foreach (var file in Directory.GetFiles(espPath)) ExportHelper.ExportEspJson(tes3convpath, file, jsonPath);

            // Extract all the relevant data from each esp.
            _dialogue = Directory.GetFiles(jsonPath).ToDictionary(x => Path.GetFileNameWithoutExtension(x), ExportHelper.ExtractJsonDialogue);
            var npcDict = Directory.GetFiles(jsonPath).ToDictionary(x => Path.GetFileNameWithoutExtension(x), ExportHelper.ExtractJsonNPCs);
            var factionDict = Directory.GetFiles(jsonPath).ToDictionary(x => Path.GetFileNameWithoutExtension(x), ExportHelper.ExtractJsonFactions);
            var regionDict = Directory.GetFiles(jsonPath).ToDictionary(x => Path.GetFileNameWithoutExtension(x), ExportHelper.ExtractJsonRegions);
            var cellDict = Directory.GetFiles(jsonPath).ToDictionary(x => Path.GetFileNameWithoutExtension(x), ExportHelper.ExtractJsonCells);

            // Sort out which are game and mod files.
            var gameFiles = new[] { "Morrowind", "Tribunal", "Bloodmoon", "adamantiumarmor", "AreaEffectArrows", "bcsounds", "EBQ_Artifact", "entertainers", "LeFemmArmor", "master_index", "Siege at Firemoth" };
            var modFiles = _dialogue.Keys.Where(x => !gameFiles.Contains(x)).ToArray();

            // Extract and overwrite data from each game file in order.
            _npcs = new Dictionary<string, Root>();
            _factions = new Dictionary<string, Root>();
            _regions = new Dictionary<string, Root>();
            _cells = new Dictionary<string, Root>();
            foreach (var file in gameFiles)
            {
                foreach (var npc in npcDict[file]) _npcs[npc.id] = npc;
                foreach (var faction in factionDict[file]) _factions[faction.id] = faction;
                foreach (var region in regionDict[file].Where(x => x.references != null)) _regions[region.id] = region;
                foreach (var cell in cellDict[file].Where(x => x.references != null))
                {
                    if (cell.references.Count > 0)
                    {
                        var cellId = "";
                        if (string.IsNullOrWhiteSpace(cell.id)) cellId = $"{cell.region},{cell.data.grid[0]}_{cell.data.grid[1]}";//  _cells[] = cell;
                        else cellId = $"{cell.region},{cell.id}";// _cells[] = cell;

                        if (_cells.ContainsKey(cellId)) _cells[cellId].references.AddRange(cell.references);
                        else _cells[cellId] = cell;
                    }
                }
            }

            // Iterate through dialogue
            var prefillTags = new[] { "%Name", "%Race", "%Class", "%Rank", "%Faction", "%PCRank", "%NextPCRank" };
            var gameDialogue = new ConcurrentQueue<DialogueLine>();
            if (ExtractGame)
            {
                foreach (var gameFile in gameFiles)
                    foreach (var line in ExtractDialogue(gameFile))
                        gameDialogue.Enqueue(line);
                //gameDialogue.AddRange(ExtractDialogue(gameFile));

                var gd = gameDialogue.ToList();
                var tr = new List<DialogueLine>();
                var lgroups = gd.GroupBy(x => $"{x.InfoId}{x.FactionRank}{x.Gender}{x.Race}").ToList();
                foreach (var lgroup in lgroups)
                {
                    var order = lgroup.OrderByDescending(x => gameFiles.ToList().IndexOf(x.Source));
                    if (order.GroupBy(x => x.Source).Any(x => x.Count() > 1))
                    {
                        // Oh no.
                    }
                    else
                    {
                        foreach (var itm in lgroup.Skip(1))
                        {
                            gd.Remove(itm);
                        }
                    }
                }
                var dupes = gd.GroupBy(x => $"{x.InfoId}{x.FactionId}{x.FactionRank}{x.Race}{x.Gender}{x.DialogueText}").ToList();
                foreach (var dgroup in dupes)
                {
                    foreach (var d in dgroup.Skip(1))
                    {
                        gd.Remove(d);
                    }
                }

                foreach (var line in gd)
                {
                    var conflicts = gd.Count(x => x.FactionRank == line.FactionRank && x.Gender == line.Gender && x.Race == line.Race && x.InfoId == line.InfoId && x.Source == line.Source && line.SpeakerId == x.SpeakerId);
                    if (conflicts > 1) line.Conflict = true;
                }

                // Save to csv
                var dictionaryCollection = gameDialogue.Select(x => x.ToStringDictionary()).ToList();
                dictionaryCollection.SaveDictToCsv(Path.Combine(csvPath, $"Morrowind.csv"));

                var brokenCollection = _brokenLines.Select(x => x.ToStringDictionary()).ToList();
                brokenCollection.SaveDictToCsv(Path.Combine(csvPath, $"Morrowind.Broken.csv"));
            }

            _brokenLines.Clear();
            foreach (var file in modFiles)
            {
                var modDialogue = new ConcurrentQueue<DialogueLine>();
                foreach (var npc in npcDict[file]) _npcs[npc.id] = npc;
                foreach (var faction in factionDict[file]) _factions[faction.id] = faction;
                foreach (var region in regionDict[file].Where(x => x.references != null)) _regions[region.id] = region;
                foreach (var cell in cellDict[file].Where(x => x.references != null))
                {
                    if (cell.references.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(cell.id)) _cells[$"{cell.region},{cell.data.grid[0]}_{cell.data.grid[1]}"] = cell;
                        else _cells[$"{cell.region},{cell.id}"] = cell;
                    }
                }
                foreach (var line in ExtractDialogue(file))
                    modDialogue.Enqueue(line);
                //modDialogue.AddRange(ExtractDialogue(file));

                // Save to csv
                var modCollection = modDialogue.Where(x => !gameDialogue.Any(g => g.Equals(x))).Select(x => x.ToStringDictionary()).ToList();
                modCollection.SaveDictToCsv(Path.Combine(csvPath, $"{file}.csv"));

                var modbrokenCollection = _brokenLines.Select(x => x.ToStringDictionary()).ToList();
                modbrokenCollection.SaveDictToCsv(Path.Combine(csvPath, $"{file}.Broken.csv"));
            }
        }
    }
}
