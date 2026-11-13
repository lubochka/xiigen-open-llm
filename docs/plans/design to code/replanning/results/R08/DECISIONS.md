# R08 — действующие решения CP3 и сохранение прежних требований

**13.10.2026 · R08-CP3.** Полный model-ready work order — [R08-PARALLEL9](../../run-prompts/R08-PARALLEL9.md). Этот файл фиксирует решения, но не повторяет его как второе задание. Research остаётся IN_PROGRESS; runtime/learning/independent reviewer не выполнялись.

Полная [версия DECISIONS CP2](https://github.com/lubochka/xiigen-planning/blob/7cbf1a744d009af57b8ac47c77442f900a8113ee/llm%20mvp/design%20to%20code%20session/replanning/results/R08/DECISIONS.md) доступна неизменно. Старые source receipts/requirement IDs сохраняют историческое значение. Ни одно «ACCEPTED» из прежнего change-map не становится доказанным runtime результатом.

## D01 — primary evidence, а не повторение чужой работы

**Сохранено и уточнено.** Original/task/goal, WorkLeft RESEARCH-01/REVIEW-20, current teaching/authority и относящиеся RAG→primary/hybrid/experiment sources обязательны для использующих их claims. R08 не ждёт полного R01/R03 или соседнего DONE и не заменяет собственное primary чтение sibling summary. Общий механизм не перечитывается целиком для каждого предмета.

Пустой API content/слишком большой файл — ошибка доступа к тексту, не доказательство отсутствия содержания. Review-20 остаётся адресным непрочитанным остатком R08-A; никакого повторного двадцатипроходного исследования вместо его чтения.

## D02 — exact DPO roles и ориентация

**Сохранено, неоднозначность исправлена.** По current RUN §§2.1–2.2: CODEX GPT-6-Astra/ultra выбирает разрешённый lesson; external admitted Chinese model пишет обе final chosen/rejected; CODEX отдельно оценивает exact human-readable pair/context и binding к projection; Student получает разрешённый update; отдельный фиксированный Evaluator сравнивает Student outcomes. DPO reference model — не judge.

Предлагаемая диагностическая схема: CHOSEN_BETTER / REJECTED_BETTER / TIE / BOTH_BAD / UNJUDGEABLE. Это не объявление существующего runtime enum. Wrong orientation ведёт к rejection, не silent swap с прежним authorship/assessment. Любая новая авторская попытка требует действующего разрешения/остатка бюджета. Изменённые bytes/context не наследуют старую оценку. Pair suitability, actual optimizer update и Student improvement независимы.

## D03 — no coverage proxy, no new test campaign

**Сохранено.** Windows/Actions/build/test counts/coverage, row counts, hash changes и loss alone не доказывают learning quality. В этом R08 ничего из этого не запускается и existing checks не отключаются. Integrity/admission/source/state constraints сохраняются как необходимые свойства контура; их форма не превращается в обязательную новую unit-test программу. Capability оценивается ML experiments на независимых families.

## D04 — combined deterministic + learned mechanism

**Сохранено.** Для каждого механизма: deterministic часть, learned authority, state/parameters, objective/signal, producer, ordinary consumer, fallback и falsifier. Допустимые constraints не подменяют learned final semantic arbitration. Correct deterministic answer может быть chosen; deterministic execution learned weights остаётся learning. Memory-based kNN learning не выдаётся за neural DPO, но и не отвергается по названию.

## D05 — четыре роли, а не четыре названия одного score

**Сохранено.** Unit: typed many→many domain/vector/output associations. Flow: связи и коррекция. Orchestrator: асинхронные допустимые combinations/selection. DynamicGraph: higher-order patterns/transfer. Actual ILogical*/LearningMachine mapping подтверждается поведением и caller chain; не вводятся четыре trainers. Для причинного сравнения lower state замораживается там, где это позволяет исследуемый механизм; это не новая глобальная очередь разработки всех уровней.

## D06 — representation, dynamic kNN, LoRA/projector отдельно

**Сохранено и уточнено.** Retrieval embedding, loss-visible tokens, feature extraction, kNN pool/metric/index/k/reranker, low-rank parameters и output projection — разные factors. LoRA не сама токенизация и не гарантированное преобразование domain vector в model token. Нужно назвать semantic coordinates/dimensions/masks/recipient/objective и ordinary consumer; unknown не masquerades as implemented.

Векторный collision требует доказать actual learning/decision path, прежде чем объявить learner неработоспособным. Dynamic k и law-sensitive geometry — проверяемые гипотезы, не уже найденные лучшие функции. Их fitting/calibration не использует closed exams. Показатель confidence отделён от measured correctness.

## D07 — разные приёмки, разные статусы

**Уточнено.** PROMPT_REPAIR означает исправленный исполняемый prompt и его readback. RESEARCH_COMPLETENESS требует заполненных primary/code/passport/split/experiment результатов, а не одних prompts. RUNTIME_MECHANISM, LESSON_OCCURRED, LEARNING_GAIN, ORDINARY_ADOPTION и INDEPENDENT_REVIEW не выводятся из документа.

Непрочитанный обязательный source или непрослеженный consumer удерживает соответствующий research claim OPEN. У R13/R18 может быть достаточно versioned subset для первого урока независимо от полной whole-program completeness. PARTIAL не переименовывается в FAILED/DONE.

## D08 — current WorkLeft authority вместо механического переноса skill rules

**Новое разрешение обнаруженного конфликта.** WorkLeft-02 02.10.2026 / current RUN уточняют S04: noise-aware criterion, actual existing CodeDocumentation Student, local lesson отдельно от remote publication. RESEARCH-01 95%+ остаётся целью программы, не confidence и не обязательным порогом принятия каждого optimizer step.

`experiment-design-SKILL.md` содержит старые five intents, replay 70/30 и auto-escalation; `improvement-measurement-SKILL.md` — mandatory unit-test layer/test-count reporting. Применяются их полезные causal/behavioral принципы, но эти конфликтующие правила **не активируются** в данном R08. Не править core skills этим нарядом; указать conflict/source/scope. Старое SCALE_UP не доказывает correct algorithm. Непротиворечащие StackDetector-before-normalization, meaningful baseline и rare-class awareness сохраняются; group disjointness не нарушается stratification.

## D09 — не повторять work orders под разными именами

**Новое.** Фактический START-NEXT-RUN уже маршрутизировал R08 в `run-prompts/R08-PARALLEL9.md`; именно этот файл исправлен. `run-prompts/R08.md` теперь явный alias. Старые LEARNING-WORK-ORDERS/PARALLEL9 summaries и CP2 A–E — reference/delta, а не дополнительная обязательная последовательность.

| CP2 часть | Где сохранено в CP3 | Что исправлено |
|---|---|---|
| A direct sources/architecture | §2, §3, A | WorkLeft review baseline и successor authority; bounded RAG receipts |
| B full code-chain audit | C, D и handoffs R04–R07 | Сохранены предметные proof obligations; снято дублирование полного соседнего аудита |
| C passports only after A/B | B1–B7 + C linkage | Все семь предметов начинают по direct primary, не ждут полного code audit |
| D DPO assessor | D | REJECTED_BETTER, no swap, human-readable exact pair vs loss-visible representation |
| E experiments/full PC | E | Existing Student first; bounded one-factor contrasts, noise/cost/retention/ordinary reopen |

CP3 A–E — пять частей одного R08, не пять реально вызванных моделей. Полный context/skills/references/prompt/good/bad/edge/verification/output/return находится у каждой части в canonical packet. Детей делить по предмету/вопросу, не копировать все мегабайтные исследования каждому.

## D10 — параллельно с R01–R12, без restart cascade

**Текущее прямое указание пользователя.** Историческое PARALLEL9 в имени файла не ограничивает запуск R08. Start по primary, consume exact versioned subsets, late changes сверять по affected blobs/semantics. Ни R07 DONE, ни R10 join, ни законченный R11/R12 analysis не prerequisite R08-A/B. Один owner пишет results/R08; чужие статусы не меняются. Broad shared dispatcher для других rounds этим адресным ремонтом не переписывается.

## D11 — source-family independence и корректные отрицательные примеры

**Сохранено и конкретизировано.** Train/development/operational/certification/composition/OOD различаются; версии/переводы/paraphrases/renders/derived pairs принадлежат одной исходной family. Проверяются index, teacher context, caches, fitting projector/tokenizer/reranker и feedback lineage. Public common specification не тождественна protected exam case/reference.

Исторический пункт «trivial negative vs law-sensitive negative» не разрешает обучать Student заведомо непригодными/неправомерными парами. Unadmitted examples допустимы только в документной/offline диагностике; реальная comparison training arm должна проходить действующий admission contract. Правильное user feedback тоже не предполагается без проверки scope/authority/conflict.

## D12 — первая реальная учебная петля без подмены продукта

**Уточнено current primary.** Первый recipe готовится для existing CodeDocumentation Student и admitted curriculum. Семь предметов и extras остаются в общей карте, но полный curriculum/весь DTC UI/посторонний Windows release не новый prerequisite первого урока. Actual loader/recipient/trainer/ordinary entry устанавливаются code evidence; неизвестный seam передаётся точному владельцу, не выдумывается исполняемая команда.

One-pair/no-repeat, full compatible state, accepted checkpoint reopen и source restrictions сохраняются. Bulk — окно новых последовательных уроков. Model noise, identical weights, NO_TRAINING_NEEDED, failure/rollback и неразличимый эффект отражаются честно. Counterfactual comparisons не разрешают повтор update одного pair ID в том же state lineage.

## D13 — компактность только при измерении всей цены

**Сохранено.** Сравнивать качество и полную цену на matching knowledge/context/memory/compute assumptions; учитывать all attempts/branches, base/activations/optimizer/adapter/projector/index/graph/cache/snapshots, inference и evaluation. Уменьшение LoRA файла, высокая pair acceptance или лучшая training loss не PC-fit и не useful learning.

Порог эффекта, retention/regression constraints, uncertainty/stopping/multiple-comparison rules и ресурсные caps задаются до outcomes в R13. Historical hardware estimates — не измерения текущей машины. Неполная evidence = INVALID_EVIDENCE, а не нулевой score.

## D14 — независимый examiner должен реально существовать

**Сохранено.** Requested Astra/ultra и actual model/effort/call/provenance различаются. Авторская проверка этого packet не независимый вызов. R15/R25/authorized runtime owner принимают exact candidate, frozen rubric и evidence; missing/unavailable invocation = NOT_PERFORMED, а не PASS. В текущем ремонте никто не запускал внешний semantic judge.

## D15 — сохранённый остаток и адресные передачи

R08-A: дочитать уникальные RESEARCH-01-REVIEW-20 deltas, previous-round/RAG/domain primary spans; API size failure не объявлять отсутствием файла. R08-B/C: заполнить actual source-family/consumer fields и исходные extras. R04/R06/R07/R12: подтвердить applicability трёх C0 code observations и current causal seam. R05/R13/R18: bound exact actual learner/LoRA/projector/ordinary consumer before runtime recipe. R21/R22/R23/R24: curriculum, ML contrasts, full PC, feedback.

Каждая передача содержит origin/local ID, source ref/blob/span, доказанный subset, точный вопрос/остаток, affected domain/role и recipient. Эти назначения не заявляют работу соседей завершённой и не дают автоматическое разрешение запускать experiments.
