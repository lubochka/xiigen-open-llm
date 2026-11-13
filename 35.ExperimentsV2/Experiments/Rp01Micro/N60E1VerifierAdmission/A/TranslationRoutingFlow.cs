// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
// N60-E1 metadata-only extraction of 27.TranslationDomain/TranslationRoutingFlow.cs line 26.
// TranslationStructuredBatchBuilder line 171 stamps this flow id into every projected example row.
// The real flow, its adapters and the orchestrator are outside this experiment. Identical in both arms.

namespace N60E1.A;

public static class TranslationRoutingFlow
{
    public const string FlowId = "translation-routing-flow";
}
