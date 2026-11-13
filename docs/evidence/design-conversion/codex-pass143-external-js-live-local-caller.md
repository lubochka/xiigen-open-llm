# Codex Pass143 - External JS Live Local Caller Proof

claim_scope:
Two bounded Pass143 live-local caller proofs: the actual exported TypeScript
Figma and Canva caller clients each called a real ASP.NET Core Kestrel
ModelServer route on 127.0.0.1:{ephemeral} and parsed refs-only JSON from
`POST /api/design-conversion/improvement-loop`.

evidence_scope:
Current workspace `C:\Projects\llm_mvp_core`; external caller worktrees
`C:\Projects\FigmaToCode-design-gen` and `C:\Projects\CanvaToCode`; required
AGENTS/session/skill reads; requested Pass142, C# host/startup, and TS
client/type/test reads; one additive local Kestrel harness in llm_mvp_core; one
additive Vitest integration test in each caller repo; final focused build/test
commands; existing caller-contract tests in both external repos; post-write
marker, whitespace, and readback checks.

count_claimed:
2 external TypeScript live-local caller proofs. 0 public deployment proofs. 0
production/cloud proofs. 0 external provider calls. 0 model training/upload or
promotion proofs. 0 full product-readiness proofs.

independent_evidence_count:
2 focused live-local caller test records: one Figma Vitest record and one Canva
Vitest record, each spawning a real local Kestrel process and using actual
exported TS client code.

subagent_id_or_NONE:
UNKNOWN_IN_SUBAGENT

parent_platform_spawn_id:
UNKNOWN_TO_WORKER

requested_model: gpt-5.5
requested_reasoning_effort: xhigh

skill_load_status:
- AGENTS.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- planning-session-startup-SKILL.md: found/read.
- architect-behavior-classifier-SKILL.md: found/read.
- authority-requirement-binding-SKILL.md: found/read.
- reference-inventory-and-source-map-SKILL.md: found/read.
- phase-planning-SKILL.md: found/read.
- plan-review-SKILL.md: found/read.
- goal-delivery-completeness-SKILL.md: found/read.
- claim-verification-SKILL.md: found/read.
- audit-protocol-SKILL.md: found/read.
- specificity-calibration-SKILL.md: found/read.
- output-skepticism-SKILL.md: found/read.
- reconnaissance-gate-SKILL.md: found/read.
- trainable-architecture-decision-SKILL.md: found/read.
- no-product-decisions-SKILL.md: found/read.
- output-readability-gate-SKILL.md: found/read.
- design-conversion-ilearningmachine-architecture-GUIDE.md: found/read.
- qa-session-type-SKILL.md: found/read.
- generated-code-review-SKILL.md: found/read.
- test-integrity-SKILL.md: found/read.
- e2e-test-matrix-SKILL.md: found/read.
- implementation-integrity-SKILL.md: found/read.
- verification-before-completion-SKILL.md: found/read.
- bug-to-tests-SKILL.md: found/read.
- test-failure-triage-SKILL.md: found/read.
- self-verification-SKILL.md: found/read.
- agent-output-format-SKILL.md: found/read.

files_changed_by_repo:
- `C:\Projects\llm_mvp_core`: added
  `tools\DesignConversionLiveLocalServer\DesignConversionLiveLocalServer.csproj`,
  added `tools\DesignConversionLiveLocalServer\Program.cs`, and added this
  evidence file.
- `C:\Projects\FigmaToCode-design-gen`: added
  `tests\design-conversion-live-local.test.ts`.
- `C:\Projects\CanvaToCode`: added
  `tests\design-conversion-live-local.test.ts`.

implementation_summary:
- The local harness binds Kestrel to `http://127.0.0.1:0`, configures
  `DesignConversionModelServerAspNetHostStartup`, registers the existing
  improvement-loop route, uses local recording browser/comparison providers,
  writes a BOM-free ready file containing only the localhost base URL, and waits
  for shutdown.
- The Figma integration test imports the actual
  `DesignConversionClient` and `buildDesignConversionImprovementLoopRequest`.
  Because the Figma client default constructor binds browser `window.fetch`, the
  test passes Node's real `globalThis.fetch` to the actual client. The request
  still travels over real localhost HTTP, not fake fetch.
- The Canva integration test imports the actual
  `DesignConversionClient` and `buildCanvaModelServerImprovementLoopRequest`
  and uses the client default fetch path to call the local server.
- Both tests assert request payloads contain private HTML/CSS/tree markers
  before the call, then assert the parsed response reports refs-only/no-raw
  metadata and the serialized response does not contain the marker, raw HTML,
  raw CSS, design-tree payload, or original-tree payload.

commands_run_by_repo:
- `C:\Projects\llm_mvp_core`: required read/search commands for AGENTS,
  session prompt, skill files, Pass142 evidence, C# host/startup files, and
  route/handler references.
- `C:\Projects\llm_mvp_core`: `dotnet build
  tools\DesignConversionLiveLocalServer\DesignConversionLiveLocalServer.csproj`
  failed during harness bring-up, then repairs were applied.
- `C:\Projects\llm_mvp_core`: final `dotnet build
  tools\DesignConversionLiveLocalServer\DesignConversionLiveLocalServer.csproj
  --no-restore` passed.
- `C:\Projects\FigmaToCode-design-gen`: final `npx --no-install vitest run
  tests/design-conversion-live-local.test.ts --reporter=verbose` passed.
