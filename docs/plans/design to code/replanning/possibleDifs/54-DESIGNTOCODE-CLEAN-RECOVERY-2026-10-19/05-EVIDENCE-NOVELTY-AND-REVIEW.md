# Доказательства, новизна, связь с RAG и проверка выводов

Дата: 19 октября 2026. Этот документ объясняет область исследования и основания выводов. Он не заменяет прямые C# источники в [02](02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md), neural comparison в [03](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md) и исторические receipts в [01](01-HISTORICAL-BASELINE-AND-RECOVERY.md).

## 1. Версии и объём проверки

| Объект | Зафиксированный commit | Что проверено |
|---|---|---|
| Основной успешный донор DesignToCode-clean | e346b88a3cc1fcce48aefa626bfc7b00509eefd6 | Source paths, neural diff, E09/E25/E43, DesignConversion checkpoint и runtime |
| Сравниваемая реализация codex/dpo-implementation | 358af351e22a92a3f7938d1f9221d5aeb7ddb053 | Те же механизмы и фактические callers; HEAD повторно проверен перед сборкой, не изменился |
| Первоначальный planning/possibleDifs | b0308fbaa783758104987e6544d057062ccf6fc3 | 416 полных текстовых файлов |
| Защищённый core/docs/plans/replanning-possible-diffs | 358af351e22a92a3f7938d1f9221d5aeb7ddb053 | 286 полных текстовых файлов, включая вложенные карточки; только чтение |
| Обновления planning во время работы | 806d1c8534f9676cd80e7c1bf3c488163e549a4d | Ещё 32 новые/изменённые Markdown-версии после первоначального снимка |
| Последняя сверка planning | b95278b25a6697b42ef7025e6965a9f2837a34b7 | Ещё 9 изменённых Markdown-версий; уточнения peer GDC и навигации |
| Соседние DesignToCode / language_translate / external_domains | 8f20ceb8… / f6032aff… / 3036a01a… | Адресная сверка E09/E25/E43; полный SHA и subtree evidence в историческом приложении |

Первоначальный корпус — **702 текстовых файла, 14 176 346 source bytes**. Получены полные тексты всех выбранных .md/.txt, ошибок скачивания не осталось. Точный набор путей и Git blob SHA перечислен в [CORPUS-MANIFEST.json](evidence/CORPUS-MANIFEST.json). К нему добавлены 32 свежие версии, затем ещё 9 изменённых версий: всего **743 текстовые версии и 733 различных repo/path**. Изменённые документы представлены более чем одним снимком; число версий не является числом уникальных документов.

Полнотекстовый поиск охватил весь этот выбранный корпус; вручную семантически разобраны релевантные совпадения и ближайшие владельцы. Утверждения о построчном ручном review всех документов или исчерпывающем просмотре всех code/JSON/binary artifacts не делаются. Новые scripts/results других агентов не запускались. Вне novelty-corpus отдельно прочитаны RAG, исходные планы, skills, конкретные code paths, архивные JSON и metadata соседних веток.

Source excerpts для двух основных находок повторно получены по commit/path/line range: [SOURCE-SPANS.json](evidence/SOURCE-SPANS.json). Поле blob — Git identity целого файла; retrieved_content — полученный текст указанного диапазона. Corpus mirror использовался для поиска, поэтому возможный дополнительный завершающий newline локальной копии не объявляется побайтной идентичностью Git blob. Для исторических локальных snapshots это различие отдельно оговорено в [HISTORICAL-SOURCES.md](evidence/HISTORICAL-SOURCES.md).

## 2. Что осталось новым после повторной дедупликации

### CLEAN-CP01: новый эффект на импорт, прежний факт удаления feature

