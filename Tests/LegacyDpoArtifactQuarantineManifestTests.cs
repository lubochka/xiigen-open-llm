// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace LlmFromScratch.Tests;

// Этот guard не обучает модель. Он доказывает, что старые пары лежат в
// карантине как историческая база и не могут молча вернуться в trainer.
public sealed class LegacyDpoArtifactQuarantineManifestTests
{
    private const string ManifestSchema =
        "xiigen-legacy-dpo-artifact-quarantine-manifest-v1";
    private const string QuarantineStatus =
        "BASELINE_ONLY_COLD_START_FOREST";
    private const string MissingArtifactHumanProblem =
        "Исторический артефакт присутствует в репозитории, но не записан в карантин";

    [Fact]
    public void CurrentManifestCoversEveryPresentLegacyArtifactAndKeepsTrainingClosed()
    {
        var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
        var quarantineRoot = QuarantineRoot(repoRoot);
        var problems = ValidateQuarantine(repoRoot, quarantineRoot);

        Assert.True(
            problems.Count == 0,
            "Проблемы карантина старых DPO-артефактов:\n" +
            string.Join('\n', problems));
    }

    [Fact]
    public void MissingThreeFieldVerdictReportsExactHumanFailure()
    {
        var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
        var quarantineRoot = QuarantineRoot(repoRoot);
        using var temp = new TempQuarantineCopy(quarantineRoot);
        var legacyPath = Path.Combine(
            temp.RootPath,
            "legacy-three-field-datasets.jsonl");
        var lines = File.ReadAllLines(legacyPath, Encoding.UTF8);
        Assert.True(lines.Length > 1, "Mutation fixture needs at least two legacy rows.");
        File.WriteAllLines(legacyPath, lines.Skip(1), Encoding.UTF8);

        // Мутация имитирует опасную ручную правку: один файл остался на диске,
        // но из карантина исчез. Ошибка должна быть понятна человеку без номера.
        var problems = ValidateQuarantine(repoRoot, temp.RootPath);

        Assert.Contains(
            problems,
            problem => problem.Contains(
                           MissingArtifactHumanProblem,
                           StringComparison.Ordinal) &&
                       problem.Contains(
                           "dpo-pairs-normalized.jsonl",
                           StringComparison.Ordinal));
    }

    [Fact]
    public void SeedPairScanKeepsHistoricalArtifactsAndSkipsRuntimeTestResults()
    {
        using var temp = new TempRepositoryRoot();
        var historicalPair = Path.Combine(
            temp.RootPath,
            "43.LearningLocalWikiData",
            "artifacts",
            "runner-metrics",
            "kept",
            "dpo-runs",
            "unit",
            "dpo-pairs.jsonl");
        var runtimePair = Path.Combine(
            temp.RootPath,
            "TestResults",
            "leftover",
            "dpo-runs",
            "unit",
            "dpo-pairs.jsonl");
        var nestedEvidencePair = Path.Combine(
            temp.RootPath,
            "docs",
            "plans",
            "evidence",
            "historical",
            "dpo-runs",
            "unit",
            "dpo-pairs.jsonl");
        var nestedResultsPair = Path.Combine(
            temp.RootPath,
            "docs",
            "plans",
            "Results",
            "historical",
            "dpo-runs",
            "unit",
            "dpo-pairs.jsonl");

        Directory.CreateDirectory(Path.GetDirectoryName(historicalPair)!);
        Directory.CreateDirectory(Path.GetDirectoryName(runtimePair)!);
        Directory.CreateDirectory(Path.GetDirectoryName(nestedEvidencePair)!);
        Directory.CreateDirectory(Path.GetDirectoryName(nestedResultsPair)!);
        File.WriteAllText(historicalPair, "{}\n", Encoding.UTF8);
        File.WriteAllText(runtimePair, "{}\n", Encoding.UTF8);
        File.WriteAllText(nestedEvidencePair, "{}\n", Encoding.UTF8);
        File.WriteAllText(nestedResultsPair, "{}\n", Encoding.UTF8);

        var presentPairs = PresentSeedPairFiles(temp.RootPath);

        // Исторические runner artifacts остаются входом проверки карантина:
        // именно они доказывают, что старый seed-run не вернулся в обучение.
        Assert.Contains(
            "43.LearningLocalWikiData/artifacts/runner-metrics/kept/dpo-runs/unit/dpo-pairs.jsonl",
            presentPairs);
        // Не всякая папка с именем evidence является мусором. Например,
        // docs/plans/evidence может хранить историческую проверочную базу; если
        // там появится dpo-run, guard обязан увидеть его и сверить с manifest.
        Assert.Contains(
            "docs/plans/evidence/historical/dpo-runs/unit/dpo-pairs.jsonl",
            presentPairs);
        // Results — мусор только в корне репозитория. Внутри docs/plans это
        // обычная историческая папка, и guard обязан проверять её как источник.
        Assert.Contains(
            "docs/plans/Results/historical/dpo-runs/unit/dpo-pairs.jsonl",
            presentPairs);
        // Корневой TestResults — рабочая папка доказательств текущих прогонов,
        // а не источник обучающих пар; закрытые служебные подпапки не должны
        // ронять проверку карантина и не должны становиться новым долгом manifest.
        Assert.DoesNotContain(
            "TestResults/leftover/dpo-runs/unit/dpo-pairs.jsonl",
            presentPairs);
    }

