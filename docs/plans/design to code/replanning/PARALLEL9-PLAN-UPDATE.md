# Опубликованное дополнение: девять независимых исследований и полный hybrid

**13 октября 2026 · PARALLEL9 / FULL-HYBRID.** Это результат редактирования плана и prompts, не выполненные R01–R25, не обучение и не независимая Astra экспертиза. Первоначальное узкое описание R07 в этой заметке уточнено: весь гибридный механизм обязателен; два connections — только часть исследования.

## Куда входить

Основной [PLAN-25-RUNS](PLAN-25-RUNS.md) сохраняет 25 назначений. [START-NEXT-RUN](START-NEXT-RUN.md) собирает одно полное поручение модели, а не отдаёт пользователю набор противоречащих ссылок. [PARALLEL-R01-R03-JOIN](PARALLEL-R01-R03-JOIN.md) имеет историческое имя, но действующий состав — девять работников и R10/J03 как адресное объединение.

Полные новые развёртки: [R07 — весь hybrid, A–I](run-prompts/R07-PARALLEL9.md), [R08 — домены, DPO и examination](run-prompts/R08-PARALLEL9.md), [R09 — процесс, ПК и цена учебного сигнала](run-prompts/R09-PARALLEL9.md). Каждый содержит общий контекст, прямые sources/RAG, skills, detailed actions, output, хорошие/плохие/крайние примеры, документную приёмку и будущий learning experiment. START включает уникальные действующие условия прежних detailed books, не вторую очередь. R07 согласован с [полным repair work order](R07-REPAIR-WORK-ORDER.md).

Действующие подробные карточки остальных запусков остаются в MODEL-RUN-PACKETS-CP5-01-10/11-25, ROUND-DELTA-RESULTS6 и PARALLEL9-MODEL-PACKETS; dispatcher загружает только назначенную карточку и необходимые excerpts. Одна таблица ниже не заменяет эти prompts.

## Девять самостоятельных стартов

| Run | Предмет и прямой вход без sibling DONE |
|---|---|
| R01 | Вся карта roots/слайсов/исходных задач: trees, WorkLeft INDEX/MASTER, original/task/goal |
| R02 | Требования/owner decisions: original prompts, DPO algorithm, decision sources, current RUN |
| R03 | RAG MD/NDJSON → primary, правильные roots и semantic receipts |
| R04 | Domain-aware representation, typed many→many: S00–S03, contracts и actual consumers |
| R05 | Exact pair → tokens/objective → существующий trainer/LoRA → ordinary consumer: S04–S06, plan07/RUN, current code |
| R06 | Mapping, dynamic kNN/neighbor use и graph authority: S07–S10, domain/search/router/graph и primary decisions |
| **R07** | **Полный causal hybrid: original task/DPO algorithm, обязательное прямое чтение WorkLeft MECH-01/RESEARCH-03, relevant RAG и code owners по A–I** |
| R08 | Laws/data/preference/splits: original domains, WorkLeft research/teaching, primary laws и representation/pair consumers |
| R09 | Process/resource/learning-signal cost: WorkLeft batching/DEC/MATRIX/REPORT, существующие workflow/run evidence и functional examination |

Работающий агент сохраняет checkpoint, IDs, visited set и прочитанные spans. Новая редакция не reset. Каждый пишет свои пять carriers в results/Rxx; чужие результаты/статусы не изменяются. Missing carrier после проверки tree/alias — RESULT_NOT_OBSERVED, не NOT_STARTED/DONE. Новое sibling evidence уточняет зависимые claims, а не заставляет всех повторять MASTER.

R10 принимает девять отдельных snapshots/statuses/limits. Прежний J03 входит в R10 и переиспользуется как подтверждённый subset. Частичный join, достаточный first-lesson subset и полнота всей программы — разные verdict. Непрочитанное получает конкретный question/span/owner; R10 не выполняет за девятерых их хвосты.

## Что обязан восстановить R07

A — первичные решения и карта всего relevant source surface; B — Unit/Flow/Orchestrator/Graph и many→many; C — laws→tokens/vectors; D — dynamic kNN/BM25/index/arbiter; E — meaningful pair/objective/semantic recipient; F — LoRA/projection/checkpoint/ordinary adoption; G — causal deterministic/learned interactions; H — PC/feedback/continued learning; I — независимые bulk examinations и интеграция.

Каждая A–I имеет собственные question/skills/context/actions/output/good-bad-edge/acceptance/handoff. Это подзадачи R07, не девять вымышленных независимых calls. При фактическом делегировании передаётся полный относящийся common и карточка; parent принимает return-only результат. Дополнительный вызов имеет именованный остаток, причину и отдельный учёт.

Четыре роли сохраняются: Unit — domain/input-vector/output schemas и typed many→many associations; Flow — отношения и correction; Orchestrator — async combinations с ценой всех branches; Graph — higher-order pattern relationships/transfer. Actual ILogical aliases сверяются поведением. Train остаётся у ILearningMachine, Graph=L5 specialization; не четыре новых trainer. Обучаемые веса могут вычисляться детерминированно; случайность сама не доказательство обучения.

