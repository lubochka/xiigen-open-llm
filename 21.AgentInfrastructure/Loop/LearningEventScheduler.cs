// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure;

public sealed record LearningEvent(
    string Domain,
    int PendingExampleCount,
    string Reason,
    DateTimeOffset QueuedAt);

public sealed record LearningDrainResult(
    bool DidRun,
    IReadOnlyList<LearningEvent> Events,
    string Reason);

public sealed class LearningEventScheduler
{
    private readonly Func<bool> _isIdle;
    private readonly int _minimumExamples;
    private readonly Queue<LearningEvent> _queue = new();

    public LearningEventScheduler(Func<bool> isIdle, int minimumExamples = 5)
    {
        _isIdle = isIdle;
        _minimumExamples = Math.Max(1, minimumExamples);
    }

    public int QueuedCount => _queue.Count;

    public OperationResult<bool> QueueIfReady(string domain, int pendingExampleCount, string reason)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return OperationResult<bool>.Fail("Domain is required.", "EMPTY_DOMAIN");
        if (pendingExampleCount < _minimumExamples)
            return OperationResult<bool>.Ok(false);

        _queue.Enqueue(new LearningEvent(domain, pendingExampleCount, reason, DateTimeOffset.UtcNow));
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<LearningDrainResult> DrainIfIdle()
    {
        if (_queue.Count == 0)
            return OperationResult<LearningDrainResult>.Ok(new LearningDrainResult(false, [], "No learning events queued."));
        if (!_isIdle())
            return OperationResult<LearningDrainResult>.Ok(new LearningDrainResult(false, [], "Computer is not idle."));

        var events = _queue.ToArray();
        _queue.Clear();
        return OperationResult<LearningDrainResult>.Ok(new LearningDrainResult(true, events, "Learning events released for idle training."));
    }
}
