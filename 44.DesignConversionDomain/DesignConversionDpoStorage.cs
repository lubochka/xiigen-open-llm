// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Json;
using AgentInfrastructure;

public sealed record DesignConversionDpoStorageQuery(
    string? UnitId = null,
    bool? IsQuarantined = null,
    bool? ContainsUserPrivateData = null,
    double? MinimumUsefulnessScore = null,
    double? MaximumUsefulnessScore = null,
    bool TrainableOnly = false);

public interface IDesignConversionDpoStorage
{
    Task<OperationResult<DesignConversionStoredDpoExample>> SaveExampleAsync(
        DesignConversionStoredDpoExample example,
        CancellationToken ct = default);

    Task<OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>> ListExamplesAsync(
        CancellationToken ct = default);

    Task<OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>> QueryExamplesAsync(
        DesignConversionDpoStorageQuery query,
        CancellationToken ct = default);

    Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryTrainReadyRecordsAsync(
        DesignConversionDpoStorageQuery? query = null,
        CancellationToken ct = default);
}

public sealed class InMemoryDesignConversionDpoStorage : IDesignConversionDpoStorage
{
    private readonly List<DesignConversionStoredDpoExample> _examples = [];
    private readonly DesignConversionDpoGenerator _generator;

    public InMemoryDesignConversionDpoStorage(DesignConversionDpoGenerator? generator = null)
        => _generator = generator ?? new DesignConversionDpoGenerator();

    public Task<OperationResult<DesignConversionStoredDpoExample>> SaveExampleAsync(
        DesignConversionStoredDpoExample example,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(Cancelled<DesignConversionStoredDpoExample>());

        if (example is null)
            return Task.FromResult(OperationResult<DesignConversionStoredDpoExample>.Fail(
                "DesignConversion DPO example is required.",
                "DESIGN_CONVERSION_DPO_EXAMPLE_REQUIRED"));

        _examples.RemoveAll(stored => string.Equals(stored.ExampleId, example.ExampleId, StringComparison.Ordinal));
        _examples.Add(example);

        return Task.FromResult(OperationResult<DesignConversionStoredDpoExample>.Ok(example));
    }

