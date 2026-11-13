# N63C: поздняя delta-проверка core d962f958

Дата: 21.10.2026. Проверен только новый сохранённый запуск N63-CORRECTION-COLLISION на **d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19**. Старый аудит на fb9175cc описывает исторический рецепт4updates/36readouts, а не факт исполнения новой версии. Ни runtime, ни build, ни inference, ни правки protected/production здесь не выполнялись.

## 1. Точный новый статус

**A выполнил один DPO update, один SFT update и18 side-score calls. Затем A завершился FAILED_PRESERVE_EVIDENCE на сохранении RNG. B не исполнялся. Полного A/B результата нет.** Попытка уже issued; нулевые итоговые counters не разрешают повтор.

Прямые доказательства:

- [A/arm-result.json:5–10][raw-start]: FAILED_PRESERVE_EVIDENCE, error, sideScoreCalls=18 и sftLossBeforeItsUpdate=7.7777586.
- [raw:135–175][raw-dpo]: StepVector outcome=RETURNED;2 completed reference forwards,2 policy forwards,2 backward,1 applyOptimizer; optimizerStep=1, loss=0.75090945, objective=response-sum-softplus-signal-weighted-v3.
- [Bridge:325–413][updates]: S0 scoring → StepVector(T1) → S1 scoring → native TrainSupervisedNextTokenStep → S2 scoring; raw содержит все три состояния.
- [Bridge:101–104,511–512][dispatch]: B вызывается только после return A; A пишет свой raw и бросает исключение. [runner-failure.txt][failure] подтверждает этот стек; [Program:19–39][program] сохраняет исключение и возвращает exit2.

| Учёт | Фактически A | B | План всей пары |
|---|---:|---:|---:|
| DPO updates | 1 | 0, не начат | 2 |
| SFT updates | 1 | 0, не начат | 2 |
| Все optimizer updates | 2, два отдельных optimizer | 0 | 4 |
| Side-score calls | 18 | 0 | 36 |

Глобальный DPO step остаётся1; отдельный SFT AdamW выполняет свой первый шаг. Это не один global optimizer step2.18 side-score =3строки×2стороны×3состояния. Correct-token знаменатели ниже относятся к панели ответа, не к количеству training targets. SFT tokenIds.Count в raw не сохранён: не подставлять вместо него denominator23.

**Ошибка счётчика:** [Bridge:486–494][counter] записывает оба update counters как error==null?1:0. Поэтому raw dpoUpdates=0/sftUpdates=0 ошибочно описывает выполненную работу после позднего save failure. Оригинальный raw сохраняется неизменным; документная сверка должна отдельно указать observed/derived completed stages1+1. sftArtifacts=null и states=null означают, что соответствующие receipts не успели сформироваться, а не что SFT не выполнялся.

## 2. Что улучшилось и что ухудшилось

Панель — teacher-forced chosen mean NLL и meanLogP(chosen)−meanLogP(rejected), **не свободная генерация**. Этот margin не равен DPO rewardMargin: raw DPO metrics относятся к иной reference-relative objective. Числа непосредственно из [T1/Q1/Q2 raw:11–108][raw-panel].

| Строка, показатель | S0 | S1 после DPO | S2 после SFT |
|---|---:|---:|---:|
| T1 chosen NLL | 7.307878 | 7.305786 | 7.3035707 |
| T1 margin | 0.2279687 | 0.23142481 | 0.23094606 |
| T1 mean gold rank | 301.3478260869565 | 301.17391304347825 | 300.9130434782609 |
| Q1 chosen NLL | 7.193537 | 7.1934257 | 7.1934357 |
| Q1 margin | −0.07175541 | −0.07163048 | −0.07171726 |
| Q1 mean gold rank | 283.48275862068965 | 283.44827586206895 | 283.41379310344826 |
| Q2 chosen NLL | 8.067877 | 8.0679035 | 8.067668 |
| Q2 margin | −0.06385994 | −0.06395626 | −0.06389809 |
| Q2 mean gold rank | 362.3333333333333 | 362.3333333333333 | 362.3333333333333 |

Полезные и отрицательные дельты, вычисленные из сохранённых чисел:

