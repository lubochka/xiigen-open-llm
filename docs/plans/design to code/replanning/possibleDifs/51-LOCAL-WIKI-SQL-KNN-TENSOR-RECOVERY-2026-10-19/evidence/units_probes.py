#!/usr/bin/env python3
"""Read-only mechanistic audit. No model, native C#, training, or product calls.

Usage: python units_probes.py --source-root /path/to/snapshots --out receipt.json
Snapshot folders: current/<repository path> and wiki/<repository path>.
Inputs are exact Git blobs fetched at immutable core/wiki commits. The script
checks bytes before reading receipts. Its synthetic prompt collision is a
source-equivalent diagnostic, never a quality result.
"""
from __future__ import annotations

import argparse
import collections
import hashlib
import json
import re
from pathlib import Path

CURRENT = '4945b8e85c17b5f150eb5b840d16bbdc4b506242'
DONOR = '219d7ba89dc3ef43e03e17f1461ad96154d3dd7a'


def blob_sha(data):
    return hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()


def digest(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, ensure_ascii=False).encode()).hexdigest()


def read_verified(root, branch, path, expected, checks):
    data = (root / branch / path).read_bytes()
    actual = blob_sha(data)
    if actual != expected:
        raise ValueError(f'Source hash mismatch: {branch}/{path}: {actual} != {expected}')
    checks.append({'branch': branch, 'path': path, 'git_blob_sha': actual})
    return data.decode('utf-8-sig')


def words(text):
    # All probe inputs are ASCII. This matches the relevant C# char predicates.
    return re.findall(r'[a-z0-9_-]+', text.lower())


def stable_hash(text):
    value = 2166136261
    for char in text:
        value = ((value ^ ord(char)) * 16777619) & 0xffffffff
    return value & 0x7fffffff


