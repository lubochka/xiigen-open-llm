#!/usr/bin/env python3
"""Optional offline checks of preparation tools; synthetic fixtures, no provider/training calls."""
from pathlib import Path
import copy
import importlib.util
import json
import subprocess
import shutil
import sys
import tempfile

base=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(base/'tools'))
import batches as b
import sets as s
root=Path(tempfile.mkdtemp(prefix='xiigen-batch-check-'))
checks=[]
def test(name, condition, detail=None):
 if not condition: raise AssertionError(name+': '+str(detail))
 checks.append({'name':name,'passed':True,'detail':detail})
def command(*args,expected=0):
 r=subprocess.run([sys.executable,str(base/'tools/batches.py'),*map(str,args)],capture_output=True,text=True)
 if r.returncode!=expected: raise AssertionError(str(args)+'\n'+r.stdout+'\n'+r.stderr)
 return r

def put(p,obj):
 p.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def putrows(p,rows): p.write_text(s.jsonl(rows),encoding='utf-8')

streams=['TR07-OMISSION/ru-en','TR10-CONDITIONAL/ru-en','DOC-CS-NULL/base','CODE-CS-NULL/base','OR-AUTO-TERMINAL/base','OR-ROUTE-INTENT/base']
for st in streams:
 result=command('prepare','--stream',st,'--run','T001','--work-root',root)
 meta=json.loads(result.stdout)
 req=b.read(Path(meta['out'])/'request.json')
 test('prepare:'+st,req['stream_id']==st and req['scope']=='DATA_PREPARATION_ONLY',{'pairs':req['count'],'families':len(req['family_ids']),'prompt_chars':meta['prompt_chars']})
 text=(Path(meta['out'])/'prompt.md').read_text()
 test('nonempty-prompt:'+st,len(text)>1000 and 'NOT_RUN' in text)
run=root/'TR07-OMISSION/ru-en/T001'
req=b.read(run/'request.json')
result=command('prepare','--stream','TR07-OMISSION/ru-en','--run','T001','--work-root',root,expected=2)
test('existing-run-kept', 'already exists' in result.stderr and (run/'request.json').exists())
result=command('prepare','--stream','TR07-OMISSION/ru-en','--run','T002','--previous',run/'request.json','--work-root',root)
req2=b.read(root/'TR07-OMISSION/ru-en/T002/request.json')
test('next-batch-families-do-not-overlap',not(set(req['family_ids'])&set(req2['family_ids'])))
result=command('prepare','--stream','DOC-CS-NULL/base','--run','T002','--previous',run/'request.json','--work-root',root,expected=2)
test('foreign-previous-request-refused','same stream' in result.stderr)

badplan={'families':[
 {'family_id':'A','origin_family_id':'root','leakage_group_ids':['g'],'split':'train','topics':['TR07'],'profiles':['ru-en'],'scenario':'x'},
 {'family_id':'B','origin_family_id':'root','leakage_group_ids':['h'],'split':'exam','topics':['TR08'],'profiles':['en-ru'],'scenario':'y'}]}
planpath=root/'bad-family-plan.json';put(planpath,badplan)
result=command('prepare','--stream','TR07-OMISSION/ru-en','--run','BAD','--family-plan',planpath,'--count','1','--work-root',root,expected=2)
test('whole-family-plan-leakage-refused','FAMILY_PLAN_SPLIT_LEAKAGE' in result.stderr)

# All following records are isolated tool fixtures; no teacher or reviewer was called.
data=[]
for i,fid in enumerate(req['family_ids']):
 data.append({'schema_version':'xiigen.dpo.v1','pair_id':'fixture-'+str(i),'domain_id':req['domain_id'],'topic_id':req['topic_id'],'profile_id':req['profile_id'],'family_id':fid,'split':req['split'],
 'prompt':'Инструментальный fixture '+str(i)+': переведи «На крыше стоит белый контейер», восстановив одну пропущенную букву в слове «контейнер».',
 'chosen':'There is a white container on the roof.','rejected':'There is a white cabinet on the roof.','error_tag':'wrong_lexeme','rationale':'Only a validator fixture, not accepted language evidence.',
 'source_refs':req['sources'],'provenance':{'kind':'TOOL_FIXTURE_NOT_PROVIDER_OUTPUT','generator_model':'TEST_FIXTURE','generation_session_id':'fixture-generator'},
 'metadata':{'origin_family_id':fid,'leakage_group_ids':[fid],'learning_objective_id':req['objective_id'],'simulation':True}})
putrows(run/'draft.jsonl',data)
result=command('check','--input',run/'draft.jsonl','--request',run/'request.json')
test('valid-structure-is-not-learning',json.loads(result.stdout)['status']=='STRUCTURE_ONLY')
for badvalue in [None,'RAG-023',[['RAG-023']],42]:
 altered=copy.deepcopy(data); altered[0]['source_refs']=badvalue
 errors=s.validate_rows(altered,request=req)
 test('malformed-source-refs:'+repr(badvalue),any('SOURCE_REFS_REQUIRED' in e for e in errors))
altered=copy.deepcopy(data);del altered[0]['metadata']['origin_family_id']
test('missing-origin-refused',any('ORIGIN_FAMILY_REQUIRED' in e for e in s.validate_rows(altered,request=req)))
altered=copy.deepcopy(data);altered[0]['metadata']['learning_objective_id']=['bad']
test('nonstring-objective-verdict',any('OBJECTIVE_ID_MUST_BE_STRING' in e for e in s.validate_rows(altered,request=req)))
leaked=copy.deepcopy(data[0]);leaked['pair_id']='leaked';leaked['family_id']='renamed';leaked['split']='exam';leaked['prompt']='A different exact prompt';leaked['metadata']['origin_family_id']=data[0]['family_id']
test('renamed-origin-leakage-refused',any('RELATED_FAMILY_SPLIT_LEAKAGE' in e for e in s.validate_rows(data+[leaked])))

