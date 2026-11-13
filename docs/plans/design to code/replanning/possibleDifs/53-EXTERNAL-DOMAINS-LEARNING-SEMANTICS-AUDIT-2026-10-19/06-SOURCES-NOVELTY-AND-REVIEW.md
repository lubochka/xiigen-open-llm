# Источники, границы новизны и проверка доказательств

Дата исследования: **19 октября 2026**. Публикация предназначена только для `xiigen-planning`, ветка `llm_plan3`, `replanning/possibleDifs`. Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` использован для чтения/дедупликации; этим исследованием туда ничего не записывается. GitHub write уже разрешён пользователем; повторная процедура согласования не вводится.

**Поздняя дельта после основного review:** [глава07](07-PUBLICATION-RECONCILIATION.md) фиксирует свежий parent `b0308fbaa783758104987e6544d057062ccf6fc3`, две поздние публикации: ещё16 новых Markdown и две replacement версии README. На этом этапе корпус 687; цифры 661/671 ниже описывают сохранённые этапы исследования. Окончательный срез protected core и корпус **689**, включая признание прежнего общего lead у PT-01, зафиксированы в [главе 08](08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md).

## 1. Зафиксированные версии

| Репозиторий / ветка | SHA | Реальный охват |
|---|---|---|
| core / `codex/dpo-implementation` | `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76` | Адресно прочитанные primary C# по DPO/LoRA, training, KNN/Units, PromptToTask и их callers; 272 прежних Markdown для поиска |
| core / `external_domains` | `3036a01acec938f0b83fa4ffbda26202cc35d639` | Приоритетный donor: E07/E09/E13B/E14/E25/E26/E27/E43/E48, primary JSON и соседние исходники; выбранные old/current diffs |
| core / `DesignToCode` | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | Сохранённый положительный ProjectWiki DPO summary025, не вся ветка |
| core / historical ProjectWiki snapshot | `dd22eb10bb4148e3124ffcbb97cf11a64dd3d5bc` | Runner, DpoTrainer, LlmModel, RmsNorm для интерпретации summary/recipient |
| core / `language_translate` | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Ветка и корневое дерево проверены; полного нового аудита нет |
| core / `DesignToCode-clean`, `PrepareToGPL`, `local_wiki_planning`, `orchestrator-extensions` | Соответственно `e346b88a3cc1fcce48aefa626bfc7b00509eefd6`, `31346d8928be4469b01aaf313780e8f950ff91e2`, `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`, `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | Refs обнаружены и закреплены; не выдаются за четыре полностью прочитанных/воспроизведённых донора |
| planning / исходный snapshot | `a90350f33955f9c4b15707a4619eeeba5d42e863` | 389 прежних Markdown; RP01/RAG и родительские F/G документы |
| planning / обновление перед интеграцией | `02532653562a5f1e27fd75b1b03d4ae23e930e2f` | 21 новый commit; 12 изменённых Markdown: 10 новых, 2 обновлённых. Новые тексты получены, адресно сопоставлены; чужие изменения сохраняются |

Проверка core HEAD на этапе интеграции вернула тот же `44507823…`. Дальнейшая публикация planning опирается на свежий parent и ожидаемый SHA ветки; это защищает от перезаписи одновременных правок. Аудируемые исходники всё равно ссылаются на полные immutable SHA.

## 2. Что было действительно прочитано и сопоставлено

Собраны **661 первоначальных Markdown: 389 planning + 272 protected core**. Для дедупликации выполнены полнотекстовые поиски точных имён, выражений и близких русских/английских описаний; совпадения и соседние отчёты прочитаны адресно. После refresh итог — **671 уникальный Markdown: 399 planning + 272 core**. Два обновлённых файла заменяют предыдущие версии в поисковом корпусе, а не считаются двумя новыми документами. [Поисковый ledger](evidence/novelty/novelty-ledger.json) [Реестр корпуса](evidence/novelty/corpus-manifest.json)

**Загрузка и поисковая доступность не равны полному чтению каждого документа.** Полное построчное чтение всех 671 файлов, всех вложенных RAG источников, бинарных моделей, private corpora и всех sibling branches не заявляется. Scripts/JSON прежних пакетов выбирались адресно; Markdown-реестр не означает полноту их небуквенных артефактов.

