#!/usr/bin/env python3
"""Offline DPO preparation. Does not call a model, train, or modify product code."""
import argparse
import hashlib
import json
import math
import os
from pathlib import Path
import random
import re
import secrets
import sys
from urllib.parse import unquote
from collections import Counter
from functools import lru_cache

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')
BASE = Path(__file__).resolve().parents[1]
FIELDS = ['schema_version','pair_id','domain_id','topic_id','profile_id','family_id',
          'split','prompt','chosen','rejected','error_tag','rationale','source_refs','provenance','metadata']
CODEX_PROFILE = 'astra 6 ultracode'
ACCEPTANCE_CONTRACT = 'xiigen.preparation.acceptance.v2'
AUTHORITY_EVIDENCE_SCOPE = 'declared_evidence_only'

def authority_template(kind, request_path, input_path=None, review_path=None, blind_path=None):
    """An incomplete receipt, never a fabricated model call or acceptance."""
    result = {'schema_version':'xiigen.codex-authority.v1','kind':kind,
        'actor':'CODEX','required_profile':CODEX_PROFILE,'observed_profile':None,'observed_model':None,
        'evidence_scope':AUTHORITY_EVIDENCE_SCOPE,
        'profile_verified':False,'evidence_ref':None,'decision':'PENDING',
        'request_sha256':digest(request_path)}
    if input_path: result['input_sha256']=digest(input_path)
    if review_path: result['review_sha256']=digest(review_path)
    elif kind=='review': result['review_sha256']=None
    if blind_path: result['blind_sha256']=digest(blind_path)
    elif kind=='review': result['blind_sha256']=None
    return result

def authority_errors(path, kind, request_path, input_path=None, review_path=None, blind_path=None):
    """Check declared provenance and byte bindings; this is not provider authentication."""
    if not path: return ['MISSING_CODEX_'+kind.upper()+'_RECEIPT']
    value=obj(path); errors=[]; prefix='CODEX_'+kind.upper()+'_'
    if not isinstance(value,dict): return [prefix+'SCHEMA']
    if value.get('schema_version')!='xiigen.codex-authority.v1' or value.get('kind')!=kind:
        errors.append(prefix+'SCHEMA')
    if value.get('actor')!='CODEX' or value.get('required_profile')!=CODEX_PROFILE:
        errors.append(prefix+'WRONG_AUTHORITY')
    if value.get('observed_profile')!=CODEX_PROFILE: errors.append(prefix+'WRONG_OBSERVED_PROFILE')
    if value.get('evidence_scope')!=AUTHORITY_EVIDENCE_SCOPE: errors.append(prefix+'EVIDENCE_SCOPE')
    if value.get('profile_verified') is not True or not isinstance(value.get('observed_model'),str) or not value['observed_model'].strip() or not isinstance(value.get('evidence_ref'),str) or not value['evidence_ref'].strip():
        errors.append(prefix+'IDENTITY_NOT_EVIDENCED')
    expected='SELECTED' if kind=='selection' else 'REVIEW_COMPLETED'
    if value.get('decision')!=expected: errors.append(prefix+'DECISION_PENDING')
    if kind=='review' and not blind_path: errors.append(prefix+'MISSING_BLIND_BINDING')
    for field, target in [('request',request_path),('input',input_path),('review',review_path),('blind',blind_path)]:
        if target and value.get(field+'_sha256')!=digest(target): errors.append(prefix+field.upper()+'_HASH_MISMATCH')
    return errors

def make_blind_packet(data, positions):
    """Reconstruct the exact A/B packet; the private key is never sent to CODEX."""
    return [{'pair_id':r['pair_id'],'prompt':r['prompt'],
             'context':{k:r[k] for k in ['domain_id','topic_id','profile_id','family_id','split','source_refs']},
             'lineage':{k:r['metadata'].get(k) for k in ['origin_family_id','leakage_group_ids','stack','simulation','learning_objective_id']},
             'A':r['chosen'] if positions[r['pair_id']]=='A' else r['rejected'],
             'B':r['chosen'] if positions[r['pair_id']]=='B' else r['rejected']} for r in data]

def blind_errors(input_path, key_path, blind_path):
    """Detect stale/swapped blind packets and key edits, not authenticate a provider."""
    if not blind_path: return ['SEMANTIC_ACCEPTANCE_REQUIRES_BLIND_PACKET']
    key=obj(key_path); errors=[]
    if not isinstance(key,dict): return ['BLIND_KEY_SCHEMA']
    if key.get('input_sha256')!=digest(input_path): errors.append('BLIND_KEY_INPUT_HASH_MISMATCH')
    if key.get('blind_sha256')!=digest(blind_path): errors.append('BLIND_KEY_PACKET_HASH_MISMATCH')
    data=rows(input_path); positions=key.get('chosen_positions')
    if not isinstance(positions,dict) or set(positions)!=set(r['pair_id'] for r in data):
        return errors+['BLIND_KEY_COVERAGE_MISMATCH']
    if any(v not in ['A','B'] for v in positions.values()):
        return errors+['BLIND_KEY_INVALID_POSITIONS']
    expected=jsonl(make_blind_packet(data,positions)).encode('utf-8')
    if Path(blind_path).read_bytes()!=expected: errors.append('BLIND_PACKET_RECONSTRUCTION_MISMATCH')
    return errors

def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

def strict_json(text):
    """Reject ambiguity before any training text or provenance can be replaced."""
    def unique(pairs):
        result={}
        for key,value in pairs:
            if key in result: raise ValueError('DUPLICATE_JSON_KEY '+key)
            result[key]=value
        return result
    def finite(value):
        raise ValueError('NON_FINITE_JSON_NUMBER '+value)
    def finite_float(value):
        result=float(value)
        if not math.isfinite(result): return finite(value)
        return result
    return json.loads(text,object_pairs_hook=unique,parse_constant=finite,parse_float=finite_float)

