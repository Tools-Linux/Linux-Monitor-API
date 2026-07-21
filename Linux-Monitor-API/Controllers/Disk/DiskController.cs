using System.Diagnostics;
using System.Text.Json;
using Linux_Monitor_API.Models.Disk;
using Microsoft.AspNetCore.Mvc;

namespace Linux_Monitor_API.Controllers.Disk;

[ApiController]
[Route("api/disk")]
public class DiskController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var lsblk = Run(
            "lsblk",
            "-J -b -o NAME,SIZE,MODEL,TYPE,MOUNTPOINT,FSTYPE"
        );

        var lsblkParts = Run(
            "lsblk",
            "-J -b -o NAME,PKNAME,MOUNTPOINT,FSTYPE"
        );

        var df = Run(
            "df",
            "-B1 --output=source,used"
        );

        Console.WriteLine("========== LSBLK ==========");
        Console.WriteLine(lsblk);

        Console.WriteLine("========== LSBLK PARTITIONS ==========");
        Console.WriteLine(lsblkParts);

        Console.WriteLine("========== DF ==========");
        Console.WriteLine(df);

        var block = JsonSerializer.Deserialize<LsblkRoot>(lsblk);

        var partitionRoot =
            JsonSerializer.Deserialize<LsblkPartitionRoot>(lsblkParts);

        var partitions = partitionRoot?.Blockdevices ?? [];

        var used = ParseDf(df);

        Console.WriteLine("========== PARSED DF ==========");
        foreach (var kv in used)
        {
            Console.WriteLine($"{kv.Key} -> {kv.Value}");
        }

        Console.WriteLine("========== PARTITIONS ==========");
        foreach (var p in partitions)
        {
            Console.WriteLine(
                $"Name={p.Name} Parent={p.Parent} Mount={p.MountPoint} Fs={p.FsType}"
            );
        }

        var snapshot = new DiskSnapshot();

        foreach (var disk in block?.Blockdevices ?? [])
        {
            if (disk.Type != "disk")
                continue;

            Console.WriteLine();
            Console.WriteLine("=========================================");
            Console.WriteLine($"DISQUE : {disk.Name}");

            var diskInfo = new DiskInfo
            {
                Device = "/dev/" + disk.Name,

                Model = string.IsNullOrWhiteSpace(disk.Model)
                    ? "Inconnu"
                    : disk.Model.Trim(),

                SizeGB = Math.Round(
                    disk.Size / 1024d / 1024d / 1024d,
                    1
                ),

                Mount = "-",
                FsType = "-",

                TempC = 0,
                ReadMBps = 0,
                WriteMBps = 0,

                Health = "ok"
            };

            long usedBytes = 0;

            foreach (var part in partitions.Where(x => x.Parent == disk.Name))
            {
                var device = "/dev/" + part.Name;

                Console.WriteLine($"Partition : {device}");

                if (used.TryGetValue(device, out var bytes))
                {
                    Console.WriteLine($" -> TROUVÉ : {bytes} octets");
                    usedBytes += bytes;
                }
                else
                {
                    Console.WriteLine(" -> NON TROUVÉ");
                }

                if (!string.IsNullOrEmpty(part.FsType))
                {
                    diskInfo.FsType = part.FsType;
                }

                if (!string.IsNullOrEmpty(part.MountPoint))
                {
                    diskInfo.Mount = part.MountPoint;
                }
            }

            diskInfo.UsedGB = Math.Round(
                usedBytes / 1024d / 1024d / 1024d,
                1
            );

            Console.WriteLine($"Taille disque : {diskInfo.SizeGB} GB");
            Console.WriteLine($"Utilisé : {diskInfo.UsedGB} GB");
            Console.WriteLine($"Montage : {diskInfo.Mount}");
            Console.WriteLine($"FS : {diskInfo.FsType}");

            snapshot.Disks.Add(diskInfo);
        }

        snapshot.TotalGb = snapshot.Disks.Sum(x => x.SizeGB);
        snapshot.UsedGb = snapshot.Disks.Sum(x => x.UsedGB);
        snapshot.FreeGb = snapshot.TotalGb - snapshot.UsedGb;

        snapshot.Usage = snapshot.TotalGb == 0
            ? 0
            : snapshot.UsedGb / snapshot.TotalGb * 100;

        Console.WriteLine();
        Console.WriteLine("========== SNAPSHOT ==========");
        Console.WriteLine($"Total : {snapshot.TotalGb} GB");
        Console.WriteLine($"Used : {snapshot.UsedGb} GB");
        Console.WriteLine($"Free : {snapshot.FreeGb} GB");
        Console.WriteLine($"Usage : {snapshot.Usage}%");

        return Ok(snapshot);
    }


    static string Run(string cmd, string args)
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

        var output = p.StandardOutput.ReadToEnd();

        p.WaitForExit();

        return output;
    }


    static Dictionary<string, long> ParseDf(string text)
    {
        var result = new Dictionary<string, long>();

        foreach (var line in text.Split('\n').Skip(1))
        {
            var cols = line.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );


            if (cols.Length < 2)
                continue;


            if (long.TryParse(cols[1], out var used))
            {
                result[cols[0]] = used;
            }
        }


        return result;
    }
}