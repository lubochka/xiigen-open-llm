// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace TransformerModel.Caching;

/// <summary>
/// Model-facing state contract for autoregressive key/value caches.
/// Implementations own the cached tensors and must return snapshots, not aliases.
/// </summary>
public interface ITransformerKvCache
{
    int LayerCount { get; }

    int GetSequenceLength(int layer);

    int GetUniformSequenceLength();

    (Tensor keys, Tensor values) Update(int layer, Tensor newK, Tensor newV);

    void Clear();
}