def obj(path):
    return strict_json(Path(path).read_text(encoding='utf-8-sig'))

def write(path, value, exclusive=True):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('x' if exclusive else 'w', encoding='utf-8', newline='\n') as f:
        f.write(value if isinstance(value,str) else json.dumps(value,ensure_ascii=False,indent=2)+'\n')

def rows(path):
    found=[]
    for n,line in enumerate(Path(path).read_text(encoding='utf-8-sig').splitlines(),1):
        if line.strip():
            try:
                value=strict_json(line)
                if not isinstance(value,dict): raise ValueError('row is not an object')
            except (ValueError,TypeError) as exc:
                raise ValueError(f'{path}:{n}: invalid JSONL: {exc}') from exc
            found.append(value)
    return found

def jsonl(data):
    return ''.join(json.dumps(x,ensure_ascii=False,separators=(',',':'))+'\n' for x in data)

@lru_cache(maxsize=1)
def registry():
    data=obj(BASE/'index.json')
    return {x['id']:x for x in data['topics']}

@lru_cache(maxsize=1)
def objectives():
    index=obj(BASE/'curriculum/index.json')
    result={}
    for name in index['pack_files']:
        for entry in obj(BASE/'curriculum'/name)['objectives']:
            oid=entry['objective_id']
            if oid in result: raise ValueError('DUPLICATE_OBJECTIVE '+oid)
            result[oid]=entry
    return result

@lru_cache(maxsize=1)
def public_reference_index():
    """Known public seeds cannot become a closed examination by relabelling."""
    references=[]
    for path in sorted((BASE/'domains').glob('*/reference-examples*.jsonl')):
        references.extend(rows(path))
    for path in sorted((BASE/'quarantine').glob('*.jsonl')):
        references.extend(rows(path))
    for lesson in objectives().values(): references.extend(lesson['examples'])
    families=set(); prompts=set()
    for r in references:
        if r.get('family_id'): families.add(r['family_id'])
        if r.get('prompt'): prompts.add(r['prompt'])
        metadata=r.get('metadata',{})
        if metadata.get('origin_family_id'): families.add(metadata['origin_family_id'])
        families.update(metadata.get('leakage_group_ids',[]))
    return families,prompts

@lru_cache(maxsize=1)
def source_catalog():
    data=obj(BASE/'sources.lock.json')['sources']
    return {r['source_id']:r for r in data}

@lru_cache(maxsize=256)
def source_id(ref):
    if not isinstance(ref,str): return None
    for sid,r in source_catalog().items():
        if ref==sid or unquote(ref.split('#')[0])==unquote(r['source_url']): return sid
    return None

def family_split(family):
    n=int(hashlib.sha256(family.encode('utf-8')).hexdigest()[:8],16)%100
    return 'train' if n<80 else ('dev' if n<90 else 'exam')

def norm(text):
    # Code/string literals, newlines and Unicode code points can be the lesson.
    # Semantic equivalence belongs to the reviewer, not whitespace collapsing.
    return text

def valid_id(text):
    if not re.fullmatch(r'[A-Za-z0-9_.-]{1,60}',text):
        raise ValueError(f'Unsafe or long ID: {text!r}')
    return text

def family_plan_errors(value):
    """Validate lineage across the whole plan, before filtering one topic/split."""
    if not isinstance(value,dict) or not isinstance(value.get('families'),list):
        return ['FAMILY_PLAN_ARRAY_REQUIRED']
    errors=[]; seen=set(); groups={}
    for entry in value['families']:
        if not isinstance(entry,dict):
            errors.append('FAMILY_PLAN_OBJECT_REQUIRED'); continue
        fid=entry.get('family_id'); origin=entry.get('origin_family_id')
        split=entry.get('split'); linked=entry.get('leakage_group_ids')
        if any(not isinstance(x,str) or not x.strip() for x in [fid,origin]) or split not in ['train','dev','exam']:
            errors.append('FAMILY_PLAN_ID_OR_SPLIT'); continue
        if fid in seen: errors.append('FAMILY_PLAN_DUPLICATE_ID '+fid)
        seen.add(fid)
        if not isinstance(linked,list) or not linked or not all(isinstance(x,str) and x.strip() for x in linked):
            errors.append('FAMILY_PLAN_LINEAGE '+fid); continue
        for key in ['topics','profiles']:
            if not isinstance(entry.get(key),list) or not entry[key] or not all(isinstance(x,str) and x.strip() for x in entry[key]):
                errors.append('FAMILY_PLAN_'+key.upper()+' '+fid)
        if not entry.get('scenario') and not entry.get('scenario_contract'):
            errors.append('FAMILY_PLAN_VISIBLE_SCENARIO_REQUIRED '+fid)
        for group in set([fid,origin]+linked):
            if groups.setdefault(group,split)!=split:
                errors.append('FAMILY_PLAN_SPLIT_LEAKAGE '+group)
    return errors

