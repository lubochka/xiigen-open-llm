"""Reproduce only DPO-style sequential FP32 log-probability reduction.

This is a numeric source proxy, not a TensorCore or learning experiment.
Run: python audit/dpo_fp32_proxy.py > audit/dpo_fp32_proxy_result.json
"""

import json
import struct


def fp32(value):
    return struct.unpack("f", struct.pack("f", value))[0]


def sum_fp32(values):
    total = fp32(0)
    for value in values:
        total = fp32(total + value)
    return total


rows = []
for length in (8, 16, 32, 64, 128, 256):
    reference = [fp32(-6)] * length
    policy = list(reference)
    policy[-1] = fp32(-6 + 1e-5)
    reference_sum_fp32 = sum_fp32(reference)
    policy_sum_fp32 = sum_fp32(policy)
    difference_fp32 = fp32(policy_sum_fp32 - reference_sum_fp32)
    difference_double = sum(policy) - sum(reference)
    rows.append({
        "response_token_count": length,
        "represented_changed_token_delta": policy[-1] - reference[-1],
        "reference_sum_fp32": reference_sum_fp32,
        "policy_sum_fp32": policy_sum_fp32,
        "difference_fp32": difference_fp32,
        "difference_double_same_float_inputs": difference_double,
        "margin_fp32_beta_0_1_rejected_delta_0": fp32(fp32(0.1) * difference_fp32),
        "margin_double_beta_0_1_rejected_delta_0": 0.1 * difference_double,
    })

result = {
    "status": "SOURCE_NUMERIC_PROXY_ONLY",
    "native_tensorcore_executed": False,
    "model_logits_measured": False,
    "optimizer_updates": 0,
    "learning_quality_measured": False,
    "claim": "Sequential FP32 accumulation can erase a representable small log-probability difference before the stable objective.",
    "limits": "Synthetic representable log-prob inputs only. Not an RP01 observation, not a proof of a regression, and not a measured learning gain.",
    "source": "https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1036-L1068",
    "method": "Round accumulator after each addition via Python struct float packing; compare double accumulation of exactly the same float inputs.",
    "rows": rows,
}
print(json.dumps(result, ensure_ascii=False, indent=2))
