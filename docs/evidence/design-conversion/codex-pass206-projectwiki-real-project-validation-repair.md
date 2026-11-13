# Pass206 ProjectWiki Real Project Validation Repair

claim_scope: Pass206 bounded ProjectWiki verification-gate repair for `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`. This is not product complete, not 50/50, not full CI, not deployed, not a public endpoint, not provider execution, not production checkpoint readiness, not git publication, not PR readiness, not cloud readiness, and not full local suite passed.
evidence_scope: Fresh-read required rules and skills by command, Pass205 artifact read, current ProjectWiki tests and real-project validation runner files inspected, exact focused pre-fix timeout reproduced, one minimal test-local fixture-bound repair applied, exact focused test passed after rebuild, exact focused blame-hang rerun passed, relevant real-project validation runner cluster passed, DesignConversion regression passed 247/247, ProjectWiki class probe attempted but inconclusive due outer command timeout and cleaned stale test processes.
count_claimed: 1 Pass206 bounded pass for repairing the exact existing-project validation timeout/failure and verifying selected gates; 0 full-suite, 0 ProjectWiki class-cleared, 0 product-complete, 0 CI, 0 publication claims.
independent_evidence_count: 18 grouped command/evidence records listed below.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; used_false_no_subagent_claim=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes. The exact focused test and relevant runner cluster are repaired and verified, but the broader ProjectWiki class gate is not proven because the class blame-hang probe timed out at the outer command boundary without usable sequence output.
requested_model: gpt-5.5
requested_reasoning_effort: xhigh

## Skill Load Status

| File | Status | Evidence |
| --- | --- | --- |
| `AGENTS.md` | found/read by command | chars 407799; sha256 `a8f6eb15692f727cd0ec5623e2c13109f3a1ac7c9ea31e4b3dc63c16e56f5d69`. |
| `docs/SESSION-START-PROMPT.md` | found/read by command | chars 42267; sha256 `450961e74c8e9db4caa6d59441e8bfc76f2bff973072ed13b8bf6d69df0d9a8c`. |
| `docs/skills/claim-verification-SKILL.md` | found/read by command | chars 65698; sha256 `f6d84c0f3815367c26012a81b6c8c6db37eb4fdd2144263842785a4e694a0d68`. |
| `docs/skills/audit-protocol-SKILL.md` | found/read by command | chars 35531; sha256 `ff0c6898debd3bf21ec538d6c32f1444044d545dc8996198a24fb3f3876d9d0a`. |
| `docs/skills/output-skepticism-SKILL.md` | found/read by command | chars 21007; sha256 `afe71fa8a2e944d0b65cf6a34acd5ddec9ee6294f89c086c66769086d45d4c8a`. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read by command | chars 38160; sha256 `18c33dd8c4d74770d5daa0a8350bd18fa3f0756f13c05d5e2138e1ab5600dcf1`. |
| `docs/skills/test-integrity-SKILL.md` | found/read by command | chars 41002; sha256 `fd545554bc746e7851119cc5bd9ec59b122f3194cd577a036220a5c17af1805b`. |
| `docs/skills/integration-testing-SKILL.md` | found/read by command | chars 9104; sha256 `6cb1698dc95c6f77e96f54bfc81cd39c8aebd878542cbb0aa8d09127d0996d8a`. |
| `docs/skills/generated-code-review-SKILL.md` | found/read by command | present QA skill; chars 30853; sha256 `ceb6cf6bbfff4121db8fa898a21765afda0cf70e2f9334cd9fa8f171c21edcc4`. |
| `docs/skills/code-examination-SKILL.md` | found/read by command | present QA skill; chars 14801; sha256 `38df19ab6d80a0ef247d538cd3a0d6ebe82c850fc76a099c6c4a412ae38d6aa6`. |
| `docs/skills/test-failure-triage-SKILL.md` | found/read by command | present QA skill; chars 8087; sha256 `151992d53fe19d0d3988882f4474d2d77cc96137f820b3abc9a15a383b507920`. |
| `docs/skills/e2e-test-matrix-SKILL.md` | found/read by command | present QA skill; chars 10011; sha256 `a4c1af58afe0966699b9c7872d05a46a017a6c51d216836f3fee5d18fa86282b`. |

