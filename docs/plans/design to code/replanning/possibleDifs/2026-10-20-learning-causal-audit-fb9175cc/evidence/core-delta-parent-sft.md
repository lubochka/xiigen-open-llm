# Ограниченная delta core: parent repair и SFT Exp reuse

21.10.2026. Проверенный новый snapshot: **`d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19`**; предыдущий source snapshot аудита — `fb9175cc752d41e1cb525e0609b7c96140940217`. Scope: три новых evidence Markdown, три source-contract JSON и canonical SFT-EXP-REUSE-01. Прочитаны только документы/metadata и уже доступные точные исходные методы для различения факторов. Runtime/build/Train/inference/checkpoint load —0; final пакет и protected corpus не изменялись. Снимки нового чтения сохранены отдельно в `audit-cache/core-delta-d962`, не под старым source pin.

## 1. Главный вывод

**В этом delta применён документный ремонт постановок; новых измеренных результатов SFT_EXP/parent/SC01/IDX не предъявлено.** У PA01, SC01 и SEXP documentary contracts прямо стоит `DESIGNED_NOT_MATERIALIZED_NOT_RUN`. Описанные 4/8/200/400 updates — будущие бюджеты, не выполненное обучение. Native SFT parent blocker получил конкретный назначенный writer→accepted selector→restore→continuation рецепт, но его runtime исправление ещё не выполнено. Нельзя написать «parent теперь работает» или «обучение разблокировано реализацией».

