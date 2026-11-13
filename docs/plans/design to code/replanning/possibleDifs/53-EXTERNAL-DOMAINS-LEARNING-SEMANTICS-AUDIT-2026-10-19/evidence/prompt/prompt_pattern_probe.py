#!/usr/bin/env python3
"""Small source-bound arithmetic probe, not execution of the C# learner."""
import argparse
import copy
import hashlib
import json
import re
from pathlib import Path

CORE_SHA = '44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76'
UNIT = '43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs'
MACHINE = '43.SmartSkillsDomain/PromptToTaskMachine.cs'
API = '43.SmartSkillsDomain/XiigenExtensionApiService.cs'

def blob_sha(content):
    return hashlib.sha1(b'blob ' + str(len(content)).encode() + b'\0' + content).hexdigest()

def signals(text, aliases):
    # All fixtures are ASCII: no claim of parity for Python/.NET Unicode rules.
    assert text.isascii()
    tokens = sorted(set(''.join(c.lower() for c in t if c.isalnum()) for t in text.split()))
    tokens = [t for t in tokens if 4 <= len(t) <= 40]
    result = set(tokens) | {'prompt:' + t for t in tokens}
    result.update(signal for phrase, signal in aliases if phrase in text.lower())
    return result

def decide(patterns, request_signals):
    ranked = []
    for pattern in patterns:
        pattern_signals = {s.strip().lower() for s in pattern['signals'] if s.strip()}
        matches = sorted(pattern_signals & request_signals)
        if not matches:
            continue
        score = min(1.0, max(0.0, pattern['confidence'] * len(matches) / len(pattern_signals)))
        ranked.append((score, len(matches), pattern['id'], pattern['action']))
    ranked.sort(key=lambda p: (-p[0], -p[1], p[2]))
    top = ranked[0] if ranked else None
    margin = 0.0 if top is None else top[0] - (ranked[1][0] if len(ranked) > 1 else 0.0)
    ready = top is not None and top[0] >= 0.01 and margin >= 0.01
    return {'ready': ready, 'action': top[3] if ready else None, 'margin': margin,
            'ranked': [{'confidence': p[0], 'matched': p[1], 'id': p[2], 'action': p[3]} for p in ranked]}

