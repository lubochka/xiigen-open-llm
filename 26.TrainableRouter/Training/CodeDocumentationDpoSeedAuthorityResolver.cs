// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using LoRA;

/// <summary>
/// Binds the admitted root seed to fixed, distinct CodeDocumentation
/// policy and reference dropout streams for each supported domain.
/// </summary>
public static class CodeDocumentationDpoSeedAuthorityResolver
{
    public const string UnsupportedDomainErrorCode =
        "CODE_DOCUMENTATION_DPO_SEED_AUTHORITY_DOMAIN_UNSUPPORTED";

    public static OperationResult<LoraPairRandomSeedAuthority> Resolve(
        string? domainSlug,
        ulong rootInitializationSeed)
    {
        var domains = domainSlug switch
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug =>
                (Policy: "xiigen.code-doc.csharp.policy.dropout.v1",
                 Reference: "xiigen.code-doc.csharp.reference.dropout.v1"),
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AngularDomainSlug =>
                (Policy: "xiigen.code-doc.angular.policy.dropout.v1",
                 Reference: "xiigen.code-doc.angular.reference.dropout.v1"),
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SqlDomainSlug =>
                (Policy: "xiigen.code-doc.sql.policy.dropout.v1",
                 Reference: "xiigen.code-doc.sql.reference.dropout.v1"),
            _ => default
        };
        if (domains.Policy is null || domains.Reference is null)
        {
            return OperationResult<LoraPairRandomSeedAuthority>.Fail(
                "CodeDocumentation DPO seed authority requires a supported domain.",
                UnsupportedDomainErrorCode);
        }

        return OperationResult<LoraPairRandomSeedAuthority>.Ok(
            new LoraPairRandomSeedAuthority(
                rootInitializationSeed,
                domains.Policy,
                domains.Reference));
    }
}
