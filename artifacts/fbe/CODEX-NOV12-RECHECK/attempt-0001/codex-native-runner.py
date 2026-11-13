import argparse, datetime, hashlib, json, os, pathlib, subprocess, sys
ROOT = pathlib.Path(r'C:\Projects\llm_mvp_core')
EVIDENCE = ROOT / 'artifacts/fbe/CODEX-NOV12-RECHECK/attempt-0001'
p = argparse.ArgumentParser()
p.add_argument('step', choices=['info', 'build', 'build-stable', 'discovery', 'test', 'layout-tests', 'layout-semantic12', 'layout-semantic29'])
a = p.parse_args()
filter_value = (EVIDENCE / 'codex-filter.txt').read_text(encoding='utf-8').strip()
project = 'Tests/LlmFromScratch.Tests.csproj'
commands = {
 'info': ['dotnet', '--info'],
 'build': ['dotnet', 'build', project, '-c', 'Release'],
 'build-stable': ['dotnet', 'build', project, '-c', 'Release'],
 'discovery': ['dotnet', 'test', project, '-c', 'Release', '--no-build', '--list-tests', '--filter', filter_value],
 'test': ['dotnet', 'test', project, '-c', 'Release', '--no-build', '--filter', filter_value, '--results-directory', str(EVIDENCE / 'results/selected'), '--logger', 'trx;LogFileName=selected.trx'],
}
artifacts_path = str(EVIDENCE / 'native-build')
layout_props = '-p:CustomBeforeDirectoryBuildProps=' + str(EVIDENCE / 'codex-artifacts-layout.props')
for name in ('build-stable', 'discovery', 'test'):
 commands[name] += ['--artifacts-path', artifacts_path, layout_props]
for name, target in [('layout-tests', project), ('layout-semantic12', '12.SemanticCompiler/SemanticCompiler.csproj'), ('layout-semantic29', '29.SemanticCompiler/SemanticCompiler.csproj')]:
 commands[name] = ['dotnet', 'msbuild', target, '-p:Configuration=Release', '-p:UseArtifactsOutput=true', '-p:ArtifactsPath=' + artifacts_path, layout_props, '-getProperty:BaseOutputPath,BaseIntermediateOutputPath,ArtifactsProjectName,MSBuildProjectName']
cmd = commands[a.step]
runid = 'CODEX-NOV12-RECHECK-attempt-0001-' + a.step
child_env = os.environ.copy()
child_env.update({'XIIGEN_EVIDENCE_RUN_ID': runid, 'XIIGEN_EVIDENCE_SOURCE': 'CODEX-NOV12-RECHECK/attempt-0001', 'XIIGEN_EVIDENCE_BUILD_INTENT': 'Release source recheck 6b52307dc26', 'XIIGEN_EVIDENCE_TEST_PROJECT': project, 'XIIGEN_EVIDENCE_TEST_FILTER': filter_value})
meta = {'agent': '/root/fresh_native_qa', 'step': a.step, 'cwd': str(ROOT), 'command': cmd, 'runId': runid, 'startedUtc': datetime.datetime.now(datetime.timezone.utc).isoformat(), 'runnerPid': os.getpid()}
mp = EVIDENCE / ('codex-' + a.step + '-process.json')
if mp.exists(): raise SystemExit('Refusing duplicate step ' + a.step)
stdoutp = EVIDENCE / 'logs' / (a.step + '.stdout.log')
stderrp = EVIDENCE / 'logs' / (a.step + '.stderr.log')
with stdoutp.open('xb') as out, stderrp.open('xb') as err:
 try:
  proc = subprocess.Popen(cmd, cwd=ROOT, env=child_env, stdout=out, stderr=err)
  meta['childPid'] = proc.pid
  mp.write_text(json.dumps(meta, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')
  print(json.dumps({'step': a.step, 'pid': proc.pid, 'state': 'running', 'evidence': str(EVIDENCE)}, ensure_ascii=False), flush=True)
  result = proc.wait()
 except Exception as ex:
  meta['launchError'] = repr(ex)
  result = -1
meta.update({'endedUtc': datetime.datetime.now(datetime.timezone.utc).isoformat(), 'exitCode': result, 'terminal': True})
mp.write_text(json.dumps(meta, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')
(EVIDENCE / 'logs' / (a.step + '.exitcode')).write_text(str(result) + '\n', encoding='utf-8', newline='\n')
for key, path in [('stdout', stdoutp), ('stderr', stderrp)]:
 data = path.read_bytes()
 meta[key] = {'path': str(path), 'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()}
mp.write_text(json.dumps(meta, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')
print(json.dumps({'step': a.step, 'exitCode': result, 'terminal': True, 'stdoutBytes': meta['stdout']['bytes'], 'stderrBytes': meta['stderr']['bytes']}, ensure_ascii=False), flush=True)
sys.exit(result if 0 <= result <= 255 else 1)
