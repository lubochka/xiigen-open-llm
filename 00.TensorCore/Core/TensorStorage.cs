// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// Flat storage for tensor data.
// Separated from the shape so that future view operations can share a single storage.
public sealed class TensorStorage
{
    // All tensor numbers in row-major order
    public float[] Data { get; }

    public TensorStorage(int totalElements)
    {
        Data = new float[totalElements];
    }

    // Constructor with initial data — copied so there are no external aliases
    public TensorStorage(float[] data)
    {
        Data = data.ToArray();
    }

    public int Length => Data.Length;
}
