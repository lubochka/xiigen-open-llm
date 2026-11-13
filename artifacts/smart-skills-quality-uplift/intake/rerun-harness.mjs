import { spawn } from 'node:child_process';
import { mkdir, readFile, rm, stat, utimes, writeFile } from 'node:fs/promises';
import path from 'node:path';

const ROOT = path.resolve('artifacts/smart-skills-quality-uplift/intake');
const SCRIPT_PATH = path.join(ROOT, 'build-source-ledger.mjs');
const FIXTURE_ROOT = path.join(ROOT, 'rerun-fixtures');
const REPORT_PATH = path.join(ROOT, 'rerun-report.json');

function sourcePath(name) {
  return path.join(FIXTURE_ROOT, `${name}.json`);
}

function outDir(name) {
  return path.join(FIXTURE_ROOT, name);
}

async function writeSource(filePath, rows) {
  const text = `[\n${rows.map((row) => JSON.stringify(row)).join(',\n')}\n]\n`;
  await writeFile(filePath, text, 'utf8');
}

async function readJson(filePath) {
  return JSON.parse(await readFile(filePath, 'utf8'));
}

async function readJsonl(filePath) {
  const text = await readFile(filePath, 'utf8');
  return text.trim().split(/\r?\n/).filter(Boolean).map((line) => JSON.parse(line));
}

function runNode(args) {
  return new Promise((resolve) => {
    const child = spawn(process.execPath, args, {
      cwd: process.cwd(),
      stdio: ['ignore', 'pipe', 'pipe']
    });
    let stdout = '';
    let stderr = '';

    child.stdout.on('data', (chunk) => {
      stdout += chunk.toString();
    });

    child.stderr.on('data', (chunk) => {
      stderr += chunk.toString();
    });

    child.on('close', (code) => {
      resolve({ code, stdout, stderr });
    });
  });
}

async function runLedger(label, source, output, previousLedger = null) {
  const args = [
    SCRIPT_PATH,
    '--source',
    source,
    '--out-dir',
    output
  ];

  if (previousLedger !== null) {
    args.push('--previous-ledger', previousLedger);
  }

  const result = await runNode(args);
  const summaryPath = path.join(output, 'source-ledger-summary.json');
  const reportPath = path.join(output, 'source-ledger-build-report.json');
  const ledgerPath = path.join(output, 'source-ledger.jsonl');

  return {
    label,
    command: `node ${args.map((arg) => JSON.stringify(arg)).join(' ')}`,
    exit_code: result.code,
    stdout: result.stdout.trim(),
    stderr: result.stderr.trim(),
    summary_path: summaryPath,
    report_path: reportPath,
    ledger_path: ledgerPath,
    summary: result.code === 0 ? await readJson(summaryPath) : null,
    report: result.code === 0 ? await readJson(reportPath) : null,
    ledger_rows: result.code === 0 ? await readJsonl(ledgerPath) : []
  };
}

function assertScenario(name, checks) {
  const failures = checks.filter((check) => !check.pass);

  return {
    scenario: name,
    status: failures.length === 0 ? 'PASS' : 'FAIL',
    checks,
    failure_count: failures.length,
    failures: failures.map((failure) => failure.name)
  };
}

function check(name, actual, expected) {
  return {
    name,
    actual,
    expected,
    pass: Object.is(actual, expected)
  };
}

function countRows(rows, predicate) {
  return rows.filter(predicate).length;
}