def validate_rows(data, allow_reference=False, request=None):
    errors=[]; ids=set(); families={}; groups={}; prompts={}; triples=set(); reg=registry()
    if not data: return ['EMPTY_DATASET']
    for n,r in enumerate(data,1):
        prefix=f'row {n} {r.get("pair_id","?")}: '
        def error(s): errors.append(prefix+s)
        missing=[k for k in FIELDS if k not in r]
        if missing: error('MISSING_FIELDS '+','.join(missing)); continue
        if r['schema_version']!='xiigen.dpo.v1': error('SCHEMA_VERSION')
        if any(not isinstance(r[k],str) or not r[k].strip() for k in FIELDS[:12]):
            error('EMPTY_OR_NONSTRING_CORE_FIELD'); continue
        if not isinstance(r['metadata'],dict) or not isinstance(r['provenance'],dict):
            error('METADATA_PROVENANCE_OBJECT_REQUIRED'); continue
        objective=r['metadata'].get('learning_objective_id')
        if objective and not isinstance(objective,str):
            error('OBJECTIVE_ID_MUST_BE_STRING'); objective=None
        if objective:
            lesson=objectives().get(objective)
            if not lesson or lesson['topic_id']!=r['topic_id'] or r['profile_id'] not in lesson['profiles']:
                error('OBJECTIVE_TOPIC_PROFILE_MISMATCH')
        if r['pair_id'] in ids: error('DUPLICATE_PAIR_ID')
        ids.add(r['pair_id'])
        t=reg.get(r['topic_id'])
        if t is None: error('UNKNOWN_TOPIC')
        else:
            if r['domain_id'] not in t['domain_ids']: error('DOMAIN_NOT_IN_TOPIC')
            if r['profile_id'] not in t['profiles']: error('PROFILE_NOT_IN_TOPIC')
            if r['domain_id'].startswith('tr.') and r['domain_id']!='tr.'+r['profile_id'].split('-')[-1]:
                error('TRANSLATION_TARGET_MISMATCH')
        if r['split'] not in ['train','dev','exam','reference']: error('BAD_SPLIT')
        if r['split']=='reference' and not allow_reference: error('REFERENCE_NOT_TRAINING')
        if r['split']!='reference' and r['metadata'].get('training_allowed') is False:
            error('TRAINING_EXPLICITLY_DISALLOWED')
        old=families.setdefault(r['family_id'],r['split'])
        if old!=r['split']: error('FAMILY_SPLIT_LEAKAGE')
        linked=r['metadata'].get('leakage_group_ids',[])
        if not isinstance(linked,list) or not all(isinstance(g,str) and g.strip() for g in linked):
            error('LEAKAGE_GROUP_IDS_MUST_BE_STRING_ARRAY');linked=[]
        origin=r['metadata'].get('origin_family_id')
        if 'origin_family_id' in r['metadata'] and (not isinstance(origin,str) or not origin.strip()):
            error('ORIGIN_FAMILY_ID_MUST_BE_NONEMPTY_STRING');origin=None
        if origin is None and r['split']!='reference': error('ORIGIN_FAMILY_REQUIRED')
        # An origin can also be another row's family ID or declared group ID.
        lineage={r['family_id']}|set(linked)
        if origin is not None: lineage.add(origin)
        for group in sorted(lineage):
            if groups.setdefault(group,r['split'])!=r['split']: error('RELATED_FAMILY_SPLIT_LEAKAGE')
        if r['split'] in ['dev','exam']:
            public_families,public_prompts=public_reference_index()
            if lineage & public_families or r['prompt'] in public_prompts:
                error('PUBLIC_REFERENCE_IN_HELD_OUT')
        for field in ['chosen','rejected']:
            if re.search(r'\[(WRONG|REJECTED|CHOSEN|CORRECT|NEGATIVE)\b',r[field],re.I):
                error('LABEL_LEAKAGE_'+field)
        if norm(r['chosen'])==norm(r['rejected']): error('IDENTICAL_ANSWERS')
        if not isinstance(r['source_refs'],list) or not r['source_refs'] or not all(isinstance(x,str) for x in r['source_refs']):
            error('SOURCE_REFS_REQUIRED')
        elif any(source_id(x) is None for x in r['source_refs']):
            error('SOURCE_NOT_IN_PINNED_CATALOG')
        if not r['provenance'].get('kind') or not r['provenance'].get('generator_model'):
            error('PROVENANCE_INCOMPLETE')
        if r['split']!='reference' and r['provenance'].get('generator_model')=='reference_authoring_not_a_kiro_run':
            error('REFERENCE_RELABELLED_AS_GENERATED')
        p=(r['domain_id'],r['profile_id'],norm(r['prompt']))
        prev=prompts.setdefault(p,(norm(r['chosen']),r['family_id'],r['split']))
        if prev[2]!=r['split']: error('PROMPT_SPLIT_LEAKAGE')
        if prev[0]!=norm(r['chosen']): error('CONFLICTING_CHOSEN_REQUIRES_REVIEW')
        triple=(p,norm(r['chosen']),norm(r['rejected']))
        if triple in triples: error('DUPLICATE_TRIPLE')
        triples.add(triple)
        if request:
            for k in ['topic_id','profile_id','domain_id','split']:
                if r[k]!=request[k]: error('REQUEST_MISMATCH_'+k)
            if request.get('objective_id') and objective!=request['objective_id']:
                error('REQUEST_MISMATCH_learning_objective_id')
            if r['family_id'] not in request['family_ids']: error('UNASSIGNED_FAMILY')
            contract=request.get('family_contracts',{}).get(r['family_id'])
            if contract:
                if r['metadata'].get('origin_family_id')!=contract['origin_family_id']: error('ORIGIN_FAMILY_MISMATCH')
                if sorted(linked)!=sorted(contract['leakage_group_ids']): error('LINEAGE_GROUP_MISMATCH')
            if isinstance(r['source_refs'],list) and all(isinstance(x,str) for x in r['source_refs']) and any(source_id(x) not in [source_id(y) for y in request['sources']] for x in r['source_refs']): error('SOURCE_OUTSIDE_REQUEST')
    if request:
        if len(data)!=request['count']: errors.append('REQUEST_COUNT_MISMATCH')
        if set(r.get('family_id') for r in data)!=set(request['family_ids']):
            errors.append('REQUEST_FAMILY_COVERAGE_MISMATCH')
        if request.get('pairs_per_family')==2:
            field=request.get('family_class_field','clarification_class')
            labels=request.get('family_class_values',['ambiguous','resolved'])
            for family in request['family_ids']:
                values=[r.get('metadata',{}).get(field) for r in data if r.get('family_id')==family]
                if any(not isinstance(value,str) for value in values):
                    errors.append('CONDITIONAL_FAMILY_CLASS_REQUIRED '+family); continue
                classes=Counter(values)
                if classes!=Counter(labels): errors.append('CONDITIONAL_FAMILY_CLASS_BALANCE '+family)
        if request.get('route_class_counts'):
            counts=Counter()
            for r in data:
                try:
                    answer=strict_json(r['chosen'])
                    label=answer.get('domain_id') if answer.get('action')=='route' else ('clarify' if answer.get('action')=='clarify' and answer.get('domain_id') is None else None)
                    counts[label]+=1
                except (ValueError,AttributeError,TypeError): errors.append('ROUTE_JSON_REQUIRED')
            if dict(counts)!=request['route_class_counts']: errors.append('ROUTE_CLASS_BALANCE_MISMATCH')
    return errors

