# 04 — Исполняемое задание для CODEX/Kiro + независимая проверка архитектора

**19.10.2026 | ДОКУМЕНТАЦИЯ ОПУБЛИКОВАНА; выполнение .NET экспериментов NOT_STARTED.** 
**Единственная разрешённая запись этим исследованием:** `lubochka/xiigen-planning@llm_plan3/llm mvp/design to code session/replanning/possibleDifs/**`. **Запрет:** НЕ изменять `lubochka/llm_mvp_core/docs/plans/replanning-possible-diffs/**`; никакие исходники core этим документарным этапом не изменялись. Для будущих native опытов — отдельный изолированный scratch worktree/ветка, без merge в core и без миграции реальных checkpoints до review.

## Подключить скилы, а не просто упомянуть роли

Из [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills) загрузить полный смысл relevant skills:

- **Архитектор:** `architect-behavior-classifier-SKILL.md`, `trainable-architecture-decision-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `architectural-decision-testing-SKILL.md`, `learning-path-audit-SKILL.md`.
- **Планирование:** `planning-session-startup-SKILL.md`, `phase-planning-SKILL.md`, `codex-planning-quality-GUIDE.md`, `reference-inventory-and-source-map-SKILL.md`.
- **Независимая проверка планов и кода:** `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `audit-protocol-SKILL.md`, `claim-as-hypothesis-SKILL.md`, `learning-loop-closure-SKILL.md`, `training-data-gap-audit-SKILL.md`.
- **Промпт-инжиниринг:** `prompt-patch-authoring-SKILL.md`, `planning-dpo-authoring-SKILL.md`; не добавлять несколько независимых патчей сразу; wrong/correct pairs с независимыми проверками.
- **Человеческая коммуникация:** `human-communication-GLOSSARY.md`; **домен** = предмет (документация/перевод/программирование/оркестрация); **профиль** = Angular→.NET→SQL или RU→EN; **Unit/Flow** = получатель/архитектура; **LoRA/kNN** = механизм, не домен.

Этот пакет использует выводы этих скилов: traceability, separation SOURCE vs CLAIM vs RUNTIME, честные A/B, привязка owner и ориентация на предметный ответ. Незагруженные в конкретной будущей сессии материалы помечать `NOT_LOADED`, не изображать завершённый review.

## Точная команда исполнителю / Architect → Executor

**Роль:** исследовательский исполнитель, который воспроизводит потерю и возвращает качество обучаемых функций. Архитектор **проверяет и фиксирует решения**, но не «чинит» код без отдельного разрешения. 

**Цель:** объяснить, что именно из `local_wiki_planning` реально давало пользу пользователю и почему current RP01 не достигает того же качества; замкнуть путь `учебные данные → преобразование DPO/Unit → изменённые веса или память → checkpoint → loaded consumer → обычный ответ → независимый оценщик`.

**Полученные входы:** 
1. [Эталон и честные historical receipts](01-BASELINE-RECEIPTS-AND-CLAIMS.md), exact branch `local_wiki_planning@219d7ba...` и current `codex/dpo-implementation@4945b8e...`.
2. [Расхождения G-W01..07](02-NEW-DIFFS-AND-DOMAIN-SCOPE.md) с line-level source links и уровнем доказательства.
3. [Определённые опыты E-W01..07](03-CAUSAL-MICRO-EXPERIMENTS.md), [fixture JSON](05-CROSS-STACK-FIXTURE.json), [выполненный статический readback](06-SOURCE-READBACK-RECEIPT.json) — **только source check, не .NET run**.
4. [RP01](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/), [вся родительская RAG библиотека](../../..), [матрица прежних результатов](../00-LANGUAGE-TRANSLATE-FIRST-PRIORITY-2026-10-19.md).
5. **READ ONLY** ранние выводы core agents: `docs/plans/replanning-possible-diffs/76-...`, `80-...`, `81-validation-program.md`, а также `35.ExperimentsV2/results`, `42.ProjectWikiDomain`, `43.LearningLocalWikiData`; **не терять ссылки по дереву**.

