"""Source-equivalent Python replay, not native C# execution.

Reads the pinned WC01 fixture and archived snapshot and derives update counts
for every epoch of the deterministic Train loop. It does not alter product data.
"""
import json, math, re, hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / 'repos/core'
HERE = Path(__file__).resolve().parent
fixture_path = ROOT / 'experiments-v2/RP01-MICRO/WC01/fixture.json'
snapshot_path = ROOT / 'experiments-v2/RP01-MICRO/WC01/results/attempt-001/A/after-snapshot.json'
fixture = json.loads(fixture_path.read_text())
snapshot = json.loads(snapshot_path.read_text())
labels = snapshot['labels']

def featurize(text):
    # The pinned fixture contains ASCII only, so these are exactly the
    # character classes used by RegexLikeWords on these concrete inputs.
    assert text.isascii()
    tokens = re.findall(r'[A-Za-z0-9_-]+', text.lower())
    features = {}
    for token in tokens:
        features[token] = features.get(token, 0.) + 1.
    for left, right in zip(tokens, tokens[1:]):
        bigram = left + '_' + right
        features[bigram] = features.get(bigram, 0.) + 1.
    normalizer = max(1., math.sqrt(sum(value * value for value in features.values())))
    return {key:value / normalizer for key,value in features.items()}

weights, biases = {}, [0.] * len(labels)
prepared = [(row,featurize(row['prompt'])) for row in fixture['train']]
epochs = []
for epoch in range(18):
    updates = []
    for row, features in prepared:
        scores = list(biases)
        for token, value in features.items():
            if token not in weights:
                continue
            for index in range(len(scores)):
                scores[index] += weights[token][index] * value
        predicted = max(range(len(scores)),key=scores.__getitem__)
        expected = labels.index(row['expectedRouteKind'])
        if predicted == expected:
            continue
        for token, value in features.items():
            w = weights.setdefault(token,[0.] * len(labels))
            w[expected] += .14 * value
            w[predicted] -= .14 * value
        biases[expected] += .14
        biases[predicted] -= .14
        updates.append({'row_id':row['id'],'predicted_before_update':labels[predicted],'expected':labels[expected]})
    epochs.append({'epoch':epoch + 1,'presentations':len(prepared),'update_count':len(updates),'updates':updates})

assert set(weights) == set(snapshot['tokenWeights'])
diffs = [abs(biases[i] - snapshot['biases'][i]) for i in range(len(labels))]
for k,w in weights.items():
    diffs.extend(abs(w[i] - snapshot['tokenWeights'][k][i]) for i in range(len(labels)))
result = {
 'kind':'source-equivalent deterministic Python replay; NOT a native C# run',
 'source_ref':'009bf226174441fa0fbf0336e74ad2f2b7174d92',
 'source_method':'42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:59-99',
 'fixture_sha256':hashlib.sha256(fixture_path.read_bytes()).hexdigest(),
 'archived_snapshot_sha256':hashlib.sha256(snapshot_path.read_bytes()).hexdigest(),
 'trained_parameter_count':len(diffs),
 'feature_count':len(weights),
 'nonzero_parameter_count':sum(x != 0 for x in biases) + sum(x != 0 for w in weights.values() for x in w),
 'max_absolute_parameter_difference_to_archived_snapshot':max(diffs),
 'exact_parameter_match':all(x == 0 for x in diffs),
 'total_updates':sum(x['update_count'] for x in epochs),
 'epochs_with_updates':sum(x['update_count'] > 0 for x in epochs),
 'empty_epochs':sum(x['update_count'] == 0 for x in epochs),
 'epoch_ledger':epochs,
 'limitation':'No native execution, wall-time/allocation comparison, generalization claim or production patch.'
}
(HERE / 'wiki_epoch_recomputation.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
print(json.dumps({k:v for k,v in result.items() if k != 'epoch_ledger'},ensure_ascii=False,indent=2))
print('updates_by_epoch:', [e['update_count'] for e in epochs])
