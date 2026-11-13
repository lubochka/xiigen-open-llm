# Два новых адресных пробела AnswerComposer: успешный SQL-донор и потеря evidence на входе policy

**19 октября 2026. READ_ONLY_RESEARCH.** Донор `local_wiki_planning` зафиксирован на `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`, current core — на `4945b8e85c17b5f150eb5b840d16bbdc4b506242`, каталог planning — на `4ca770a2c7354d7985f34c516f087e4b49cd01df`. При подготовке исследования native training, генерации модели, правки core и CI не выполнялись. AC-SQL01/AC-EV02 — адресные IDs этого пакета; совпадения с чужими U-N1/U-N2 исключены.

## Приоритет возврата к успешной ветке

| Приоритет | Кандидат | Что уже установлено | Что ещё нельзя утверждать |
|---|---|---|---|
| **P0 / AC-SQL01** | Вернуть и разделённо проверить доменное SQL-представление chosen и curriculum по конкретной ошибке | В финальном доноре есть условное различение читающих и изменяющих таблицы операций и выборка `table-effects-focus`. В current эти части удалены. В доноре сохранены поздние успешные DPO preference receipts 110/111/112 с 47/50, включая исправленные read-only сигналы 111/112. | Нельзя приписать 47/50 одному удалённому условию: состояния, распределение ошибок и время продолжения обучения различались. Нельзя выдавать это за качество свободного SQL-ответа. |
| P1 / AC-EV02 | Дать neural AnswerComposer те же допустимые evidence и documentation version, на которых структурный composer строит ответ | `ProjectWikiAnswerComposerDpoPair` содержит rich evidence; `ToDpoMetricPair` передаёт только вопрос и ответы. Реальная evidence доступна отдельному structural evaluator. Это конкретная новая граница потребителя. | Пробел унаследован от донора, поэтому сам по себе не доказывает регрессию RP01. Изменение neural prompt не меняет автоматически ordinary deterministic composer. |

Первым ставится AC-SQL01: он возвращает конкретные части известной успешной ветки. AC-EV02 — отдельная проверка полноты алгоритма, необходимая для обучения ответу, зависимому от предоставленного контекста. История успеха донора сохранена с её отрицательными результатами и границами. [Код SQL донора][C1], [current][C2], [разделение recipients][C3].

**Роли будущего опыта:** CODEX Astra6ultracode выбирает задачу и исходный материал; внешняя открытая китайская модель пишет обе финальные стороны chosen/rejected; отдельный CODEX judge принимает точные bytes пары, а затем отдельно оценивает student answer вслепую к плечу. Native-опыт здесь только спроектирован. Эта роль авторства не заменяется вручную написанным эталоном в нашем Python-proxy.

## AC-SQL01. При переносе исчезли условные SQL-признаки и отдельная выборка ошибок table effects

### Что говорит точный old/current diff

В `ProjectWikiAnswerComposerDpoGenerator` донор выбирает специальную выборку по `table-effects-focus` / `table_effects_focus` и ставит её перед остальными SQL-ветвями. В ней семь из десяти форм вопросов относятся к `missing_table_side_effects`; остальные — result shape, parameters и callers. Current больше не содержит этого selector и массива. Тот же batchId теперь попадает в обычную SQL-выборку, где table effects занимают две из десяти форм. При пяти evidence-записях и 50 строках это **35 против 10 table-effects строк** на split. Это source-derived изменение распределения, а не measured quality gain. [selector донора,16–36][C1-selector], [наборы вопросов,469–523][C1-shapes]; [current,21–38][C2].

Второе отличие находится в положительном учебном ответе **только при signal=true и questionType=stored_procedure_answer**. Оно не утверждается для обычного текстового и compact представлений; selector focused curriculum — самостоятельная ось. Донор передаёт в `BuildSignalChosen` тип отклонённого ответа и добавляет признаки только соответствующего аспекта. Для table effects он использует настоящие массивы evidence:

```csharp
var tableWriteTokens = evidence.SqlTablesWritten.Length > 0
    ? " SQL_TABLE_WRITES_PRESENT WRITE_TABLES_PRESENT"
    : " SQL_TABLE_WRITES_NONE_CONFIRMED READ_ONLY_PERSISTENCE_SCOPE_PRESENT";
```

Отдельно добавляются признаки полного result shape и недостаточности одного procedure name. Current удаляет аргумент `rejectedType` и весь этот условный блок. **Числовые признаки не исчезли:** current по-прежнему записывает `SQL_TABLE_WRITTEN_COUNT_0` или другое число, имя процедуры и размеры SQL-контрактов. Поэтому корректная гипотеза — потеря дополнительного доменного представления и focused negative curriculum, а не «вся SQL-информация стерта». [Донор,175–212][C1-chosen]; [current,177–199][C2-chosen].

Значение признака отсутствия записи особенно важно для двух разных SQL-ситуаций: read-only отчёт и транзакция, меняющая таблицу вместе с audit row. В доноре обе ситуации заданы явными fixtures; у `GetDailyOrderSummary` список записываемых таблиц пуст, у `MarkInvoicePaid` — `billing.Invoices` и `billing.InvoiceAudit`. В heldout присутствует `GetPendingNotifications`, который читает **и помечает** строки: имя `Get` не гарантирует отсутствие записи. [Fixtures,537–552][C1-fixtures].

### Сохранённая история успехов и неудач

Все следующие записи — **реальные сохранённые receipts в pinned donor**, а не повторный запуск. Все имеют 50 train и 50 heldout строк, пять эпох и learning rate около 0.0005. В колонке «время ID» показан timestamp из `runId` от 24 мая 2026; его timezone не установлен. Elapsed относится к записанному DPO runner, не ко всему пользовательскому flow. [Источники и SHA каждого файла — ledger](evidence/sql-source-ledger.md).

| Run | Время ID | Предыдущее состояние policy / reference | Heldout DPO | Table effects на heldout | Loop summary | Elapsed DPO |
|---|---|---|---:|---:|---|---:|
| 079 | 13:09:38 | fresh policy / copied fresh reference | 40/50 = 0.80 | Не разбирались raw diagnostics | passed | 178.91 s |
| 099 | 14:41:25 | fresh / copied fresh | 38/50 = 0.76 | Не разбирались raw diagnostics | passed | 163.59 s |
| **106** | 15:50:51 | policy105 / reference105 | **37/50 = 0.74** | **22/35** | **failed, publication skipped** | 192.55 s |
| **110** | 16:11:43 | policy109 / reference109 | **47/50 = 0.94** | **9/10** | passed | 163.85 s |
| **111** | 16:16:15 | policy110 / reference110 | **47/50 = 0.94** | **9/10** | passed | 143.82 s |
| **112** | 16:19:34 | policy111 / reference111 | **47/50 = 0.94** | **9/10** | passed | 142.24 s |
| **113** | 16:23:45 | policy112 / reference112 | **44/50 = 0.88** | **7/10** | passed | 150.22 s |

Записи инициализации указывают на файлы предыдущего run. Их содержимое и загрузка в настоящую модель здесь не проверялись; это lineage по receipt, не аттестация бинарников. Прямые summary: [079][H079S], [099][H099S], [106][H106S], [110][H110S], [111][H111S], [112][H112S], [113][H113S].

У106 `overallVerdict` равен `PROJECT_WIKI_ANSWER_COMPOSER_GATE_PASSED`, но `summary.passed=false` и warning прямо говорит `answer_composer_dpo_accuracy_below_0_75_publication_skipped`. Структурный verdict не заменяет полный итог. В110–113 `summary.passed=true`, warnings пусты. [106 loop,5–21/55–57][H106L]; [111 loop][H111L].

Raw пары позволяют уточнить историю без догадки по имени run:

