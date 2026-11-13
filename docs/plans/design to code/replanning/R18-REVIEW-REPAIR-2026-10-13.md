# R18 review/repair — конкретные дефекты и исправления

**13.10.2026.** Scope: исправление действующего R18 и его model prompt, публикация planning в `lubochka/xiigen-planning:llm_plan3`. Это fresh editorial source/code review, не execution A–D, не 25 research runs, не independent Astra invocation и не product patch. Авторский review count не выдаётся за независимый. `independent_evidence_count=0; subagent_id_or_NONE=NONE; learning=NOT_MEASURED`.

## 1. Почему требовалась правка действующих файлов

В R18 уже были полезные подробные A–D и реальный найденный TextGenerator. Перезапускать R01–R17 или сочинять новый trainer было бы потерей результата. Ошибки находились на границах исполнения: all-gates запрет требовал будущие наблюдения заранее; accepted-only G1 терял предусмотренную кодом условную ветку; общий service/kernel не фиксировал направление зависимостей и входную семантику. Исправлена сама инструкция, а не только статусный отчёт.

| ID | Проверенная проблема / граница вывода | Исправление в текущем work order | Контрпример, который теперь обрабатывается |
|---|---|---|---|
| R18R-01 | Old §4 D: «Не запускай никакую модель, пока G1–G6 не закрыты actual receipts»; G5 требует real read-only invocation и cold-load evidence | §6 разделяет preconditions и observations, G5-pre/G5-post, восемь causal stages | Можно разрешённо загрузить parent и проверить ordinary path до получения trial/cold-adoption результата, не разрешая optimizer |
| R18R-02 | G3 сводил actual models/request identities в предварительный допуск calls | Grant/expected actual model/profile до call; observed request IDs/bytes после, assessed pair до mutation | Нет требования угадать response ID, нет вызова без grant |
| R18R-03 | G1/arms требовали accepted checkpoint; actual StartService имеет clean-missing authorised cold-start branch | §3.3, §5, G1 сохраняют conditional initial baseline с отдельной identity и snapshot | Clean missing с applicable authority не вечный deadlock; corrupt/unavailable не обходит отказ; S04 может запретить конкретную ветку |
| R18R-04 | Old §3.4 «assessment uses same service/kernel» не выбирало layer; 26 уже ссылается на14 | §3.4 выбирает shared read-only facade в14→09, wrapper26→14, assessment14→facade14 | Не создаётся14→26 project cycle и не появляется второй generator |
| R18R-05 | Old ordinary CodeDocumentationRequest→generation не задавал x renderer; actual assessment передаёт exact OwnerInput | §3.1 owner-input-verbatim-v1 + proposed explicit ordinary input и versioned structured adapter boundary | Нет разных скрытых prompts при одинаковом названии kernel; source/model context не считается metadata conditioning |
| R18R-06 | Old m=0 wording могло читаться как runtime гарантия от одинаковых weights; code calls policy training:true/reference:false | §3.2 требует mode/RNG evidence; m0 не semantic rejection, freeze/recipient остаются | Stochastic mode differences не объявляются automatically invalid pair/guaranteed numerical equality |
| R18R-07 | Planning COMMON запрещает core edits, а future children говорят «реализуй» без effective mode compilation | §0/§4 и active prompt явно разделяют documentary/product/read-only/training modes | Worker не получает взаимно исключающие permissions и не наследует training grant от GitHub write |
| R18R-08 | Есть риск вернуть узкий E1 как завершение mapping/kNN/LoRA/four-role программы | §7 даёт механизм→конкретный state/signal/consumer experiment→frozen controls/falsifier→R19–R25 | k>1 first-hit, shape-only LoRA или solver-only gain не закрывают архитектуру |

R18R-04/05 — **design ambiguities**, не утверждения о существующем runtime defect/готовом новом сервисе. Cold-start branch доказана как code capability, не как действующее разрешение конкретного S04. Никакая actual efficacy/PC экономия по этому review не установлена.

## 2. Что намеренно сохранено

Segmentation/BPE и immutable loss masks, exact Chinese both-side authorship и separate CODEX/Astra selection/judgment, response-only conditional sum и соответствующий backward, public average scorer, frozen VectorSignal/raw-versus-optimized metrics, actual LoRA keys/ref independence, существующий trainer/publication/recovery, one-pair/no-repeat, EOS qualification/length outcomes, isolated trial до promotion, cold ordinary use после verdict, associations/typed OutputRefs, native trusted root, property-specific R17 evidence и no coverage substitution.

R12-ARCH-2 E1 conditional selection принимается как integrated design, а не readiness. R13 REPAIR_REQUIRED и R15 independent NOT_PERFORMED сохраняются. Previous research incompleteness не закрывается редактором. Обзор WorkLeft/RAG выполнен адресно для R18; full recursive corpus traversal не заявлен. Исходные prompts/WorkLeft и результаты other workers не изменены.

