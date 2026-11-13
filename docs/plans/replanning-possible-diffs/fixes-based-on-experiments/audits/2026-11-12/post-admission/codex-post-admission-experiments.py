#!/usr/bin/env python3
"""Recount frozen rows and derive selected-test cost, without invoking a learner.

python codex-post-admission-experiments.py --core CORE --output NEW_JSON
The adjacent codex-post-admission-inputs.json pins every source byte. Existing
output is refused. Cost is conditional source arithmetic, never a native run.
"""
import argparse
import hashlib
import json
import re
from collections import Counter
from pathlib import Path


def require(condition, message):
    if not condition:
        raise ValueError(message)


def mask_csharp(source):
    """Mask comments and strings so named-method call counts exclude prose."""
    result = list(source)
    i = 0
    while i < len(source):
        end = None
        if source.startswith('//', i):
            end = source.find('\n', i)
            if end < 0:
                end = len(source)
        elif source.startswith('/*', i):
            close = source.find('*/', i + 2)
            require(close >= 0, 'Unclosed C# comment')
            end = close + 2
        elif source.startswith('"""', i):
            width = len(re.match(r'"+', source[i:]).group())
            close = source.find('"' * width, i + width)
            require(close >= 0, 'Unclosed raw C# string')
            end = close + width
        elif source[i] == '"':
            verbatim = i > 0 and source[i - 1] == '@'
            end = i + 1
            while end < len(source):
                if verbatim and source.startswith('""', end):
                    end += 2
                elif not verbatim and source[end] == '\\':
                    end += 2
                elif source[end] == '"':
                    end += 1
                    break
                else:
                    end += 1
        if end is not None:
            for j in range(i, end):
                if result[j] != '\n':
                    result[j] = ' '
            i = end
        else:
            i += 1
    return ''.join(result)


def fact_bodies(source):
    masked = mask_csharp(source)
    facts = {}
    for match in re.finditer(r'\[Fact\]\s+public void (\w+)\(\)\s*\{', masked):
        start = match.end() - 1
        depth, pos = 1, start + 1
        while depth and pos < len(masked):
            depth += (masked[pos] == '{') - (masked[pos] == '}')
            pos += 1
        require(depth == 0, 'Unclosed Fact body')
        facts[match.group(1)] = {
            'line': source.count('\n', 0, match.start()) + 1,
            'body': masked[start + 1:pos - 1],
        }
    return facts


def call_count(body, name):
    return len(re.findall(r'\b' + re.escape(name) + r'\s*\(', body))


def method_body(source, name):
    """Read one fixed-source method; not a general C# control-flow interpreter."""
    masked = mask_csharp(source)
    matches = list(re.finditer(
        r'\b(?:public|private)\s+static\s+[^\n]*\b' + re.escape(name)
        + r'\s*\([^)]*\)\s*(=>|\{)', masked))
    require(len(matches) == 1, 'Expected one fixed-source method: ' + name)
    match = matches[0]
    if match.group(1) == '=>':
        end = masked.find(';', match.end())
        require(end >= 0, 'Unclosed expression method: ' + name)
        return masked[match.end():end]
    depth, pos = 1, match.end()
    while depth and pos < len(masked):
        depth += (masked[pos] == '{') - (masked[pos] == '}')
        pos += 1
    require(depth == 0, 'Unclosed method: ' + name)
    return masked[match.end():pos - 1]


