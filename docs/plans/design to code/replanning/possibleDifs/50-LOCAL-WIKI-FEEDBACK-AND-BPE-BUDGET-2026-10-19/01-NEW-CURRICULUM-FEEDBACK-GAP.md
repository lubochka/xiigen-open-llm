# Восстановление `local_wiki_planning`: следующий урок снова учитывает уже принятое обучение

## Результат свежей проверки

Найдено конкретное отличие от пользовательской рабочей ветки: `WikiTraceAggregatorUnit` утратил учёт принятых checkpoint и ограниченного окна попыток при рекомендации следующего обучаемого Unit. Нынешний вариант ранжирует Units по сумме **всех исторических** записей `rejected`, `blocked`, `experimental_no_authority`. Новая запись `accepted` не уменьшает эту сумму. Это подтверждённый кодовый механизм на пути планирования следующего урока; его влияние на предметное качество обучения остаётся проверяемой гипотезой. [D1][C1]

На одном и том же нынешнем JSON-реестре из 421 записи исполненный Python-перенос правил дал разные рекомендации: donor — `DocumentationGeneratorUnit`, current — `ProjectWikiRequestRouterUnit`. У нынешнего выбранного Router зарегистрированы два accepted checkpoint с accuracy `1`; 51 историческая rejected-запись всё равно даёт приоритет `153`. Добавление одной или восьми новых accepted-записей с accuracy `1` оставляет `153`. Добавление семи rejected и одной accepted даёт `174`. Это результат **source-equivalent Python**, не native .NET и не новое обучение. [E1][C3]

**Классификация:** `SOURCE_CONFIRMED_DONOR_DELTA`; `EXECUTED_SOURCE_EQUIVALENT_COUNTERFACTUAL`; `NATIVE_NOT_RUN`; `LEARNING_QUALITY_GAIN_NOT_MEASURED`. Не доказаны ни бесконечный цикл, ни причина опубликованных RP01 `0/3` / `0/4`.

## 1. Точные границы источников

| Назначение | Git SHA | Файл / диапазон |
|---|---|---|
| Пользовательский donor | `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` | `42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs`, 60–170 [D1] |
| Нынешняя реализация | `4945b8e85c17b5f150eb5b840d16bbdc4b506242` | тот же файл, 63–115 [C1] |
| Достижимый caller | тот же current SHA | `ProjectWikiTrainingFlowAdapter.cs`, 40–42, 135–179, 395–398 [C2] |
| Создание контекста без свежих traces | тот же current SHA | `ProjectWikiTrainingOrchestrator.cs`, 62–104 [C4] |
| Нынешний реестр | тот же current SHA; blob `f4a2a6b4f9e164a3e91fa64671e4efcb59c3c077` | `43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json` [C3] |
| Проверка допустимости реестра | current SHA / donor SHA | `ProjectWikiTrainingAssetRegistry.cs`, current 48–98 / donor 43–79 [C5][D2] |

Регистры не редактировались в репозитории. Для переносимого повторения рядом со скриптом приложены [current-registry-input.json](evidence/current-registry-input.json) и [donor-registry-input.json](evidence/donor-registry-input.json): все исходные entries в исходном порядке, только точно скопированные поля, которые читают формулы и validators. `_evidence` содержит SHA исходного blob и SHA256 скачанного текста. У donor 426 entries. Сравнение двух селекторов для основного результата выполнено **на одном нынешнем корпусе**, а не на двух разных регистрах.

В текущем `status_counts.accepted` написано 19, тогда как пересчёт actual `entries` даёт 20 accepted / 182 rejected / 193 experimental / 26 blocked = 421. Селектор использует `Entries`; результат 153 получен по записям, не по этому summary. Эта побочная несогласованность не объявляется самостоятельной причиной качества. [C3][E1]

## 2. Что именно было и стало

### Donor: accepted checkpoint меняет оценку потребности в следующем уроке

