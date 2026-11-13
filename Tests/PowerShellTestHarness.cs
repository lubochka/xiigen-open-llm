// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;

internal static class PowerShellTestHarness
{
    private const string WindowsInteropDistroEnvironmentName = "XIIGEN_WSL_DISTRO_NAME";

    internal static ProcessStartInfo CreateScriptStartInfo(
        string scriptPath,
        string workingDirectory,
        bool includeNoLogo = false,
        bool includeNonInteractive = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        var executable = ResolveExecutable();
        var windowsInteropDistroName = executable.UsesWindowsInterop
            ? ResolveValidatedWslDistroName()
            : null;
        var scriptPathForPowerShell = executable.UsesWindowsInterop
            ? ConvertLinuxPathForWindowsInterop(scriptPath, windowsInteropDistroName!, "script")
            : scriptPath;
        var workingDirectoryForPowerShell = executable.UsesWindowsInterop
            ? ConvertLinuxPathForWindowsInterop(workingDirectory, windowsInteropDistroName!, "working directory")
            : null;

        var startInfo = new ProcessStartInfo
        {
            FileName = executable.FileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };
        if (windowsInteropDistroName is not null)
        {
            // Windows `pwsh.exe`, запущенный из WSL, должен передать дочернему
            // launcher имя исходного дистрибутива: иначе `/tmp/...` невозможно
            // честно превратить в UNC-путь без догадки о машине пользователя.
            startInfo.Environment[WindowsInteropDistroEnvironmentName] = windowsInteropDistroName;
        }

        AddStandardScriptArguments(
            startInfo,
            scriptPathForPowerShell,
            includeNoLogo,
            includeNonInteractive,
            includeWindowsExecutionPolicy: OperatingSystem.IsWindows() || executable.UsesWindowsInterop,
            workingDirectoryForPowerShell);
        return startInfo;
    }

    private static void AddStandardScriptArguments(
        ProcessStartInfo startInfo,
        string scriptPath,
        bool includeNoLogo,
        bool includeNonInteractive,
        bool includeWindowsExecutionPolicy,
        string? workingDirectoryForPowerShell)
    {
        if (includeNoLogo)
            startInfo.ArgumentList.Add("-NoLogo");

        // Все тестовые .ps1 запускаются без профиля, чтобы локальные настройки
        // пользователя не меняли stdout, stderr или exit code проверяемого среза.
        startInfo.ArgumentList.Add("-NoProfile");

        if (includeNonInteractive)
            startInfo.ArgumentList.Add("-NonInteractive");

        if (includeWindowsExecutionPolicy)
        {
            // ExecutionPolicy нужен Windows-движку PowerShell, включая случай,
            // когда WSL запускает Windows `pwsh.exe` как PowerShell Core.
            // Native Linux `pwsh` его не получает, чтобы не расширять контракт
            // Linux-запуска за пределы нужного тестового harness.
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
        }

        if (!string.IsNullOrWhiteSpace(workingDirectoryForPowerShell))
        {
            // Windows `pwsh.exe`, запущенный из WSL, понимает рабочую папку как
            // Windows/UNC путь. Оставлять здесь `/tmp/...` нельзя: shell стартует,
            // но сам скрипт видит другую файловую систему.
            startInfo.ArgumentList.Add("-WorkingDirectory");
            startInfo.ArgumentList.Add(workingDirectoryForPowerShell);
        }

        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
    }

    private static PowerShellExecutable ResolveExecutable()
    {
        var checkedCandidates = new List<string>();

        if (!OperatingSystem.IsWindows())
        {
            // Linux ищет PowerShell Core как `pwsh`, а WSL может получить тот же
            // Core из Windows PATH как `pwsh.exe`. Оба имени означают один и тот
            // же новый PowerShell, поэтому тестовый harness принимает оба варианта.
            foreach (var fileName in new[] { "pwsh", "pwsh.exe" })
            {
                var resolved = FindOnPath(fileName, checkedCandidates);
                if (resolved is not null)
                {
                    return new PowerShellExecutable(
                        resolved,
                        UsesWindowsInteropFromWsl(fileName, resolved));
                }
            }

            // Старый `powershell.exe` здесь не fallback: это Windows PowerShell 5,
            // и он может спрятать Linux/WSL-дефект вместо честного красного теста.
            throw CreateMissingPowerShellException(checkedCandidates);
        }

        // На Windows сначала берём встроенный Windows PowerShell из SystemRoot:
        // он есть на стандартной системе и не зависит от пользовательского PATH.
        // PATH-поиск ниже нужен только как явный fallback для нестандартных сред
        // или машин, где доступен только PowerShell Core.
        var systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
        if (!string.IsNullOrWhiteSpace(systemRoot))
        {
            var windowsPowerShell = Path.Combine(
                systemRoot,
                "System32",
                "WindowsPowerShell",
                "v1.0",
                "powershell.exe");
            checkedCandidates.Add(windowsPowerShell);
            if (File.Exists(windowsPowerShell))
                return new PowerShellExecutable(Path.GetFullPath(windowsPowerShell), usesWindowsInterop: false);
        }
        else
        {
            checkedCandidates.Add("%SystemRoot%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe");
        }

        foreach (var fileName in new[] { "powershell.exe", "powershell", "pwsh.exe", "pwsh" })
        {
            var resolved = FindOnPath(fileName, checkedCandidates);
            if (resolved is not null)
                return new PowerShellExecutable(resolved, usesWindowsInterop: false);
        }

        throw CreateMissingPowerShellException(checkedCandidates);
    }

