// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace TensorCore.Layers;

// Base class for all neural-network layers.
// Analogous to torch.nn.Module. Holds parameters and child layers.
public abstract class Module
{
    private readonly Dictionary<string, Parameter> _parameters = new();
    private readonly Dictionary<string, Module>    _submodules = new();

    // Register a trainable parameter — called from the child class constructor
    protected void RegisterParameter(string name, Parameter parameter)
        => _parameters[name] = parameter;

    // Register a child layer (submodule)
    protected void RegisterModule(string name, Module module)
        => _submodules[name] = module;

    // All parameters of this layer and every nested layer, recursively.
    // Used by the optimizer to update the weights.
    public IEnumerable<Parameter> Parameters()
    {
        foreach (var p in _parameters.Values)
            yield return p;

        foreach (var sub in _submodules.Values)
        foreach (var p in sub.Parameters())
            yield return p;
    }

    // Zero the gradients of all parameters before a new iteration
    public void ZeroGrad()
    {
        foreach (var p in Parameters())
            p.ZeroGrad();
    }
}
