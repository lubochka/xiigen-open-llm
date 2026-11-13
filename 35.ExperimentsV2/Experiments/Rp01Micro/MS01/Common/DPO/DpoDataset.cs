// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.ObjectModel;
using System.Text.Json;
using DPO.Models;

namespace DPO;

/// <summary>
/// Loads DPO preference pairs from a JSON-lines file.
/// Legacy line: { "Prompt": "...", "Chosen": "...", "Rejected": "..." }
/// Admitted line also carries: chosenAuthor, rejectedAuthor, admissionIssuerName.
/// </summary>
public class DpoDataset
{
    private static readonly JsonSerializerOptions LegacyPairJsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    private static readonly string[] CoreFieldNames =
    [
        "Prompt",
        "Chosen",
        "Rejected"
    ];

    private static readonly string[] AdmissionFieldNames =
    [
        "chosenAuthor",
        "rejectedAuthor",
        "admissionIssuerName"
    ];

    private static readonly string[] StrictFieldNames =
        CoreFieldNames.Concat(AdmissionFieldNames).ToArray();

    private readonly ReadOnlyCollection<DpoDatasetRow> _rows;

    public int Count => _rows.Count;

    // Rows отдает один read-only wrapper поверх private snapshot: caller не может
    // downcast-ом получить List и поменять порядок/допуск строк.
    public IReadOnlyList<DpoDatasetRow> Rows => _rows;

    private DpoDataset(List<DpoDatasetRow> rows)
        => _rows = new ReadOnlyCollection<DpoDatasetRow>(rows.ToArray());

    public static DpoDataset Load(string path)
    {
        var rows = File.ReadAllLines(path)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(ParseRow)
            .ToList();
        return new DpoDataset(rows);
    }

    public IEnumerable<DpoPair> Shuffle(int? seed = null)
    {
        var rng  = seed.HasValue ? new Random(seed.Value) : new Random();
        var copy = _rows.Select(row => row.Pair).ToList();
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    public DpoPair this[int i] => _rows[i].Pair;

    public DpoDatasetRow GetRow(int i) => _rows[i];

    private static DpoDatasetRow ParseRow(string line)
    {
        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Failed to parse DPO dataset row: {line}");

        ValidateStrictFieldNames(root);

        var admissionFieldCount = AdmissionFieldNames.Count(
            name => root.TryGetProperty(name, out _));
        if (admissionFieldCount is > 0 and < 3)
        {
            throw new InvalidOperationException(
                "DPO dataset row has partial admission metadata.");
        }

        if (admissionFieldCount == 3)
        {
            var prompt = ReadRequiredString(root, "Prompt");
            var chosen = ReadRequiredString(root, "Chosen");
            var rejected = ReadRequiredString(root, "Rejected");
            var chosenAuthor = ReadRequiredAuthor(root, "chosenAuthor");
            var rejectedAuthor = ReadRequiredAuthor(root, "rejectedAuthor");
            var issuerName = ReadRequiredString(root, "admissionIssuerName");

            try
            {
                var admitted = DpoTrainerPairCustoms.CreateAdmitted(
                    prompt,
                    chosen,
                    rejected,
                    chosenAuthor,
                    rejectedAuthor,
                    issuerName);
                return new DpoDatasetRow(admitted.Pair, admitted.Admission);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException(
                    "DPO dataset row has invalid admission metadata.",
                    exception);
            }
        }

        var pair = JsonSerializer.Deserialize<DpoPair>(
                       line,
                       LegacyPairJsonOptions)
                   ?? throw new InvalidOperationException($"Failed to parse: {line}");
        return new DpoDatasetRow(pair, admission: null);
    }

    private static void ValidateStrictFieldNames(JsonElement root)
    {
        var seen = StrictFieldNames.ToDictionary(
            fieldName => fieldName,
            _ => 0,
            StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            var exactName = StrictFieldNames.FirstOrDefault(
                fieldName => string.Equals(
                    fieldName,
                    property.Name,
                    StringComparison.Ordinal));
            if (exactName is not null)
            {
                seen[exactName]++;
                if (seen[exactName] > 1)
                {
                    throw new InvalidOperationException(
                        $"DPO dataset row has duplicate '{exactName}' field.");
                }

                continue;
            }

            if (StrictFieldNames.Any(fieldName => string.Equals(
                    fieldName,
                    property.Name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"DPO dataset row has wrong-cased '{property.Name}' field.");
            }
        }
    }

    private static DpoPairSideAuthorKind ReadRequiredAuthor(
        JsonElement root,
        string propertyName)
    {
        var value = ReadRequiredString(root, propertyName);
        if (!Enum.GetNames<DpoPairSideAuthorKind>().Contains(
                value,
                StringComparer.Ordinal) ||
            !Enum.TryParse<DpoPairSideAuthorKind>(
                value,
                ignoreCase: false,
                out var author))
        {
            throw new InvalidOperationException(
                $"DPO dataset row has unknown '{propertyName}' author.");
        }

        return author;
    }

    private static string ReadRequiredString(
        JsonElement root,
        string propertyName)
    {
        if (!TryReadString(root, propertyName, out var value))
        {
            throw new InvalidOperationException(
                $"DPO dataset row is missing required '{propertyName}' string.");
        }

        return value!;
    }

    private static bool TryReadString(
        JsonElement root,
        string propertyName,
        out string? value)
    {
        value = null;
        if (!root.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return value is not null;
    }
}