def validation_path(path):
    p=Path(path)
    return p.with_name('validation.json' if p.name=='draft.jsonl' else p.stem+'.validation.json')

def prepare(a):
    objective=getattr(a,'objective',None)
    reg=registry(); t=reg.get(a.topic)
    if not t: raise ValueError('Unknown topic: '+a.topic)
    if a.profile not in t['profiles']: raise ValueError('Unknown profile for topic')
    lesson=objectives().get(objective) if objective else None
    if objective and (not lesson or lesson['topic_id']!=a.topic or a.profile not in lesson['profiles']):
        raise ValueError('Objective does not match topic/profile')
    for v in [a.topic,a.profile,a.run]: valid_id(v)
    if a.count<1 or a.count>50 or a.start<0: raise ValueError('count 1..50, start >=0')
    pairs_per_family=lesson.get('pairs_per_family',1) if lesson else 1
    if a.count%pairs_per_family: raise ValueError('count must be divisible by '+str(pairs_per_family)+'; conditional lessons need ambiguous+resolved for every family')
    family_count=a.count//pairs_per_family
    if objective=='OR-ROUTE-INTENT' and a.count%21:
        raise ValueError('OR-ROUTE-INTENT requires count 21 or 42: all 20 routes plus clarify in every batch')
    ragroot=Path(a.rag_root); manifest=obj(ragroot/'rag/manifest.json')
    sources=manifest.get('sources',manifest.get('resources',[])) if isinstance(manifest,dict) else manifest
    byid={x['source_id']:x for x in sources}
    chunks=[]
    assigned_sources=lesson['source_refs'] if lesson else t['source_refs']
    for ref in assigned_sources:
        sid=source_id(ref)
        if sid not in byid: raise ValueError('Missing RAG source: '+ref)
        p=ragroot/'rag/sources'/f'{sid}.md'
        if digest(p)!=source_catalog()[sid]['local_excerpt_sha256']: raise ValueError('Changed RAG slice: '+sid)
        chunks.append(p.read_text(encoding='utf-8'))
    scope=objective or a.topic
    ns='tr/'+scope if t['domain_ids'][0].startswith('tr.') else scope+'/'+a.profile
    selected=[]; candidate=a.start; contracts={}
    if a.family_plan:
        plan=obj(a.family_plan); problems=family_plan_errors(plan)
        if problems: raise ValueError('\n'.join(problems))
        available=plan['families']
        eligible=[x for x in available if a.topic in x['topics'] and a.profile in x['profiles'] and x['split']==a.split
                  and (not objective or objective in x.get('objective_ids',[objective]))]
        picked=eligible[a.start:a.start+family_count]
        if len(picked)!=family_count: raise ValueError('Family plan does not contain requested number of eligible families')
        for x in picked:
            selected.append(x['family_id']); contracts[x['family_id']]=x
        candidate=a.start+family_count
    else:
        while len(selected)<family_count:
            family=f'v1/{ns}/F{candidate:07d}'
            candidate+=1
            if family_split(family)==a.split:
                selected.append(family)
                contracts[family]={'origin_family_id':family,'leakage_group_ids':[family],
                    'scenario_rule':'A genuinely new self-contained simulation, not a renamed prior scenario.'}
    domain='tr.'+a.profile.split('-')[-1] if t['domain_ids'][0].startswith('tr.') else t['domain_ids'][0]
    if len(set(selected))!=len(selected): raise ValueError('Family plan contains duplicate family IDs')
    out=Path(a.work_root)/a.topic/a.profile/a.run
    if out.exists(): raise ValueError(f'Run already exists; inspect its state, no overwrite/restart: {out}')
    card_path='curriculum/cards/'+objective+'.md' if lesson else t['card']
    request={'schema_version':'xiigen.request.v2' if lesson else 'xiigen.request.v1','topic_id':a.topic,'domain_id':domain,'profile_id':a.profile,
        'run_id':a.run,'split':a.split,'count':a.count,'family_ids':selected,'next_start':candidate,
        'family_contracts':contracts,'family_plan_sha256':digest(a.family_plan) if a.family_plan else None,
        'model_selector':'current selected external open Chinese model in Kiro; no override',
        'selection_authority':{'actor':'CODEX','required_profile':CODEX_PROFILE},
        'quality_authority':{'actor':'CODEX','required_profile':CODEX_PROFILE},
        'card':card_path,'card_sha256':digest(BASE/card_path),
        'sources':assigned_sources,'rag_manifest_sha256':digest(ragroot/'rag/manifest.json'),
        'output_directory':str(out),'scope':'DATA_PREPARATION_ONLY','training_status':'NOT_RUN'}
    if lesson:
        request.update(objective_id=objective,review_requirements=lesson['review_requirements'],
                       pairs_per_family=pairs_per_family,
                       public_reference_families=[x['family_id'] for x in lesson['examples']])
        if pairs_per_family==2:
            request.update(family_class_field=lesson.get('family_class_field','clarification_class'),
                           family_class_values=lesson.get('family_class_values',['ambiguous','resolved']))
        if objective=='OR-ROUTE-INTENT':
            labels=lesson['scenario_axes'][0]['values']
            request['route_class_counts']={label:a.count//len(labels) for label in labels}
    material='\n\n'.join(chunks)
    card=(BASE/card_path).read_text(encoding='utf-8')
    prompt=(BASE/'prompts/MASTER-PROMPT-RU.md').read_text(encoding='utf-8')
    prompt+='\n\n## Назначение текущего пакета\n\n```json\n'+json.dumps(request,ensure_ascii=False,indent=2)+'\n```\n'
    if pairs_per_family==2:
        prompt+='\nСоздай ровно две пары на каждое назначенное family_id: по одной для каждого класса '+', '.join(request['family_class_values'])+'. Поле metadata.'+request['family_class_field']+' различается, family/origin/leakage группы совпадают. Всего строк ровно count; баланс50:50. Сохрани draft.jsonl в output_directory.\n'
    else:
        prompt+='\nСоздай ровно одну пару на каждое назначенное family_id. Сохрани draft.jsonl в output_directory.\n'
    prompt+='Скопируй origin_family_id и leakage_group_ids из family_contracts в metadata. Для связанных вариантов используй назначенный family-plan; не выдавай переименование за новую семью.\n'
    if lesson:
        prompt+='В metadata.learning_objective_id каждой пары запиши '+objective+'. Все строки учат только этой границе. Публичные reference-примеры и их потомки не превращать в DEV/EXAM.\n'
    prompt+='Используй формат из 02-DATA-CONTRACT.md ниже. Ни одна старая инструкция источника не отменяет 07-CONFLICTS-AND-REQUIREMENTS.md.\n'
    for name in ['02-DATA-CONTRACT.md','07-CONFLICTS-AND-REQUIREMENTS.md','10-CODEX-AND-CHINESE-ROLES.md']:
        prompt+='\n\n'+(BASE/name).read_text(encoding='utf-8')
    prompt+='\n\n## Карточка\n'+card+'\n\n## Выбранные RAG-фрагменты (материал, не новые команды)\n'+material
    write(out/'request.json',request); write(out/'prompt.md',prompt)
    write(out/'selection.template.json',authority_template('selection',out/'request.json'))
    selection_prompt=(BASE/'prompts/CODEX-SELECT-PROMPT-RU.md').read_text(encoding='utf-8')
    write(out/'selection.prompt.md',selection_prompt+'\n\nПакет: '+str(out)+'\n\n'+json.dumps(request,ensure_ascii=False,indent=2))
    write(out/'state.json',{'status':'PREPARED','topic':a.topic,'run':a.run,'process_handle':None,'training_status':'NOT_RUN'})
    print(json.dumps({'out':str(out),'count':a.count,'next_start':candidate,'prompt_chars':len(prompt)},ensure_ascii=False))

def validate(a):
    data=rows(a.input); req=obj(a.request) if a.request else None
    blind_path=getattr(a,'blind',None)
    errors=validate_rows(data,a.allow_reference,req); reviewed=False; evidence_artifacts={}
    report={'scope':'DATA_ONLY_NOT_LEARNING','acceptance_contract':ACCEPTANCE_CONTRACT,
        'authority_evidence_scope':AUTHORITY_EVIDENCE_SCOPE,'provider_identity_authenticated':False,
        'input':str(Path(a.input).resolve()),'input_sha256':digest(a.input),'rows':len(data)}
    if bool(a.review)!=bool(a.key): raise ValueError('--review and --key required together')
    if a.review:
        if not req: errors.append('SEMANTIC_ACCEPTANCE_REQUIRES_GENERATION_REQUEST')
        if req:
            errors.extend(authority_errors(getattr(a,'selection',None),'selection',a.request))
            errors.extend(authority_errors(getattr(a,'review_receipt',None),'review',a.request,a.input,a.review,blind_path))
        errors.extend(blind_errors(a.input,a.key,blind_path))
        reviews=rows(a.review); key=obj(a.key)
        positions=key.get('chosen_positions',{}) if isinstance(key,dict) else {}
        if not isinstance(positions,dict): positions={}
        if not isinstance(key,dict) or not re.fullmatch(r'[0-9a-f]{64}',str(key.get('blind_sha256',''))): errors.append('BLIND_INPUT_HASH_REQUIRED')
        byid={r.get('pair_id'):r for r in reviews}
        if len(byid)!=len(reviews) or set(byid)!=set(r['pair_id'] for r in data): errors.append('REVIEW_COVERAGE_MISMATCH')
        for r in data:
            v=byid.get(r['pair_id'],{}); checks=v.get('checks',{})
            ok=isinstance(checks,dict) and all(checks.get(k) is True for k in ['semantic','sufficient_context','single_error','version','family'])
            pending=['needs_oracle','needs_language_review']
            if not isinstance(checks,dict) or any(checks.get(x) is not False for x in pending):
                errors.append(r['pair_id']+': EXPLICIT_REVIEW_COMPLETION_REQUIRED')
            if v.get('valid') is not True or v.get('winner') not in ['A','B'] or v.get('winner')!=positions.get(r['pair_id']) or not ok or not v.get('reason'):
                errors.append(r['pair_id']+': SEMANTIC_REVIEW_NOT_ACCEPTED')
            needed='language_review' if r['domain_id'].startswith('tr.') else ('oracle' if r['domain_id'].startswith(('code.','docs.')) else None)
            if needed:
                evidence_map=v.get('evidence',{})
                evidence=evidence_map.get(needed,{}) if isinstance(evidence_map,dict) else {}
                if not isinstance(evidence,dict) or evidence.get('status')!='passed' or not evidence.get('path') or not evidence.get('sha256'):
                    errors.append(r['pair_id']+': '+needed.upper()+'_EVIDENCE_REQUIRED')
                else:
                    artifact=Path(evidence['path'])
                    if not artifact.is_absolute(): artifact=Path(a.review).parent/artifact
                    if not artifact.is_file() or digest(artifact)!=evidence['sha256']:
                        errors.append(r['pair_id']+': '+needed.upper()+'_EVIDENCE_HASH_MISMATCH')
                    else:
                        proof=obj(artifact)
                        if not isinstance(proof,dict): raise ValueError('Review evidence must be an object')
                        evidence_artifacts[str(artifact.resolve())]=digest(artifact)
                        if proof.get('status')!='completed' or proof.get('scope')!='DATA_REVIEW_NOT_LEARNING' or proof.get('blind_input_sha256')!=key.get('blind_sha256') or not proof.get('method') or not proof.get('reviewer'):
                            errors.append(r['pair_id']+': '+needed.upper()+'_EVIDENCE_BINDING_MISMATCH')
                        if needed=='oracle':
                            mode=proof.get('verification_mode','execution')
                            if mode=='source_grounded':
                                # Only a non-executable documentation contract can use this mode.
                                # CODEX must justify the semantic classification; this checks its
                                # explicit declaration and exact pinned excerpt bindings only.
                                if not r['domain_id'].startswith('docs.'):
                                    errors.append(r['pair_id']+': SOURCE_GROUNDED_DOCUMENTATION_ONLY')
                                if proof.get('preference_basis')!='source_contract':
                                    errors.append(r['pair_id']+': SOURCE_CONTRACT_BASIS_REQUIRED')
                                citations=proof.get('source_evidence')
                                if not isinstance(citations,list) or not citations:
                                    errors.append(r['pair_id']+': DOCUMENTATION_SOURCE_EVIDENCE_REQUIRED')
                                    citations=[]
                                assigned={source_id(ref) for ref in r['source_refs']}
                                for citation in citations:
                                    if not isinstance(citation,dict) or not isinstance(citation.get('source_id'),str) or not citation.get('path') or not citation.get('sha256') or not citation.get('claim'):
                                        errors.append(r['pair_id']+': DOCUMENTATION_SOURCE_CITATION_REQUIRED'); continue
                                    sid=source_id(citation['source_id'])
                                    if sid is None or sid not in assigned:
                                        errors.append(r['pair_id']+': DOCUMENTATION_SOURCE_OUTSIDE_ROW'); continue
                                    source_path=Path(citation['path'])
                                    if not source_path.is_absolute(): source_path=artifact.parent/source_path
                                    if not source_path.is_file() or digest(source_path)!=citation['sha256'] or citation['sha256']!=source_catalog()[sid]['local_excerpt_sha256']:
                                        errors.append(r['pair_id']+': DOCUMENTATION_SOURCE_HASH_MISMATCH'); continue
                                    span=citation.get('excerpt_lines')
                                    line_count=len(source_path.read_text(encoding='utf-8').splitlines())
                                    if not isinstance(span,list) or len(span)!=2 or any(type(n) is not int for n in span) or not 1<=span[0]<=span[1]<=line_count:
                                        errors.append(r['pair_id']+': DOCUMENTATION_EXCERPT_LINES_INVALID'); continue
                                    evidence_artifacts[str(source_path.resolve())]=digest(source_path)
                            elif mode=='execution':
                                execution=proof.get('execution',{})
                                if not isinstance(execution,dict) or not execution.get('command') or not execution.get('environment') or type(execution.get('exit_code')) is not int or execution.get('exit_code')!=0 or not execution.get('artifacts'):
                                    errors.append(r['pair_id']+': ORACLE_EXECUTION_EVIDENCE_REQUIRED')
                                else:
                                    for raw in execution['artifacts']:
                                        if not isinstance(raw,dict) or not raw.get('path') or not raw.get('sha256'):
                                            errors.append(r['pair_id']+': ORACLE_RAW_ARTIFACT_REQUIRED'); continue
                                        raw_path=Path(raw['path'])
                                        if not raw_path.is_absolute(): raw_path=artifact.parent/raw_path
                                        if not raw_path.is_file() or digest(raw_path)!=raw['sha256']:
                                            errors.append(r['pair_id']+': ORACLE_RAW_ARTIFACT_HASH_MISMATCH')
                                        else: evidence_artifacts[str(raw_path.resolve())]=digest(raw_path)
                            else:
                                errors.append(r['pair_id']+': ORACLE_VERIFICATION_MODE_INVALID')
                        verdicts=proof.get('rows',{})
                        verdict=verdicts.get(r['pair_id'],{}) if isinstance(verdicts,dict) else {}
                        if not isinstance(verdict,dict) or verdict.get('status')!='passed' or not verdict.get('reason'):
                            errors.append(r['pair_id']+': '+needed.upper()+'_PAIR_EVIDENCE_MISSING')
        report.update(review_sha256=digest(a.review),key_sha256=digest(a.key),review_path=str(Path(a.review).resolve()),key_path=str(Path(a.key).resolve()))
        for kind,path in [('selection',getattr(a,'selection',None)),('review_receipt',getattr(a,'review_receipt',None)),('blind',blind_path)]:
            if path: report.update({kind+'_path':str(Path(path).resolve()),kind+'_sha256':digest(path)})
        reviewed=not errors
    if req: report.update(request_path=str(Path(a.request).resolve()),request_sha256=digest(a.request))
    report.update(errors=errors,structural_valid=not errors,semantic_review_accepted=reviewed,
                  evidence_artifacts=evidence_artifacts,
                  status='CANDIDATE_VALIDATED' if reviewed else ('STRUCTURE_ONLY' if not errors else 'NEEDS_REPAIR'))
    # Validation is reproducible derived evidence; raw inputs/review never overwritten.
    write(validation_path(a.input),report,exclusive=False)
    print(json.dumps(report,ensure_ascii=False,indent=2))
    return 1 if errors else 0

def blind(a):
    data=rows(a.input); problems=validate_rows(data,getattr(a,'allow_reference',False))
    if problems: raise ValueError('\n'.join(problems))
    out=Path(a.out); keypath=out.with_name(out.stem+'.key.json')
    if out.exists() or keypath.exists(): raise ValueError('Blind files already exist; do not overwrite')
    rng=random.Random(secrets.randbits(128)); key={}
    for r in data:
        key[r['pair_id']]=rng.choice(['A','B'])
    write(out,jsonl(make_blind_packet(data,key)))
    write(keypath,{'input_sha256':digest(a.input),'blind_sha256':digest(out),'chosen_positions':key})
    request_path=Path(a.input).parent/'request.json'
    if request_path.exists():
        write(out.with_name(out.stem+'.review-receipt.template.json'),authority_template('review',request_path,a.input,blind_path=out))
        review_prompt=(BASE/'prompts/REVIEW-PROMPT-RU.md').read_text(encoding='utf-8')
        write(out.with_name(out.stem+'.review.prompt.md'),review_prompt+'\n\nВход: '+str(out)+'\nSHA-256 blind: '+digest(out)+'\n')
    print(str(out))

def init_catalog(a):
    write(a.path,{'schema_version':'xiigen.corpus.v1','scope':'new-preparation-v1','releases':[]})
    print(a.path)

def audit(a):
    catalog=obj(a.catalog); allpaths=[x['path'] for x in catalog['releases']]+a.inputs
    paths=list(dict.fromkeys(str(Path(x).resolve()) for x in allpaths)); data=[]
    for p in paths: data.extend(rows(p))
    errors=validate_rows(data)
    for r in catalog['releases']:
        if digest(r['path'])!=r['sha256']: errors.append('CATALOG_RELEASE_HASH_CHANGED '+r['path'])
    report={'scope':'new-preparation-v1 corpus, all registered releases + explicit candidate inputs',
      'catalog_path':str(Path(a.catalog).resolve()),'catalog_sha256':digest(a.catalog),
      'inputs':{p:digest(p) for p in paths},'errors':errors,'passed':not errors,
      'rows':len(data),'families':len({r['family_id'] for r in data}),
      'semantic_near_duplicate_limit':'Exact IDs and declared lineage are checked. Semantic reviewer must check concealed same-family variants; no automatic semantic proof.'}
    write(a.out,report)
    print(json.dumps(report,ensure_ascii=False,indent=2));return 1 if errors else 0

def merge(a):
    data=[]; inputs=[]
    audit_report=obj(a.audit)
    if not audit_report.get('passed') or audit_report.get('errors'): raise ValueError('Corpus audit not passed')
    catalog_path=Path(audit_report['catalog_path'])
    if digest(catalog_path)!=audit_report['catalog_sha256']: raise ValueError('Corpus catalog changed; repeat audit against new releases')
    for p,sha in audit_report['inputs'].items():
        if digest(p)!=sha: raise ValueError('Corpus audit input changed: '+p)
    for name in a.inputs:
        v=obj(validation_path(name))
        if v.get('input_sha256')!=digest(name) or not v.get('semantic_review_accepted') or v.get('errors'):
            raise ValueError('Input lacks accepted current review: '+name)
        if v.get('acceptance_contract')!=ACCEPTANCE_CONTRACT: raise ValueError('Review predates CODEX authority contract; retain old files and re-review: '+name)
        for kind in ['review','key','selection','review_receipt','blind']:
            if not v.get(kind+'_path'): raise ValueError('Missing authority/review binding: '+kind)
            if digest(v[kind+'_path'])!=v[kind+'_sha256']: raise ValueError('Changed review/key: '+name)
        for path,sha in v.get('evidence_artifacts',{}).items():
            if digest(path)!=sha: raise ValueError('Changed review evidence: '+path)
        if not v.get('request_path') or digest(v['request_path'])!=v.get('request_sha256'): raise ValueError('Missing or changed generation request: '+name)
        if audit_report['inputs'].get(str(Path(name).resolve()))!=digest(name): raise ValueError('Input absent from corpus audit: '+name)
        local_errors=validate_rows(rows(name),request=obj(v['request_path']))
        local_errors+=authority_errors(v['selection_path'],'selection',v['request_path'])
        local_errors+=authority_errors(v['review_receipt_path'],'review',v['request_path'],name,v['review_path'],v['blind_path'])
        local_errors+=blind_errors(name,v['key_path'],v['blind_path'])
        if local_errors: raise ValueError('\n'.join(local_errors))
        data.extend(rows(name)); inputs.append({'path':name,'sha256':digest(name),'validation_sha256':digest(validation_path(name))})
    errors=validate_rows(data)
    if len({(r['domain_id'],r['topic_id'],r['profile_id']) for r in data})!=1: errors.append('MIXED_TRAINING_TOPICS')
    if len({r['metadata'].get('learning_objective_id') for r in data})!=1: errors.append('MIXED_LEARNING_OBJECTIVES')
    if errors: raise ValueError('\n'.join(errors))
    out=Path(a.out)
    if out.exists() or (out.parent/'release.json').exists(): raise ValueError('Release already exists')
    write(out,jsonl(data))
    write(out.parent/'release.json',{'kind':'immutable_shard','acceptance_contract':ACCEPTANCE_CONTRACT,
        'authority_evidence_scope':AUTHORITY_EVIDENCE_SCOPE,'provider_identity_authenticated':False,
        'status':'CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY','sha256':digest(out),'rows':len(data),
        'families':len({r['family_id'] for r in data}),'inputs':inputs,'corpus_audit_sha256':digest(a.audit),
        'error_tag_counts':dict(Counter(r['error_tag'] for r in data)), 'profile_counts':dict(Counter(r['profile_id'] for r in data)),
        'split_counts':{s:sum(r['split']==s for r in data) for s in ['train','dev','exam']}})
    catalog=obj(catalog_path)
    catalog['releases'].append({'path':str(out.resolve()),'sha256':digest(out)})
    # Catalog is an append-only inventory; underlying releases remain immutable.
    write(catalog_path,catalog,exclusive=False)
    print(str(out))

def compose(a):
    """Build an immutable cumulative snapshot from already accepted shards."""
    catalog=obj(a.catalog)
    registered={str(Path(x['path']).resolve()):x['sha256'] for x in catalog['releases']}
    paths=[str(Path(x).resolve()) for x in a.inputs]
    if len(set(paths))!=len(paths): raise ValueError('Repeated shard input')
    data=[]; inputs=[]
    for path in paths:
        sha=digest(path)
        if registered.get(path)!=sha: raise ValueError('Shard missing or changed in corpus catalog: '+path)
        release_path=Path(path).parent/'release.json'; release=obj(release_path)
        if release.get('sha256')!=sha or release.get('kind')!='immutable_shard' or release.get('status')!='CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY':
            raise ValueError('Input is not an accepted immutable shard: '+path)
        if release.get('acceptance_contract')!=ACCEPTANCE_CONTRACT: raise ValueError('Shard needs CODEX authority review: '+path)
        data.extend(rows(path))
        inputs.append({'path':path,'sha256':sha,'release_sha256':digest(release_path)})
    errors=validate_rows(data)
    if len({(r['domain_id'],r['topic_id'],r['profile_id']) for r in data})!=1: errors.append('MIXED_TRAINING_TOPICS')
    if len({r['metadata'].get('learning_objective_id') for r in data})!=1: errors.append('MIXED_LEARNING_OBJECTIVES')
    if errors: raise ValueError('\n'.join(errors))
    out=Path(a.out)
    if out.exists() or (out.parent/'release.json').exists(): raise ValueError('Snapshot already exists; use a new version')
    write(out,jsonl(data))
    write(out.parent/'release.json',{'kind':'dataset_snapshot','acceptance_contract':ACCEPTANCE_CONTRACT,
        'authority_evidence_scope':AUTHORITY_EVIDENCE_SCOPE,'provider_identity_authenticated':False,
        'status':'CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY',
        'sha256':digest(out),'rows':len(data),'families':len({r['family_id'] for r in data}),
        'inputs':inputs,'catalog_sha256':digest(a.catalog),'catalog_path':str(Path(a.catalog).resolve()),
        'error_tag_counts':dict(Counter(r['error_tag'] for r in data)),
        'profile_counts':dict(Counter(r['profile_id'] for r in data)),
        'split_counts':{s:sum(r['split']==s for r in data) for s in ['train','dev','exam']}})
    # A snapshot references existing rows; registering it again would duplicate the corpus.
    print(str(out))

def export(a):
    source=Path(a.input); release=obj(source.parent/'release.json')
    if release.get('sha256')!=digest(source): raise ValueError('Release hash mismatch')
    if release.get('kind') not in ['immutable_shard','dataset_snapshot'] or release.get('status')!='CANDIDATE_VALIDATED_NOT_NATIVE_TRAIN_READY' or release.get('acceptance_contract')!=ACCEPTANCE_CONTRACT:
        raise ValueError('Export requires a release with the current CODEX authority contract')
    data=rows(source); errors=validate_rows(data)
    if errors: raise ValueError('\n'.join(errors))
    out=Path(a.out)
    if out.exists(): raise ValueError('Export already exists; use a new version')
    for split in ['train','dev','exam']:
        write(out/f'dpo.{split}.jsonl',jsonl([{k:r[k] for k in ['prompt','chosen','rejected']} for r in data if r['split']==split]))
    write(out/'provenance.jsonl',jsonl(data))
    write(out/'export.json',{'status':'INTERCHANGE_FORMAT_ONLY','source_sha256':digest(source),'native_import_verified':False,
                           'authority_evidence_scope':AUTHORITY_EVIDENCE_SCOPE,'provider_identity_authenticated':False,
                           'files':{p.name:digest(p) for p in out.iterdir() if p.is_file()}})
    print(str(out))

def main():
    p=argparse.ArgumentParser(description=__doc__); sub=p.add_subparsers(dest='action',required=True)
    sub.add_parser('list')
    q=sub.add_parser('prepare')
    for name in ['topic','profile','run']: q.add_argument('--'+name,required=True)
    q.add_argument('--count',type=int,default=25);q.add_argument('--start',type=int,default=0)
    q.add_argument('--split',choices=['train','dev','exam'],default='train')
    q.add_argument('--work-root',default=str(BASE.parent))
    q.add_argument('--rag-root',default=str(BASE))
    q.add_argument('--family-plan')
    q.add_argument('--objective',help='One narrow objective from curriculum/index.json; legacy topic runs remain readable')
    q=sub.add_parser('validate');q.add_argument('--input',required=True);q.add_argument('--request');q.add_argument('--review');q.add_argument('--key');q.add_argument('--blind');q.add_argument('--selection');q.add_argument('--review-receipt');q.add_argument('--allow-reference',action='store_true')
    q=sub.add_parser('blind');q.add_argument('--input',required=True);q.add_argument('--out',required=True);q.add_argument('--allow-reference',action='store_true')
    q=sub.add_parser('init-catalog');q.add_argument('--path',required=True)
    q=sub.add_parser('audit');q.add_argument('--catalog',required=True);q.add_argument('--inputs',nargs='+',required=True);q.add_argument('--out',required=True)
    q=sub.add_parser('merge');q.add_argument('--inputs',nargs='+',required=True);q.add_argument('--out',required=True);q.add_argument('--audit',required=True)
    q=sub.add_parser('compose');q.add_argument('--catalog',required=True);q.add_argument('--inputs',nargs='+',required=True);q.add_argument('--out',required=True)
    q=sub.add_parser('export');q.add_argument('--input',required=True);q.add_argument('--out',required=True)
    a=p.parse_args()
    try:
        if a.action=='list':
            print(json.dumps(list(registry().values()),ensure_ascii=False,indent=2));return 0
        result=globals()[a.action.replace('-','_')](a)
        return result or 0
    except (ValueError,KeyError,TypeError,OSError) as exc:
        print('ERROR: '+str(exc),file=sys.stderr);return 2

if __name__=='__main__':
    sys.exit(main())

