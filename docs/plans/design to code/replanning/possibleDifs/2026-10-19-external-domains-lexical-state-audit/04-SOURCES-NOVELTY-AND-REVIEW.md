# Источники, новизна, выполненные проверки и границы

Дата: 19 октября 2026. Проверка относится к указанным ниже снимкам. Исследование подготовило доказательные карточки и эксперименты; алгоритм продукта и обученные состояния не изменялись.

## Зафиксированные версии

| Источник | SHA и роль |
|---|---|
| [external_domains](https://github.com/lubochka/llm_mvp_core/tree/3036a01acec938f0b83fa4ffbda26202cc35d639) | `3036a01acec938f0b83fa4ffbda26202cc35d639` — главный эталон пользователя |
| [codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76) | `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` — текущие исходники, конфигурации и прежние исследования |
| [planning: исходный корпус](https://github.com/lubochka/xiigen-planning/tree/a90350f33955f9c4b15707a4619eeeba5d42e863) | `a90350f33955f9c4b15707a4619eeeba5d42e863` — полная исходная выгрузка Markdown |
| [planning: первая проверенная дельта](https://github.com/lubochka/xiigen-planning/tree/82d146cd7f1a628dbb95bc1989c341ae2e8e6e1d) | `82d146cd7f1a628dbb95bc1989c341ae2e8e6e1d` — параллельные дополнения перед сборкой |
| [planning: вторая проверенная дельта](https://github.com/lubochka/xiigen-planning/tree/02532653562a5f1e27fd75b1b03d4ae23e930e2f) | `02532653562a5f1e27fd75b1b03d4ae23e930e2f` — ещё четыре версии прежних Markdown |
| [planning: перед публикацией](https://github.com/lubochka/xiigen-planning/tree/4cb6a92662b9cb1667c158800ea36d65724bedc5) | `4cb6a92662b9cb1667c158800ea36d65724bedc5` — семь новых Markdown пакета 53 и новая версия README |

Для соседних веток зарегистрированы `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `local_wiki_planning@219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`, `PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2`, `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548`. Их прежние результаты и разборы использованы для сопоставления владельцев находок. Полная повторная проверка всего кода каждой соседней ветки здесь не заявляется; восстановительный эталон этого пакета остаётся `external_domains`.

## Охват двух коллекций прежних находок

| Срез | Число уникальных Markdown |
|---|---:|
| planning `possibleDifs`: 312 корневых +77 вложенных | 389 |
| core `replanning-possible-diffs`: 92 корневых +180 вложенных | 272 |
| Исходный полный корпус | **661** |
| Дополнительные новые Markdown в дельте до `82d146cd` | **10** |
| Уникальные пути на границе `025326535` | **671** |
| Новые Markdown пакета 53 перед публикацией | **7** |
| Итоговые уникальные пути | **678** |

Все исходные 661 документа получены полностью и проверены поиском. Дельта содержит 15 изменённых файлов: 10 новых и 2 изменённых Markdown, 2 Python и 1 JSON. Все 15 последних версий получены и проверены; содержательные новые N52-разделы прочитаны отдельно. На этой границе проверено 673 Markdown-версии, включая две версии уже существующих путей. Следующая дельта `82d146cd → 02532653562a5f1e27fd75b1b03d4ae23e930e2f` добавила четыре версии прежних Markdown, без новых путей: итог **671 уникальный путь / 677 версий**. Все четыре текста и изменённые фрагменты проверены; пересечений RX1/RX2/OBS1 нет, README не менялся. Перед публикацией появился ещё один коммит `025326535 → 4cb6a926`: семь новых Markdown пакета 53 и изменённый README. Все восемь полных текстов проверены на пересечения, содержательные новые гипотезы прочитаны. Итоговый охват — **678 уникальных Markdown-путей / 685 версий**. Нет заявления о ручном чтении каждой строки всех документов.

Воспроизводимые реестры: [исходный manifest](evidence/CORPUS-MANIFEST.json), [адресные поиски](evidence/NOVELTY-SEARCH.json), [список последней дельты](evidence/LATEST-DELTA-MANIFEST.json), [поиск в дельте](evidence/LATEST-DELTA-SEARCH.json), [финальная дельта](evidence/FINAL-DELTA-MANIFEST.json), [её проверка](evidence/FINAL-DELTA-SEARCH.json), [publication delta](evidence/PUBLICATION-DELTA-MANIFEST.json), [её поиск](evidence/PUBLICATION-DELTA-SEARCH.json). Полный обзор исследователя с первичными результатами и источниками находится в [INVENTORY-REVIEW.md](evidence/INVENTORY-REVIEW.md). Поля `cache_path` обозначают местоположение исследовательской копии, а immutable repository/ref/path — удалённый первоисточник.

Поиск имён дополнен чтением ближайших по смыслу карточек. Отсутствие имени класса само по себе не доказывает новизну идеи. Проверка относится к доступному текстовому корпусу; бинарные checkpoints, все архивы и все non-Markdown evidence файлы двух репозиториев в полный охват не входят.

## Что действительно новое в этом пакете

| ID | Точный механизм | Ближайший прежний вопрос | Решение |
|---|---|---|---|
| C1 / KNN-RX1 | `//` внутри C# string удаляет настоящий code tail до AST | Общие tokenizer/feature-loss риски | Конкретная regex-последовательность и вход не найдены в исследованном корпусе; подтверждены исходником и ASCII-зеркалом |
| C2 / KNN-RX2 | Валидная interpolation теряет expression; raw TRAIN и normalized query получают разные AST features | E13 expression-body, E14 type alias | Другие функции и другие входы; сохранение syntax validity не защищает этот сигнал |
| C3 / KNN-OBS1 | `MemberBindingExpression` условного вызова пропускается; та же экстракция сужает E02 denominator | LT-N03 repeated Compile увеличивает counts; новые N52 сравнивают `ObservationCount` с весами | Потеря наблюдения и переучёт наблюдения — разные операции; `MethodFrequencyTable` и `DesignConversionLocalContinueTrainingProbe` — разные получатели |

Последний пакет [53: sparse AdamW, signed scores и AST position](../53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md) рассмотрен отдельно. Его NEW-AGE, NEW-SCORE и NEW-POSITION не совпадают с RX1/RX2/OBS1; новые файлы и блок индекса другого автора сохранены.

Три карточки в [главе 2](02-NEW-CSHARP-INPUT-AND-CALL-LOSS.md) содержат immutable ссылки на исходники, пример до/после, применимость, ремонт, опыт и опровержение. RX1/RX2 имеют общую точку preprocessing и не считаются двумя независимыми историческими результатами обучения. Supplemental случаи `Read/read` и `db.Users/db.Log` сопровождают C3, но не увеличивают счёт основных находок.

Все шесть связанных production-файлов совпадают между external/current после исключения пяти copyright-строк. Это **унаследованные ограничения**, а не доказанные новые source-регрессии RP01. Паритет записан в [source_parity.json](evidence/source_parity.json). Для причинного объяснения ухудшения нужны изменившиеся входы, опции, состояние либо фактический caller; новый прирост качества ещё не измерен.

## Что проверялось, но не объявляется новым

| Область запроса | Итог сопоставления и прежний владелец |
|---|---|
| DPO-контраст, exact material, response masks, градиент | [Глава 1](01-LEARNING-INTERNALS-AND-EXISTING-GAPS.md) ссылается на current исходники и прежние core41/P10/planning38; текущие исправления сохраняются |
| LoRA recipient и размеры A/B | Разделены generic tiny и RP01; старое отсутствие установки wrapper и исправленный current forward уже известны |
| TeachingVector scalar, веса пары и AdamW | Уже core30/P06; новое причинное улучшение от иной шкалы не заявлено |
| Токенизатор, prefix-64, ключи train/query | Уже [vector/KNN](../05-SEMANTIC-VECTOR-KNN.md), [N28](../28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md); raw executor key drift снят как повтор |
| DPO → подзадачи/Units, synthetic targets, verifier | Уже [структурные Unit-пробелы](../21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md); добавлена точная карта текущих трёх записей и контроль G4-Units |
| Восстановление weights/tokenizer/KNN, replay и обычный потребитель | [Глава 0](00-EXTERNAL-DOMAINS-RESTORATION-FIRST.md), прежний core09; новый cache/manifest путь требует matched-state опыта |
| Матрица → KNN, фиксированная смесь и предметные признаки | [Прежний разбор переноса](../12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md), первичный parent integration plan и actual E25 runner; восстановление механизма G2a |
| E09 labels/denominator, E13 expression bodies, E14 aliases, E05 самообучение | Есть прежние владельцы; не используются как новые IDs |
| Флаг `_hasStructuredTraining` после heldout/no-op | Уже P53; уточнено, что здесь речь об оценке/статусе, а не переключении orchestration на другой путь |
| Exact log-probability floor | На проверенном current exact участке floor отсутствует; конкретное подозрение не подтвердилось |
| QR-факторизация как новая ошибка размерности | После проверки формы thin-Q исходное подозрение не принято; карточка дефекта не создана |

Один широкий вопрос может содержать уже исправленные и ещё открытые детали. Эта таблица обозначает владельцев исследования, а не утверждает, что все перечисленные прежние дефекты остаются в current.

## Первичные исторические результаты и архитектурные источники

[Глава 0](00-EXTERNAL-DOMAINS-RESTORATION-FIRST.md) приводит четыре actual result JSON E09/E25/E26/E43 вместе с кодом runner и его реальным типом состояния. Положительная запись E25 подтверждает обучение памяти на POS, E26 — пользу контекстного выбора, E43 — пользу replay в bigram, E09 — положительный архив с несопоставимой validation панелью. Сохранены успехи и указана именно та часть механизма, которую стоит вернуть потребителю.

[Глава 1](01-LEARNING-INTERNALS-AND-EXISTING-GAPS.md) приводит current DPO/LoRA/AdamW формулы, actual конфигурацию RP01 и два scientific-result JSON с изменёнными адаптерами при неизменном нулевом качестве свободных ответов маленькой панели. Это первичные research отчёты; их checkpoints и grade bindings заново не исполнялись.

Прочитаны [RP01 SOURCE-AND-ARCHITECTURE](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md), релевантные RAG index/conformance, родительский [KNN integration plan](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/orchestrator%20implementation/PLAN-KNN-MATRIX-INTEGRATION-v3.md) и [ARBITER-KNN-AUTHORITY amendment](https://github.com/lubochka/xiigen-planning/blob/a90350f33955f9c4b15707a4619eeeba5d42e863/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md). Их архитектурные требования использованы как контракты; текст плана не выдаётся за runtime доказательство.

## Какие навыки применены и что исправил review

Использованы прочитанные проектные skills из [docs/skills current SHA](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/skills): архитектурное решение, phase-planning, plan-review, code-examination, experiment-design, prompt-patch-authoring, human-readable-architecture-audit и словарь human-communication. Применимые правила дали конкретную структуру: источник → механизм → получатель → локальный ремонт → один изменяемый фактор → предметный результат. Условия старых readiness-процессов не представлены как выполненные циклы review этого исследования.

Автор KNN-механизмов и независимый исследователь DPO/LoRA отдельно прочитали source/caller/options и сравнили шесть пар файлов. [Независимый review](evidence/INDEPENDENT-SOURCE-REVIEW.md) потребовал вывести actual C# route, ожидаемые query/control actions и `TotalPairs`, а также явно отделить diagnostic clones от новых семейств TEST. Эти изменения внесены в native проект. Дополнительно проверены все 12 Git blob SHA с учётом одного конечного LF, добавленного локальным кэшем. Число reviewer не равно числу независимых оценок качества модели.

## Выполнено и подготовлено

| Работа | Статус и предел |
|---|---|
| Источники, corpus/novelty, branch body parity | Выполнено; ссылки и JSON приложены |
| Python regex mirror | Выполнено: 4 ASCII fixtures дали заявленные преобразования; [результат](evidence/regex_proxy_result.json) |
| Python case-key arithmetic | Выполнено: `Read×5/read×1` склеивается в count=6, tie зависит от candidate order; [результат](evidence/e02_identity_proxy_result.json) |
| Независимый source review | Выполнено с ограниченной областью; [review](evidence/INDEPENDENT-SOURCE-REVIEW.md) |
| Native C#/Roslyn и actual KNN consumer probe | Проект подготовлен, **NOT_RUN**: .NET SDK в среде отсутствует; компиляция не заявляется |
| G0–G4, новые DPO/LoRA updates, cold semantic quality | Проекты опытов, **NOT_RUN** |
| Новые вызовы китайского генератора и CODEX quality judge | **0** |
| Доказанный новый прирост качества обучения | **Не измерен** |

Публикуется документация и диагностический проект в planning `possibleDifs`. Защищённый core `docs/plans/replanning-possible-diffs/**` использован только для чтения. Полного объяснения всех возможных причин ухудшения этим bounded исследованием не заявлено; собраны новые конкретные механизмы, подробная связь с известными проблемами и проверяемая очередь восстановления.
