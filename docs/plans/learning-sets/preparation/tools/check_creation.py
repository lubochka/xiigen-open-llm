#!/usr/bin/env python3
"""Maintainer-only regression checks for paths and CREATE_ONLY; no model or learning.

All rows are explicitly synthetic tool fixtures in a temporary directory. This
script is not a quality review of a dataset and is not part of generation.
"""
from pathlib import Path, PureWindowsPath
import copy
import json
import os
import shutil
import subprocess
import sys
import tempfile

BASE = Path(__file__).resolve().parents[1]
ROOT = Path(tempfile.mkdtemp(prefix='xiigen creation '))
CHECKS = []
ENV = dict(os.environ, PYTHONDONTWRITEBYTECODE='1', PYTHONUTF8='1')


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def put(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def rows(path, data):
    path.write_text(''.join(json.dumps(r, ensure_ascii=False) + '\n' for r in data), encoding='utf-8')


def check(name, condition, detail=None):
    if not condition:
        raise AssertionError(name + ': ' + str(detail))
    CHECKS.append({'name': name, 'passed': True, 'detail': detail})


def call(script, *args, expected=0):
    result = subprocess.run([sys.executable, '-B', str(script), *map(str, args)],
                            cwd=ROOT, env=ENV, capture_output=True, text=True)
    if result.returncode != expected:
        raise AssertionError(str(args) + '\n' + result.stdout + '\n' + result.stderr)
    return result


def main():
    source_queue = read(BASE / 'execution-queue.json')
    script = BASE / 'tools/batches.py'
    # A relocated package in a directory containing spaces simulates invocation
    # from a different checkout, with no hard-coded developer path available.
    relocated = ROOT / 'Windows-like path with spaces/learning-sets/preparation'
    shutil.copytree(BASE, relocated, ignore=shutil.ignore_patterns('__pycache__', '*.pyc'))
    moved_script = relocated / 'tools/batches.py'
    output = json.loads(call(moved_script, 'prepare', '--stream', 'DOC-CS-NULL/base',
                             '--run', 'DEFAULT').stdout)
    out = Path(output['out'])
    req = read(out / 'request.json')
    check('default-output-follows-script-not-cwd', out == relocated.parent / 'DOC-CS-NULL/base/DEFAULT')
    check('absolute-paths-and-inline-schema', all(Path(req['paths'][k]).is_absolute()
          for k in ['preparation_root', 'learning_sets_root', 'card', 'contract', 'schema', 'draft', 'tools_script'])
          and '"$schema"' in (out / 'prompt.md').read_text(encoding='utf-8'))
    check('all-assigned-sources-local', all(Path(p).is_file() for p in req['paths']['sources'].values()))
    check('prepared-is-not-generated', read(out / 'state.json')['generation_status'] == 'NOT_RUN'
          and not (out / 'draft.jsonl').exists())
    duplicate = call(moved_script, 'prepare', '--stream', req['stream_id'], '--run', 'DEFAULT', expected=2)
    check('existing-run-gives-exact-resume', 'already exists' in duplicate.stderr and 'resume' in duplicate.stderr
          and str(out / 'request.json') in duplicate.stderr)

    data = []
    for i, fid in enumerate(req['family_ids']):
        data.append({'schema_version': 'xiigen.dpo.v1', 'pair_id': 'TOOL-FIXTURE-' + str(i),
            'domain_id': req['domain_id'], 'topic_id': req['topic_id'], 'profile_id': req['profile_id'],
            'family_id': fid, 'split': req['split'],
            'prompt': 'Tool fixture only ' + str(i) + ': C# 10/.NET 6 string F(string? x) => x ?? "f"; F(null)?',
            'chosen': '"f"', 'rejected': 'null', 'error_tag': 'fixture-null-fallback',
            'rationale': 'Tool fixture, not a generated or reviewed training example.',
            'source_refs': req['sources'], 'provenance': {'kind': 'TOOL_FIXTURE_NOT_PROVIDER_OUTPUT',
            'generator_model': 'TEST_FIXTURE'}, 'metadata': {'origin_family_id': fid,
            'leakage_group_ids': [fid], 'learning_objective_id': req['objective_id'],
            'simulation': True, 'measured_training_result': False, 'review_status': 'not_requested'}})
    draft = out / 'draft.jsonl'
    rows(draft, data[:-1])
    incomplete = json.loads(call(moved_script, 'finish', '--request', out / 'request.json', expected=1).stdout)
    check('incomplete-draft-does-not-complete', incomplete['status'] == 'DRAFT_INCOMPLETE'
          and read(out / 'state.json')['status'] == 'DRAFT_INCOMPLETE')
    rows(draft, data)
    complete = json.loads(call(moved_script, 'finish', '--request', out / 'request.json', '--model', 'TEST_FIXTURE').stdout)
    check('finish-writes-data-status-without-review', complete['status'] == 'DRAFT_CREATED'
          and complete['semantic_quality'] == 'NOT_ESTABLISHED' and complete['training_status'] == 'NOT_RUN'
          and complete['next_stream_id'] == 'CODE-CS-NULL/base' and Path(complete['report_path']).is_file())
    check('no-later-stage-files-created', not any('blind' in p.name or p.name.startswith(('review.', 'accepted.')) for p in out.iterdir()))
    before_files = sorted(p.name for p in out.iterdir())
    call(moved_script, 'finish', '--request', out / 'request.json')
    check('repeat-finish-does-not-produce-report-loop', before_files == sorted(p.name for p in out.iterdir()))
    resumed = json.loads(call(moved_script, 'resume', '--request', out / 'request.json').stdout)
    check('completed-run-resumes-at-next', resumed['action'] == 'NEXT_STREAM')
    state = read(out / 'state.json'); state['process_handle'] = 'FAKE_TEST_HANDLE'; state['process_status'] = 'running'; put(out / 'state.json', state)
    waiting = json.loads(call(moved_script, 'resume', '--request', out / 'request.json').stdout)
    check('live-handle-wins-over-full-draft', waiting['action'] == 'WAIT_OR_RESOLVE_EXISTING_PROCESS')
    refused = call(moved_script, 'finish', '--request', out / 'request.json', expected=2)
    check('finish-does-not-invent-process-completion', 'terminal evidence' in refused.stderr)
    state['process_status'] = 'completed'; put(out / 'state.json', state)
    for label, mutation in [
        ('model-type', lambda r: r['provenance'].update(generator_model=123)),
        ('identity-type', lambda r: r['provenance'].update(model_identity_observed=1)),
        ('measured-claim', lambda r: r['metadata'].update(measured_training_result=True)),
        ('simulation-type', lambda r: r['metadata'].update(simulation='yes'))]:
        bad = copy.deepcopy(data); mutation(bad[0]); rows(draft, bad)
        result = json.loads(call(moved_script, 'finish', '--request', out / 'request.json', expected=1).stdout)
        check('schema-refuses-' + label, any('CANDIDATE_SCHEMA_' in e for e in result['errors']))
    draft.write_text('{invalid', encoding='utf-8')
    resumed = json.loads(call(moved_script, 'resume', '--request', out / 'request.json').stdout)
    check('malformed-draft-not-completed', resumed['action'] == 'CONTINUE_ASSIGNED_DRAFT' and resumed['draft_parse_error'])
    rows(draft, data)
    call(moved_script, 'finish', '--request', out / 'request.json')
    check('old-reports-and-states-preserved', (out / 'report.creation.md').is_file()
          and (out / 'report.creation.v002.md').is_file() and (out / 'state.previous.json').is_file())
    copied_run = ROOT / 'copied complete run'
    shutil.copytree(out, copied_run)
    copied = json.loads(call(moved_script, 'resume', '--request', copied_run / 'request.json').stdout)
    check('copied-run-never-reads-original-draft', Path(copied['draft']).parent == copied_run
          and str(copied_run / 'draft.jsonl') in copied['finish_command'])

    # A header-only source must not repeat the failure in the attached session.
    source_path = Path(next(iter(req['paths']['sources'].values())))
    original_source = source_path.read_text(encoding='utf-8')
    source_path.write_text('# Fake placeholder\n', encoding='utf-8')
    result = call(moved_script, 'prepare', '--stream', req['stream_id'], '--run', 'MISSING', expected=2)
    check('placeholder-source-refused-with-exact-path', str(source_path) in result.stderr and 'saved evidence excerpt' in result.stderr)
    source_path.write_text(original_source, encoding='utf-8')

    stream_results = []
    forbidden_instructions = ['CODEX проверяет все 10', 'далее 64 и 256 после review',
        'выбирает исходный материал, проверяет пары и решает об их допуске',
        'Записывай draft.jsonl, review.json', 'Финальный выбор и оценка качества — независимый CODEX']
    for stream in source_queue['streams']:
        result = json.loads(call(script, 'prepare', '--stream', stream['stream_id'], '--run', 'ALL',
                                 '--work-root', ROOT / 'all streams').stdout)
        target = Path(result['out']); current = read(target / 'request.json')
        prompt = (target / 'prompt.md').read_text(encoding='utf-8')
        operative = prompt.split('## Материал ', 1)[0]
        if any(term in operative for term in forbidden_instructions):
            raise AssertionError('Active old instruction: ' + stream['stream_id'])
        if current['mode'] != 'CREATE_ONLY' or current['next_stream_id'] != stream['next_stream_id']:
            raise AssertionError('Mode or queue mismatch: ' + stream['stream_id'])
        if not all(Path(path).is_file() for path in current['paths']['sources'].values()):
            raise AssertionError('Missing assigned source: ' + stream['stream_id'])
        commands = (target / 'commands.md').read_text(encoding='utf-8')
        if any(' ' + action + ' --' in commands for action in ['blind', 'accept', 'export']):
            raise AssertionError('Automatic later-stage command: ' + stream['stream_id'])
        stream_results.append({'stream_id': stream['stream_id'], 'count': current['count'],
                               'families': len(current['family_ids']), 'prompt_chars': result['prompt_chars']})
    check('all-streams-prepare-in-create-only', len(stream_results) == len(source_queue['streams']), len(stream_results))
    windows_max = max(len(str(PureWindowsPath(s['first_output_directory']) / 'report.creation.v999.md'))
                      for s in source_queue['streams'])
    check('designated-windows-output-paths-fit-260', windows_max < 260, windows_max)
    cards = [BASE / t['card'] for t in read(BASE / 'index.json')['topics']]
    cards += [BASE / o['card'] for o in read(BASE / 'curriculum/index.json')['objectives']]
    check('every-card-has-current-mode', len(cards) == source_queue['counts']['physical_cards']
          and all('<!-- CREATE-ONLY-START -->' in p.read_text(encoding='utf-8') for p in cards), len(cards))
    refresh = json.loads(call(BASE / 'tools/refresh_indexes.py', '--check').stdout)
    check('indexes-regenerate-without-diff', refresh['changed_files'] == 0)
    report = {'scope': 'PREPARATION_TOOL_CHECKS_ONLY', 'model_calls': 0, 'training_runs': 0,
              'semantic_reviews': 0, 'real_generated_training_pairs': 0, 'windows_execution': 'NOT_RUN',
              'fixture_note': 'All draft rows are isolated TOOL_FIXTURE records, never a teacher-generated corpus.',
              'passed': len(CHECKS), 'failed': 0, 'checks': CHECKS, 'streams': stream_results}
    put(ROOT / 'summary.json', report)
    print(json.dumps({'passed': len(CHECKS), 'failed': 0, 'streams': len(stream_results),
                      'root': str(ROOT), 'report': str(ROOT / 'summary.json')}, indent=2))


if __name__ == '__main__':
    main()
