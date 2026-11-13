import datetime, hashlib, json, pathlib, subprocess, sys
R=pathlib.Path(r'C:\Projects\llm_mvp_core'); E=R/'artifacts/fbe/CODEX-NOV12-RECHECK/attempt-0001'
stage=sys.argv[1]
base=json.loads((E/'codex-source-final-before.json').read_text(encoding='utf-8'))
paths={x['file'] for x in base['inputs']}
paths.update(p.relative_to(R).as_posix() for p in (R/'Tests/ExperimentFixes').glob('*.cs'))
rows=[{'file':p,'sha256':hashlib.sha256((R/p).read_bytes()).hexdigest()} for p in sorted(paths) if (R/p).is_file()]
sha=lambda x:hashlib.sha256(x).hexdigest()
def git(args):
 p=subprocess.run(['git',*args],cwd=R,capture_output=True)
 if p.returncode: raise RuntimeError(p.stderr.decode('utf-8','replace'))
 return p.stdout
head=git(['rev-parse','HEAD']).decode().strip()
diff=git(['diff','--binary','HEAD'])
obj={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'stage':stage,'head':head,'count':len(rows),'fingerprint':sha(json.dumps(rows,ensure_ascii=False,separators=(',',':')).encode()),'diffSha256':sha(diff),'inventoryDescription':base['inventoryDescription'],'inputs':rows,'projectClosure':base['projects']}
with (E/('codex-source-'+stage+'.json')).open('x',encoding='utf-8',newline='\n') as f: json.dump(obj,f,ensure_ascii=False,indent=2);f.write('\n')
(E/'logs'/('source-'+stage+'.diff')).write_bytes(diff)
print(json.dumps({k:obj[k] for k in ('utc','stage','head','count','fingerprint','diffSha256')},ensure_ascii=False),flush=True)
