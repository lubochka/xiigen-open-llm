"""Recompute retained observations only. Never imports or runs XIIGEN learning code."""
from pathlib import Path
import argparse, hashlib, json, re, statistics

parser = argparse.ArgumentParser()
parser.add_argument('--input-root', type=Path, required=True)
parser.add_argument('--output', type=Path, required=True)
args = parser.parse_args()
root = args.input_root
sources = []

def read(path):
    f = root / path
    data = f.read_bytes()
    sources.append({'path': path, 'local_sha256': hashlib.sha256(data).hexdigest()})
    return json.loads(data)

def norm(text):
    text = re.sub(r'\s+', ' ', text.lower().replace('ё', 'е')).strip()
    return re.sub(r'^[\W_]+|[\W_]+$', '', text)

def count_panel(raw, scored, gold):
    cases = {x['id']: x for x in gold['cases']}
    by_key = {(x.get('arm'), x.get('phase'), x['id']): x for x in scored}
    groups, errors = {}, []
    raw_keys = [(x.get('arm'), x.get('phase'), x['id']) for x in raw]
    if len(by_key) != len(scored) or len(set(raw_keys)) != len(raw):
        errors.append({'duplicate_keys': True})
    if set(raw_keys) != set(by_key): errors.append({'raw_score_key_sets_differ': True})
    for row in raw:
        key = (row.get('arm'), row.get('phase'), row['id'])
        s = by_key[key]
        for k in ['source', 'actual'] + (['verdict'] if 'verdict' in row else []):
            if s[k] != row[k]: errors.append({'key': key, 'field': k})
        answered = (row['success'] and row['verdict'] == 'TRANSLATED') if 'verdict' in row else bool(row.get('actual'))
        correct = bool(answered and norm(row['actual']) in {norm(x) for x in cases[row['id']]['accepted']})
        if correct != s['correct'] or bool(answered and not correct) != s['acceptedWrong']:
            errors.append({'key': key, 'gold_vs_retained_score': {'correct': correct, 'retained': s['correct']}})
        name = ':'.join(str(x) for x in key[:2])
        g = groups.setdefault(name, {'n': 0, 'correct': 0, 'answered': 0, 'accepted_wrong': 0, 'correct_ids': [], 'wrong_ids': [], 'buckets': {}, 'answer_scope': 'ordinary_flow_TRANSLATED' if 'verdict' in row else 'nonempty_generated_text_no_acceptance_verdict'})
        g['n'] += 1; g['correct'] += correct; g['answered'] += answered
        g['accepted_wrong'] += bool(answered and not correct)
        if correct: g['correct_ids'].append(row['id'])
        if answered and not correct: g['wrong_ids'].append(row['id'])
        b = g['buckets'].setdefault(row['bucket'], {'n': 0, 'correct': 0})
        b['n'] += 1; b['correct'] += correct
    for g in groups.values():
        if g['answer_scope'] == 'nonempty_generated_text_no_acceptance_verdict':
            g['non_gold_nonempty_outputs'] = g.pop('accepted_wrong')
    return {'groups': groups, 'raw_rows': len(raw), 'score_rows': len(scored), 'mismatches': errors}

result = {'kind': 'REANALYSIS_OF_RETAINED_DATA_NO_NEW_EXPERIMENT', 'native_runs': 0, 'panels': {}}
p = '2026-10-18/P125-enru-memory-01/'
result['panels']['P125'] = count_panel(read(p+'raw-outputs.json'), read(p+'scores.json'), read(p+'panel-gold.json'))
result['P125_initial_equivalence'] = read(p+'initial-equivalence.json')
result['P125_training_receipts'] = read(p+'training-receipts.json')
gold = read('2026-10-18/P125-enru-memory-01/panel-gold.json')
for name in ['translation-memory-ingress-01', 'embedding-sft8-01', 'dpo-mask-live8-01']:
    pair = {}
    own_gold = read(f'2026-10-18/{name}/inputs/panel-gold.json')
    for arm in ['before', 'after']:
        p = f'2026-10-18/{name}/{arm}/'
        pair[arm] = count_panel(read(p+'raw-outputs.json'), read(p+'scores.json'), own_gold)
    result['panels'][name] = pair