async function main() {
  await rm(FIXTURE_ROOT, { force: true, recursive: true });
  await mkdir(FIXTURE_ROOT, { recursive: true });

  const firstSource = sourcePath('first-done-and-todo');
  await writeSource(firstSource, [
    { path: 'fixture://done-alpha.md', file_name: 'done-alpha.md', state: 'done', content_sha256: 'alpha-v1' },
    { path: 'fixture://todo-beta.md', file_name: 'todo-beta.md', state: 'todo', content_sha256: 'beta-v1' }
  ]);
  const firstRun = await runLedger('first-done-run', firstSource, outDir('first-done-run'));
  const firstDoneScenario = assertScenario('first_done_run_processes_done_once', [
    check('exit_code', firstRun.exit_code, 0),
    check('report_status', firstRun.report?.status, 'PASS'),
    check('new_processed_row_count', firstRun.summary?.new_processed_row_count, 1),
    check('new_content_version_count', firstRun.summary?.new_content_version_count, 2),
    check('process_candidate_done_count', firstRun.summary?.processing_decision_counts?.process_candidate_done, 1)
  ]);
  const todoBacklogScenario = assertScenario('todo_backlog_is_recorded_not_processed', [
    check('backlog_not_processed_count', firstRun.summary?.processing_decision_counts?.backlog_not_processed, 1),
    check('todo_row_count', countRows(firstRun.ledger_rows, (row) => row.last_seen_state === 'todo'), 1),
    check('todo_new_processed_absent', countRows(firstRun.ledger_rows, (row) => row.last_seen_state === 'todo' && row.processing_decision === 'process_candidate_done'), 0)
  ]);

  const immediateRun = await runLedger('immediate-rerun', firstSource, outDir('immediate-rerun'), firstRun.ledger_path);
  const immediateScenario = assertScenario('immediate_rerun_has_zero_new_processed_rows', [
    check('exit_code', immediateRun.exit_code, 0),
    check('report_status', immediateRun.report?.status, 'PASS'),
    check('new_processed_row_count', immediateRun.summary?.new_processed_row_count, 0),
    check('new_content_version_count', immediateRun.summary?.new_content_version_count, 0),
    check('already_processed_done_count', immediateRun.summary?.already_processed_done_count, 1)
  ]);

  const todoInitialSource = sourcePath('todo-to-done-initial');
  await writeSource(todoInitialSource, [
    { path: 'fixture://todo-gamma.md', file_name: 'todo-gamma.md', state: 'todo', content_sha256: 'gamma-v1' }
  ]);
  const todoInitialRun = await runLedger('todo-initial', todoInitialSource, outDir('todo-initial'));
  const todoDoneSource = sourcePath('todo-to-done-current');
  await writeSource(todoDoneSource, [
    { path: 'fixture://todo-gamma.md', file_name: 'todo-gamma.md', state: 'done', content_sha256: 'gamma-v1' }
  ]);
  const todoDoneRun = await runLedger('todo-to-done', todoDoneSource, outDir('todo-to-done'), todoInitialRun.ledger_path);
  const todoDoneRow = todoDoneRun.ledger_rows.find((row) => row.path === 'fixture://todo-gamma.md');
  const todoToDoneScenario = assertScenario('todo_to_done_processes_first_time_with_same_hash', [
    check('new_processed_row_count', todoDoneRun.summary?.new_processed_row_count, 1),
    check('new_content_version_count', todoDoneRun.summary?.new_content_version_count, 0),
    check('first_seen_state_retained', todoDoneRow?.first_seen_state, 'todo'),
    check('last_seen_state_updated', todoDoneRow?.last_seen_state, 'done'),
    check('processing_decision', todoDoneRow?.processing_decision, 'process_candidate_done')
  ]);

  const mtimeSource = sourcePath('mtime-same-hash');
  await writeSource(mtimeSource, [
    { path: 'fixture://mtime-delta.md', file_name: 'mtime-delta.md', state: 'done', content_sha256: 'delta-v1', mtime: '2026-01-01T00:00:00.000Z' }
  ]);
  const mtimeFirstRun = await runLedger('mtime-first', mtimeSource, outDir('mtime-first'));
  const mtimeBefore = (await stat(mtimeSource)).mtimeMs;
  await writeSource(mtimeSource, [
    { path: 'fixture://mtime-delta.md', file_name: 'mtime-delta.md', state: 'done', content_sha256: 'delta-v1', mtime: '2026-01-02T00:00:00.000Z' }
  ]);
  await utimes(mtimeSource, new Date(), new Date(Date.now() + 5000));
  const mtimeSecondRun = await runLedger('mtime-second', mtimeSource, outDir('mtime-second'), mtimeFirstRun.ledger_path);
  const mtimeAfter = (await stat(mtimeSource)).mtimeMs;
  const mtimeSameHashScenario = assertScenario('changed_mtime_same_hash_has_no_new_content_version', [
    check('new_processed_row_count', mtimeSecondRun.summary?.new_processed_row_count, 0),
    check('new_content_version_count', mtimeSecondRun.summary?.new_content_version_count, 0),
    check('already_processed_done_count', mtimeSecondRun.summary?.already_processed_done_count, 1),
    { name: 'fixture_mtime_changed', actual: mtimeAfter > mtimeBefore, expected: true, pass: mtimeAfter > mtimeBefore }
  ]);

  const hashSourceV1 = sourcePath('hash-changed-v1');
  await writeSource(hashSourceV1, [
    { path: 'fixture://hash-epsilon.md', file_name: 'hash-epsilon.md', state: 'done', content_sha256: 'epsilon-v1' }
  ]);
  const hashFirstRun = await runLedger('hash-first', hashSourceV1, outDir('hash-first'));
  const hashSourceV2 = sourcePath('hash-changed-v2');
  await writeSource(hashSourceV2, [
    { path: 'fixture://hash-epsilon.md', file_name: 'hash-epsilon.md', state: 'done', content_sha256: 'epsilon-v2' }
  ]);
  const hashSecondRun = await runLedger('hash-second', hashSourceV2, outDir('hash-second'), hashFirstRun.ledger_path);
  const hashRows = hashSecondRun.ledger_rows.filter((row) => row.path === 'fixture://hash-epsilon.md');
  const changedHashScenario = assertScenario('changed_hash_creates_new_retained_version', [
    check('new_processed_row_count', hashSecondRun.summary?.new_processed_row_count, 1),
    check('new_content_version_count', hashSecondRun.summary?.new_content_version_count, 1),
    check('source_rows_with_multiple_versions', hashSecondRun.summary?.source_rows_with_multiple_versions, 1),
    check('versions_retained_for_same_source_row', hashRows.length, 2),
    check('distinct_hashes_for_retained_versions', new Set(hashRows.map((row) => row.sha256)).size, 2)
  ]);

  const scenarios = [
    firstDoneScenario,
    immediateScenario,
    todoBacklogScenario,
    todoToDoneScenario,
    mtimeSameHashScenario,
    changedHashScenario
  ];
  const runs = [
    firstRun,
    immediateRun,
    todoInitialRun,
    todoDoneRun,
    mtimeFirstRun,
    mtimeSecondRun,
    hashFirstRun,
    hashSecondRun
  ].map((run) => ({
    label: run.label,
    command: run.command,
    exit_code: run.exit_code,
    summary_path: run.summary_path,
    report_path: run.report_path,
    ledger_path: run.ledger_path,
    stdout: run.stdout,
    stderr: run.stderr
  }));
  const status = scenarios.every((scenario) => scenario.status === 'PASS') ? 'PASS' : 'FAIL';
  const report = {
    artifact: 'rerun-report',
    work_order_id: 'Q1-03',
    generated_at_utc: new Date().toISOString(),
    status,
    scope: 'Fixture-level rerun semantics for the repeatable intake command.',
    evidence_scope: 'Uses small synthetic fixture source-state files through build-source-ledger.mjs; this is not proof of model quality, DPO materialization, training, production runtime, dashboard behavior, or host behavior.',
    source_under_test: SCRIPT_PATH,
    scenarios,
    runs,
    forbidden_verification_not_run: [
      'dotnet build',
      'dotnet test',
      'training',
      'host/runtime probes',
      'production/test/dashboard code'
    ],
    exact_next_action: status === 'PASS' ? 'Q1 commit-boundary pre-commit reviews before any commit/push.' : 'Repair the failing Q1-03 rerun semantics scenario and rerun this harness.'
  };

  await writeFile(REPORT_PATH, `${JSON.stringify(report, null, 2)}\n`, 'utf8');
  console.log(JSON.stringify({ status, report_path: REPORT_PATH }, null, 2));

  if (status !== 'PASS') {
    process.exitCode = 1;
  }
}

main().catch((error) => {
  console.error(JSON.stringify({
    status: 'FAIL',
    reason: error instanceof Error ? error.message : String(error)
  }, null, 2));
  process.exitCode = 1;
});
