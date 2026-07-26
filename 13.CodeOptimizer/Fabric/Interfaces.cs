// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace CodeOptimizer;

public interface ICodeReader
{
    OperationResult<string>                  ReadSource(string path);
    OperationResult<IReadOnlyList<string>>   ListSourceFiles(string directoryPath);
}

public interface ICodeWriter
{
    OperationResult<bool> WriteOptimized(string path, string optimizedCode, bool overwrite);
}

public interface IOptimizationReporter
{
    void ReportFinding(OptimizationFinding finding);
    void ReportFileSummary(OptimizationReport report);
}