- T1: DPO улучшил NLL на−0.002092, margin на+0.00345611, rank на−0.17391304. Последующий SFT ещё уменьшил chosen NLL на−0.0022153 и rank на−0.26086957, **но ухудшил margin на−0.00047875**. Итог S0→S2: NLL−0.0043073, margin+0.00297736, rank−0.43478261.
- Q1: DPO NLL−0.0001113/margin+0.00012493; SFT **NLL+0.0000100/margin−0.00008678**. Итог NLL−0.0001013, margin+0.00003815. Rank улучшается на−0.03448276 на каждом шаге.
- Q2: DPO **NLL+0.0000265/margin−0.00009632**; SFT NLL−0.0002355/margin+0.00005817. Итог NLL−0.000209, **margin−0.00003815**. Rank неизменен.

Число правильных chosen-response токенов во всех S0/S1/S2 остаётся **T1 0/23; Q1 0/29; Q2 0/21**; full teacher-forced exact=false. Q1/Q2 margin отрицателен во всех состояниях. Поэтому приобретение/стирание освоенного навыка не установлено; специальный learning verdict LEARNING_VERDICT_INVALID_ZERO_TO_ZERO сохраняется.

**Узкий успех по метрикам всё равно существует:** улучшения NLL/rank и DPO margin T1 — фактически измеренные полезные изменения обучения в домене C# code-documentation. Пользователь не требует accuracy gain для такого статуса. Фраза нового [report:73–78][report-verdict] «Полезного измеренного выигрыша нет» слишком широка и противоречит raw и правилу карточки. Корректно: «Есть небольшие улучшения отдельных показателей плеча A и ухудшения рядом; A/B-гипотеза замены stale SFT на corrected SFT не проверена, освоение ответа не доказано».

Это кандидат **документной корректировки отчёта и протокола N63C**, а не доказанный кандидат замены production SFT.Response. B отсутствует; сравнение корректного и устаревшего SFT текста невозможно. Само падение T1 margin S1→S2 наблюдалось, но после него margin всё ещё выше S0, а chosen NLL продолжил улучшаться.

## 3. Где именно упало и как описать будущую правку

[Свежая карточка:128][card-save] требует вызвать native RepositoryOwnedDpoRandomStateFile.WriteCreateNew с именем after-sft.rng.json. [Bridge:419–427][save] сначала сохраняет after-sft.bin и after-sft.optimizer.bin, затем выполняет именно этот вызов.

Native [BuildFileName:70–75][rng-name] выдаёт **dpo_random_state_step_000001.bin** для step1. [ValidateNewPath:417–429][rng-path] требует точного совпадения basename и иначе возвращает UNAVAILABLE. Несовместимое имя гарантированно нарушает контракт ещё до сериализации; [WriteCreateNewCore:122–124][rng-write] возвращает этот отказ. Raw error и failure stack это подтверждают. Это конфликт карточки/host с существующим native writer, а не доказательство неисправности RNG обучения или повод ослаблять production validation.

Точный document-only кандидат на последующее отдельное решение пользователя:

1. В N63C composition bridge сохранить RNG в **отдельном** каталоге armRoot/after-sft/, созданном штатным SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory; basename получить только RepositoryOwnedDpoRandomStateFile.BuildFileName(1). Итого A/after-sft/dpo_random_state_step_000001.bin. DPO RNG в A/checkpoints/ остаётся отдельным. Стадия receipt явно SFT; native DPO step1 не повышать и не выдавать этот SFT snapshot за новое DPO применение.
2. В Bridge после фактического return StepVector и return TrainSupervisedNextTokenStep фиксировать независимые stage-started/stage-completed flags и optimizer step receipts. Финальная ошибка Save не должна обнулять историю выполненных updates. Для начатого, но не вернувшегося вызова хранить UNKNOWN_PARTIAL, а не автоматически0.
3. Обновить **описание** текущего опыта: A_PARTIAL_AFTER_UPDATES,1DPO+1SFT,18readouts,B_NOT_STARTED,save_failed,no_rerun. Сохранённые raw/claim не переписывать. Это документация предлагаемых исправлений; ни bridge, ни writer, ни protected здесь не редактировались, новый запуск не назначен.

