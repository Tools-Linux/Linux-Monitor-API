using System.Text.Json.Serialization;

public class LsblkDevice
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("mountpoint")]
    public string? MountPoint { get; set; }

    [JsonPropertyName("fstype")]
    public string? FsType { get; set; }

    [JsonPropertyName("children")]
    public List<LsblkDevice>? Children { get; set; }
}


public class LsblkRoot
{
    [JsonPropertyName("blockdevices")]
    public List<LsblkDevice> Blockdevices { get; set; } = [];
}

public class LsblkPartition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("pkname")]
    public string? Parent { get; set; }

    [JsonPropertyName("mountpoint")]
    public string? MountPoint { get; set; }

    [JsonPropertyName("fstype")]
    public string? FsType { get; set; }
}


public class LsblkPartitionRoot
{
    [JsonPropertyName("blockdevices")]
    public List<LsblkPartition> Blockdevices { get; set; } = [];
}