    public Task<OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>> ListExamplesAsync(
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(Cancelled<IReadOnlyList<DesignConversionStoredDpoExample>>());

        return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>.Ok(
            _examples.OrderBy(example => example.ExampleId, StringComparer.Ordinal).ToArray()));
    }

    public Task<OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>> QueryExamplesAsync(
        DesignConversionDpoStorageQuery query,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(Cancelled<IReadOnlyList<DesignConversionStoredDpoExample>>());

        return Task.FromResult(QueryExamples(_examples, query));
    }

    public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryTrainReadyRecordsAsync(
        DesignConversionDpoStorageQuery? query = null,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(Cancelled<IReadOnlyList<DesignConversionDpoTrainingRecord>>());

        return Task.FromResult(BuildTrainReadyRecords(_examples, query));
    }

    private OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>> QueryExamples(
        IReadOnlyList<DesignConversionStoredDpoExample> examples,
        DesignConversionDpoStorageQuery? query)
    {
        var build = _generator.BuildTrainReadyRecords(examples);
        if (!build.IsSuccess)
            return build.MapError<IReadOnlyList<DesignConversionStoredDpoExample>>();

        var recordsById = build.Value!.AllRecords.ToDictionary(record => record.RecordId, StringComparer.Ordinal);
        var result = examples
            .Where(example => MatchesExample(example, recordsById, query ?? new DesignConversionDpoStorageQuery()))
            .OrderByDescending(example => example.UsefulnessScore)
            .ThenBy(example => example.ExampleId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>.Ok(result);
    }

    private OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>> BuildTrainReadyRecords(
        IReadOnlyList<DesignConversionStoredDpoExample> examples,
        DesignConversionDpoStorageQuery? query)
    {
        var build = _generator.BuildTrainReadyRecords(examples);
        if (!build.IsSuccess)
            return build.MapError<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        var effectiveQuery = query ?? new DesignConversionDpoStorageQuery();
        var records = build.Value!.AcceptedRecords
            .Where(record => MatchesRecord(record, effectiveQuery, trainableQuery: true))
            .OrderByDescending(record => record.UsefulnessScore)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>.Ok(records);
    }

    private static bool MatchesExample(
        DesignConversionStoredDpoExample example,
        IReadOnlyDictionary<string, DesignConversionDpoTrainingRecord> recordsById,
        DesignConversionDpoStorageQuery query)
    {
        var key = string.IsNullOrWhiteSpace(example.ExampleId) ? string.Empty : example.ExampleId;
        recordsById.TryGetValue(key, out var record);

        if (query.TrainableOnly && (record is null || !IsTrainable(record)))
            return false;

        if (!MatchesUnit(record?.UnitId ?? example.UnitId, query.UnitId))
            return false;

        if (query.IsQuarantined is not null && (record?.Quarantine.IsQuarantined ?? false) != query.IsQuarantined.Value)
            return false;

        if (query.ContainsUserPrivateData is not null
            && (example.License?.ContainsUserPrivateData ?? false) != query.ContainsUserPrivateData.Value)
        {
            return false;
        }

        if (query.MinimumUsefulnessScore is not null && example.UsefulnessScore < query.MinimumUsefulnessScore.Value)
            return false;

        if (query.MaximumUsefulnessScore is not null && example.UsefulnessScore > query.MaximumUsefulnessScore.Value)
            return false;

        return true;
    }

    private static bool MatchesRecord(
        DesignConversionDpoTrainingRecord record,
        DesignConversionDpoStorageQuery query,
        bool trainableQuery)
    {
        if ((query.TrainableOnly || trainableQuery) && !IsTrainable(record))
            return false;

        if (!MatchesUnit(record.UnitId, query.UnitId))
            return false;

        if (query.IsQuarantined is not null && record.Quarantine.IsQuarantined != query.IsQuarantined.Value)
            return false;

        if (query.ContainsUserPrivateData is not null
            && record.License.ContainsUserPrivateData != query.ContainsUserPrivateData.Value)
        {
            return false;
        }

        if (query.MinimumUsefulnessScore is not null && record.UsefulnessScore < query.MinimumUsefulnessScore.Value)
            return false;

        if (query.MaximumUsefulnessScore is not null && record.UsefulnessScore > query.MaximumUsefulnessScore.Value)
            return false;

        return true;
    }

    private static bool MatchesUnit(string unitId, string? requestedUnitId)
    {
        if (string.IsNullOrWhiteSpace(requestedUnitId))
            return true;

        var normalizedStored = DesignConversionDpoUnits.TryNormalize(unitId, out var stored)
            ? stored
            : unitId;
        var normalizedRequested = DesignConversionDpoUnits.TryNormalize(requestedUnitId, out var requested)
            ? requested
            : requestedUnitId;

        return string.Equals(normalizedStored, normalizedRequested, StringComparison.Ordinal);
    }

    private static bool IsTrainable(DesignConversionDpoTrainingRecord record)
        => record is not null
            && !record.Quarantine.IsQuarantined
            && record.License.AllowsTraining
            && !record.License.ContainsUserPrivateData
            && !string.IsNullOrWhiteSpace(record.UnitId)
            && !IsExplicitlyNotTrainingGrade(record)
            && !HasProviderMissingStatus(record);

    private static bool IsExplicitlyNotTrainingGrade(DesignConversionDpoTrainingRecord record)
        => string.Equals(ReadMetadata(record.Provenance.Metadata, "not_training_grade"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(ReadMetadata(record.Provenance.Metadata, "training_grade"), bool.FalseString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(ReadMetadata(record.Provenance.Metadata, "synthetic_negative"), bool.TrueString, StringComparison.OrdinalIgnoreCase)
           || string.Equals(ReadMetadata(record.Provenance.Metadata, "scaffold_negative"), bool.TrueString, StringComparison.OrdinalIgnoreCase);

    private static string ReadMetadata(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null && metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static bool HasProviderMissingStatus(DesignConversionDpoTrainingRecord record)
    {
        var values = new[]
            {
                record.Quarantine.Reason
            }
            .Concat(record.Quarantine.EvidenceRefs ?? [])
            .Concat((record.Provenance.Metadata ?? new Dictionary<string, string>())
                .SelectMany(pair => new[] { pair.Key, pair.Value }));

        return values.Any(value =>
            !string.IsNullOrWhiteSpace(value)
            && (value.Contains(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, StringComparison.OrdinalIgnoreCase)
                || value.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, StringComparison.OrdinalIgnoreCase)
                || (value.Contains("provider", StringComparison.OrdinalIgnoreCase)
                    && (value.Contains("missing", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("required", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("blocked", StringComparison.OrdinalIgnoreCase)))));
    }

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail("DesignConversion DPO storage operation cancelled.", "CANCELLED");
}

public sealed class JsonlDesignConversionDpoStorage : IDesignConversionDpoStorage
{
    private static readonly JsonSerializerOptions JsonlOptions = new(DesignConversionCheckpointJsonOptions.Standard)
    {
        WriteIndented = false
    };

    private readonly string _path;
    private readonly DesignConversionDpoGenerator _generator;
    private InMemoryDesignConversionDpoStorage _inner;

    public JsonlDesignConversionDpoStorage(string path, DesignConversionDpoGenerator? generator = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("A JSONL storage path is required.", nameof(path));

        _path = Path.GetFullPath(path);
        _generator = generator ?? new DesignConversionDpoGenerator();
        _inner = new InMemoryDesignConversionDpoStorage(_generator);
    }

    public async Task<OperationResult<DesignConversionStoredDpoExample>> SaveExampleAsync(
        DesignConversionStoredDpoExample example,
        CancellationToken ct = default)
    {
        var load = await LoadAsync(ct);
        if (!load.IsSuccess)
            return load.MapError<DesignConversionStoredDpoExample>();

        var save = await _inner.SaveExampleAsync(example, ct);
        if (!save.IsSuccess)
            return save;

        var persist = await PersistAsync(ct);
        if (!persist.IsSuccess)
            return persist.MapError<DesignConversionStoredDpoExample>();

        return save;
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>> ListExamplesAsync(
        CancellationToken ct = default)
    {
        var load = await LoadAsync(ct);
        if (!load.IsSuccess)
            return load.MapError<IReadOnlyList<DesignConversionStoredDpoExample>>();

        return await _inner.ListExamplesAsync(ct);
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionStoredDpoExample>>> QueryExamplesAsync(
        DesignConversionDpoStorageQuery query,
        CancellationToken ct = default)
    {
        var load = await LoadAsync(ct);
        if (!load.IsSuccess)
            return load.MapError<IReadOnlyList<DesignConversionStoredDpoExample>>();

        return await _inner.QueryExamplesAsync(query, ct);
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryTrainReadyRecordsAsync(
        DesignConversionDpoStorageQuery? query = null,
        CancellationToken ct = default)
    {
        var load = await LoadAsync(ct);
        if (!load.IsSuccess)
            return load.MapError<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        return await _inner.QueryTrainReadyRecordsAsync(query, ct);
    }

    private async Task<OperationResult<bool>> LoadAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("DesignConversion DPO storage operation cancelled.", "CANCELLED");

        if (!File.Exists(_path))
            return OperationResult<bool>.Ok(true);

        var examples = new List<DesignConversionStoredDpoExample>();
        var lines = await File.ReadAllLinesAsync(_path, ct);
        for (var i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var example = JsonSerializer.Deserialize<DesignConversionStoredDpoExample>(
                lines[i],
                JsonlOptions);
            if (example is null)
                return OperationResult<bool>.Fail(
                    $"DesignConversion DPO storage line {i + 1} could not be read.",
                    "DESIGN_CONVERSION_DPO_STORAGE_LINE_INVALID");

            examples.Add(example);
        }

        var reset = new InMemoryDesignConversionDpoStorage(_generator);
        foreach (var example in examples)
        {
            var save = await reset.SaveExampleAsync(example, ct);
            if (!save.IsSuccess)
                return save.MapError<bool>();
        }

        _inner = reset;
        return OperationResult<bool>.Ok(true);
    }

    private async Task<OperationResult<bool>> PersistAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("DesignConversion DPO storage operation cancelled.", "CANCELLED");

        var list = await _inner.ListExamplesAsync(ct);
        if (!list.IsSuccess)
            return list.MapError<bool>();

        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
        var lines = list.Value!.Select(example => JsonSerializer.Serialize(
            example,
            JsonlOptions));
        await File.WriteAllLinesAsync(_path, lines, ct);

        return OperationResult<bool>.Ok(true);
    }
}
