#!/usr/bin/env python3
"""Derive preparation registries and card command tails; never call a model/train.

Run --write to refresh, --check to check deterministic regeneration. Authoritative
inputs: parent topic indexes, the four curriculum packs, refresh-config.json.
New addition records are imported once, without replacing existing records.
"""
import argparse
from collections import Counter, defaultdict
from copy import deepcopy
import json
from pathlib import Path
import re
from creation_mode import MODE, ROLE_CONTRACT, operational_text, with_card_mode

BASE = Path(__file__).resolve().parents[1]
DATE = '2026-10-25'
BEGIN = '<!-- GENERATED-QUEUE-START -->'
END = '<!-- GENERATED-QUEUE-END -->'


def read(path):
    return json.loads((BASE / path).read_text(encoding='utf-8'))


def dump(value):
    return json.dumps(value, ensure_ascii=False, indent=2) + '\n'


def lines(path):
    return [json.loads(s) for s in (BASE / path).read_text(encoding='utf-8').splitlines() if s.strip()]


def domain(topic, profile):
    return 'tr.' + profile.split('-')[-1] if topic['domain_ids'][0].startswith('tr.') else topic['domain_ids'][0]


def command(stream, run, count, split='train', root=None):
    root = root or read('paths.json')['learning_sets_root_windows']
    script = root + '/preparation/tools/create_sets.ps1'
    return (f'powershell -NoProfile -ExecutionPolicy Bypass -File "{script}" '
            f'-Stream "{stream}" -Run "{run}" -Count {count} -Split {split} -Start 0')


def public_row(o, e):
    metadata = deepcopy(e.get('metadata', {}))
    metadata.update(reference_only=True, training_allowed=False, split='reference',
                    learning_objective_id=o['objective_id'], measured_training_result=False)
    return {'schema_version': 'xiigen.dpo.v1', 'pair_id': e['example_id'],
            'domain_id': domain(o, e['profile_id']), 'topic_id': o['topic_id'],
            'profile_id': e['profile_id'], 'family_id': e['family_id'], 'split': 'reference',
            **{k: e[k] for k in ['prompt', 'chosen', 'rejected', 'error_tag', 'rationale']},
            'source_refs': e.get('source_refs', o['source_refs']),
            'provenance': {'kind': 'synthetic_from_repo', 'generator_model': 'reference_authoring_not_a_kiro_run'},
            'metadata': metadata}


def strip_generated(text):
    text = re.sub(re.escape(BEGIN) + r'.*?' + re.escape(END) + r'\s*', '', text, flags=re.S)
    # Replace only these generated sections, retaining any unrelated later H2.
    heads = ['Переходы единой очереди', 'Переходы полной очереди', 'Точные команды действующих потоков']
    for heading in heads:
        text = re.sub(r'^## ' + re.escape(heading) + r'\s*\n.*?(?=^## |\Z)', '', text, flags=re.M | re.S)
    return text.rstrip() + '\n\n'