command('blind','--input',run/'draft.jsonl','--request',run/'request.json','--out',run/'blind.jsonl')
key=b.read(run/'blind.key.json');review=b.read(run/'blind.review.template.json')
put(run/'review.json',review)
errors=b.review_errors(data,run/'blind.jsonl',run/'blind.key.json',run/'review.json')
test('pending-template-not-acceptance',any('REVIEW_PENDING' in e for e in errors))
review['status']='completed';review['reviewer']={'name':'TEST_FIXTURE_NO_REAL_REVIEW','observed_model':'TEST_FIXTURE','session_id':'fixture-review','independence':'separate_session','saw_label_key':False}
for v in review['rows']:
 v['winner']=key['chosen_positions'][v['pair_id']];v['valid']=True;v['reason_ru']='Synthetic tool fixture; no model review claimed.';v['checks']={k:True for k in b.CHECKS}
 v['evidence']={'kind':'bilingual_review','status':'completed','method':'TOOL_FIXTURE','detail_ru':'Fixture tests the checker branch, not translation quality.'}
put(run/'review.json',review)
test('complete-declared-review-structure',b.review_errors(data,run/'blind.jsonl',run/'blind.key.json',run/'review.json')==[])
stale=copy.deepcopy(data);stale[0]['chosen']='Changed after blind'
test('stale-draft-refused','INPUT_CHANGED_SINCE_BLIND' in b.review_errors(stale,run/'blind.jsonl',run/'blind.key.json',run/'review.json'))
badreview=copy.deepcopy(review);badreview['rows'][0]['winner']='B' if badreview['rows'][0]['winner']=='A' else 'A';put(run/'bad-review.json',badreview)
test('wrong-winner-refused',any('PREFERENCE_NOT_ACCEPTED' in e for e in b.review_errors(data,run/'blind.jsonl',run/'blind.key.json',run/'bad-review.json')))
badreview=copy.deepcopy(review);badreview['reviewer']['session_id']='fixture-generator';put(run/'bad-review.json',badreview)
test('same-session-not-independent',any('GENERATOR_CANNOT' in e for e in b.review_errors(data,run/'blind.jsonl',run/'blind.key.json',run/'bad-review.json')))
command('accept','--input',run/'draft.jsonl','--request',run/'request.json','--blind',run/'blind.jsonl','--key',run/'blind.key.json','--review',run/'review.json','--out',run/'accepted.jsonl')
command('export','--inputs',run/'accepted.jsonl','--out',run/'export')
exported=s.rows(run/'export/dpo.train.jsonl')
test('export-only-model-fields',all(set(r)=={'prompt','chosen','rejected'} for r in exported))
test('export-no-native-claim',b.read(run/'export/export.json')['native_import_verified'] is False)
moved=root/'relocated-run';shutil.copytree(run,moved)
command('export','--inputs',moved/'accepted.jsonl','--out',moved/'export-relocated')
test('whole-run-portable-with-relative-bindings',(moved/'export-relocated/export.json').is_file())
putrows(run/'accepted.jsonl',stale)
result=command('export','--inputs',run/'accepted.jsonl','--out',run/'export2',expected=2)
test('changed-accepted-data-refused','changed after review' in result.stderr)

# General family-class handling, independent of old clarification-specific names.
paired=copy.deepcopy(data[:2]); paired[1]['family_id']=paired[0]['family_id'];paired[1]['metadata']['origin_family_id']=paired[0]['family_id'];paired[1]['metadata']['leakage_group_ids']=[paired[0]['family_id']]
paired[0]['metadata']['span_treatment']='restore';paired[1]['metadata']['span_treatment']='preserve'
pr=copy.deepcopy(req);pr.update(count=2,family_ids=[paired[0]['family_id']],pairs_per_family=2,family_class_field='span_treatment',family_class_values=['restore','preserve'])
test('generic-paired-family-valid',s.validate_rows(paired,request=pr)==[])
paired[1]['metadata']['span_treatment']='restore'
test('generic-paired-family-missing-control',any('CONDITIONAL_FAMILY_CLASS_BALANCE' in e for e in s.validate_rows(paired,request=pr)))

doc=copy.deepcopy(data[0]);doc['domain_id']='docs.csharp';doc['metadata']['learning_objective_id']='DOC-CS-SNAPSHOT';doc['source_refs']=['RAG-027']
declared_source={'kind':'source_grounded','status':'completed','method':'TOOL_FIXTURE','detail_ru':'Declared source-only fixture, not executed evidence.','source_refs':['RAG-027'],'depends_on_execution':False}
test('runtime-objective-cannot-self-declare-source-only',any('OBJECTIVE_REQUIRES_EXECUTABLE_ORACLE' in e for e in b.evidence_errors(declared_source,doc,run/'review.json')))
del doc['metadata']['learning_objective_id']
test('nonexecutable-parent-source-review-still-allowed',b.evidence_errors(declared_source,doc,run/'review.json')==[])

report={'scope':'PREPARATION_TOOL_CHECKS_ONLY','provider_calls':0,'training_runs':0,'independent_semantic_reviews':0,'fixture_note':'Synthetic test records and declared reviewers were created only in scratch to exercise tool branches. None is a Chinese-model output, native train-ready dataset, or measured learning result.', 'checks':checks,'passed':len(checks),'failed':0}
put(root/'summary.json',report)
print(json.dumps({'passed':len(checks),'failed':0,'root':str(root),'report':str(root/'summary.json')},indent=2))
