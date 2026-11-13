# Row16 harness review approval

Статус: `ROW16_HARNESS_READ_ONLY_DECIDE_PROBE_APPROVED_FOR_ROW16_EVIDENCE`

Дата UTC: `2026-06-26T12:18:38Z`

Сценарий: `frdp-p01-cursor-asset-lifecycle-roundtrip`

Проверенный harness:
`C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z\row16-fresh-load-probe\Program.cs`

## Противоречие

Prep JSON запрещает `DecideFromTrainedState` в harness, чтобы fresh-load доказательство не превратилось в произвольный unreviewed skill-selection путь, training/eval proof, checkpoint promotion или fake JSON-only proof.

Существующий Row16 harness вызывает `DecideFromTrainedState` после:

1. чтения imported manifest/checkpoint из `fresh-user-asset-root`;
2. построения `LearningMachineExport`;
3. `SkillSelectionMachine.ImportAsync`;
4. `SkillSelectionMachine.LoadCheckpointAsync`;
5. проверки expected SHA256 до и после load.

## Решение

Для этого run folder `DecideFromTrainedState` разрешен только как read-only финальный skill-selection probe после `ImportAsync` и `LoadCheckpointAsync`.

Разрешение узкое:

- разрешен только существующий harness path выше;
- разрешен только запуск с `--asset-root` равным `fresh-user-asset-root` внутри этого Row16 run folder;
- probe должен выбирать ожидаемый skill `client-adaptive-learning-loop`;
- probe не заменяет hash comparison, `ImportAsync` и `LoadCheckpointAsync`;
- evidence должен включать command log, JSON/MD result и hash comparison, а не только JSON summary;
- если harness будет изменен так, что `DecideFromTrainedState` вызывается до import/load, запускает training/eval, пишет checkpoint/manifest, обновляет locator или обращается к host UI, это approval недействительно.

## Почему это не training/promotion/locator update

По проверенному `Program.cs` harness не вызывает training API, eval API, checkpoint promotion API, locator update API и host UI действия. Он создает test-only request, вызывает selection decision на imported/loaded machine, записывает evidence files в `fresh-load-probe` и должен показать stable checkpoint/manifest hashes после load/probe.

Это не checkpoint mutation, потому что требуемое доказательство включает совпадение checkpoint/manifest SHA256 до и после load/probe:

- checkpoint: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`;
- manifest: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`.

Это не promotion и не locator update, потому что разрешенный output scope находится только внутри Row16 run folder, а harness не получает production/common/user locator root как аргумент.

Это не fake JSON-only proof, потому что приемлемое Row16 evidence должно ссылаться на:

- этот review/approval artifact;
- реальную rerun command log;
- `fresh-load-probe-result.json`;
- `fresh-load-probe-report-ru.md`;
- hash comparison source/backup/share/readback/fresh import.

## Non-claims

Этот approval не утверждает:

- Row16 board acceptance;
- host UI readiness;
- product readiness;
- token savings;
- semantic quality;
- training readiness;
- checkpoint promotion;
- locator update;
- full joined pass;
- full extension readiness.

Узкое утверждение: существующий Row16 harness можно один раз перезапустить как core evidence probe, если он использует `DecideFromTrainedState` только после import/load для проверки выбора `client-adaptive-learning-loop` и сохраняет все доказательства внутри текущего Row16 run folder.
