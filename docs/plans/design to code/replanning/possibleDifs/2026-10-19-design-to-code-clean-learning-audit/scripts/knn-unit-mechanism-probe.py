#!/usr/bin/env python3
"""Bounded source-equivalent RU tokenizer probe; NOT a native/model-quality run.

Reads the pinned source cache. Tables and suffix order are extracted from C#;
the port is intentionally limited to proper UTF-8 Russian and the fixed rows
below. No source writes, teacher calls, checkpoints, or optimizer calls.
"""
from pathlib import Path
import argparse
import hashlib
import json
import re
import unicodedata

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source', type=Path, default=ROOT / 'core/27.TranslationDomain/TranslationSemanticTokenizer.cs')
args = parser.parse_args()
SRC_PATH = args.source
source_bytes = SRC_PATH.read_bytes()
EXPECTED_BLOB = 'ee6d81c7933fa33a9c57a2111990019ec55e5dab'
def git_blob_sha(data):
    return hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
cache_extra_terminal_lf_removed = False
if git_blob_sha(source_bytes) != EXPECTED_BLOB and source_bytes.endswith(b'\n') and git_blob_sha(source_bytes[:-1]) == EXPECTED_BLOB:
    source_bytes = source_bytes[:-1]
    cache_extra_terminal_lf_removed = True
if git_blob_sha(source_bytes) != EXPECTED_BLOB:
    raise SystemExit('Source is not the pinned C# blob; refuse to mix code revisions.')
SRC = source_bytes.decode('utf-8')

def section(a, b):
    return SRC.split(a, 1)[1].split(b, 1)[0]

def strings(text):
    return re.findall(r'"([^"\\]*(?:\\.[^"\\]*)*)"', text)

BASE = re.findall(r'\{ "([^"\n]+)", SemanticConcept\.([A-Z_]+) \}',
    section('BaseConceptsRu', 'private static readonly HashSet<string> RuPrepositions'))
CONTEXT = re.findall(r'\{ \("([^"\n]+)", SemanticConcept\.([A-Z_]+)\), SemanticConcept\.([A-Z_]+) \}',
    section('E26Mappings = new()', 'private static readonly Dictionary<string, SemanticConcept> BaseConceptsRu'))
CONTEXT = {(stem, prev): value for stem, prev, value in CONTEXT}
PREPS = set(strings(section('RuPrepositions', 'private static readonly HashSet<string> RuPronouns')))
PRONS = set(strings(section('RuPronouns', 'private static readonly HashSet<string> RuAdverbAllowList')))
ADVERBS = set(strings(section('RuAdverbAllowList', 'public SemanticTokenSequence Tokenize')))
SUFFIXES = strings(section('private static string TrimVerbSuffix', 'foreach (var suffix in new[] { "Ñ'))
assert SUFFIXES[:4] == ['ться', 'лась', 'лся', 'ала']
VERB_SOURCE = section('private static bool LooksLikeRussianVerbUtf8', 'private static bool LooksLikeRussianGenitiveUtf8')
VERB_SUFFIXES = re.findall(r'EndsWith\("([^"\n]+)"', VERB_SOURCE)
ADVERB_SUFFIXES = re.findall(r'EndsWith\("([^"\n]+)"',
    section('private static bool LooksLikeRussianAdverbUtf8', 'private static bool ContainsCyrillic'))
GEN_SUFFIXES = re.findall(r'EndsWith\("([^"\n]+)"',
    section('private static bool LooksLikeRussianGenitiveUtf8', 'private static bool LooksLikeRussianAdverbUtf8'))

def clean_word(word):
    start, end = 0, len(word)
    while start < end and unicodedata.category(word[start])[0] in 'PS':
        start += 1
    while end > start and unicodedata.category(word[end-1])[0] in 'PS':
        end -= 1
    return word[start:end]

def trim_verb(word):
    for suffix in SUFFIXES:
        if word.endswith(suffix) and len(word) > len(suffix) + 1:
            return word[:-len(suffix)]
    return word

def classify(word):
    lower = clean_word(word).lower()
    if lower in ('null', 'true', 'false'):
        return 'TECH_TERM', lower
    if lower in PREPS:
        return 'PREP', lower
    if lower in PRONS:
        return 'PRON', lower
    if lower in ADVERBS or (len(lower) >= 4 and any(lower.endswith(s) for s in ADVERB_SUFFIXES)):
        return 'ADV', lower
    if any(lower.endswith(s) and (s != 'л' or len(lower) > 3) for s in VERB_SUFFIXES) or (lower.endswith('ла') and len(lower) > 3):
        return 'VERB', trim_verb(lower)
    if any(lower.endswith(s) for s in GEN_SUFFIXES):
        return 'NOUN_GEN', lower
    return 'NOUN', lower

