#!/usr/bin/env python3
"""Restricted source-equivalent mechanism probe; NOT native C# or measured learning.
Run: python3 verifier_marker_probe.py --core-root /path/to/llm_mvp_core
Reads immutable fetched sources, verifies Git blobs, derives explicit suffix rules.
Only simple valid-Cyrillic fixtures below are supported; no full tokenizer claim.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--core-root', type=Path, required=True)
parser.add_argument('--output', type=Path, default=ROOT / 'verifier_marker_probe_result.json')
args = parser.parse_args()
SRC = args.core_root / '27.TranslationDomain'
SHAS = {
    'TranslationSemanticTokenizer.cs': 'ee6d81c7933fa33a9c57a2111990019ec55e5dab',
    'TranslationTrainedUnits.cs': '1742abfc9fb617dfc66e4d618e0c169da01a6ccc',
    'TranslationStructuredBatchBuilder.cs': '7ceed82476edae9173acc44e2298885c561ca4d5',
    'TranslationUnitContracts.cs': '0f88d0d9cc3a62d2b2f8b0b9321142a4a4c1b0d8',
    'TranslationMachine.cs': 'fd0e736de3739236b4dde2d6b9d000f5ebae8416',
    'TranslationBatch029Runner.cs': 'bd96e036b00f00a031ffb1ee9766994950523569',
}

def git_sha(b):
    return hashlib.sha1(b'blob ' + str(len(b)).encode() + b'\0' + b).hexdigest()

sources = {}
for name, expected in SHAS.items():
    raw = (SRC / name).read_bytes()
    assert git_sha(raw) == expected, (name, git_sha(raw), expected)
    sources[name] = raw.decode('utf-8')
tok = sources['TranslationSemanticTokenizer.cs']
units = sources['TranslationTrainedUnits.cs']
builder = sources['TranslationStructuredBatchBuilder.cs']

def method(source, name):
    # One explicitly named private method; every used method has a later declaration.
    match = re.search(r'    private static [^\n]+\b' + re.escape(name) + r'\(', source)
    assert match, name
    following = source.find('\n    private static ', match.end())
    return source[match.start(): following if following >= 0 else len(source)]

def literals(s):
    return re.findall(r'"([^"\r\n]*)"', s)

def ends(s):
    return re.findall(r'EndsWith\("([^"\r\n]*)"', s)

def hashset(name):
    return set(literals(tok.split('HashSet<string> ' + name, 1)[1].split('};', 1)[0]))

pron = hashset('RuPronouns')
prep = hashset('RuPrepositions')
allow = hashset('RuAdverbAllowList')
adverb_suffixes = ends(method(tok, 'LooksLikeRussianAdverb')) + ends(method(tok, 'LooksLikeRussianAdverbUtf8'))
verb_suffixes = ends(method(tok, 'LooksLikeRussianVerb')) + ends(method(tok, 'LooksLikeRussianVerbUtf8'))
gen_suffixes = ends(method(tok, 'LooksLikeRussianGenitive')) + ends(method(tok, 'LooksLikeRussianGenitiveUtf8'))
old_suffixes = ends(method(units, 'LooksLikeMannerMarker'))
builder_suffixes = ends(method(builder, 'LooksLikeMannerMarker'))
assert 'о' in builder_suffixes and 'о' not in old_suffixes
fixed_suffixes = old_suffixes + ['о']  # sole proposed change; all other predicates fixed
concept_names = re.findall(r'^    ([A-Z_]+),?$', sources['TranslationUnitContracts.cs'].split('enum SemanticConcept', 1)[1].split('}', 1)[0], re.M)
assert 'UNKNOWN' in concept_names
assert 'translated quality marker' in method(builder, 'ExtractRequiredTerm') or 'translated quality marker' in builder
assert 'if (!qualityMarkerIssue && SignatureHasSpecificConcept' in units
assert 'AddTag("RU:TENSE:PAST")' in tok
assert 'RU:SUBJECT:' in tok

def pos(word):
    # Strict valid-Cyrillic branch only. Fixture words avoid ambiguous short verb guards.
    w = word.lower()
    assert re.fullmatch('[а-яё]+', w), w
    if w in prep: return 'PREP'
    if w in pron: return 'PRON'
    if w in allow or (len(w) >= 4 and any(w.endswith(s) for s in adverb_suffixes)): return 'ADV'
    if any(w.endswith(s) and (s not in ('л', 'ла') or len(w) > 3) for s in verb_suffixes): return 'VERB'
    if any(w.endswith(s) for s in gen_suffixes): return 'NOUN_GEN'
    return 'NOUN'

def marker(source, suffixes):
    words = source.lower().split()
    tokens = [(w, pos(w)) for w in words]
    # All fixtures have recognised subject and past verb, so actual RuleTags.Count > 0.
    assert words[0] in ('он', 'она') and any(p == 'VERB' for _, p in tokens)
    for w, p in tokens:
        if p == 'ADV': return w
    base_keys = re.findall(r'\{ "([^"]+)"', tok.split('BaseConceptsRu', 1)[1].split('};', 1)[0])
    contextual_keys = re.findall(r'\("([^"]+)"', tok.split('E26Mappings', 1)[1].split('};', 1)[0])
    for w, p in tokens:
        if p in ('PREP', 'PRON', 'CODE_IDENTIFIER', 'VERB'): continue
        # For these uninflected noun/adverb forms lemma=word and no known concept.
        assert not any(w.startswith(k) for k in base_keys + contextual_keys), w
        if len(w) > 2 and any(w.endswith(s.lower() if s == 'ly' else s) for s in suffixes):
            return w
    return None

TRAIN = [
    {'family': 'door_open', 'source': 'Он тихо открыл дверь', 'chosen': 'He quietly opened the door', 'rejected': 'He opened the door', 'required': 'quietly'},
    {'family': 'bag_pack', 'source': 'Она быстро собрала сумку', 'chosen': 'She quickly packed the bag', 'rejected': 'She packed the bag', 'required': 'quickly'},
]
EXAM = [
    {'family': 'letter_read', 'source': 'Он тихо прочитал письмо', 'candidate': 'He quietly read the letter', 'gold_pass': True},
    {'family': 'letter_read', 'source': 'Он тихо прочитал письмо', 'candidate': 'He read the letter', 'gold_pass': False},
    {'family': 'note_write', 'source': 'Она быстро написала записку', 'candidate': 'She quickly wrote the note', 'gold_pass': True},
    {'family': 'note_write', 'source': 'Она быстро написала записку', 'candidate': 'She wrote the note', 'gold_pass': False},
]
assert not ({x['family'] for x in TRAIN} & {x['family'] for x in EXAM})
assert pos('тихо') == pos('быстро') == 'NOUN'
assert all(marker(x['source'], old_suffixes) is None for x in TRAIN + EXAM)

trace = []
for row in TRAIN:
    issue = f"expected '{row['required']}' for translated quality marker"
    rest = issue.split("'")[2]
    assert not any(n.lower() in rest.lower() for n in concept_names)
    assert re.search(r'\b[A-Za-z][A-Za-z\x27\-]*ly\b', row['chosen']).group() == row['required']
    assert row['required'] not in row['rejected']
    row['verifier_chosen'] = {'Pass': False, 'Issues': [issue], 'CorrectionHint': row['chosen']}
    row['verifier_rejected'] = {'Pass': True, 'Issues': []}
    trace.append({'source': row['source'], 'pos': [(w, pos(w)) for w in row['source'].split()], 'rule_tags_nonempty': True, 'builder_marker': marker(row['source'], builder_suffixes), 'old_marker': marker(row['source'], old_suffixes), 'fixed_marker': marker(row['source'], fixed_suffixes), 'concept_issue_parser': False, 'signature_fallback_allowed': False})

factorial = []
for train_fixed in (False, True):
    rules = {}
    for row in TRAIN:
        m = marker(row['source'], fixed_suffixes if train_fixed else old_suffixes)
        if m: rules.setdefault(m, []).append(row['required'])
    for eval_fixed in (False, True):
        predictions = []
        for row in EXAM:
            m = marker(row['source'], fixed_suffixes if eval_fixed else old_suffixes)
            reqs = rules.get(m, [])
            verdict = not reqs or any(r.lower() in row['candidate'].lower() for r in reqs)
            predictions.append({**row, 'selected_marker': m, 'pass': verdict})
        factorial.append({'train_suffix_fix': train_fixed, 'eval_suffix_fix': eval_fixed, 'learned_marker_count': len(rules), 'rules': rules, 'accuracy': sum(x['pass'] == x['gold_pass'] for x in predictions) / len(predictions), 'false_accepts': sum(x['pass'] and not x['gold_pass'] for x in predictions), 'false_rejects': sum(not x['pass'] and x['gold_pass'] for x in predictions), 'judgments': predictions})
assert [x['accuracy'] for x in factorial] == [0.5, 0.5, 0.5, 1.0]
assert marker('Он долго читал книгу', old_suffixes) == 'долго'
assert pos('окно') == 'ADV'  # A real fixture trap: this word masks the intended contrast.
assert marker('Она быстро закрыла окно', old_suffixes) == marker('Она быстро закрыла окно', fixed_suffixes) == 'окно'

result = {'status': 'SOURCE_EQUIVALENT_MECHANISM_ONLY', 'native_csharp_executed': False, 'quality_gain_native': 'NOT_RUN', 'independent_codex_astra_6_ultracode_exam': 'NOT_RUN', 'external_open_chinese_model_generation': 'NOT_RUN', 'commit': 'eb05219c27663835ce927c7bf97cf1274c07cddb', 'git_blobs_verified': SHAS, 'scope': 'Restricted valid-Cyrillic fixtures; no complete tokenizer or checkpoint simulation', 'training_fixtures_illustrative': TRAIN, 'trace': trace, 'factorial': factorial, 'controls': {'known_adv_dolgo': 'passes old and fixed marker selection', 'okno': 'Actual tokenizer suffix-no classifies ADV; do not use as contrast fixture'}}
result['planned_native_budget_not_executed'] = {
    'main': {'states': ['A0', 'A1', 'B0', 'B1'], 'verify_calls': 16, 'train_on_example_calls': 4, 'unique_lessons': 2},
    'optional_ingress_lookup_decomposition': {'additional_verify_calls': 8, 'additional_train_on_example_calls': 0},
    'optional_cold_restore': {'additional_export_calls': 2, 'additional_import_calls': 2, 'additional_verify_calls': 8, 'additional_train_on_example_calls': 0},
    'all_three_if_separately_chosen': {'verify_calls': 32, 'train_on_example_calls': 4},
}
out = args.output
out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n')
print(json.dumps({'status': result['status'], 'source_hashes_verified': len(SHAS), 'factorial_accuracy': [x['accuracy'] for x in factorial], 'output': str(out)}, ensure_ascii=False))
