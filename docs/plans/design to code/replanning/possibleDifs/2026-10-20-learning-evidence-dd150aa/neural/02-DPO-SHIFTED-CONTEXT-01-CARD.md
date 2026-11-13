# Карточка DPO-SHIFTED-CONTEXT-01

**Режим текущего задания: только документация. Статус: PREPARATION_REQUIRED; RUN_NOT_AUTHORIZED_BY_THIS_CARD.** Ни host, ни команды ниже в этом аудите не создавались и не запускались. Это новый кандидат эксперимента; production и `docs/plans/replanning-possible-diffs` не менять. Старые опыты и результаты сохранять.

## Паспорт и порядок

| Поле | Зафиксированное значение |
|---|---|
| ID | DPO-SHIFTED-CONTEXT-01; короткое имя SC01 |
| Цель | Дать реальный exact-DPO update уроку с carrier длины ContextLen+1, сохранив последний смысловой target |
| Приоритет | REGISTRY общего пакета, строка **A1**: после исходных №09–40, перед read-only №41–42; завершённый №08 не повторяется |
| Домен первого опыта | code-documentation / минимальный C# fixture |
| Продуктовый SHA | dd150aa5216e42b8a8bba14955337398ec5f8397 |
| Executor model | Та же модель, что используется по умолчанию в текущей сессии; не выбирать другую |
| Аналитик/оценщик DPO | CODEX Astra 6 ultracode: выбор и оценка качества; это не разрешение подменять executor |
| Язык отчёта | Русский; имена метрик/полей допустимы по-английски |
| Skills | generated-code-review-SKILL.md, training-data-gap-audit-SKILL.md, prompt-patch-authoring-SKILL.md, trainable-architecture-decision-SKILL.md, intent-to-plan-SKILL.md, plan-review-SKILL.md, human-readable-architecture-audit-SKILL.md; все в docs/skills |
| Host будущего опыта | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01 |
| Локальные исходники будущего опыта | C:/Rp01/SC01-src |
| Результат первой попытки | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001 |
| Входы | C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/fixture-boundary.jsonl и C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/fixture-control.jsonl; точные байты в [fixture](04-SC01-FIXTURE.md) |
| Разрешённая запись сейчас | Только новые документы possibleDifs; это не разрешение исполнителю менять код или запускать обучение |
| Следующее действие | Подготовить фиксированный host и явный readiness record отдельной назначенной задачей; затем вернуться в общий реестр |

**STOP/NEXT обязателен:** при «стоп» прекратить немедленно; при «следующий эксперимент» закрыть текущий коротким статусом и перейти только к назначенному ID. Нельзя продолжать прежний опыт ради тестов, отчёта, коммита, cleanup, дополнительного прогона или «завершения цепочки». Не удалять даже неудачную папку результата. Prepare создаёт attempt-0001 один раз; если root уже существует на Prepare — BLOCKED_OUTPUT_EXISTS. Calibrate/Compare используют тот же root только при точном совпадении неизменного manifest.json (ID, SHA, fixture/donor identities) и создают свои новые subdirs calibrate/compare. Если назначенный phase subdir уже существует — BLOCKED_PHASE_EXISTS. Старые phase outputs никогда не перезаписываются. При любой коллизии вернуться к реестру, не выбирать новый путь и не перезапускать самостоятельно.

## Гипотеза и исходный код

[Доказательства, точная call-chain и отличие от PAD/N63/HEAD](01-DPO-SHIFTED-CONTEXT-01.md). Измеряется реальный learner: `DpoTrainer.StepVector` → reference/policy forward → response-sum loss → LoRA backward → AdamW → явное повторное измерение на обновлённых весах.

A сохраняет текущую проверку полного carrier T≤64 и full-T forward. B оставляет те же полные IDs/masks, но использует input length T−1 и согласованные scorer/backward. Для T=65 A должен отказать **до update**, B может выполнить update. Эта пара не является equal-compute speed benchmark.

## Сохранённое обученное состояние: конкретный донор и его границы

Использовать только следующие файлы core на зафиксированном SHA:

- `experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-weights.bin`: LFS oid SHA256 **ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e**, размер **43500**.
- `.../trained-optimizer.bin`: LFS oid SHA256 **603cf13423f7c6ae5af073a4ea824f49009175b145ce551f6c1b5cf59b66842a**, размер **1112**.
- `.../restore-minimal-set.json` и `../Program.cs` — описание и сохранённые измерения.

