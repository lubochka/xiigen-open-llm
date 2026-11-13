// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;

// This exposes internal inspector primitives to the focused test assembly only.
[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]
