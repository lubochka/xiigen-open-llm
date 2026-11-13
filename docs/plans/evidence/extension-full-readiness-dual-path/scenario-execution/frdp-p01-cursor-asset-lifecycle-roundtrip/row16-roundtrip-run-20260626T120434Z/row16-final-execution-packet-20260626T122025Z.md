# Row16 final execution packet

Вердикт: `ROW16_ROUNDTRIP_FIX_EXECUTION_PASS_REVIEW_REQUIRED`

Дата UTC: `2026-06-26T12:20:25Z`

Сценарий: `frdp-p01-cursor-asset-lifecycle-roundtrip`

Run folder:
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z`

## Что исправлено

Предыдущий acceptance blocker был реальным: prep JSON запрещал `DecideFromTrainedState`, а harness `row16-fresh-load-probe\Program.cs` использовал его как final skill-selection probe. В этом fix создан отдельный review/approval artifact, который явно разрешает этот вызов только как read-only probe после `ImportAsync` и `LoadCheckpointAsync`, только для текущего Row16 run folder.

Approval artifact:

- `row16-harness-review-approval-20260626T121838Z.md`
- `row16-harness-review-approval-20260626T121838Z.json`

## Команда rerun

Команда была выполнена один раз:

```powershell
dotnet run --project "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z\row16-fresh-load-probe\Row16FreshLoadProbe.csproj" -- --asset-root "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z\fresh-user-asset-root" --evidence-dir "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z\fresh-load-probe" --expected-checkpoint-sha256 "c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb" --expected-manifest-sha256 "4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475"
```

Log:
`logs\dotnet-run-fresh-load-probe-rerun-20260626T121838Z.log`

Exit code: `0`

## Hash comparison

Primary hash comparison source:
`row16-hash-manifest-pre-probe.json`

That manifest reports `allHashesMatchExpected=true` and `sourceUnchangedAfterCopy=true`.

| Label | File | SHA256 | Match |
| --- | --- | --- | --- |
| source-before | checkpoint | `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb` | PASS |
| source-before | manifest | `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475` | PASS |
| backup-export | checkpoint | `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb` | PASS |
| backup-export | manifest | `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475` | PASS |
| share-package | checkpoint | `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb` | PASS |
| share-package | manifest | `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475` | PASS |
| share-package-readback | checkpoint | `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb` | PASS |
| share-package-readback | manifest | `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475` | PASS |
| fresh-user-asset-root-before-load | checkpoint | `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb` | PASS |
| fresh-user-asset-root-before-load | manifest | `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475` | PASS |
| source-after-copy | checkpoint | `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb` | PASS |
| source-after-copy | manifest | `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475` | PASS |

Fresh-load rerun result:

- checkpoint before load: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- checkpoint after load: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- manifest before load: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`
- manifest after load: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`
- hashStableAcrossLoad: `true`

## Fresh-load probe

Evidence:

- `fresh-load-probe\fresh-load-probe-result.json`
- `fresh-load-probe\fresh-load-probe-report-ru.md`

Result:

- `manifestLoaded=true`
- `checkpointLoaded=true`
- `importAsyncSucceeded=true`
- `loadCheckpointAsyncSucceeded=true`
- `machineId=skill-selection-machine`
- `domainTag=SmartSkillsDomain.SkillSelection`
- `version=1.0.0`
- `trainedPairCount=10`
- expected selected skill: `client-adaptive-learning-loop`
- selected skill: `client-adaptive-learning-loop`
- `pass=true`

## Files changed or created by this fix

- created: `row16-harness-review-approval-20260626T121838Z.md`
- created: `row16-harness-review-approval-20260626T121838Z.json`
- created: `logs\dotnet-run-fresh-load-probe-rerun-20260626T121838Z.log`
- updated by harness rerun: `fresh-load-probe\fresh-load-probe-result.json`
- updated by harness rerun: `fresh-load-probe\fresh-load-probe-report-ru.md`
- created: `row16-final-execution-packet-20260626T122025Z.md`
- created: `row16-final-execution-packet-20260626T122025Z.json`

## Non-claims

Не выполнялось и не заявляется:

- host UI actions/readiness;
- training;
- eval;
- checkpoint mutation;
- checkpoint promotion;
- locator update;
- production code edit;
- git action;
- token savings;
- semantic quality;
- product readiness;
- full joined pass;
- full extension readiness;
- Row16 board acceptance.

## Узкое человекочитаемое утверждение

Внутри текущего Row16 run folder есть scoped core lifecycle evidence: backup/export, share/package, share/readback и fresh import hashes совпадают с ожидаемыми Row14 staged asset hashes; fresh-load harness после явного review approval один раз запущен с `--asset-root` на `fresh-user-asset-root`, загрузил imported manifest/checkpoint через core API и выбрал ожидаемый skill `client-adaptive-learning-loop`. Это требует review и не является Row16 board acceptance.