- `C:\Projects\CanvaToCode`: final `npx --no-install vitest run
  tests/design-conversion-live-local.test.ts --reporter=verbose` passed.
- `C:\Projects\FigmaToCode-design-gen`: `npx --no-install vitest run
  tests/design-conversion-client.test.ts --reporter=verbose` passed.
- `C:\Projects\CanvaToCode`: `npx --no-install vitest run
  tests/design-conversion-client.test.ts --reporter=verbose` passed.
- `C:\Projects\llm_mvp_core`: fixed-string marker/encoding/identity scan over
  all changed files in all three repos returned no matches.
- All three touched repos: tracked `git diff --check` commands returned no
  output for touched paths.
- All three touched repos: no-index whitespace checks returned
  `NO_INDEX_WHITESPACE_OK` for the new/untracked files.

interim_failures_repaired:
- Initial harness compile failed on top-level helper/type shape; repaired before
  any PASS claim.
- First Figma live test failed on BOM parsing of the ready file; repaired by
  BOM-free server output and tolerant test parsing.
- Second Figma live test failed because the actual Figma client default
  constructor expects browser `window`; repaired by passing real Node fetch.
- Third Figma live test reached Kestrel but returned 500 because the local test
  adapter duplicated a provider-owned metadata key; repaired by removing the
  duplicate adapter metadata. No PASS claim is based on failed runs.

test_or_build_results:
- PASS: `dotnet build
  tools\DesignConversionLiveLocalServer\DesignConversionLiveLocalServer.csproj
  --no-restore` exited 0 with Build succeeded, 0 warnings, 0 errors.
- PASS: Figma live-local Vitest exited 0 with 1 test file passed and 1 test
  passed. The test ran actual `DesignConversionClient.runModelServerImprovementLoop(...)`
  against a real Kestrel route on 127.0.0.1:{ephemeral}.
- PASS: Canva live-local Vitest exited 0 with 1 test file passed and 1 test
  passed. The test ran actual `DesignConversionClient.runModelServerImprovementLoop(...)`
  against a real Kestrel route on 127.0.0.1:{ephemeral}.
- PASS: Figma existing caller-contract Vitest exited 0 with 5 tests passed.
- PASS: Canva existing caller-contract Vitest exited 0 with 7 tests passed.
  Canva Vitest emitted a Vite CJS deprecation warning; it did not fail the test.

marker_scan_results:
PASS: fixed-string scan over the new Kestrel harness, both new Vitest files,
and this evidence file returned `NO_MATCHES` for configured conflict,
encoding, and identity-prohibited markers.

diff_check_results:
PASS: tracked `git diff --check` over touched paths in `llm_mvp_core`,
`FigmaToCode-design-gen`, and `CanvaToCode` exited 0 with no output. PASS:
no-index whitespace checks over all new/untracked files returned
`NO_INDEX_WHITESPACE_OK`.

readback_summary:
PASS: direct readback confirmed the harness binds Kestrel to
`http://127.0.0.1:0`, configures
`DesignConversionModelServerAspNetHostStartup`, writes the ready file, and uses
local recording providers plus no-provider/product-ready-false metadata. PASS:
direct readback confirmed the Figma test imports actual
`DesignConversionClient` and `buildDesignConversionImprovementLoopRequest`,
passes real `globalThis.fetch`, calls
`client.runModelServerImprovementLoop(...)`, asserts 127.0.0.1 loopback
metadata, asserts refs-only/no-raw flags, and asserts the response omits the
private marker plus raw HTML/CSS/tree payloads. PASS: direct readback confirmed
the Canva test imports actual `DesignConversionClient` and
`buildCanvaModelServerImprovementLoopRequest`, calls
`client.runModelServerImprovementLoop(...)`, asserts 127.0.0.1 loopback
metadata, asserts refs-only/no-raw flags, and asserts the response omits the
private marker plus raw HTML/CSS/tree payloads.

mental_simulation:
External TS client -> actual request builder creates HTML, CSS, generated source
tree/design tree, original tree, and design evidence refs containing a private
marker -> actual client method serializes the request and sends POST to
`http://127.0.0.1:{ephemeral}/api/design-conversion/improvement-loop` -> live
Kestrel host configured through `DesignConversionModelServerAspNetHostStartup`
routes through ASP.NET endpoint mapper -> endpoint registry ->
`DesignConversionModelServerImprovementLoopHandler` -> local service
composition providers -> refs-only JSON response -> actual TS client parses the
response into refs-only/no-raw flags. Stop boundary: localhost proof only; no
public deployment, provider, training, or product-readiness claim.

coverage_delta:
Pass142 proved live-local C# caller-shaped payloads only and explicitly left
external JS live caller execution open. Pass143 closes that narrow gap for both
requested caller repos by running their actual exported TS clients against a
real live-local Kestrel route and parsing refs-only JSON.

remaining_gaps:
- Public, production, cloud, and non-local deployment remain not proven.
- External provider execution remains not proven and was not called.
- Model training/upload/promotion and learned-quality readiness remain not
  proven.
- Full product readiness remains not proven.
- Broad CI remains not run.

downgrade_needed:
No downgrade for the narrow Pass143 Figma plus Canva external TypeScript
live-local caller proof after the final focused command passes. Downgrade
remains required for all broader deployment/provider/model/product-readiness
claims.

verdict:
PASS
