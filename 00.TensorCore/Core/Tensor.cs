// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Core;

// Multidimensional float array with automatic-differentiation support.
// Analogous to torch.Tensor. Holds the data, shape, gradient, and computation graph.
public sealed class Tensor
{
    public TensorStorage Storage { get; }
    public TensorShape   Shape   { get; }

    // Convenient access to the flat data array
    public float[] Data => Storage.Data;

    // true for trainable parameters — Grad is computed during backward
    public bool RequiresGrad { get; set; }

    // Accumulated gradient (same shape as the tensor); null until the first backward
    public Tensor? Grad { get; set; }

    // Backward-pass function; null for leaf tensors
    internal Action? BackwardFunc { get; set; }

    // Input tensors in the computation graph (needed for topological sorting)
    internal List<Tensor> Children { get; } = new();

    // Zero tensor of the given shape
    public Tensor(TensorShape shape, bool requiresGrad = false)
    {
        Shape        = shape;
        Storage      = new TensorStorage(shape.TotalElements);
        RequiresGrad = requiresGrad;
    }

    // Tensor from an existing array — the data is copied in
    public Tensor(float[] data, TensorShape shape, bool requiresGrad = false)
    {
        if (data.Length != shape.TotalElements)
            throw new ArgumentException(
                $"Data size {data.Length} does not match shape {shape}");
        Shape        = shape;
        Storage      = new TensorStorage(data);
        RequiresGrad = requiresGrad;
    }

    // Access by multidimensional indices: tensor[2, 3]
    public float this[params int[] indices]
    {
        get => Data[Shape.FlatIndex(indices)];
        set => Data[Shape.FlatIndex(indices)] = value;
    }

    // Zero the accumulated gradient before a new training iteration
    public void ZeroGrad()
    {
        if (Grad != null)
            Array.Clear(Grad.Data, 0, Grad.Data.Length);
    }

    // Run gradient backpropagation from this node
    public void Backward()
    {
        // Initial gradient of the output tensor = 1 (dLoss/dLoss = 1)
        Grad = TensorOps.OnesLike(this);

        // Traverse the graph from output to inputs
        foreach (var node in TopologicalSort())
            node.BackwardFunc?.Invoke();
    }

    // DFS topological sort — consumers first, then producers
    private List<Tensor> TopologicalSort()
    {
        var sorted  = new List<Tensor>();
        var visited = new HashSet<Tensor>(ReferenceEqualityComparer.Instance);

        void Visit(Tensor node)
        {
            if (visited.Contains(node)) return;
            visited.Add(node);
            foreach (var child in node.Children)
                Visit(child);
            sorted.Add(node);
        }

        Visit(this);
        sorted.Reverse();
        return sorted;
    }

    public void Backward(Tensor initialGradient)
    {
        if (!initialGradient.Shape.Dimensions.SequenceEqual(Shape.Dimensions))
            throw new ArgumentException("Initial gradient shape must match tensor shape.", nameof(initialGradient));

        Grad = new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape);

        foreach (var node in TopologicalSort())
            node.BackwardFunc?.Invoke();
    }

    public override string ToString() => $"Tensor{Shape}, RequiresGrad={RequiresGrad}";
}