Источники статуса: [PA01 contract:1–10](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/reconciliation/pa01-source-contract.json#L1-L10), [SC01 contract:1–8](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/reconciliation/sc01-source-contract.json#L1-L8), [SEXP contract:1–11](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/reconciliation/sexp-source-contract.json#L1-L11). [Новый SEXP evidence:3–12](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-21-sft-exp-reuse-evidence.md#L3-L12) отдельно сообщает runtime0 и отсутствие физических PA01/SEXP01 host на его metadata check.

## 2. Какие прежние proposals теперь приняты

«Приняты» здесь означает включены в canonical документацию/очередь и получили авторский рецепт. Это не публикация обученной модели и не исполнение опыта.

| Прежняя proposal | Новое состояние | Что не произошло |
|---|---|---|
| **DPO-SHIFTED-CONTEXT-01 / SC01** | Canonical card/prompt/source contract; назначены full target carrier при inputT−1, Prepare gradient-probes, ранний bridge guard, fixed calibration/comparison. Contract содержит114native source entries,26dependency entries, плановые максимумы400updates/1672Forward/808Backward. | `runtime_executed:false`; нет нового quality/cost результата. Source pin SC01 — dd150aa…, current read — fb9175cc…, а не молчаливая замена всего материала на d962. |
| **IDENTITY-TRAIN-INDEX-01 / IDX01** | Canonical card/prompt с HashSet после настоящего TrainOnExample, восстановлением индекса при import и явной ценой restore+Train. План130Train,520row presentations при8distinct contents,96Flow. | Бюджет/source review не является measurement. Нет доказанного allocation/time gain или улучшения качества в этом delta. |
| **SFT-EXP-REUSE-01 / SEXP01** | Canonical card/prompt №45 после IDX, next IDENTITY-M1-01. Исходный H64/sign/непредъявленный S0 заменён автором на совместимую H16/PA01 цепь;87native source entries+28dependency metadata;4updates на arm,8всего,28Forward/8Backward. | Нет host/сборки/ABI/runtime/измеренного выигрыша. Аналитические2NV→NV Exp в BackwardLoss и+4096Bscratch за4updates не являются измеренными time/allocation deltas. |

Основание SC01/IDX adoption и runtime0: [parent repair summary:34–42](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-repair-and-corrected-08.md#L34-L42), [SC01 maxima/runtime flag:1192–1199](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/reconciliation/sc01-source-contract.json#L1192-L1199), [IDX bounded source/design review:49–53](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-adoption-repair-evidence.md#L49-L53). Основание SEXP: [canonical card:3–23](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L3-L23), [contract factor/budget/queue:991–1013](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/reconciliation/sexp-source-contract.json#L991-L1013).

## 3. Parent blocker: что стало конкретным и что осталось открытым

Причина локализована в прежнем N63I consumer: он загружал свежую базу, устанавливал новый adapter и создавал новый AdamW; accepted learned SFT parent не выбирался. Наличие native DPO Publish/Load не делало этот SFT путь исправным и не позволяло маркировать SFT snapshot фиктивной DPO lineage. Это по-прежнему **локальный разрыв caller integration**, не доказательство поломки всех loaders. [Evidence:13–18](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-adoption-repair-evidence.md#L13-L18).

Теперь назначены два последовательных будущих задания:

1. **PARENT-ADOPTION-01, №10:** создать truthful SFT accepted owner/schema/selector/session factory в PA01. A/B используют private repos и одинаковый reviewed H16 snapshot после1000steps. В A latest остаётся baseline ordinal000001/step0; в B accepted ordinal000002/step1000. `acceptedOrdinal=2` отделён от `optimizerStep=1000`; неизвестный historical training parent не выдумывается. План4новыхupdates/29Forward/4Backward. Before-handoff snapshot1000 остаётся immutable; B-after1002 не подменяет donor для следующего опыта.
2. **N63-INGRESS-LENGTH-LEARNED-01, №11:** corrected learning objective старого№08 с одним accepted000002/1000 для обеих arm. A чисто отклоняет неподходящий batch до update; B получает200admitted native SFT updates. План220Forward/200Backward; long examples deferred явно. Старый attempt не переисполняется.

[Назначенные работы и открытый статус:13–29](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-repair-and-corrected-08.md#L13-L29), [PA01 causal protocol/handoff:33–45](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-adoption-repair-evidence.md#L33-L45).

Зависимость закрывается только фактическим PA01 `technicalStatus=COMPLETED AND handoffReady=true` с source/parent receipts. Новый SEXP требует именно этот future module, byte-copy после его действительного выполнения; сейчас prerequisites не выполнены. Module/projects/Run/ABI/load/full m/v equality/publication/selector ещё не materialized/не проверены. У donor прочитаны текстовые LFS pointers и sizes, actual binary payload не хэшировался/не загружался в новом исследовании. [PA01 оставшиеся gaps:51–53](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-adoption-repair-evidence.md#L51-L53), [SEXP prerequisite:48–58](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L48-L58).

Старая№08 теперь **ATTEMPT_CONSUMED / LEARNING_OBJECTIVE_OPEN**, а не безусловно завершённая учебная задача. Это адресная поправка прежнему выводу: сохранить старый attempt и отдельно назначить недостающую постановку; не удалять raw/claim и не повторять старый entry.

## 4. Измерения: новых положительных runtime результатов нет

Новый evidence повторно читает уже известный RESTORE-MINIMAL-SET результат:

- После исторических1000steps: TRAIN3/40правильных response tokens, **0/2полных ответов**, NLL4.766696420187372, rank51.825.
- После исторических200continuation:4/40tokens, **0/2полных ответов**, NLL4.711598363502413; контроль Pos NLL ухудшился5.8079633717111525→5.857194149707198.
- Совпавший сохранённый model hash/load и import bool/step поддерживают прежний restore observation. Полное новое побитное сравнение всех m/v только назначено будущему PA01 и не выполнено.

Это полезные исторические partial-token/NLL наблюдения с соседней регрессией, **не новые PA01/SC01/IDX/SEXP успехи и не выполнение требования ненулевых целых ответов**. Ожидаемый3/40→4/40 в corrected08 нельзя предварительно занести как результат. Наличие метода/manifest/документного reviewed статуса не равно улучшению accuracy. [Raw readback boundary:20–31](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-adoption-repair-evidence.md#L20-L31), [SEXP learned-state limit:24–32](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-21-sft-exp-reuse-evidence.md#L24-L32).

В новом SEXP документе явно разрешено сохранить будущий useful cost/NLL/rank эффект при whole-answer0/2→0/2, оставляя whole-skill objective OPEN. Это не доказывает выполнение пользовательского пункта15 о ненулевых правильных ответах. Для нашего отчёта фиксировать эти статусы отдельно: ресурсное наблюдение может быть полезным по своей оси, но `learning objective complete` из такого числа не следует. [SEXP budget/ограничение:72–76](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L72-L76), [evidence58](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-21-sft-exp-reuse-evidence.md#L56-L62).

## 5. Fresh SFT_EXP не дублирует новые SiLU/HNSW

| Фактор | Где живёт reuse | Новая стоимость/граница |
|---|---|---|
| Canonical SFT-EXP-REUSE | `04.TrainingLoop/Trainer.BackwardLoss`: Exp(logit−max) повторяется в denominator-loop и gradient-loop одной позиции; один scratch float[V] на метод, перезаписываемый на каждой позиции. [Source389–426](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/04.TrainingLoop/Trainer.cs#L389-L426) | Убирает повторный Exp в loss backward; добавляет scratch array. Не меняет TensorOps.Silu и не сохраняет forward activation cache. |
| Наш SILU-SIGMOID-REUSE | `00.TensorCore/Core/TensorOps.Silu`: сохранить d=1+Exp(−x) из forward до backward того же узла. [Source1132–1158](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/00.TensorCore/Core/TensorOps.cs#L1132-L1158) | Дополнительный массив размера activation с временем жизни графа. Другой метод, другая длительность хранения, другой размер и tradeoff. Не объединять оба фактора в B. |
| Наш HNSW-QUERY-NORM-REUSE | Норма query в локальном GreedySearch/SearchLayer/SelectNeighbours, вызываемом настоящим Add. [Source164–230](https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/06.VectorSearch/HnswIndex.cs#L164-L230) | Non-neural KNN training cost, без Exp, autograd и PA01. Дубликата не появляется. |

Принятие SFT_EXP меняет статус **соседней известной гипотезы**, не отменяет bounded novelty двух наших кандидатов. H16/PA01 donor нового SEXP не подставляется автоматически в нашу SiLU H64/sign recipe; это изменило бы заранее выбранную постановку. Новая потребность SEXP в87source+28deps объясняется PA01 reader и не делает автоматически недостаточным наш36source standalone SiLU closure без этого reader. Оба наших опыта остаются NOT_RUN.

## 6. Что адресно синхронизировать в final пакете

1. Старый dispatch **NEXT10=N63-CORRECTION-COLLISION** устарел после нового canonical ремонта. Начало теперь **PARENT-ADOPTION-01 №10 → N63-INGRESS-LENGTH-LEARNED-01 №11 → N63C №12**. На20Oct summary было46ID; его собственный header указывает, что latest21Oct стало47 после добавления SFT_EXP. Считать эти исторические числа частью разных срезов, не конфликтом одного живого index. [Summary3,17–20,40–42](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/2026-10-20-parent-repair-and-corrected-08.md#L3-L42), [SEXP latest position45](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/reconciliation/sexp-source-contract.json#L1011-L1013).
2. Три прежние proposals пометить canonical-author-designed вместо «ещё нет в protected»; execution status оставить NOT_MATERIALIZED/NOT_RUN. Не превращать inclusion в measured success.
3. P02.PARENT больше не тупиковый `schema gap/entry=null`: есть concrete repair work order. Но `runtime unblocked` пока неверно. Цель№08 OPEN, старыйattemptconsumed.
4. Сохранить immutable source pin fb9175cc для прежнего source audit и наших рецептов; добавить этот documentary delta наd962. Не переатрибутировать старые цифры новому коммиту как новым запускам. Новое заявление чужого автора о праве менять protected относится к его поручению; наша read-only граница не меняется.

## Provenance прочитанных новых документов

| Файл внутри docs/plans/replanning-possible-diffs | Git blob наd962 |
|---|---|
| 2026-10-21-sft-exp-reuse-evidence.md | `27c8bc5bfbac02c0585baf39572231d7bac6b6f7` |
| 2026-10-20-parent-adoption-repair-evidence.md | `27f10bb0013f6e36b470a327d7ac3f061a0a009e` |
| 2026-10-20-parent-repair-and-corrected-08.md | `3a513a650e0eda6c4c1d22224a707265550770af` |
| reconciliation/sexp-source-contract.json | `0d61077271c3e0a1aa9644624865035af296c95a` |
| reconciliation/pa01-source-contract.json | `77a264cfd20ab07e47843b27bd8973d742e46fc1` |
| reconciliation/sc01-source-contract.json | `0b32879bfa8cf5c69bf56584738d0ece94d6a8e0` |
| experimets/cards/SFT-EXP-REUSE-01.md | `dc74bb2da1546172c2f85273fdf485a4b9dfb41d` |

Scope receipt: три JSON полностью разобраны; статусы/counts/owners/budgets/dependency fields просмотрены. Не заявляется независимая семантическая проверка87/81/114native исходников или binary identities. Fresh evidence содержит большие inventories; вывод этого delta ограничен назначенными изменениями и status/result distinction, не повторным одобрением всего каталога.
