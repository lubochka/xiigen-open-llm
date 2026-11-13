# Источники, версии, новизна и границы проверки

Дата: 20 октября 2026. В этом пакете разделены первичный исходник, исторический runtime receipt, повторный пересчёт архивных ответов и ещё не выполненный проект эксперимента. У них разная доказательная сила.

## 1. Зафиксированные версии

| Источник | Версия и роль |
|---|---|
| xiigen-planning / llm_plan3 | `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0` — исходный корпус possibleDifs и RAG-контекст до нашего добавления |
| llm_mvp_core / codex/dpo-implementation | `bf9b543e149ef197da85a767821d98c9b67916d2` — полный локальный срез248 protected MD и current code review |
| Последующее чтение core | `de4ef0f14172dada4daf142a4c66ba19e96bb6d4` — два новых коммита планов, прочитанных перед публикацией; действующие поправки отражены в01/06 |
| language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` — первичные E25/E43 |
| orchestrator-extensions | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` — E09, orchestration-v1, происхождение новых Mode1-особенностей |
| DesignToCode | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` — первичный V210 neural receipt и runner |
| DesignToCode-clean | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` — repeat10 summary и metadata двух checkpoints |
| local_wiki_planning | `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` — CodeTrace064, QueryPlanning134, RequestRouter043/092 и actual runner |

Другие ветки инвентаризовались для поиска доноров, но это не означает полного построчного чтения каждой. Ссылки первичных исторических файлов, методов и диапазонов находятся рядом с соответствующими выводами в02/03.

## 2. Что означают числа покрытия

В исходном possibleDifs обнаружены **869 файлов, из них555 Markdown**. Все555 MD материализованы для поиска совпадений и сопоставления; содержательная проверка была адресной — current owners, подтверждённые historical paths, active experimental methodology и поздние дополнения. Нельзя приравнивать materialization или keyword search к семантическому прочтению каждого абзаца. [Исходный реестр](evidence/planning-source-manifest.json).

В protected-срезе `bf9b543` материализованы **248 Markdown**:92 корневых,156 в `experimets`; среди последних125 P-карточек,24 документа малого пакета и7 общих документов. [Реестр](evidence/protected-source-manifest.json). В начале и после работы проверен Git blob SHA каждой локальной копии. Совпадение **248/248** отражено в [integrity receipt](evidence/protected-integrity.json).

[coverage.json](coverage.json) и [coverage.tsv](coverage.tsv) описывают125 семейств. [subcase-ledger.tsv](subcase-ledger.tsv) содержит470 индивидуальных A/B-постановок и source pointers. Это извлечение конкретных условий плюс семейная triage; **не470 свежих независимых code reviews и не470 выполненных native опытов**. Для выбранных новых карточек source trace проверен отдельно. Наличие общего P-номера означает родство, а не достаточное покрытие каждого уточнения.

## 3. Core изменился независимо во время аудита

