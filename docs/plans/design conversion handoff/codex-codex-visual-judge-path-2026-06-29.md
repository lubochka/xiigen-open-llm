# CODEX_VISUAL_JUDGE_PATH - 2026-06-29

claim_scope:
- Created this bounded Codex visual judge path packet.
- Recorded Luba's current decision: `Codex agent is the visual judge`.
- Read the three required handoff packets and searched the available handoff runtime/browser evidence folder for screenshots, design exports, render outputs, and related evidence.
- Performed an artifact sufficiency assessment, not a visual fidelity assessment.
- Did not run runtime/browser/tests/builds, did not inspect secrets, did not call an external AI provider, did not mutate git/PR/source branches, did not clean/revert/stash/restore, did not touch claude-* files, and did not touch packages/traces/corpora/fixtures.

evidence_scope:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-external-visual-judge-owner-decision-collector-2026-06-29.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-proof-task-scenario-queue-2026-06-29.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-runtime-browser-command-discovery-2026-06-29.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\browser-runtime-precheck.json`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\designconversion-browser-real-screenshot-command.json`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\designconversion-browser-real-screenshot-dotnet-test.log`
- Read-only file discovery and folder listings under `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\` and `...\runtime-browser-evidence\`.

count_claimed:
- required handoff packets read: 3
- runtime/browser evidence files found: 3
- usable screenshots/design exports/render outputs found: 0
- image files inspected visually: 0
- external AI calls: 0
- secret values read/captured: 0
- runtime/browser/test/build commands run: 0
- source/package/trace/corpora/fixture mutations: 0

independent_evidence_count:
- 10 current-turn evidence records: 3 required packet readbacks, 1 broad artifact discovery command, 1 focused extension search, 1 handoff directory listing, 1 evidence folder file listing, 2 evidence JSON readbacks, and 1 exact evidence folder metadata listing.

subagent_id_or_NONE:
- Codex visual judge sub-agent

downgrade_needed:
- Yes. Evidence supports only this path packet and artifact sufficiency assessment.
- It does not support a visual fidelity pass/fail decision because no screenshot, design export, render output, rendered HTML, PDF, SVG, or comparable image artifact was found in the available evidence folder.

## Luba Decision

`Codex agent is the visual judge`.

That replaces the earlier external-provider decision path for this bounded task. The safe path here is:

- `no external AI call`
- `no secrets captured`
- no Claude/OpenAI/Gemini provider selection needed
- no provider account, endpoint, model, secret env var, or external upload needed

## What Codex Visual Judging Can Prove

Простыми словами:

Codex может посмотреть на локальные изображения, скриншоты, HTML/PDF/SVG exports или другие сохраненные визуальные артефакты, если они уже есть в evidence. После этого Codex может описать видимые различия: что видно, чего не видно, какие элементы похожи или расходятся, где есть явные проблемы с layout, spacing, typography, color, clipping, overlap или missing content.

Codex может доказать только узкую вещь: что именно видно в конкретных локальных артефактах, которые были реально найдены и просмотрены в этом запуске.

Codex не может доказать:

- что runtime/browser реально работал, если отдельный runtime/browser worker не создал evidence;
- что UI полностью совпадает с source design, если нет source design reference/export рядом с runtime screenshot;
- что продукт готов к merge/release;
- что все viewports, states, clicks, login/session paths, fonts, animations, hover/focus states или responsive layouts проверены;
- что внешний AI judge прошел, потому что здесь должен быть `no external AI call`;
- что secrets безопасны за пределами факта `no secrets captured`, потому что secret values не читались.

`visual fidelity proof is a task not a stop blocker`.

По-человечески: отсутствие screenshots/design exports сейчас не значит "финально заблокировано". Это значит, что следующему runtime/browser evidence worker нужна точная задача на генерацию визуальных artifacts.

## Artifact Search Result

Artifacts found in the available handoff runtime/browser evidence folder:

| Path | Type | Size | Visual-use status |
| --- | --- | ---: | --- |
| `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\browser-runtime-precheck.json` | JSON precheck | 345 bytes | Not a visual artifact. Shows default Chromium-family executable found: `msedge.exe`. |
| `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\designconversion-browser-real-screenshot-command.json` | JSON command record | 554 bytes | Not a visual artifact. Records intended `dotnet test` command and TRX logger path. |
| `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\designconversion-browser-real-screenshot-dotnet-test.log` | Log | 0 bytes | Empty; no visual or runtime result evidence. |

Focused extension search found no files ending in:

- `.png`
- `.jpg`
- `.jpeg`
- `.webp`
- `.bmp`
- `.gif`
- `.svg`
- `.html`
- `.htm`
- `.pdf`

No TRX result file was found in the exact evidence folder listing.

## Assessment Performed

assessment_performed: no full visual assessment.

What was performed:

- artifact availability assessment;
- evidence-boundary assessment;
- Codex visual judge path planning.

What was not performed:

- no image comparison;
- no screenshot inspection;
- no rendered-output inspection;
- no source-design comparison;
- no pass/fail visual fidelity judgment.

Reason:

There were not enough local visual artifacts. I found only command/precheck evidence and an empty log. Since there is no screenshot or design export in the available evidence folder, any visual fidelity score would be invented.

## Current Narrow Finding

The evidence supports only:

- Luba selected `Codex agent is the visual judge`;
- the current path needs `no external AI call`;
- `no secrets captured`;
- available runtime/browser evidence is precheck/command metadata, not visual evidence;
- no usable local screenshot/design-export/render-output artifact was found in the searched handoff evidence folder.

## Next Bounded Work Order

Worker:
- `bounded-runtime-browser-visual-evidence-generation-worker`

Purpose:
- Generate or collect local visual artifacts for Codex visual judging without any external AI provider and without secrets.

Allowed read inputs:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-runtime-browser-command-discovery-2026-06-29.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\browser-runtime-precheck.json`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\designconversion-browser-real-screenshot-command.json`
- approved runtime/browser scenario prep packet if one exists or is supplied

Allowed write output:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence\`

Exact requested artifacts:

1. Runtime/browser command transcript with cwd, exact command, timestamps, stdout/stderr, and exit code.
2. TRX or equivalent result file if `dotnet test` is run.
3. Actual screenshot file copied into the evidence folder, preferably named `designconversion-browser-real-screenshot.png`, if the test produces one.
4. SHA256 for every screenshot or render output copied into evidence.
5. If no screenshot is produced, exact blocker line such as `BROWSER_RUNTIME_UNAVAILABLE` or the exact test failure.
6. If a source design reference exists and is approved for local use, copy or reference the exact local design export path; otherwise state `source design reference missing`.

Allowed execution lane, if approved by Luba/parent work order:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionBrowserQaProviderTests.LocalChromiumProcessAdapter_CapturesRealScreenshotWhenRuntimeIsAvailableOrReportsBlocker --results-directory "C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\runtime-browser-evidence" --logger "trx;LogFileName=designconversion-browser-real-screenshot.trx"
```

