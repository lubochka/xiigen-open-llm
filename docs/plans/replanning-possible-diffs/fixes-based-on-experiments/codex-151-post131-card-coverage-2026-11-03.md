# После P131: какие карточки усилены и какой повтор ошибки они предотвращают

Дата: 3 ноября2026. Основание — [аудит149](codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) и [адресный план150](codex-150-post131-addressed-repair-plan-2026-11-03.md). Приёмка исполнителя — [договор49](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md). Проверенная исходная ревизия core `b27783593745ffac8e74e81f917539717cd33615`, planning `9aef94e0aa02f462141ffb82331eccb96da282a5`.

**Объём дельты:**13 экспериментальных карточек, одна карточка переноса35, четыре существующих общих документа приёмки/доказательств/ремонта, четыре входа навигации core и два planning-документа. Новые149/150/151/49 объединяют выводы, план и договор. Это адресная профилактика по выявленным причинам, а не13 новых найденных дефектов или повторный аудит всех131 опытов.

## 1. Собственные карточки и передаваемые задания

Во всех13 карточках обновлён действующий блок и копируемый prompt либо добавлена явная копируемая дельта. Для Q130 изменён именно текущий неверный future-статус; исторический паспорт отмечен как история. В P129 снят уже устаревший NEXT Q130/Q131; в P131 выделен текущий RESULT_RECORDED/saved-only вход. Учебные источники, gold, бюджет, оплаченные попытки и сырые ответы сохранены.

| Карточка | Именованная проверка | Уточнение | Действующий prompt |
|---|---|---|---|
| [KNN-ORDER-PERMUTATION-01](../experimets/cards/KNN-ORDER-PERMUTATION-01.md) | `POST131-P127-ORDERED-CONTROL` | Подтверждённая слабость: сравнивались имена маршрутов; добавлено соответствие полным шагам и отдельно точным C-целям. | Дополнен существующий |
| [P16-semantic-order-roles](../experimets/cards/P16-semantic-order-roles.md) | `POST131-P16-ORDERED-PAYLOAD` | Полные упорядоченные роли связаны с фактическим ответом; краткой метки недостаточно. | Добавлена копируемая дельта |
| [P45-learned-reordering](../experimets/cards/P45-learned-reordering.md) | `POST131-P45-REORDER-BINDING` | Проверяется полный результат действительно достигнутой перестановки; ранний возврат не скрывается. | Добавлена копируемая дельта |
| [P97-learned-graph-order](../experimets/cards/P97-learned-graph-order.md) | `POST131-P97-TRACE-SINK-BINDING` | Обученный порядок связан с рёбрами, журналом исполнения и конечным результатом графа. | Добавлена копируемая дельта |
| [P105-frozen-reference](../experimets/cards/P105-frozen-reference.md) | `POST131-P105-STATE-AND-OBSERVATION` | Разделены фактическое состояние reference, право изменения recipient и источник наблюдаемого ответа. | Дополнен существующий |
| [SAVED-STATE-PANEL-0407-01](../experimets/cards/SAVED-STATE-PANEL-0407-01.md) | `POST131-P126-TF-SOURCE` | Teacher-forced token score сохраняет своё значение; генерация при generate_calls=0 не утверждается. | Дополнен существующий |
| [P25-cold-restore](../experimets/cards/P25-cold-restore.md) | `POST131-P25-NATIVE-READER-BINDING` | Два производных отчёта сопоставляются с имеющимися native ответами; равенство их длин не заменяет источник. | Дополнен существующий |
| [METHOD-KNN-TARGET-SCOPE-01](../experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md) | `POST131-METHOD-NATIVE-TARGET-BINDING` | Source/symbol/span, actual KindBag и существующий native Predict образуют проверяемую цепочку. | Добавлена копируемая дельта |
| [METHOD-KNN-SCOPED-EXPRESSION-TARGET-01](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md) | `POST131-Q130-RECORDED-DELTA` | Исправлен текущий статус уже выполненного ST01, сохранены расход и HOLD, паспорт помечен историческим. | Дополнен существующий |
| [WIKI-MARGIN-BALANCED-01](../experimets/cards/WIKI-MARGIN-BALANCED-01.md) | `POST131-WIKI-EXACT-ROUTE-CONTROL` | Точный назначенный класс и payload проверяются отдельно от любого изменения метки и числа отказов. | Добавлена копируемая дельта |
| [PHRASE-COVERAGE-CAP-01](../experimets/cards/PHRASE-COVERAGE-CAP-01.md) | `POST131-P128-COMPOSER-READER-SOURCE` | Сохранены отдельные источники copied composer, ordinary reader и live planner; их показатели не подменяют друг друга. | Дополнен существующий |
| [P131-negation-training-contrast](../experimets/cards/P131-negation-training-contrast.md) | `NATIVE_OUTPUT_BINDING` |128 исходных raw/native совпадений; обязательная связь полного actual и16-row mutant. Старый producer и новый verifier имеют разные identity/schema. H/X0/4 остаются научным результатом. | Новый current block и полный prompt |
| [HELDOUT-ADMISSION-ATOMICITY-01](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md) | `HELDOUT_SYNTAX_TYPED_FINITE` | Syntax не доказывает DTO/finite guard. Immediate failure state измеряется до restore; staging atomicity уже была, preflight экономит ненужную работу. Native binding здесь профилактика, не доказанный обход. | Заменены устаревший current block и prompt |

