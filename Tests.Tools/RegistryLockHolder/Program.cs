// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
    return 3;

if (args.Length is 2 or 3 &&
    StringComparer.Ordinal.Equals(args[0], "--probe-exclusive-owner") &&
    Path.IsPathFullyQualified(args[1]))
{
    var ownerLockLeaf = args.Length == 2 ||
                        StringComparer.Ordinal.Equals(args[2], "adapter")
        ? ".continuous-learning.adapter-owner.lock"
        : StringComparer.Ordinal.Equals(args[2], "registry")
            ? ".continuous-learning.registry-owner.lock"
            : null;
    if (ownerLockLeaf is null)
        return 2;
    var ownerLockPath = Path.Combine(
        Path.GetFullPath(args[1]),
        ownerLockLeaf);
    try
    {
        using var probe = new FileStream(
            ownerLockPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        return 0;
    }
    catch (IOException)
    {
        // FileShare.None in a different process must reject this probe.
        return 10;
    }
}

if (args.Length != 1 || !Path.IsPathFullyQualified(args[0]))
    return 2;

var lockPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);
using var stream = new FileStream(
    lockPath,
    FileMode.OpenOrCreate,
    FileAccess.ReadWrite,
    FileShare.ReadWrite,
    bufferSize: 1,
    FileOptions.WriteThrough);
if (stream.Length == 0)
{
    stream.SetLength(1);
    stream.Flush(flushToDisk: true);
}
if (stream.Length != 1)
    return 4;

stream.Lock(0, 1);
try
{
    Console.Out.WriteLine("READY");
    Console.Out.Flush();
    var command = Console.In.ReadLine();
    return StringComparer.Ordinal.Equals(command, "RELEASE") ? 0 : 5;
}
finally
{
    stream.Unlock(0, 1);
}
