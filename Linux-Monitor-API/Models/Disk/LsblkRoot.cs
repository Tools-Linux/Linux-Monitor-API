using System.Text.Json.Serialization;

namespace Linux_Monitor_API.Models.Disk;

public class LsblkRoot
{
    [JsonPropertyName("blockdevices")]
    public List<LsblkDevice> Blockdevices { get; set; } = [];
}

public class LsblkDevice
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("mountpoint")]
    public string? MountPoint { get; set; }

    [JsonPropertyName("fstype")]
    public string? FsType { get; set; }
}