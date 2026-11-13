#!/usr/bin/env python3
"""Focused offline regression checks for DPO preparation input integrity.

All generated rows and review receipts are DATA_ONLY_TEST_FIXTURE objects in a
TemporaryDirectory. They are NOT semantic reviews, Chinese-model outputs,
runtime oracle evidence or measurements of learning. No model, compiler,
database, browser or other generated code is executed. The only permanent
output, when --out is supplied, is a hash-bound check report.
"""
import argparse
import contextlib
import copy
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import shlex
import sys
import tempfile
from types import SimpleNamespace

BASE = Path(__file__).resolve().parents[1]


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--rag-root', required=True, help='Actual main-branch preparation folder containing rag/manifest.json')
    ap.add_argument('--out', help='New JSON report path; existing reports are not overwritten')
    args = ap.parse_args()
    spec = importlib.util.spec_from_file_location('xiigen_sets_under_review', BASE / 'tools/sets.py')
    sets = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(sets)
    files = ['tools/sets.py', 'index.json', 'sources.lock.json', 'curriculum/index.json', 'curriculum/pilot-queue.json',
             'verification/check-preparation-v2.py']
    index = sets.obj(BASE / 'curriculum/index.json')
    files += ['curriculum/' + x for x in index['pack_files']]
    files += [x['card'] for x in index['objectives']]
    report = {'scope': 'DATA_PREPARATION_REGRESSION_ONLY_NOT_LEARNING', 'model_calls': 0,
              'native_oracle_runs': 0, 'student_training_runs': 0,
              'file_sha256': {p: sets.digest(BASE / p) for p in files},
              'rag_manifest_sha256': sets.digest(Path(args.rag_root) / 'rag/manifest.json'), 'checks': []}

    def check(name, fn):
        try:
            detail = fn()
            report['checks'].append({'name': name, 'passed': True, 'detail': detail})
        except Exception as exc:
            report['checks'].append({'name': name, 'passed': False, 'error': type(exc).__name__ + ': ' + str(exc)})

    def quiet(fn, **kwargs):
        with contextlib.redirect_stdout(io.StringIO()):
            return fn(SimpleNamespace(**kwargs))

    def refused(fn, fragment):
        try:
            fn()
        except (ValueError, KeyError, TypeError) as exc:
            assert fragment in str(exc), str(exc)
            return str(exc)
        raise AssertionError('Expected refusal containing ' + fragment)

    def index_check():
        lessons = sets.objectives()
        assert len(lessons) == len(index['objectives']) == index['counts']['objectives']
        streams = index['stream_order']
        assert len(streams) == len({s['stream_id'] for s in streams}) == index['counts']['streams']
        for n, stream in enumerate(streams):
            assert stream['order'] == n + 1
            assert stream['next_stream_id'] == (streams[n + 1]['stream_id'] if n + 1 < len(streams) else None)
            lesson = lessons[stream['objective_id']]
            assert lesson['topic_id'] == stream['topic_id'] and stream['profile_id'] in lesson['profiles']
        for item in index['objectives']:
            assert (BASE / item['card']).is_file()
            assert item['source_refs'] == lessons[item['id']]['source_refs']
        return {'objectives': len(lessons), 'streams': len(streams), 'no_map_override': True}

    check('real_index_and_next_chain', index_check)

    def queue_check():
        queue = sets.obj(BASE / 'curriculum/pilot-queue.json')['streams']
        expected = index['stream_order']
        assert len(queue) == len(expected)
        outputs = set()
        for item, stream in zip(queue, expected):
            for field in ['order', 'stream_id', 'objective_id', 'topic_id', 'profile_id', 'domain_id', 'next_stream_id']:
                assert item[field] == stream[field], (item['stream_id'], field)
            assert item['first_output_directory'] not in outputs
            outputs.add(item['first_output_directory'])
            for split in ['train', 'dev', 'exam']:
                argv = shlex.split(item['prepare_first_' + split])
                def value(flag): return argv[argv.index(flag) + 1]
                assert value('--topic') == item['topic_id'] and value('--profile') == item['profile_id']
                assert value('--objective') == item['objective_id'] and value('--split') == split
                assert int(value('--count')) == item['batch_pairs']
                assert value('--start') == '0'
                assert item['batch_pairs'] % item['pairs_per_family'] == 0
                if split == 'train':
                    assert value('--run') == item['first_run_id']
                    assert '/'.join([value('--work-root'), item['topic_id'], item['profile_id'], value('--run')]) == item['first_output_directory']
        for split in ['train', 'dev', 'exam']:
            assert sum(s['planned_' + split + '_pairs'] for s in queue) == index['counts']['planned_pilot_' + split + '_pairs']
        return {'streams': len(queue), 'commands_checked': len(queue) * 3, 'paths_unique': True}

    check('pilot_queue_commands_counts_and_paths', queue_check)
    with tempfile.TemporaryDirectory(prefix='xiigen-preparation-check-') as td:
        work = Path(td)
        prepared = {}

        def prepare(oid, topic, profile, count, run, start=0):
            quiet(sets.prepare, topic=topic, objective=oid, profile=profile, count=count, run=run,
                  start=start, split='train', work_root=str(work), rag_root=args.rag_root, family_plan=None)
            folder = work / topic / profile / run
            req = sets.obj(folder / 'request.json')
            prompt = (folder / 'prompt.md').read_text(encoding='utf-8')
            assert req['objective_id'] == oid and req['count'] == count
            assert req['card_sha256'] == sets.digest(BASE / req['card'])
            assert (BASE / req['card']).read_text(encoding='utf-8') in prompt
            assert req['training_status'] == 'NOT_RUN' and req['scope'] == 'DATA_PREPARATION_ONLY'
            assert str(folder) == req['output_directory']
            prepared[run] = (folder, req)
            return req

        normal = [('DOC-CS-NULL', 'DOC-CS-01', 'base', 3, 'DOC'),
                  ('TR07-OMISSION', 'TR07', 'ru-en', 3, 'TR'),
                  ('OR-AUTO-TERMINAL', 'OR-AUTO-01', 'base', 3, 'AUTO')]
        for params in normal:
            check('prepare_' + params[4], lambda params=params: {'families': len(prepare(*params)['family_ids'])})

        def conditional():
            req = prepare('TR10-CONDITIONAL', 'TR10', 'ru-en', 10, 'COND')
            assert req['pairs_per_family'] == 2 and len(req['family_ids']) == 5
            assert len(set(req['family_ids'])) == 5
            return {'rows': 10, 'families': 5, 'pairs_per_family': 2, 'next_start': req['next_start']}

        def routing():
            req = prepare('OR-ROUTE-INTENT', 'OR-ROUTE-01', 'base', 21, 'ROUTE')
            assert len(req['route_class_counts']) == 21 and set(req['route_class_counts'].values()) == {1}
            assert len(req['family_ids']) == 21
            return {'rows': 21, 'classes': 21, 'families': 21}

        check('prepare_conditional_10_rows_5_families', conditional)
        check('prepare_routing_21_classes', routing)
        check('reject_conditional_odd_count', lambda: refused(lambda: prepare('TR10-CONDITIONAL', 'TR10', 'ru-en', 9, 'ODD'), 'divisible'))
        check('reject_routing_incomplete_class_batch', lambda: refused(lambda: prepare('OR-ROUTE-INTENT', 'OR-ROUTE-01', 'base', 20, 'BADROUTE'), '21 or 42'))
        check('reject_zero_count', lambda: refused(lambda: prepare('DOC-CS-NULL', 'DOC-CS-01', 'base', 0, 'ZERO'), 'count 1..50'))

        def no_overwrite():
            folder, _ = prepared['DOC']
            before = {p.name: sets.digest(p) for p in folder.iterdir()}
            refused(lambda: prepare('DOC-CS-NULL', 'DOC-CS-01', 'base', 3, 'DOC'), 'Run already exists')
            assert before == {p.name: sets.digest(p) for p in folder.iterdir()}
            return 'Existing request/prompt/state hashes unchanged'

        check('existing_run_not_overwritten', no_overwrite)

        def fixture_rows(req):
            result = []
            labels = list(req.get('route_class_counts', {}))
            for n, family in enumerate(req['family_ids']):
                for k in range(req.get('pairs_per_family', 1)):
                    cls = ['ambiguous', 'resolved'][k] if req.get('pairs_per_family') == 2 else None
                    chosen = 'DATA_ONLY chosen ' + str(n) + ':' + str(k)
                    if labels:
                        label = labels[n % len(labels)]
                        chosen = json.dumps({'action': 'clarify' if label == 'clarify' else 'route', 'domain_id': None if label == 'clarify' else label})
                    meta = {'learning_objective_id': req['objective_id'], 'origin_family_id': family, 'leakage_group_ids': [family], 'test_fixture': 'DATA_ONLY'}
                    if cls: meta['clarification_class'] = cls
                    result.append({'schema_version': 'xiigen.dpo.v1', 'pair_id': req['run_id'] + '.' + str(n) + '.' + str(k),
                                   'domain_id': req['domain_id'], 'topic_id': req['topic_id'], 'profile_id': req['profile_id'],
                                   'family_id': family, 'split': req['split'], 'prompt': 'DATA_ONLY fixture ' + req['run_id'] + ':' + str(n) + ':' + str(k),
                                   'chosen': chosen, 'rejected': 'DATA_ONLY different ' + str(n) + ':' + str(k),
                                   'error_tag': 'data_integrity_fixture', 'rationale': 'Admission test only, not semantic evidence.',
                                   'source_refs': req['sources'], 'provenance': {'kind': 'DATA_ONLY_TEST_FIXTURE', 'generator_model': 'NO_MODEL_CALL'}, 'metadata': meta})
            return result

        def conditional_balance():
            req = prepared['COND'][1]; rows = fixture_rows(req)
            assert not sets.validate_rows(rows, request=req)
            rows[1]['metadata']['clarification_class'] = 'ambiguous'
            errors = sets.validate_rows(rows, request=req)
            assert any('CONDITIONAL_FAMILY_CLASS_BALANCE' in x for x in errors)
            return 'Valid 5x2 admitted structurally; missing resolved class refused'

        def route_balance():
            req = prepared['ROUTE'][1]; rows = fixture_rows(req)
            assert not sets.validate_rows(rows, request=req)
            rows[1]['chosen'] = rows[0]['chosen']
            assert 'ROUTE_CLASS_BALANCE_MISMATCH' in sets.validate_rows(rows, request=req)
            return 'All 21 classes admitted structurally; duplicate target replacing a class refused'

        check('conditional_class_balance', conditional_balance)
        check('routing_class_balance', route_balance)

        def heldout_guard():
            req = copy.deepcopy(prepared['DOC'][1]); rows = fixture_rows(req)
            public = sets.objectives()['DOC-CS-NULL']['examples'][0]
            rows[0].update(split='exam', family_id=public['family_id'])
            rows[0]['metadata'].update(origin_family_id=public['family_id'], leakage_group_ids=[public['family_id']])
            assert any('PUBLIC_REFERENCE_IN_HELD_OUT' in x for x in sets.validate_rows(rows))
            rows[0]['family_id'] = 'unrelated-id'; rows[0]['metadata'].update(origin_family_id='unrelated-id', leakage_group_ids=['unrelated-id'])
            rows[0]['prompt'] = public['prompt']
            assert any('PUBLIC_REFERENCE_IN_HELD_OUT' in x for x in sets.validate_rows(rows))
            return 'Known public lineage and exact public prompt both refused in exam'

        check('known_public_heldout_guard', heldout_guard)
        check('duplicate_json_key', lambda: refused(lambda: sets.strict_json('{"prompt":"first","prompt":"second"}'), 'DUPLICATE_JSON_KEY'))
        check('overflow_not_accepted_as_json_infinity', lambda: refused(lambda: sets.strict_json('{"n":1e999}'), 'NON_FINITE_JSON_NUMBER'))

        def literal():
            req = prepared['DOC'][1]; rows = fixture_rows(req)
            rows[0].update(chosen='return "a  b";', rejected='return "a b";')
            assert not sets.validate_rows(rows, request=req)
            return 'Significant literal whitespace remains distinct'

        check('literal_whitespace_preserved', literal)

        def reviewed_pipeline():
            # This exercises review plumbing with or.automation data-only fixtures;
            # no executable/language oracle is claimed or run.
            folder, req = prepared['AUTO']; data = fixture_rows(req)
            draft = folder / 'draft.jsonl'; sets.write(draft, sets.jsonl(data))
            blind = folder / 'blind.jsonl'; quiet(sets.blind, input=str(draft), out=str(blind))
            keypath = folder / 'blind.key.json'; key = sets.obj(keypath)
            review = folder / 'review.jsonl'
            reviews = [{'pair_id': r['pair_id'], 'winner': key['chosen_positions'][r['pair_id']], 'valid': True,
                        'reason': 'DATA_ONLY_TEST_FIXTURE of review shape; not a genuine semantic review.',
                        'checks': {'semantic': True, 'sufficient_context': True, 'single_error': True, 'version': True, 'family': True,
                                   'needs_oracle': False, 'needs_language_review': False}, 'evidence': {}} for r in data]
            sets.write(review, sets.jsonl(reviews))
            rc = quiet(sets.validate, input=str(draft), request=str(folder / 'request.json'), review=str(review), key=str(keypath), allow_reference=False)
            assert rc == 0
            catalog = work / 'catalog.json'; quiet(sets.init_catalog, path=str(catalog))
            audit = work / 'audit.json'; assert quiet(sets.audit, catalog=str(catalog), inputs=[str(draft)], out=str(audit)) == 0
            release = work / 'release' / 'data.jsonl'; quiet(sets.merge, inputs=[str(draft)], out=str(release), audit=str(audit))
            export = work / 'export'; quiet(sets.export, input=str(release), out=str(export))
            payload = sets.rows(export / 'dpo.train.jsonl')
            assert len(payload) == len(data) and all(set(r) == {'prompt', 'chosen', 'rejected'} for r in payload)
            assert payload == [{k: r[k] for k in ['prompt', 'chosen', 'rejected']} for r in data]
            assert sets.obj(export / 'export.json')['native_import_verified'] is False
            return {'reviewed_fixture_rows': len(data), 'lossless_export': True, 'native_import_verified': False}

        check('review_audit_merge_export_lossless', reviewed_pipeline)

        def mixing_guard():
            # Two deliberately constructed immutable-shard-shaped fixtures are
            # used only to exercise compose's objective isolation refusal.
            paths = []; cat = {'releases': []}
            for n, oid in enumerate(['TR07-OMISSION', 'TR07-TRANSPOSE']):
                lesson = sets.objectives()[oid]
                req = dict(prepared['TR'][1], objective_id=oid, run_id='MIX'+str(n), family_ids=['mix'+str(n)], count=1)
                data = fixture_rows(req); path = work / ('mix' + str(n)) / 'data.jsonl'
                sets.write(path, sets.jsonl(data))
                sets.write(path.parent / 'release.json', {'kind': 'immutable_shard', 'status': 'CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY', 'sha256': sets.digest(path)})
                paths.append(str(path)); cat['releases'].append({'path': str(path.resolve()), 'sha256': sets.digest(path)})
            catalog = work / 'mix-catalog.json'; sets.write(catalog, cat)
            reason = refused(lambda: quiet(sets.compose, catalog=str(catalog), inputs=paths, out=str(work / 'mixed' / 'data.jsonl')), 'MIXED_LEARNING_OBJECTIVES')
            assert not (work / 'mixed').exists()
            return reason

        check('compose_refuses_same_topic_different_objectives', mixing_guard)

        def merge_mixing_guard():
            # Synthetic receipt SHAPES prove plumbing only. They deliberately
            # contain DATA_ONLY labels and never leave this temporary folder.
            inputs = []
            for oid, run in [('TR07-OMISSION', 'MOM'), ('TR07-TRANSPOSE', 'MTR')]:
                req = prepare(oid, 'TR07', 'ru-en', 1, run)
                folder = prepared[run][0]; data = fixture_rows(req)
                draft = folder / 'draft.jsonl'; sets.write(draft, sets.jsonl(data))
                blind = folder / 'blind.jsonl'; quiet(sets.blind, input=str(draft), out=str(blind))
                keypath = folder / 'blind.key.json'; key = sets.obj(keypath)
                proof = folder / 'data-only-language-receipt.json'
                sets.write(proof, {'scope': 'DATA_REVIEW_NOT_LEARNING', 'status': 'completed',
                                  'blind_input_sha256': key['blind_sha256'],
                                  'method': 'DATA_ONLY_TEST_FIXTURE: receipt shape, no language model called',
                                  'reviewer': 'DATA_ONLY_TEST_FIXTURE harness',
                                  'rows': {r['pair_id']: {'status': 'passed', 'reason': 'DATA_ONLY admission fixture, not a language judgement.'} for r in data}})
                reviews = [{'pair_id': r['pair_id'], 'winner': key['chosen_positions'][r['pair_id']], 'valid': True,
                            'reason': 'DATA_ONLY_TEST_FIXTURE',
                            'checks': {'semantic': True, 'sufficient_context': True, 'single_error': True, 'version': True, 'family': True,
                                       'needs_oracle': False, 'needs_language_review': False},
                            'evidence': {'language_review': {'status': 'passed', 'path': proof.name, 'sha256': sets.digest(proof)}}} for r in data]
                review = folder / 'review.jsonl'; sets.write(review, sets.jsonl(reviews))
                assert quiet(sets.validate, input=str(draft), request=str(folder / 'request.json'), review=str(review), key=str(keypath), allow_reference=False) == 0
                inputs.append(str(draft))
            catalog = work / 'merge-mix-catalog.json'; quiet(sets.init_catalog, path=str(catalog))
            audit = work / 'merge-mix-audit.json'; assert quiet(sets.audit, catalog=str(catalog), inputs=inputs, out=str(audit)) == 0
            output = work / 'merge-mixed' / 'data.jsonl'
            reason = refused(lambda: quiet(sets.merge, inputs=inputs, out=str(output), audit=str(audit)), 'MIXED_LEARNING_OBJECTIVES')
            assert not output.parent.exists() and not sets.obj(catalog)['releases']
            return reason

        check('merge_refuses_reviewed_different_objectives', merge_mixing_guard)

        def heldout_merge_guard():
            req = copy.deepcopy(prepared['DOC'][1]); data = fixture_rows(req)
            public = sets.objectives()['DOC-CS-NULL']['examples'][0]
            data[0].update(split='exam', family_id=public['family_id'])
            data[0]['metadata'].update(origin_family_id=public['family_id'], leakage_group_ids=[public['family_id']])
            draft = work / 'heldout' / 'draft.jsonl'; sets.write(draft, sets.jsonl(data))
            catalog = work / 'heldout-catalog.json'; quiet(sets.init_catalog, path=str(catalog))
            audit = work / 'heldout-audit.json'
            assert quiet(sets.audit, catalog=str(catalog), inputs=[str(draft)], out=str(audit)) == 1
            assert any('PUBLIC_REFERENCE_IN_HELD_OUT' in e for e in sets.obj(audit)['errors'])
            output = work / 'heldout-release' / 'data.jsonl'
            reason = refused(lambda: quiet(sets.merge, inputs=[str(draft)], out=str(output), audit=str(audit)), 'Corpus audit not passed')
            assert not output.parent.exists() and not sets.obj(catalog)['releases']
            return reason

        check('known_public_heldout_cannot_merge', heldout_merge_guard)

    report['passed'] = all(x['passed'] for x in report['checks'])
    report['counts'] = {'checks': len(report['checks']), 'passed': sum(x['passed'] for x in report['checks'])}
    rendered = json.dumps(report, ensure_ascii=False, indent=2, allow_nan=False) + '\n'
    if args.out:
        target = Path(args.out); target.parent.mkdir(parents=True, exist_ok=True)
        with target.open('x', encoding='utf-8') as f: f.write(rendered)
    print(rendered)
    return 0 if report['passed'] else 1


if __name__ == '__main__':
    sys.exit(main())
