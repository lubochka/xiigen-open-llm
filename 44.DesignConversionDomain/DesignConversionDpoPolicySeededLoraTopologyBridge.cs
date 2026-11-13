// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace DesignConversionDomain;

using DPO;

public static class DesignConversionDpoPolicySeededLoraTopologyBridge
{
    public static DpoTrainingSeededLoraTopology
        CreateCanonicalSeededLoraTopology()
    {
        // Только DesignConversion знает живую каноническую LoRA-форму своей
        // policy-base модели. Translation получает нейтральный DPO-дескриптор,
        // а не копию чисел rank/alpha/dropout/targets.
        return DpoTrainingSeededLoraTopology.FromLoraConfig(
            DesignConversionDpoPolicyBaseMetadata.CreateCanonicalLoraConfig());
    }
}
