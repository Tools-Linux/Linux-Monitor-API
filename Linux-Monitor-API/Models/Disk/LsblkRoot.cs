public class LsblkDevice
{
    public string Name { get; set; } = "";
    public long Size { get; set; }

    public string? Model { get; set; }

    public string Type { get; set; } = "";

    public string? MountPoint { get; set; }

    public string? FsType { get; set; }

    public List<LsblkDevice>? Children { get; set; }
}


public class LsblkRoot
{
    public List<LsblkDevice> Blockdevices { get; set; } = [];
}