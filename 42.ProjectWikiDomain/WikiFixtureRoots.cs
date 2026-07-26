// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

/// <summary>
/// Central source for the synthetic repository roots used by the ProjectWiki DPO
/// generators. Fixtures point at a sandbox base rather than any real on-disk
/// checkout, so the generated training rows are portable and never leak a
/// machine-specific path. Project identifiers (askapediaweb, genietasks, ...)
/// are preserved verbatim; only the base directory is synthetic.
/// </summary>
internal static class WikiFixtureRoots
{
    /// <summary>Synthetic base directory that stands in for the real repository parent.</summary>
    public const string RepoRootBase = @"C:\sandbox";

    /// <summary>Synthetic repository root for a project, e.g. <c>C:\sandbox\askapediaweb</c>.</summary>
    public static string ProjectRoot(string projectId) => $@"{RepoRootBase}\{projectId}";

    /// <summary>Synthetic wiki-domain root for a project, e.g. <c>C:\sandbox\askapediaweb\.xiigen\wiki-domain</c>.</summary>
    public static string WikiDomainRoot(string projectId) => $@"{RepoRootBase}\{projectId}\.xiigen\wiki-domain";
}
