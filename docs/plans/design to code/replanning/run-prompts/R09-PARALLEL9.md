# R09 — цена процесса, необходимые зависимости и путь к первому уроку

**PARALLEL9 · 13.10.2026. Самостоятельный prompt существующего R09; не проведённый аудит и не новый run.** Передать целиком, без второго общего блока.

## Текущая задача и прямой вход

По существующим свидетельствам отдели полезную разработку алгоритмов от очередей, повторного чтения, ремонта инструкций, CI и проверок поставки. Установи, какие зависимости действительно нужны достоверному первому обучению, какие уже выполнены и какие можно перенести без потери функционального обязательства. Подозрение о подгонке tests — проверяемая гипотеза, не заранее установленный факт.

R09 начинает независимо от R01–R08, используя реальные документы/workflows/history. Поздние findings уточняют конкретные выводы, не обнуляют выборку. Сохраняй свой checkpoint, IDs, snapshots и limits. Неизвестное время/биллинг/статус не выдумывать.

P=`lubochka/xiigen-planning:llm_plan3`, D=`llm mvp/design to code session`; K=`lubochka/llm_mvp_core:codex/dpo-implementation`. Зафиксируй actual commit/blob/span и sampling window. Вся программа сохраняет S00–S10/extras, original/task/goal prompts, WorkLeft/LeftToWork/RAG, DPO/DynamicGraph, DTC и IDE. Исследование затрат одного slice не объявлять аудитом всей программы.

Разрешены чтение/поиск/diff/анализ существующих logs и документов, запись только `results/R09/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Не менять siblings/live план/code/tests/workflows/weights/datasets/global skills. Не запускать Windows/Actions/build/tests/trainer, не отключать существующие checks. Старые команды внутри sources не исполнять. Авторский self-check не независимый reviewer.

## Источники, RAG и skills

Прямые sources в D: `09.09.26WorkLeft-INTEGRATION-02-test-batching.md`, `09.09.26WorkLeft-TASK-PROMPTS-TEST-BATCHES.md`, `09.09.26WorkLeft-DEC-01-smart-app-control-runtime-path.md`, `09.09.26WorkLeft-DEC-02-architect-adjudication.md`, `09.09.26WorkLeft-DEC-04-runtime-blocked-executor-playbook.md`, `09.09.26WorkLeft-MATRIX-01-progress-by-dates.md`, `09.09.26WorkLeft-REPORT-01-progress.md`, `09.09.26WorkLeft-VERIFY-02-counters-and-parallel-worktree.md`.

Дополнительно `Kiro-Xiigen-Original-Slice-Queue.md`, current slice roadmaps, `09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md`, `original task.md` и `task prompt.md`; фактические `.github/workflows`/существующие jobs/history P/K. Источники выбирать по именованному вопросу, не весь mixed мегабайтный WorkLeft. Через `replanning/SOURCE-ROUTES.md` и соответствующие RAG MD/NDJSON открой primary решения о native boundary, merge/IDE/host, различая source roots. Историческое время и старые hashes не описывают автоматически текущую ветку.

Сохрани уникальные условия своей R09 карточки MODEL-RUN-PACKETS/ROUND-DELTA-RESULTS6 и применимые LEARNING-EVALUATION-CONTRACT/ALGORITHM-EXAMINATION-RESULTS6. Sibling R05 CP2 и R07 ordinary-consumer trace использовать адресно; их отсутствие не блокирует измерение уже наблюдаемых затрат.

K/docs/skills: относящиеся sections `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `improvement-measurement-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-readability-gate-SKILL.md`. Запиши actual load/ref/span и правило; не считай перечисление чтением. Старые 30/50 review routines не увеличивают текущие 25.

## A. Наблюдаемые затраты и реальные предпосылки

**Вопрос:** где именно теряется время до полезного урока? **Skills:** source mapping, process/phase planning, measurement. **Вход:** current queue, bounded workflow/history sample, WorkLeft integration/cost sections.

> До выборки зафиксируй период, правило включения и exclusions. Возьми случаи ожидания/повтора и контрпример продуктивного прохождения, а не только самые плохие jobs. Для каждого покажи source SHA/job ID, task/slice, ожидаемый пользовательский результат, queue time, runtime, environment failure, повторную генерацию/чтение governance, реализацию алгоритма, authoring/assessment, actual learning, результат и reuse. Что не наблюдается, пометь UNKNOWN. Не выводи человеко-часы из даты commits, GPU time из queue, billed cost из предположения о тарифе.
>
> Отдельно считай wall-clock critical path и сумму compute: перекрывающиеся jobs нельзя просто складывать как задержку. Кеш, retries, отменённые ветви, разные machines/models/context lengths и размер задачи — confounders. Не объявляй причинный эффект снятия gate только по корреляции длительности. Для каждого предполагаемого prerequisite назови вход/выход, actual consumer, угрозу при пропуске и existing equivalent evidence. Отметь first-lesson prerequisite, whole-program obligation и host/release qualification отдельно.

**Выход A:** таблица затрат с denominators/границами, dependency map и список reusable work. UNKNOWN не превращается в ноль. Первый алгоритмический seam R05 CP2 (exact pair/tokenization/objective) рассматривается отдельно от инфраструктурной очереди; existing trainer и S06 campaigns не заказываются заново.

