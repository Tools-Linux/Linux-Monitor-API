namespace Linux_Monitor_API.Models.Memory;

public class MemoryInfo
{
    public long Total { get; set; }
    public long Used { get; set; }
    public long Available { get; set; }
    public double Usage { get; set; }
}