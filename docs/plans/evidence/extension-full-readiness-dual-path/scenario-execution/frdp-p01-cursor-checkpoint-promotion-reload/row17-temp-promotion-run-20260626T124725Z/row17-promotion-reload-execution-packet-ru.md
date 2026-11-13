# Row17 checkpoint promotion/reload execution

Статус: `ROW17_PROMOTION_RELOAD_EXECUTION_PASS_REVIEW_REQUIRED`

PASS: `True`

Run folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z`

Команда: `dotnet run --project C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z\row17-promotion-reload-probe\Row17PromotionReloadProbe.csproj -- --run-folder C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z --source-root C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export --harness-dir C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z\row17-promotion-reload-probe --expected-checkpoint-sha256 c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb --expected-manifest-sha256 4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475 --log-path C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z\logs\dotnet-run-row17-promotion-reload-probe-rerun3.log`

## Hash evidence

- source checkpoint before: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- source manifest before: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`
- temp checkpoint before reload: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- temp manifest before reload: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`
- temp checkpoint after reload: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- temp manifest after reload: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`
- temp checkpoint after rollback: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- temp manifest after rollback: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`
- hashChecksPass: `True`

## Reload/probe evidence

- core import path: `SkillSelectionMachine.ImportAsync`
- core load path: `SkillSelectionMachine.LoadCheckpointAsync`
- expected selected skill: `client-adaptive-learning-loop`
- loaded selected skill: `client-adaptive-learning-loop`
- imported selected skill: `client-adaptive-learning-loop`
- reloadProbePass: `True`

## Rollback evidence

- active locator before switch SHA-256: `fad3b5582e907a62dea0b5a3313e87b5594c5e22b0a65864e2a00f490309d298`
- active locator after switch SHA-256: `8638a48ea598c6acb977ec2ad53890c599a48fd1229f95d67a74ed2e45e524c0`
- active locator after rollback SHA-256: `fad3b5582e907a62dea0b5a3313e87b5594c5e22b0a65864e2a00f490309d298`
- baseline locator SHA-256: `fad3b5582e907a62dea0b5a3313e87b5594c5e22b0a65864e2a00f490309d298`
- rollbackRestoredBaseline: `True`
- rollback evidence JSON: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z\row17-temp-rollback-evidence.json`

## Non-claims

- product readiness: `false`
- token savings: `false`
- full semantic quality: `false`
- training readiness: `false`
- common promotion: `false`
- real production locator update: `false`
- real active locator mutation: `false`
- host UI readiness: `false`
- full joined pass: `false`
- full extension readiness: `false`
- Row17 accepted: `false`

## Narrow human claim

Row17 temp-only core checkpoint promotion/reload proof passed: the copied candidate was selected through a temporary active locator, loaded through core SkillSelection APIs, selected `client-adaptive-learning-loop`, and the temp locator was rolled back to baseline.

JSON packet: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-20260626T124725Z\row17-promotion-reload-execution-packet.json`