Новым гипотезам предшествует primary trace по source, затем поиск actual caller и контрдоказательств. У KU разобраны существующие tests как тексты: они проверяют другие случаи и не были запущены. У AUT найдена явная отрицательная граница — отсутствие non-test composition root. У PT различены direct Unit, machine fallback и следующий API consumer. Исторические результаты перепроверены по малым raw JSON и реализациям, а не по названиям PASS/FAIL.

Реестр исторической подзадачи содержит **84 полученных текста**, а не 84 независимых эксперимента. [History manifest](evidence/history/downloaded-source-manifest.json). Реестр AUT связывает 11 точных Git blobs с SHA256; во время переноса первоначальная лишняя конечная LF удалялась только при совпадении с ожидаемым Git blob. [AUT sources](evidence/autonomous/source_manifest.json). Для PT три blob SHA независимо сверены вторым исследователем. [PT review](evidence/review/prompt_to_task_independent_review.md). Для KU сохранены 13 source blobs/URLs и проверка диапазонов ссылок. [KU sources](evidence/routing/source_manifest.json) [KU references](evidence/routing/reference_check.json)

## 3. Новизна — механизм, а не новое название старой темы

| Наш ID | Проверены точные выражения | Ближайшая уже известная тема | Что действительно добавлено |
|---|---|---|---|
| KU-01 | `AreEquivalentLearnedSegmentDecisions`, `learnedDecisionByTarget`, exact error | Multi-router, trained domain router, сохранение many-to-many связей | Конфликт штатного producer unique IDs и target-only aggregate, даже при совпадающем решении |
| KU-02 | `DeterministicRegistryEdgeCandidateProvider`, `ScoreRelevant`, `relevanceFloor`, exact no-match | Domain filter, lexical fusion, learned edge authority | Filter существует, но применяется после global truncation; вне-edge кандидат выключает inference |
| PT-01 | `ContinueWithAcceptedPattern`, `PromptToTaskTaskPatternUnit`, top2 duplicate | Общая тема confidence/margin и N52 asset readiness | Один accepted одинаковый semantic output ухудшает доступность прежнего task |
| PT-02 | `missing_gates`, PromptToTask aliases | Потеря отрицания в других Units; authority/quarantine | Конкретное создание failure-feature из substring gate в данном extractor |
| AUT-WEIGHT-01 | `AutonomousLearningPipeline`, `AutonomousTrainer`, `DatasetWeighter` как отдельные имена, `rareCount/domainCount/numClasses/maxWeight` | E44DatasetWeighterV2, rare-class support и confounded transitions | Single-domain admission делает коэффициент1 на каждом принятом batch; теряется provider weight>1 |
| AUT-REPLAY-02 | Pipeline/trainer names, `ACCURACY_NOT_IMPROVED`, rejected-adapter replay | E43, capability replay aperture/newest/top3 | Момент положительного replay admission относительно последующего candidate verdict |

Для точных цепочек прежних совпадений не найдено. Более широкие поиски **дают совпадения**, и они не скрыты: например, 16 строк с фрагментом DatasetWeighter относятся к историческому `E44DatasetWeighterV2`, шесть широких router/compiler упоминаний — к другим проблемам, четыре fresh PromptToTask упоминания — к asset/index/readiness. Отсутствие точного имени само по себе не было единственным основанием новизны; прочитаны соседние смысловые случаи. Поисковые regex, excerpts и номера строк сохранены в ledger.

Не включены как новые: current repaired DPO gradient, q/v против head, scalar teaching vector, BPE identity/EOF/stride, sum/mean cancellation, SFT first-step/Embedding/bias, Tensor Mean/broadcast из package51, E09 label/denominator, E25/E27 transfer, E43 boundaries, E48 early benefit, translation keys/morphology и ProjectIndexer filename-only/DPO→CE, уже упомянутый peer draft в package51. Они остаются в общей карте и приоритетном восстановлении там, где применимы.

Чужие неопубликованные черновики и недоступные private материалы не обследованы. Поэтому **абсолютная новизна во всём проекте не утверждается**. После окончательной дельты шесть — число конкретных source findings: **пять новых гипотез плюс PT-01 как локализация прежнего generic lead**. Этот счёт не является числом доказанных причин неудачи RP01.

## 4. Роль RAG и применённые проектные навыки

