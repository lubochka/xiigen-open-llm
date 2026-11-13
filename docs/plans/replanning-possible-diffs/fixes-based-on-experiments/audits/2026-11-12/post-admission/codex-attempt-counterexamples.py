#!/usr/bin/env python3
"""Pinned source review plus filesystem counterexamples; never a C#/PowerShell PASS.

Run with Python 3.10+:
  python3 codex-attempt-counterexamples.py --repo-root /path/to/core --output result.json
The expected blobs intentionally pin f089a7d. Stop on changed source, then review
and deliberately update the pins before using this as evidence on another tree.
No Git mutation, model training, network, or repository evidence write occurs.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import uuid

SOURCE_SHA = 'f089a7d55d359ff103d0cdd9ddfb26eee346a16b'
PINS = {
    'Tests/ExperimentFixes/ExperimentEvidenceSink.cs': '1066100b3c1d645f04de027c2480720b18663987',
    'Tests/ExperimentFixes/ExperimentEvidenceSinkTests.cs': '45518e7ed7c05d3b63dfd1fc845f99c01c47351b',
    'Tests/ExperimentFixes/TranslationRequiredWholeTermOrdinaryPathTests.cs': '871f7fea5828b31cf7a822802bebc8477c3cb9fc',
    'scripts/experiment-launcher/ExperimentLauncher.psm1': 'c56dffe17a587772783d64992745fe9c0c05611e',
    'tools/test-batches/summarize-test-batches.ps1': 'fc59159efb3fec3da02961f6c1dadfe0576ac2e7',
    'Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs': 'a0e4f9156e4b852c37fbf44a8c30df99577b0a1a',
    'Tests/Learn07RollbackRunnerTests.cs': '60acbf0667b117cc7d8f70598399a0b88bc02a30',
    'Tests/TestSuiteBatchManifest.cs': 'f4f653ff45916f1bd48ae26cde84a671bf149cea',
}


def git_blob(data: bytes) -> str:
    return hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()


def read_sources(root: Path) -> tuple[dict, list]:
    texts, receipt = {}, []
    for relative, expected in PINS.items():
        data = (root / relative).read_bytes()
        actual = git_blob(data)
        if actual != expected:
            raise ValueError(f'SOURCE_CHANGED: {relative}; expected {expected}; got {actual}')
        texts[relative] = data.decode('utf-8')
        receipt.append(dict(path=relative, git_blob_sha=actual, sha256=hashlib.sha256(data).hexdigest(), bytes=len(data)))
    return texts, receipt


def require(text: str, fragment: str) -> None:
    if fragment not in text:
        raise ValueError('SOURCE_ASSUMPTION_NOT_FOUND: ' + fragment)


def selected_sink_path(root: Path, declared: Path, tracked_count: int) -> Path:
    """Only the successful Git-query branch and redirect rule, not C# execution.

    tracked_count is an explicit fixture input. This does not execute Git's
    IsPublishedEvidenceDirectory probe or establish cross-platform locking.
    """
    if tracked_count == 0:
        return declared
    return root / 'verification' / ('run-' + uuid.uuid4().hex[:8])


def write_like_sink(directory: Path, payload: str) -> Path:
    directory.mkdir(parents=True, exist_ok=True)
    target = directory / 'metrics.json'
    target.write_text(payload, encoding='utf-8')
    return target


def project_candidates(root: Path) -> tuple[list[str], int, int]:
    """Translation of candidate enumeration (2007-2057), not MSBuild/discovery."""
    declared = set()
    solutions = list(root.glob('*.sln'))
    for solution in solutions:
        for line in solution.read_text(encoding='utf-8').splitlines():
            if not line.startswith('Project('):
                continue
            for value in line.split('"'):
                if value.lower().endswith('.csproj'):
                    candidate = (root / value.replace('\\', '/')).resolve()
                    if candidate.is_file():
                        declared.add(candidate)
    ignored = {'bin', 'obj', 'testresults', 'nuget'}
    chosen = declared if declared else {
        p for p in root.rglob('*.csproj') if not any(part.lower() in ignored for part in p.parts)
    }
    return sorted(str(p.relative_to(root)) for p in chosen), len(solutions), len(declared)


def probe(root: Path) -> list[dict]:
    cases = []
    # C1 is a deterministic legal interleaving: both allocations complete before
    # either writer starts. It is not a timing-dependent stress-test claim.
    declared = root / 'c1' / 'attempt'
    first = selected_sink_path(root, declared, 0)
    second = selected_sink_path(root, declared, 0)
    target = write_like_sink(first, 'owner-A')
    before = target.read_bytes()
    write_like_sink(second, 'owner-B')
    observed = first == second and before != target.read_bytes()
    assert observed
    cases.append(dict(id='C1-two-unpublished-allocations', kind='SOURCE_TRANSLATION_FILESYSTEM',
                      same_address=True, writers_admitted=2, first_payload_replaced=True,
                      observed_counterexample=observed))

    declared = root / 'c2' / 'attempt'
    target = write_like_sink(declared, 'interrupted-run-raw')
    chosen = selected_sink_path(root, declared, 0)
    write_like_sink(chosen, 'new-run-raw')
    assert target.read_text() == 'new-run-raw'
    cases.append(dict(id='C2-occupied-untracked', kind='SOURCE_TRANSLATION_FILESYSTEM',
                      tracked_count_fixture=0, was_nonempty=True, previous_raw_replaced=True))

    declared = root / 'c3' / 'published'
    target = write_like_sink(declared, 'frozen-raw')
    first = selected_sink_path(root, declared, 1)
    second = selected_sink_path(root, declared, 1)
    write_like_sink(first, 'verification-A')
    write_like_sink(second, 'verification-B')
    assert target.read_text() == 'frozen-raw' and first != second
    cases.append(dict(id='C3-published-redirect-control', kind='SOURCE_TRANSLATION_FILESYSTEM',
                      original_preserved=True, verification_roots_distinct=True,
                      claim_scope='successful tracked-file branch only; no C# execution'))

    # Static Lazy belongs to one loaded type, regardless of how many logical
    # invocations call the named writer. No timestamp collision is needed.
    lazy_root = root / 'c4' / 'attempt' / 'runs' / 'first-timestamp-pid'
    first_invocation = lazy_root
    second_invocation = lazy_root
    write_like_sink(first_invocation, 'invocation-A')
    write_like_sink(second_invocation, 'invocation-B')
    assert (lazy_root / 'metrics.json').read_text() == 'invocation-B'
    cases.append(dict(id='C4-P56-static-lifetime', kind='SOURCE_TRANSLATION_FILESYSTEM',
                      same_loaded_type_invocations=2, distinct_roots=1,
                      same_named_receipt_replaced=True,
                      limitation='No claim that xUnit ordinarily repeats the same Fact in one process.'))

    # A reference control for the proposed invariant, NOT a repaired sink test.
    claim_root = root / 'c5'; claim_root.mkdir()
    callback_counts = [0, 0]
    for owner in range(2):
        try:
            with (claim_root / 'owner.claim').open('x', encoding='utf-8') as handle:
                handle.write(str(owner))
            callback_counts[owner] += 1
        except FileExistsError:
            pass
    assert callback_counts == [1, 0]
    cases.append(dict(id='C5-exclusive-create-reference-control', kind='REFERENCE_ORACLE_ONLY',
                      winners=1, callback_counts=callback_counts,
                      implementation_acceptance=False))

    child_root = root / 'c6'; child_root.mkdir()
    child_code = "import json,pathlib,sys; pathlib.Path(sys.argv[1]).write_text(json.dumps({'measurement_outcome':'SUPPORTED','exit_code':0})); sys.exit(7)"
    completed = subprocess.run([sys.executable, '-c', child_code, str(child_root / 'inner.json')],
                               capture_output=True, text=True, timeout=10)
    inner = json.loads((child_root / 'inner.json').read_text())
    assert completed.returncode == 7 and inner['exit_code'] == 0
    cases.append(dict(id='C6-inner-success-then-child-failure', kind='REAL_PYTHON_CHILD_BOUNDARY',
                      inner_exit_field=0, actual_child_exit=7, raw_retained=True,
                      required_outer_outcome='FAILED', csharp_or_launcher_execution=False))

    missing = root / 'c7' / 'missing.trx'
    assert not missing.exists()
    journal = dict(state='COMPLETED_GREEN', total=1, passed=1, failed=0, not_executed=0)
    # Exact fallback + label rules from summarize-test-batches.ps1 536-560.
    counts = {k: journal[k] for k in ('total', 'passed', 'failed', 'not_executed')}
    open_count = 0 if journal['state'].startswith('COMPLETED_') else 1
    readable = 1 if journal['state'].startswith('COMPLETED_') else 0
    assert counts['total'] == counts['passed'] + counts['failed'] + counts['not_executed']
    assert counts['passed'] == 1 and open_count == 0 and readable == 1
    cases.append(dict(id='C7-missing-TRX-fallback', kind='POWERSHELL_BRANCH_TRANSLATION',
                      trx_exists=False, counts=counts, OpenCount=open_count,
                      ReadableBatchCount=readable,
                      limitation='A summary provenance defect; not proof the native wrapper returned PASS.'))

    for label, solution_content in (
        ('no-solution-synthetic-control', None),
        ('empty-present-solution', ''),
        ('all-declared-projects-missing', 'Project("{TYPE}") = "Gone", "gone/Gone.csproj", "{GUID}"\n'),
        ('valid-declared-project-control', 'Project("{TYPE}") = "Real", "Real.csproj", "{GUID}"\n'),
    ):
        fixture_root = root / 'c8' / label
        foreign = fixture_root / 'artifacts' / 'foreign-copy' / 'Foreign.csproj'
        foreign.parent.mkdir(parents=True); foreign.write_text('<Project/>')
        if solution_content is not None:
            (fixture_root / 'LlmFromScratch.sln').write_text(solution_content)
        if label == 'valid-declared-project-control':
            (fixture_root / 'Real.csproj').write_text('<Project/>')
        candidates, solution_count, declared_count = project_candidates(fixture_root)
        includes_foreign = 'artifacts/foreign-copy/Foreign.csproj' in candidates
        assert includes_foreign == (label != 'valid-declared-project-control')
        cases.append(dict(id='C8-' + label, kind='CSHARP_ENUMERATION_TRANSLATION_FILESYSTEM',
                          solution_files=solution_count, existing_declared_projects=declared_count,
                          candidate_projects=candidates, undeclared_candidate_included=includes_foreign,
                          limitation='Candidate enumeration only; no compiled-project/test discovery claim.'))
    return cases


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repo-root', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    # Reserve the output before any probe. An existing receipt is immutable;
    # exclusive creation also closes the check-then-open race.
    args.output.parent.mkdir(parents=True, exist_ok=True)
    output_handle = args.output.open('x', encoding='utf-8')
    try:
        run_claimed(args, output_handle)
    finally:
        output_handle.close()


def run_claimed(args: argparse.Namespace, output_handle) -> None:
    texts, sources = read_sources(args.repo_root)
    sink = texts['Tests/ExperimentFixes/ExperimentEvidenceSink.cs']
    require(sink, 'if (!published)\n            return new ExperimentEvidenceSink(declared, declared, reason, redirected: false);')
    require(sink, 'File.WriteAllText(path, exactPayload, Utf8WithoutByteOrderMark);')
    p56 = texts['Tests/ExperimentFixes/TranslationRequiredWholeTermOrdinaryPathTests.cs']
    require(p56, 'private static readonly Lazy<string> AllowedReceiptDirectory')
    launcher = texts['scripts/experiment-launcher/ExperimentLauncher.psm1']
    require(launcher, '[IO.FileShare]::None')
    summarizer = texts['tools/test-batches/summarize-test-batches.ps1']
    require(summarizer, 'if ($null -eq $counts)')
    require(summarizer, '$readableBatchCount++')
    inventory = texts['Tests/TestSuiteBatchManifest.cs']
    require(inventory, 'return declaredProjects.Count > 0')
    with tempfile.TemporaryDirectory(prefix='codex-attempt-counterexamples-') as temporary:
        cases = probe(Path(temporary))
    output = dict(schema='codex-attempt-audit-counterexamples/v1', source_commit=SOURCE_SHA,
                  evidence_scope='Pinned source translations plus local filesystem and one Python-child probe. No native C#/PowerShell execution.',
                  csharp_native_tests_executed=0, powershell_native_tests_executed=0,
                  train_calls=0, production_changes=0, sources=sources, cases=cases,
                  unavailable_native_tools=[name for name in ('dotnet', 'pwsh') if shutil.which(name) is None],
                  result='ALL_DECLARED_OBSERVATIONS_REPRODUCED_NOT_PRODUCT_ACCEPTANCE')
    output_handle.write(json.dumps(output, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps(dict(result=output['result'], source_files=len(sources), observation_records=len(cases),
                          native_tests=0, train_calls=0), ensure_ascii=False))

if __name__ == '__main__':
    main()