Required boundaries:

- no external AI call;
- no secrets captured;
- no provider/model/endpoint/secret-env-var decision needed;
- do not read or print environment variable values;
- no git/PR/source branch mutation;
- no cleanup/revert/stash/restore;
- do not touch claude-* files;
- `packages/traces/corpora/fixtures stay untouched unless needed for a concrete proof task`.

Stop/downgrade rules:

- If screenshot generation fails, record the exact failure and do not claim visual proof.
- If only runtime metadata exists, claim only runtime metadata.
- If only screenshot exists without source design reference, Codex may judge visible UI sanity but not source-design fidelity.
- If source design exists without runtime screenshot, Codex may describe the design reference but not implementation fidelity.
- If both runtime screenshot and source design export exist, Codex can perform a bounded visual comparison and list visible mismatches, still without product readiness or merge readiness claims.

## Future Codex Visual Judge Worker

Worker:
- `bounded-codex-visual-assessment-worker`

Use only after visual artifacts exist.

Inputs needed:
- runtime screenshot path(s);
- source design export/reference path(s), or explicit source-reference waiver;
- viewport/device names;
- expected UI state;
- local-only rubric;
- allowed output report path.

Output:
- local text assessment only;
- no external upload;
- no external AI;
- no secrets;
- visible differences and missing evidence;
- narrow claim boundary.

## Files Changed

- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-codex-visual-judge-path-2026-06-29.md`

## Verification

Post-write verification should read this file and check exact required literals:

- `CODEX_VISUAL_JUDGE_PATH`
- `Codex agent is the visual judge`
- `no external AI call`
- `no secrets captured`
- `visual fidelity proof is a task not a stop blocker`
- `packages/traces/corpora/fixtures stay untouched unless needed for a concrete proof task`

No runtime/browser/test/build/git/external-provider verification is implied by this document.

## Final Boundary

This is a Codex visual judge path packet, not a completed visual fidelity proof.

Visual fidelity proof remains an executable evidence task. The next safe followup is to generate/copy concrete local screenshots and any approved local source-design exports into the evidence folder, then run the bounded Codex visual assessment over those local artifacts.
