# Evidence scopes HOWTO

This glossary makes the main rule explicit: narrow evidence cannot be silently upgraded to readiness. Each scope states what it can prove, what it cannot prove and what safe wording must be used.

## Total / Done / Now / Remaining

| Field | Status |
|---|---|
| Total | It is necessary to distinguish dashboard/API, handler-level, CLI/helper, package smoke, live host runtime, token, quality, DPO/lifecycle, docs QA and final reports. |
| Done | This glossary describes scopes and safe wording; the docs QA review checks that the updated docs do not upgrade narrow evidence to readiness. |
| Now | Documentation coverage improved; product readiness claims remain false; token 0/9, quality 0/9, DPO 0/9, lifecycle 0/9, final reviews 0/10. |
| Remaining | After the docs QA review, the real evidence tracks remain: live runtime, token pairing, quality scoring, DPO/lifecycle and final reviews only after evidence exists. |

## Scope table

This table covers: Dashboard/API; Handler-level and CLI helper; Package smoke and test-only adapter; Live host runtime; Token/quality/local lifecycle/final report.

| Evidence scope | What it proves | What it cannot prove | Safe wording |
|---|---|---|---|
| Dashboard/API | Dashboard-to-core request/response, human labels, examples, result/debug/raw JSON display. | Live Claude/Cursor/Codex runtime, token savings, quality improvement, trained model readiness. | "This proves dashboard/API behavior only." |
| Handler-level | A named handler or `XiigenExtensionApiService` path handles the supplied request. | Host adapter load, host-visible chat/UI, packaging, extension installability. | "This is handler-level evidence, not host runtime evidence." |
| CLI helper/local adapter | A local command can format a request and reach core boundary. | Live host UI/app/plugin readiness. | "This is helper evidence only." |
| Package smoke | A package or installed adapter responds narrowly. | Full extension functionality, negative paths, token/quality/learning. | "This is smoke evidence only." |
| Test-only adapter | A safe intermediate adapter calls the production boundary. | Production extension behavior. | "This may support implementation but cannot close host runtime." |
| Live host runtime | Named Claude/Cursor/Codex surface accepted prompt, called core, displayed answer/error/debug with correlation/freshness. | Token improvement, quality improvement, learnability, another host/surface. | "This proves the named host runtime scenario only." |
| Token delta report | Same-pair baseline/adaptive token counts under approved methodology. | Quality, runtime readiness, learnability, target promotion. | "Token change is a cost metric and requires quality non-regression." |
| Quality eval report | Rubric quality on evaluated scenarios with controls. | Learning, common readiness, future unseen behavior. | "This proves quality on the evaluated set only." |
| Local DPO candidate | A correction can be represented as chosen/rejected/provenance candidate. | Training happened, learning happened, common readiness. | "This is DPO candidate evidence, not training evidence." |
| Continue-training report | Approved local training continued from an existing checkpoint. | Common readiness, semantic quality, target promotion. | "This proves approved local continue-training only." |
| Fresh-load report | A target checkpoint can be loaded through public boundary. | Better quality, no memorization, rollback safety. | "Fresh-load proves loadability only." |
| Held-out/ablation report | Named controls did not show the tested leakage/memorization failure. | Host runtime, token savings, license safety. | "This proves the named controls only." |
| Rollback report | Named rollback checkpoint can be restored. | Target quality or promotion. | "Rollback proves recovery only." |
| Documentation QA report | Named docs exist, are readable, include required boundaries and avoid overclaim wording. | Product behavior described by the docs. | "This proves documentation coverage only." |
| Final full-extension report | All required packets exist and pass for named scope. | Anything outside named packets and approved tracks. | "Full functionality is claimed only for the packet-backed scope listed in the report." |

## Forbidden upgrades

- Dashboard/API -> live host runtime: forbidden.
- Handler/helper/package smoke -> full extension: forbidden.
- Token delta -> quality improvement: forbidden without quality gate.
- DPO candidate -> learnability: forbidden without approved training/lifecycle evidence.
- Fresh-load -> model readiness: forbidden without held-out/ablation/rollback.
- Documentation QA -> product behavior: forbidden.

## Next bounded work order

After `DOCS-QA-DOCUMENTATION-UPDATE-REVIEW-01_NO_FINAL_READINESS`, the next bounded work order must be a separate missing-evidence packet, for example `RUNTIME-CLAUDE-CURSOR-CODEX-LIVE-EVIDENCE-PLAN-OR-REPAIR-01`. Documentation QA can pass only as documentation coverage; it does not create runtime, token, quality, learning or final readiness evidence.

## Phase 13 no-overclaim gate

Phase 13/14: QA automation, documentation, and no-overclaim evidence.

What this gate accomplishes: Phase 13 collects supporting evidence into a single verifiable QA packet, but does not upgrade it to final readiness.

Phase 13 command:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\phase13-qa-docs-no-overclaim.ps1 -Json
```

How to read the result:

- `PASS_PHASE13_SUBAGENT_SLICE_PARENT_BOUNDARY_REMAINS` means: the QA/docs/no-overclaim slice passed, but the parent boundary is still open.
- `Documentation QA report` stays documentation coverage only.
- `Dashboard/API` stays dashboard-to-core evidence only.
- `Continue-training report`, `Fresh-load report`, `Held-out/ablation report` and `Rollback report` prove only their named lifecycle row.
- The Phase 13 packet does not replace Phase 14 final readiness passes.
