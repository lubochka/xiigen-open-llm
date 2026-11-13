import datetime, fnmatch, hashlib, json, os, pathlib, subprocess, sys, xml.etree.ElementTree as ET
ROOT = pathlib.Path(r'C:\Projects\llm_mvp_core').resolve()
E = ROOT / 'artifacts/fbe/CODEX-NOV12-RECHECK/attempt-0001'
stage = sys.argv[1]
projects = {}
files = set()
pruned = {'bin','obj','TestResults','artifacts','.git','node_modules','docs','.vs','.venv'}
patterns = ('*.cs','*.csproj','*.props','*.targets','*.resx','global.json','NuGet.config','NuGet.Config','nuget.config','packages.lock.json')
def rel(p): return pathlib.Path(p).resolve().relative_to(ROOT).as_posix()
def add(p):
 p = pathlib.Path(p).resolve()
 if p.is_file(): files.add(p)
def visit(p):
 p = pathlib.Path(p).resolve()
 if p in projects: return
 tree = ET.parse(p)
 elements = list(tree.iter())
 refs = [x.attrib['Include'] for x in elements if x.tag.split('}')[-1] == 'ProjectReference' and 'Include' in x.attrib]
 compile_items = [{'kind':x.tag.split('}')[-1],**x.attrib} for x in elements if x.tag.split('}')[-1] == 'Compile']
 projects[p] = {'project':rel(p),'references':refs,'compileItems':compile_items}
 add(p)
 for folder, dirs, names in os.walk(p.parent):
  dirs[:] = [d for d in dirs if d not in pruned]
  for name in names:
   if any(fnmatch.fnmatchcase(name, pattern) for pattern in patterns): add(pathlib.Path(folder)/name)
 for x in elements:
  kind=x.tag.split('}')[-1]
  if kind in ('Compile','Import'):
   included = x.attrib.get('Include') if kind=='Compile' else x.attrib.get('Project')
   if included and '$(' not in included and '*' not in included and ';' not in included: add(p.parent/included.replace('\\','/'))
 for reference in refs:
  if '$(' in reference: raise RuntimeError('Unresolved ProjectReference '+reference)
  visit(p.parent/reference.replace('\\','/'))
visit(ROOT/'Tests/LlmFromScratch.Tests.csproj')
for p in list(projects):
 for folder in [p.parent,*p.parent.parents]:
  if folder != ROOT and ROOT not in folder.parents: continue
  for name in ('Directory.Build.props','Directory.Build.targets','Directory.Packages.props','global.json','NuGet.Config','NuGet.config','nuget.config'): add(folder/name)
inputs=[{'file':rel(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(files,key=lambda p:rel(p))]
fingerprint=hashlib.sha256(json.dumps(inputs,ensure_ascii=False,separators=(',',':')).encode()).hexdigest()
def git(args):
 r=subprocess.run(['git',*args],cwd=ROOT,capture_output=True)
 if r.returncode: raise RuntimeError(r.stderr.decode('utf-8','replace'))
 return r.stdout.decode('utf-8','replace')
snapshot={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'stage':stage,'head':git(['rev-parse','HEAD']).strip(),'trackedStatus':git(['status','--porcelain=v1','--untracked-files=no']),'inventoryDescription':'Direct filesystem inventory of transitive ProjectReference directories, plus explicit Compile/Import paths and ancestor build configuration. Includes untracked files. Conservative source superset: default conditional Compile Remove/Include declarations recorded, not all conditions evaluated. Prunes generated/runtime/artifact/document/node directories; explicit included files still read.','prunedDirectories':sorted(pruned),'count':len(inputs),'fingerprint':fingerprint,'projectClosure':list(projects.values()),'inputs':inputs}
with (E/('codex-source-'+stage+'.json')).open('x',encoding='utf-8',newline='\n') as out: json.dump(snapshot,out,ensure_ascii=False,indent=2);out.write('\n')
print(json.dumps({'stage':stage,'head':snapshot['head'],'count':len(inputs),'fingerprint':fingerprint,'projects':len(projects),'trackedStatus':snapshot['trackedStatus']},ensure_ascii=False),flush=True)