Это snapshot после **1000 SFT updates**, а не после 1200: save находится до continuation в [Program.cs, 74–129](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L74-L129). В raw report уже есть проверка fresh-object restore, сохранение шага optimizer1000 и положительный token-level сигнал TRAIN: 3/40 response tokens, затем после 200 дополнительных SFT updates 4/40 и response NLL 4.766696420187372 → 4.711598364. Полные правильные ответы — 0; held-out token top-1 остаётся 0; контроль другой конструкции ухудшается. [Raw result до](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json#L68-L123), [после и контроль](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json#L270-L363).

**Это чужой сохранённый измеренный результат, прочитанный в аудите.** В нынешней сессии доступны LFS pointers; бинарные payload не загружались, native restore заново не выполнялся. Pointer на 130 байт не является весами. Если payload не разрешается в точные hash/size, статус PREPARATION_REQUIRED / DONOR_PAYLOAD_UNAVAILABLE; не создавать замену из случайной модели и не повторять скрыто 1000-step warm-up.

Профиль строго из донора: vocab256, context64, hidden16, 1 layer, 2 query heads / 1 KV head, FFN32, RoPE10000, RMS epsilon1e−5; LoRA q/v, rank2, alpha4, dropout0. [Program.cs, 36–49](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L36-L49).

### Оптимизатор не переносится молча между задачами

Донорский optimizer — память **SFT**, а новый урок — **DPO**. Текущий exact trainer требует соответствующий continuation proof для импорта состояния: [DpoTrainer.cs, 533–560](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L533-L560). Нельзя выдавать SFT snapshot за доказанную DPO continuation.

Оба плеча получают одинаковые warm **weights1000**, отдельный frozen reference с этими же весами и **явно новый DPO optimizer step0**. Это общий переход задачи, не фактор A/B. Донорский optimizer сохраняется и может использоваться только для проверки заявленного SFT restore; его не меняют и не «конвертируют» во время SC01. DPO LR=1e−5, weightDecay=.01, DPO beta=.1, AdamW beta1=.9, beta2=.95, epsilon1e−8 — текущие native defaults: [DpoTrainer.cs, 328–376](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/11.DPO/DpoTrainer.cs#L328-L376), [AdamW.cs, 38–39](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Optimizers/AdamW.cs#L38-L39). Все значения записать в readiness. Это не LR3e−4 донора.

## Данные и минимальный обучающий механизм

Fixture содержит две пары One/Two. Сначала идёт тот же prefix и 20-byte source, на которых получены ненулевые donor anchors. Затем комментарий о возвращаемом числе. Chosen и rejected имеют **одинаковые первые 64 токена** и отличаются только последним, 65-м:

- One: code возвращает 1, правильный финальный target `1`, отклоняемый `2`.
- Two: code возвращает 2, правильный финальный target `2`, отклоняемый `1`.

Поэтому потеря последнего target уничтожит **весь** preference contrast. Это смысловой тест последнего target, а не проверка дополнительного пробела. Это механический simulation fixture; здесь не объявляют внешнего teacher или authored production DPO. Если позже понадобится generated corpus, external open Chinese model может только генерировать; отбор/оценка остаются CODEX Astra 6 ultracode. Нельзя переименовать готовые строки в «модель сгенерировала».

Native BPE должен иметь ровно 256 byte IDs и ноль merges, как donor: [BpeTokenizer.cs, 42–81](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/01.Tokenizer/BpeTokenizer.cs#L42-L81). Перед сравнением выполнить Encode/Decode exact checks, зафиксировать tokenizer bytes/hash и фактические IDs. Число токенов не выводится только из числа символов для произвольного BPE.

Контроль T=64 использует те же две пары с на один байт более коротким комментарием. Контроль нужен только для проверки согласованного target mapping и того, что одинаковое обучение не сломалось внутри старой допустимой области; он не заменяет T=65 эксперимент. Входы и роли описаны в [04-SC01-FIXTURE.md](04-SC01-FIXTURE.md).

## Подготовка и критерии готовности

Назначенный preparer получает фиксированный donor host: `experiments-v2/RP01-MICRO/LF-03/` на том же SHA — только project/props/dependency-layout; алгоритм LF-03 и его Tensor patch не переносятся. Shared bridge donor: [N62ExperimentBridge.cs](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs#L40-L215).

Новая копия host располагается ровно по указанным выше коротким путям. Закрытая source-композиция использует native `00.TensorCore`, `04.TrainingLoop`, `11.DPO`, `14.ContinuousLearning` на dd150aa; остальные binary references берутся только из явного LF-03 dependency manifest. В обеих сборках TensorCore идентичен оригиналу dd150aa: не переносить copy-optimization другого опыта. Сохранить actual assembly identity и friend composition; не имитировать mutation capability. Полная solution/CI не запускается.

Shared bridge требует явной адаптации только в копии: вместо hard-coded необученного B0 — указанный tiny donor weights1000/config; никакого canonical-parent acceptance не обещается. Сохранить настоящий native materializer, recipient, admission и `CreateForProjectLora`. Existing B0 loader в [N62ExperimentBridge.cs, 346–380](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs#L346-L380) сам по себе с warm tiny donor несовместим. Не запускать его неизменённым и не объявлять это restore.

Prepare создаёт неизменные `results/attempt-0001/manifest.json` и `prepare/readiness.json` со статусом PREPARED_NOT_CALIBRATED. Calibrate добавляет собственный `calibrate/readiness.json`, ссылающийся на SHA preparation record; только он может выдать READY_COMPARE. Старые records не переписываются. До допуска Compare эти records совместно должны содержать:

1. SHA всех локальных source copies/assembly references, fixed host, длина самого длинного пути Windows; runtime net9.0; команды ниже существуют.
2. Payload weights проверен на 43500 байт и указанный oid; fresh policy/reference получены из отдельных объектов с attach q/v до LoadCheckpoint. Reference frozen; базовые веса frozen; trainable keys — только разрешённые LoRA A/B.
3. Native tokenizer identity, actual full lengths64/65, first differing target63/64, masks и отсутствие truncation.
4. Полностью измеренные **до обучения** anchors первых 20 response токенов One/Two: integer count **ровно 3/40** (tolerance к счётчику не применяется), raw per-token данные. StateHash по исходной donor-функции должен совпасть с `db44f1a437653299b345dc83c85523aad1ba1c55d3285865fe92501e5e78e386`. Anchor response NLL mean сверяется с `4.766696420187372` по заранее заданному `abs(actual-reference) <= 1e-5 + 1e-6*abs(reference)`. Если count/hash не совпал, совместимость донора не доказана; нельзя запустить random fallback или ослабить tolerance после результата.
5. Внутри старого лимита T=64 текущий и shifted пути включают последний target, дают согласованные likelihood и связанные LoRA gradients. Для каждого per-side logP применяется `abs(B-A) <= 1e-5 + 1e-6*abs(A)`; для каждой gradient компоненты `abs(B-A) <= 1e-6 + 1e-4*max(abs(A),abs(B))`. Обе стороны finite; последняя logit row имеет ненулевую норму gradient. Integer target counts/indices совпадают точно. При превышении tolerance статус PREPARATION_REQUIRED, без её расширения или оправдания нулевым full-answer accuracy.
6. Отдельный явный calibration record DPO на двух коротких T=64 парах: **variant A (original full-input execution), ровно 200 optimizer updates суммарно**, alternating One/Two, batch1: по 100 предъявлений каждой пары; variant B в этой calibration не обучается. Без скрытого retry/расширения. Его назначают в реестре самостоятельной подготовкой; это **обучение**, поэтому отчёт с before/after обязателен. SFT успех за 200 шагов не доказывает достаточность DPO бюджета200. Для calibration требуется measurable signal и положительный semantic terminal verdict после обучения (`twoCandidatePreferenceCorrect > 0 /2`, ties не победа); terminal 0→1 допускается. Positive anchor сам по себе не заменяет terminal результат. Если этот бюджет не даёт измеримого сигнала, terminal остаётся 0→0 либо уничтожены все anchors, статус остаётся PREPARATION_REQUIRED. Новые LR/budget — отдельное решение, не автоматический перебор.
7. Успех control64 калибрует бюджет для близкого same-domain урока, но не гарантирует улучшение boundary65 заранее. Calibration snapshot не заменяет warm donor основного опыта: A/B основного сравнения снова получают один и тот же weights1000. Calibration предназначен только для подтверждения пригодности бюджета и фикстуры.

Сейчас пункты native payload, prepared host, exact BPE/readout parity и DPO calibration в этой сессии **не выполнены**. Поэтому карточка честно не READY. Известный donor устраняет необходимость искать модель, но не выдаёт отсутствующий исполняемый host за готовый.

## Запуск после отдельного разрешённого materialization/readiness

Будущая единственная оболочка: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/Run-Sc01.ps1`. Контракт команд заранее фиксирован; эти команды не предлагаются как существующие сейчас:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/Run-Sc01.ps1 -Phase Prepare -Attempt attempt-0001
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/Run-Sc01.ps1 -Phase Calibrate -Attempt attempt-0001
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/Run-Sc01.ps1 -Phase Compare -Attempt attempt-0001
```

Один назначенный phase → один конечный результат → возврат к реестру. Prepare создаёт только root manifest и новую папку prepare, делает подготовку/restore/zero-update readout и gradient mapping check без optimizer step; Calibrate скрыто не начинается. Calibrate требует успешный prepare record, создаёт новую папку calibrate, выполняет ровно 200 updates variant A на control64, затем останавливается. Compare создаёт новую папку compare с A/B и разрешён только после READY_COMPARE; не выполнять все три команды автоматически.

В Compare:

- A и B в отдельных процессах, одинаковые weights1000, reference, tokenizer, seed, начальные optimizer step0.
- Before-readout одного формата вне таймера обучения. A использует общий диагностический shifted scorer только для оценки; этот scorer ничего не обучает и не подменяет A-learner.
- A передаёт boundary batch по исходному native preparation/trainer path. Дополнительный CaptureAndValidate в shared bridge может выдать capacity error ещё до StepVector: сохранить точную stop phase и error code, actualUpdates=0, веса не меняются. Не утверждать, что StepVector был вызван, если trace показывает более ранний отказ.
- B выполняет **ровно 200 DPO updates**, alternating One/Two, batch1, по тому же фиксированному списку пар, не добавляя новых строк. Это минимальный назначенный бюджет после успешной calibration, не обещание заранее заданного улучшения.
- After-readout пересчитывается по обновлённой policy B. Online `DpoMetrics` pre-update не принимается за after.
- Никакого исправления product/tests, merge, канонического checkpoint publication, автоповтора и следующей серии после результата.

## Показатели и решение

| Метрика | До/после и смысл |
|---|---|
| actualUpdates, rejectedPairs | Доказывают сам доступ урока к обучению; A0/B200 — unblock, не качество само по себе |
| lastTargetIndex, lastTargetMask, lastLogitGradientNorm | Должны подтвердить участие последнего смыслового токена64; missing target делает опыт невалидным |
| semanticTailGoldNll, goldDigitProbability, goldDigitRank | Общий shifted diagnostic scorer на финальной цифре; одинаковый алгоритм до/после и A/B |
| twoCandidatePreferenceAccuracy | Доля One/Two, где правильная цифра имеет строго больший logP, чем неправильная; ties отдельно |
| allVocabTailTop1 /2 | Правильная последняя цифра среди всего vocab, отдельно от двух вариантов |
| anchorResponseTop1 /40, anchorResponseNll | Первые исходные 20 токенов каждой response; сохраняет видимость реального донорского обучения |
| teacherForcedResponseExact /2 | Все response targets верны при gold prefix; ненулевые anchors это не заменяют. Free-generation exact=N/A: autoregressive decoding здесь не измеряется |
| heldout Three/Four и CONTROL Pos | Те же неизменные donor строки, отдельные NLL/top1/rank; не смешивать с TRAIN |
| время/CPU/allocations/GC | Весь update loop и на completed update; A0 знаменатель per-update=N/A |
| frozen base/reference hashes, LoRA delta, optimizer0→200 | Контроль реального получателя обновления и отсутствия подмены результата |

**Любое измеренное улучшение полезного показателя — успех по этому показателю и данному домену; роста accuracy не требуется.** Рядом обязательно показать ухудшения. Успех NLL не переименовывать в full-answer accuracy; teacher-forced exact не выдавать за свободную генерацию. Для смыслового SC01 подответа финальная цифра — отдельный verdict: terminal two-candidate correctness **0→0** не допускается как доказательство решённого учебного подответа, даже при сохранённых anchors3/40. Сохранить NLL/probability/resource improvements как успехи их собственных показателей, но добавить TERMINAL_0_0_NO_TASK_PROOF и NEEDS_CALIBRATION. Переход **0→1** валиден и приветствуется; positive terminal after вместе с положительным warm anchor до достаточно. Если terminal 1→0, показать регрессию и не заявлять сохранение навыка. Старые resource-success других опытов этим не отменяются.

Только A0/B200 без положительного измеримого учебного сдвига — SOURCE_CONFIRMED_UNBLOCK, а не «качество обучения улучшилось». Учебный сдвиг финальной preference/NLL — отдельный измеренный успех своей оси; полноценное доказательство смыслового SC01 подответа также требует terminal correctness после >0 и подтверждённый positive donor anchor до. Это не требует роста accuracy: 1→1 при улучшении NLL может быть успешным опытом своей оси. Не переносить вывод с этого byte-level C# fixture на EN→RU или production RP01.

## Обязательные выходы и кандидат в исправления

В собственной новой папке назначенной фазы (`prepare`, `calibrate` или `compare`; у compare также A/B): `status.json`, `source-manifest.json`, `fixture-manifest.json`, `before.json`, `after.json`, `per-token.jsonl`, `update-trace.jsonl`, `report.ru.md`, `candidate-fix.md`. Report содержит A/B до/после/дельты, знаменатели, cold-DPO transition, все регрессии, exact sources и limitations. Кандидат допускает только пути DpoExactPairMaterial/DpoTrainer/profile из [findings](01-DPO-SHIFTED-CONTEXT-01.md); описывает способ разделения input/targets, не меняя оригиналы.

После отчёта — короткий итог и следующий назначенный ID. В защищённую папку не писать. Решение о переносе в продукт остаётся пользователю.

