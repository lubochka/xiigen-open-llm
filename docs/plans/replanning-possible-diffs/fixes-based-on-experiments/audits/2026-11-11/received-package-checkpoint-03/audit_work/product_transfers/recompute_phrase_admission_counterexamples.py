#!/usr/bin/env python3
"""Independent exhaustive plans for the current P54 objective versus its caller gate.

No native C# is run; no product or test files are edited. Enumerates every
non-overlapping phrase selection, independently of product dynamic programming.
Results establish the conflict between selection and admission on synthetic
in-scope normalized inputs, not linguistic quality or the final fallback answer.
"""
import hashlib
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent / 'repos' / 'core'
SOURCES = [
    '27.TranslationDomain/TranslationPhraseCompositionPlanner.cs',
    '27.TranslationDomain/TranslationTrainedUnits.cs',
]


def plans(tokens, phrases, position=0, chosen=()):
    if position == len(tokens):
        yield chosen
        return
    yield from plans(tokens, phrases, position + 1, chosen)
    for phrase in phrases:
        words = phrase.split()
        if tokens[position:position + len(words)] == words:
            yield from plans(tokens, phrases, position + len(words), chosen + (phrase,))


def score(plan):
    return sum(len(p.split()) for p in plan), len(plan)


def admitted(plan, count):
    covered, spans = score(plan)
    # Current caller: spans > 0 and (coverage >= .85 or spans >= 2 and coverage >= .50).
    return bool(spans and (covered / count >= .85 or spans >= 2 and covered / count >= .50))


def objective(plan):
    covered, spans = score(plan)
    return -covered, spans, plan


def greedy(tokens, phrases):
    entries = sorted(phrases, key=lambda p: -len(p.split()))
    chosen = []
    position = 0
    while position < len(tokens):
        found = next((p for p in entries if tokens[position:position + len(p.split())] == p.split()), None)
        if found:
            chosen.append(found)
            position += len(found.split())
        else:
            position += 1
    return tuple(chosen)


def describe(plan, count):
    covered, spans = score(plan)
    return {'chosen_source_keys': plan, 'covered': covered, 'tokens': count,
            'coverage': covered / count, 'spans': spans, 'admitted': admitted(plan, count)}


cases = [
    ('higher-coverage-displaces-admissible-two-span-plan', 8,
     ['a1 a2', 'a5 a6', 'a2 a3 a4 a5 a6 a7']),
    ('fewest-spans-discards-admissible-plan-at-equal-coverage', 6,
     ['a1 a2', 'a3 a4', 'a2 a3 a4 a5']),
]
rows = []
for name, count, phrases in cases:
    tokens = [f'a{i}' for i in range(1, count + 1)]
    all_plans = list(plans(tokens, phrases))
    selected = min(all_plans, key=objective)
    previous = greedy(tokens, phrases)
    feasible = [p for p in all_plans if admitted(p, count)]
    best_admitted = min(feasible, key=objective)
    assert admitted(previous, count)
    assert not admitted(selected, count)
    assert admitted(best_admitted, count)
    rows.append({'case': name, 'normalized_source_tokens': tokens,
                 'learned_phrase_keys': phrases,
                 'scope_preconditions': {'direction': 'EN_RU', 'accepted_word_mappings': 0,
                                         'source_tokens_lte_8': True},
                 'number_of_exhaustive_plans': len(all_plans),
                 'previous_assembler': describe(previous, count),
                 'current_planner_objective_winner': describe(selected, count),
                 'best_admissible_plan': describe(best_admitted, count)})

result = {
    'core_ref': '009bf226174441fa0fbf0336e74ad2f2b7174d92',
    'execution_kind': 'fresh Python independent exhaustive oracle, not native C#',
    'source_sha256': {name: hashlib.sha256((ROOT / name).read_bytes()).hexdigest() for name in SOURCES},
    'conclusion': 'The maximum-coverage / minimum-spans winner can fail the current caller gate while another available plan and the old assembler pass it.',
    'limits': ['Normalized inputs are synthetic.', 'No tokenizer, trainer, checkpoint loader or product facade executed.',
               'No claim about final fallback answer or linguistic quality.', 'Native in-scope regression required before a product repair.'],
    'cases': rows,
}
(HERE / 'phrase_admission_counterexamples.json').write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n')
print(json.dumps(result, ensure_ascii=False, indent=2))
