// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// Result of attributed routing: each relevant dataset gets only its own slice of the prompt.
public sealed record AttributedRoutingResult
{
    public required IReadOnlyDictionary<string, string> DatasetToSubprompt { get; init; }

    public int ActiveDatasets => DatasetToSubprompt.Count;

    public int TotalSubpromptLength => DatasetToSubprompt.Values.Sum(v => v.Length);

    public string? ForDataset(string datasetName)
        => DatasetToSubprompt.TryGetValue(datasetName, out var v) ? v : null;
}