- **106:** 42 из 100 chosen имеют одновременно `SQL_TABLE_WRITTEN_COUNT_0` и `SQL_TABLE_WRITES_PRESENT`. Это противоречивое представление при семантике конечного donor-кода. Половина корпуса — heldout; число42 не следует читать как42 training updates. [106 pairs,1–100][H106P].
- **110:** таких chosen уже12. **0.94 существует до следующей read-only correction.** Следовательно correction111 нельзя объявить причиной роста до0.94. [110 pairs][H110P], [110 summary][H110S].
- **110→111:** prompts и rejected неизменны; меняются20 chosen. Для12 read-only строк `WRITES_PRESENT` заменён на `WRITES_NONE_CONFIRMED` / `READ_ONLY_PERSISTENCE_SCOPE_PRESENT`; у остальных table-effects chosen меняется порядок групп признаков. Противоречий становится0, preference accuracy остаётся0.94. [111 pairs][H111P].
- **111→112:** все100 троек `(prompt, chosen, rejected)` дословно совпадают после исключения `id`. Это продолжение на том же корпусе, **не независимая реплика**. [111 pairs][H111P], [112 pairs][H112P].
- **112→113:** prompts/rejected неизменны; каждый из100 chosen получает дополнительные global persistence-mode признаки — read-only для60 строк или mutation для40. Accuracy снижается до0.88, table effects — с9/10 до7/10, result shape — с9/10 до8/10. Состояние policy при этом продолжает изменяться. Final donor source не содержит именно этих global добавлений113; по branch HEAD нельзя восстановить все промежуточные бинарники. [113 pairs/diagnostics][H113P], [113 diagnostics][H113D], [final source][C1].

Такая история поддерживает проверку **узко применяемого, семантически корректного** представления. Она не поддерживает правило «добавлять как можно больше положительных маркеров» и не даёт основания автоматически возвращать именно70% table-effects curriculum из неуспешного106.

### Где будет фактический получатель ремонта

Доменный генератор формирует `ProjectWikiAnswerComposerDpoPair` → pipeline сохраняет chosen/rejected в `ProjectWikiDpoMetricPair` → metric runner токенизирует их → `DpoTrainer` меняет policy. Для этого пути интервенция в chosen действительно может изменить учебные токены. В pipeline отдельно вызван structural `_runner`, а `ProjectLocalWikiModelUnit` строит ordinary answer из evidence полей и фиксированных секций. **Улучшение DPO preference ещё должно быть соединено с реальным neural consumer ответа; этот пробел не переименовывается в новое открытие AC-SQL01.** [Pipeline,51–61/106–111][C3]; [tokenization,1780–1820][C6]; [ordinary composer,40–81][C5].

Описанный current путь условен: default metric runner создаётся без `ServerOwnedLegalTrainingTeacherAdmissionAuthority`, а код отклоняет такой train на строках262–268. Для нового native-опыта нужен существующий корректно допущенный runner; изучение source и старых receipts не доказывает успешность сегодняшнего default-вызова. Это известная зависимость исполнения, обходов и запусков не было. [Конструктор/граница допуска][C9].

### Быстрый различающий эксперимент AC-SQL01 — один target, 2×2

**Цель:** отделить полезность representation от доли конкретных отрицательных примеров. Во всех плечах один `project-wiki-answer-composer-unit`, одинаковый разрешённый training route и фиксированный SQL corpus. Нельзя сравнивать110 checkpoint с106 checkpoint как чистое A/B.

| Плечо | Representation chosen | Curriculum 50 строк |
|---|---|---|
| A00 | Current: общие/count признаки | General:10 table-effects, по10 на остальные четыре типа |
| A10 | Возвращённые donor111/112-compatible условные признаки | То же general распределение |
| A01 | Current representation | Focus:35 table-effects, по5 result/parameters/callers |
| A11 | Возвращённые условные признаки | То же focus распределение |


