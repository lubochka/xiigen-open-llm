# Независимый review RMS-COORD-ALLOC-01

Дата: 2026-10-20. Проверены `agent_work/RMS-COORD-ALLOC-01-CARD.md` (142 строки) и `agent_work/RMS-COORD-ALLOC-01-EXECUTOR-PROMPT.md` (24 строки), сохранённые native исходники на core `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, а также реальные Program/Runner/run.py прежнего transpose-host из planning `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`.

**Финальный вердикт после текстовых исправлений: `PASS_DOCUMENTED_DESIGN_NOT_RUN`.** Learning design валиден, обязательный конфликт подготовки runner и остальные перечисленные замечания устранены. Готовый RMS host не материализован и не запускался — карточка честно сообщает это. Ни build, ни tests, ни training не выполнялись. Карточку и промпт исправил core-agent; независимый reviewer их только перечитал. Ниже сохранён исходный перечень замечаний как история review; итог закрытия находится в последнем разделе.

## R1 — обязательное уточнение lifecycle results и преобразования run.py

**Где:** карточка строки89–100, промпт шаги1–2/5; исходный `agent_work/rms_scaffold_reference/run.py:12–33`.

Шаг1 карточки требует заранее создать `results`. Шаг6 предлагает повторить прежний runner, сохранив запрет старого результата. Прежний runner явно отклоняет любой `result.exists()` до build. Следовательно, буквальное следование двум инструкциям способно остановить новый опыт до обучения, как уже случалось с BPE attempt. Кроме того, прежний runner требует `--results-root`, `experiment.json` и `source-copy-manifest.json`, а новый паспорт задаёт `python ./run.py` без аргументов и не определяет создание этих JSON. Это не утверждение, что новый runner уже сломан; это неоднозначность обязательного рецепта, которую нельзя перекладывать на будущую модель.

**Минимальное документарное исправление:**

1. При подготовке создать attempt, `src`, Program/Runner/run.py, но **не results/bin/obj**. Runner сам создаёт новый results с exclusive semantics. Существующий каталог не удаляется и не превращается в retry.
2. Прямо задать новый no-arg `run.py`: `root=Path(__file__).resolve().parent`, `host=root/'Runner.csproj'`, `artifact=root`, `result=root/'results'`, постоянный ID RMS-COORD-ALLOC-01, два jobs A/B code-sign. Удалить legacy argparse `--results-root`, чтение `experiment.json` и прежние длинные nested paths. Это изменение оболочки, не измеряемого learner.
3. Для старого `source-copy-manifest` выбрать и назвать один вариант. Предпочтительно при подготовке записать **новый** manifest RMS Program + фактических 36 src файлов после единственного экспериментального diff, а runner читает его по `root/'source-copy-manifest.json'`. Не использовать SHA изменённого Linear/Program от прежнего transpose-host. Для source provenance использовать заданный read-only pin и path-list; не посылать исполнителя искать альтернативные проекты.
4. В runner сохранить ровно один build, два native процесса, таймауты и FileMode.CreateNew. Подкаталоги `results/A` и `results/B` создать до запуска соответствующей DLL. Указанные в карточке команды DLL остаются прежними.

Это устраняется текстом рецепта; дополнительное обучение или общий test suite не требуются.

## R2 — ещё три коротких уточнения для завершённой карточки

| Приоритет | Наблюдение | Достаточное уточнение |
|---|---|---|
| P2, experiment exceptions | Отсутствие product tests и isolated copy уже разрешены, но exception rule выражено косвенно. Пользователь отдельно запретил перенос prod-ограничений на эксперименты. | Одной фразой разрешить experiment-only switch, literal tiny fixtures и standalone source host в этом attempt; не переносить исключения, обходы или diff в prod автоматически. Native Tensor/Trainer/LoRA не заменяются mock. |
| P2, failure counts | Сохранённый Program пишет JSON только после TRAIN и after Observe. На внешнем timeout actual count может быть неизвестен. Промпт требует actual counts без указания источника на таком failure. | Если процесса не хватило до receipt, `actual_optimizer_steps = null/не измерено`, а не0; deadline/exit/stage сохраняются. Не требовать ещё один запуск ради счётчика. На успешном receipt `optimizer.CaptureState().Step == 4` и actual_parameter_write проверяются по уже записанным полям. |
| P2, report paths | Отчёт расположен в `.../results/report.md`, но текст ссылается на `results/A/code-sign.json`, что как относительный Markdown target добавит лишнее `results/`. `one-factor.diff` назван, но место/создание не заданы. | Относительные links из report: `A/code-sign.json`, `B/code-sign.json`; candidate → `report.md`. При подготовке сохранить `one-factor.diff` в одном явно указанном месте, например `results/one-factor.diff` после exclusive создания results runner, до timed TRAIN. |

Допустимо явно назвать `training_losses` как **online pre-step losses**, чтобы будущий отчёт не назвал последний loss «после четвёртого update». Метрики after берутся из `Observe(model)` после цикла; это уже правильно предусмотрено карточкой.

## Проверенная учебная причинность и корректность измерителей

| Требование пользователя | Вердикт по прочитанному native коду/рецепту |
|---|---|
| Реальное обновление обучаемого состояния | PASS. `Trainer.TrainSupervisedNextTokenStep` строки102–145 создаёт shifted sequence, `model.Forward(training:true)`, backward, clipping и `optimizer.Step`. AdamW строки72–106 действительно меняет параметры и moments; Step counter сам по себе недостаточен, поэтому template дополнительно сохраняет actual_parameter_write и frozen_state_unchanged. |
| Проверяемый участок участвует в обучении | PASS. LlmModel/TransformerBlock вызывают RmsNorm; RmsNorm29–40 дважды вызывает MultiplyBroadcast. TensorOps168–184 делает `FlatToIndices` на элемент;1008–1017 выделяет int array. Это путь того же SFT forward перед тем же backward/optimizer. |
| Единственный фактор | PASS по математическому чтению. B вычисляет тот же bFlat через trailing coordinates; singleton axes по-прежнему0. `mappedBIndices`, float multiplication order и общий backward187–210 сохраняются. Branch выбирается вне element loop. Нового cache или изменения mask/objective нет. Runtime numeric equality пока не измерена. |
| A0/B0 идентичность | PASS как обязательство дизайна. Одинаковые base seed62001, LoRA seed62002 и dropout0, одинаковая последовательная конструкция, отдельные процессы, before обоих на A. Native Initializers.UseDeterministicSeed и LoraApplicator.ApplySeeded существуют. Будущий readout должен подтвердить равенство входов/config/начальных hashes/logits; равенство seeds не заменяет этот receipt. |
| A1/B1 действительно после update | PASS. В исходном Program после цикла4 steps вычисляется optimizer state, затем `after=Observe(model)`; Observe делает отдельный `model.Forward(training:false)` по prompt. Gold token используется только для NLL/exact; не выдаётся моделью вместо фактического argmax. |
| Точный бюджет | PASS. 4 TRAIN calls ×2 arms =8 updates. 2 EXAM prompts ×before/after ×2arms =8 EXAM forwards. Full-sequence SFT задан явно, DPOsteps0; дополнительных warmup/train не разрешено. Одна сборка timeout120s, native60s/arm и общий native deadline120s. |
| Стоимость обучения | PASS. Primary allocated bytes, Stopwatch, CPU и GC измерены вокруг четырёх native calls. Hash/export/Observe находятся снаружи. Process peak — за весь процесс до конца TRAIN и честно отделён от allocation/retained memory. Это сравнение двух цен обучения, а не выдуманная стоимость «до=0». |
| Успех без роста accuracy | PASS. Уменьшение allocations признаётся полезным успехом на заданном учебном workload; NLL/exact/CPU/wall/peak ухудшения сохраняются рядом. Узость двух EXAM cases описана. |
| Domain | PASS. Это маленький code-sign panel, не широкое доказательство code correctness или математики. Resource mechanism может быть шире домена, но карточка не объявляет неизмеренный DPO gain. |
| Default model | PASS. Исполнитель сохраняет текущую модель/режим сессии. Ученик — явно другой native C# model; внешний judge не требуется. |
| Короткий Windows path | PASS дизайна. `C:/xg/RMS01/aNNN` и плоский csproj, src tree только36 файлов, bin/obj локальны; прежняя длинная metadata структура исключена. Наличие конкретного SDK на будущей Windows машине не проверено — missing tool остаётся честным статусом. |
| Stop/next/no deletion | PASS. В обоих документах прямой запрет действий после stop и запрет продолжать прошлый опыт после next; no retry/cleanup/result overwrite. Старые эксперименты сохраняются. |
| Report and candidate | PASS с R2 paths. Русский, конкретный Markdown путь, ≤350слов+таблица, candidate≤120слов, навыки названы, после одного чтения завершить. Продвижение в production требует отдельного решения пользователя. |
| Known API vs proposed shell | PASS по статусу. Native namespaces `TensorCore.Core`, `LoRA`, `TrainingLoop`, `TransformerModel`/Config существуют; ApplySeeded и Trainer API совпадают с template. `ExperimentUseArithmeticBroadcastIndex` явно proposed в копии. Новые runner/files не объявлены существующими, но R1 нужно исправить, чтобы подготовка была однозначной. |

## Source provenance: расхождение commit labels не оказалось расхождением этих 36 файлов

Старый `source-manifest.json` на planning pin объявляет core `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`, новая карточка — `ab583032…`. Выполнена только проверка сохранённых файлов, без исполнения: **Git blob SHA совпали для всех36 path entries** текущего snapshot с этим старым manifest. Следовательно, ни один из этих source files не оказался неожиданно другой версией. Это отдельное read-only подтверждение snapshot compatibility; новая карточка всё равно должна использовать свой current pin и свой Program/diff, а не метаданные прежнего опыта.

Просмотренные материалы: `core_snapshot/00.TensorCore/Core/TensorOps.cs`, `03.TransformerModel/Layers/RmsNorm.cs`, `04.TrainingLoop/Trainer.cs`, `04.TrainingLoop/Optimizers/AdamW.cs`, `00.TensorCore/Init/Initializers.cs`, `10.LoRA/LoraApplicator.cs`; исходные `agent_work/rms_scaffold_reference/{Program.cs,Runner.csproj,run.py}`; полный36-file manifest. Имена методов/строки в карточке соответствуют исходникам. Этот review не является тестом сборки, численной эквивалентности или измеренным успехом RMS изменения.


## Финальное закрытие замечаний после корректировок

Повторно прочитаны исправленные CARD/PROMPT и новый `RMS-COORD-ALLOC-01-SOURCE-MANIFEST.json`; после последней адресной правки отдельно перечитаны только изменённые шаги6–9 и соответствующий промпт.

- **R1 закрыто:** при подготовке results/bin/obj не создаются; no-arg runner имеет буквальные root/host/result/jobs, не требует legacy args/meta/nested paths; results и arm directories создаются эксклюзивно самим runner. Исходный36-file manifest отделён от подготовленного copy manifest.
- **Порядок создания manifest закрыт:** шаг6 завершает новый run.py; шаг7 фиксирует checksum последним, явно запрещая изменение охваченных файлов после этого. Это устраняет обнаруженное на промежуточном reread устаревание checksum.
- **R2 закрыто:** разрешения experiment-only явно ограничены опытом; на внешнем timeout actual steps без receipt = null/не измерено; losses обозначены online pre-step; after NLL/logits остаются результатом отдельного Observe после updates.
- **Пути закрыты:** one-factor.diff готовится в корне attempt и копируется runner в results до TRAIN; ссылки из report используют A/code-sign.json и B/code-sign.json, candidate ссылается на report.md.
- **Учебный контракт сохранён:** один native mapping factor, одинаковый A0/B0,4 actual updates на плечо,8 EXAM forwards всего; default model, русский отчёт, правило успеха по любой полезной метрике с ухудшениями, no deletion/retry и немедленный stop/next присутствуют в обоих документах.

Оставшихся существенных документарных противоречий в проверенном контракте не найдено. Итог — подтверждение проекта эксперимента, а не готовности несуществующей сборки и не измеренного улучшения. Новые runtime проверки, model calls и product edits не выполнялись.
