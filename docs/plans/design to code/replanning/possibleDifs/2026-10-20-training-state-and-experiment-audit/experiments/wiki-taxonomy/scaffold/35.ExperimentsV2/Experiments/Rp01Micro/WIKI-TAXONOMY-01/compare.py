#!/usr/bin/env python3
"""Compare two observed reports. No model, inference or training runs here."""
import json
import sys
from pathlib import Path

if len(sys.argv) != 4:
    raise SystemExit('Usage: compare.py A/report.json B/report.json NEW-comparison.json')
a_path, b_path, out_path = map(Path, sys.argv[1:])
a, b = (json.loads(p.read_text()) for p in (a_path, b_path))
assert a['arm'] == 'A' and b['arm'] == 'B'
assert a['runtimeStatus'] == b['runtimeStatus'] == 'MEASURED_NATIVE_SOURCE_COPY'
assert a['experimentId'] == b['experimentId'] == 'WIKI-TAXONOMY-01'
assert a['coreCommit'] == b['coreCommit'] == '62bcc8f2368840e22d2ea98ef16d9bd526ad9a39'
assert a['fixtureSha256'] == b['fixtureSha256']
assert a['observedTrainingRows'] == b['observedTrainingRows'] == 4
assert a['observedExamRows'] == b['observedExamRows'] == 6
assert a['nonemptyTrainCalls'] == b['nonemptyTrainCalls'] == 1
assert a['rowPresentations'] == b['rowPresentations'] == 4 * 18
ai, bi = a['initialState'], b['initialState']
assert ai['allZero'] and bi['allZero'] and ai['featureCount'] == bi['featureCount'] == 0
assert len(ai['labels']) == 15
assert ai['labels'] == bi['labels'][:15]
assert ai['biases'] == bi['biases'][:15] == [0] * 15
if a['experimentId'] == 'WIKI-TAXONOMY-01':
    assert bi['labels'][15:] == ['SystemBoundaryMapperTraining', 'CrossStackBusinessFlowGraphTraining']
    assert bi['biases'][15:] == [0, 0]
else:
    assert len(bi['labels']) == 15
    assert ai['stateSha256'] == bi['stateSha256']
assert not a['trainedState']['publishAllowed'] and not b['trainedState']['publishAllowed']

metric_directions = {'correctSelectedRate': 'higher', 'wrongSelectedRate': 'lower', 'deferredRate': 'lower'}
rows = []
for name, direction in metric_directions.items():
    av0, av1 = a['before']['metrics'][name], a['after']['metrics'][name]
    bv0, bv1 = b['before']['metrics'][name], b['after']['metrics'][name]
    dd = (bv1 - bv0) - (av1 - av0)
    raw_improved = dd > 0 if direction == 'higher' else dd < 0
    rows.append({'metric': name, 'betterDirection': direction, 'aBefore': av0, 'aAfter': av1,
                 'bBefore': bv0, 'bAfter': bv1, 'aChange': av1-av0, 'bChange': bv1-bv0,
                 'differenceOfChanges': dd, 'observedDirectionalChange': raw_improved,
                 'candidateMetricImproved': raw_improved if name != 'deferredRate' else None,
                 'diagnosticsOnly': name == 'deferredRate',
                 'conditionalBenefit': 'Assess together with correct/wrong selected per domain; no automatic useful-success label for defer alone.' if name == 'deferredRate' else None,
                 'limitation': 'A lower deferral rate counts as useful only alongside correct selected and wrong selected, never alone if wrong decisions merely replace safe deferrals.' if name == 'deferredRate' else ''})
resource_deltas = []
for section, name, scope in [
    ('costs', 'nonemptyTrainWallMilliseconds', 'Actual nonempty native Train wall time; single-run JIT/timer noise.'),
    ('costs', 'nonemptyTrainCpuMilliseconds', 'Actual nonempty native Train process CPU; short-run resolution/noise.'),
    ('costs', 'nonemptyTrainAllocatedBytesCurrentThread', 'Actual nonempty native Train allocations on the synchronous current thread.'),
    ('costs', 'wholeProcessPeakWorkingSetBytes', 'Whole-process peak through learning/evaluation; not a phase-only training peak.'),
    ('trainedState', 'snapshotBytes', 'Serialized after-training native snapshot size, including label/gate metadata.'),
    ('trainedState', 'parameterCount', 'After-training learned-state coefficient count; compare beside capacity and useful selected quality, not as quality by itself.')
]:
    av, bv = a[section][name], b[section][name]
    resource_deltas.append({'metric': name, 'a': av, 'b': bv, 'bMinusA': bv-av,
                            'betterDirection': 'lower', 'candidateMetricImproved': bv < av,
                            'candidateMetricRegressed': bv > av, 'scope': scope,
                            'interpretation': 'Observed local effect only; improvement on this resource metric does not require an accuracy gain, and every other metric remains visible.'})
groups = []
for ag, bg in zip(a['groups'], b['groups']):
    assert ag['group'] == bg['group']
    groups.append({'group': ag['group'], 'aBefore': ag['before'], 'aAfter': ag['after'],
                   'bBefore': bg['before'], 'bAfter': bg['after'],
                   'differenceOfChanges': {name: (bg['after'][name]-bg['before'][name])-(ag['after'][name]-ag['before'][name]) for name in metric_directions}})
result = {'experimentId': a['experimentId'], 'status': 'MEASURED_COMPARISON', 'reports': [str(a_path.resolve()), str(b_path.resolve())],
          'fixtureSha256': a['fixtureSha256'], 'initialShared15CoordinatesEqualAndZero': True,
          'metrics': rows, 'groups': groups, 'resourceDeltas': resource_deltas,
          'costsA': a['costs'], 'costsB': b['costs'],
          'updatesA': a['modelUpdateCount'], 'updatesB': b['modelUpdateCount'],
          'stateA': a['trainedState'], 'stateB': b['trainedState'],
          'rule': 'Any measured useful-metric improvement is success for that metric and domain; accuracy increase is not required; show every regression. A single short timing observation is an observation, not stable performance evidence.',
          'publicationOrProductionMergeApproved': False}
if out_path.exists():
    raise SystemExit('Append-only: comparison output already exists.')
out_path.parent.mkdir(parents=True, exist_ok=True)
with out_path.open('x') as f:
    json.dump(result, f, ensure_ascii=False, indent=2)
    f.write('\n')
print(out_path)