Четыре saved-state карточки — P25, P105, SAVED-STATE-PANEL, PHRASE-COVERAGE — дополнительно получили конкретное правило совместимости producer/verifier: новый checker не должен требовать, чтобы его DLL имела старый producer hash. Если provenance этого типа применяется, исторические bytes/schema проверяются отдельно; отсутствие originals не закрывается self-reseal. Это профилактическое распространение зависимости, найденной в P131; наличие аналогичного дефекта в этих четырёх hosts не заявляется.

В KNN-ORDER отдельно названы `label_sensitive_c_holds` и его косвенные control-зависимости `c_substituted_by_b_control_passed`/`corrupted_receipt_control_passed`: в новом finalizer научный успех не входит в procedural all-pass ни прямо, ни через helper. Integrity-controls принимают согласованный неверный actual и отвергают порчу полей; отдельные unit-controls scorer помечаются синтетическими. Exact assigned C-target — метрика, а непротиворечивость route/steps/существующего graph trace — обязательная целостность. Исторический status с девятью проверками сохраняется. Честный согласованный неверный ответ не превращается в INVALID.

## 2. Перенос и общие договоры

| Изменённый документ | Конкретное уточнение | Что предотвращает |
|---|---|---|
| [35-method-knn-target-scope](35-method-knn-target-scope.md) | Записанный ST01; source/symbol/span→target extraction→actual native pair→ordinary Predict; граница старых обученных targets | Повтор PRIMARY и выдачу experimental extractor за завершённую интеграцию |
| [03-test-and-execution-contract](03-test-and-execution-contract.md) | Required FQN/discovery/TRX, guard reachability, raw/native, producer/verifier versions | Неполный зелёный набор и «положительный» контроль, не достигший проверяемой ветви |
| [04-evidence-and-novelty](04-evidence-and-novelty.md) | Источник actual, полные IDs/payload, независимый gold, тип метрики, происхождение старого TRX | Подмену научного результата summary, уверенность вместо точности и смешение разных прогонов |
| [QA46](../experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md) | Broad OR+2classes, четыре разные причины падений, фактические case IDs и реальный outer exit | Замена expected под дефект продукта, пропущенные tests и fake native PASS |
| [План146](codex-146-post128-addressed-repair-plan-2026-11-03.md) | Исправлена сама действующая команда filter и copied prompt; дельта150/49 | Конкурирующий старый наряд, который снова не исполняет нужные три tests |
| [Новый executor49](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) | Шесть точных FQN, commands и outcome mapping; P127/P131 контроли; versioned reanalysis | Разные трактовки одного нового требования в соседних карточках |

Два неверных expected назначаются тестам по105; недостижимая fixture — setup-ремонту; rows[0] — product resolver-ремонту. Общей замены assertions нет. Обязательный набор шести FQN сохраняет всё семейство ExperimentFixes и не отменяет candidate/Q01 тесты.

## 3. Два репозитория и две разные очереди

В core обновлены [README переносов](README.md), [реестр](01-registry.md#executor-next), [README опытов](../experimets/00-README.md) и [индекс](../experimets/cards/00-execution-index.md#experiment-next). Основные Q-строки не меняются:131 уникальная,131 RESULT_RECORDED. У очереди опытов нет незаписанного NEXT. У очереди переносов остаётся Q01/135-R01-CONCURRENCY→Q02→Q03→Q06, Q04 приостановлен, Q05 зависит от него. Прямое назначение пользователя имеет приоритет.

В planning добавлены [новый POST131 обзор](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/codex-2026-11-03-POST131-TRANSFER-AND-EXPERIMENT-AUDIT.md) и верхняя ссылка в [README](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/README.md). Исторические POST128 и их source pins не перезаписаны. Planning не создаёт собственную очередь рядом с canonical core.

## 4. Проверка документной дельты

Список изменённых файлов и их identities находится в [evidence](../../../../experiments-v2/_audits/post131-2026-11-03/README.md). Проверяются: исходные Git blob hashes; сохранность Q-строк; существование новых link targets; отсутствие изменения product/test/experiment-source/raw/checkpoint; сохранение исторических паспортов; соответствие собственного current block и copied prompt. Новые вставки проходят parent review и адресную независимую проверку, без фиктивных30/50 проходов.

Независимый review первоначального плана нашёл действительную неисполнимую зависимость P131 producer/verifier identity и неверное имя store; обе исправлены. Дополнительный review указал на конкретный старый mandatory scientific gate P127 — теперь его переклассификация названа прямо. В evidence сохранены первоначальные замечания и последующая проверка исправленного кандидата; ранний NEEDS_FIX не стирается и не считается автоматическим PASS.

Проверка документов не заменяет сборку и native приёмку. В этом host .NET/PowerShell отсутствуют; Python-rechecks подтверждают только указанные сохранённые данные и source-derived контрпримеры. Кодовый ремонт, fresh TRX и настоящий verifier exit остаются открытыми строками150 до их исполнения.