Предыдущий main file архивируется в том же каталоге как `R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.v1.md` с тем же blob `af505592907a43dc2022e22854e2b71f23a7e276`, поэтому относительные ссылки и bytes сохранены. Existing results/R18 остаются историческими carriers. Текущие решения исполнения находятся в активном main и run-prompts/R18, не в двух конкурирующих очередях.

## 3. Evidence ledger нового чтения

K=`lubochka/llm_mvp_core@3c7f5f8c7809936e25579603c96ea6df8598fc40` (branch ref проверен). Planning refs читались live с собственными blob identities; перед подготовкой publication P=`0115dd8538657fb74e5fd26dd95ea8a2e2cdd146`. Изменение unrelated planning HEAD не переносит source receipts задним числом.

| Источник | Blob / реально прочитанный scope | Использование |
|---|---|---|
| Original active R18 work order | `af505592907a43dc2022e22854e2b71f23a7e276`; всё тело адресными диапазонами | Точная baseline, unique A–D clauses, defects R01–R07 |
| Original run-prompts/R18 | `3cd450a10e62610971310845fa10590007695d3a`; всё тело, fresh header recheck на P | Governing packet, late FS additions |
| results/R18/FINDINGS | `4d5aa9963ccd4c48cc3b35c069ee5dda12b4db79`; всё | Preserve F18-01…08, no status upgrade |
| results/R12/DECISIONS | `edfeeb3280927c187d60c69c669cc643091aa394`; всё | R12-ARCH-2 integrated conditional E1, whole-system scope |
| results/R13/FINDINGS | `b5a5c809276dab2bb0471006370a5d4b825380be`; 1–105 | Current repair override, causal dimensions, не filled actual profile |
| results/R15/DECISIONS | `8a1ea9033ba41fed1ea5f2c86530dc18f276591e`; всё | Независимость, exact candidate, preserved algorithm obligations |
| results/R10/FINDINGS | `b53086a411aecdbe57f0567198e7faba0675f350`; 1–100 | Partial join/whole-program, bounded sibling results |
| results/R11/DECISIONS | `4419992c6405954f399d5a94f40a7172937916e8`; 1–110 | Causal first-loop order, no universal Windows gate |
| D/original task.md | `10b9c752ec740d439b46e0730f4cde2562ebe033`; 1–115 | Four original roles/async combinations; provider history not current permission |
| previous rounds thinking results.md | `df1fb01c3375c72c69c61ab9af20fc7bcf6bd8e5`; returned opening and120–205 | Repeated S04 narrowing, source-family evaluation, fixed criteria, bounded rework; не whole long-file audit |
| AGENTS/COMMON/PARALLEL/R17 policy | Actual retrieved governing sections: AGENTS whole; COMMON opening/current §0–3; PARALLEL §1–4 opening; R17 opening/§1–3 | Mode scope, semantic join, state/native risks. Truncated later tails не заявлены прочитанными |
| WorkLeft-RAG-00→WorkLeft-02 | Current retrieved opening/active clauses | Direct RAG→primary routing, role/no-repeat/NO_TRAINING_NEEDED, existing Student |
| K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs | `cb8744fcbbcff99821852e789dc92163976c0ad7`;2100–2225,2250–EOF | Full-state ref checks, exact BuildBatch/Compact defect |
| K/11.DPO/DpoTrainer.cs | `9c358978075db8c22fe0125eaea9e89e831d5635`;90–245,560–720 | Read-only vs mutation capability, loss/weighting/forward modes/public scorer |
| K/09.Inference/TextGenerator.cs | `544b58f130ac065f3462e3f452f0eedb55164081`; whole | Actual kernel/prompt/cancellation/context/stop |
| K/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs | `832bf00e4862ffc0a3cf2f98baa33b689d0c2fe6`;1–165,175–300,430–565 | Loaded model passport, actual generator, exact OwnerInput/corpus boundary |
| K/26.TrainableRouter/TrainableRouter.csproj | `4b2ae26801058fe98a2bca4624e2dd886dd91ea0`; whole | Dependency26→14 |
| K/14.ContinuousLearning/ContinuousLearning.csproj | `0ceb27cb3cb1532e8021792aee7895efb8da4628`; whole | Existing14→09/03/21; no14→26 |
| K/26.TrainableRouter/Training/CodeDocumentationRoutingFlow.cs | `1a115ed7811b78e5dbbcbe3708a0cb26fcd7d8a1`;1–135 | Routing/source/version/transit contract, not Student text |
| K/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs | `354bf8f72a2362349057e269378fa777204f5374`;1–265 | Accepted load, exact clean-missing authorised cold start, failure dispositions |
| K/03.TransformerModel/LlmModel.cs | `f7bf15cdfb1ebcceb2f6ddecd8ca1bbde7f64665`;1–145 | Actual base keys/adapter slots; not complete runtime key inspection |

Other code anchors inherited from old R18 remain **ATTRIBUTED source/code findings** with old SOURCES.csv, not new full-file read. No code was edited; future code writer must perform full affected file/caller review.

