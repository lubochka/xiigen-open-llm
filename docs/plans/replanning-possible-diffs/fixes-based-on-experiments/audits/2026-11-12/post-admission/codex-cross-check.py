#!/usr/bin/env python3
"""Two pinned source counterexamples, without native execution or training.

python3 codex-cross-check.py --core /path/to/llm_mvp_core --output fresh.json
The result is a source translation and a mathematical check, not product PASS.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import math
from pathlib import Path

SOURCE_COMMIT = 'f089a7d55d359ff103d0cdd9ddfb26eee346a16b'
PINS = {
    'experiments-v2/RP01-MICRO/P5001/attempt-0002/Program.cs':
        '2e044e23aab485f7e653d6cd170573716ed8be45',
    '35.ExperimentsV2/Core/ProbeNeuralNetwork.cs':
        'ccef82f6577df962ac439d33971a78b9cd1e861f',
}


def read_pins(core: Path) -> tuple[dict, list]:
    texts, records = {}, []
    for relative, expected in PINS.items():
        data = (core / relative).read_bytes()
        actual = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        if actual != expected:
            raise ValueError(f'SOURCE_CHANGED: {relative}; expected {expected}; got {actual}')
        texts[relative] = data.decode('utf-8')
        records.append(dict(path=relative, git_blob_sha=actual,
                            sha256=hashlib.sha256(data).hexdigest(), bytes=len(data)))
    return texts, records


def loss_for_target_logit(target_logit: float, clipped: bool) -> float:
    # Two logits [0, target_logit], target index 1. All test points are negative.
    if clipped:
        probability = math.exp(target_logit) / (1.0 + math.exp(target_logit))
        return -math.log(max(probability, 1e-7))
    return math.log1p(math.exp(target_logit)) - target_logit


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--core', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    texts, pins = read_pins(args.core)
    p50 = texts[next(iter(PINS))]
    for fragment in [
        'if (spend.MainLessons != 8) guardProblems.Add(',
        'var learningValidity = trainingHappened && improvedOverBefore && beatsCeiling',
        'var hypothesisStatus = beatsCeiling && improvedOverBefore && sweepIsMonotoneRising',
        'var executionStatus = guardProblems.Count == 0 ? "COMPLETED" : "COMPLETED_WITH_GUARD_PROBLEMS";',
        'return 0;',
    ]:
        if fragment not in p50:
            raise ValueError('SOURCE_ASSUMPTION_NOT_FOUND: ' + fragment)
    loss_source = texts['35.ExperimentsV2/Core/ProbeNeuralNetwork.cs']
    for fragment in ['-MathF.Log(MathF.Max(probs[nextToken], 1e-7f))',
                     'dLogits[nextToken] -= 1f;']:
        if fragment not in loss_source:
            raise ValueError('SOURCE_ASSUMPTION_NOT_FOUND: ' + fragment)

    # Explicit hypothetical fixture, not an alteration of any saved attempt.
    lessons, train_calls, before, after, ceiling = 9, 1, 2, 4, 2
    sweep_first, sweep_last = 2, 4
    guard_problems = [] if lessons == 8 else ['main lesson count differs from assigned eight']
    training_happened = train_calls > 0 and lessons > 0
    improved, beats = after > before, after > ceiling
    learning = ('VALID_FOR_NAMED_UNIT' if training_happened and improved and beats
                else 'VALID_BUT_NO_GAIN_OVER_CEILING' if training_happened
                else 'INVALID_NO_TRAINING')
    hypothesis = ('SUPPORTED_FOR_NAMED_METRIC' if beats and improved and sweep_last > sweep_first
                  else 'OPEN')
    p50_case = dict(kind='PINNED_SOURCE_BRANCH_TRANSLATION',
                    fixture=dict(main_lessons=lessons, assigned_main_lessons=8,
                                 train_calls=train_calls, before_correct=before,
                                 after_correct=after, ceiling_correct=ceiling, exam_total=4,
                                 sweep_first_correct=sweep_first, sweep_last_correct=sweep_last),
                    guard_problems=guard_problems,
                    execution_status='COMPLETED_WITH_GUARD_PROBLEMS',
                    learning_validity=learning, hypothesis_status=hypothesis,
                    final_return_on_this_source_path=0,
                    native_execution=False, saved_attempt_mutated=False)
    assert guard_problems and learning == 'VALID_FOR_NAMED_UNIT'
    assert hypothesis == 'SUPPORTED_FOR_NAMED_METRIC'

    target_logit, step = -20.0, 1e-4
    probability = math.exp(target_logit) / (1.0 + math.exp(target_logit))
    clipped_fd = (loss_for_target_logit(target_logit + step, True)
                  - loss_for_target_logit(target_logit - step, True)) / (2.0 * step)
    stable_fd = (loss_for_target_logit(target_logit + step, False)
                 - loss_for_target_logit(target_logit - step, False)) / (2.0 * step)
    gradient = probability - 1.0
    assert clipped_fd == 0.0 and abs(stable_fd - gradient) < 1e-8
    loss_case = dict(kind='PINNED_FORMULA_DOUBLE_PRECISION_CHECK', logits=[0.0, target_logit],
                     target_index=1, probability=probability,
                     clipped_reported_loss=loss_for_target_logit(target_logit, True),
                     stable_cross_entropy=loss_for_target_logit(target_logit, False),
                     source_backward_target_gradient=gradient,
                     finite_difference_step=step,
                     clipped_loss_finite_difference=clipped_fd,
                     stable_loss_finite_difference=stable_fd,
                     native_float_execution=False,
                     limitation='Checks the formulas using Python doubles, not the C# float network, all parameter blocks or training quality.')
    _, after_pins = read_pins(args.core)
    assert pins == after_pins
    result = dict(schema='codex-post-admission-cross-check/v1', source_commit=SOURCE_COMMIT,
                  sources=pins, P50=p50_case, loss=loss_case,
                  native_tests=0, train_calls=0, production_changes=0,
                  conclusion='SOURCE_COUNTEREXAMPLES_REPRODUCED_NOT_PRODUCT_ACCEPTANCE')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open('x', encoding='utf-8') as handle:
        handle.write(json.dumps(result, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps(dict(sources_verified=len(pins), cases=2, native_tests=0, train_calls=0)))


if __name__ == '__main__':
    main()
