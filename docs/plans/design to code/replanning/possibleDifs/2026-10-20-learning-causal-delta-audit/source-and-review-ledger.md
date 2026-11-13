# Источники, навыки, проверка новизны и пределы заключения

## Версии и доказательность

| Источник | Pin / локальная идентичность | Глубина фактической проверки |
|---|---|---|
| xiigen-planning / llm_plan3 | f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9 → a868f46ee3695728406173f8cd945e278ddb0def | Весь Markdown-корпус possibleDifs для поиска; адресное содержательное чтение relevant hits; RP01/RAG/parent docs отдельно |
| llm_mvp_core / codex/dpo-implementation | ab583032562d6656de9dd5ca6fa0a325fd4b1f30 → 65d03a73a76810a0425a41c33d3c7eeaf4cdbc48 | Нейронные computational regions/callers, selected current domain code, полный текст карточек и отдельных retained results; late diff целиком |
| DesignToCode | 8f20ceb8a9bfbadab2ad309226c47dd76e714548 | E07/E09/E25/E43, StructuralKnn, named JSON runs; selected neural source |
| language_translate | f6032aff81f1f83de7c09cf9b9efe6b058dd12b9 | PatternClassifier и selected neural source; общее experiment subtree подтверждено |
| local_wiki_planning | 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a | Два точных ToDpoMetricPair excerpt против current65d03; не полный аудит ветки |
| Присланные пользователем материалы | Четыре локальных файла из project_sources; SHA256 в source-inventory.json | ZIP распакованы с сохранением исходников; использованы compact skills, startup/context и отдельный v2 документ |

Ссылки на код/JSON используют immutable commit, а не плавающий branch URL. Полный Git tree не равен чтению всех файлов; полученные source bytes не равны runtime-проверке. Ни один результат в этом пакете не основан на новом обучающем запуске.

## Корпус и что означает «полностью»

Исходный possibleDifs на f73d229c содержит 1 203 файла, из них **678 Markdown**. Получены все 678 Markdown и 348 выбранных JSON: 1 026 текстовых документов. Большие прочие JSON, бинарные payload и все LFS-объекты не объявлены прочитанными. Новое planning изменение добавило 30 файлов, включая **18 Markdown**; все 18 включены в окончательный поиск. Итого **696 planning Markdown**.

Для RMS-проверки новизны получены **301 protected Markdown** на ab583032: 92 в корне и 209 в experimets. Затем добавлены две N63 карточки, и отдельно проверен весь добавленный текст 156 late Markdown patches. Полнотекстовая совокупность составляет **999 Markdown**, плюс late patch aggregate: 696 planning + 303 protected. Собственные новые документы исключены из поиска. [Точный query/hit ledger](RMS-COORD-ALLOC-01-NOVELTY-SEARCH.json).

Для каталога экспериментов полностью получены 135 исходных карточек и реконструированы **137 актуальных**. Их bytes подтверждены Git blob; выделены **470 P-подслучаев**. Два из них — P86.a/P86.b — оформлены жирными абзацами, а не числовыми заголовками; поэтому первоначальный regex count468 исправлен без обвинения в удалении экспериментов. [Полный структурный реестр](experiment_audit.json).

**Ограничение:** эти числа доказывают полноту названного текстового учёта и поиска. Они не означают ручную семантическую аттестацию каждого подслучая во всех доменах, сборку 137 host или выполнение 470 опытов. Адресное углубление перечислено в тематических отчётах.

## RAG, RP01 и родительские документы

Прочитаны исходные уровни алгоритма DPO→Unit/Flow/Orchestrator/DynamicGraph, RAG01/02/03/04, AGENTS и документы RP01 learning-first pack, включая SOURCE-AND-ARCHITECTURE, CURRENT-CODE-MAP, LEARNING-EXAMINATION, ACTIVE-LEARNING-FIRST-REPAIR, BATCH500 reconciliation и последующие recovery/causal diagnostics. [Session source][session], [RP01 pack][rp01-pack].

Связанные parent directories просмотрены по Git trees; получены и прочитаны относящиеся к задаче документы:

| Родительская папка | Прочитанный документ / назначение |
|---|---|
| llm mvp/dpo legal build | active-codex-execution-03-ilearningmachine-pattern-notes.md — роли обучаемых patterns |
| llm mvp/dpo legal build | active-codex-execution-05-dpo-data-training-measurement-guide.md — данные, обучение и измерение |
| llm mvp/dpo legal build | active-codex-execution-06-ilearningdynamicgraph-architecture-brief.md — higher-order отношения |
| llm mvp/language translate learning | TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md — доменные ablations и split |
| llm mvp/language translate learning | DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md — полезные контрасты chosen/rejected |
| llm mvp/language translate learning | TRANSLATION-TOKENIZER-GUIDE-v1.md — смысл tokenization в языке |
| llm mvp/language translate learning | TRANSLATION-TRAINING-PLAN-v1.md — получение обучаемого материала и потребители |
| llm mvp/local wiki learning | ORCHESTRATION-PROTOCOL-REFERENCE-v1.md — protocol/reference границы |

Это конкретное прохождение ссылок в родительские документы, а не предположение, что вся история находится только внутри possibleDifs. Древние thresholds, обязательные многократные проходы и локальные модельные назначения служат историческим контекстом. Прямые текущие правила пользователя о полезных метриках, малых опытах и documentation-only имеют приоритет.

## Применённые навыки и решения

