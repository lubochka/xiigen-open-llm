// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

internal static class ExtensionRuntimeTokenQualityArtifactPaths
{
    internal const string CollectionName = "ExtensionRuntimeTokenQualityArtifacts";

    private const string ArtifactDirectoryName = "extension-runtime-token-quality-learnability";
    private const string StateFileName = "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json";
    private const string ArtifactRelativePrefix = "artifacts/";
    private const string CanonicalWindowsRepoRoot = "C:\\Projects\\llm_mvp_core";
    private const string StaleDpoImplementationWindowsRepoRoot = "C:\\Projects\\llm_mvp_core-DpoImplementation";

    internal static readonly DateTimeOffset DeterministicArtifactSavedAtUtc = new(2026, 8, 26, 0, 0, 0, TimeSpan.Zero);

    internal static readonly string RepoRoot = FindRepoRoot();

    internal static readonly string ArtifactDir = Path.Combine(
        RepoRoot,
        "artifacts",
        ArtifactDirectoryName);

    internal static readonly string StatePath = Path.Combine(
        RepoRoot,
        "docs",
        "plans",
        StateFileName);

    internal static string ResolveArtifactRef(string refValue, string context)
    {
        if (string.IsNullOrWhiteSpace(refValue))
        {
            throw CreateRejectedRefException(refValue, context, "пустая ссылка на artifact-файл");
        }

        var trimmedRef = refValue.Trim();

        if (IsWindowsAbsoluteRef(trimmedRef))
        {
            return ResolveWindowsArtifactRef(trimmedRef, context);
        }

        if (Path.IsPathFullyQualified(trimmedRef))
        {
            return NormalizeUnderRepoRoot(trimmedRef, refValue, context, "абсолютная ссылка выходит за текущий canonical repo");
        }

        var normalizedRelativeRef = NormalizeSeparators(trimmedRef);
        if (normalizedRelativeRef.Equals("artifacts", StringComparison.Ordinal)
            || normalizedRelativeRef.StartsWith(ArtifactRelativePrefix, StringComparison.Ordinal))
        {
            return NormalizeUnderRepoRoot(
                Path.Combine(RepoRoot, ToNativeSeparators(normalizedRelativeRef)),
                refValue,
                context,
                "relative artifact-ссылка после нормализации выходит за текущий canonical repo");
        }

        throw CreateRejectedRefException(
            refValue,
            context,
            "relative artifact-ссылка должна начинаться с artifacts/ и не может быть внешним путём");
    }

    internal static string ResolveSiblingArtifactRef(string resolvedArtifactPath, string siblingFileName, string context)
    {
        if (string.IsNullOrWhiteSpace(siblingFileName) || Path.GetFileName(siblingFileName) != siblingFileName)
        {
            throw CreateRejectedRefException(
                siblingFileName,
                context,
                "имя sibling-файла должно быть простым именем без каталогов");
        }

        var resolvedParentArtifactPath = ResolveArtifactRef(resolvedArtifactPath, $"{context}: resolved parent artifact");
        var parentDirectory = Path.GetDirectoryName(resolvedParentArtifactPath)
            ?? throw CreateRejectedRefException(resolvedArtifactPath, context, "у resolved artifact-ссылки нет родительского каталога");

        return NormalizeUnderRepoRoot(
            Path.Combine(parentDirectory, siblingFileName),
            siblingFileName,
            context,
            "sibling artifact-ссылка после нормализации выходит за текущий canonical repo");
    }

