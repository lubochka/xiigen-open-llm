// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// FIX-HCL: the hierarchy seam is internal on purpose — it is an experiment
// detail, not public API. Only the test assembly needs to see it, so that the
// reader can be checked against the trained table itself instead of through a
// second copy of the algorithm written inside the test.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]