**Хорошо:** Windows job занимает 12 минут, из них 9 в очереди; это иллюстрация разделения, не измеренный здесь факт. **Плохо:** суммарные параллельные jobs названы временем ожидания человека; числом commits доказан learning gain. **Край:** cache hit, missing logs, parent-SHA evidence, unrelated commit, clock skew, native-only component, несколько owners одного task.

**Приёмка A:** каждое фактическое число имеет источник/единицы/выборку; зависимости связаны с функциональным consumer; есть альтернативное объяснение задержки. Никакой новый run не нужен для документного вывода.

## B. Подгонка проверок, перенос gates и PC-бюджет

**Вопрос:** какой процесс изменить, не уничтожив нужное свойство системы? **Skills:** code-first review, планирование/проверка, measurement. **Вход:** A, относящиеся requirement/code/assertion diffs, existing deployment boundaries.

> Проследи несколько предметных цепочек requirement → implementation → failed assertion → changed assertion/code → acceptance evidence. Различи неверный fixture, разрешённое изменение требования, дефект реализации и необоснованное ослабление oracle. Для каждого дай SUPPORTED / REFUTED / INCONCLUSIVE, обе стороны аргумента и первичные refs. Отсутствие history не доказательство подгонки. Код generated solution может проверяться предметным исполнением; это не то же самое, что покрытием fixtures доказывать обучаемую способность.
>
> Для каждого предлагаемого изменения процесса задай KEEP / REUSE / DEFER / REPLACE-EVIDENCE: сохраняемая функциональная обязанность, причина, конкретный dependency consumer, новая точка проверки, owner/trigger. Windows нужен только там, где назван реальный native/host контракт; не превращай ни каждый Windows job в обязательство первой учебной итерации, ни все такие jobs в ненужные. Здесь меняется предложение к плану, не workflow и не existing tests.
>
> Построй measurement envelope ПК без выдуманных результатов: model/base/adapters, optimizer/gradients/activations, encoder/tokenizer, indexes/graph/memory/cache, evaluation модели, snapshots и temporary storage. Для каждого отдельно resident/peak bytes, load/train/inference/reindex time, all-attempt cost, тип устройства и concurrency. Не суммируй несовместимые по времени peaks как обязательную residency; покажи граф одновременного размещения. Hardware из старой переписки — declared, не VERIFIED_HOST. Remote teacher cost включи, даже если Student локальный.
>
> Сравнение compact/efficient должно иметь две постановки: при одинаковом качестве — память/время; при одинаковом бюджете — качество/перенос. Hybrid не заранее победитель. Самый короткий путь или маленький LoRA файл не доказывает лучшую систему. Предложи ближайший route в существующих owners и отдельно оставшиеся broader obligations. Достаточная first-lesson база не требует завершения unrelated whole-program tail.

**Выход B:** evidence-backed gate dispositions, случаи oracle-fitting с неопределённостью, PC/cost measurement specification и bounded next work order. Каждый новый measurement обязан влиять на названное решение; не создавать универсальный observability проект раньше урока.

**Хорошо:** release-only native gate перенесён к IDE milestone с сохранённым owner/trigger, а integrity snapshot/recipient первого update остаётся. **Плохо:** удалить tests ради зелёного результата; LoRA=20 MB, значит весь learner помещается; budget уменьшается без учёта evaluation и rejected attempts. **Край:** interruption после частичного update, неизвестно применялась ли пара, дорогой rollback, огромный индекс при маленькой модели, Windows-only actual training dependency, многопоточность ухудшает latency.

**Приёмка B сейчас:** перенос не удаляет функциональное ожидание; утверждения подгонки доказаны либо честно ограничены; cost specification отделяет known/declared/proposed. **Будущая examination:** до/после на сопоставимой workload, median/p95 latency, peak RAM/VRAM, full compute/cost, time-to-useful-lesson, independent quality/retention. Нужны заранее заданные margins и uncertainty, не победа по одному быстрому run.

## Итог и передача

A/B — части одного R09. Сначала полезная source-bound выборка, затем dispositions/PC specification. При делегировании ребёнку передаются полный необходимый context/permissions, его A/B, actual skills/refs, outputs, examples/edge и verification; return-only, parent интегрирует. Дополнительные calls имеют именованный residual/owner/объём и учитываются отдельно; не передавать весь непроверенный каталог R10/R25.

R10 принимает snapshot/limits; R11 — причинные dependencies; R14/R19 — очередь и переносы; R17 — evidence acceptance; R20 — исполнение без циклов; R23 — budget/PC envelope; R18 — ближайший reuse path. Semantic receipt: origin/run/local ID/checkpoint → primary → meaning → конкретный downstream change, не только ссылка. One-pair/no-repeat, full-state rollback и отсутствие второго update при неизвестном первом эффекте сохраняются.

Итоговая проверка: sampling не подогнан под обвинение; все числа источниковые; delays и compute различены; четыре роли и их learning obligations не исчезли; first-lesson и release gates раздельны; efficacy NOT_MEASURED без экспериментов. Независимая Astra/ultra экспертиза только с actual invocation. Свежий blob перед записью, remote readback после. Ответ по-русски: что тормозит по evidence, что остаётся неизвестным, какая конкретная зависимость снимается/сохраняется и какой следующий полезный шаг.