## Context Read

| File or source | Status | Use |
| --- | --- | --- |
| `docs/evidence/design-conversion/codex-pass205-projectwiki-request-router-orchestrator-timeout-repair.md` | found/read | Confirmed accepted Pass205 and its next blocker: `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent`. |
| `Tests/ProjectWikiDomainTests.cs` | found/read | Confirmed exact test used five fixed optional external project roots before this pass and default unbounded validation. |
| `42.ProjectWikiDomain/ProjectWikiRealProjectValidationRunner.cs` | found/read | Confirmed `ValidateProjectsAsync` defaulted `maxEvidenceRecordsPerProject` to `int.MaxValue` and generated questions for every selected evidence record. |
| `42.ProjectWikiDomain/ProjectWikiQuestionSeedGenerator.cs` | found/read | Confirmed nine basic questions are generated per selected evidence record. |
| `42.ProjectWikiDomain/ProjectWikiCoverageVerifier.cs` | found/read | Used as comparison for existing wiki coverage tests already bounded differently. |
| `42.ProjectWikiDomain/ProjectWikiDomainModels.cs` | found/read line window | Confirmed validation report fields used by assertions. |
| External optional roots under `C:\Projects` | read-only inspection | All five configured wiki roots existed; evidence line counts were 204, 240, 6, 236, and 114. No external files were mutated. |

## Mental Simulation

Pre-repair path:

1. The exact test built `existingProjectRoots` from five fixed local `C:\Projects` paths.
2. On this machine, all five `.xiigen/wiki-domain` directories existed.
3. The test called `ValidateProjectsAsync(existingProjectRoots)` with the default unbounded `maxEvidenceRecordsPerProject`.
4. The runner loaded each wiki, generated nine basic questions per evidence record, added one developer-reasoning question per selected evidence record, and added 45 prompt-validation questions per project plus stored-procedure prompt extensions when applicable.
5. With 800 total local evidence lines across those five roots, the verification gate became a broad optional local-data workload rather than a bounded class test. The 45 second blame-hang run aborted while the exact test was active.

Post-repair path:

1. The exact test now creates a bounded temp project-shaped fixture with `SampleApp.csproj`, sample C# source files, a SQL procedure file, and an existing `.xiigen/wiki-domain/evidence/evidence-index.jsonl`.
2. The test passes that fixture root to the same `ProjectWikiRealProjectValidationRunner.ValidateProjectsAsync` API.
3. The call sets `maxEvidenceRecordsPerProject: 3`, matching the fixture evidence count and preserving the real validation assertions: project count, exact evidence accuracy, query-planning invocation rate, entity-impact trace pass rate, question count formulas, and per-project exact metrics.
4. Optional external local project discovery no longer decides whether this test times out.

## Root Cause

The timeout was caused by optional external/local project data discovery and unbounded validation, not by a proven deadlock in `ProjectWikiRealProjectValidationRunner`. The test depended on machine-local wiki directories under `C:\Projects`; because all five existed here, it attempted a broad validation workload over hundreds of evidence records and thousands of generated questions.

## Repair Summary

Patched only `Tests/ProjectWikiDomainTests.cs` for code/test behavior in this pass.

1. Replaced the fixed external `C:\Projects` scan in `ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent` with `using var project = CreateProjectWithWikiDomain();` and `existingProjectRoots = [project.Root]`.
2. Passed `maxEvidenceRecordsPerProject: 3` to keep the validation bounded while still exercising the real runner path.
3. Expanded `CreateProjectWithWikiDomain()` so the fixture is project-shaped, not just a loose wiki folder: it now writes `SampleApp.csproj`, C# source files matching the evidence paths, and a SQL procedure file matching the stored-procedure evidence.