def build():
    cfg = read('curriculum/refresh-config.json')
    paths = read('paths.json')
    root = paths['learning_sets_root_windows']
    preparation_root = paths['preparation_root_windows']
    if cfg['work_root'] != root:
        raise ValueError('refresh-config work_root differs from paths.json; update the designated path once')
    outputs = {}
    topics = []
    for path in sorted(BASE.glob('domains/*/topic-index.json')):
        topics.extend(json.loads(path.read_text(encoding='utf-8')))
    topics.sort(key=lambda x: x['order'])
    topic_by = {x['id']: x for x in topics}
    if len(topic_by) != len(topics):
        raise ValueError('Duplicate parent topic ID')
    packs = {f: read('curriculum/' + f) for f in cfg['pack_files']}
    imported = []
    for spec in cfg['addition_imports']:
        addition = read(spec['file'])
        objs = addition.get('objectives', [addition])
        target = packs[spec['pack']]['objectives']
        known = {o['objective_id'] for o in target}
        for obj in objs:
            if obj['objective_id'] not in known:
                target.append(deepcopy(obj))
                known.add(obj['objective_id'])
                imported.append(obj['objective_id'])
    objectives = {}
    for f, pack in packs.items():
        for o in pack['objectives']:
            if 'generation_prompt_ru' in o:
                o['generation_prompt_ru'] = operational_text(o['generation_prompt_ru'])
            # Some original packs kept these assignments only in pilot-queue.
            # Preserve them from the saved source config instead of inventing defaults.
            old_assignments = [s for s in cfg['objective_streams'] if s['stream_id'].split('/')[0] == o['objective_id']]
            for key, old_key in [('generation_batch_size', 'batch_pairs'), ('pilot_train_pairs', 'planned_train_pairs'),
                                 ('pilot_dev_pairs', 'planned_dev_pairs'), ('pilot_exam_pairs', 'planned_exam_pairs'),
                                 ('pairs_per_family', 'pairs_per_family')]:
                if key not in o:
                    values = {s[old_key] for s in old_assignments if old_key in s}
                    if len(values) == 1:
                        o[key] = values.pop()
                    elif key == 'pairs_per_family' and not values:
                        o[key] = 1
                    else:
                        raise ValueError('Missing/unresolved assignment ' + key + ' for ' + o['objective_id'])
            if o['objective_id'] in objectives:
                raise ValueError('Duplicate objective: ' + o['objective_id'])
            objectives[o['objective_id']] = o
        outputs['curriculum/' + f] = dump(pack)
    # Preserve old rows exactly at the data level; append only missing IDs.
    refs = lines('curriculum/reference-examples.jsonl')
    ref_by = {r['pair_id']: r for r in refs}
    if len(ref_by) != len(refs):
        raise ValueError('Duplicate existing public reference pair ID')
    for o in objectives.values():
        for e in o['examples']:
            if e['example_id'] not in ref_by:
                r = public_row(o, e)
                refs.append(r)
                ref_by[r['pair_id']] = r
    outputs['curriculum/reference-examples.jsonl'] = ''.join(json.dumps(r, ensure_ascii=False) + '\n' for r in refs)
    source_order = cfg['objective_streams']
    ids = [s['stream_id'] for s in source_order]
    if len(ids) != len(set(ids)):
        raise ValueError('Duplicate stream in source order')
    if ids[:len(cfg['existing_objective_prefix'])] != cfg['existing_objective_prefix']:
        raise ValueError('Previously assigned objective order changed')
    expected = {o['objective_id'] + '/' + p for o in objectives.values() for p in o['profiles']}
    if set(ids) != expected:
        raise ValueError('Objective streams missing/extra: ' + repr(sorted(set(ids) ^ expected)))
    pilots, objective_rows, objective_index = [], [], []
    indexed = set()
    for n, spec in enumerate(source_order, 1):
        oid, profile = spec['stream_id'].split('/')
        o = objectives[oid]
        topic = topic_by[o['topic_id']]
        if profile not in topic['profiles']:
            raise ValueError('Profile outside parent topic: ' + spec['stream_id'])
        if oid not in indexed:
            objective_index.append({'id': oid, 'topic_id': o['topic_id'], 'title_ru': o['title_ru'],
                                    'profiles': o['profiles'], 'card': f'curriculum/cards/{oid}.md',
                                    'source_refs': o['source_refs']})
            indexed.add(oid)
        run, count = spec['first_run_id'], o['generation_batch_size']
        d = domain(topic, profile)
        out = f'{root}/{oid}/{profile}/{run}'
        pairsize = o.get('pairs_per_family', 1)
        if count % pairsize:
            raise ValueError('Incomplete paired batch: ' + spec['stream_id'])
        p = {'order': n, 'stream_id': spec['stream_id'], 'objective_id': oid, 'topic_id': o['topic_id'],
             'profile_id': profile, 'domain_id': d, 'card': f'curriculum/cards/{oid}.md',
             'first_run_id': run, 'batch_pairs': count, 'pairs_per_family': pairsize,
             'planned_train_pairs': o['pilot_train_pairs'], 'planned_dev_pairs': o['pilot_dev_pairs'],
             'planned_exam_pairs': o['pilot_exam_pairs'],
             'prepare_first_train': command(spec['stream_id'], run, count, root=root),
             'prepare_first_dev': command(spec['stream_id'], run + 'D01', count, 'dev', root)
                 if not o.get('family_plan_files') or 'dev' in o['family_plan_files'] else None,
             'prepare_first_exam': command(spec['stream_id'], run + 'E01', count, 'exam', root)
                 if not o.get('family_plan_files') or 'exam' in o['family_plan_files'] else None,
             'first_output_directory': out,
             'dataset_directory': f'{root}/datasets/{oid}-{profile}-v1',
             'baseline_directory': f'{root}/baselines/{d}/{profile}/{oid}-v1/B1',
             'state': 'PREPARED_NOT_GENERATED', 'learning_status': 'NOT_RUN',
             'continuation_rule': 'For an existing run read request/state and resume its actual handle. A new batch uses previous next_start and a new run ID; never delete/overwrite. Paths designate artifacts, not existing weights.'}
        p.update({k: v for k, v in spec.items() if k.startswith('legacy_')})
        if pairsize == 2:
            p['family_class_field'] = o.get('family_class_field', 'clarification_class')
            p['family_class_values'] = o.get('family_class_values', ['ambiguous', 'resolved'])
        pilots.append(p)
        objective_rows.append({'kind': 'objective', 'stream_id': p['stream_id'], 'topic_id': p['topic_id'],
                               'objective_id': oid, 'profile_id': profile, 'domain_id': d, 'card': p['card'],
                               'candidate_count': count, 'first_run_id': run, 'command': p['prepare_first_train'],
                               'first_output_directory': out, 'budget_pool': p['topic_id'] + '/' + profile,
                               'source_queue': 'curriculum/pilot-queue.json', 'source_order': n,
                               'training_status': 'NOT_RUN'})
    replacements = defaultdict(list)
    for s in objective_rows:
        replacements[s['budget_pool']].append(s['stream_id'])
    parent_rows, generation = [], []
    parents_expected = {t['id'] + '/' + p for t in topics for p in t['profiles']}
    if {x['stream_id'] for x in cfg['parent_streams']} != parents_expected:
        raise ValueError('Parent stream source order differs from topic catalog')
    for n, spec in enumerate(cfg['parent_streams'], 1):
        tid, profile = spec['stream_id'].split('/')
        t = topic_by[tid]
        covered = bool(replacements[spec['stream_id']])
        cmd = command(spec['stream_id'], spec['first_run_id'], spec['candidate_count'], root=root)
        g = {'sequence': n, 'stream_id': spec['stream_id'], 'card_order': t['order'], 'topic_id': tid,
             'profile_id': profile, 'domain_id': domain(t, profile), 'card': t['card'],
             'candidate_count': spec['candidate_count'], 'target_accepted_train_pairs': t['target_pairs_per_profile'],
             'first_run_id': spec['first_run_id'], 'command': None if covered else cmd,
             'covered_by_objectives': covered, 'replacement_stream_ids': replacements[spec['stream_id']],
             'default_execution': not covered, 'legacy_command': spec.get('legacy_command'), 'training_status': 'NOT_RUN'}
        generation.append(g)
        if not covered:
            parent_rows.append({'kind': 'topic', 'stream_id': g['stream_id'], 'topic_id': tid,
                                'objective_id': None, 'profile_id': profile, 'domain_id': g['domain_id'],
                                'card': t['card'], 'candidate_count': g['candidate_count'],
                                'first_run_id': g['first_run_id'], 'command': cmd,
                                'first_output_directory': f'{root}/{tid}/{profile}/{g["first_run_id"]}',
                                'budget_pool': g['stream_id'], 'source_queue': 'generation-queue.json',
                                'source_order': n, 'training_status': 'NOT_RUN'})
    execution = objective_rows + parent_rows
    full_by = {}
    for n, s in enumerate(execution, 1):
        s['order'] = n
        s['next_stream_id'] = execution[n]['stream_id'] if n < len(execution) else 'END'
        full_by[s['stream_id']] = s
    for n, p in enumerate(pilots):
        p['next_stream_id'] = pilots[n + 1]['stream_id'] if n + 1 < len(pilots) else None
        p['execution_next_stream_id'] = full_by[p['stream_id']]['next_stream_id']
    pilot_by = {p['stream_id']: p for p in pilots}
    group = {s['stream_id']: (len(objectives[s['objective_id']]['scenario_axes'][0]['values'])
                              if s.get('objective_id') == 'OR-ROUTE-INTENT'
                              else objectives[s['objective_id']].get('pairs_per_family', 1))
             if s['kind'] == 'objective' else 1 for s in execution}
    budget_pools, scale_rows, allocations = [], [], {}
    for g in generation:
        pool = g['stream_id']
        members = [s for s in execution if s['budget_pool'] == pool]
        budget = g['target_accepted_train_pairs']
        per = budget // len(members)
        assigned = {s['stream_id']: (per // group[s['stream_id']]) * group[s['stream_id']] for s in members}
        remain = budget - sum(assigned.values())
        while True:
            affordable = [s for s in members if group[s['stream_id']] <= remain]
            if not affordable:
                break
            pick = min(affordable, key=lambda s: (assigned[s['stream_id']], s['order']))
            assigned[pick['stream_id']] += group[pick['stream_id']]
            remain -= group[pick['stream_id']]
        allocations.update(assigned)
        budget_pools.append({'budget_pool': pool, 'topic_id': g['topic_id'], 'profile_id': g['profile_id'],
                             'parent_train_budget_pairs': budget, 'allocated_train_pairs': sum(assigned.values()),
                             'reserve_train_pairs': remain, 'status': 'ALLOCATION_ONLY_NOT_GENERATED',
                             'covered_by_objectives': g['covered_by_objectives'],
                             'allocations': [{'stream_id': s['stream_id'], 'target_accepted_train_pairs': assigned[s['stream_id']],
                                              'complete_group_size': group[s['stream_id']]} for s in members],
                             'reserve_reason': 'Incomplete class/family group cannot be allocated.' if remain else None})
    for s in execution:
        p = pilot_by.get(s['stream_id'])
        oid = s.get('objective_id') or s['topic_id']
        sc = {'stream_id': s['stream_id'], 'kind': s['kind'], 'topic_id': s['topic_id'],
              'profile_id': s['profile_id'], 'objective_id': s['objective_id'], 'budget_pool': s['budget_pool'],
              'execution_order': s['order'], 'target_accepted_train_pairs': allocations[s['stream_id']],
              'generation_batch_pairs': s['candidate_count'], 'complete_group_size': group[s['stream_id']],
              'allocation_status': 'PLANNED_NOT_GENERATED', 'pilot_included_in_target': True,
              'first_output_directory': s['first_output_directory'],
              'pilot_train_pairs': p['planned_train_pairs'] if p else None,
              'additional_after_pilot_pairs': allocations[s['stream_id']] - p['planned_train_pairs'] if p else None,
              'pilot_dev_pairs': p['planned_dev_pairs'] if p else None,
              'pilot_exam_pairs': p['planned_exam_pairs'] if p else None,
              'dataset_directory': p['dataset_directory'] if p else f'{root}/datasets/{oid}-{s["profile_id"]}-v1',
              'baseline_directory': p['baseline_directory'] if p else f'{root}/baselines/{s["domain_id"]}/{s["profile_id"]}/{oid}-v1/B1',
              'artifact_status': 'DESIGNATED_PATHS_NOT_EXISTING_WEIGHTS'}
        if p and sc['additional_after_pilot_pairs'] < 0:
            raise ValueError('Pilot exceeds total shared quota: ' + s['stream_id'])
        if not p:
            sc['pilot_rule'] = 'Parent card/native binding assigns a sufficient real-learning pilot before scale; no invented training command.'
        scale_rows.append(sc)
    parent_budget = sum(x['parent_train_budget_pairs'] for x in budget_pools)
    assigned_budget = sum(allocations.values())
    reserve = sum(x['reserve_train_pairs'] for x in budget_pools)
    if parent_budget != cfg['parent_train_budget_pairs'] or assigned_budget + reserve != parent_budget:
        raise ValueError('Parent TRAIN budget changed or allocation is not conserved')
    all_ref_ids = set(ref_by)
    for path in BASE.glob('domains/*/reference-examples*.jsonl'):
        for r in lines(str(path.relative_to(BASE))):
            if r['pair_id'] in all_ref_ids:
                raise ValueError('Duplicate public reference ID: ' + r['pair_id'])
            all_ref_ids.add(r['pair_id'])
    counts = {'parent_topics': len(topics), 'parent_profile_streams': len(generation),
              'objective_cards': len(objectives), 'objective_profile_streams': len(pilots),
              'covered_parent_profile_streams': sum(bool(g['covered_by_objectives']) for g in generation),
              'remaining_parent_profile_streams': len(parent_rows), 'effective_streams': len(execution),
              'physical_cards': len(topics) + len(objectives), 'active_reference_rows': len(all_ref_ids),
              'target_accepted_train_pairs': parent_budget}
    budget_rule = (f'Parent/profile pools total {parent_budget} TRAIN pairs; assigned complete groups {assigned_budget}, '
                   f'unallocated reserve {reserve}. Objective allocations replace covered parent streams; pilot TRAIN is included. '
                   'DEV/EXAM are independent held-out data. All values are planned quotas, not observed generation or learning.')
    order_rule = (f'Explicit user ID wins; otherwise first unfinished stream. Preserve the original {len(cfg["existing_objective_prefix"])} '
                  f'objective streams, then new objectives ({len(pilots)} total), then {len(parent_rows)} uncovered parent streams. '
                  'NEXT uses exact stream IDs and this complete queue. STOP halts actions. Resume existing request/state/handle; do not rerun it.')
    outputs['execution-queue.json'] = dump({'schema_version': 'xiigen.execution-queue.v2', 'stage': 'DATA_PREPARATION_ONLY', 'mode': MODE,
         'completion_rule': 'prepare -> generate real draft.jsonl -> finish structure/count -> DRAFT_CREATED -> next stream; no review, oracle, acceptance, export or training in this stage.',
         'path_authority': 'paths.json', 'work_root': root,
         'source_indexes': ['index.json', 'curriculum/index.json'], 'source_queues': ['curriculum/pilot-queue.json', 'generation-queue.json'],
         'source_order': 'curriculum/refresh-config.json', 'ordering_rule': order_rule, 'counts': counts,
         'budget_rule': budget_rule, 'budget_plan': 'curriculum/scale-plan.json', 'streams': execution})
    outputs['generation-queue.json'] = dump({'schema_version': 'xiigen.generation-queue.v2', 'stage': 'DATA_PREPARATION', 'mode': MODE,
         'source': 'index.json', 'streams': generation, 'execution_queue': 'execution-queue.json',
         'scope': 'Parent/profile catalog only. Covered parents have command=null and replacement_stream_ids; never run them again on top of objectives. Legacy commands are historical.'})
    scope = f'{len(pilots)} objective/profile streams in full {len(execution)}-stream queue. Local null ends the subset, not the full queue. No automatic generation or training.'
    outputs['curriculum/pilot-queue.json'] = dump({'schema_version': 'xiigen.pilot.queue.v3', 'canonical_index': 'curriculum/index.json', 'mode': MODE,
         'scope': scope, 'execution_queue': '../execution-queue.json', 'streams': pilots})
    cidx = read('curriculum/index.json')
    cidx.update(schema_version='xiigen.curriculum.index.v3', date=DATE, pack_files=cfg['pack_files'], mode=MODE,
                objectives=objective_index, stream_order=[{k: p[k] for k in ['order', 'stream_id', 'objective_id', 'topic_id',
                    'profile_id', 'domain_id', 'next_stream_id', 'execution_next_stream_id']} for p in pilots], scope=scope,
                refresh_source='refresh-config.json', counts={'objectives': len(objectives), 'streams': len(pilots),
                    'reference_examples': len(refs), 'initial_train_batch_pairs': sum(p['batch_pairs'] for p in pilots),
                    'planned_pilot_train_pairs': sum(p['planned_train_pairs'] for p in pilots),
                    'planned_pilot_dev_pairs': sum(p['planned_dev_pairs'] for p in pilots),
                    'planned_pilot_exam_pairs': sum(p['planned_exam_pairs'] for p in pilots)})
    outputs['curriculum/index.json'] = dump(cidx)
    idx = read('index.json')
    idx.update(topics=topics, updated_on=DATE, mode=MODE,
               generation_model='Current selected Chinese model in Kiro; no override. Record the observed identity; openness/version are not inferred.',
               scale_gate='CREATE_ONLY: generate bounded candidate batches within the shared quotas. Finish records file structure, then advance. Review and learning pilots are separate future tasks, not gates for creating candidates. Do not claim accepted or trained data.',
               selection_authority=ROLE_CONTRACT['selection'],
               quality_authority=ROLE_CONTRACT['quality_assessment'],
               role_contract=ROLE_CONTRACT,
               counts={'topics': len(topics), 'streams': len(generation), 'target_accepted_pairs': parent_budget, 'target_split': 'train',
                       'dev_exam': 'additional independent splits, assigned before learning', 'effective_execution_streams': len(execution),
                       'physical_cards': counts['physical_cards'], 'active_reference_rows': len(all_ref_ids)},
               microcurriculum={'index': 'curriculum/index.json', 'human_index': 'curriculum/INDEX.md', 'queue': 'curriculum/pilot-queue.json',
                                'relationship': f'{len(objectives)} objectives / {len(pilots)} streams replace {counts["covered_parent_profile_streams"]} parent streams. Shared pools are not duplicated.',
                                'priority': 'Explicit user ID > execution-queue.json; parent/local indexes do not declare full END.'},
               execution_queue={'json': 'execution-queue.json', 'human': 'EXECUTION-QUEUE.md', 'rule': order_rule,
                                'counts': counts, 'budget_plan': 'curriculum/scale-plan.json'})
    outputs['index.json'] = dump(idx)
    outputs['curriculum/scale-plan.json'] = dump({'schema_version': 'xiigen.dataset.scale-plan.v3', 'status': 'PLANNED_NOT_GENERATED', 'mode': MODE,
        'unit': 'accepted TRAIN-pair quotas, not independent scenario counts', 'canonical_queue': 'execution-queue.json', 'allocation_only': True,
        'promotion_rule': 'Candidate generation is CREATE_ONLY and may proceed in assigned batches. Admission and scaling student training require a separately requested semantic review and valid learning pilot. No production integration.',
        'budget_rule': budget_rule,
        'allocation_method': 'Divide each parent/profile pool equally in complete_group_size units; assign affordable remainder to smallest allocation, execution-order tie break. Keep remainder reserve.',
        'budget_pools': budget_pools, 'streams': scale_rows,
        'counts': {'streams': len(execution), 'objective_streams': len(pilots), 'remaining_parent_streams': len(parent_rows),
                   'parent_budget_pools': len(generation), 'parent_train_budget_pairs': parent_budget, 'target_accepted_train_pairs': assigned_budget,
                   'objective_allocated_train_pairs': sum(allocations[s['stream_id']] for s in objective_rows),
                   'remaining_parent_allocated_train_pairs': sum(allocations[s['stream_id']] for s in parent_rows),
                   'unallocated_reserve_train_pairs': reserve,
                   'pilot_train_pairs': sum(p['planned_train_pairs'] for p in pilots),
                   'pilot_dev_pairs': sum(p['planned_dev_pairs'] for p in pilots),
                   'pilot_exam_pairs': sum(p['planned_exam_pairs'] for p in pilots),
                   'pilot_quota_scope': f'Known quotas for {len(pilots)} objective streams; parent pilots need their native binding.',
                   'generated_pairs_observed_this_update': 0, 'accepted_pairs_observed_this_update': 0}})
    # Human indexes and command tails derive from the same execution list.
    head = (f'# Единая очередь подготовки DPO\n\nКанонический порядок: **{len(execution)} потоков** — {len(pilots)} узких и {len(parent_rows)} непокрытых родительских. '
            f'**{counts["physical_cards"]} карточки, {len(all_ref_ids)} публичные reference-строки**. Это материал подготовки, не обученный корпус.\n\n'
            'Прямой stream ID пользователя выше очереди. NEXT означает немедленный переход к названному ID; STOP прекращает действия. '
            f'Прежние {len(cfg["existing_objective_prefix"])} objective-потоков и first_run ID сохранены. Живую попытку продолжать по её request/state/handle.\n\n'
            'Новые команды используют `tools/batches.py`, текущую модель Kiro и локальный RAG плановой ветки. '
            'Режим **CREATE_ONLY**: подготовить prompt, создать реальные строки, выполнить структурный finish, перейти к следующему потоку. '
            'Review/oracle/accept/export/обучение сейчас не запускаются. prepare сам не генерирует данные. '
            f'Корень наборов: `{root}`. Команды через create_sets.ps1 работают из любого каталога. Старые `legacy_*` команды служат только истории.\n\n'
            f'{budget_rule}\n\n[Общий промпт](prompts/MASTER-PROMPT-RU.md) · [Бюджет](curriculum/scale-plan.json) · [Воспроизводимое обновление](tools/refresh_indexes.py)\n\n'
            '| Порядок | Поток | Карточка | Первая партия | Следующий поток |\n|---:|---|---|---:|---|\n')
    outputs['EXECUTION-QUEUE.md'] = head + ''.join(f'| {s["order"]} | `{s["stream_id"]}` | [{s.get("objective_id") or s["topic_id"]}]({s["card"]}) | {s["candidate_count"]} | `{s["next_stream_id"]}` |\n' for s in execution)
    gh = (f'# Родительский каталог и команды подготовки\n\n{len(generation)} родительских пулов; фактический порядок — [EXECUTION-QUEUE.md](EXECUTION-QUEUE.md). '
          'Для покрытого parent указаны заменяющие микроцели: его общий набор повторно не генерируется. '\
          'RAG находится внутри плановой ветки; `batches.py prepare` только создаёт задание.\n\n')
    for g in generation:
        gh += f'## {g["sequence"]}. {g["stream_id"]}\n\n[{topic_by[g["topic_id"]]["title"]}]({g["card"]}) · `{g["domain_id"]}`.\n\n'
        if g['covered_by_objectives']:
            gh += 'Выполнять узкие потоки: ' + ', '.join('`' + s + '`' for s in g['replacement_stream_ids']) + '. Родительская квота общая.\n\n'
        else:
            gh += '```powershell\n' + g['command'] + '\n```\n\n'
    outputs['GENERATION-QUEUE.md'] = gh
    ih = (f'# Индекс узких заданий для Kiro\n\n**{len(objectives)} микроцели, {len(pilots)} потоков, {len(refs)} публичных reference-строк**. '
          f'Они входят в общий пакет {counts["physical_cards"]} карточек / {len(execution)} потоков. История и старые опоры сохранены.\n\n'
          '[Общий промпт](../prompts/MASTER-PROMPT-RU.md) · [Полная очередь](../EXECUTION-QUEUE.md) · [Точные команды](pilot-queue.json) · [Доли бюджета](scale-plan.json). '
          'Первый batch включён в TRAIN-квоту. Подготовка, генерация, предметный oracle и обучение имеют разные статусы.\n\n'
          '## Карточки\n\n| ID | Родитель | Узкий навык | Профилей | Reference |\n|---|---|---|---:|---:|\n')
    for o in objective_index:
        ih += f'| [{o["id"]}](cards/{o["id"]}.md) | {o["topic_id"]} | {o["title_ru"]} | {len(o["profiles"])} | {len(objectives[o["id"]]["examples"])} |\n'
    ih += '\n## Переходы полной очереди для этого подмножества\n\n| Общий порядок | Поток | Следующий поток |\n|---:|---|---|\n'
    ih += ''.join(f'| {p["order"]} | `{p["stream_id"]}` | `{p["execution_next_stream_id"]}` |\n' for p in pilots)
    outputs['curriculum/INDEX.md'] = ih
    for folder in ['translation', 'documentation', 'coding', 'orchestration']:
        path = f'domains/{folder}/README.md'
        domain_topics = [t for t in topics if t['card'].startswith('domains/' + folder + '/')]
        parent_ids = {t['id'] for t in domain_topics}
        domain_objs = [o for o in objective_index if o['topic_id'] in parent_ids]
        text = (BASE / path).read_text(encoding='utf-8')
        text = re.sub(r'Каталог: \d+ родительских тем и \d+ микроцелей\.',
                      f'Каталог: {len(domain_topics)} родительских тем и {len(domain_objs)} микроцелей.', text)
        table = '| ID | Учебная граница | Reference |\n|---|---|---:|\n' + ''.join(
            f'| [{o["id"]}](../../curriculum/cards/{o["id"]}.md) | {o["title_ru"]} | {len(objectives[o["id"]]["examples"])} |\n'
            for o in domain_objs)
        text = re.sub(r'(## Узкие задания\n\n)(?:\|[^\n]*\n)+', lambda m: m.group(1) + table, text)
        outputs[path] = text
    for ident, card in [(t['id'], t['card']) for t in topics] + [(o['id'], o['card']) for o in objective_index]:
        cp = BASE / card
        if not cp.is_file():
            raise ValueError('Missing card: ' + card)
        selected = [s for s in execution if (s['objective_id'] == ident if ident in objectives else s['topic_id'] == ident)]
        tail = (BEGIN + '\n## Переходы единой очереди\n\n'
                'Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. '
                'Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. '
                'Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. '
                'При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.\n\n'
                'Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. '
                'Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. '
                'Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.\n\n'
                '| Поток | Общий порядок | Следующий поток |\n|---|---:|---|\n')
        tail += ''.join(f'| `{s["stream_id"]}` | {s["order"]} | `{s["next_stream_id"]}` |\n' for s in selected)
        tail += f'\n## Точные команды действующих потоков\n\nПодготовка: `{preparation_root}`. Команды работают из любого каталога. RAG уже внутри пакета. '
        if ident not in objectives and any(s['kind'] == 'objective' for s in selected):
            tail += 'Эта родительская карточка уточнена микроцелями: исполнять их команды ниже, не прибавлять отдельный родительский корпус.\n\n'
        else:
            tail += 'Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.\n\n'
        for s in selected:
            tail += f'### {s["stream_id"]}\n\n```powershell\n{s["command"]}\n'
            p = pilot_by.get(s['stream_id'])
            tail += '```\n\n'
            if p and p['prepare_first_dev'] is not None and p['prepare_first_exam'] is not None:
                tail += ('DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, '
                         'точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.\n\n')
            elif p:
                tail += ('Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; '
                         '`prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать '
                         'непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. '
                         'Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.\n\n')
            tail += f'Первая TRAIN-папка: `{s["first_output_directory"]}`. Следующий поток: `{s["next_stream_id"]}`. '
            sc = next(x for x in scale_rows if x['stream_id'] == s['stream_id'])
            tail += f'Доля TRAIN: {sc["target_accepted_train_pairs"]} пар; первая партия {s["candidate_count"]}. '
            if p:
                tail += f'Пилот TRAIN/DEV/EXAM: {p["planned_train_pairs"]}/{p["planned_dev_pairs"]}/{p["planned_exam_pairs"]}; '
            tail += f'набор `{sc["dataset_directory"]}`; B1 `{sc["baseline_directory"]}`. Это назначенные пути, не существующие веса.\n\n'
        tail += END + '\n'
        outputs[card] = with_card_mode(strip_generated(cp.read_text(encoding='utf-8')), root, preparation_root) + tail
    summary = {**counts, 'curriculum_reference_rows': len(refs), 'allocated_train_pairs': assigned_budget,
               'reserve_train_pairs': reserve, 'preserved_objective_prefix': len(cfg['existing_objective_prefix']),
               'first_run_ids_preserved': True, 'learning_status': 'NOT_RUN', 'generator_status': 'NOT_RUN'}
    return outputs, summary


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument('--write', action='store_true')
    mode.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs, summary = build()
    changed = [path for path, content in outputs.items()
               if not (BASE / path).exists() or (BASE / path).read_text(encoding='utf-8') != content]
    if args.write:
        for path in changed:
            (BASE / path).parent.mkdir(parents=True, exist_ok=True)
            (BASE / path).write_text(outputs[path], encoding='utf-8')
    print(json.dumps({'mode': 'write' if args.write else 'check', 'changed_files': len(changed),
                      'paths': changed, 'summary': summary}, ensure_ascii=False, indent=2))
    if args.check and changed:
        raise SystemExit(1)


if __name__ == '__main__':
    main()