`SelectRegistryWeakUnit` группирует записи по Unit. `ActionableWeakSignalScore` находит accepted-записи, разрешённые для continuation, и максимальное записанное accuracy. Затем берёт восемь записей с лексикографически наибольшими `BatchId`. Если accepted accuracy ≥ `0.85` и среди этих восьми ≥ 3 rejected, score этого Unit становится нулём. Иначе вычисляется `5 × recentRejected + 2 × allBlocked + min(allExperimental,10)`, с добавками 20 при отсутствии accepted, 15 при accepted accuracy < .75 либо 5 при accuracy < .90. [D1] (106–157)

Это конкретная прежняя эвристика распределения обучающего бюджета. Она не является KNN, LoRA, градиентом или обучаемой моделью. Числа `.85/.75/.90` и состояние `accepted` здесь означают записанные значения реестра; данная проверка не подтверждает качество соответствующих checkpoint.

### Current: положительное завершение обучения отсутствует в формуле

`SelectNextTrainingAction` сначала отбрасывает accepted-записи через `Where(IsWeakRegistrySignal)`. Затем `WeakSignalScore` складывает rejected × 3, blocked × 2 и experimental × 1 за всю историю. Нет ни ограничения числа записей, ни зависимости от checkpoint accuracy, ни вычитания закрытых ранее проблем. Технически high score означает **более высокий приоритет следующей рекомендации**, поскольку используется `OrderByDescending`. [C1] (81–115)

Для фиксированного Unit с историей `H`:

`score_current(H ∪ {accepted}) = score_current(H)`.

`score_current(H ∪ {rejected}) = score_current(H) + 3`.

Это непосредственно следует из фильтра и суммы; для текущего Router показано на реальном реестре в [E1]. Если внешний исполнитель после каждого шага вновь запрашивает этот plan и следует его первой рекомендации, ранее часто проверявшийся Unit может постоянно получать бюджет впереди остальных. Существование такого многократного исполнительного цикла и его фактическое влияние на качество здесь не установлены.

### Путь до обычного планировочного Flow

`training_mode = plan_next_training_action`, `registry_plan_only` или `next_training_action` направляет `ProjectWikiTrainingFlowAdapter.ExecuteAsync` в `ExecutePlanNextTrainingActionAsync`. Он вызывает `PlanNextTrainingAction(learningDataRoot)` без аргумента aggregation. Orchestrator создаёт `registry-plan-only` aggregation с пустым `FailedUnits` и вызывает селектор. Следовательно, fallback на статусы реестра достижим через штатный Flow. [C2][C4]

**Существенное ограничение:** этот Flow явно записывает `training_status=plan_only_no_batch_created`, нулевые training/heldout row counts и `training_started=false; new_batches_created=0`. Доказана рекомендация следующего Unit, а не автоматический запуск обучения. Пустые live traces в default caller существовали и у donor; новым отличием является формула приоритета. [C2] (151–179) [D3]

## 3. Исполненный matched-registry контрфактический контроль

Скрипт [selector_probe.py](evidence/selector_probe.py) переносит обе формулы, порядок `GroupBy` после исходного `Where`, стабильный tie-order нынешнего варианта и проверку допустимости реестра. Полный результат — [selector-receipt.json](evidence/selector-receipt.json). Запуск: `python selector_probe.py`; входы по умолчанию находятся рядом с файлом, доступны `--donor`, `--current`, `--output`. Это прозрачный короткий контроль исходной логики. `dotnet` не запускался; checkpoint не загружались в модель; ни одного optimizer update не было.

| Вход к обеим формулам | Donor: score Router | Current: score Router | Рекомендация donor | Рекомендация current |
|---|---:|---:|---|---|
| Один текущий реестр, 421 entries | 0 | 153 | DocumentationGeneratorUnit | ProjectWikiRequestRouterUnit |
| Тот же реестр + 1 новый accepted accuracy=1 | 0 | 153 | DocumentationGeneratorUnit | ProjectWikiRequestRouterUnit |
| Тот же реестр + 8 новых accepted accuracy=1 | 0 | 153 | DocumentationGeneratorUnit | ProjectWikiRequestRouterUnit |
| Тот же реестр + 7 rejected, затем 1 accepted accuracy=1 | 0 | 174 | DocumentationGeneratorUnit | ProjectWikiRequestRouterUnit |
| Тот же реестр + 1 rejected | 0 | 156 | DocumentationGeneratorUnit | ProjectWikiRequestRouterUnit |