def pinned_cardinalities(text):
    """Derive the multipliers from reviewed, hash-pinned successful paths.

    Shape checks explain the fixed-source arithmetic; they do not execute C#,
    prove arbitrary future helper behavior or count hidden runtime operations.
    """
    builder_path = '27.TranslationDomain/TranslationStructuredBatchBuilder.cs'
    correction_path = 'experiments-v2/RP01-MICRO/P5801/src/CorrectiveLessonCounterExampleArms.cs'
    annotation_path = 'experiments-v2/RP01-MICRO/P5201/src/RejectedAnnotationAddressArms.cs'
    test58_path = 'Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs'
    test52_path = 'Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs'
    builder = text(builder_path)
    general = method_body(builder, 'BuildGeneralBatch')
    examples_per_pair = len(re.findall(r'\bexamples\.Add\s*\(\s*BuildExample\s*\(', general))
    require(examples_per_pair == 3 and 'foreach (var pair in pairs)' in general,
            'Reviewed successful per-pair builder path changed')
    require('Examples: examples' in general, 'Returned lesson collection changed')
    ruen = method_body(builder, 'BuildRuEnGeneralBatch')
    require(re.search(r'return\s+BuildGeneralBatch\(batchId, pairs, heldOut, signatureMode\)', ruen),
            'RU_EN wrapper no longer forwards the same pairs')

    test58, test52 = text(test58_path), text(test52_path)
    setup_pairs = len(re.findall(r'\bnew\s+CorpusRow\s*\(', method_body(test58, 'TrainRows')))
    panel58 = len(re.findall(r'\bnew\s+CorpusRow\s*\(', method_body(test58, 'ExamRows')))
    setup = method_body(test58, 'BuildSetupBatch')
    require(re.search(r'TrainRows\(\)\.Select\(row => new TranslationTrainReadyPair\(', setup),
            'Setup no longer maps each source row to one pair')
    require(call_count(setup, 'BuildGeneralBatch') == 1, 'Setup builder path changed')
    correction = method_body(text(correction_path), 'BuildCorrectiveBatch')
    correction_pairs = len(re.findall(r'\bnew\s+TranslationTrainReadyPair\s*\(', correction))
    require(correction_pairs == 1 and re.search(r'BuildGeneralBatch\(\s*batchId,\s*\[pair\]', correction),
            'Correction no longer forwards exactly one pair')
    trainer58 = method_body(test58, 'TrainBatch')
    require('foreach (var example in batch.Examples)' in trainer58
            and call_count(trainer58, 'TrainOnExample') == 1,
            'P58 successful one-call-per-example loop changed')
    run58 = method_body(test58, 'RunArm')
    require(call_count(run58, 'TrainBatch') == 2 and call_count(run58, 'BuildSetupBatch') == 1
            and call_count(run58, 'BuildCorrectiveBatch') == 1,
            'P58 successful arm no longer has setup plus correction')

    pairs52 = len(re.findall(r'\bnew\s+TranslationTrainReadyPair\s*\(', method_body(test52, 'TrainPairs')))
    panel52 = len(re.findall(r'\bnew\s+ExamRow\s*\(', method_body(test52, 'ExamRows')))
    annotation = method_body(text(annotation_path), 'BuildArm')
    require('foreach (var pair in pairs)' in annotation
            and len(re.findall(r'\barmPairs\.Add\s*\(', annotation)) == 2
            and re.search(r'armPairs\.Add\(pair with \{ VerifiedRuleTags = harvested\.Value! \}\);\s*continue;', annotation)
            and re.search(r'BuildRuEnGeneralBatch\(\s*batchId,\s*armPairs,', annotation),
            'Reviewed exclusive branches no longer preserve P52 pair count')
    run52 = method_body(test52, 'RunArm')
    require('foreach (var example in batch.Value!.Examples)' in run52
            and call_count(run52, 'TrainOnExample') == 1,
            'P52 successful one-call-per-example loop changed')

    require((setup_pairs, panel58, pairs52, panel52) == (8, 8, 2, 4),
            'Frozen test-row cardinalities changed')
    return {
        'builder_path': builder_path, 'examples_per_successful_pair': examples_per_pair,
        'P58': {'test_path': test58_path, 'correction_helper_path': correction_path,
                'setup_pairs': setup_pairs, 'corrective_pairs': correction_pairs,
                'setup_unit_lesson_calls': setup_pairs * examples_per_pair,
                'corrective_unit_lesson_calls': correction_pairs * examples_per_pair,
                'unit_lesson_calls_per_arm': (setup_pairs + correction_pairs) * examples_per_pair,
                'panel_rows': panel58, 'train_control_rows': setup_pairs},
        'P52': {'test_path': test52_path, 'annotation_helper_path': annotation_path,
                'train_pairs_per_arm': pairs52,
                'unit_lesson_calls_per_arm': pairs52 * examples_per_pair,
                'panel_rows': panel52},
        'scope': 'Hash-pinned, reviewed successful construction paths and explicit Unit TrainOnExample call sites only; no C# execution or transitive runtime call accounting.'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--core', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    require(not args.output.exists(), 'Preserve earlier output; choose a new file')
    root = args.core.resolve()
    manifest = json.loads(Path(__file__).with_name('codex-post-admission-inputs.json').read_text())
    inputs = {}
    for item in manifest['inputs']:
        data = (root / item['path']).read_bytes()
        sha = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        require(sha == item['git_blob_sha1'], 'Wrong source bytes: ' + item['path'])
        inputs[item['path']] = {'git_blob_sha1': sha, 'sha256': hashlib.sha256(data).hexdigest(), 'bytes': len(data)}

    def text(path):
        require(path in inputs, 'Unpinned source: ' + path)
        return (root / path).read_text(encoding='utf-8-sig')

    def raw(experiment, attempt, filename):
        return json.loads(text(f'experiments-v2/RP01-MICRO/{experiment}/results/attempt-{attempt:04d}/{filename}'))

    a48 = raw('P4801', 3, 'arms.json')
    left, right = [a48[k]['Panel'] for k in ('плечо_равных_весов', 'контроль_механизма')]
    indexed48 = [{r['RowId']: r for r in rows} for rows in (left, right)]
    require(indexed48[0].keys() == indexed48[1].keys(), 'P48 panel mismatch')
    pairs48 = [(indexed48[0][k], indexed48[1][k]) for k in indexed48[0]]
    p48 = {
        'correct': [sum(r['Answer'] == r['Gold'] for r in rows) for rows in (left, right)],
        'rows': len(left),
        'answer_changes': sum(a['Answer'] != b['Answer'] for a, b in pairs48),
        'gained': [a['RowId'] for a, b in pairs48 if a['Answer'] != a['Gold'] and b['Answer'] == b['Gold']],
        'lost': [a['RowId'] for a, b in pairs48 if a['Answer'] == a['Gold'] and b['Answer'] != b['Gold']],
    }

    a49 = raw('P4901', 3, 'arms.json')
    panel49 = a49['состав']['панель']
    exam49 = [r for r in panel49 if r['Kind'] == 'EXAM']
    warm = a49['состав']['пары_подготовки']
    new = a49['состав']['пары_нового_обучения']

    def exposure(rows, training):
        seen_input = {p['Current'] for p in training}
        seen_pair = {(p['Current'], p['Next']) for p in training}
        return [{'row_id': r['RowId'], 'input': r['Input'], 'gold': r['Gold'],
                 'exposure': 'exact_pair_seen' if (r['Input'], r['Gold']) in seen_pair else
                 'input_seen_target_new' if r['Input'] in seen_input else 'input_unseen'} for r in rows]

    p49 = {'exposure': {}, 'new_project_gains': []}
    for name, training in [('cold_new_only', new), ('warm_plus_new', warm + new)]:
        rows = exposure(exam49, training)
        p49['exposure'][name] = {'rows': rows, 'counts': dict(Counter(r['exposure'] for r in rows))}
    by49 = {k: {r['RowId']: r for r in v} for k, v in a49['показания'].items()}
    for row in panel49:
        ca = by49['холодное_плечо'][row['RowId']]['Answer'] == row['Gold']
        cb = by49['тёплое_плечо'][row['RowId']]['Answer'] == row['Gold']
        if row['ProjectId'] == 'B' and cb and not ca:
            p49['new_project_gains'].append(exposure([row], new)[0])
    synthetic = [{'RowId': 'same-input-new-target', 'Input': 1, 'Gold': 5}]
    p49['target_only_negative_control'] = exposure(synthetic, warm + new)
    require(p49['target_only_negative_control'][0]['exposure'] == 'input_seen_target_new', 'Target edit invented input novelty')

    before52 = raw('P5201', 2, 'before.json')['строки']
    arms52 = raw('P5201', 2, 'arms.json')
    p52 = {'cold': sum(r['AnswerText'].lower() == r['GoldText'].lower() for r in before52),
           'constant_ceiling': max(Counter(r['GoldText'].lower() for r in before52).values()),
           'rows': len(before52), 'arms': {}}
    for k in ('плечо_до_исправления', 'плечо_после_исправления'):
        rows = arms52[k]['Rows']
        p52['arms'][k] = {'answer_correct': sum(r['AnswerText'].lower() == r['GoldText'].lower() for r in rows),
                         'tense_correct': sum(r['TrainedTense'] == r['GoldTense'] for r in rows)}

    before58 = raw('P5801', 2, 'before.json')['после_установочных_уроков']['строки']
    correction = next(r for r in before58 if r['Answer'].lower() != r['Gold'].lower())
    arms58 = raw('P5801', 2, 'arms.json')
    p58 = {'chosen_correction': correction,
           'partition_rule': 'Literal Source equality with the one corrective input; no normalization, alias, model-key or source-family exclusion is inferred.',
           'arms': {}}
    for k in ('плечо_A_строка_корпуса', 'плечо_B_фактический_ошибочный_ответ', 'контроль_измерителя_фразовая_пометка'):
        arm = arms58[k]
        require(arm['LessonChosenText'] == correction['Gold'], 'Correction chosen mismatch')
        rows = arm['PanelAfter']
        require(len({r['Id'] for r in rows}) == len(rows), 'Duplicate P58 row identity')
        seen = [r for r in rows if r['Source'] == correction['Source']]
        other_source = [r for r in rows if r['Source'] != correction['Source']]
        p58['arms'][k] = {'corrected_item_rows': len(seen), 'corrected_item_correct': sum(r['Answer'].lower() == r['Gold'].lower() for r in seen),
                         'corrected_item_adapters': [r['Adapter'] for r in seen],
                         'other_literal_source_rows': len(other_source),
                         'other_literal_source_correct': sum(r['Answer'].lower() == r['Gold'].lower() for r in other_source),
                         'other_literal_source_row_ids': [r['Id'] for r in other_source]}
        require(len(seen) + len(other_source) == len(rows), 'Literal-source partition does not close')

    cardinalities = pinned_cardinalities(text)
    costs = {}
    for exp, classname in [
        ('P58', 'P5801CorrectiveLessonCounterExampleArmsTests'),
        ('P52', 'P5201RejectedAnnotationAddressArmsTests'),
    ]:
        lessons_per_arm = cardinalities[exp]['unit_lesson_calls_per_arm']
        panel_size = cardinalities[exp]['panel_rows']
        source = text(f'Tests/ExperimentFixes/{classname}.cs')
        facts = fact_bodies(source)
        rows = []
        for method, fact in facts.items():
            body = fact['body']
            arms = call_count(body, 'RunArm')
            direct_setup = call_count(body, 'TrainBatch') if exp == 'P58' else 0
            lessons = arms * lessons_per_arm + direct_setup * cardinalities['P58']['setup_unit_lesson_calls']
            reads = panel_size * (arms + call_count(body, 'RunPanel') + call_count(body, 'RunTrainControl') + call_count(body, 'ColdPanel'))
            rows.append({'method': method, 'fact_line': fact['line'], 'run_arm_calls': arms,
                         'direct_setup_batches': direct_setup, 'unit_lesson_calls': lessons, 'direct_output_reads': reads})
        costs[exp] = {'source_fact_count': len(facts), 'facts': rows,
                      'unit_lesson_calls': sum(r['unit_lesson_calls'] for r in rows),
                      'direct_output_reads': sum(r['direct_output_reads'] for r in rows),
                      'scope': 'Source-derived: each current Fact once, every path successful, no retry; not native observation or historical run total.'}
    require(costs['P58']['source_fact_count'] == 16, 'P58 discovery assumption changed')
    require(costs['P58']['unit_lesson_calls'] == 261, 'P58 cost assumption changed')
    require(costs['P58']['direct_output_reads'] == 104, 'P58 read assumption changed')
    require(costs['P52']['source_fact_count'] == 2, 'P52 discovery assumption changed')
    require(costs['P52']['unit_lesson_calls'] == 36, 'P52 cost assumption changed')
    require(costs['P52']['direct_output_reads'] == 28, 'P52 read assumption changed')
    result = {'core_ref': manifest['ref'], 'revision': 'v2',
              'revision_note': 'Adds three pinned cardinality helpers (29 -> 32 inputs); narrows P58 partition to literal Source; preserves the separate v1 result.',
              'execution': 'Python recount and bounded source arithmetic; native C#/training UNRUN',
              'inputs': inputs, 'P48': p48, 'P49': p49, 'P52': p52, 'P58': p58,
              'cardinality_derivation': cardinalities, 'selected_class_costs': costs}
    # The early exists check gives a clear error; exclusive create is the
    # authority if another writer creates the path while inputs are read.
    with args.output.open('x', encoding='utf-8', newline='\n') as output:
        output.write(json.dumps(result, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'verified_inputs': len(inputs), 'P48': p48, 'P49': p49, 'P52': p52, 'P58': p58,
                      'selected_class_costs': {k: {n: v[n] for n in ('source_fact_count', 'unit_lesson_calls', 'direct_output_reads')} for k, v in costs.items()}}, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
