// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using TensorCore.Layers;

internal interface ISupervisedTrainingStepExecutor
{
    float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<Parameter> adapterParameters);
}

internal sealed class ProductionSupervisedTrainingStepExecutor
    : ISupervisedTrainingStepExecutor
{
    public float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<Parameter> adapterParameters)
        => session.Trainer.TrainSupervisedNextTokenStep(
            session.Model,
            session.Optimizer,
            tokenIds,
            weight);

    // MS01 card E-SFT-RESPONSE-MASK: the narrow masked path. It passes the session's own model,
    // optimizer and trainer plus the exact 0/1 target mask, and nothing else changes. The existing
    // Execute above stays untouched, so arm A keeps the original objective.
    public float ExecuteMasked(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<float> targetMask)
        => session.Trainer.TrainSupervisedNextTokenStepMasked(
            session.Model,
            session.Optimizer,
            tokenIds,
            weight,
            targetMask);
}
