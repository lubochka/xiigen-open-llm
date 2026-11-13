# Поздний срез core827: шестой completed, новые пути и остаточные конфликты

> **Исторический промежуточный срез827.** Его NEXT LF-03 и counts38/6/32 больше не действуют. Последняя сверка6e/eaed — [10](10-final-reconciliation.md); действующая очередь — [04](04-execution-registry.md). EOPT erratum на6e уже признаёт описанные здесь ошибки. Численные первичные наблюдения827 сохранены.

20 октября 2026. Базовый аудит выполнен на `66947785d1318f7cbbc88118f3156cdc75001e16`; пока документы собирались, появилась следующая версия `827a1acdbef95d220886e54f9d36bce0b405c8cd`. Этот раздел дополняет, а не переписывает свидетельства первого среза. Здесь только чтение Git-файлов, сравнение текста и арифметическое чтение существующих raw; ни сборки, ни обучения не выполнялись.

## 1. Очередь на промежуточном срезе827 — 38 ID, шесть completed, следующий LF-03

В [current index:5](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5) следующий — **№07 LF-03**; №01–06 уже завершены. Позиция 06 E-OPT-STEP подтверждена не только текстом индекса: два первичных arm-result имеют COMPLETED и issued2/completed2. Старые E-OPT-STEP wrapper, future output-root и новая TRAIN-панель для него не назначаются.

Current core теперь включает все 36 прежних ID planning53cb, в том числе ROPE-TRAIN-TABLE-01, AST-SIB-CACHE-01 и BPE-REALIZED-MERGE-COUNT-01. Они больше не planning-only. NG01 остаётся одним ID; SKNN остаётся reference alias AST. Вместе с двумя отдельными новыми VT/RMS получаются **38 уникальных ID:6 completed,32 future**. Новые прямые учебные факторы помещаются до reader-only IDENTITY/KN; исходные номера core и номера объединённой очереди надо различать.

## 2. E-OPT-STEP: primary измерения и граница достоверности

Первичные [A](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/A/arm-result.json), [B](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/B/arm-result.json), [study](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/study.json) и [report](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md) перечитаны; их Git blob hashes проверены. A закончено 17:13:15.9841655 UTC, B17:14:15.671675 UTC. Каждый arm реально выполнил два native DPO StepVector, изменил 16 LoRA arrays, сохранил 39 base arrays и переоткрыл optimizer/RNG/model state. Policy hashes и записанные readback совпали между плечами. Raw source pin остаётся 62 bcc8f;827 — место текущего хранения, не исходный training source pin.

| Показатель | A | B | B−A / смысл |
|---|---:|---:|---|
| Allocated bytes учебной фазы | 3 764 994 488 | 3 764 526 544 | **−467 944;−0,01242881%** |
| Elapsed учебной фазы, мс | 50 068,549 | 40 665,43 | Наблюдение fixed A→B, причинный speedup не установлен |
| CPU учебной фазы, мс | 37 687,5 | 35 828,125 | То же ограничение |
| Process peak working set, байт | 840 916 992 | 1 045 463 040 | **+204 546 048, ухудшение** |
| GC0/1/2 | 193/19/13 | 189/14/10 | −4/−5/−3, полезное наблюдение этой серии |

**Полезный allocation-результат сохранён как шестой кандидат.** Его не отменяет отсутствие роста accuracy. В то же время «не меняет обучение совсем» шире наблюдений данной пары; совпадение hashes и записанных state не проверяет все возможные входы, rollback и ошибки валидации.

### Фактическая измерительная граница

[Bridge:191–234](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L191-L234) снимает `GC.GetAllocatedBytesForCurrentThread` вокруг цикла двух шагов. В интервал также входят guard exports optimizer state и записи issued/completed JSON. Это matched полная фаза фактического опыта, **не чистый microtimer одного metadata getter и не process-wide allocations**. Final canonical readback расположен после timer. Ни learning_signal, ни новая A0/A1 TRAIN NLL/rank/exact panel в raw не присутствуют; задним числом их не добавлять.

### Два конкретных ограничения атрибуции

