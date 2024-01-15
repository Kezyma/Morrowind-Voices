public class Reference
{
    public int mast_index { get; set; }
    public int refr_index { get; set; }
    public string id { get; set; }
    public bool temporary { get; set; }
    public List<double> translation { get; set; }
    public List<double> rotation { get; set; }
    public List<double> door_destination_coords { get; set; }
    public string door_destination_cell { get; set; }
    public double? scale { get; set; }
    public string owner { get; set; }
    public double? health_left { get; set; }
    public int? lock_level { get; set; }
    public string key { get; set; }
    public int? charge_left { get; set; }
    public int? stack_size { get; set; }
    public string owner_global { get; set; }
}
