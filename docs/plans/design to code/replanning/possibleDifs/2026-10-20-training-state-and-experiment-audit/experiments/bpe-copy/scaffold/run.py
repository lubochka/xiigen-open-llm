#!/usr/bin/env python3
"""Explicit future execution entry point. Never invoked during the document audit."""
from pathlib import Path
import argparse
import hashlib
import json
import subprocess
import time


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--results-root', type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parent
    meta = json.loads((root / 'experiment.json').read_text())
    artifact = root / 'experiments-v2/RP01-MICRO' / meta['id']
    host = root / '35.ExperimentsV2/Experiments/Rp01Micro' / meta['id'] / 'Runner.csproj'
    manifest = json.loads((artifact / 'source-copy-manifest.json').read_text())
    if hashlib.sha256((artifact/'Program.cs').read_bytes()).hexdigest() != manifest['program_sha256']:
        raise SystemExit('PROGRAM_CHANGED_AFTER_PREPARATION')
    for entry in manifest['files']:
        actual = hashlib.sha256((artifact/'src'/entry['path']).read_bytes()).hexdigest()
        if actual != entry['copied_sha256']:
            raise SystemExit('EXPERIMENT_SOURCE_CHANGED_AFTER_PREPARATION: '+entry['path'])
    result = args.results_root.resolve()
    if result.exists():
        raise SystemExit('RESULTS_ALREADY_EXIST: preserve the earlier run; no automatic rerun or overwrite.')
    for old_build in (artifact/'bin', artifact/'obj'):
        if old_build.exists():
            raise SystemExit('BUILD_ARTIFACT_ALREADY_EXISTS: preserve it; no cleanup or rebuild: '+str(old_build))
    result.mkdir(parents=True)
    jobs = [('A','code-sign'),('B','code-sign')] if meta['kind']=='linear-transpose' else [('A','code-doc'),('B','code-doc'),('A','en-ru'),('B','en-ru')]
    timeline = []
    # This is the only build, for this tiny source-closed host. No tests/solution/dispatcher.
    try:
        with (result/'build.stdout').open('wb') as out, (result/'build.stderr').open('wb') as err:
            built = subprocess.run(['dotnet','build',str(host),'-c','Release','--output',str(artifact/'bin'),
                '-p:ImportDirectoryBuildProps=false','-p:ImportDirectoryBuildTargets=false',
                '-p:BaseIntermediateOutputPath='+str(artifact/'obj')+'/'], stdout=out, stderr=err, timeout=120)
    except FileNotFoundError as error:
        (result/'status.json').write_text(json.dumps({'status':'BUILD_TOOL_MISSING','stage':'build','exit_code':127,'error':str(error),'no_auto_retry':True})+'\n')
        return 127
    except subprocess.TimeoutExpired as error:
        (result/'status.json').write_text(json.dumps({'status':'BUILD_TIMEOUT','stage':'build','exit_code':124,'error':str(error),'no_auto_retry':True})+'\n')
        return 124
    if built.returncode:
        (result/'status.json').write_text(json.dumps({'status':'BUILD_FAILED','exit_code':built.returncode})+'\n')
        return 1
    deadline = time.monotonic()+120
    for arm, domain in jobs:
        folder = result/arm
        folder.mkdir(exist_ok=True)
        output = folder/(domain+'.json')
        command=['dotnet',str(artifact/'bin'/(meta['id']+'.dll')),arm,domain,str(output)]
        remaining=deadline-time.monotonic()
        if remaining <= 0:
            timeline.append({'arm':arm,'domain':domain,'status':'BUDGET_EXHAUSTED'})
            break
        try:
            with (folder/(domain+'.stdout')).open('wb') as out, (folder/(domain+'.stderr')).open('wb') as err:
                proc=subprocess.run(command,stdout=out,stderr=err,timeout=min(60,remaining))
            timeline.append({'arm':arm,'domain':domain,'exit_code':proc.returncode})
            if proc.returncode:
                break
        except subprocess.TimeoutExpired:
            timeline.append({'arm':arm,'domain':domain,'status':'TIMEOUT_NO_RETRY'})
            break
    (result/'execution.json').write_text(json.dumps({'core_commit':meta['core_commit'],'jobs':timeline,'no_auto_retry':True},indent=2)+'\n')
    return 0 if len(timeline)==len(jobs) and all(x.get('exit_code')==0 for x in timeline) else 1


if __name__=='__main__':
    raise SystemExit(main())