RP01 `SOURCE-AND-ARCHITECTURE` / code map / recovery и causal diagnostic использованы для ориентации в фактическом первом slice. Из session RAG выбраны DPO conformance, legal-build и branch/merge pointers. Ссылки D/F/G разрешались относительно реального дерева репозитория: родительские `llm mvp/llm dynamic flow` и `llm mvp/dpo legal build` не искались как дети session. Прочитаны F arbiter amendment/locked decisions, G04/G06/G07. Их исторические execution banners не подменяли текущий запрос пользователя на исследование и разрешённую публикацию.

| Роль навыка | Применение | Честный load status |
|---|---|---|
| Архитектор: `architect-behavior-classifier-SKILL.md` | Learned selection, fixed integrity, candidate support и many-to-many assembly разделены по ответственности | Полностью прочитан исследователем KNN/Units |
| Планирование: `intent-to-plan-SKILL.md` | Одна цель/вмешательство/consumer, recovery-first и готовое задание | Полностью прочитан интегратором |
| Проверка плана: `plan-review-SKILL.md`, `phase-planning-SKILL.md` | Evidence honesty, скрытый детерминизм, lifecycle, доменный EXAM и отсутствие fake readiness | Прочитаны применимые фрагменты; полная загрузка крупных файлов и всех транзитивных skills не заявляется |
| Проверка кода/данных: `generated-code-review-SKILL.md`, `training-data-gap-audit-SKILL.md` | Source важнее комментария; trace weight/replay, recipient и отрицательное свидетельство | Оба полностью прочитаны исследователем DPO/LoRA; хвост большого code-review файла дочитан отдельно |
| Prompt engineering: `prompt-patch-authoring-SKILL.md` | Самодостаточный малый work order, roles, expected evidence и falsifier | Полностью прочитан интегратором |
| Человеческая коммуникация: `conversation-intent-and-self-reflection-SKILL.md`, `human-communication-GLOSSARY.md` | Русский текст, смысл результата до механики, явные ограничения и предметная применимость | Полностью прочитаны интегратором |

Это проектные файлы из [docs/skills на pinned current](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/docs/skills). Само наличие имени навыка в списке не засчитывается как прочтение. Применимые session/replanning AGENTS прочитаны полностью; massive current core AGENTS в исторической подзадаче — только относящийся блок, без заявления о полном чтении. Более старые scope directives не отменяют явный текущий запрет редактировать protected core и явное разрешение публиковать planning.

## 5. Выполненное, невыполненное и независимая проверка

**Выполнено:** адресный primary/source research; полный текстовый поиск описанного корпуса; три семейства изолированных Python probes; отдельная проверка PT вторым исследователем; review точного итогового кандидата и проверка ссылок/границ публикации. Выходные механические сигнатуры: KU source-ID collision и cutoff; PT margin0 и шесть alias fixtures; AUT weight collapse и replay cohort membership.

**Не выполнено:** native C# / .NET, продуктовый inference/training, новая DPO teacher generation, независимый DPO judge, cold replay модели, CI, benchmark PC, изменение core, публикация адаптера. Native runtime dotnet в доступной среде не найден. Отсутствующий запуск не объявляется FAIL. Source/formula probes не доказывают частоту дефекта в реальных данных и не являются шестью опытами качества.

Контракт будущих DPO ролей взят из RP01/replanning: CODEX с требуемой там Astra 6 ultracode выполняет отбор и отдельную оценку точной пары; допущенная внешняя открытая китайская модель создаёт обе стороны. В исследовании такие вызовы **не происходили**. Работа субагентов в текущем наследуемом runtime — реальное отдельное исследовательское рецензирование, но не выдуманная аттестация исполнения именно этого будущего model contract.

Независимый reviewer проверил exact candidate hashes и принял пакет как **`ACCEPTED_AS_BOUNDED_RESEARCH_DOCUMENTATION`**. Два средних и одно небольшое замечание закрыты: actual downstream source carrier в KU-01; отдельное конфликтующее state в PT-01; confidence conflict как свойство текущего контракта. [Исходная стадия review](evidence/review/final-candidate-review.md) и [итоговое дополнение](evidence/review/final-candidate-review-addendum.md) сохраняют замечания и точные hashes. Историческую главу 00 отдельно проверил интегратор по primary JSON/source; автор этой главы не выдаёт её самопроверку за независимый review. Итоговый content manifest связывает подготовленный вариант с опубликованным деревом. Git commit/ref readback подтверждает размещение документов, а не качество модели.
