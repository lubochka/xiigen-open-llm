#!/usr/bin/env python3
"""Transparent logic-only proxy for historical E05/E07/E20/E21.

NOT a .NET or Roslyn execution, NOT a DPO/LoRA learning-quality measurement.
Derived logic must be checked against source links in the companion Markdown.
"""
import json
from pathlib import Path


def grammar_table(tokens):
    out = {}
    for prev2, prev1, nxt in zip(tokens, tokens[1:], tokens[2:]):
        out.setdefault(f"{prev2}:{prev1}", set()).add(nxt)
    return out


def e05():
    train = [1, 2, 3, 4, 5]
    validation = [6, 7, 8, 9]
    train_only = grammar_table(train)
    full = grammar_table(train + validation)
    key = "6:7"  # observed only inside held-out suffix
    return {
        "example": "validation-only trigram 6,7 -> 8",
        "train_only_mask_contains_answer": 8 in train_only.get(key, set()),
        "full_corpus_mask_contains_answer": 8 in full.get(key, set()),
        "effect_measured": "LOOKUP_LEAK_DEMO_ONLY",
    }


def e07():
    pieces = ["public", " ", "class", " ", "Demo", " ", "{", "}"]
    positions = []
    charpos = 0
    for i, p in enumerate(pieces):
        positions.append({"token_index": i, "actual_char_offset": charpos,
                          "proxy_map_index_used_by_code": i, "aligned": i == charpos})
        charpos += len(p)
    return {"symbolic_pieces_only_not_real_BPE": pieces,
            "misaligned_after_multichar_pieces": sum(not p["aligned"] for p in positions),
            "total_pieces": len(positions), "positions": positions,
            "measured_actual_BPE_Roslyn_quality": False}


def e20():
    # Raw and normalized marker sets intentionally identical to isolate denominator.
    six_markers = {"task", "ienumerable", "promise", "injectable", "def", "yield"}
    examples = [({"task", "ienumerable"}, {"task", "ienumerable"}),
                ({"promise", "injectable"}, {"promise", "injectable"}),
                ({"def"}, {"def"})]
    raw_count = sum(len(raw & six_markers) for raw, _ in examples)
    retained_count = sum(len(norm & six_markers) for _, norm in examples)
    return {"historical_denominator_formula": "six markers * three examples",
            "historical_proxy_scaling_signal": round(retained_count / (len(six_markers) * len(examples)), 4),
            "true_marker_retention_proxy": round(retained_count / raw_count, 4),
            "archived_E20_signal": 0.2778,
            "warning": "Synthetic marker proxy, real SemanticNormalizer not executed"}


def e21():
    thresholds = [round(0.20 + n * 0.05, 2) for n in range(15)]
    scores = [10 for _ in thresholds]  # ProductionSplitCount ignores threshold argument.
    chosen = thresholds[scores.index(max(scores))]
    return {"threshold_values": thresholds, "proxy_scores_invariant": len(set(scores)) == 1,
            "current_source_would_choose_first_threshold": chosen,
            "archived_reported_threshold": 0.3,
            "source_result_version_parity": "NEEDS_GIT_LINEAGE; CURRENT_SOURCE_NOT_RUN"}


def main():
    result = {"status": "SOURCE_LOGIC_PROXY_ONLY_NOT_NATIVE_NOT_LEARNING",
              "E05_validation_mask_leak": e05(), "E07_position_index_alias": e07(),
              "E20_denominator_bias": e20(), "E21_phantom_threshold": e21()}
    assert not result["E05_validation_mask_leak"]["train_only_mask_contains_answer"]
    assert result["E05_validation_mask_leak"]["full_corpus_mask_contains_answer"]
    assert result["E07_position_index_alias"]["misaligned_after_multichar_pieces"] >= 4
    assert result["E20_denominator_bias"]["historical_proxy_scaling_signal"] == 0.2778
    assert result["E20_denominator_bias"]["true_marker_retention_proxy"] == 1.0
    assert result["E21_phantom_threshold"]["current_source_would_choose_first_threshold"] == 0.2
    print(json.dumps(result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
