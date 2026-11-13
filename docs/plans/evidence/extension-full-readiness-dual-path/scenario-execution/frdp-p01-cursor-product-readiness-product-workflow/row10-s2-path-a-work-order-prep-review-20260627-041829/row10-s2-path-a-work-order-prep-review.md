# Row10-S2 Path A Work-Order Prep Review

```text
claim_scope: review of Row10-S2 Path A work-order prep Markdown/JSON only; no Path A execution, no product/API scenario execution, no Cursor UI, no runtime invocation, no tests/builds beyond JSON/text/whitespace checks, no code edits, no board/status edits, no training/checkpoint/locator/lifecycle, no commit/push, no web/external AI, no nested subagents
evidence_scope: two target prep files read; JSON parse; prompt text/hash computation and Markdown/JSON agreement checks; request/response/trace shape checks; output filename checks; claim-lock checks; packet-text boundary/interface check; prescribed git diff whitespace command plus direct text whitespace scan
count_claimed: 1 review packet folder, 1 Markdown review packet, 1 JSON review packet, 1 PASS verdict for prep readiness only
independent_evidence_count: 5 usable evidence command outputs plus 1 recorded PowerShell parameter-error attempt that was not counted as JSON parse evidence
subagent_id_or_NONE: Row10-S2-PathA-prep-reviewer-current-session
downgrade_needed: yes for external boundary verification: I did not independently read source files or execute the interface; evidence supports only that the named interface is concrete and not obviously invented from the packet text
```

## Verdict

PASS for Row10-S2 Path A work-order prep.

This is not a Path A execution pass. I did not run `tools\xiigen-extension-core-api.ps1`, did not invoke `XiigenExtensionApiService`, did not run Cursor UI, did not run product/runtime/tests/builds, and did not verify product readiness. The evidence supports only that the work-order packet is precise enough to hand to a future bounded Path A executor after this review.

## Checked Files

- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s2-path-a-work-order-prep-20260627-041013\row10-s2-path-a-work-order-prep.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s2-path-a-work-order-prep-20260627-041013\row10-s2-path-a-work-order-prep.json`

## Findings

- Work-order precision: PASS. The packet defines future executor scope, forbidden actions, request/response/trace shapes, output filenames, pass/fail assertions, blocked states, evidence boundaries, and exact next review/execution separation.
- Exact prompt and hash: PASS. Markdown exact prompt and JSON `exact_prompt.text` match. Computed SHA256 over UTF-8 LF/no trailing newline equals `e5334133911acf3638934d4a752e9924baf2a3209d5277df58b56cb1097f9506`. JSON declared hash, request shape hash, and trace expected hash also match.
- JSON parse: PASS. Companion JSON parsed successfully after rerunning without unsupported `ConvertFrom-Json -Depth`.
- Request JSON shape: PASS. Required future request properties are present, including `TargetLocator` as an explicit nullable property.
- Response JSON shape: PASS. Required top-level response fields and PASS/FAIL assertions are present.
- Trace JSON shape: PASS. Required trace fields and FAIL conditions are present.
- Output filenames: PASS. Markdown and JSON agree on request, response, trace, adapter/core boundary packet, thin Cursor boundary evidence, and execution report filenames.
- Concrete boundary/interface: PASS with evidence-boundary downgrade. The packet names `tools\xiigen-extension-core-api.ps1`, parameters, `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj`, and `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`. Based only on available packet text, it is not obviously invented because the packet records `KNOWN_FROM_S1_AND_READ_ONLY_CONTRACT_INSPECTION` and cites the same interface consistently. I did not independently verify those source files in this review.
- Claim locks: PASS. Markdown and JSON keep product readiness, Cursor UI readiness, Cursor host readiness, token savings, semantic quality, training/learnability, checkpoint promotion, locator update, lifecycle readiness, runtime execution, and Path B UI execution claims false.
- Boundary checks: PASS. The packet requires logic in `llm_mvp_core`, thin extension evidence, no hidden deterministic shortcut, and no candidate-position-like masquerade.
- Whitespace: PASS with command note. The prescribed `git diff --check --no-index` command was run against the two packet files and emitted no whitespace diagnostics, but returned exit code 1 due `--no-index`/different-file semantics. A direct text scan found no trailing whitespace, no UTF-8 BOM, and LF-only line endings in both target files.

## Evidence Boundary

I did not verify this beyond packet text: existence or current behavior of `tools\xiigen-extension-core-api.ps1`, `43.SmartSkillsDomain\43.SmartSkillsDomain.csproj`, or `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`.

I did not run this: Path A, product/API scenario, Cursor UI, runtime invocation, tests/builds, training, checkpoint, locator, lifecycle, commit, push, web/external AI, or nested subagents.

The evidence supports only: the prep packet is ready for a separate bounded Path A executor.

## Exact Next Bounded Path A Execution Task

Assign a separate bounded Row10-S2 Path A executor, maximum 10 active minutes.

Authority: this review verdict PASS only authorizes a future bounded Path A execution, not Path B UI and not product readiness.

Allowed read scope:

- The reviewed prep Markdown and JSON packet.
- The concrete boundary files named by the prep only as needed to execute the command/interface and validate the expected contract.

Allowed write scope:

- Create one fresh execution evidence folder under `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-product-readiness-product-workflow\row10-s2-path-a-execution-YYYYMMDD-HHMMSS\`.
- Write only the future request, response, trace, adapter/core boundary packet, optional thin Cursor boundary evidence or explicit BLOCKED marker, and execution report files named by the prep.

Required execution:

1. Create `row10-s2-path-a-request.json` with the exact prompt and hash from the prep.
2. Run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\xiigen-extension-core-api.ps1 -RequestPath "<future-run-folder>\row10-s2-path-a-request.json" -ResponsePath "<future-run-folder>\row10-s2-path-a-response.json" -TracePath "<future-run-folder>\row10-s2-path-a-trace.json" -CoreRoot "C:\Projects\llm_mvp_core"
```

3. Parse and validate request/response/trace against the prep assertions.
4. Create `row10-s2-path-a-adapter-core-boundary.json`.
5. Create or collect `row10-s2-path-a-host-boundary-cursor.json`; if not authorized or unavailable, mark `BLOCKED_MISSING_CURSOR_HOST_THIN_BOUNDARY_EVIDENCE` and do not PASS the thin-boundary subcheck.
6. Write `row10-s2-path-a-execution-report.md` with claim locks still false.

Forbidden for that executor:

- Cursor UI execution, Path B UI proof, product readiness claim, token savings claim, semantic quality claim, training/learnability claim, checkpoint promotion, locator update, lifecycle readiness, hidden deterministic shortcut, candidate-position-like masquerade, board/status edit, commit/push, web/external AI, nested subagents.
