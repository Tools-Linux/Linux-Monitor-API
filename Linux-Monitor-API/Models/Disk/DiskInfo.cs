
namespace Linux_Monitor_API.Models.Disk;

public class DiskSnapshot
{
    public double TotalGb { get; set; }
    public double UsedGb { get; set; }
    public double FreeGb { get; set; }
    public double Usage { get; set; }

    public List<DiskInfo> Disks { get; set; } = [];
}

public class DiskInfo
{
    public string Device { get; set; } = "";
    public string Model { get; set; } = "";
    public string Mount { get; set; } = "-";
    public string FsType { get; set; } = "-";

    public double SizeGB { get; set; }
    public double UsedGB { get; set; }

    public double TempC { get; set; }
    public double ReadMBps { get; set; }
    public double WriteMBps { get; set; }

    public string Health { get; set; } = "ok";
}