for name in ['embedding-sft8-01', 'dpo-mask-live8-01']:
    arms = {}
    for arm in ['before', 'after']:
        p = f'2026-10-18/{name}/{arm}/'
        steps, cost, updates, initial = (read(p+x) for x in ['training-steps.json', 'cost.json', 'parameter-updates.json', 'initial-state.json'])
        losses = [x['loss'] if 'loss' in x else x['metrics']['loss'] for x in steps]
        arms[arm] = {'ids': [x['id'] for x in steps], 'steps': [x['step'] for x in steps],
            'optimizer_steps': [x['optimizer']['step'] if 'optimizer' in x else x['optimizerStep'] for x in steps],
            'online_pre_update_losses': losses, 'mean_online_pre_update_loss': statistics.mean(losses),
            'train_elapsed_ms_sum': sum(x['elapsedMs'] for x in steps), 'whole_process_cost': cost,
            'changed_tensors': sum(bool(x['delta']['changed']) for x in updates),
            'tensor_count': len(updates), 'initial_state': initial,
            'base_unchanged_every_step': all(x['baseUnchanged'] for x in steps) if all('baseUnchanged' in x for x in steps) else None,
            'reference_unchanged_every_step': all(x['referenceUnchanged'] for x in steps) if all('referenceUnchanged' in x for x in steps) else None,
            'training_material': read(p+'training-material.json')}
    a,b = arms['before'], arms['after']
    equality_fields = ['weightsSha256','rngSha256','tokenizerSha256','modelConfig','optimizerStep','dropout','trainableTensorCount','baseTensorCount'] + (['trainingConfig'] if name == 'embedding-sft8-01' else ['referenceSha256','baseSha256','optimizerSettings'])
    result[name] = {'arms': arms,
        'matched_ids_steps': a['ids'] == b['ids'] and a['steps'] == b['steps'],
        'matched_initial_fields': {k:a['initial_state'][k] == b['initial_state'][k] for k in equality_fields},
        'matched_training_material': a['training_material'] == b['training_material'],
        'optimizer_steps_are_1_through_8': all(v['optimizer_steps'] == list(range(1,9)) for v in arms.values()),
        'paired_loss_improvements': sum(y<x for x,y in zip(a['online_pre_update_losses'], b['online_pre_update_losses'])),
        'paired_loss_regressions': sum(y>x for x,y in zip(a['online_pre_update_losses'], b['online_pre_update_losses'])),
        'mean_loss_B_minus_A': b['mean_online_pre_update_loss'] - a['mean_online_pre_update_loss']}
    for v in arms.values():
        material = v.pop('training_material')
        v['training_material_rows'] = len(material)

p = '2026-10-18/P25-enru-cold-01/'
old = read('2026-10-18/P125-enru-memory-01/raw-outputs.json')
old = {x['id']:x for x in old if x['arm']=='B' and x['phase']=='after'}
fresh = read(p+'raw-outputs.json')
fields = ['id','source','actual','verdict','selectedFlow','adapterId','metrics']
result['P25_restore'] = {'fresh_rows': len(fresh),
    'mismatch_ids': [x['id'] for x in fresh if any(x.get(k)!=old[x['id']].get(k) for k in fields)],
    'canonical_equal': read(p+'source-canonical.json') == read(p+'loaded-canonical.json') == read(p+'after-canonical.json'),
    'load_receipt': read(p+'load-receipt.json'), 'retained_comparison': read(p+'comparison-summary.json')}

p='2026-10-20/N-KU-01/'
g = {x['id']:x['expected'] for x in read(p+'gold.json')['exam']}
def nku_score(rows):
    details = []
    for x in rows:
        expected = g[x['id']]; answer = x['answer']
        correct = answer is not None and ((answer == 'ADD_LOOP') if expected == 'ADD_LOOP' else answer != 'ADD_LOOP')
        details.append({'id':x['id'],'expected':expected,'answer':answer,'correct':correct})
    return {'n':len(rows),'correct':sum(x['correct'] for x in details),'details':details}
nku={}
for order in ['', 'order-check-BA/']:
    arms={}
    for arm in ['A','B']:
        v=read(p+order+'arm-'+arm+'.json')
        arms[arm]={'before':nku_score(v['state0']), 'after':nku_score(v['state1']),
            'training':v['training'], 'process':v['process'], 'memory_state':v['memoryState']}
    nku[order or 'main-AB']=arms
result['N-KU-01']=nku
result['dpo_mask_saved_derivative_checks'] = {}
for arm in ['before', 'after']:
    guard = read(f'2026-10-18/dpo-mask-live8-01/{arm}/component-guard.json')
    result['dpo_mask_saved_derivative_checks'][arm] = {
        'recorded_coordinate_step_size_cases': len(guard['rows']),
        'recorded_passes': sum(bool(x['pass']) for x in guard['rows']),
        'scope': 'Count of retained checks of the same native training backward; no numerical probe executed by this script'}
result['sources'] = sources
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps({'panels':{k:({a:g['correct'] for a,g in v['groups'].items()} if 'groups' in v else {a:{n:g['correct'] for n,g in x['groups'].items()} for a,x in v.items()}) for k,v in result['panels'].items()},
 'sft_mean_delta':result['embedding-sft8-01']['mean_loss_B_minus_A'],
 'native_runs':0, 'output':str(args.output)},ensure_ascii=False))