**Applied skill excerpts in K/docs/skills:** trainable-architecture-decision1–160 (`81e96fb8981d4eeb4a3f25f57001e74c453ffdbd`); code-examination1–140 (`5f6a9eb87686dfc67ef2eda0086c43e7dce47184`); phase-planning1–115 (`27ba212c829148158511c7a5a69bc418abfc276c`); plan-review1–135 (`b99f4ce280f48d0242a35f2ba421a2dc1996399f`); prompt-patch-authoring1–160 (`063fe9ca26b2a7921279dd803db91ac4e2551c2c`); human-readable-architecture-audit1–140 (`ee70e5f46472b6203382d0b393c72c1c79f96f70`); output-readability-gate1–115 (`ea3dd31450e3ed14f165b421b8c6b8a2471c3b28`). Full skill-library load/30/50 simulations не заявлены. Actual task-specific application: source/caller context, solved architecture, complete child prompt, scoped permissions, exact-candidate review, readable Russian and honest evidence.

Primary mathematical references were opened as HTML: https://arxiv.org/html/2305.18290v3 (§4 objective) и https://arxiv.org/html/2106.09685v2 (§4 low-rank adaptation). Они не свидетельства XIIGEN learning gain.

## 4. Independent verification work order — подготовлен, НЕ выполнен

**CURRENT TASK:** независимо проверить exact опубликованный R18-v2 candidate как implementation/examination design. **NOT THE TASK:** менять candidate, реализовывать core, выполнять тренировки, повторять все R01–R17 или выдавать coverage/Windows зелёный статус за learning.

**Invocation:** requested CODEX/GPT-6 Astra UltraCode согласно user/current policy. Dispatcher записывает фактически вызванные model/effort/session ID и candidate commit/tree/blob identities; отсутствие такого вызова остаётся NOT_PERFORMED. Нельзя назвать автора этой правки отдельным reviewer или перенести PASS со старого SHA.

**Контекст/skills:** main v2 целиком, active R18 prompt, эта таблица findings, original four-role paragraph, R12-C/F, current R13 override, R15 independence, R17 property matrix; trainable-architecture-decision, code-examination, plan-review, human-readable-architecture-audit/output-readability excerpts. RAG: WorkLeft-RAG-00→WorkLeft-02 current learning contract; K methods из ledger по каждой consequential claim. Непрочитанное отметить, не заменять full trace названием файла.

**Задание:** (1) вручную проиграть stage DAG от missing/accepted baseline до keep/promote/cold adoption; найти любой future-evidence prerequisite своего effect; (2) проследить x/response target boundaries и gradient signs, compare policy modes/storage/keys; (3) проверить project reference direction и proposed facade против actual code; (4) пройти законный negative/TIE/overflow/unknown-update scenario без повторного optimizer; (5) проверить причинный E1 contrast, независимость source families/criteria и full PC costs; (6) убедиться, что §7 не выдаёт E1 за domain/kNN/Flow/Graph learning и конкретные owners/experiments сохранены; (7) проверить contradictory instructions в compiled A–D и unresolved actual runtime fields; (8) дать disposition каждого R18R-01…08 и unique baseline requirement.

**Хороший результат:** конкретный PASS/NEEDS_FIX на reviewed design scope с counterexample, exact source span, требуемой bounded correction и неизменённым статусом неподтверждённого runtime. **Плохой:** только найденные keywords, self-awarded independent PASS, число прочитанных файлов, новая обязательная test campaign. **Края:** output ref association mismatch, wrong-domain same-shape LoRA, actual cold-start denied by S04, stochastic forward inequality, exhausted budget, mutable source after judgment, concurrent source/accepted-state update.

**Allowed actions:** read/search/direct diff/source verification в разрешённом scope; return-only. **Forbidden:** edits, trainer/model calls, weights/data access beyond grant, paid generation, Actions/build/tests, new infrastructure. **Return packet:** actual reviewer/candidate/loaded sources, per-finding disposition, requirement→algorithm→consumer→falsifier, substantive objections, unobserved bindings, scope-qualified verdict. Один review плюс один bounded correction/recheck при необходимости; actual calls считать честно. Parent интегрирует исправления и новая semantic diff требует соответствующего exact-candidate review, без бесконечного ритуального повторения неизменённых sections.

## 5. Publication/claim boundary

Назначены только четыре planning paths: active main, active run-prompts/R18, этот review carrier и byte-identical v1 archive. Публикация должна сохранять latest parent tree, без force-push и без изменения results/R01–R25/core. После commit проверить remote branch/candidate и published text/blobs. Само наличие этой инструкции не является readback receipt: фактический результат GitHub publication сообщается после tool response.

Ремонт делает последовательность действий непротиворечивее, но не предоставляет actual checkpoint/grants/keys/panel thresholds/hardware или независимый verdict. Эти поля имеют определённые источники/owners/stage boundaries в §6 main. Нет фактически измеренных time/accuracy/PC gains и нет заявленных выполненных model workers.