The repair did not skip, delete, or weaken the exact test. It removed the local-machine external data dependency while preserving real/project-shaped wiki validation through the existing public runner.

## Files Changed

| File | Change |
| --- | --- |
| `Tests/ProjectWikiDomainTests.cs` | Pass206 authored the exact existing-project validation fixture-bound repair and project-shaped fixture additions. The full file diff also shows prior accepted ProjectWiki timeout-repair hunks from earlier passes; those prior hunks were not authored by Pass206. |
| `docs/evidence/design-conversion/codex-pass206-projectwiki-real-project-validation-repair.md` | This bounded evidence artifact. |

## Commands Run With Exit Codes

| # | Command or action | Exit | Evidence summary |
| ---: | --- | ---: | --- |
| 1 | Fresh-read `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `claim-verification`, `audit-protocol`, and `output-skepticism` | 0 | Required governance and claim-boundary files read by command. |
| 2 | Fresh-read `implementation-integrity`, `test-integrity`, `integration-testing`, optional QA skill presence, and Pass205 artifact | 0 | Required implementation/test skills read; optional QA skills found. |
| 3 | Fresh-read optional QA skills, `Tests/ProjectWikiDomainTests.cs`, and targeted ProjectWiki symbol search | 0 | Optional QA skill contents read; current test file and symbol map inspected. |
| 4 | Read `ProjectWikiRealProjectValidationRunner.cs`, question seed generator, coverage verifier, relevant test window, and external root presence | 0 | Confirmed unbounded runner default and all five optional local wiki roots existed. |
| 5 | Read exact test window, validation report contracts, external evidence line counts, and `git status --short` | 0 | Confirmed exact test shape and external optional evidence volume. |
| 6 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent" --blame-hang --blame-hang-timeout 45s ...` | 1 | Pre-fix focused reproduction aborted after 45 seconds; active test was the exact named test. |
| 7 | Read pre-fix sequence XML and process inventory | 0 | Sequence showed the exact named test `Completed="False"`; no stale testhost/vstest after that reproduction. |
| 8 | `apply_patch` on `Tests/ProjectWikiDomainTests.cs` | 0 | Applied the bounded project-shaped fixture repair. |
| 9 | `dotnet test ... --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent"` | 0 | Post-fix build-backed focused run passed 1/1; build emitted unrelated pre-existing warnings. |
| 10 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiRealProjectValidationRunner_ValidatesExistingDotNetProjectWikisWhenPresent" --blame-hang --blame-hang-timeout 45s ...` | 0 | Post-fix exact focused blame-hang run passed 1/1 and generated no sequence file. |
| 11 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiRealProjectValidationRunner" --blame-hang --blame-hang-timeout 45s ...` | 0 | Relevant real-project validation runner cluster passed 2/2 and generated no sequence file. |
| 12 | `dotnet test ... --no-build --filter "FullyQualifiedName~ProjectWikiDomainTests" --blame-hang --blame-hang-timeout 60s ...` | 124 | ProjectWiki class probe timed out at the outer command boundary after about 154 seconds with no usable test result. This is inconclusive class evidence, not a class pass or a next-test failure. |
| 13 | Process command-line inspection and exact stale cleanup | 0 | PIDs 6068, 19556, 21180, and 18696 were tied to the timed-out class probe and stopped; only unrelated VBCSCompiler `dotnet.exe` remained. |
| 14 | `dotnet test ... --no-build --filter "FullyQualifiedName~DesignConversion" ...` | 0 | DesignConversion regression passed 247/247 in 1 minute 28 seconds. |
| 15 | `git diff -- Tests\ProjectWikiDomainTests.cs`, changed-line readback, process inventory, file hash | 0 | Diff/readback confirmed Pass206 lines; no active testhost/vstest remained. |
| 16 | Compact skill hash helper | 1 then 0 | First PowerShell helper had a syntax error and was not used; corrected helper produced the skill status table above. |
| 17 | Artifact creation via `apply_patch` | 0 | Created this exact Pass206 evidence artifact. |
| 18 | Artifact marker/readback check, process inventory, and `git diff --check -- Tests\ProjectWikiDomainTests.cs docs\evidence\design-conversion\codex-pass206-projectwiki-real-project-validation-repair.md` | 0 | Verdict count 1; exact spawned identity line count 1; bad identity count 0; conflict marker count 0; mojibake marker count 0; changed-file diff check passed; readback showed clean ASCII text; only unrelated VBCSCompiler `dotnet.exe` remained. |