## Все 25 назначений сохранены

| Этап | Назначения и результат |
|---|---|
| **10 сбор информации/гипотез** | R01–R09 выше; R10 объединяет evidence и гипотезы без фиктивного завершения соседей. |
| **5 анализа** | R11 first-lesson/full-delivery dependencies; R12 выбранные architecture/objective/owner/index решения; R13 первый заполненный distinguishing experiment; R14 новая whole-program очередь; R15 actual independent review либо NOT_PERFORMED. |
| **5 применения** | R16 requirements patches; R17 learning/PC/host acceptance; R18 ближайший полный implementation packet; R19 согласованные entrypoints; R20 полные model/child packets и девять documentary cold-start walkthrough. |
| **5 дополнения** | R21 curriculum/Builder; R22 algorithm/connection experiments; R23 PC и feedback lifecycle; R24 ordinary DTC/IDE/adoption/portability; R25 independent final handoff review. |

Не добавлены автоматические 26-й run, новые20/30/50 reviews или девять GPU jobs. Индивидуальные задания/проверки могут быть разделены при реальном ограничении контекста, но actual calls считаются отдельно, а обязанность не теряется.

## Ближайший evidence-backed путь к уроку

По опубликованному [R05 CP2](results/R05/FINDINGS.md) существующий trainer/backward/AdamW/publication pipeline уже есть. Два code findings: whitespace/480-character Compact до Encode при паспорте полной пары; mean log-probability prompt+ответ вместо заданного conditional whole-response objective. Это атрибутированное исследование, не выполненная этой редакцией правка и не измеренный gain. Source refs и текущая актуальность проверяются назначенным исполнителем.

Плановый путь: current R05 + относящиеся R04/R07 → R11/R12 input/objective/recipient decisions → R13 сопоставимый recipe с R08/R09 controls → R18 future implementation order. R18 имеет две части: сохранить exact assessed meaning до токенов; реализовать выбранный response mask/likelihood/backward у existing owners. Public average inference scorer не менять всем consumers молча. Accepted state должен достигать ordinary request, не только trial evaluator.

Не назначать без causal evidence prerequisite: новый encoder/HNSW/trainer, все доменные кампании, полный UI/IDE или unrelated Windows qualification. R04 encoder E01 не prerequisite learning E02. R06 checkpoint C отделяет documentation lesson от graph-policy lesson: неиспользуемый Graph/L3 не задерживает первый, но solver gain не выдаётся за graph/selector learning. S06.08B/S06.09C уже дают кампании новых уроков; S04 one-update-total не вечный запрет continued learning.

## Чем проверять замысел

Для каждого механизма: источник/ожидание → fixed contribution → learned owner/state/objective → ordinary consumer → controlled intervention → semantic outcome/falsifier → full cost/retention. Общего процента coverage вместо этой цепи нет.

Domain mapping измеряется отдельно от solver; oracle/predicted domain сравниваются на тех же задачах. Representation сохраняет domain laws и semantic versions; shape equality не compatibility. Dynamic kNN разделяется на pool/metric/k/radius/index/reranker/consumer; candidate function — гипотеза, не заранее optimum. LoRA parameter delta, tokenizer/codebook, retrieval vector и output projection — разные объекты. Frozen Units/changed Flow и обратный, primitive/higher-order compositions и same-memory no-update/update отделяют возможные причины эффекта.

DPO: общий immutable context, содержательные positive/negative, TIE/BOTH_BAD/UNJUDGEABLE без forced preference. Chinese model автор обеих final sides; CODEX Astra/ultra выбирает разрешённый урок и отдельно оценивает exact pair. Student response — context, не обязательный rejected. Actual invocation/provenance не выдумываются. Pair validity, optimizer admission, update и utility различаются; correct deterministic answer не negative по происхождению.

Bulk — окно новых последовательных уроков. Same-panel/rubric before/after, source-family splits, rare/unknown/mixed domains, retention и новый structural holdout. Повторно используемая operational panel не untouched certification; её ответы не передаются teacher. До данных фиксируются meaningful effect, tolerated regression, uncertainty/budget/stopping. IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE не заменяются одним judge PASS. One-pair/no-repeat/full-state accept-or-rollback сохраняются; неизвестный исход update не разрешает второй.

PC comparison учитывает весь learner, adapters/optimizer/activations, indexes/graph/cache, evaluator/snapshots/reindex и стоимость всех attempts/branches/remote teacher. Equal-quality cost и equal-budget quality различаются. Hardware из истории — provisional. User feedback оценивается до допуска нового урока, не автоматически истина.

## Границы этой публикации

Созданы/обновлены planning prompts и маршруты исполнения. Research results R01–R09 не перезаписывались; product code/tests/workflows/weights/datasets не менялись; training/Windows/Actions не запускались. Full distributed source examination остаётся задачей назначенных Rxx. Эффективность/PC-fit — NOT_MEASURED; отдельная Astra экспертиза — NOT_PERFORMED. GitHub commits/readback подтверждают публикацию документов, не качество обученной системы.
