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
}