Добавленные события имеют явную метку `SOURCE_PROXY_EVENT_ONLY_NOT_A_RUN` и существуют только в копиях в памяти. Их идентификаторы специально сортируются выше существующих, чтобы проверка затрагивала donor-окно. Таблица не утверждает, что добавление accepted переключило выбранный Unit: рекомендация в каждой колонке **сохранилась**. Различие имеется между двумя формулами уже на исходном matched corpus. [E1]

Контроли:

1. При свежем `FailedUnits=[DocumentationGeneratorUnit]` обе формулы выбирают этот Unit: старый механизм не перекрывает явно переданный failure. [C1] (81) [D1] (79) [E1]
2. При `TrainingContinuationBlockedUntilRegistryValidated=true` обе стороны прекращают выбор. Проверки допуска не отключались. [C5][D2][E1]
3. Полный исторический donor-реестр проходит собственный validator, но не нынешний: у старых accepted-записей нет новых locator key / remote unit полей. Это отражено как `ACCEPTED_LOCATOR_KEY_MISSING`; основной matched-counterfactual потому использует валидный текущий реестр. [E1]

## 4. Малый проверочный эксперимент для восстановления

**ExperimentId:** `LW-FB-01`. **Tier 1.**

### Этап A: native воспроизведение выбора, без обучения

Сделать изолированный harness над двумя селекторами с **одинаковым** `ProjectWikiTrainingAssetRegistry` текущего SHA. Повторить пять входов таблицы и два контроля через обычный `ProjectWikiTrainingFlowAdapter` plan-mode. В варианте B заменить только участок выбора при пустых `FailedUnits` на donor-формулу; сохранить нынешние registry validation и формат ответа. Записать target, score decomposition, восемь использованных `BatchId`, evidence ids и `training_started=false`.

Основная метрика этапа A: идентичность native выбранного target с source-receipt для каждого из пяти входов; результат — число совпадений из пяти отдельно для каждой формулы. Это проверяет механизм, ещё не качество обучения. Дополнительно проверить, что записанный accepted checkpoint действительно отражается в donor-оценке и не меняет current-оценку.

### Этап B: влияет ли рекомендация на полезное обучение

Применим к ProjectWiki: **маршрутизация запросов и документация C#/SQL** как две разные функции одного проекта. На исходной независимой проверке подтвердить по одному реальному дефициту каждой функции; заранее заморозить **12 новых вопросов**, шесть на routing и шесть на source-grounded документацию. Обычный ответ должен доказанно использовать тот checkpoint/Unit, который изменяет урок; если связь learner→ordinary consumer отсутствует, остановить именно quality-часть и отнести её к уже известному разрыву consumer, не засчитывать симуляционные PASS.

Зафиксировать одну исходную модель/память каждой функции, один валидный registry snapshot и **одинаковый бюджет шести уроков/шести optimizer updates на сторону**. Учебный пул для обеих функций подготовить заранее, одинаковый между A/B; конкретные checkpoint, DPO, tokenization, masks, seed, learning rate, reference и оценочную панель не менять. В A CODEX получает нынешнюю рекомендацию, в B — рекомендацию с учётом accepted history. Кандидатный набор одинаковый, ограничен двумя проверяемыми функциями. CODEX принимает выбор урока и фиксирует, следовал ли рекомендации; эвристика не приобретает человеческую роль.

Для каждого из шести слотов выбрать следующий **новый** допустимый урок выбранной функции; у пула должно быть достаточно разных задач, чтобы повтор одного и того же DPO не подменил эксперимент. Повторное применение уже принятого урока не разрешать. После слота обновить registry одинаковым правилом по измеренному результату на отдельной dev-панели; финальные 12 вопросов не использовать для выбора или acceptance промежуточного урока. В журнале фиксировать выбранный Unit, exact lesson id, parent/result checkpoint, рекомендацию, принятое решение CODEX и предметное основание acceptance. Счётчик retries сам по себе не означает потребность в новом уроке.