def base_concept(lemma):
    return next((c for prefix, c in BASE if lemma.casefold().startswith(prefix.casefold())), 'UNKNOWN')

def concept(word, pos, lemma, prev, use_surface_first=False):
    if pos == 'TECH_TERM' and lemma == 'null':
        return 'NULL_LITERAL'
    if pos in ('PREP', 'PRON', 'ADV'):
        return 'UNKNOWN'
    known = base_concept(clean_word(word).lower()) if use_surface_first else 'UNKNOWN'
    if known == 'UNKNOWN':
        known = base_concept(lemma)
    if known == 'UNKNOWN':
        for n in (6, 5):
            stem = lemma[:n] if len(lemma) > n else lemma
            if (stem, prev) in CONTEXT:
                return CONTEXT[stem, prev]
    return known

def analyze(text, use_surface_first=False, clean_gender_word=False, past_only_when_morphological=False):
    tokens, previous = [], 'UNKNOWN'
    for word in [x for x in text.split(' ') if x]:
        pos, lemma = classify(word)
        value = concept(word, pos, lemma, previous, use_surface_first)
        tokens.append({'word': word, 'lemma': lemma, 'pos': pos, 'concept': value})
        if value != 'UNKNOWN':
            previous = value
    first_verb = next((i for i, t in enumerate(tokens) if t['pos'] == 'VERB'), -1)
    subject_area = tokens[:first_verb] if first_verb > 0 else tokens
    subject = next((t for t in subject_area if t['pos'] in ('PRON', 'NOUN')), None)
    def gender_word(t):
        return (clean_word(t['word']) if clean_gender_word else t['word']).lower()
    subject_gender = None
    if subject:
        raw, lemma = gender_word(subject), subject['lemma'].lower()
        if raw in ('она', 'та') or lemma in ('она', 'та'):
            subject_gender = 'FEM'
        elif raw in ('он', 'тот') or lemma in ('он', 'тот'):
            subject_gender = 'MASC'
        elif subject['pos'] == 'NOUN' and any('\u0400' <= c <= '\u04ff' for c in raw):
            subject_gender = 'FEM' if raw.endswith(('а', 'я')) else 'MASC'
    tags = []
    def add(tag):
        if tag not in tags:
            tags.append(tag)
    if subject_gender:
        add('RU:SUBJECT:' + subject_gender)
    for token in tokens:
        if token['pos'] == 'VERB':
            raw = gender_word(token)
            verb_gender = ('FEM' if raw.endswith(('лась', 'ла')) else
                'PLUR' if raw.endswith(('лись', 'ли')) else
                'MASC' if raw.endswith('лся') or (raw.endswith('л') and len(raw) > 3) else None)
            if not past_only_when_morphological or verb_gender is not None:
                add('RU:TENSE:PAST')
            if verb_gender:
                add('RU:VERB_GENDER:' + verb_gender)
                if subject_gender:
                    add(f'RU:AGREEMENT:SUBJECT_{subject_gender}_PAST_{verb_gender}')
        if token['pos'] == 'NOUN_GEN':
            add('RU:CASE:GENITIVE')
        if token['pos'] == 'PREP':
            cases = {'к': 'DATIVE', 'из': 'GENITIVE', 'от': 'GENITIVE',
                'с': 'INSTRUMENTAL_OR_GENITIVE', 'со': 'INSTRUMENTAL_OR_GENITIVE',
                'в': 'ACCUSATIVE_OR_PREPOSITIONAL', 'на': 'ACCUSATIVE_OR_PREPOSITIONAL'}
            if token['lemma'] in cases:
                add('RU:CASE:' + cases[token['lemma']])
    has_pron_before = first_verb > 0 and any(t['pos'] == 'PRON' for t in tokens[:first_verb])
    pattern = ('UNKNOWN' if not tokens else 'IMPERATIVE_V' if first_verb == 0 and not has_pron_before else
        'SOV' if has_pron_before and first_verb > 0 and tokens[-1]['pos'] == 'VERB' else 'SVO')
    concepts = [t['concept'] for t in tokens if t['concept'] != 'UNKNOWN']
    return {'source': text, 'tokens': tokens, 'rule_tags': tags,
        'signature': '|'.join([pattern] + concepts + tags),
        'prefix_signature': '|'.join([pattern] + concepts[:2] + tags[:3]),
        'structure_signature': '|'.join([pattern] + [t['pos'] for t in tokens] + tags),
        'ru_en_semantic_knn_gate': bool(concepts)}

