// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Layers;
using TransformerModel;

namespace LoRA;

// Prepares the model for LoRA fine-tuning:
//   1. Freezes all base parameters.
//   2. Returns LoraLinear adapters for each matching attention layer.
// The base model forward pass is unchanged; the adapters are applied externally.
public static class LoraApplicator
{
    public static IReadOnlyList<LoraLinear> Apply(LlmModel model, LoraConfig config)
    {
        // Freeze all base parameters
        foreach (var p in model.Parameters())
            p.Value.RequiresGrad = false;

        var adapters = new List<LoraLinear>();

        foreach (var block in model.Blocks)
        {
            var attn = block.Attention;

            // Create a LoRA adapter for each Linear attention submodule
            // whose name matches the target modules in config
            foreach (var (name, subModule) in GetNamedLinears(attn))
            {
                // Normalize the field name (_qProj) and the target name (q_proj): strip underscores
                // and compare case-insensitively, so that "_qProj" matches "q_proj"
                string normName = name.Replace("_", "").ToLowerInvariant();
                bool isTarget = config.TargetModules.Any(t =>
                    normName.Contains(t.Replace("_", "").ToLowerInvariant()));
                if (!isTarget) continue;

                adapters.Add(new LoraLinear(subModule, config));
            }
        }

        return adapters;
    }

    // Returns all named Linear submodules via reflection, by convention
    private static IEnumerable<(string name, Linear linear)> GetNamedLinears(Module parent)
    {
        // Use reflection to find fields of type Linear
        var type = parent.GetType();
        foreach (var field in type.GetFields(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public))
        {
            if (field.GetValue(parent) is Linear linear)
                yield return (field.Name, linear);
        }
    }
}
