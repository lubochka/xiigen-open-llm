#!/usr/bin/env python3
"""Verify preparation assembly, declared lineage and source/navigation integrity.

No model, learner, generated code, compiler or database is executed. Temporary
request/prompt files and DATA_ONLY_TEST_FIXTURE rows are not training evidence.
Run from the preparation checkout with --rag-root pointing to its main RAG copy.
Reports use exclusive creation so earlier evidence is not silently replaced.
"""
import argparse
import contextlib
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import re
import shlex
import sys
import tempfile
from types import SimpleNamespace
from urllib.parse import unquote

BASE = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--rag-root', required=True)
    parser.add_argument('--out', required=True, help='New JSON report; never overwritten')
    args = parser.parse_args()
    rag = Path(args.rag_root).resolve()
    spec = importlib.util.spec_from_file_location('xiigen_expansion_sets', BASE / 'tools/sets.py')
    sets = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(sets)
    index = sets.obj(BASE / 'curriculum/index.json')
    queue = sets.obj(BASE / 'curriculum/pilot-queue.json')['streams']
    report = {'scope': 'DATA_PREPARATION_ONLY_NOT_LEARNING', 'model_calls': 0,
              'native_oracle_runs': 0, 'student_training_runs': 0, 'checks': []}
    watched = ['tools/sets.py', 'sources.lock.json', 'curriculum/index.json',
               'curriculum/pilot-queue.json', 'curriculum/scale-plan.json',
               'curriculum/reference-examples.jsonl', '14-GENERATION-CAMPAIGN.md',
               'prompts/MASTER-PROMPT-RU.md', 'prompts/REPAIR-PROMPT-RU.md',
               'verification/check-expansion-v3.py']
    watched += ['curriculum/' + name for name in index['pack_files']]
    watched += [o['card'] for o in index['objectives']]
    report['file_sha256'] = {p: sets.digest(BASE / p) for p in watched}
    report['rag_manifest_sha256'] = sets.digest(rag / 'rag/manifest.json')

    def check(name, fn):
        try:
            detail = fn()
            report['checks'].append({'name': name, 'passed': True, 'detail': detail})
        except Exception as exc:
            report['checks'].append({'name': name, 'passed': False,
                                     'error': type(exc).__name__ + ': ' + str(exc)})

    def references():
        data = sets.rows(BASE / 'curriculum/reference-examples.jsonl')
        assert len(data) == index['counts']['reference_examples']
        assert all(r['split'] == 'reference' for r in data)
        errors = sets.validate_rows(data, allow_reference=True)
        assert not errors, errors
        packed = {r.get('pair_id', r.get('example_id')): r for lesson in sets.objectives().values() for r in lesson['examples']}
        assert None not in packed
        assert set(packed) == {r['pair_id'] for r in data}
        for row in data:
            for field in ['prompt', 'chosen', 'rejected', 'family_id']:
                assert row[field] == packed[row['pair_id']][field], (row['pair_id'], field)
        return {'public_rows': len(data), 'objectives': len(sets.objectives()),
                'semantic_or_runtime_claim': False}

    def sources():
        items = sets.obj(rag / 'rag/manifest.json')['sources']
        catalog = sets.source_catalog()
        assert len(items) == len(catalog) == len({r['source_id'] for r in items})
        assert set(catalog) == {r['source_id'] for r in items}
        for row in items:
            other = catalog[row['source_id']]
            for field in ['commit', 'blob_sha', 'path', 'branch', 'local_excerpt_sha256']:
                assert row[field] == other[field], (row['source_id'], field)
            assert row['branch'] in ['main', 'llm_plan3']
            assert re.fullmatch('[0-9a-f]{40}', row['commit'])
            assert re.fullmatch('[0-9a-f]{40}', row['blob_sha'])
            assert sets.digest(rag / 'rag' / row['local_excerpt_path']) == row['local_excerpt_sha256']
        return {'sources': len(items), 'lock_matches_manifest': True,
                'content_truth_not_proven_by_hash': True}

    def assembly():
        count = 0
        paths = set()
        win_paths = []
        with tempfile.TemporaryDirectory(prefix='xiigen-expansion-') as td:
            work = Path(td)
            for item in queue:
                for split in ['train', 'dev', 'exam']:
                    argv = shlex.split(item['prepare_first_' + split])
                    assert argv[:3] == ['python', 'tools/sets.py', 'prepare']
                    def value(flag): return argv[argv.index(flag) + 1]
                    params = dict(topic=value('--topic'), profile=value('--profile'),
                                  objective=value('--objective'), run=value('--run'),
                                  count=int(value('--count')), start=int(value('--start')),
                                  split=value('--split'), work_root=str(work),
                                  rag_root=str(rag), family_plan=None)
                    with contextlib.redirect_stdout(io.StringIO()):
                        sets.prepare(SimpleNamespace(**params))
                    folder = work / params['topic'] / params['profile'] / params['run']
                    assert str(folder) not in paths
                    paths.add(str(folder))
                    req = sets.obj(folder / 'request.json')
                    state = sets.obj(folder / 'state.json')
                    prompt = (folder / 'prompt.md').read_text(encoding='utf-8')
                    for key in ['topic_id', 'profile_id', 'domain_id', 'objective_id']:
                        assert req[key] == item[key], (item['stream_id'], key)
                    assert req['split'] == split and req['count'] == item['batch_pairs']
                    assert req['card_sha256'] == sets.digest(BASE / item['card'])
                    assert (BASE / item['card']).read_text(encoding='utf-8') in prompt
                    assert req['training_status'] == state['training_status'] == 'NOT_RUN'
                    assert state['process_handle'] is None
                    assert len(req['family_ids']) * req['pairs_per_family'] == req['count']
                    assert all(sets.family_split(f) == split for f in req['family_ids'])
                    for ref in req['sources']:
                        sid = sets.source_id(ref)
                        assert (rag / 'rag/sources' / (sid + '.md')).read_text(encoding='utf-8') in prompt
                    win_paths.append('/'.join([value('--work-root'), params['topic'],
                                              params['profile'], params['run'], 'request.json']))
                    count += 1
            for item in queue:
                win_paths += [item['dataset_directory'] + '/release.json',
                              item['baseline_directory'] + '/bundle.json']
        assert count == len(queue) * 3
        assert max(map(len, win_paths)) < 260
        return {'streams': len(queue), 'request_prompt_state_assemblies': count,
                'max_designated_windows_path_chars': max(map(len, win_paths)),
                'model_calls': 0, 'temporary_files_only': True}

    def origin_regression():
        # Reproduces the reported gap: different family/group IDs must not hide
        # the SAME explicitly declared source across TRAIN and EXAM.
        prototype = copy.deepcopy(sets.rows(BASE / 'curriculum/reference-examples.jsonl')[0])
        data = []
        for n, split in enumerate(['train', 'exam']):
            row = copy.deepcopy(prototype)
            row.update(pair_id='DATA_ONLY_ORIGIN_' + str(n), family_id='data-only-family-' + str(n),
                       split=split, prompt='DATA_ONLY unique input ' + str(n),
                       chosen='DATA_ONLY chosen ' + str(n), rejected='DATA_ONLY alternative ' + str(n))
            row['provenance'] = {'kind': 'DATA_ONLY_TEST_FIXTURE', 'generator_model': 'NO_MODEL_CALL'}
            row['metadata'] = {'learning_objective_id': prototype['metadata']['learning_objective_id'],
                               'origin_family_id': 'data-only-shared-origin',
                               'leakage_group_ids': ['data-only-distinct-group-' + str(n)]}
            data.append(row)
        errors = sets.validate_rows(data)
        assert any('ORIGIN_FAMILY_SPLIT_LEAKAGE' in e or 'RELATED_FAMILY_SPLIT_LEAKAGE' in e for e in errors), errors
        same_split = copy.deepcopy(data)
        same_split[1]['split'] = 'train'
        assert not sets.validate_rows(same_split)
        independent = copy.deepcopy(data)
        independent[1]['metadata']['origin_family_id'] = 'data-only-independent-origin'
        assert not sets.validate_rows(independent)
        invalid_results = []
        for invalid in ['', ' ', None, [], {}]:
            bad = copy.deepcopy(data[:1])
            bad[0]['metadata']['origin_family_id'] = invalid
            errors_bad = sets.validate_rows(bad)
            assert any('ORIGIN' in e for e in errors_bad), (invalid, errors_bad)
            invalid_results.append(type(invalid).__name__)
        return {'cross_split_shared_origin': 'refused', 'same_split_shared_origin': 'accepted_structurally',
                'different_origins': 'accepted_structurally', 'invalid_types_refused': invalid_results}

    def local_links():
        checked = 0
        for root in [BASE, rag]:
            for path in root.rglob('*.md'):
                body = re.sub(r'```.*?```', '', path.read_text(encoding='utf-8'), flags=re.S)
                for target in re.findall(r'(?<!!)\[[^\]\n]*\]\(([^)\n]+)\)', body):
                    target = target.strip().split('#', 1)[0]
                    if not target or re.match(r'^[a-zA-Z][a-zA-Z0-9+.-]*:', target):
                        continue
                    dest = path.parent / unquote(target)
                    assert dest.exists(), str(path.relative_to(root)) + ' -> ' + target
                    checked += 1
        return {'relative_markdown_paths_checked': checked, 'anchor_and_remote_URL_checks': 'not_in_scope'}

    def scale_counts():
        scale = sets.obj(BASE / 'curriculum/scale-plan.json')
        execution = sets.obj(BASE / 'execution-queue.json')['streams']
        assert [s['stream_id'] for s in scale['streams']] == [s['stream_id'] for s in execution]
        for item in scale['streams']:
            assert item['target_accepted_train_pairs'] % item['complete_group_size'] == 0
            if item['pilot_train_pairs'] is not None:
                assert item['additional_after_pilot_pairs'] + item['pilot_train_pairs'] == item['target_accepted_train_pairs']
        assert sum(s['target_accepted_train_pairs'] for s in scale['streams']) == scale['counts']['target_accepted_train_pairs']
        allocated = 0
        reserve = 0
        for pool in scale['budget_pools']:
            assert sum(x['target_accepted_train_pairs'] for x in pool['allocations']) == pool['allocated_train_pairs']
            assert pool['allocated_train_pairs'] + pool['reserve_train_pairs'] == pool['parent_train_budget_pairs']
            allocated += pool['allocated_train_pairs']
            reserve += pool['reserve_train_pairs']
        assert allocated == scale['counts']['target_accepted_train_pairs']
        assert allocated + reserve == scale['counts']['parent_train_budget_pairs']
        assert scale['counts']['generated_pairs_observed_this_update'] == 0
        assert scale['counts']['accepted_pairs_observed_this_update'] == 0
        return scale['counts']

    check('all_public_reference_records_and_packs', references)
    check('rag_lock_excerpt_integrity', sources)
    check('every_train_dev_exam_command_assembles', assembly)
    check('declared_origin_cross_split_regression', origin_regression)
    check('relative_document_links', local_links)
    check('scale_plan_arithmetic_and_groups', scale_counts)
    report['passed'] = all(c['passed'] for c in report['checks'])
    report['counts'] = {'checks': len(report['checks']), 'passed': sum(c['passed'] for c in report['checks'])}
    target = Path(args.out)
    target.parent.mkdir(parents=True, exist_ok=True)
    with target.open('x', encoding='utf-8') as f:
        json.dump(report, f, ensure_ascii=False, indent=2, allow_nan=False)
        f.write('\n')
    print(json.dumps({'passed': report['passed'], 'counts': report['counts'],
                      'checks': report['checks']}, ensure_ascii=False, indent=2))
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    sys.exit(main())