## Verification

Focused selected gate:

- Pre-fix exact focused blame-hang run aborted after 45 seconds with the named existing-project validation test active.
- Post-fix build-backed focused run passed 1/1.
- Post-fix exact focused blame-hang rerun passed 1/1 in under the inactivity timeout and generated no sequence file.

Relevant validation cluster:

- `FullyQualifiedName~ProjectWikiDomainTests.ProjectWikiRealProjectValidationRunner` passed 2/2 with blame-hang enabled and no sequence file.

DesignConversion regression:

- `FullyQualifiedName~DesignConversion` passed 247/247.

Class probe:

- The broader `FullyQualifiedName~ProjectWikiDomainTests` blame-hang probe was attempted.
- The outer command timed out after about 154 seconds before returning usable test output.
- A new empty test-result directory was observed.
- Exact stale PIDs from that timed-out probe were identified by command line and stopped.
- Therefore this pass does not claim the ProjectWiki class is clear and does not name a next class blocker from that inconclusive command.

## Code Review And Test Integrity Notes

| Check | Result |
| --- | --- |
| Test skipped, deleted, or assertion-weakened | No. The same exact test now always validates a bounded project-shaped wiki fixture instead of returning early when external roots are absent or scanning all local roots when present. |
| Real/project-shaped artifacts preserved | Yes. The fixture includes a `.csproj`, C# source files, SQL procedure file, and `.xiigen/wiki-domain/evidence/evidence-index.jsonl`. |
| External repo mutation | No. External `C:\Projects\...` roots were only read for diagnosis; the repaired test no longer depends on them. |
| Production code changed | No. `42.ProjectWikiDomain` runner code was inspected but not patched. |
| DesignConversion protections weakened | No DesignConversion source or test behavior was changed by this pass; selected regression passed 247/247. |
| Trainable quality overclaimed | No. This pass claims only bounded local ProjectWiki verification-gate repair. |

## Next Timeout Or Failure Identified

No new specific ProjectWiki test blocker was identified. The class probe was inconclusive because the outer command timeout killed the run before a sequence result was available. The repaired exact test and the relevant runner cluster are clear.

## Remaining Broad Suite Blockers

| Scope | Evidence | Status |
| --- | --- | --- |
| Exact existing-project validation test | Post-fix build-backed and blame-hang runs passed 1/1. | repaired in Pass206 |
| Real-project validation runner cluster | Post-fix no-build blame-hang cluster passed 2/2. | repaired in Pass206 |
| `ProjectWikiDomainTests` class | Class probe attempted but command timed out externally; stale processes cleaned. | not proven clear |
| DesignConversion regression | `FullyQualifiedName~DesignConversion` passed 247/247. | selected regression passed |
| Full local suite | Not run. | not claimed |

## Next Recommended Pass

Pass207 should continue the broader ProjectWiki class gate with a smaller segmented `ProjectWikiDomainTests` probe rather than one long class command. Start from the post-Pass206 state and split the remaining class filter into bounded groups so the next blocker, if any, returns a usable sequence record instead of an outer command timeout.

pass206_verdict: PASS_PROJECTWIKI_REAL_PROJECT_VALIDATION_REPAIRED_BOUNDED