**Материал и авторство.** Старые signal-пары допустимы как frozen архивные входы для реконструкции механизма; они не становятся новыми принятыми DPO от факта чтения. Для нового quality-пилота внешняя открытая китайская модель пишет обе финальные стороны, а для representation A/B — final chosenA/chosenB и общий rejected. Отдельный CODEX judge проверяет exact bytes, исходные SQL facts и единственность разрешённого различия представления. CODEX выбирает задачу и материал, но не переписывает ответы учителя. Неподходящие варианты возвращаются автору на новую генерацию до фиксации набора. Подробная постановка и договор ролей — [05](05-SMALL-DOMAIN-EXPERIMENTS.md#roles).

Один общий начальный policy checkpoint и **один и тот же reference checkpoint** копируются независимо в четыре плеча; optimizer state, trainable set, tokenization, epoch/update count, learning rate, order policy и seeds зафиксированы. Сравнения A00↔A10 и A01↔A11 меняют только representation; A00↔A01 и A10↔A11 — только распределение уроков. Для curriculum меняется выбор строк из одной заранее утверждённой pool, а не качество авторства. **Representation intervention применяется только к TRAIN. EXAM для всех плеч — один frozen файл с одинаковыми exact prompt/chosen/rejected, token IDs/masks и scorer, включая family-disjoint процедуры.** Равенства вопросов или rowId недостаточно: нынешний pipeline применяет generator также к heldout, поэтому простое глобальное переключение BuildSignalChosen изменило бы и экзамен. В опыте exam material фиксируется отдельно до изменения TRAIN и сверяется по hash. Восстановление continuation — отдельная уже известная ось, здесь оно одинаково во всех плечах.

Начальный прогон: **50 train /50 heldout /1 epoch**, то есть10 updates на плечо при mini-batch5. Сначала A00/A10. Максимум5 epochs/50 updates на плечо задаётся заранее; дополнительные A01/A11 запускаются только для отдельного curriculum-вопроса. Это согласуется с обязательными размерами actual Wiki runner; приёмка качества не зависит от заполнения100 строк. В каждом плече логировать фактическую модельно-видимую тройку, masks и active token counts: более длинный chosen может обрезать важный хвост, что должно попасть в результат. Вернуть только переменные представления/распределения; не менять LoRA, KNN или tokenizer одновременно.

**Предметная панель:** read-only отчёт; transactional update с audit; `GetPendingNotifications`, который меняет статус; одна процедура с изменённым result contract. В выбранном SQL-поддомене отдельно проверять: правильность reads/writes/none, точные таблицы, сохранение transaction semantics, result columns и C# потребителя. Domain transfer на translation или order-insensitive lookup из этого результата не следует. Для KNN на AST/code путь использования этих SQL-признаков требуется определить отдельно.

**Контроли:** одинаковые bytes chosen должны дать одинаковые model-input hashes; переименование batchId без смены selector semantics не должно тайно менять данные; `Get` с записью исключает shortcut по имени. Unknown `SqlTablesWritten=[]` нельзя без внешней полноты evidence трактовать как доказанное отсутствие записей: рассмотренное условие donor проверяет только длину массива, а не полноту извлечения. В эксперименте read-only gold удостоверяется исходным SQL, а unknown пример получает abstention. Это граница переноса старого признака, не утверждение, что все пустые массивы продукта ошибочны. [Условие175–188][C1].

**Результаты:** preference по каждому типу ошибки; отдельно actual grounded answer, если consumer загружает этот checkpoint; critical false read-only/write claims; regression на result shape/callers; время/update, параметры total/trainable, RAM/VRAM, размер checkpoint. Если обычный ответ не читает policy, его metric записывается `NOT_APPLICABLE_TO_THIS_RECIPIENT`, а не «DPO не помогает». Hypothesis опровергается как quality fix, если при matched state и сохранённом signal B не улучшает независимые SQL контрасты либо ухудшает критические ошибки. Source delta при этом остаётся фактом.

## AC-EV02. Богатое evidence есть у DPO-pair и structural evaluator, но его нет у neural policy

### Прямое доказательство границы

Модель `ProjectWikiAnswerComposerDpoPair` хранит отдельно `Request`, `Evidence`, `A`, chosen/rejected. Generator кладёт в Request `ProjectId` и `DocumentationVersion`, в `Evidence` — фактический record, а в `A` — только вопрос. [Record,261–271][C4]; [Generator,94–118][C1-rows].

Обе версии pipeline выполняют такое преобразование:

```csharp
new ProjectWikiDpoMetricPair(
    pair.PairId,
    "Task: compose a project-local wiki answer. ...\nQuestion: " + pair.A,
    pair.Chosen,
    pair.Rejected);
```

В prompt нет `pair.Evidence`, `pair.Request.ProjectId`, `pair.Request.DocumentationVersion`. Цитата сокращает лишь неизменную instruction, а полное точное тело доступно [current,106–111][C3] и [donor,101–106][C7]. При этом structural evaluator делает `BuildContext(row)` → `new ProjectWikiEvidenceHit(row.Evidence, 1.0, ...)` → `_composer.AnswerAsync(row.Request, context)`. То есть высокий structural score проверяет другой набор входных данных. [Runner,51–79][C8-eval], [BuildContext,169–179][C8-context].

Гипотеза не говорит, что исходный вопрос или оба ответа удалены: они сохранены. Новое отличие — **несогласованность контекста двух recipients** внутри именно AnswerComposer. Для обычного RAG-вопроса правильный ответ меняется вместе с разрешённым evidence. Если нейросеть его не видит, она может запомнить устойчивую связь symbol→answer, но не выбрать между двумя версиями того же symbol.

### Выполненный минимальный proxy

`units_probes.py` создаёт две синтетические rich rows с одинаковым вопросом `Which tables does InvoiceRepository.MarkInvoicePaid write?`, project/evidence/symbol IDs. В версииv1 accepted source — read-only preview, вv2 — транзакционная запись invoice+audit. Source-equivalent проекция даёт одинаковые prompt hashes и одинаковые default token IDs. Это следует из кода ещё до обучения. [Скрипт](evidence/units_probes.py), [receipt](evidence/units-probe-receipt.json).

Для одинакового контекста и двух фиксированных кандидатов одна preference-функция не может одновременно строго предпочитать read-only ответ в первой строке и обратный ответ во второй; на такой сбалансированной диагностической паре верхняя граница контекстно-независимой accuracy —1/2. **Это математический контрпример, не измеренная accuracy native-модели и не оценка всего SQL corpus.**

Proxy и независимая проверка уточняют важную границу. Runner сохраняет только первые **contextLen/4=48 prompt-токенов**. В этой конкретной fixture appended evidence **уже делает prefixes разными** при context192: различаются токены43 и48; JSON receipt честно содержит append_only_at_default_context_still_collides=false. Но полные имена записываемых таблиц и DocumentationVersion остаются дальше лимита. При diagnostic context1024 prefixes также различаются. Следовательно простая различимость входов не доказывает видимость всех нужных facts; требуется trace surviving tokens. Общий риск budget/truncation уже известен и не считается новой находкой AC-EV02. Независимый пересчёт — [SQL review](evidence/independent-sql-checks.json). [Runner defaults,122–133][C6-defaults], [token budget,1796–1817][C6-prefix].

### Быстрый эксперимент AC-EV02

**Изоляция:** сначала зафиксировать50 training rows и50 family-disjoint heldout, состоящие из same-question/different-evidence контрастов. Домены: SQL reads/writes; .NET return value и caller меняются между documentation versions; SQL result shape после миграции. Correct chosen/rejected строятся из разрешённого source evidence и независимо проверяются до train. В prompt запрещены `Expected`, правильный route/answer или `RejectedType` как подсказка. Авторы пар и examiner раздельны согласно исходной роли пользователя; здесь их модельных вызовов не было.

**A:** нынешняя проекция вопроса. **B:** тот же вопрос плюс сериализованные `ProjectId`, `DocumentationVersion` и минимально достаточные принятые `EvidenceId`, `SymbolId`, source location, facts/parameters/returns/callers, SQL tables/result/transaction fields для этой панели. Одинаковые policy/reference, tokenization, trainable parameters и budget. Для изолированного теста семантического evidence выбрать **общий** context во всех плечах так, чтобы реальный tokenizer сохранял выбранные evidence поля и оба candidate answers. Это изменение экспериментальной установки от default объявить явно; перенос обратно в ограниченный production budget — отдельная проверка layout.

**Дешёвый preflight:** до optimizer записать actual prefix IDs/masks; один flipped evidence-факт должен менять разрешённый model input, а permutation незначимых JSON-полей после canonical serialization — не менять. Нельзя читать сырой prompt как доказательство, что факт дошёл до policy.

**До обучения:** прогнать A и B на одном неизменном checkpoint, затем повторить после одинакового числа updates. Показать baseline(A), baseline(B), after(A), after(B) и разницу приростов. Улучшение B уже при нуле updates является эффектом доставки evidence, а не новым усвоенным уроком. Обычный composer и его входы сохраняются отдельным контролем.

**Главные показатели:** доля пар, где обе версии отвечены верно; unsupported/stale-evidence preference; grounded reads/writes/result assertions; correct abstention при missing evidence. Negative control — два разных rowId при одинаковых evidence не должны менять ответ. Edge — project/symbol одинаковы, версии разные; другой project с тем же symbol; отсутствующие или неполные evidence. Поведение ordinary composer записывается отдельно: на текущем concrete endpoint его deterministic output уже читает rich context и может быть верным без улучшения neural learner.

**Опровержение:** обнаруженный active путь уже сериализует те же evidence в policy input для этого recipient, либо matched evidence-aware train не выигрывает на заранее закрытых counterfactual facts. Если B полагается только на version ID, heldout с новыми version IDs и изменёнными facts это выявит. Если B ухудшает устойчивость к нерелевантному evidence или latency, tradeoff должен быть сохранён.

## Что не заявляется новым

AC-SQL01 не заменяет известные generic marker shortcut, continuation/reference restore, neural-vs-structural publication и LoRA/optimizer вопросы. AC-EV02 не повторяет P118: там KNN lesson Chosen не доходил до текста orchestration plan; здесь фактический evidence не доходит **до входа neural AnswerComposer**. Core76 исследовал другой `WikiTraceAggregatorTrainingPipeline`; N49-01/N49-03 — request router / code trace и их замену исходных prompt/answers. Наш inherited Composer seam уточняет их явный контрпример «у Composer chosen/rejected сохранены». Подробнее — [novelty ledger](evidence/sql-source-ledger.md).

Выполненная проверка hash'ей, старых receipts и source-equivalent projection подтверждает структуру входа и историю сохранённых preference метрик. Она не является новым успешным обучением. Оба предложения снабжены конкретным адресом потребителя, фиксированными сравнительными условиями и наблюдаемым условием опровержения.

<!-- IMMUTABLE REFERENCES -->
[C1]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs
[C2]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs
[C3]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingPipeline.cs
[C4]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDomainModels.cs
[C5]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectLocalWikiModelUnit.cs
[C6]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs
[C7]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingPipeline.cs
[C8]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingRunner.cs
[C9]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs
[H079S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-stored-procedure-promotion-confirmation-079/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-promotion-confirmation-batch-079-project-wiki-answer-composer-unit-20260524130938/project-wiki-dpo-metric-summary.json#L1-L27
[H079L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-stored-procedure-promotion-confirmation-079/project-wiki-training-loop-result.json#L1-L60
[H099S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-table-effects-promotion-confirmation-099/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-table-effects-batch-099-project-wiki-answer-composer-unit-20260524144125/project-wiki-dpo-metric-summary.json#L1-L27
[H099L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-table-effects-promotion-confirmation-099/project-wiki-training-loop-result.json#L1-L60
[H106S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-table-effects-focus-promotion-confirmation-106/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-table-effects-focus-batch-106-project-wiki-answer-composer-unit-20260524155051/project-wiki-dpo-metric-summary.json#L1-L27
[H106L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-table-effects-focus-promotion-confirmation-106/project-wiki-training-loop-result.json#L1-L60
[H110S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-final-scoped-positive-promotion-confirmation-110/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-final-scoped-positive-batch-110-project-wiki-answer-composer-unit-20260524161143/project-wiki-dpo-metric-summary.json#L1-L27
[H110L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-final-scoped-positive-promotion-confirmation-110/project-wiki-training-loop-result.json#L1-L60
[H111S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-111/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-readonly-write-aware-batch-111-project-wiki-answer-composer-unit-20260524161615/project-wiki-dpo-metric-summary.json#L1-L27
[H111L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-111/project-wiki-training-loop-result.json#L1-L60
[H112S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-112/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-readonly-write-aware-batch-112-project-wiki-answer-composer-unit-20260524161934/project-wiki-dpo-metric-summary.json#L1-L27
[H112L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-112/project-wiki-training-loop-result.json#L1-L60
[H113S]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-persistence-mode-promotion-confirmation-113/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-persistence-mode-batch-113-project-wiki-answer-composer-unit-20260524162345/project-wiki-dpo-metric-summary.json#L1-L27
[H113L]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-persistence-mode-promotion-confirmation-113/project-wiki-training-loop-result.json#L1-L60
[H106D]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-table-effects-focus-promotion-confirmation-106/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-table-effects-focus-batch-106-project-wiki-answer-composer-unit-20260524155051/dpo-pair-diagnostics.jsonl#L1-L100
[H106P]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-table-effects-focus-promotion-confirmation-106/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-table-effects-focus-batch-106-project-wiki-answer-composer-unit-20260524155051/dpo-pairs.jsonl#L1-L100
[H110D]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-final-scoped-positive-promotion-confirmation-110/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-final-scoped-positive-batch-110-project-wiki-answer-composer-unit-20260524161143/dpo-pair-diagnostics.jsonl#L1-L100
[H110P]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-final-scoped-positive-promotion-confirmation-110/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-final-scoped-positive-batch-110-project-wiki-answer-composer-unit-20260524161143/dpo-pairs.jsonl#L1-L100
[H111D]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-111/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-readonly-write-aware-batch-111-project-wiki-answer-composer-unit-20260524161615/dpo-pair-diagnostics.jsonl#L1-L100
[H111P]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-111/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-readonly-write-aware-batch-111-project-wiki-answer-composer-unit-20260524161615/dpo-pairs.jsonl#L1-L100
[H112D]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-112/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-readonly-write-aware-batch-112-project-wiki-answer-composer-unit-20260524161934/dpo-pair-diagnostics.jsonl#L1-L100
[H112P]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-readonly-write-aware-promotion-confirmation-112/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-readonly-write-aware-batch-112-project-wiki-answer-composer-unit-20260524161934/dpo-pairs.jsonl#L1-L100
[H113D]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-persistence-mode-promotion-confirmation-113/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-persistence-mode-batch-113-project-wiki-answer-composer-unit-20260524162345/dpo-pair-diagnostics.jsonl#L1-L100
[H113P]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-answer-composer-persistence-mode-promotion-confirmation-113/dpo-runs/project-wiki-answer-composer-unit/project-wiki-answer-composer-signal-token-realistic-stored-procedure-persistence-mode-batch-113-project-wiki-answer-composer-unit-20260524162345/dpo-pairs.jsonl#L1-L100

[C1-selector]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L16-L36
[C1-shapes]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L469-L523
[C1-chosen]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L175-L212
[C1-fixtures]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L537-L552
[C1-rows]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L94-L118
[C2-chosen]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerDpoGenerator.cs#L177-L199
[C8-eval]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingRunner.cs#L51-L79
[C8-context]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiAnswerComposerTrainingRunner.cs#L169-L179
[C6-defaults]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L122-L133
[C6-prefix]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L1796-L1817