В исходном корпусе не было exact совпадений по source_manifest:hash_diversity, StageLearnedWeights, CHECKPOINT_LEARNED_WEIGHTS_INVALID и соответствующему семейству whitelist keys. Однако свежий peer [54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW](https://github.com/lubochka/xiigen-planning/blob/806d1c8534f9676cd80e7c1bf3c488163e549a4d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md#L12-L23) уже описал удаление feature и различный смысл hash diversity при train/inference.

**Его приоритет сохранён.** Новая добавка этого пакета — прослеженная цепь old producer → сериализованное допустимое состояние CLEAN → current all-or-nothing StageLearnedWeights → отказ также в доступе ко всем оставшимся допустимым weights. Из неё следует отдельный предметный вопрос миграции состояния. Предложенный мигратор не возвращает retired provenance feature в runtime и не переносит старые quality claims автоматически.

Статус: **PRIOR_FEATURE_RETIREMENT + NEW_WHOLE_CHECKPOINT_REJECTION_AND_MIGRATION_CHAIN**. Это дополнительный source-backed механизм относительно обоих проверенных снимков реестра. Не утверждается, что тема несовместимости checkpoint впервые возникла в проекте.

Ближайшие старые владельцы: [общий checkpoint restore](../06-CHECKPOINT-RESTORE-ADOPTION.md), [legacy donor migration](../43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/01-LEGACY-CHECKPOINT-GAP.md), [P123 visual critic](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/cards/P123-design-conversion-critic-evidence.md) и [core64 finite admission](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/64-design-conversion-finite-admission-callers.md). Они задают контекст, но не подменяют найденный конкретный whole-checkpoint guard.

### CLEAN-CTX01: новая цепочка runtime conditioning, частично наследуемая проблема

Exact поиск srcenc, vecp=, BuildPolicyPrompt, DesignConversionDpoPolicyRuntimePair и root ownership family не нашёл прежнего описания этой цепи ни в 702 исходных документах, ни в 32 свежих версиях; последние 9 изменённых версий также сверены отдельно.

Смысловой сосед [canonical prefix16](https://github.com/lubochka/xiigen-planning/blob/806d1c8534f9676cd80e7c1bf3c488163e549a4d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-SOURCE-PARITY-AND-NEW-GAPS-2026-10-19.md#L12-L15) относится к другому encoder и локальным feature weights. [PromptToTask PT-01/PT-02](https://github.com/lubochka/xiigen-planning/blob/806d1c8534f9676cd80e7c1bf3c488163e549a4d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/03-NEW-PROMPT-PATTERN-GAPS.md) разбирает выбор паттерна и aliases. Здесь проверен переход source encoder policy → coordinator-issued RuntimePair → реальный runtime log-prob scorer. Prefix8, canonical prefix16 и оконная обрезка prompt — разные операции, их нельзя объединять по похожему названию.

Статус: **NEW_CONCRETE_RUNTIME_CONDITION_CALL_CHAIN; INHERITED_MISMATCH; QUALITY_EFFECT_NOT_MEASURED**. CLEAN тоже использовал иной runtime prompt, к тому же synthetic стороны. Поэтому исправление должно сохранять нынешние точные HTML/CSS стороны; откат всего runtime не следует из находки.

### Снятые и уточнённые кандидаты

| Кандидат | Итог проверки | Почему не новый headline |
|---|---|---|
| Replay пополняется раньше quality promotion; current/replay SourceId overlap | HIST-R01 в [04](04-DOMAIN-MICRO-EXPERIMENTS.md) уточняет [AUT-REPLAY-02](https://github.com/lubochka/xiigen-planning/blob/806d1c8534f9676cd80e7c1bf3c488163e549a4d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/04-AUTONOMOUS-LEARNING-CONDITIONAL-GAPS.md) | Fresh peer уже владеет основной цепью; live RP01 consumer не доказан, повтор consumed pair может правомерно отвергаться |
| DTC train sum, runtime mean | Не подтверждено в прослеженном caller | DTC builder сохраняет legacy material version; exact response-sum другого consumer не переносится автоматически |
| «В clean LoRA не теряла graph, а current потеряла» | Не подтверждено точным diff | Подключение LoRA и правильный DPO backward уже есть в pinned CLEAN |
| AdamW отменяет scalar Weight/TeachingVector | Известно | N26, language-restore/04a; источник не новый |
| Overflow clip и decay-only update, sparse optimizer age | Уже есть владельцы | Карточки численной устойчивости и sparse Adam; [crosswalk 03](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md#4-покрытие-всех-запрошенных-осей-и-предыдущие-владельцы) |
| Pre-update metrics рядом с post-update artifact | Уточнение semantics отчёта | Отдельный current quality gate выполняет trial calibration; ошибочное promotion по этим старым метрикам не доказано |
| E09/E25/E43, E26 lookup, 4→6 executors, directionless KNN | Восстановление известных линий | Их положительные/отрицательные результаты не присваиваются как новые находки |

Нулевой exact match — средство поиска, а не самостоятельное доказательство новизны. Окончательное решение принято по сравниваемым функциям, caller, состоянию, причинному следствию и опыту, который может опровергнуть гипотезу.

## 3. Связь с исходным планом и RAG-требованиями

Исследование использовало [RP01 pack README](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md), [fast-learning contract](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-V6-FAST-LEARNING-CONTRACT-2026-10-14.md), [learning recovery/code audit](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), исходные RAG и отсылки в родительскую директорию. Исторические документы трактуются в их роли; наличие запланированного узла не доказывает, что он обучен или вызывается сегодня.

| Исходная потребность | Проверяемый контракт реализации | Где разбирать отличие и малый опыт |
|---|---|---|
| Unit принимает domain/vector/input/output и допускает many→many | Восстановлены нужные записи, k и список кандидатов; обученный экземпляр действительно получает запрос | R0/R1/R2; [03 crosswalk](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md), [исходные роли](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/original%20task.md) |
| Flow корректирует отношение входа к выходу между Units | Каждый под-Unit получает локальный вход, локальный target и память нужного scope; конечный ответ не рассылается всем как один урок | [Subphase conditioning](../2026-10-19-language-restore/03-subphase-conditioning.md), [mapping к узлам](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/learning-sets/MAPPING-DOCUMENT-TO-NODES.md) |
| Orchestrator и graph должны использовать комбинации Units | Один и тот же learned state проходит через наблюдаемый обычный route и меняет возвращаемый ответ | R0 и стадия ordinary consumer в каждом [опыте 04](04-DOMAIN-MICRO-EXPERIMENTS.md); [orchestrator audit](../52-ORCHESTRATOR-DOMAIN-MICROEXPERIMENTS-2026-10-19.md) |
| Обучение продолжается с сохранённой модели и DPO | Раздельно проверены neural weights/optimizer/RNG, feature weights, KNN memory, Unit/Flow IDs и источник уроков | CLEAN-CP01, neural restore в 03; [цель раунда](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/goal%20of%20the%20round.md) |
| DPO строится из выявленной слабости, а обе стороны отвечают одному условию | Сохранены точные prompt/response bytes и роль каждой стадии; сигнал оценки не заменяет исходную задачу | CLEAN-CTX01; [алгоритм DPO](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/design%20to%20code%20session/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%20%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D1%8F%20%D0%94%D0%9F%D0%9E.md) |
| KNN учитывает область применимости, риск и прошлые результаты | Scope применяется до similarity; failed examples сохраняют корректную роль; близость не подменяет окончательную оценку | R1–R3 и negative controls; [Arbiter KNN amendment](https://github.com/lubochka/xiigen-planning/blob/b0308fbaa783758104987e6544d057062ccf6fc3/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md) |

Из этого crosswalk не выводится, что все плановые Units отсутствуют. Это перечень границ, которые требуется проверить в конкретном consumer. Отдельно следить за отрицательной памятью: «результат был rejected» не означает, что пример бесполезен для KNN. Он может быть предупреждением о типе ошибки, если scope, роль и качество свидетельства сохранены. Но отрицательная память не делает повтор уже consumed DPO-пары допустимым.

Домен здесь — предметная область. AST-функция полезна при валидном дереве и задаче, зависящей от структуры; POS/order — при задаче о языковом порядке; визуальный source condition — при преобразовании макета. Направление языка, конкретный LoRA adapter и номер слоя не превращаются автоматически в новые домены. Измерения разных задач не объединяются в одну accuracy.

## 4. Правила постановки небольшого опыта

Каждая карточка задаёт один причинный контраст, фиксированное исходное состояние, одинаковые inputs и target rubric, split по source families и критерий отрицательного вывода. Сначала диагностируется механика, затем использование результата обычным consumer, затем качество предметного ответа. **Import PASS, изменившийся вес и уменьшающийся loss — разные технические факты; ни один не равен сам по себе улучшенному ответу.**

Для восстановления не нужно одновременно менять donor checkpoint, rank, tokenizer, teacher и размер корпуса. Такой опыт не покажет, что сработало. Указанные десятки примеров и десятки минут — ориентир короткого исследования при готовом окружении; это не замер скорости и не обещание включить поиск отсутствующего checkpoint или новую интеграцию в тот же срок.

Для CLEAN-CTX01 первая component-проверка formatter/windowing может обойтись без новых optimizer updates. Полный runtime A/B требует двух допустимых coordinator-owned carriers с одинаковыми semantic inputs, собственными корректными bindings и независимо выданной authority. Ручное редактирование подписанного receipt или повтор уже consumed capability измерит другую проблему. Существующие guards сохраняются.

При новых DPO-уроках обе стороны создаёт предусмотренная внешняя открытая китайская модель. CODEX Astra 6 ultracode выполняет делегированный человеческий выбор случая и независимо оценивает exact pair по предметной рубрике. Оценка heldout не раскрывает labels в обучающий или рабочий source prompt. Это описание ролей будущего опыта; вызов именно этой модели или внешнего teacher в текущей сессии не заявляется.

Ресурсный журнал: число уникальных пар и presentations, длины tokens, actual updates, trainable tensors/elements, время load/tokenization/forward/backward/optimizer/readback, размер восстановленной KNN памяти и время обычного запроса. Влияние stochastic seed отделяется от фиксированных правил, которые выбирают ответ за модель.

## 5. Применение запрошенных skills и независимый review

Все перечисленные project skills взяты из docs/skills на pinned current. Команда применяла релевантные разделы; формальная сертификация полного выполнения всех тысяч строк skills не заявляется.

| Область пользователя | Применённые skills | Как это изменило результат |
|---|---|---|
| Архитектура | [architect-behavior-classifier](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/architect-behavior-classifier-SKILL.md), [trainable-architecture-decision](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/trainable-architecture-decision-SKILL.md) | Прослежены реальные producer/consumer, тип обучаемого состояния и границы переноса на RP01 |
| Планирование | [phase-planning](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/phase-planning-SKILL.md) | В начало вынесено восстановление donor; новым идеям заданы отдельные контролируемые опыты |
| Проверка планирования | [plan-review](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/plan-review-SKILL.md) | Исторические метрики, вызывавшийся learner, scope и source-disjoint panels разделены |
| Проверка кода | [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/generated-code-review-SKILL.md) | Проверен graph/gradient/optimizer/caller; популярные, но неподтверждённые neural версии сняты |
| Prompt engineering | [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/prompt-patch-authoring-SKILL.md) | Найдено точное несовпадение условия, предложен общий source encoder с отдельной проверкой входных bytes |
| Человеческая коммуникация | [human-communication-GLOSSARY](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/human-communication-GLOSSARY.md), [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/skills/human-readable-architecture-audit-SKILL.md) | Домены не смешаны с механизмами; доказанное, возможное и неизвестное отмечены рядом с каждым выводом |

Исследование разделено между history/experiments, clean/consumer и DPO/LoRA reviewers. Нейронный reviewer дополнительно проверил чужие source chains CLEAN-CP01 и CLEAN-CTX01. Существенное замечание — одноразовая runtime authority — включено в A/B. Исторический reviewer повторно проверил новые peer документы и снял самостоятельную новизну replay-кандидата. Root сверил ключевые spans и интегрировал свежий peer owner удаления feature.

**Итоговая межагентная проверка: PASS_WITH_LIMITS — SOURCE/DOCUMENT REVIEW.** Проверены причинные цепочки, clean/current и exact/legacy, доменная очередь, attribution свежих peer findings, условия A/B и ключевые evidence spans. Замечания к producer trigger, повтору carrier, области authority и согласованию очереди исправлены и повторно прочитаны. [Review receipt](evidence/REVIEW.md). Reviewer ранее подготовил главу03, поэтому это не независимый second-review каждого её утверждения; root отдельно проверял ключевые первичные spans. Review относится к документам и source reasoning; отдельный запущенный evaluator CODEX Astra 6 ultracode, multi-model benchmark и native quality review не заявляются.

## 6. Фактически выполненная работа и ограничения свидетельств

Выполнены pinned source retrieval, чтение архивных receipts, сравнение определённых code paths, полнотекстовый поиск novelty-corpus, semantic review релевантных владельцев, проектирование небольших опытов и проверка документов. Изученные репозитории и защищённый core-каталог не изменялись этой исследовательской работой. Публикуется только пакет документации и навигация внутри planning/possibleDifs.

Два workers в начале исследования, до передачи им ограничения session/AGENTS.md на новые runners, выполнили локальные синтетические Python-диагностики: membership/string-prefix и комбинаторику SourceId overlap. Это не C#, не model training и не проверка качества. Их scripts/results не включены в пакет и не используются как допустимое доказательство native эффекта. Математические следствия в карточках опираются на приведённые C# исходники. Последующая обработка файлов, поиск, сравнение и проверка ссылок — операции над документами.

Новых C# builds/tests, native experiments, optimizer runs, inference, внешней генерации DPO и модельного judging в этой сессии не было. **Восстановленное качество не объявляется измеренным.** Полный архивный CLEAN feature checkpoint с нужным key не найден в просмотренных 309 entries DesignConversion evidence и root models; доступный producer manifest не содержит сами weights.

Положительный пользовательский опыт в CLEAN остаётся важным свидетельством и первым направлением восстановления. Чтобы превратить его в воспроизводимое исправление, следующий этап должен связать успешный state с точным алгоритмом и штатным получателем, после чего измерить предметный результат на одинаковой закрытой панели.



## 7. Дополнение после параллельной публикации

Во время публикации planning получил commit `6b97c01deecc0382e1a5b5c68b10d520a483c1f2` с новым peer-пакетом. Дополнительно получены 15 Markdown-версий; общий проверенный текстовый набор стал 758 версиями / 747 repo-paths. Основной manifest и review выше сохранены в их первоначальной области до b95278b; [публикационная сверка](evidence/PUBLICATION-RECONCILIATION.md) фиксирует новый снимок, владельцев и подтверждение различий. В частности, StageLearnedWeights теперь встречается в peer-review округления, поэтому нулевой exact-match по имени метода не переносится на последнюю дельту. Новый whole-checkpoint consequence и RuntimePair condition различаются по проверенному смыслу.