**Строгое управление выполнением:**
1. Re-read pinned gold/current exact commits before work. Build compact map of **what was really trained and served** for *each* unit; record comparable metric owners, train/val partitions, data source/seed, checkpoint hashes; link evidence to source line.
2. **Сначала E-W00**, по уже опубликованным [N49-01/N49-03](../49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md): выполнить current native B `RejectedType` vs old faithful A/C `Prompt/Chosen/Rejected`; печатать model-visible input cardinality и схлопывание train/heldout, отдельно DPO score и ordinary route, cold consumer. Source-equivalent projection peer уже сделан; не повторять под другим именем. Отсюда прямо измерять алгоритм обучения, а не только retrieval.\n3. Затем **E-W01**: проверить настоящий `ClientServerBusinessFlow` vs `WorkflowTrace` на одинаковой паре клиент+сервер. Использовать [набор](05-CROSS-STACK-FIXTURE.json) с gold labels как предварительными гипотезами; сначала independent judge/counterexamples. Только при реальной потере восстановить old branch pattern **в scratch**, сохранив current entity-impact improvement. Не переписывать автоматически existing training requirements.
4. **E-W02**: проверить current trained `semanticQualityClaimed=false` и actual ordinary consumer `ProjectWikiRequestRouterUnit`; записать, существует ли иной путь подключения trained snapshot. До независимого semantic gate не изменять publish flag. Если route не достигает Flow — искать подключение через интерфейс + с сохранением safety fallback.
5. **E-W03**: route `chosen/rejected` through actual DPO weights and ordinary planner, фиксировать изменения `PlanAsync` и ответа. Не путать `DpoMetricRunner.HeldOutAccuracy` с evaluator `ProjectWikiQueryPlanningTrainingRunner.QueryPlanAccuracy`; не выдавать unchanged golden archive за current successful run.
6. **E-W04**: провести точный donor replay или вернуть `DONOR_ARTIFACT_UNAVAILABLE`. Preserve positive results E09/E26/E43 в своих доменах и сравнить только через own matched controls.
7. Только после P0 — E-W05/E-W06; release notes и compatibility map; не тратить время на full Windows GitHub Actions до принятого local learning result.
8. Во всех измерениях: против leakage `split by project/source file/source family`; held-out gold sealed before training; independent semantic labels; SAME learner/recipient/seed, clock, data budget; `A` vs `B` при **одном** изменении; wrong-project/stale-source/abstention negative controls. At most small trial (~12–30 questions) first; then broaden to 50/50 if needed for native runner constraints.
9. Фиксировать статус `CODE_DIFF_VERIFIED`, `PROXY_ONLY`, `NATIVE_TESTED`, `QUALITY_IMPROVED`, `HYPOTHESIS_REJECTED`, `NOT_RUN` **по отдельности**. Архитектор делает review по источникам, независимый QA проверяет hidden deterministic fallback, exact run identity, correct domain scope.

**Выход исполнителя в рамках этого задания (ещё НЕ получен):**
- `results/E-W00/*`: actual `metricTrain/metricHeldout` cardinality, faithful-vs-type-only native DPO, independent routed quality, cold serving adoption.\n- `results/E-W01/*`: gold/current/candidate `intent + plan steps + evidence family + ordinary answer + blind rubric`.
- `results/E-W02/*`: training, publication, cold load and consumer route trace + negative.
- `results/E-W03/*`: matched DPO pair perturbation / deterministic plan / free QA split.
- `results/E-W04/*`: historical donor provenance, actual checkpoint/shas, cold-restored outputs or explicit unavailable.
- `results/E-W05/E-W06/*`: migration/calibration source-vs-native results; no unsupported causal claims.
- Итоговый русский отчёт: **«что работало у эталона», «что исчезло», «что не использовалось», «что стало лучше», «что осталось гипотезой»**; найденные новые гипотезы приоритизировать только после прямого сравнения с предыдущими 400+ записей.

## Примеры good / bad и edge cases

**Хорошо:** «В gold `ClientServerBusinessFlow` имел 15 evidence steps, в current отдельный route отсутствует; на 12 слепых задачах current retrieval недополучил X требуемых классов, shadow восстановление вернуло Y при неизменном судье — сохранён run ID». До исполнения X/Y **UNKNOWN**.

**Плохо:** «В старой ветке DPO accuracy 98%, поэтому current LoRA неверная и нужно увеличить к=128». Разные метрики и owners, не следует никакого причинного вывода.

**Плохо:** «Поставили `SemanticQualityClaimed=true`; теперь всё опубликовано». Это фальсификация safety gate, не обучение.

**Хорошо:** «На RU→EN E26 проверяем многозначный перевод по POS с перестановкой слов и новыми контекстами. C# AST-код не подключаем к переводу, пока нет предметного evidence».

**Edge cases для обязательной проверки:** неправильный проект, stale branch, два одноимённых метода, неверный SQL stored procedure, отсутствующее UI evidence, вопрос одновременно о UI/сервере и добавлении поля, отсутствие tokenizer compatibility, новый label которого не было в snapshot, несовместимое numeric enum, cold restart, несколько разных получателей и пустой граф, легальный source-provenance для DPO, POS/слово-порядок только релевантным языковым профилям.

## Независимый review после записи результатов

**Проверяющий архитектор не автор того же delta.** Проверяет:
- Источник с точным commit/path/line; нет цитаты на движущийся HEAD там, где необходим pinned proof.
- Claim/область домена доказана именно выбранным recipient и ordinary Flow, а не historical scorer или synthetic fixture.
- `current minus gold` не скрыл улучшение current cross-stack entity impact.
- P0 fixed именно поведение, а не подогнан тест/rubric; негативные случаи не ухудшились.
- `local_wiki_planning` имеет в отчёте первенство; E09/E26/E43 — **параллельные узкие позитивные donors**, не считаются восстановлением wiki.
- Любой прокси и каждый неисполненный native опыт чётко помечены; нет `PASSED` по одному source-only сравнению.
- Изменены только разрешённые плановые документы; protected core path untouched.

**Итог review:** `ACCEPTED` только для опубликованных документарных code-diff/source claims; `LEARNING_QUALITY_PENDING` до повторного native experiment. Любая новая локальная гипотеза снабжена direct source, contrast against older agents, recipient, домен, минимум одним falsifier и маленькой реализацией.