Повторное чтение HEAD обнаружило два новых коммита: `a318d13709b405114b3a83360ad45c051488d552` и `de4ef0f14172dada4daf142a4c66ba19e96bb6d4`, оба с сообщением `Small sets testing plan`. [GitHub compare](https://github.com/lubochka/llm_mvp_core/compare/bf9b543e149ef197da85a767821d98c9b67916d2...de4ef0f14172dada4daf142a4c66ba19e96bb6d4) показывает152 изменённых/добавленных пути, все внутри protected-документов. Изменений исследуемого C# нет. [Безопасный metadata snapshot](evidence/live-core-delta.json) содержит только filenames, SHA, status и размеры diff; токенизированных download URLs в нём нет.

Поэтому фраза «защищённая папка не менялась» здесь означает **мы её не редактировали** и сохранили фиксированные read-only копии. Она не означает, что другие коммиты не могли обновить remote. Старый аудит общего `PENDING`, чрезмерного исключения cold/serving и прежних N60-постановок сохранён как анализ `bf9b543`; актуальное состояние сверено отдельно. [Точная дельта и оставшиеся вопросы](06-LIVE-CORE-DELTA.md).

## 4. Как проверена новизна

Для H-M1-RETENTION и H-M1-IDENTITY использованы точные owners и операции: `PromptFlowGroundingModel`, `BuildWeights`, `_weights[pair.FlowRef]`, `FindLearnedPair`, финальный flow filter, повторное обучение разных PairId одного FlowRef. Поиск выполнялся по всем555 planning MD и248 protected MD исходных срезов, затем прочитаны релевантные совпадения. Отдельно сопоставлены P67, P118 и поздние orchestration документы.

P67 про повторную identity не описывает потерю признаков от **разных** PairId одного Flow. P118 про Mode2/3 style-only consumer не описывает несогласованный Mode1 Flow/body. На этой ограниченной базе точных recipes не найдено. Поздняя дельта de4 проверена дополнительно в01/06. Это обоснованная новизна относительно проверенного корпуса; глобальная новизна во всех неизвестных ветках и переписках не заявляется.

FP32 accumulation/margin сначала рассматривался как возможная новая деталь, но был найден в прежних PrepareToGPL/PG-09 документах. Padding, SFT mask/state/order, sparse AdamW age, raw correction key и corrected semantic target тоже уже описаны. Они не выданы за новые открытия.

## 5. Что перепроверено численно

48 архивных native outputs ingress заново оценены исходным frozen scorer и исходным gold. [Наш пересчёт](evidence/ingress-rescore.json) сохраняет результаты по каждому примеру и группе: correct11→19, accepted12→22, wrong accepted1→3. Это проверка исторического измерения, **не новый запуск обучения**. Primary source/report/raw/gold/scorer links находятся в02. Файл `comparison.json` оказался LFS pointer; его payload не объявлен прочитанным.

Для Local Wiki CodeTrace064/QueryPlanning134 дополнительно сверены pair diagnostics и соответствующий runner. [Повторная агрегация](evidence/history-diagnostics-rescore.json) содержит46/50 и50/50 на heldout, отдельно от train. Train/heldout знаменатели, fresh/continued initialization и повторяемость targets разобраны отдельно. Ни один отсутствующий initial receipt не заменён придуманным нулём. Два clean checkpoints имеют один Git blob по API metadata; это доказательство равенства Git-объектов, а не tensor-level анализ остальных checkpoints или отрицание всех исторических успехов.

Ручные Jaccard/overlap расчёты новых Mode1-панелей служат ожидаемым различающим результатом. Они не выполняют роль нового native measurement. JS использовался только для архивного scorer; Python — для инвентаризации, проверки hash и документов. Синтетическим Python/JS learner настоящую реализацию не заменяли.

## 6. Скилы и способ проверки

Применены релевантные разделы запрошенных project skills: [архитектурный классификатор](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/architect-behavior-classifier-SKILL.md), [planning session](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/planning-session-startup-SKILL.md), [plan review](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/plan-review-SKILL.md), [code examination](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/code-examination-SKILL.md), [prompt patch authoring](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/prompt-patch-authoring-SKILL.md), [человеческие термины](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/human-communication-GLOSSARY.md). Большие skill-файлы читались по применимым разделам; полное усвоение всего их объёма не заявляется.

Практически это означает: сначала найти реальный caller и получателя, затем состояние и reader; отличить архитектурную гарантию от наблюдения; назвать исправление в оригинальном коде; проверить один фактор; оформить понятное короткое поручение с заданными входами, бюджетом и правилом любого полезного успеха. Статусы источников, гипотез и результатов не смешиваются. Проектные архивы пользователя использованы как контекст; старые полные CI/Windows-программы не превращены в обязательный запуск этого документного аудита.

Нейронный путь, история/kNN и экспериментальная методика исследованы отдельными участниками; новые карточки и общий индекс дополнительно сверены перекрёстно. Конкретные исправления review и итоговая проверка ссылок/счётчиков фиксируются в [final validation](evidence/final-validation.json). Наличие нескольких читателей не заменяет runtime-доказательства.

## 7. Чего результат не утверждает

Не запускались новый native Train/StepVector, новый inference, новый restore checkpoint, product CI или полный набор470 опытов. В доступной среде не найден .NET compiler/runtime; это наблюдение о среде аудита, а не утверждение о невозможности выполнить программу на исходной машине проекта. Бинарные LFS payloads и фактические Windows paths B0 не восстановлены этим аудитом. Они не подменены предположительными hashes или выдуманной командой.

Новый пакет делает причины и следующий ограниченный опыт проверяемыми. Он не объявляет все гипотезы причиной RP01, все карточки готовыми к запуску, все historical PASS end-to-end качеством или всю библиотеку семантически проверенной.