    internal static string ToCanonicalWindowsRepoRef(string repoPath, string context)
    {
        if (IsWindowsAbsoluteRef(repoPath))
        {
            repoPath = ResolveWindowsArtifactRef(repoPath, context);
        }

        var fullRepoPath = NormalizeUnderRepoRoot(
            repoPath,
            repoPath,
            context,
            "repo-файл для записи ссылки выходит за текущий canonical repo");
        var fullRepoRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RepoRoot));
        var repoRelativePath = Path.GetRelativePath(fullRepoRoot, fullRepoPath);
        if (Path.IsPathRooted(repoRelativePath)
            || repoRelativePath.Equals("..", StringComparison.Ordinal)
            || repoRelativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || repoRelativePath.StartsWith("../", StringComparison.Ordinal))
        {
            throw CreateRejectedRefException(
                repoPath,
                context,
                "repo-файл для Windows-ссылки вышел за текущий canonical repo после повторной проверки");
        }

        // Эти ссылки попадают в evidence и в документы плана, которые читаются из Windows и WSL.
        // Поэтому пишем один устойчивый путь текущего canonical repo, а не временный /mnt/c
        // и не удалённый sibling checkout.
        return CanonicalWindowsRepoRoot + "\\" + NormalizeSeparators(repoRelativePath).Replace('/', '\\');
    }

    internal static string[] ToCanonicalWindowsRepoRefs(string context, params string[] repoPaths)
        => repoPaths
            .Select(path => ToCanonicalWindowsRepoRef(path, context))
            .ToArray();

    internal static string ToCanonicalWindowsRepoRefIfOwned(string path, string context)
    {
        if (IsWindowsAbsoluteRef(path))
        {
            var normalized = NormalizeSeparators(path);
            if (StartsWithRoot(normalized, NormalizeSeparators(CanonicalWindowsRepoRoot))
                || StartsWithRoot(normalized, NormalizeSeparators(StaleDpoImplementationWindowsRepoRoot)))
            {
                return ToCanonicalWindowsRepoRef(path, context);
            }

            return path;
        }

        var fullPath = Path.GetFullPath(path);
        var fullRepoRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RepoRoot));
        var relativePath = Path.GetRelativePath(fullRepoRoot, fullPath);
        if (Path.IsPathRooted(relativePath)
            || relativePath.Equals("..", StringComparison.Ordinal)
            || relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || relativePath.StartsWith("../", StringComparison.Ordinal))
        {
            return fullPath;
        }

        // State patches и validation reports сравниваются между Windows и WSL.
        // Поэтому repo-owned ссылки пишутся в одном Windows-виде, а внешние пути
        // остаются как есть: caller всё ещё видит реальное внешнее расположение.
        return ToCanonicalWindowsRepoRef(fullPath, context);
    }

    private static string FindRepoRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var current = new DirectoryInfo(Path.GetFullPath(start));
            while (current is not null)
            {
                if (IsRepoRoot(current.FullName))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate repository root containing Tests/LlmFromScratch.Tests.csproj and artifacts/{ArtifactDirectoryName}.");
    }

    private static bool IsRepoRoot(string path)
        => File.Exists(Path.Combine(path, "Tests", "LlmFromScratch.Tests.csproj"))
           && Directory.Exists(Path.Combine(path, "artifacts", ArtifactDirectoryName));

    private static string ResolveWindowsArtifactRef(string refValue, string context)
    {
        // JSON артефакты могут хранить Windows-написание C:\..., а Linux/WSL иначе
        // принял бы такую строку за относительное имя файла. Поэтому корень разбирается
        // явно, до Path APIs, и переносится только текущий canonical repo.
        var normalizedRef = NormalizeSeparators(refValue);
        var canonicalRoot = NormalizeSeparators(CanonicalWindowsRepoRoot);
        var staleRoot = NormalizeSeparators(StaleDpoImplementationWindowsRepoRoot);

        if (StartsWithRoot(normalizedRef, canonicalRoot))
        {
            var repoRelativeSuffix = normalizedRef[canonicalRoot.Length..].TrimStart('/');
            return NormalizeUnderRepoRoot(
                Path.Combine(RepoRoot, ToNativeSeparators(repoRelativeSuffix)),
                refValue,
                context,
                "Windows artifact-ссылка текущего repo после нормализации выходит за canonical repo");
        }

        if (StartsWithRoot(normalizedRef, staleRoot))
        {
            throw CreateRejectedRefException(
                refValue,
                context,
                $"ссылка указывает на старый sibling repo {StaleDpoImplementationWindowsRepoRoot}; обновите artifact на current canonical repo {CanonicalWindowsRepoRoot}");
        }

        throw CreateRejectedRefException(
            refValue,
            context,
            "Windows artifact-ссылка указывает на внешний repo и не переносится внутрь core");
    }

    private static string NormalizeUnderRepoRoot(string path, string refValue, string context, string rejectionReason)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRepoRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RepoRoot));
        var relativePath = Path.GetRelativePath(fullRepoRoot, fullPath);

        if (relativePath == "."
            || (!Path.IsPathRooted(relativePath)
                && !relativePath.Equals("..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith("../", StringComparison.Ordinal)))
        {
            return fullPath;
        }

        // Fail-closed: stale sibling/external/traversal refs — это красная
        // evidence-граница, а не повод молча подставлять другой checkout.
        throw CreateRejectedRefException(refValue, context, rejectionReason);
    }

    private static bool IsWindowsAbsoluteRef(string refValue)
        => refValue.Length >= 3
           && char.IsLetter(refValue[0])
           && refValue[1] == ':'
           && (refValue[2] == '\\' || refValue[2] == '/');

    private static bool StartsWithRoot(string normalizedPath, string normalizedRoot)
        => normalizedPath.Equals(normalizedRoot, StringComparison.OrdinalIgnoreCase)
           || normalizedPath.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSeparators(string value)
        => value.Replace('\\', '/');

    private static string ToNativeSeparators(string value)
        => value.Replace('/', Path.DirectorySeparatorChar);

    private static InvalidOperationException CreateRejectedRefException(string? refValue, string context, string reason)
        => new(
            $"Rejected artifact ref for {context}: '{refValue ?? "<null>"}'. Reason: {reason}.");
}

[CollectionDefinition(ExtensionRuntimeTokenQualityArtifactPaths.CollectionName, DisableParallelization = true)]
public sealed class ExtensionRuntimeTokenQualityArtifactsCollection
{
}
