#!/usr/bin/env python3
"""Merged offline regression checks for DPO preparation input integrity.

Combines the incoming curriculum/evidence checks with CODEX authority, blind
reconstruction, explicit training exclusion and new-reference leakage checks.

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
import shutil
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
             'verification/check-preparation-v2.py', 'verification/check-preparation-merged.py']
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

    def fixture_authority_args(folder, draft, blind, review):
        """Synthetic declarations for admission shape tests; no provider is called."""
        paths = {}
        for kind, filename in [('selection', 'selection.fixture.json'), ('review', 'review.receipt.fixture.json')]:
            kwargs = {} if kind == 'selection' else {'input_path': draft, 'review_path': review, 'blind_path': blind}
            value = sets.authority_template(kind, folder / 'request.json', **kwargs)
            value.update(observed_model='DATA_ONLY_TEST_FIXTURE_NO_PROVIDER_CALL', observed_profile=sets.CODEX_PROFILE,
                         profile_verified=True, evidence_ref='fixture://not-real-CODEX',
                         decision='SELECTED' if kind == 'selection' else 'REVIEW_COMPLETED')
            target = folder / filename
            sets.write(target, value, exclusive=False)
            paths['selection' if kind == 'selection' else 'review_receipt'] = str(target)
        return dict(paths, blind=str(blind))

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
                                   'source_refs': req['sources'], 'provenance': {'kind': 'DATA_ONLY_TEST_FIXTURE', 'generator_model': 'NO_MODEL_CALL'}, 'metadata': dict(meta, training_allowed=True, fixture_only=True)})
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
            data[0].update(prompt='DATA_ONLY literal and Unicode preservation', chosen='return \"a  b\";\n// e\u0301', rejected='return \"a b\";\n// é')
            draft = folder / 'draft.jsonl'; sets.write(draft, sets.jsonl(data))
            blind = folder / 'blind.jsonl'; quiet(sets.blind, input=str(draft), out=str(blind))
            keypath = folder / 'blind.key.json'; key = sets.obj(keypath)
            review = folder / 'review.jsonl'
            reviews = [{'pair_id': r['pair_id'], 'winner': key['chosen_positions'][r['pair_id']], 'valid': True,
                        'reason': 'DATA_ONLY_TEST_FIXTURE of review shape; not a genuine semantic review.',
                        'checks': {'semantic': True, 'sufficient_context': True, 'single_error': True, 'version': True, 'family': True,
                                   'needs_oracle': False, 'needs_language_review': False}, 'evidence': {}} for r in data]
            sets.write(review, sets.jsonl(reviews))
            rc = quiet(sets.validate, input=str(draft), request=str(folder / 'request.json'), review=str(review), key=str(keypath), allow_reference=False, **fixture_authority_args(folder, draft, blind, review))
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
                sets.write(path.parent / 'release.json', {'kind': 'immutable_shard', 'status': 'CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY', 'acceptance_contract': sets.ACCEPTANCE_CONTRACT, 'sha256': sets.digest(path)})
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
                assert quiet(sets.validate, input=str(draft), request=str(folder / 'request.json'), review=str(review), key=str(keypath), allow_reference=False, **fixture_authority_args(folder, draft, blind, review)) == 0
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

        def save(path, value):
            sets.write(path, value, exclusive=False)

        def save_rows(path, value):
            save(path, sets.jsonl(value))

        def make_admission_fixture(topic, profile, run, mode='language_review'):
            # No --objective attribute: legacy direct prepare callers remain valid.
            quiet(sets.prepare, topic=topic, profile=profile, run=run, count=1, start=0,
                  split='train', work_root=str(work), rag_root=args.rag_root, family_plan=None)
            folder = work / topic / profile / run
            req = sets.obj(folder / 'request.json')
            assert req['schema_version'] == 'xiigen.request.v1'
            assert req['selection_authority'] == req['quality_authority'] == {'actor': 'CODEX', 'required_profile': sets.CODEX_PROFILE}
            pending = sets.obj(folder / 'selection.template.json')
            assert pending['observed_profile'] is None and pending['profile_verified'] is False and pending['decision'] == 'PENDING'
            family = req['family_ids'][0]
            data = [{'schema_version': 'xiigen.dpo.v1', 'pair_id': run+'.FIXTURE', 'domain_id': req['domain_id'],
                     'topic_id': topic, 'profile_id': profile, 'family_id': family, 'split': 'train',
                     'prompt': 'DATA_ONLY admission fixture '+run, 'chosen': 'DATA_ONLY preferred fixture',
                     'rejected': 'DATA_ONLY alternate fixture', 'error_tag': 'fixture',
                     'rationale': 'Synthetic integrity fixture, no semantic review or learning claim.',
                     'source_refs': req['sources'], 'provenance': {'kind': 'DATA_ONLY_TEST_FIXTURE', 'generator_model': 'NO_MODEL_CALL'},
                     'metadata': {'origin_family_id': family, 'leakage_group_ids': [family], 'training_allowed': True, 'fixture_only': True}}]
            draft = folder / 'draft.jsonl'; save_rows(draft, data)
            blind = folder / 'blind.jsonl'; quiet(sets.blind, input=str(draft), out=str(blind))
            key = sets.obj(folder / 'blind.key.json')
            proof = {'scope': 'DATA_REVIEW_NOT_LEARNING', 'status': 'completed', 'blind_input_sha256': sets.digest(blind),
                     'method': 'DATA_ONLY fixture of evidence binding; no oracle/model executed',
                     'reviewer': 'DATA_ONLY_TEST_FIXTURE',
                     'rows': {data[0]['pair_id']: {'status': 'passed', 'reason': 'Receipt-shape fixture only.'}}}
            kind = 'language_review' if topic.startswith('TR') else 'oracle'
            if mode == 'source_grounded':
                sid = sets.source_id(req['sources'][0]); source = Path(args.rag_root) / 'rag/sources' / (sid+'.md')
                local = folder / 'pinned-excerpt.md'; local.write_bytes(source.read_bytes())
                proof.update(verification_mode='source_grounded', preference_basis='source_contract',
                             source_evidence=[{'source_id': sid, 'path': local.name, 'sha256': sets.digest(local),
                                               'excerpt_lines': [1, 1], 'claim': 'DATA_ONLY citation binding fixture, not a substantive assessment.'}])
            elif mode == 'execution':
                raw = folder / 'mock.native.stdout'; raw.write_text('DATA_ONLY_TEST_FIXTURE: no native command was executed.\n', encoding='utf-8')
                proof.update(verification_mode='execution', execution={'command': 'DATA_ONLY_NO_COMMAND_EXECUTED',
                             'environment': 'synthetic admission fixture', 'exit_code': 0,
                             'artifacts': [{'path': raw.name, 'sha256': sets.digest(raw)}]})
            proof_path = folder / 'proof.fixture.json'; save(proof_path, proof)
            review = folder / 'review.jsonl'
            save_rows(review, [{'pair_id': data[0]['pair_id'], 'winner': key['chosen_positions'][data[0]['pair_id']], 'valid': True,
                                'reason': 'DATA_ONLY_TEST_FIXTURE, not a CODEX semantic decision.',
                                'checks': {'semantic': True, 'sufficient_context': True, 'single_error': True, 'version': True, 'family': True,
                                           'needs_oracle': False, 'needs_language_review': False},
                                'evidence': {kind: {'status': 'passed', 'path': proof_path.name, 'sha256': sets.digest(proof_path)}}}])
            fixture_authority_args(folder, draft, blind, review)
            return folder

        def validate_fixture(folder):
            code = quiet(sets.validate, input=str(folder/'draft.jsonl'), request=str(folder/'request.json'),
                         review=str(folder/'review.jsonl'), key=str(folder/'blind.key.json'), blind=str(folder/'blind.jsonl'),
                         selection=str(folder/'selection.fixture.json'), review_receipt=str(folder/'review.receipt.fixture.json'),
                         allow_reference=False)
            return code, sets.obj(folder/'validation.json')

        def refresh_fixture_review(folder):
            review = sets.rows(folder/'review.jsonl')
            for row in review:
                for envelope in row.get('evidence', {}).values():
                    path = folder / envelope['path']
                    if path.is_file(): envelope['sha256'] = sets.digest(path)
            save_rows(folder/'review.jsonl', review)
            receipt = sets.obj(folder/'review.receipt.fixture.json')
            for kind, filename in [('input', 'draft.jsonl'), ('review', 'review.jsonl'), ('blind', 'blind.jsonl')]:
                receipt[kind+'_sha256'] = sets.digest(folder/filename)
            save(folder/'review.receipt.fixture.json', receipt)

        fixture_bases = {}

        def legacy_authority_pipeline():
            folder = make_admission_fixture('TR01', 'ru-en', 'AUTHORITY')
            fixture_bases['tr'] = folder
            rc, result = validate_fixture(folder)
            assert rc == 0 and result['semantic_review_accepted'] and result['provider_identity_authenticated'] is False
            catalog = work/'authority-catalog.json'; quiet(sets.init_catalog, path=str(catalog))
            audit = work/'authority-audit.json'; assert quiet(sets.audit, catalog=str(catalog), inputs=[str(folder/'draft.jsonl')], out=str(audit)) == 0
            shard = work/'authority-shard/data.jsonl'; quiet(sets.merge, inputs=[str(folder/'draft.jsonl')], out=str(shard), audit=str(audit))
            snapshot = work/'authority-snapshot/data.jsonl'; quiet(sets.compose, catalog=str(catalog), inputs=[str(shard)], out=str(snapshot))
            quiet(sets.export, input=str(snapshot), out=str(work/'authority-export'))
            assert sets.rows(work/'authority-export/dpo.train.jsonl') == [{k:r[k] for k in ['prompt','chosen','rejected']} for r in sets.rows(folder/'draft.jsonl')]
            return {'legacy_prepare_without_objective': True, 'provider_identity_authenticated': False, 'fixture_only': True}

        check('authority_positive_mock_prepare_blind_validate_audit_merge_compose_export', legacy_authority_pipeline)

        def negative_fixture(name, mutate, fragment, base='tr'):
            folder = work/'negative'/name; folder.parent.mkdir(exist_ok=True)
            shutil.copytree(fixture_bases[base], folder)
            mutate(folder)
            code, result = validate_fixture(folder)
            assert code == 1 and not result['semantic_review_accepted'], result
            assert any(fragment in error for error in result['errors']), result['errors']
            return {'expected': fragment, 'errors': result['errors']}

        def mutate_json(folder, filename, change):
            value = sets.obj(folder/filename); change(value); save(folder/filename, value)

        check('authority_wrong_actor', lambda: negative_fixture('actor', lambda f: mutate_json(f,'selection.fixture.json',lambda x:x.update(actor='ChineseGenerator')), 'WRONG_AUTHORITY'))
        check('authority_wrong_observed_profile', lambda: negative_fixture('profile', lambda f: mutate_json(f,'selection.fixture.json',lambda x:x.update(observed_profile='some other profile')), 'WRONG_OBSERVED_PROFILE'))

        def swap_blind(folder):
            data = sets.rows(folder/'blind.jsonl'); data[0]['A'],data[0]['B'] = data[0]['B'],data[0]['A']; save_rows(folder/'blind.jsonl',data)
            mutate_json(folder,'blind.key.json',lambda x:x.update(blind_sha256=sets.digest(folder/'blind.jsonl')))
            mutate_json(folder,'proof.fixture.json',lambda x:x.update(blind_input_sha256=sets.digest(folder/'blind.jsonl')))
            refresh_fixture_review(folder)

        def swap_key(folder, third=False):
            key = sets.obj(folder/'blind.key.json'); pair = next(iter(key['chosen_positions']))
            key['chosen_positions'][pair] = 'C' if third else ('B' if key['chosen_positions'][pair]=='A' else 'A')
            save(folder/'blind.key.json',key)
            review = sets.rows(folder/'review.jsonl'); review[0]['winner'] = key['chosen_positions'][pair]; save_rows(folder/'review.jsonl',review)
            refresh_fixture_review(folder)

        def changed_input(folder, excluded=False):
            data = sets.rows(folder/'draft.jsonl')
            if excluded: data[0]['metadata']['training_allowed'] = False
            else: data[0]['chosen'] = 'DATA_ONLY changed after review'
            save_rows(folder/'draft.jsonl',data)
            if excluded:
                mutate_json(folder,'blind.key.json',lambda x:x.update(input_sha256=sets.digest(folder/'draft.jsonl')))
                refresh_fixture_review(folder)

        check('authority_changed_blind_even_with_updated_hashes', lambda: negative_fixture('blind',swap_blind,'BLIND_PACKET_RECONSTRUCTION_MISMATCH'))
        check('authority_changed_key_even_with_matching_review', lambda: negative_fixture('key',swap_key,'BLIND_PACKET_RECONSTRUCTION_MISMATCH'))
        check('authority_changed_input_after_review', lambda: negative_fixture('input',changed_input,'INPUT_HASH_MISMATCH'))
        check('authority_nonexistent_candidate_C', lambda: negative_fixture('third',lambda f:swap_key(f,True),'BLIND_KEY_INVALID_POSITIONS'))
        check('authority_explicit_training_exclusion', lambda: negative_fixture('excluded',lambda f:changed_input(f,True),'TRAINING_EXPLICITLY_DISALLOWED'))

        def reference_non_regression():
            row = sets.rows(fixture_bases['tr']/'draft.jsonl')[0]
            row['split'] = 'reference'; row['metadata']['training_allowed'] = False
            row['provenance']['generator_model'] = 'reference_authoring_not_a_kiro_run'
            assert not sets.validate_rows([row],allow_reference=True)
            assert any('REFERENCE_NOT_TRAINING' in x for x in sets.validate_rows([row]))
            return 'Explicit reference exclusion remains allowed for structural inspection only.'

        check('authority_reference_training_false_allowed_only_with_allow_reference',reference_non_regression)

        def modify_review(folder, change):
            data = sets.rows(folder/'review.jsonl'); change(data[0]); save_rows(folder/'review.jsonl',data); refresh_fixture_review(folder)

        check('review_missing_completion_flag',lambda:negative_fixture('completion',lambda f:modify_review(f,lambda r:r['checks'].pop('needs_oracle')),'EXPLICIT_REVIEW_COMPLETION_REQUIRED'))
        check('review_missing_language_evidence',lambda:negative_fixture('missing-proof',lambda f:modify_review(f,lambda r:r.update(evidence={})),'LANGUAGE_REVIEW_EVIDENCE_REQUIRED'))

        def modify_proof(folder, change):
            mutate_json(folder,'proof.fixture.json',change); refresh_fixture_review(folder)

        check('review_wrong_blind_evidence_binding',lambda:negative_fixture('proof-binding',lambda f:modify_proof(f,lambda x:x.update(blind_input_sha256='0'*64)),'LANGUAGE_REVIEW_EVIDENCE_BINDING_MISMATCH'))

        def stale_proof_merge():
            folder = work/'stale-proof'; shutil.copytree(fixture_bases['tr'],folder)
            assert validate_fixture(folder)[0] == 0
            catalog = work/'stale-catalog.json'; quiet(sets.init_catalog,path=str(catalog))
            audit = work/'stale-audit.json'; assert quiet(sets.audit,catalog=str(catalog),inputs=[str(folder/'draft.jsonl')],out=str(audit)) == 0
            mutate_json(folder,'proof.fixture.json',lambda x:x.update(method='CHANGED AFTER VALIDATION'))
            return refused(lambda:quiet(sets.merge,inputs=[str(folder/'draft.jsonl')],out=str(work/'stale-release/data.jsonl'),audit=str(audit)),'Changed review evidence')

        check('review_evidence_change_blocks_merge',stale_proof_merge)

        def docs_source_positive():
            folder = make_admission_fixture('DOC-CS-01','base','DOCSOURCE','source_grounded');fixture_bases['docs']=folder
            assert validate_fixture(folder)[0] == 0
            # Immutable URL and RAG ID resolve to the same approved source.
            proof = sets.obj(folder/'proof.fixture.json'); sid=proof['source_evidence'][0]['source_id']
            proof['source_evidence'][0]['source_id']=sets.source_catalog()[sid]['source_url'];save(folder/'proof.fixture.json',proof)
            refresh_fixture_review(folder);assert validate_fixture(folder)[0] == 0
            return {'source_grounded_shape_only':True,'native_oracle_runs':0,'id_and_immutable_url_supported':True}

        check('docs_source_grounded_pinned_evidence',docs_source_positive)
        check('docs_source_grounded_requires_contract_basis',lambda:negative_fixture('docs-basis',lambda f:modify_proof(f,lambda x:x.update(preference_basis='runtime_behavior')),'SOURCE_CONTRACT_BASIS_REQUIRED','docs'))
        check('docs_source_grounded_invalid_excerpt_lines',lambda:negative_fixture('docs-lines',lambda f:modify_proof(f,lambda x:x['source_evidence'][0].update(excerpt_lines=[0,1])),'DOCUMENTATION_EXCERPT_LINES_INVALID','docs'))

        def changed_pinned_source(folder):
            source=folder/'pinned-excerpt.md';source.write_text(source.read_text(encoding='utf-8')+'\nCHANGED FIXTURE\n',encoding='utf-8')
            modify_proof(folder,lambda x:x['source_evidence'][0].update(sha256=sets.digest(source)))

        check('docs_source_hash_must_match_pinned_catalog',lambda:negative_fixture('docs-source',changed_pinned_source,'DOCUMENTATION_SOURCE_HASH_MISMATCH','docs'))

        def code_execution_positive():
            folder=make_admission_fixture('CODE-CS-01','base','CODEEXEC','execution');fixture_bases['code']=folder
            assert validate_fixture(folder)[0] == 0
            return {'execution_receipt_shape_only':True,'native_oracle_runs':0}

        check('code_execution_artifact_shape_binding',code_execution_positive)
        check('code_requires_execution_evidence',lambda:negative_fixture('code-exec',lambda f:modify_proof(f,lambda x:x.pop('execution')),'ORACLE_EXECUTION_EVIDENCE_REQUIRED','code'))

        def code_cannot_use_source_mode():
            folder=make_admission_fixture('CODE-CS-01','base','CODESOURCE','source_grounded')
            code,result=validate_fixture(folder)
            assert code==1 and any('SOURCE_GROUNDED_DOCUMENTATION_ONLY' in x for x in result['errors'])
            return result['errors']

        check('code_source_grounded_mode_refused',code_cannot_use_source_mode)

        def v2_reference_guard():
            public=sets.rows(BASE/'domains/translation/reference-examples-v2.jsonl')[0]
            row=copy.deepcopy(public);row['split']='exam';row['prompt']='DATA_ONLY distinct exam prompt';row['metadata']['training_allowed']=True
            row['provenance']={'kind':'DATA_ONLY_TEST_FIXTURE','generator_model':'NO_MODEL_CALL'}
            assert any('PUBLIC_REFERENCE_IN_HELD_OUT' in x for x in sets.validate_rows([row]))
            row['family_id']='DATA_ONLY.UNRELATED';row['metadata'].update(origin_family_id='DATA_ONLY.UNRELATED',leakage_group_ids=['DATA_ONLY.UNRELATED'])
            row['prompt']=public['prompt']
            assert any('PUBLIC_REFERENCE_IN_HELD_OUT' in x for x in sets.validate_rows([row]))
            return 'Both lineage and exact prompt guards include reference-examples-v2.jsonl.'

        check('new_v2_reference_lineage_and_prompt_not_exam',v2_reference_guard)

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