## 4. Цена и дополнительные ограничения recorder

[Raw costs:110–134][raw-cost]:

| Фаза A | Wall ms | CPU ms | Allocated bytes |
|---|---:|---:|---:|
| Загрузка | 6782.7431 | 3937.5 | 290812928 |
| DPO | 6672.6416 | 6609.375 | 937035776 |
| SFT | 744.4054 | 750 | 253031760 |
| scoringCost, как записан | 8097.4494 | 8156.25 | 3569316168 |
| Save | NOT_COMPLETED | NOT_COMPLETED | NOT_COMPLETED |

Нет B и сопоставимого ресурсного контроля. Разницу DPO/SFT time нельзя называть ускорением. saveCost записан нулями, потому что присваивание находится **после** отказавшего RNG write ([Bridge:428–432][save]); это не нулевая стоимость уже выполненных весовых/optimizer записей.

Дополнительный точный конфликт измерителя: score wall stopwatch приостановлен на DPO/SFT и промежуточной подготовке, но scoring CPU/alloc вычисляются как весь интервал от scoreCpu0/scoreAlloc0 минус только dpoCost и sftCost ([Bridge:325–409][updates]). В него также попадают snapshots/проверки/создание SFT trainer и encoding между scoring calls. Поэтому scoringCost CPU/alloc — остаточные расходы этого интервала, не чистая цена18forward. Будущая кандидатная правка измерителя — отдельно снимать и суммировать wall/CPU/alloc непосредственно вокруг трёх уже существующих ScoreAll, без дополнительных calls.

Финальные states, changed_adapter_arrays_by_sft и whole-path reference/base hash checks вычисляются **после** успешного RNG save ([Bridge:434–473][final-state]); до них execution не дошёл. Проверки base/reference после DPO действительно пройдены (351–366), но raw не доказывает полную финальную hash-equivalence после SFT. Отдельный post-SFT snapshot/optimizer receipt сейчас отсутствует. Указанные в report after-sft files согласуются с control flow, однако бинарные payload мной не скачивались и не проверялись.

## 5. Состояние карточки и очереди после delta

У свежей карточки остаются устаревшие фразы «host не материализован», «новых результатов нет» ([строки3,27,118][card-status]), хотя новые Program/Run-With-Learning-Signal и A raw уже существуют. Это документный конфликт текущего статуса, не разрешение исполнить карточку заново.

Header [карточки:8][card-route] теперь показывает маршрут №12 N63-CORRECTION-COLLISION → WIKI-TAXONOMY-01. Прежний NEXT10-N63C из pinned fb9175cc аудита нельзя выдавать за актуальный. Общая очередь синхронизируется родительским заданием; этот delta не назначает переход/запуск.

[Run-With-Learning-Signal:14–29,38–42][run] сохраняет early execution.claim, отказывает при существующем results и не делает retry после nonzero exit. Уже выполненные1+1updates расходуют попытку независимо от ошибочных нулевых counters. Не удалять partial, не создавать новый run root для обхода claim и не повторять A/B ради недостающей половины.

## 6. Provenance проверки

Считаны8 exact files по новому immutable SHA: report, runner-failure, A/arm-result, Program, Run-With-Learning-Signal, текущая карточка, N63CollisionBridge и native RNG writer. Копии сохранены под audit-cache/core-delta-d962/. Delta ограничена этим новым запуском; прежний широкий аудит не повторялся.

[raw-start]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L5-L10
[raw-dpo]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L135-L175
[raw-panel]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L11-L108
[raw-cost]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L110-L134
[failure]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/runner-failure.txt
[program]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/Program.cs#L19-L39
[dispatch]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L100-L110
[updates]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L325-L413
[counter]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L486-L513
[save]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L415-L432
[final-state]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionBridge.cs#L434-L473
[rng-name]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L70-L75
[rng-path]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L417-L429
[rng-write]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L112-L124
[report-verdict]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/report.md#L73-L78
[card-save]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L128
[card-status]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L3-L27
[card-route]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L8
[run]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/Run-With-Learning-Signal.ps1#L14-L42

