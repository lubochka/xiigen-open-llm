// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

/// <summary>Creates durable DPO pair stores behind the public <see cref="IDpoPairStore"/> contract.</summary>
public static class ContinuousLearningDpoPairStores
{
    /// <summary>
    /// Creates a file-backed store at a checked repository-owned DPO address.
    /// </summary>
    public static IDpoPairStore CreateFileStore(
        RepositoryOwnedDpoDatasetAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address))
        {
            throw new ArgumentException(
                "Repository-owned DPO dataset address is invalid.",
                nameof(address));
        }

        PrepareRepositoryOwnedDpoCustody(address);

        // WHY: callers that already selected a domain/pattern/version should
        // write the pair files under that immutable dataset's pairs directory,
        // while FileDpoPairStore keeps the existing transaction and replay code.
        return new FileDpoPairStore(address.PairStoreRoot);
    }

    /// <summary>
    /// Creates a bounded file-backed store at a checked repository-owned DPO address.
    /// </summary>
    public static IDpoPairStore CreateFileStore(
        RepositoryOwnedDpoDatasetAddress address,
        int maximumRecordCount)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address))
        {
            throw new ArgumentException(
                "Repository-owned DPO dataset address is invalid.",
                nameof(address));
        }

        PrepareRepositoryOwnedDpoCustody(address);

        // WHY: this overload keeps old capacity tests and new repo-owned
        // callers on the same implementation instead of forking serialization.
        return new FileDpoPairStore(address.PairStoreRoot, maximumRecordCount);
    }

    private static void PrepareRepositoryOwnedDpoCustody(
        RepositoryOwnedDpoDatasetAddress address)
    {
        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PrepareDpoContextForStrictReaders(address.RepositoryRoot);
        if (custodyContext.IsSuccess
            && custodyContext.Value is not null
            && custodyContext.Value.CoversDpoRoot(address.RepositoryRoot))
        {
            return;
        }

        // WHY: репозиторная фабрика принимает только уже проверенный адрес, но
        // после Git clone Linux может вернуть безопасную форму пути с широкими
        // mode bits. Сужаем только канонический DPO-корень через существующий
        // no-follow preparer; произвольный string-root по-прежнему остаётся под
        // строгим fail-closed контролем FileDpoPairStore без автоматического chmod.
        var reason = custodyContext.IsSuccess
            ? "prepared context did not cover the DPO root"
            : custodyContext.ErrorCode ?? "unknown custody preparation error";
        throw new InvalidOperationException(
            "Repository-owned DPO pair-store custody could not be established: " +
            reason + ".");
    }

    internal static IDpoPairStore CreateFileStoreWithPreparedRepositoryOwnedDpoCustody(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedTrainingArtifactCloneCustodyPreparer.PreparedContext
            custodyContext)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(custodyContext);
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address))
        {
            throw new ArgumentException(
                "Repository-owned DPO dataset address is invalid.",
                nameof(address));
        }

        if (!custodyContext.CoversDpoRoot(address.RepositoryRoot))
        {
            throw new InvalidOperationException(
                "Prepared repository-owned DPO custody context does not cover " +
                "the requested repository root.");
        }

        // WHY: внутри manifest-operation DPO custody уже была пройдена один
        // раз на публичной границе. Повторный обход clone-tree здесь неверен:
        // он снова наблюдается как root preparation, может повторно трогать
        // chmod и размывает invariant "одна публичная операция — один custody
        // prepare". Opaque PreparedContext доказывает тот же canonical DPO root.
        return new FileDpoPairStore(address.PairStoreRoot);
    }

    /// <summary>
    /// Creates a file-backed store rooted at <paramref name="storageRoot"/>.
    /// Persistence and readback failures are returned through <see cref="AgentInfrastructure.OperationResult{T}"/>.
    /// </summary>
    public static IDpoPairStore CreateFileStore(string? storageRoot)
        => new FileDpoPairStore(storageRoot);

    /// <summary>
    /// Creates a bounded file-backed store rooted at <paramref name="storageRoot"/>.
    /// Persistence and readback failures are returned through <see cref="AgentInfrastructure.OperationResult{T}"/>.
    /// </summary>
    public static IDpoPairStore CreateFileStore(
        string? storageRoot,
        int maximumRecordCount)
        => new FileDpoPairStore(storageRoot, maximumRecordCount);
}
