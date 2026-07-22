using System.Diagnostics;
using System.Text.Json;
using Linux_Monitor_API.Models.Disk;

namespace Linux_Monitor_API.Services.Disk;

public class DiskServices
{
    public Task<object> Get()
    {
        var result = IsLxc()
            ? GetContainerStorage()
            : GetPhysicalDisks();

        return Task.FromResult<object>(result);
    }

    private static bool IsLxc()
    {
        try
        {
            if (File.Exists("/.dockerenv"))
                return true;

            if (File.Exists("/run/.containerenv"))
                return true;

            if (File.Exists("/proc/1/environ"))
            {
                var env = File.ReadAllText("/proc/1/environ");

                if (env.Contains("container=lxc") ||
                    env.Contains("container=lxc-libvirt"))
                    return true;
            }

            if (File.Exists("/proc/1/cgroup"))
            {
                var cgroup = File.ReadAllText("/proc/1/cgroup");

                if (cgroup.Contains("lxc"))
                    return true;
            }
        }
        catch
        {
        }

        return false;
    }

    private DiskSnapshot GetContainerStorage()
    {
        var df = Run(
            "df",
            "-B1 --output=source,size,used,target /"
        );

        var lines = df.Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries
        );

        var snapshot = new DiskSnapshot();

        if (lines.Length < 2)
            return snapshot;

        var cols = lines[1].Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
        );

        if (cols.Length < 4)
            return snapshot;


        var size = long.Parse(cols[1]);
        var used = long.Parse(cols[2]);


        var disk = new DiskInfo
        {
            Device = cols[0],
            Model = "Container Storage",
            Mount = cols[3],
            FsType = "-",

            SizeGB = Math.Round(
                size / 1024d / 1024d / 1024d,
                1
            ),

            UsedGB = Math.Round(
                used / 1024d / 1024d / 1024d,
                1
            ),

            TempC = 0,
            ReadMBps = 0,
            WriteMBps = 0,
            Health = "ok"
        };


        snapshot.Disks.Add(disk);

        Calculate(snapshot);

        return snapshot;
    }


    private DiskSnapshot GetPhysicalDisks()
    {
        var lsblk = Run(
            "lsblk",
            "-J -b -o NAME,SIZE,MODEL,TYPE,MOUNTPOINT,FSTYPE"
        );


        var df = Run(
            "df",
            "-B1 --output=source,used"
        );


        var block = JsonSerializer.Deserialize<LsblkRoot>(lsblk);

        var used = ParseDf(df);


        var snapshot = new DiskSnapshot();


        foreach(var disk in block?.Blockdevices ?? [])
        {
            if(disk.Type != "disk")
                continue;


            var info = new DiskInfo
            {
                Device = "/dev/" + disk.Name,

                Model = string.IsNullOrWhiteSpace(disk.Model)
                    ? "Inconnu"
                    : disk.Model.Trim(),

                Mount = "-",
                FsType = "-",

                SizeGB = Math.Round(
                    disk.Size / 1024d / 1024d / 1024d,
                    1
                ),

                TempC = 0,
                ReadMBps = 0,
                WriteMBps = 0,
                Health = "ok"
            };


            long usedBytes = 0;


            foreach(var part in disk.Children ?? [])
            {
                var device = "/dev/" + part.Name;


                if(used.TryGetValue(device,out var bytes))
                    usedBytes += bytes;


                if(!string.IsNullOrWhiteSpace(part.MountPoint))
                    info.Mount = part.MountPoint;


                if(!string.IsNullOrWhiteSpace(part.FsType))
                    info.FsType = part.FsType;
            }


            info.UsedGB = Math.Round(
                usedBytes / 1024d / 1024d / 1024d,
                1
            );


            snapshot.Disks.Add(info);
        }


        Calculate(snapshot);

        return snapshot;
    }


    private static void Calculate(DiskSnapshot snapshot)
    {
        snapshot.TotalGb =
            snapshot.Disks.Sum(x => x.SizeGB);

        snapshot.UsedGb =
            snapshot.Disks.Sum(x => x.UsedGB);

        snapshot.FreeGb =
            snapshot.TotalGb - snapshot.UsedGb;


        snapshot.Usage =
            snapshot.TotalGb == 0
            ? 0
            : snapshot.UsedGb / snapshot.TotalGb * 100;
    }


    private static string Run(
        string cmd,
        string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,

            RedirectStandardOutput = true,
            RedirectStandardError = true,

            UseShellExecute = false
        };


        using var p = Process.Start(psi)!;


        var output =
            p.StandardOutput.ReadToEnd();


        p.WaitForExit();


        return output;
    }


    private static Dictionary<string,long> ParseDf(string text)
    {
        var result = new Dictionary<string,long>();

        foreach(var line in text.Split('\n').Skip(1))
        {
            var cols = line.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );


            if(cols.Length < 2)
                continue;


            if(long.TryParse(cols[1],out var used))
                result[cols[0]] = used;
        }


        return result;
    }
}