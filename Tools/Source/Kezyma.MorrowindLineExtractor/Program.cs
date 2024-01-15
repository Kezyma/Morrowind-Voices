using Kezyma.Core.Utilities.Collection;
using Kezyma.Core.Utilities.Extensions;
using Kezyma.MorrowindLineExtractor.Services;
using Kezyma.MorrowindSharedCode.Exports;
using Kezyma.MorrowindSharedCode.Helpers;
using Newtonsoft.Json;

namespace Kezyma.MorrowindLineExtractor
{
    public class MapCell
    {
        public string Id { get; set; }
        public string Region { get; set; }
        public int[] Grid { get; set; }
        public int Flags { get; set; }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Contains("extract") || !args.Any())
                {
                    var le = new LineExtractor(!args.Contains("nogame"));
                    le.ExtractDialogue();
                }
                if (args.Contains("generate"))
                {
                    var lg = new LineGenerator(args.Contains("existingonly"), args.Contains("organise"));
                    lg.GenerateLines(args.FirstOrDefault(x => x.StartsWith("csv:"))?.Split(":")[1] ?? "Morrowind.csv");
                }
                if (args.Contains("map"))
                {
                    var jsonPath = Path.GetFullPath("Json");
                    var cellList = new List<MapCell>();
                    foreach (var file in Directory.GetFiles(Path.GetFullPath("Json")))
                    {
                        var fn = Path.GetFileNameWithoutExtension(file);
                        var jsonData = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(file));
                        var typeGroups = jsonData.GroupBy(x => x.type);
                        var cellGroup = typeGroups.FirstOrDefault(x => x.Key == "Cell")?.ToList() ?? new List<Root>();
                        foreach (var cell in cellGroup)
                        {
                            if (!string.IsNullOrWhiteSpace(cell.region))
                            {
                                if (cellList.Any(x => x.Grid[0] == cell.data.grid[0] && x.Grid[1] == cell.data.grid[1]))
                                {
                                    var currentCell = cellList.First(x => x.Grid[0] == cell.data.grid[0] && x.Grid[1] == cell.data.grid[1]);
                                    if (!string.IsNullOrWhiteSpace(cell.id))
                                    {
                                        currentCell.Id = cell.id;
                                        currentCell.Region = cell.region;
                                        currentCell.Flags = cell.data.flags;
                                    }
                                }
                                else
                                {
                                    cellList.Add(new MapCell
                                    {
                                        Id = cell.id,
                                        Region = cell.region,
                                        Grid = cell.data.grid.ToArray(),
                                        Flags = cell.data.flags
                                    });
                                }
                            }
                        }
                    }
                    File.WriteAllText(Path.Combine(Path.GetFullPath("Json"), $"cells.json"), JsonConvert.SerializeObject(cellList));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.TargetSite);
                Console.ReadLine();
            }
        }
    }
}