def idempotent_semantic_append(patterns, candidate):
    # Deliberately narrow intervention: identical signal set AND full action payload.
    # The fixture action represents all emitted semantic fields. Production must
    # preserve accepted provenance separately and must not coalesce conflicts.
    if any(set(p['signals']) == set(candidate['signals']) and p['action'] == candidate['action']
           and p['confidence'] == candidate['confidence'] for p in patterns):
        return copy.deepcopy(patterns)
    return copy.deepcopy(patterns) + [copy.deepcopy(candidate)]

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source-root', type=Path, required=True)
    parser.add_argument('--out', type=Path, required=True)
    args = parser.parse_args()
    source = {p: (args.source_root / p).read_text(encoding='utf-8') for p in (UNIT, MACHINE, API)}
    required = {
        UNIT: ['top.BaselineConfidence - second.BaselineConfidence',
               'if (topTwoMargin < state.MinimumTopTwoMargin)',
               'Math.Clamp(pattern.Confidence * coverage, 0.0, 1.0)',
               'AddAliasIf(aliases, normalized, "gate", "missing_gates");'],
        MACHINE: ['(current.LearnedPatterns ?? []).Concat([learnedPattern]).ToArray()',
                  '_importedLearnedState = continued;',
                  'TryBuildBaselineInternalTaskResponse(',
                  'TaskType: "baseline_internal_universal_task"'],
        API: ['activeLoadState.PromptToTaskMachine.Translate(promptToTaskRequest)']}
    for path, anchors in required.items():
        for anchor in anchors:
            assert anchor in source[path], f'Missing source anchor: {path}: {anchor}'
    aliases = re.findall(r'AddAliasIf\(aliases, normalized, "([^"]+)", "([^"]+)"\);', source[UNIT])
    assert ('gate', 'missing_gates') in aliases and ('missing gate', 'missing_gates') in aliases

    target = {'id': 'p1', 'signals': ['translation', 'verify'], 'confidence': 0.9,
              'action': 'verify_translation_with_same_requirements_constraints_and_hosts'}
    duplicate = dict(target, id='p2')
    conflicting = dict(target, id='p3', action='edit_translation_with_different_forbidden_actions')
    query = {'translation', 'verify'}
    before = decide([target], query)
    appended = decide([target, duplicate], query)
    deduped = decide(idempotent_semantic_append([target], duplicate), query)
    conflict_control = decide(idempotent_semantic_append([target], conflicting), query)
    unrelated_control = decide([target, dict(target, id='p4', signals=['invoice'])], query)
    assert before['ready'] and not appended['ready'] and deduped['ready']
    assert not conflict_control['ready'] and unrelated_control['action'] == before['action']

    patterns = [
        {'id': 'complete', 'signals': ['complete'], 'confidence': 0.9, 'action': 'review_completed_work'},
        {'id': 'missing', 'signals': ['missing_gates'], 'confidence': 0.95, 'action': 'repair_missing_gate'}]
    intervention = [a for a in aliases if a != ('gate', 'missing_gates')]
    rows = []
    for prompt, expected, kind in [
        ('The approval gate is complete.', 'review_completed_work', 'gate_without_defect'),
        ('The gate is complete.', 'review_completed_work', 'gate_without_defect'),
        ('The delegated review is complete.', 'review_completed_work', 'substring_without_gate_word'),
        ('The delegate is complete.', 'review_completed_work', 'substring_without_gate_word'),
        ('Repair the missing gate.', 'repair_missing_gate', 'explicit_missing_control'),
        ('The documentation is complete.', 'review_completed_work', 'unrelated_control')]:
        a_signals, b_signals = signals(prompt, aliases), signals(prompt, intervention)
        a, b = decide(patterns, a_signals), decide(patterns, b_signals)
        rows.append({'prompt': prompt, 'expected': expected, 'kind': kind,
                     'A': a, 'B': b, 'removed_signals': sorted(a_signals - b_signals),
                     'A_correct': a['action'] == expected, 'B_correct': b['action'] == expected})
    assert all(not r['A_correct'] and r['B_correct'] for r in rows[:4])
    assert all(r['A_correct'] and r['B_correct'] for r in rows[4:])
    hashes = []
    for path in source:
        raw = (args.source_root / path).read_bytes()
        hashes.append({'path': path, 'git_blob_sha': blob_sha(raw),
                       'sha256': hashlib.sha256(raw).hexdigest(),
                       'url': f'https://github.com/lubochka/llm_mvp_core/blob/{CORE_SHA}/{path}'})
    result = {
        'method': 'SOURCE_BOUND_PYTHON_CONTRACT_PROBE', 'core_sha': CORE_SHA,
        'native_csharp_executed': False, 'product_training_executed': False,
        'independent_codex_astra_ultracode_judge_executed': False,
        'quality_gain_established': False, 'source_checks_passed': True,
        'ascii_fixtures_only': True, 'sources': hashes, 'alias_mapping_count': len(aliases),
        'duplicate_positive_pattern': {'before': before, 'A_append': appended, 'B_idempotent': deduped,
            'conflicting_output_control': conflict_control, 'unrelated_control': unrelated_control,
            'observed_unit_boundary': 'ready -> not ready after a redundant accepted semantic pattern',
            'source_predicted_machine_no_api_context': 'loaded_state_no_matching_learned_pattern',
            'source_predicted_machine_api_context': 'baseline_internal_semantic_task_produced'},
        'gate_alias_rows': rows,
        'interpretation': 'Mechanical counterexamples with constructed pattern state; not measured RP01 learning quality.'}
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'source_checks_passed': True, 'duplicate_margin': appended['margin'],
                      'alias_A_correct': sum(r['A_correct'] for r in rows),
                      'alias_B_correct': sum(r['B_correct'] for r in rows),
                      'fixture_count': len(rows), 'native_training': False}))

if __name__ == '__main__':
    main()