def prefix_ids(text, context_len=192, vocab_size=2048):
    return [1 + stable_hash(w) % (vocab_size - 1)
            for w in words(text)[:max(8, context_len // 4)]]


def shape_types(source, name):
    block = re.search(r'AnswerShape\[\] ' + name + r'\s*=\s*\[(.*?)\];', source, re.S)
    if not block:
        return None
    return re.findall(r',\s*"([a-z_]+)"\)', block.group(1))


def run(root):
    trees = json.loads((Path(__file__).resolve().parent / 'units-source-tree.json').read_text())
    checks = []
    sources = {}
    paths = ['42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs',
             '42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingPipeline.cs',
             '42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingRunner.cs',
             '42.ProjectWikiDomain/ProjectLocalWikiModelUnit.cs',
             '42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs']
    for branch in ['current', 'wiki']:
        for path in paths:
            sources[branch, Path(path).name] = read_verified(
                root, branch, path, trees[branch][path], checks)

    runs = {}
    manifest = json.loads((Path(__file__).resolve().parent / 'historical-manifests.json').read_text())
    for item in manifest:
        text = read_verified(root, 'wiki', item['path'], item['sha'], checks)
        filename = Path(item['path']).name
        runs.setdefault(item['id'], {})[filename] = (
            [json.loads(line) for line in text.splitlines() if line.strip()]
            if filename.endswith('.jsonl') else json.loads(text))

    history = {}
    for run_id, files in runs.items():
        summary = files['project-wiki-dpo-metric-summary.json']
        loop = files['project-wiki-training-loop-result.json']['value']
        row = {'run_id': summary['runId'],
               'train_rows': summary['trainingRowCount'],
               'heldout_rows': summary['heldOutRowCount'],
               'epochs': summary['epochCount'],
               'learning_rate': summary['learningRate'],
               'heldout_accuracy': summary['heldOutAccuracy'],
               'policy_initialization': summary['policyInitialization'],
               'reference_initialization': summary['referenceInitialization'],
               'initial_policy_reference_max_abs_delta': summary['initialPolicyReferenceMaxAbsDelta'],
               'elapsed_seconds': summary['totalElapsedMs'] / 1000,
               'structural_overall_verdict': loop['overallVerdict'],
               'loop_summary_passed': loop['summary']['passed'],
               'loop_warnings': loop['warnings']}
        if 'dpo-pairs.jsonl' in files:
            pairs = files['dpo-pairs.jsonl']
            diagnostics = files['dpo-pair-diagnostics.jsonl']
            groups = collections.defaultdict(list)
            for item in diagnostics:
                if item['Split'] == 'heldout':
                    groups[item['RejectedType']].append(item['Accuracy'])
            row['heldout_types'] = {key: {'n': len(vals), 'correct': sum(vals),
                                          'accuracy': sum(vals) / len(vals)}
                                    for key, vals in groups.items()}
            row['readonly_chosen_rows'] = sum('SQL_TABLE_WRITTEN_COUNT_0' in p['chosen'] for p in pairs)
            row['contradictory_readonly_write_present_lines'] = [i + 1 for i, p in enumerate(pairs)
                if 'SQL_TABLE_WRITTEN_COUNT_0' in p['chosen'] and 'SQL_TABLE_WRITES_PRESENT' in p['chosen']]
            row['explicit_no_writes_lines'] = [i + 1 for i, p in enumerate(pairs)
                if 'SQL_TABLE_WRITES_NONE_CONFIRMED' in p['chosen']]
            triples = [(p['prompt'], p['chosen'], p['rejected']) for p in pairs]
            row['semantic_corpus_sha256_excluding_ids'] = digest(triples)
            row['exact_train_heldout_triplet_overlap'] = len(set(triples[:50]) & set(triples[50:]))
        history[run_id] = row

    comparisons = {}
    for left, right in [('110', '111'), ('111', '112'), ('112', '113')]:
        old = runs[left]['dpo-pairs.jsonl']; new = runs[right]['dpo-pairs.jsonl']
        diff = {key: sum(a[key] != b[key] for a, b in zip(old, new))
                for key in ['prompt', 'chosen', 'rejected']}
        added = collections.Counter(); removed = collections.Counter()
        for a, b in zip(old, new):
            added.update(set(b['chosen'].split()) - set(a['chosen'].split()))
            removed.update(set(a['chosen'].split()) - set(b['chosen'].split()))
        comparisons[f'{left}_to_{right}'] = {
            'changed_fields_by_row': diff, 'chosen_added_token_row_counts': dict(added),
            'chosen_removed_token_row_counts': dict(removed)}

    generator_current = sources['current', 'ProjectWikiAnswerComposerDpoGenerator.cs']
    generator_wiki = sources['wiki', 'ProjectWikiAnswerComposerDpoGenerator.cs']
    shapes = {}
    for branch, text in [('current', generator_current), ('wiki', generator_wiki)]:
        shapes[branch] = {}
        for name in ['StoredProcedureAnswerShapes', 'StoredProcedureTableEffectsFocusAnswerShapes']:
            vals = shape_types(text, name)
            shapes[branch][name] = None if vals is None else dict(collections.Counter(vals))
    assert shapes['wiki']['StoredProcedureTableEffectsFocusAnswerShapes']['missing_table_side_effects'] == 7
    assert shapes['current']['StoredProcedureTableEffectsFocusAnswerShapes'] is None

    # Synthetic same question, two accepted documentation versions. Correct
    # pair preference reverses. Output wording has equal IDs, not gold metadata.
    common_question = 'Which tables does InvoiceRepository.MarkInvoicePaid write?'
    fixtures = [
        {'Request': {'ProjectId': 'billing', 'DocumentationVersion': 'v1'},
         'A': common_question,
         'Evidence': {'EvidenceId': 'ev-pay', 'SymbolId': 'InvoiceRepository.MarkInvoicePaid',
                      'SqlTablesRead': ['billing.Invoices'], 'SqlTablesWritten': [],
                      'Returns': 'InvoicePaymentPreview', 'SqlTransactionBehavior': 'read only preview'}},
        {'Request': {'ProjectId': 'billing', 'DocumentationVersion': 'v2'},
         'A': common_question,
         'Evidence': {'EvidenceId': 'ev-pay', 'SymbolId': 'InvoiceRepository.MarkInvoicePaid',
                      'SqlTablesRead': ['billing.Invoices'],
                      'SqlTablesWritten': ['billing.Invoices', 'billing.InvoiceAudit'],
                      'Returns': 'InvoicePaymentResult', 'SqlTransactionBehavior': 'transactional update'}}]
    pipeline = sources['current', 'ProjectWikiAnswerComposerTrainingPipeline.cs']
    block = pipeline[pipeline.index('private static ProjectWikiDpoMetricPair ToDpoMetricPair'):]
    assert 'pair.A' in block and 'pair.Chosen' in block and 'pair.Rejected' in block
    assert 'pair.Evidence' not in block and 'pair.Request' not in block
    # Decode only the C# string literal, preserving its actual newline value.
    literal = re.search(r'"(Task: compose.*)" \+ pair.A', block).group(1)
    instruction = json.loads('"' + literal + '"')
    current_prompts = [instruction + f['A'] for f in fixtures]
    donor_block = sources['wiki', 'ProjectWikiAnswerComposerTrainingPipeline.cs']
    assert 'pair.Evidence' not in donor_block[donor_block.index('private static ProjectWikiDpoMetricPair ToDpoMetricPair'):]
    enriched_prompts = [p + '\nAcceptedEvidence: ' + json.dumps(
        {'Request': f['Request'], 'Evidence': f['Evidence']}, sort_keys=True)
        for p, f in zip(current_prompts, fixtures)]
    # Common 1024 context is a diagnostic setup to avoid an unrelated prefix cap;
    # it is not a claim that production has this value.
    collision = {
        'synthetic_fixtures': fixtures,
        'rich_input_hashes_differ': digest(fixtures[0]) != digest(fixtures[1]),
        'current_prompt_hashes_equal': digest(current_prompts[0]) == digest(current_prompts[1]),
        'current_default_prefix_ids_equal': prefix_ids(current_prompts[0]) == prefix_ids(current_prompts[1]),
        'enriched_raw_prompts_differ': enriched_prompts[0] != enriched_prompts[1],
        'append_only_at_default_context_still_collides': prefix_ids(enriched_prompts[0]) == prefix_ids(enriched_prompts[1]),
        'enriched_at_common_diagnostic_context1024_prefix_ids_differ': prefix_ids(enriched_prompts[0], 1024) != prefix_ids(enriched_prompts[1], 1024),
        'context_independent_preference_maximum_on_reversed_pair': 0.5,
        'boundary': 'The 0.5 bound is reasoning for two contradictory preferences at identical x, not measured training accuracy. Equal prior state, equal candidate strings, no hidden side channel assumed.'}
    assert collision['current_prompt_hashes_equal'] and collision['enriched_raw_prompts_differ']
    assert collision['enriched_at_common_diagnostic_context1024_prefix_ids_differ']

    return {'status': 'READ_ONLY_SOURCE_AND_RECEIPT_PROXY',
            'native_training_runs': 0, 'model_generations': 0,
            'pins': {'current': CURRENT, 'donor': DONOR},
            'verified_source_count': len(checks), 'verified_sources': checks,
            'historical_runs': history, 'historical_pair_comparisons': comparisons,
            'source_curriculum_shape_counts': shapes,
            'synthetic_answer_composer_evidence_projection': collision}


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--source-root', type=Path, required=True)
    parser.add_argument('--out', type=Path)
    args = parser.parse_args()
    result = run(args.source_root)
    output = args.out or Path(__file__).resolve().parent / 'units-probe-receipt.json'
    output.write_text(json.dumps(result, indent=2, ensure_ascii=False) + '\n')
    print(json.dumps({'output': str(output), 'verified_sources': result['verified_source_count'],
                      'native_training_runs': 0,
                      'source_collision_confirmed': result['synthetic_answer_composer_evidence_projection']['current_prompt_hashes_equal']}))
