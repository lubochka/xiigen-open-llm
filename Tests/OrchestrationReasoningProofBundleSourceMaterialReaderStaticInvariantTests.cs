// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningProofBundleSourceMaterialReaderStaticInvariantTests
{
    [Fact]
    public void WindowsProofBundleReaderRootAndLeafSharesExcludeDelete()
    {
        var shareDelete = ReadUIntConstant("WindowsShareDelete");
        var rootShare = ReadUIntConstant("WindowsBundleRootShareMode");
        var leafShare = ReadUIntConstant("WindowsLeafFileShareMode");

        Assert.NotEqual(0u, rootShare);
        Assert.NotEqual(0u, leafShare);
        Assert.Equal(0u, rootShare & shareDelete);
        Assert.Equal(0u, leafShare & shareDelete);
    }

    private static uint ReadUIntConstant(string name)
    {
        var type = typeof(OrchestrationReasoningMachine).Assembly.GetType(
            "SmartSkillsDomain.OrchestrationReasoningProofBundleSourceMaterialReader",
            throwOnError: true)!;
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        return Assert.IsType<uint>(field.GetRawConstantValue());
    }
}