Навыки использованы из приложенных исходников и указанной пользователем core/docs/skills. Компактные профильные версии прочитаны целиком; большие расширенные тексты планирования/ревью изучены по относящимся разделам. Полное последовательное прочтение сотен килобайт каждого общего регламента не заявляется. [Core skills на source pin][skills].

| Навык / семейство | Применённый принцип и его видимый результат |
|---|---|
| architect-behavior-classifier | Разделить learned memory, gradient learning, fixed rules, report-only logic и restore; назвать actual writer/state/reader |
| phase-planning / planning-session-startup | Сначала learning blockers и конфликты; exact source/data/host/commands до передачи исполнителю |
| plan-review / plan-self-validation | Проверить, что рецепт не заставляет executor выбирать проект, конфликты и модель; устранить results lifecycle и manifest ordering |
| code-examination / generated-code-review | Читать реальные методы/callers; сравнить current/donor; не считать grep или имя класса доказательством обучения |
| claim-verification | Отделить raw recount, retained aggregate, source fact и NOT_RUN hypothesis; не приписывать LFS pointer недоступный payload |
| prompt-patch-authoring | Новый executor prompt самодостаточен по разрешённому действию, бюджету, путям, формату и запретам stop/next |
| human-communication-GLOSSARY / output-readability | Русский отчёт; предметный домен отдельно от алгоритма; каждая метрика сохраняет смысл и знаменатель |
| experiment-design / improvement-measurement / learning-path-audit | Один фактор, настоящий native learner либо доказанная dependency, A0/A1/B0/B1, полезный gain без обязательного роста accuracy |
| conversation-intent-and-self-reflection | Цитированный пример остановки не принят за новое текущее stop; действительное новое stop/next прекращает старую работу немедленно |

При конфликте исторического AGENTS/skill workflow с текущим запросом применены текущие границы: писать только добавления в planning possibleDifs; protected core не менять; не запускать experiment/CI/product-fix pipeline; не продолжать предыдущий опыт после следующего назначения. Это разрешение конфликтов текущим поручением, а не новая неопределённая стадия согласования.

## Независимая проверка и исправления документов

Выполнены три независимых тематических разбора: нейронный learner, исторический KNN/memory и audit experiment cards. Проверяющий истории отдельно прочитал RMS-card/prompt/36-file manifest и соответствующий native/template code.

Проверка RMS обнаружила и закрыла конкретные ошибки подготовки: преждевременное создание results, наследование несовместимых аргументов/meta старого runner, checksum до завершения run.py, неизвестные counts при timeout и неверные относительные report links. Итог — **PASS_DOCUMENTED_DESIGN_NOT_RUN**, с сохранённой историей замечаний. Это проверка документа, не успешная сборка. [Review](rms-independent-review.md).

Интеграционные README/axis-coverage/candidate-register проверены отдельно по актуальным counts, статусам fixes, полезным метрикам, regressions, memory-vs-neural и NOT_RUN. Исправлены две точности формулировок: frozen transpose относится к forward, а не только backward; EXAM измеряется до updates и после них. Никакие новые training calls для исправления текста не выполнялись.

## Границы evidence, которые нельзя потерять при следующей передаче

1. **N-KU comparison.json — LFS pointer.** Payload не получен; ключевые выводы независимо восстановлены из полных raw arms/gold.
2. **P125 и ingress — разные опыты.** Их baseline и знаменатели не объединяются. Cold comparison прочитан; raw cold заново не пересчитан.
3. **EMB/MASK raw пересчитаны.** 12 JSON, Decimal sums/means, без повторного запуска; online pre-step loss и полные process costs различаются.
4. **E07/E09 — не чистая matched migration.** Tier/key, token/AST alignment, target codec, panel indices и timer regions различаются. Положительный structural KNN signal сохраняется.
5. **Матрица→KNN lineage одного алгоритма не установлена.** Это не отрицание слов пользователя; имеющиеся адресные receipts не замещают отсутствующий точный donor/caller того переноса.
6. **New RMS — только source/design.** Нет A1/B1, build, численной runtime equivalence или measured allocation gain.
7. **Новая деталь не внесена в protected.** Запрет изменения соблюдён; подготовленный внешний recipe не называется физическим покрытием protected.
8. **Результаты соседних веток не автоматически current runtime.** Конкретные source writers/readers и статус fixes показаны отдельно.
9. **Выигрыш и причинная атрибуция — разные вопросы.** Полезный измеренный результат сохраняется с ограничением дизайна; несопоставимые области времени не создают корректной speedup метрики.
10. **Публикация заканчивает эту документарную задачу.** Она не запускает новую серию экспериментов и не разрешает внедрение в продукт.

[session]: https://github.com/lubochka/xiigen-planning/tree/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session
[rp01-pack]: https://github.com/lubochka/xiigen-planning/tree/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack
[skills]: https://github.com/lubochka/llm_mvp_core/tree/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/skills


## Последние24 Markdown перед записью ветки

После основного исследования появился planning8aa040fc676973154e009e0eec13689eb9f6bd1d. Все24 новых Markdown получены полностью, включены в дополнительный поиск новизны; названные четыре новых механизма прочитаны. Финальный охват720 planning +303 protected =1023 Markdown, плюс core patch aggregate. Основной999-document ledger сохранён как исходный срез, а дельта дана в [publication-head-reconciliation.md](publication-head-reconciliation.md). Новых raw replays нет.
