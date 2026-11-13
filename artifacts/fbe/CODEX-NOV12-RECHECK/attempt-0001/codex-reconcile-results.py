import collections, datetime, hashlib, json, os, pathlib, re, xml.etree.ElementTree as ET
E=pathlib.Path(r'C:\Projects\llm_mvp_core\artifacts\fbe\CODEX-NOV12-RECHECK\attempt-0001'); R=E.parents[3]
read=lambda p:json.loads((E/p).read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(pathlib.Path(p).read_bytes()).hexdigest()
req=read('codex-required-cases-current.json'); discovered=read('codex-discovery-reconciliation.json')
r=ET.parse(E/'results/selected/selected.trx').getroot(); n={'t':r.tag.split('}')[0][1:]}
results=r.findall('t:Results/t:UnitTestResult',n)
defs={x.attrib['id']:x for x in r.findall('t:TestDefinitions/t:UnitTest',n)}
entries={(x.attrib['testId'],x.attrib['executionId']) for x in r.findall('t:TestEntries/t:TestEntry',n)}
records=[]
for x in results:
 d=defs[x.attrib['testId']]; m=d.find('t:TestMethod',n)
 fqn=m.attrib['className'].split(',')[0]+'.'+m.attrib['name'].split('(')[0]
 records.append({'testId':x.attrib['testId'],'executionId':x.attrib['executionId'],'name':x.attrib['testName'],'fqn':fqn,'outcome':x.attrib['outcome'],'codeBase':m.attrib.get('codeBase'),'entryMatched':(x.attrib['testId'],x.attrib['executionId']) in entries})
issues=[]
if len({x['testId'] for x in records})!=len(records): issues.append('duplicate testId')
if len({x['executionId'] for x in records})!=len(records): issues.append('duplicate executionId')
if any(not x['entryMatched'] for x in records): issues.append('missing entry')
for q in req:
 actual=[x for x in records if x['fqn']==q['fqn']]
 if len(actual)!=q['cases']: issues.append('case count '+q['fqn'])
 if q['cases']>1:
  for attr in q['attributes']:
   if not attr.startswith('[InlineData'): continue
   first=re.match(r'\[InlineData\(\s*("[^"]*"|-?\d+)',attr).group(1)
   if first.startswith('"'): matches=[x for x in actual if first in x['name']]
   else: matches=[x for x in actual if re.search(r'\([^:(),]+:\s*'+re.escape(first)+r'\s*[,)]',x['name'])]
   if len(matches)!=1: issues.append('InlineData identity '+q['fqn']+' '+first)
required_fqns={x['fqn'] for x in req}
if any(x['fqn'] not in required_fqns for x in records): issues.append('unknown FQN')
expected_names={name for item in discovered['mapping'] for name in item['discovered']}
actual_names={x['name'] for x in records}
if expected_names!=actual_names: issues.append('discovery/result name mismatch')
basic=read('codex-trx-basic.json'); counters=basic['counters']
if int(counters['total'])!=len(results) or int(counters['executed'])!=len(results) or int(counters['passed'])!=len(results): issues.append('counter mismatch')
if any(x['outcome']!='Passed' for x in records): issues.append('nonpassed result')
for field in ('failed','error','timeout','aborted','inconclusive','notRunnable','notExecuted','disconnected','pending','inProgress'):
 if int(counters.get(field,0)): issues.append('counter '+field)
meta=read('codex-test-process.json'); build=read('codex-build-stable-process.json'); discovery=read('codex-discovery-process.json')
if any(not x.get('terminal') or x['exitCode']!=0 for x in (meta,build,discovery)): issues.append('process terminal')
date=datetime.datetime.fromisoformat
if not date(meta['startedUtc'])<=date(basic['times']['start'])<=date(basic['times']['finish'])<=date(meta['endedUtc']): issues.append('timestamp chain')
assembly=read('codex-assembly-before-test.json'); dll_changes=[]
for item in assembly['assemblies']:
 actual=sha(item['file'])
 if actual!=item['sha256']: dll_changes.append({'file':item['file'],'before':item['sha256'],'after':actual})
if dll_changes: issues.append('DLL/PDB drift')
props_sha=sha(E/'codex-artifacts-layout.props')
if props_sha!=assembly['propsSha256'] or props_sha!=read('codex-build-config-identity.json')['props']['sha256']: issues.append('props drift')
main=next(x for x in assembly['assemblies'] if x['file'].endswith('LlmFromScratch.Tests.dll'))
if any(os.path.normcase(x['codeBase'] or '')!=os.path.normcase(main['file']) for x in records): issues.append('wrong executed assembly')
log=(E/'logs/test.stdout.log').read_text(encoding='utf-8'); summary=[x for x in log.splitlines() if 'Passed!' in x][-1]
cli={k:int(v) for k,v in re.findall(r'(Failed|Passed|Skipped|Total):\s*(\d+)',summary)}
if cli!={'Failed':0,'Passed':len(results),'Skipped':0,'Total':len(results)}: issues.append('CLI summary mismatch')
published=read('codex-published-before.json'); changes=[]
for item in published['files']:
 p=R/item['file']; actual=sha(p) if p.exists() else 'MISSING'
 if actual!=item['sha256']: changes.append({'file':item['file'],'before':item['sha256'],'after':actual,'isCSharpSource':item['file'].endswith('.cs')})
raw_changes=[x for x in changes if not x['isCSharpSource']]
if raw_changes: issues.append('published non-CSharp evidence drift')
byclass=collections.Counter(x['fqn'].rsplit('.',1)[0] for x in records)
report={'runId':basic['runId'],'counts':basic,'processes':{'build':build,'discovery':discovery,'test':meta},'classCounts':dict(byclass),'recordCount':len(records),'requiredMethodCount':len(req),'records':records,'cliSummary':cli,'mainDll':main,'dllAndPdbFilesChecked':len(assembly['assemblies']),'dllChanges':dll_changes,'propsSha256':props_sha,'publishedFilesChecked':len(published['files']),'publishedNonCSharpFilesChecked':sum(not x['file'].endswith('.cs') for x in published['files']),'publishedChanges':changes,'publishedNonCSharpChanges':raw_changes,'issues':issues}
(E/'codex-result-reconciliation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
print(json.dumps({'unitResults':len(records),'requiredMethods':len(req),'classCounts':dict(byclass),'issues':issues,'dllAndPdbChecked':len(assembly['assemblies']),'dllChanges':dll_changes,'publishedNonCSharpChecked':report['publishedNonCSharpFilesChecked'],'publishedNonCSharpChanges':raw_changes,'publishedCSharpChanges':[x['file'] for x in changes if x['isCSharpSource']]},ensure_ascii=False))
raise SystemExit(1 if issues else 0)