    [Fact]
    public void HistoricalArtifactHashIgnoresPlatformLineEndings()
    {
        using var temp = new TempRepositoryRoot();
        var jsonl = Path.Combine(temp.RootPath, "legacy.jsonl");
        File.WriteAllText(
            jsonl,
            "{\"a\":1}\r\n{\"b\":2}\r\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var canonicalHash = Sha256OfUtf8Text("{\"a\":1}\n{\"b\":2}\n");

        // Git хранит эти jsonl как текст. Guard сравнивает смысловой текстовый
        // снимок, иначе один и тот же исторический файл будет краснеть на Windows
        // из-за CRLF и зеленеть на Linux из-за LF. UTF-8 BOM не считается
        // содержанием строки: это маркер кодировки, а не обучающие данные.
        Assert.Equal(canonicalHash, Sha256(jsonl));
    }

    [Fact]
    public void SeedPairScanDoesNotHideUnreadableProjectDirectoryErrors()
    {
        using var temp = new TempRepositoryRoot();
        var unreadableDirectory = Path.Combine(temp.RootPath, "docs", "plans", "hidden");
        Directory.CreateDirectory(Path.GetDirectoryName(unreadableDirectory)!);
        File.WriteAllText(unreadableDirectory, "not a directory", Encoding.UTF8);

        var exception = Assert.Throws<InvalidOperationException>(
            () => EnumerateFilesInOneDirectory(
                temp.RootPath,
                unreadableDirectory,
                "dpo-pairs.jsonl").ToArray());

        // Если обычный путь проекта нельзя прочитать как каталог, это красная
        // проверка, а не повод тихо пропустить возможный старый dpo-run.
        Assert.Contains("обычный каталог", exception.Message, StringComparison.Ordinal);
        Assert.Contains("docs/plans/hidden", exception.Message, StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> ValidateQuarantine(
        string repoRoot,
        string quarantineRoot)
    {
        var problems = new List<string>();
        var manifest = ReadJsonObject(
            Path.Combine(quarantineRoot, "manifest.json"),
            "главный manifest.json",
            problems);
        var legacyRecords = ReadJsonl(
            Path.Combine(quarantineRoot, "legacy-three-field-datasets.jsonl"),
            "legacy-three-field-datasets.jsonl",
            problems);
        var seedRecords = ReadJsonl(
            Path.Combine(quarantineRoot, "seed-dpo-run-directories.jsonl"),
            "seed-dpo-run-directories.jsonl",
            problems);

        if (manifest is not null)
        {
            RequireClosedManifest(manifest, problems);
            RequireManifestCounts(manifest, legacyRecords, seedRecords, problems);
        }

        var legacyByPath = BuildRecordMap(
            legacyRecords,
            record => RequiredString(record, "relative_path", problems));
        var seedByPath = BuildRecordMap(
            seedRecords,
            record => RequiredString(record, "pair_file", problems));

        foreach (var record in legacyRecords)
            ValidateFileRecord(repoRoot, record, "relative_path", problems);
        foreach (var record in seedRecords)
        {
            ValidateFileRecord(repoRoot, record, "pair_file", problems);
            ValidateHeldOutRecord(repoRoot, record, problems);
        }

        // Живой скан репозитория защищает от тихого появления нового legacy-файла
        // вне manifest: такой файл остается на диске, но не получает training=false.
        foreach (var path in PresentThreeFieldDatasets(repoRoot))
        {
            if (!legacyByPath.ContainsKey(path))
                problems.Add($"{MissingArtifactHumanProblem}: {path}");
        }

        IReadOnlyList<string> presentSeedPairFiles;
        try
        {
            presentSeedPairFiles = PresentSeedPairFiles(repoRoot);
        }
        catch (InvalidOperationException exception)
        {
            problems.Add(exception.Message);
            presentSeedPairFiles = [];
        }

        foreach (var path in presentSeedPairFiles)
        {
            if (!seedByPath.ContainsKey(path))
                problems.Add($"{MissingArtifactHumanProblem}: {path}");
        }

        return problems
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static void RequireClosedManifest(
        JsonObject manifest,
        List<string> problems)
    {
        if (StringValue(manifest, "schema") != ManifestSchema)
            problems.Add("Главный манифест карантина имеет неверную схему.");
        if (StringValue(manifest, "status") != QuarantineStatus)
            problems.Add("Главный манифест карантина имеет неверный статус хранения.");
        if (BoolValue(manifest, "training_allowed") != false)
            problems.Add("Главный манифест карантина ошибочно разрешает обучение.");
        if (BoolValue(manifest, "semantic_publication_allowed") != false)
            problems.Add("Главный манифест карантина ошибочно разрешает смысловую публикацию.");
        if (BoolValue(manifest, "files_deleted") != false)
            problems.Add("Главный манифест карантина заявляет удаление файлов.");
        if (BoolValue(manifest, "historical_artifacts_rewritten") != false)
            problems.Add("Главный манифест карантина заявляет переписывание исторических артефактов.");
    }

    private static void RequireManifestCounts(
        JsonObject manifest,
        IReadOnlyList<JsonObject> legacyRecords,
        IReadOnlyList<JsonObject> seedRecords,
        List<string> problems)
    {
        var scopes = manifest["scopes"]?.AsObject();
        var legacyScope = scopes?["legacy_three_field_datasets"]?.AsObject();
        var seedScope = scopes?["seed_dpo_run_directories"]?.AsObject();

        if (IntValue(legacyScope, "catalog_records") != legacyRecords.Count)
            problems.Add("Главный манифест не совпадает с числом старых трехполевых наборов.");
        if (IntValue(seedScope, "catalog_records") != seedRecords.Count)
            problems.Add("Главный манифест не совпадает с числом посевных прогонов.");
    }

    private static void ValidateFileRecord(
        string repoRoot,
        JsonObject record,
        string pathProperty,
        List<string> problems)
    {
        RequireClosedRecord(record, RequiredString(record, pathProperty, problems), problems);
        var relativePath = RequiredString(record, pathProperty, problems);
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            if (BoolValue(record, "git_tracked") == true)
                problems.Add($"Отслеживаемый git артефакт записан в карантин, но отсутствует на диске: {relativePath}.");
            return;
        }

        if (StringValue(record, "sha256") != Sha256(fullPath))
            problems.Add($"Хеш исторического артефакта изменился после записи карантина: {relativePath}.");
        if (IntValue(record, "non_empty_jsonl_rows") != NonEmptyLineCount(fullPath))
            problems.Add($"Число непустых строк исторического артефакта изменилось после записи карантина: {relativePath}.");
    }

    private static void RequireClosedRecord(
        JsonObject record,
        string relativePath,
        List<string> problems)
    {
        if (StringValue(record, "artifact_status") != QuarantineStatus)
            problems.Add($"У записи карантина неверный статус хранения: {relativePath}.");
        if (BoolValue(record, "training_allowed") != false)
            problems.Add($"Запись карантина ошибочно разрешает обучение: {relativePath}.");
        if (BoolValue(record, "semantic_publication_allowed") != false)
            problems.Add($"Запись карантина ошибочно разрешает смысловую публикацию: {relativePath}.");
        if (string.IsNullOrWhiteSpace(StringValue(record, "human_reason")))
            problems.Add($"Запись карантина не объясняет человеку причину хранения: {relativePath}.");

        var rebuildRule = StringValue(record, "rebuild_rule");
        if (!rebuildRule.Contains("DpoTrainerPairAdmission", StringComparison.Ordinal) ||
            !rebuildRule.Contains("authorship", StringComparison.Ordinal) ||
            !rebuildRule.Contains("licens", StringComparison.Ordinal))
        {
            problems.Add(
                $"Запись карантина не называет путь пересборки через допуск пары, авторство и лицензионную проверку: {relativePath}.");
        }
    }

    private static void ValidateHeldOutRecord(
        string repoRoot,
        JsonObject record,
        List<string> problems)
    {
        var relativePath = StringValue(record, "heldout_pair_file");
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return;

        if (StringValue(record, "heldout_sha256") != Sha256(fullPath))
            problems.Add($"Хеш отложенного файла посевного прогона изменился после записи карантина: {relativePath}.");
        if (IntValue(record, "heldout_non_empty_jsonl_rows") != NonEmptyLineCount(fullPath))
            problems.Add($"Число непустых строк отложенного файла посевного прогона изменилось: {relativePath}.");
    }

    private static IReadOnlyDictionary<string, JsonObject> BuildRecordMap(
        IReadOnlyList<JsonObject> records,
        Func<JsonObject, string> pathSelector)
    {
        var map = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        foreach (var record in records)
        {
            var path = pathSelector(record);
            if (!string.IsNullOrWhiteSpace(path))
                map.TryAdd(path.Replace('\\', '/'), record);
        }

        return map;
    }

    private static IReadOnlyList<string> PresentThreeFieldDatasets(string repoRoot)
    {
        var root = Path.Combine(
            repoRoot,
            "experiments-v2",
            "INTERNAL-TRAIN-READY-V191",
            "results");
        if (!Directory.Exists(root))
            return [];

        return Directory.EnumerateFiles(
                root,
                "dpo-pairs-normalized.jsonl",
                SearchOption.AllDirectories)
            .Select(path => Relative(repoRoot, path))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> PresentSeedPairFiles(string repoRoot)
        => EnumerateRepositoryFiles(
                repoRoot,
                "dpo-pairs.jsonl")
            .Where(path => path.Replace('\\', '/').Contains("/dpo-runs/", StringComparison.Ordinal))
            .Select(path => Relative(repoRoot, path))
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IEnumerable<string> EnumerateRepositoryFiles(
        string root,
        string searchPattern)
    {
        var pending = new Stack<string>();
        var rootFullPath = Path.GetFullPath(root);
        pending.Push(rootFullPath);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (IsRuntimeDirectory(rootFullPath, current))
                continue;

            foreach (var file in EnumerateFilesInOneDirectory(rootFullPath, current, searchPattern))
                yield return file;

            foreach (var child in EnumerateChildDirectories(rootFullPath, current))
            {
                if (!IsRuntimeDirectory(rootFullPath, child))
                    pending.Push(child);
            }
        }
    }

    private static bool IsRuntimeDirectory(
        string rootPath,
        string directoryPath)
    {
        var directoryName = Path.GetFileName(directoryPath.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar));
        var normalizedRelativePath = Relative(rootPath, directoryPath);

        // Только корневые runtime-семейства — мусор текущих прогонов. Вложенные
        // Results/TestResults/evidence-папки в документации, artifacts или
        // training-data не прячем: они могут быть настоящей исторической базой,
        // которую manifest обязан покрыть.
        if (IsRootRuntimeFamily(normalizedRelativePath, "evidence") ||
            IsRootRuntimeFamily(normalizedRelativePath, "Results") ||
            IsRootRuntimeFamily(normalizedRelativePath, "TestResults"))
            return true;

        return string.Equals(directoryName, ".git", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, ".tmp", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, ".clone", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, ".codex-test-temp", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, ".vs", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, "bin", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, "node_modules", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, "obj", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(directoryName, "packages", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRootRuntimeFamily(
        string normalizedRelativePath,
        string rootName)
        => string.Equals(normalizedRelativePath, rootName, StringComparison.OrdinalIgnoreCase) ||
           normalizedRelativePath.StartsWith(rootName + "/", StringComparison.OrdinalIgnoreCase);

    private static string[] EnumerateFilesInOneDirectory(
        string rootPath,
        string directoryPath,
        string searchPattern)
    {
        try
        {
            return Directory
                .EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly)
                .ToArray();
        }
        catch (Exception exception) when (IsSafeRepositoryScanMiss(exception))
        {
            throw RepositoryScanFailure(rootPath, directoryPath, exception);
        }
    }

    private static string[] EnumerateChildDirectories(
        string rootPath,
        string directoryPath)
    {
        try
        {
            return Directory
                .EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly)
                .ToArray();
        }
        catch (Exception exception) when (IsSafeRepositoryScanMiss(exception))
        {
            throw RepositoryScanFailure(rootPath, directoryPath, exception);
        }
    }

    private static bool IsSafeRepositoryScanMiss(Exception exception)
        => exception is UnauthorizedAccessException
            or DirectoryNotFoundException
            or IOException;

    private static InvalidOperationException RepositoryScanFailure(
        string rootPath,
        string directoryPath,
        Exception exception)
        => new(
            "Карантин не может проверить обычный каталог проекта: " +
            $"{Relative(rootPath, directoryPath)}. " +
            "Проверка остановлена, чтобы не скрыть незарегистрированный исторический DPO-файл.",
            exception);

    private static JsonObject? ReadJsonObject(
        string path,
        string humanName,
        List<string> problems)
    {
        if (!File.Exists(path))
        {
            problems.Add($"Карантин не может быть проверен: отсутствует файл {humanName}.");
            return null;
        }

        return JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))?.AsObject();
    }

    private static IReadOnlyList<JsonObject> ReadJsonl(
        string path,
        string humanName,
        List<string> problems)
    {
        if (!File.Exists(path))
        {
            problems.Add($"Карантин не может быть проверен: отсутствует файл {humanName}.");
            return [];
        }

        var rows = new List<JsonObject>();
        foreach (var line in File.ReadLines(path, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            rows.Add(JsonNode.Parse(line)!.AsObject());
        }

        return rows;
    }

    private static string RequiredString(
        JsonObject record,
        string propertyName,
        List<string> problems)
    {
        var value = StringValue(record, propertyName);
        if (string.IsNullOrWhiteSpace(value))
            problems.Add($"Запись карантина не содержит обязательное поле {propertyName}.");
        return value.Replace('\\', '/');
    }

    private static string StringValue(JsonObject? node, string propertyName)
        => node?[propertyName]?.GetValue<string>() ?? string.Empty;

    private static bool? BoolValue(JsonObject? node, string propertyName)
        => node?[propertyName]?.GetValue<bool>();

    private static int? IntValue(JsonObject? node, string propertyName)
        => node?[propertyName]?.GetValue<int>();

    private static int NonEmptyLineCount(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Count(line => !string.IsNullOrWhiteSpace(line));

    private static string Sha256(string path)
    {
        var canonicalText = File
            .ReadAllText(path, Encoding.UTF8)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        return Sha256OfUtf8Text(canonicalText);
    }

    private static string Sha256OfUtf8Text(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))
            .ToLowerInvariant();

    private static string Relative(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

    private static string QuarantineRoot(string repoRoot)
        => Path.Combine(
            repoRoot,
            "training-data",
            "legacy-dpo-artifact-quarantine",
            "2026-08-30");

    private sealed class TempQuarantineCopy : IDisposable
    {
        public TempQuarantineCopy(string sourceRoot)
        {
            RootPath = Path.Combine(
                Path.GetTempPath(),
                "legacy-dpo-quarantine-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);

            foreach (var file in Directory.EnumerateFiles(sourceRoot))
            {
                File.Copy(file, Path.Combine(RootPath, Path.GetFileName(file)));
            }
        }

        public string RootPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }

    private sealed class TempRepositoryRoot : IDisposable
    {
        public TempRepositoryRoot()
        {
            RootPath = Path.Combine(
                Path.GetTempPath(),
                "legacy-dpo-repo-scan-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }
}