    private static bool UsesWindowsInteropFromWsl(string fileName, string resolvedPath)
    {
        if (!string.Equals(fileName, "pwsh.exe", StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedPath = Path.GetFullPath(resolvedPath).Replace('\\', '/');
        return normalizedPath.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase);
    }

    private static string ConvertLinuxPathForWindowsInterop(
        string path,
        string wslDistroName,
        string description)
    {
        var fullPath = Path.GetFullPath(path);
        if (TryConvertWslMountPathForWindows(fullPath, out var mountedPath))
            return mountedPath;

        if (!fullPath.StartsWith("/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"PowerShell {description} path is not a Linux absolute path and cannot be passed to Windows pwsh.exe: {fullPath}");
        }

        return @"\\wsl.localhost\" + wslDistroName + fullPath.Replace('/', '\\');
    }

    private static bool TryConvertWslMountPathForWindows(string fullPath, out string windowsPath)
    {
        windowsPath = string.Empty;
        const string MountPrefix = "/mnt/";
        if (!fullPath.StartsWith(MountPrefix, StringComparison.OrdinalIgnoreCase)
            || fullPath.Length < MountPrefix.Length + 1
            || !char.IsAsciiLetter(fullPath[MountPrefix.Length]))
        {
            return false;
        }

        var afterDriveIndex = MountPrefix.Length + 1;
        if (fullPath.Length != afterDriveIndex && fullPath[afterDriveIndex] != '/')
            return false;

        var driveLetter = char.ToUpperInvariant(fullPath[MountPrefix.Length]);
        var suffix = fullPath.Length == afterDriveIndex
            ? string.Empty
            : fullPath[afterDriveIndex..].Replace('/', '\\');
        windowsPath = driveLetter + ":" + (suffix.Length == 0 ? "\\" : suffix);
        return true;
    }

    private static string ResolveValidatedWslDistroName()
    {
        var distroName = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");
        if (string.IsNullOrWhiteSpace(distroName))
        {
            throw new InvalidOperationException(
                "Windows pwsh.exe was selected from WSL, but WSL_DISTRO_NAME is missing; " +
                "the test harness refuses to guess the UNC share for Linux paths.");
        }

        foreach (var value in distroName)
        {
            if (char.IsControl(value) || value is '\\' or '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|')
            {
                throw new InvalidOperationException(
                    $"WSL_DISTRO_NAME contains a character that is unsafe for a Windows UNC share: {distroName}");
            }
        }

        if (distroName is "." or "..")
        {
            throw new InvalidOperationException(
                $"WSL_DISTRO_NAME is not a valid Windows UNC share segment: {distroName}");
        }

        return distroName;
    }

    private static string? FindOnPath(string fileName, List<string> checkedCandidates)
    {
        checkedCandidates.Add($"PATH:{fileName}");

        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
            return null;

        foreach (var rawEntry in pathValue.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(rawEntry))
                continue;

            var candidate = Path.Combine(rawEntry, fileName);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        return null;
    }

    private static InvalidOperationException CreateMissingPowerShellException(
        IReadOnlyCollection<string> checkedCandidates)
    {
        return new InvalidOperationException(
            "PowerShell executable was not found for test harness. Checked candidates: " +
            string.Join(", ", checkedCandidates));
    }

    private sealed class PowerShellExecutable(string fileName, bool usesWindowsInterop)
    {
        internal string FileName { get; } = fileName;

        internal bool UsesWindowsInterop { get; } = usesWindowsInterop;
    }
}
