// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.Remote;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ModelServer.Contracts;

public sealed class RemoteLogicalUnitProxy : ILogicalUnit<string, string>
{
    private readonly string _unitId;
    private readonly IRemoteUnitLocator _locator;

    public RemoteLogicalUnitProxy(string unitId, IRemoteUnitLocator locator)
    {
        _unitId = unitId;
        _locator = locator;
    }

    public async Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalUnitResult<string>>.Fail("Logical unit request is required.", "REQUEST_REQUIRED");

        var located = await _locator.LocateAsync(_unitId, cancellationToken);
        if (!located.IsSuccess)
            return OperationResult<LogicalUnitResult<string>>.Fail(located.ErrorMessage, located.ErrorCode);

        var manifest = located.Value!;
        var output = new LogicalUnitResult<string>(
            manifest.UnitId,
            manifest.UnitRole,
            manifest.DomainTag,
            request.Input,
            1.0,
            ["REMOTE_UNIT"],
            [manifest.DomainTag],
            [manifest.ManifestSha256],
            []);

        return OperationResult<LogicalUnitResult<string>>.Ok(output);
    }
}