Production `ProjectWikiDpoMetricRunner` требует ровно50 train/50 heldout; **не изменять этот guard ради микроопыта**. Для шести updates нужен отдельный малый harness существующего DPO trainer API с тем же recipient и ordinary checkpoint consumer. Не выдавать запуск такого harness за полный production `RunOneBatchAsync`; это контролируемый эксперимент распределения равного бюджета. [D4] (66–70,143–186)

Основная метрика: изменение корректных обычных ответов из **12** после одинаковых шести updates, отдельно routing из6 и documentation из6. Дополнительно — доля учебных слотов каждой функции, сохранение изначально правильных ответов и совпадение ответов после cold restore. `accepted_count`, доля повторных попыток и DPO pair accuracy не заменяют предметную метрику.

**Фальсификаторы:** если CODEX с полным качественным контекстом выбирает одинаковый полезный следующий Unit независимо от этих подсказок, если реальный исполнитель не использует данный plan-mode, либо если одинаковые шесть updates дают одинаковое изменение12 независимых ответов — данное отличие не объясняет наблюдаемую деградацию в проверенном сценарии. Не переносить его автоматически на RP01 decoder, EN→RU KNN или q/v-LoRA, где этот caller не доказан.

### Что нельзя переносить из donor автоматически

Donor-окно — **восемь лексикографически наибольших `BatchId`**, а не надёжно последние по времени события. Например, буквенный вариант `z-cross-stack...022` может сортироваться выше batch с большим числовым суффиксом `...029`; это не устанавливает их реальную хронологию. Поэтому B сначала восстанавливает точный прежний механизм как контроль, а улучшение ordering по явному timestamp/sequence оформляется отдельным фактором. Веточный успех не доказывает оптимальность каждой старой эвристики. [D1] (138–142)

Дополнительный **предлагаемый, ещё не исполненный** control внутри `LW-FB-01`: в изолированной копии реестра сохранить все статусы, accuracy, continuation, timestamps/sequence (если присутствуют), checkpoint и artifact references; изменить только буквенную часть `BatchId`. Зафиксировать восемь попавших в окно идентификаторов, score и target до и после. У exact-donor контроль обнаружит зависимость от имени, если переименование меняет состав окна с различными статусами; у варианта с явным ordering одинаковые времена/sequence должны давать одинаковый выбор. Это проверка безопасного переноса старого механизма, не самостоятельная новая гипотеза и не выполненный эксперимент.

Также старое `bestAcceptedAccuracy≥.85` не доказывает актуальную предметную компетентность: checkpoint мог стать устаревшим, а его метрика могла измерять только предпочтение. Нужны актуальные blind domain results и сохранение приоритета свежего явного failure. Это препятствует ошибочному отбрасыванию реальной новой проблемы.

## 5. Первичные положительные результаты donor, пригодные для отдельной восстановительной панели

Свежим чтением runner-emitted JSON и всех 100 diagnostic rows на run подтверждены следующие **исторические reference-relative DPO результаты**. Они не являются свежими запусками и не равны свободным ответам пользователю. Сводка пересчёта с исходными путями и SHA256 локальных свидетельств — [donor-receipts.json](evidence/donor-receipts.json).

| Donor run | Train / heldout | Верные предпочтения heldout | Деталь, существенная для восстановления |
|---|---:|---:|---|
| CodeTraceBuilder batch019, 21.05 | 50 / 50 | **49/50** | 5 epochs; LR 5e-4; policy018 + прежний `reference_base`; invented_callee 9/10, прочие четыре типа по10/10 [R0][G0] |
| CodeTraceBuilder batch064, 24.05 | 50 / 50 | **46/50** | fresh policy/reference; missing_value_flow8/10, invented_callee8/10, остальные по10/10 [R1][G1] |
| QueryPlanning angular-dotnet134, 24.05 | 50 / 50 | **50/50** | continuation checkpoint065; единственный тип `client_server_trace_incomplete` [R2][G2] |
| ContextRouting075, 24.05 | 50 / 50 | **50/50** | fresh policy/reference; единственный тип `answer_instead_of_abstain` [R3][G3] |