1. **Неверные подписи фактора в обоих raw.** `result.factor` утверждает старое R1 frozen-base comparison, `measured_path` указывает `DpoProjectLoraRecipient.RequireValid`. Источник ошибки найден непосредственно в [EOPT bridge:277–281](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/Common/ContinuousLearning/N62ExperimentBridge.cs#L277-L281): строки скопированы из R1. Это ошибка метаданных, не доказательство, что измерялся R1. Current B сохраняет прежнее LINQ сравнение frozen base и добавляет именно вызов metadata reader для RNG step.
2. **B provenance не замкнут.** В [source-manifest:829–845](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/source-manifest.json#L829-L845) все три `bSha256` равны `APPLIED_AFTER_FACTOR_EDIT`: DpoTrainer.cs, AdamW.cs, AdamWPortableState.cs. Study ссылается на hash этого manifest, а не на исправленный B identity. Чтение current B text и build script не восстанавливает автоматически точный historical binary binding. Полная атрибуция остаётся непроверенной; старые raw и manifest не переписываются.

Конкретная текущая B-цепочка: [DpoTrainer:546–555](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L546-L555) вызывает `ReadPortableMetadata`, затем `RequireAllowedMetadata`; [ExecuteWithPublication:1277](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs#L1277) использует этот getter только для первого RNG callback. Оригинальное место кандидата — `11.DPO/DpoTrainer.cs::ExecuteWithPublication` и metadata reader в `04.TrainingLoop/Optimizers/AdamW.cs`; source-copy экспериментальные типы не становятся product API без решения пользователя.

В [report:63–71](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/report.md#L63-L71) «детерминированно» и «меньшие выделения дали больший пик» — чрезмерные выводы. Теоретические 425 984 байта payload согласуются с механизмом, но не объясняют однозначно весь measured delta467 944: остаток 41 960 не разложен измерением. Один A→B не показывает причинную зависимость роста RSS от уменьшения allocations. Правильный additive итог сохраняет обе наблюдаемые оси и эту границу, без повторов.

## 3. Остаточный конфликт protocol:14 написанных строк,13 действующих future ID

`00-execution-policy-2026-10-20.md` и `24-neural-learning-signal-protocol.md` содержательно не изменились между 669 и 827. В [protocol:17–21](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md#L17-L21) всё ещё написаны NEXT E-OPT-STEP и его будущие 8 forwards. В current N62 card верхняя строка 6 закрывает completed01–06, но [строки 9–11](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L6-L11) и старый EOPT prompt ниже продолжают называть его future.

Действующее additive разрешение однозначно: EOPT завершён, его observer/wrapper/readout больше не future work. Для **13 оставшихся ID в 13 карточках** действует прежняя assigned TRAIN-панель: LF-03; N61-REF-CACHE (queue alias LC-REF-CACHE); GRADB-LOCALITY-01; DPO-TOKENIZER-SNAPSHOT-01; N-HEAD-01; E-DPO-VALID-LENGTH; E-SFT-RESPONSE-MASK; E-SFT-MOMENT-CONTINUITY; E-DPO-SFT-ORDER; N63-INGRESS-LENGTH; N63-CORRECTION-COLLISION; N63-AR-CAP; RMS-COORD-ALLOC-01. Самостоятельный ROPE имеет собственный declared TRAIN observer; он не превращается в четырнадцатую строку этого DPO/learner protocol и не получает дополнительный wrapper.

## 4. Полный current inventory и P-family delta

Полный Contents inventory current cards: **158 файлов =157 карточек +index**. Против 669 добавлены ровно ROPE/AST/BPE-REALIZED; удалений нет. Из 155 прежних файлов изменены 34: index,29 queued-card files и P52/P57/P106/P125;121 прежний файл имеет прежний Git blob SHA.

Для четырёх P-карточек diff содержит только вставки после заголовка и конечных пустых строк: все прежние 133 inline prompts и 470 unique subcases сохранены. Применены адресные уточнения:

| Карточка | Изменение 827 | Граница |
|---|---|---|
| P52, строки 3–5 | Structured-unit chosen JSON и neural corpus — разные учебные маршруты; rejected annotations могут быть законным supervision | Не объявлять blanket rejected stripping общей правкой; недостающий producer contract остаётся NEEDS_INPUT_FOR_RECIPE |
| P57, строки 3–5 | Старое accuracyBefore берётся из previous batch scalar и не образует paired panel | Нужна одна literal панель before/after, не новый Train ради формы отчёта |
| P106, строки 3–5 | Builder self-generated POS target отделён от способности Tokenizer Unit принять corrected chosen JSON | Не выводить «Unit не учится» из отсутствия исправленного target у конкретного producer |
| P125, строки 3–5 | Exact/phrase/word/kNN memory state и neural StepMany отделены | Назначить writer/state/consumer; поле, которое learner не читает, даёт NO_CONTRAST |

Наличие 157 файлов не означает 157 выполненных hosts или 470 полностью готовых рецептов. Базовый файл 154-role-map сохраняет pin669; текущие three new-card identities и изменение очереди описаны этой дельтой. P54 не изменён и его семь авторских NEEDS_INPUT_FOR_RECIPE остаются открытыми.

## 5. Выходные пути:30 current future назначений и три реальные смены

[source output map](evidence/core827-output-map.json) содержит 30 distinct future core IDs с exact root, report/candidate names, single outer entry либо одной последовательной canonical chain, current card URL и предыдущим planning назначением. Ни один current result-root не находится в protected docs. Из старой 32-row planning map отозваны только future R2/EOPT; raw обоих completed остаются на прежнем месте.

**Нельзя утверждать, что все оставшиеся пути идентичны planning53cb.** Current827 явно изменил три:

| ID | Старое future назначение | Действующее 827 |
|---|---|---|
| AST-SIB-CACHE-01 | Planning/possibleDifs/runs/...; WorkCore C:/x/ast01 | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AST01/results/attempt-0001` |
| NG01 | `C:/xg/NG01/r/attempt-0001` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01/r/attempt-0001` |
| BPE-REALIZED-MERGE-COUNT-01 | `C:/xg/br01/r/0001` | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/BR01/r/0001` |

В AST [окончательное уточнение:221](https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/AST-SIB-CACHE-01.md#L221) назначает Host=Core=AST01, Native=Host/Native/A.cs и B.cs, SourceCore read-only, Planning read-only. Старые paragraphs, где Run пишет Planning/runs или ищет sibling checkout, отозваны. Один Run-Assigned.ps1 владеет prepare/build/nA/nB/wB/wA/compare; prepare один раз резервирует Out. Смена адреса не разрешает обход уже начатой legacy попытки.

ROPE root прежний, но candidate name теперь `candidate.md`; NG01 также `candidate.md`. AST сохраняет `candidate-fix.md`; WIKI-FEATURE-KEYS/ONCE сохраняют `report.ru.md`. Остальные report/candidate — `report.md`/`candidate.md`. Все эти scripts остаются описаниями будущих hosts; их сборка и Windows path lengths этим аудитом не проверены. Не выводить runnable из факта literal пути.

Общий lifecycle сохраняет один execution owner. Host-owned claim/blueprint/source/dependency manifests не создают results заранее. После полного закрытия script/input и guard единственная назначенная цепочка владеет results; observer не вызывает второй prepare. Existing/partial/unknown не удалять, не переносить, не обходить другим attempt. В нынешнем scope protected required-changes не writable: кандидат остаётся в разрешённом result-root. Это относится к будущему исполнению, а не поручает запуск сейчас.

## 6. Машиночитаемые доказательства

[delta.json](evidence/core-827-delta.json) содержит arithmetic delta, statuses, timestamps, current queue36, census157 и additive-only P-diff. [source manifest](evidence/core-827-source-manifest.json) содержит 49 прочитанных current файлов с проверенными Git blob SHA и SHA256, включая все изменившиеся card files, три новые карточки и EOPT primary/code. [current inventory](evidence/core827-cards-inventory.json) — полный current inventory158. Исходные 16 primary files669 и 154-card audit остаются отдельными, не выдаются за новые 827 измерения.
