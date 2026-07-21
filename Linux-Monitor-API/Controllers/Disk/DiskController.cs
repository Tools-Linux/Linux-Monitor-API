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

        var df = Run(
            "df",
            "-B1 --output=source,used"
        );

        var block = JsonSerializer.Deserialize<LsblkRoot>(lsblk);
        var used = ParseDf(df);

        var snapshot = new DiskSnapshot();

        foreach (var disk in block?.Blockdevices ?? [])
        {
            if (disk.Type != "disk")
                continue;

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

            foreach (var part in disk.Children ?? [])
            {
                var device = "/dev/" + part.Name;

                if (used.TryGetValue(device, out var bytes))
                {
                    usedBytes += bytes;
                }

                if (!string.IsNullOrWhiteSpace(part.MountPoint))
                    diskInfo.Mount = part.MountPoint;

                if (!string.IsNullOrWhiteSpace(part.FsType))
                    diskInfo.FsType = part.FsType;
            }

            diskInfo.UsedGB = Math.Round(
                usedBytes / 1024d / 1024d / 1024d,
                1
            );

            snapshot.Disks.Add(diskInfo);
        }

        snapshot.TotalGb = snapshot.Disks.Sum(x => x.SizeGB);
        snapshot.UsedGb = snapshot.Disks.Sum(x => x.UsedGB);
        snapshot.FreeGb = snapshot.TotalGb - snapshot.UsedGb;

        snapshot.Usage = snapshot.TotalGb == 0
            ? 0
            : snapshot.UsedGb / snapshot.TotalGb * 100;

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