Для восстановительной проверки пары CodeTrace019/064 информативнее по разнообразию исходных текстов: у каждой 100 разных prompts и 100 разных chosen, у 064 также 100 разных rejected. [P0][P1] В query134 все100 строк имеют **16 разных prompts, один chosen и один rejected**; в context075 — 10 prompts, два chosen и один rejected. [P2][P3] Эти точные количества получены пересчётом архивных `dpo-pairs.jsonl`. Поэтому 50/50 этих двух узких run нельзя называть доказательством общей cross-stack или abstention generalization.

Перенос исходных Prompt/Chosen/Rejected, reference continuation, head-versus-LoRA и связь scorer с ordinary consumer уже принадлежат прежним пакетам `49-LOCAL-WIKI-DONOR-DELTA`, `49-LOCAL-WIKI-DONOR-CAUSAL`, `49-LOCAL-WIKI-GOLDEN-RECOVERY` и core/25. Настоящая дополнительная карточка посвящена **формуле следующей рекомендации после accepted обучения**. Перечисленные исторические результаты — дополнительные первичные свидетельства и пригодная контрольная панель, не переименованные новые гипотезы.

## Роли исполнения и честность

CODEX выполняет человеческую роль выбора урока и оценку качества DPO и результата обучения; в обоих случаях спецификация пользователя задаёт **astra 6 ultracode**. Внешняя открытая китайская модель создаёт обе стороны DPO. Здесь эти модели не вызывались. Детерминированный селектор формирует проверяемую рекомендацию и не заменяет ни выбор CODEX, ни предметную оценку.

В рамках этой проверки продуктовый код и защищённый каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` не изменялись. Выполнены чтение GitHub, сравнение исходников, подсчёт архивных rows и Python-перенос формул. Нативное обучение, Windows CI, blind CODEX assessment и качественный A/B не выполнялись.

## Ссылки на первичные источники

[D1]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs#L60-L170
[D2]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiTrainingAssetRegistry.cs#L43-L79
[D3]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L53-L95
[D4]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L48-L193
[C1]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs#L63-L115
[C2]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiTrainingFlowAdapter.cs#L135-L199
[C3]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json#L96-L138
[C4]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs#L62-L104
[C5]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/ProjectWikiTrainingAssetRegistry.cs#L48-L98
[E1]: evidence/selector-receipt.json
[R0]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/project-wiki-dpo-metric-summary.json#L1-L30
[R1]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/project-wiki-dpo-metric-summary.json#L1-L30
[R2]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/project-wiki-dpo-metric-summary.json#L1-L30
[R3]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-wiki-context-routing-compact-abstain-strong-confirmation-075/dpo-runs/wiki-context-routing-unit/wiki-context-routing-compact-abstain-strong-answer-abstain-remediation-075-wiki-context-routing-unit-20260524125418/project-wiki-dpo-metric-summary.json#L1-L30
[G0]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/dpo-pair-diagnostics.jsonl#L1-L100
[P0]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-21-code-trace-builder-promotion-confirmation-batch-019/dpo-runs/code-trace-builder-unit/code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019-code-trace-builder-unit-20260521190138/dpo-pairs.jsonl#L1-L100
[G1]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/dpo-pair-diagnostics.jsonl#L1-L100
[P1]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/dpo-pairs.jsonl#L1-L100
[G2]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pair-diagnostics.jsonl#L1-L100
[P2]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pairs.jsonl#L1-L100
[G3]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-wiki-context-routing-compact-abstain-strong-confirmation-075/dpo-runs/wiki-context-routing-unit/wiki-context-routing-compact-abstain-strong-answer-abstain-remediation-075-wiki-context-routing-unit-20260524125418/dpo-pair-diagnostics.jsonl#L1-L100
[P3]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-wiki-context-routing-compact-abstain-strong-confirmation-075/dpo-runs/wiki-context-routing-unit/wiki-context-routing-compact-abstain-strong-answer-abstain-remediation-075-wiki-context-routing-unit-20260524125418/dpo-pairs.jsonl#L1-L100