forms = ['добавить', 'добавите', 'добавит', 'запустить', 'запустите', 'запустит', 'проверить', 'проверите', 'проверит', 'файл', 'проверку', 'null', 'метод']
lexical = []
for word in forms:
    a, b = analyze(word), analyze(word, use_surface_first=True)
    lexical.append({'word': word, 'lemma': a['tokens'][0]['lemma'], 'pos': a['tokens'][0]['pos'],
        'current_concept': a['tokens'][0]['concept'], 'surface_first_concept': b['tokens'][0]['concept']})
lost = [r for r in lexical if r['current_concept'] == 'UNKNOWN' and r['surface_first_concept'] != 'UNKNOWN']
tense = []
for text, label in [('Она работает', 'present'), ('Она будет работать', 'future'), ('работать', 'infinitive'),
                    ('запустите поток', 'imperative'), ('Она ответила', 'past'), ('Он ответил', 'past')]:
    a = analyze(text)
    tense.append({'source': text, 'linguistic_fixture_label': label, 'current_rule_tags': a['rule_tags'],
        'false_past_tag_on_nonpast_fixture': label != 'past' and 'RU:TENSE:PAST' in a['rule_tags']})
punctuation = []
for left, right in [('Она ответила', 'Она ответила.'), ('Она вчера ответила', 'Она вчера ответила.'),
                    ('Мама пришла', 'Мама пришла.'), ('Он ответил', 'Он ответил.')]:
    a, b = analyze(left), analyze(right)
    fixed_b = analyze(right, clean_gender_word=True)
    punctuation.append({'source_a': left, 'source_b': right,
        'pos_a': [t['pos'] for t in a['tokens']], 'pos_b': [t['pos'] for t in b['tokens']],
        'tags_a': a['rule_tags'], 'tags_b': b['rule_tags'],
        'current_signatures_equal': a['signature'] == b['signature'],
        'clean_gender_signatures_equal': a['signature'] == fixed_b['signature']})
chain = [{'current': analyze(x), 'surface_first': analyze(x, use_surface_first=True)}
         for x in ['запустить поток', 'запустите поток']]
result = {
    'evidence_type': 'SOURCE_EQUIVALENT_FORMULA_PROBE',
    'native_dotnet_run': False, 'training_run': False, 'quality_gain_established': False,
    'current_commit': '358af351e22a92a3f7938d1f9221d5aeb7ddb053',
    'clean_commit': 'e346b88a3cc1fcce48aefa626bfc7b00509eefd6',
    'git_source_blob_both_commits': 'ee6d81c7933fa33a9c57a2111990019ec55e5dab',
    'source_bytes_sha256': hashlib.sha256(source_bytes).hexdigest(),
    'source_git_blob_verified': git_blob_sha(source_bytes),
    'cache_extra_terminal_lf_removed': cache_extra_terminal_lf_removed,
    'scope': 'Proper UTF-8 RU inputs listed here; no mojibake, unknown source languages or learned corrections',
    'probe_revision_note': 'First attempt expected файл to be a preserved noun control; source-equivalent result falsified that assumption: suffix -л marks it VERB and strips л. Original JSON retained in knn-unit-mechanism-probe-initial-failed-control.json. File is now an explicit lexical anomaly; метод is the independently unchanged control.',
    'lexical_rows': lexical, 'lost_dictionary_concepts': len(lost),
    'tense_rows': tense, 'punctuation_rows': punctuation, 'e26_knn_gate_chain': chain,
    'assertions': {
        'dictionary_misses_equal_10_including_file_false_verb': len(lost) == 10,
        'three_lexical_controls_preserved': all(r['current_concept'] == r['surface_first_concept'] != 'UNKNOWN' for r in lexical[-3:]),
        'four_nonpast_fixtures_receive_past': sum(r['false_past_tag_on_nonpast_fixture'] for r in tense) == 4,
        'two_past_controls_keep_past': all('RU:TENSE:PAST' in r['current_rule_tags'] for r in tense if r['linguistic_fixture_label'] == 'past'),
        'punctuation_changes_all_4_signatures': all(not r['current_signatures_equal'] for r in punctuation),
        'clean_gender_restores_all_4_signatures': all(r['clean_gender_signatures_equal'] for r in punctuation),
        'two_e26_context_gates_open_only_after_surface_lookup': all(not r['current']['ru_en_semantic_knn_gate'] and r['surface_first']['ru_en_semantic_knn_gate'] for r in chain)
    }
}
print(json.dumps(result, ensure_ascii=False, indent=2))
if not all(result['assertions'].values()):
    raise SystemExit('Formula assertion failed; inspect output, do not claim native validation.')
