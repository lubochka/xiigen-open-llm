#!/usr/bin/env python3
"""Recount saved P125 records with its frozen accepted strings; no native run.

Usage: python3 recount_historical_panel.py --core-root /path/to/llm_mvp_core
The three source blobs must match the pinned audit commit. This verifies the
saved panel arithmetic, not the experiment's runtime provenance or new semantics.
"""
import argparse
import hashlib
import json
import re
from collections import defaultdict
from pathlib import Path

COMMIT = 'eb05219c27663835ce927c7bf97cf1274c07cddb'
REL = Path('docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01')

def git_blob(data):
    return hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()

def normalize(s):
    return re.sub(r'\s+', ' ', s.lower().replace('ё', 'е').strip()).strip(' .,!?:;\"\'')

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--core-root', type=Path, required=True)
    parser.add_argument('--output', type=Path, default=Path(__file__).with_name('historical_panel_recount.json'))
    args = parser.parse_args()
    manifest = json.loads(Path(__file__).with_name('historical_panel_source_hashes.json').read_text())
    data = {}
    for name, sha in manifest['git_blobs'].items():
        raw = (args.core_root / REL / name).read_bytes()
        assert git_blob(raw) == sha, (name, git_blob(raw), sha)
        data[name] = json.loads(raw)
    raw = data['raw-outputs.json']
    gold = {x['id']: x for x in data['panel-gold.json']['cases']}
    expected = {(x['arm'], x['phase']): x for x in data['score-summary.json']['summary'] if x['bucket'] == 'all'}
    assert len(raw) == 144 and len(gold) == 24
    groups = defaultdict(list)
    for x in raw:
        groups[x['arm'], x['phase']].append(x)
    assert set(groups) == {(a, p) for a in 'ABC' for p in ('before', 'after')}
    inputs = {}
    result = []
    for key, rows in sorted(groups.items()):
        assert len(rows) == 24 and {x['id'] for x in rows} == set(gold)
        by_id = {x['id']: (x['source'], x['inputSha256']) for x in rows}
        if inputs:
            assert by_id == inputs
        else:
            inputs = by_id
        correct, answered, accepted_wrong = [], [], []
        for x in rows:
            is_answered = x['verdict'] == 'TRANSLATED'
            # Match the frozen whole-phrase alternatives; never inspect them in a trainer.
            is_correct = is_answered and normalize(x['actual']) in {normalize(s) for s in gold[x['id']]['accepted']}
            if is_correct: correct.append(x['id'])
            if is_answered: answered.append(x['id'])
            if is_answered and not is_correct: accepted_wrong.append(x['id'])
        e = expected[key]
        assert (len(correct), len(answered), len(accepted_wrong)) == (e['correct'], e['answered'], e['acceptedWrong'])
        result.append({'arm': key[0], 'phase': key[1], 'total': len(rows), 'correct': len(correct),
                       'answered': len(answered), 'accepted_wrong': len(accepted_wrong),
                       'correct_ids': correct, 'accepted_wrong_ids': accepted_wrong})
    output = {'status': 'SAVED_RECORD_RECOUNT_MATCHES_FROZEN_SUMMARY', 'native_rerun': False,
              'new_semantic_judgment': False, 'core_commit': COMMIT, 'source_git_blobs': manifest['git_blobs'],
              'raw_records': len(raw), 'unique_case_ids': len(gold), 'identical_inputs_across_six_states': True,
              'groups': result,
              'limits': 'Recount of saved native outputs only; no native runtime, DLL/PDB replay, fresh training, full provenance audit, or new held-out test.'}
    args.output.write_text(json.dumps(output, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'status': output['status'], 'raw_records': len(raw), 'groups': [(r['arm'], r['phase'], r['correct']) for r in result]}, ensure_ascii=False))

if __name__ == '__main__':
    main()
