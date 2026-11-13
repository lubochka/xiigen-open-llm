# R17 — политика достаточных доказательств и условных Windows-проверок

**13.10.2026 | R17-REPAIR-2 | действующий planning acceptance contract | статус: DOCUMENTARY_POLICY_APPLIED; PRODUCT_LEARNING_NOT_MEASURED.**

**Решение.** Приёмка должна защищать свойство, а не количество прогонов. Ранний эксперимент с реальным обучением не ждёт нерелевантного Windows/native gate; обязательные проверки state, rights, native и delivery не отменяются. Это изменение требований и планирования; workflows, branch rules, код, Student, tests и веса этим R17 **не изменяются**. Полное поручение и criteria AC01–AC12: [run-prompts/R17.md](run-prompts/R17.md). A–F — части одного R17, не новая очередь.

**Параллельность и scope.** R17 может начинать одновременно с R01–R16; конкретный clause зависит от своего evidence, не от общего DONE R16. Доступные результаты сохраняются, не перезапускаются. Для будущего опыта перечислить его собственные unresolved R12/R13 bindings и applicable R15 review; общий статус раунда не заменяет причинную зависимость. Ни один E1–E4 experiment этой редакцией не объявлен READY или выполненным. Достаточный CodeDocumentation first-lesson packet не требует полного Graph/curriculum/всех ablations; отсутствующие собственные data/owner/objective/state/consumer/criteria/permission остаются препятствием. Independent reviewer NOT_PERFORMED нельзя заменить self-check.

**Совмещение правок.** Сохранены параллельно опубликованные ограничения R09 aggregates и различие local Windows/bootstrap versus GitHub Windows campaign. Новые уточнения касаются scoped evidence, взаимодействия механизмов, сравнимости bulks и durable no-repeat, не отменяют чужие source-backed corrections.

## 1. Факты и границы

- **R09 — ATTRIBUTED_AGGREGATES / RAW_SAMPLE_NOT_RECONSTRUCTED.** В `results/R09/FINDINGS.md` §2 опубликовано сообщение о 30 Actions runs (12–13 октября): Windows family — 13 runs, 368.3 минуты, median 38.1; focused family — 17 runs, 114.2 минуты, median 6.9. Эти числа сохранены только как идентификация исходного утверждения, **не как подтверждённое измерение или основание численной экономии**. Предыдущий completion repair не восстановил 30 исходных строк; параллельная поправка R17 обнаружила повторный перенос агрегатов без этого основания. Для принятия нужны run_id/attempt/head_sha/workflow/status/start/end, состав jobs и точная формула. Run elapsed, сумма job elapsed, queue wait и время человека различаются. Пока исходной выборки нет, отношения 3.2×/5.5× и прогнозы экономии из неё не использовать. R09 сохраняет собственные carriers; policy ограничивает потребление чисел.
- **Отдельное ограниченное наблюдение, не восстановление sample:** `COMPLETION-REPAIR-INTEGRATION-2026-10-13.md` §5 содержит три raw job rows одного существующего run, включая Windows-job 20 минут 53 секунды. Оно не подтверждает агрегаты 30 runs, не является новым запуском и не доказывает типичную цену. PsecPhysical в R09 — отдельное атрибутированное evidence native boundary, не статистическое доказательство кампании.
- **Core workflows, прочитанные предыдущим R17 на codex/dpo-implementation:** .github/workflows/s03-windows-verification.yml — push по .kiro/xiigen-slices/luna/s04/windows-verification-request.json и manual workflow_dispatch с candidate_sha/gate; windows-verification на windows-2022, 90 минут. .github/workflows/design-to-code-focused-verification.yml — push на DesignToCode-clean/codex/dpo-implementation/main, pull_request в main и dispatch; focused на ubuntu-latest, 30 минут. Policy markdown сама **не предотвращает** широкий push trigger. Это source-read прежнего R17, не повторный workflow audit автора настоящей поправки.
- При прежней проверке R17 branch protection обоих репозиториев ответил HTTP 403 Resource not accessible by integration. Сохранён статус **REQUIRED_STATUS_RULES_NOT_OBSERVED**, не «required checks отсутствуют» и не «GitHub read-only». Эта редакция не повторяла запросы защиты веток.
- Источники решений: R09/FINDINGS §§2–10 с указанными ограничениями, R11 causal DAG, R13/FINDINGS §§4–13 с действующим repair override, R15/DECISIONS, R16/DECISIONS, WorkLeft-02/INTEGRATION-01/INTEGRATION-02. S00–S10, DTC и IDE сохраняются. Gate переносится по причинной карте, а не неподтверждённой численной экономии. Текущий статус каждого Rxx читается из его собственного checkpoint.

## 2. Одна матрица по типу изменения

В каждой строке: **риск → доказательство → среда/частота → retry/stop → блокирует что**. Более строгий applicable product/host gate сохраняется до предметной разрешённой миграции. Это не новые test suites и не требование выполнить все строки на каждом bulk.

| Изменение | Что защищаем | Достаточное доказательство | Среда, момент, частота | Stop/блокировка |
|---|---|---|---|---|
| Markdown, planning, prompt, source-map | Смысл, полнота требований, ссылка и полномочия | Before/after exact text, source→requirement→consumer, рецензия, remote readback SHA, отсутствие конфликтующей инструкции | Документная проверка изменённой редакции; без product test/Windows | Только неприменённая/противоречивая плановая правка |
| Детерминированный parser, schema, law, safety, permissions | Типы, права, законы, отказ до side-effects | Code/contract trace + минимальное affected-invariant evidence; reuse existing focused/impacted evidence и negative boundary | Подходящая affected среда при будущей кодовой правке; не full Windows по умолчанию | Нарушенный invariant блокирует admission/execution независимо от ML |
| DPO case/teacher/pair | Законный урок, содержательная preference и exact payload | CODEX/Astra выбирает lesson; разрешённая Chinese model пишет обе final sides; CODEX/Astra отдельно оценивает exact pair/context; rights/provenance; preferred_side отдельно от pair_usable; смысл сохраняется до objective | На новой разрешённой pair до optimizer; material change требует нового assessment | INVALID/TIE/BOTH_BAD/UNJUDGEABLE не превращаются в forced preference |
| Optimizer/checkpoint/counters/snapshot/no-repeat | Правильная мутация и безопасное восстановление | Actual state/parameter owner, one new pair→one permitted application, durable journal, full applicable weights/adapter/optimizer/scheduler/RNG/counters/identities/token/index/graph snapshot, fault/reopen trace | После изменения state path; адресное recovery evidence, не произвольные 30 Windows runs | UNKNOWN → quarantine/reconcile до следующего update; rollback не стирает использованные пары |
| Unit/Student и DPO objective | Польза обычному consumer, не только loss | Frozen parent/challenger/comparable no-update, same panel/rubric, independent families, actual ordinary cold reload, per-domain/retention/uncertainty/time/full cost | Разрешённый опыт/bulk новых пар; Windows только при actual native dependency | IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE; promotion по заранее допустимому outcome и approval |
| Domain mapping/tokens/vectors/dynamic kNN | Домен, семантика и полезные реально используемые соседи | Oracle vs predicted domain; law-sensitive contrasts; eligibility/features/metric/k/radius/index/state/neighbor consumer; fixed vs learned, exact vs ANN where applicable | При изменении соответствующей representation/routing/retrieval; не все варианты на каждом уроке | Law violation blocks affected route; k или retrieval receipt не доказывают gain |
| Flow/Orchestrator/DynamicGraph | Коррекции, lawful async route и higher-order transfer | Separate role/state/credit/consumer, frozen lower roles where causal, feasible path baselines, all branch/cancel costs, unseen compositions, bounded interaction §4 | Scoped E3/E4/R22; не prerequisite несвязанного E1 | Unit success не закрывает остальные; best observed path не global optimum |
| LoRA/projection/tokenizer/index migration | Осмысленное source→target соответствие | Actual weight/adapter/projection owner, target signal, coordinate meanings/versions, downstream semantics, reindex/rollback plan | При изменении map/interoperability seam | LoRA не tokenizer; shape/bytes/hash/loss не semantic correctness |
| Compact PC/continual learning/feedback | Полезность на реальном ПК без скрытой цены | Whole RAM/VRAM/time/growth: base/reference/LoRA/optimizer/activations/encoder/index/graph/cache/snapshots/rebuild; teacher/judge/evaluator; failed attempts; feedback rights/novelty/conflicts | Material bulk/resource change; same measured hardware/task/quality envelope | PC NOT_MEASURED без профиля; budget exhaustion → stop/keep parent |
| Windows native/launcher/process/security | Свойство целевого host | Exact SHA, applicable Windows process/resources/Job/ACL/security/Smart App Control raw evidence, separate review where required | Native boundary change или native/delivery milestone; existing request/dispatch admission | Affected native execution/release не готовы; не blanket gate всех learning designs |
| S05 remote publication/final release | Разрешённая передача и интеграция | Rights/custody/exact artifact identity/approved destination, host delivery и actual required statuses | S05/release, отдельный host/remote gate | Missing permission/status блокирует публикацию; local S04 не S05 |

**Правило выбора.** Для каждой строки назвать causal edge от changed state/code до защищаемого свойства. Нет ребра — DEFERRED_TO_OWNER(milestone,trigger), не DELETED и не обязательный повтор. Suitable existing evidence переиспользуется с identity/limits. Hard invariants обязательны до effects. Полнота программы отслеживается по AC01–AC12/original requirements; отдельный bulk экзаменует changed capability плюс необходимые integrity/retention controls.

**Local Windows ≠ GitHub Windows campaign.** Ненативная математическая objective не доказывает переносимость действующего bootstrap. В сохранённом source evidence K=`3c7f5f8c7809936e25579603c96ea6df8598fc40`, `25.ModelServer/Program.cs` 83–88, repository-owned bootstrap закрепляет `C:\Projects\llm_mvp_core`. Не менять protected root и не придумывать Linux-ready entry ради ускорения. R18 устанавливает actual supported host/caller, затем определяет relevant existing native evidence; это не blanket admission нового Windows CI run. Настоящая редакция сохраняет это source-backed ограничение, не заявляя нового чтения указанного кода.

## 2a. Старый gate → новое evidence → owner/trigger

| Вредная трактовка | Замена | Владелец и момент |
|---|---|---|
| «Перед любым шагом полный Windows/S03» | Причинно необходимый native guard, candidate SHA и raw host result | Native owner при native change; S05/release для поставки |
| «Green/coverage/loss означает обучение» | Ordinary consumer + lawful parent/challenger/no-update + independent quality/retention | R13 scoped protocol → R18 learner owner → R22 опыт |
| «Изменились k/LoRA/file — польза доказана» | Actual learned signal/state/recipient и counterfactual downstream quality/time/cost | R12/R18/R22, E1/E2 по actual scope |
| «RED → до GREEN или поменять assertion» | Original requirement/invariant, root cause, bounded diagnostic hypothesis | Implementation state owner; native owner на native boundary; R23 budget |
| «Crash UNKNOWN значит update не было» | Full journal/snapshot/no-repeat lineage/resolved disposition | Trainer/state owner до любой следующей мутации |
| «Local S04 green разрешает remote» | Rights/custody/versioned release/native/actual status evidence | S05/release; ruleset owner устанавливает required contexts |
| «Авторское ревью = Astra PASS» | Actual отдельный reviewer и exact candidate evidence | R15/R25 applicable scope; иначе NOT_PERFORMED |
| «Каждый bulk проверяет все механизмы» | Affected mechanism + necessary integrity/retention; отдельные scheduled interaction/domain studies | R13/R21/R22 по named change/uncertainty |
| «R12/R13/R15 не DONE — никакая работа не начинается» | Конкретные prerequisites и review scope каждого claim/опыта | R17/R18/R19; независимые clauses продолжаются, реальные blockers не обходятся |

Это поручения существующим потребителям, не новые обязательные сессии/test campaigns.

## 3. Регистр доказательств для R18/R19/R22/R23

Для меняемой возможности сохранить: requirement/AC ID; source/ref/span; changed artifact/commit; ordinary caller/consumer; risk/native edge; law/admissibility; learned state/recipient/update owner; actual Teacher/assessor/evaluator identities; pair_id/exact seal и preferred_side/pair_usable; parent/challenger/no-update IDs; train/dev/untouched-final partitions; domain/output schemas; representation/encoder/vector/index/LoRA versions; hypothesis/falsifier; independent families/sampling; rubric до outcome; meaningful effect/regression tolerance/uncertainty; whole cost including failures; runtime/host; applicable status audit; grants/budget; outcome/promotion/rollback; recovery/consumed-pair disposition; separate reviewer/exact candidate.

Не придумывать universal 95%, p-value или panel size. Историческая 95% — продуктовая цель по owner-defined domain rubric, не coverage/confidence и не допуск первого урока. До runtime использовать valid owner decision либо явное DESIGN_PROPOSAL с binding. Незаполненный numeric policy = DESIGN_BINDING_REQUIRED этому опыту; не 0/95% и не запрет независимой документной правки.

## 4. Функциональная examination алгоритмов

**Данные.** Законные предметные families с независимыми источниками; train/dev/final разделены по source family/template lineage. Final exam нельзя использовать для DPO authoring, выбора уроков/tuning. Domain panel содержит релевантные valid/invalid/adversarial/mixed/unknown/law-sensitive strata; не требовать искусственно все типы от каждой family. Oracle/predicted domain измерять отдельно. Семь baseline subjects/extras сохраняются через passports/owners, не сводятся к CodeDocumentation.

**Локальные контрасты.** Freeze parent/data/candidates/rights/cost envelope; для локализации изменять причинный фактор: DPO transport/objective/recipient; representation; fixed-vs-learned kNN с real neighbor use; LoRA/projection; Unit; Flow при фиксированных Units; Orchestrator при фиксированных solvers; Graph при фиксированных lower roles. Связанное изменение не приписывать одному фактору.

**Взаимодействия.** Claim о полезности соединения требует bounded interaction comparison, не только single-factor victories. Для выбранной пары A/B — compatible frozen old/old, new/old, old/new, new/new на одной panel/budget; joint benefit и interaction различаются, super-additivity не обязательна. Incompatible crossed state не выдумывать: указать допустимый matched contrast и предел причинного вывода. D fixed-rule, L learned-choice, H hybrid, H-frozen/no-update сравниваются при одинаковых hard laws/rights/safety. Контроли НЕ разрешают повторный optimizer step использованной пары. R22 исследует interactions по trigger, не до каждого E1.

**Показатели.** Выбрать применимые к claim: domain confusion/unknown-abstention calibration; law violation/semantic retention/representation incompatibility; legal neighbor recall/rank/actual use; lawful path quality относительно declared alternatives, steps/latency/all branches; pair usability/preference отдельно от Student utility; per-domain/worst-domain quality/retention/transfer; family uncertainty; PC peak/growth/time-to-quality/total attempted cost. Показывать N и independent families. Adapter bytes не whole-system compactness. Случайность не proof learning; deterministic trained computation не его опровержение.

**Bulk-to-bulk.** Bulk — окно новых допустимых lessons. Parent/challenger внутри bulk используют одну versioned panel. Между bulks сохранять declared anchor либо учитывать смену cohort/difficulty; более лёгкий следующий bulk не gain. Operational panel не untouched final; repeated checkpoint selection/tuning и evaluator disagreement отражаются в limits/uncertainty. Retention не разрешает replay старых пар.

**Вердикты.** VALID_MECHANISM, PAIR_ADMITTED, OPTIMIZER_ENTERED, UPDATE_OBSERVED — отдельные факты. IMPROVE — заранее достаточный valid effect без недопустимой регрессии; REGRESS — valid вред; INCONCLUSIVE — недостаточная/неопределённая разница; INVALID_EVIDENCE — leakage/rubric drift/state mismatch/unknown disposition; без опыта NOT_MEASURED. Valid REGRESS/INCONCLUSIVE закрывает опыт, не requirement improvement.

**Promotion.** Isolated candidate → independent frozen assessment → разрешённый accept/retain → cold ordinary consumer использует выбранную identity. Loss без gain, source breach/inconclusive не выбирают child. Identical weights — допустимое наблюдение, не force mutation. NO_TRAINING_NEEDED сохраняет достаточный parent без выдуманных teacher/update. Допуск опыта не требует заранее доказанной эффективности; promotion и delivery — отдельные решения.

## 5. Stop / retry / неизвестные исходы

- До effects объявить конечные limits новых lessons/bulks/calls/time/memory/attempts/branches/budget, owner и stop. Нет repeat-until-green/improve; новый run_id/pair_id не делает использованный урок новым.
- RED existing focused evidence: invariant regression / fragile assertion / environment / unknown; сверить original requirement до изменения oracle. Не подгонять assertion ради цвета без authorized semantic change.
- Retry требует новой причины/различающего вопроса и незатраченных прав/лимита. PENDING/SKIPPED не SUCCESS. Negative valid experiment завершить, не повторять до положительного.
- UNKNOWN mutation → freeze/quarantine/reconcile full snapshot/journal до любого нового update. Unknown не failed step.
- Rollback возвращает применимое model/adapter/optimizer/scheduler/RNG/counters/representation/index/graph состояние, **но не стирает durable no-repeat ledger и terminal/unknown disposition использованной пары**. Иначе rollback незаконно разрешил бы тот же урок повторно.
- INVALID_EVIDENCE → owner конкретного дефекта; новый независимый evidence не получается retune threshold. User feedback проверяется на rights/meaning/novelty/conflicts до admission.
- Budget exhaustion → STOP/keep accepted parent, actual cost/status и owner следующей гипотезы; не ещё один Windows run ради badge.

## 6. Windows/CI: наблюдавшаяся конфигурация vs proposal

Прежние source observations сохранены в §1 с ограничениями. Policy commit не управляет core triggers и не доказывает прекращения jobs. Прежний branch-protection 403 не разрешает снять required check/объявить его отсутствие. Никакого skip-ci/paths-ignore/fake SUCCESS/skip-as-PASS или изменений jobs/rulesets этим документом.

**Migration proposal НЕ ПРИМЕНЁН:** [R17-WORKFLOW-MIGRATION-PROPOSAL.md](R17-WORKFLOW-MIGRATION-PROPOSAL.md). Core/release owner сначала читает actual required contexts и выбирает разрешённую стратегию честных statuses; только отдельная authorized core change меняет triggers/dispatch. Required jobs завершаются honest applicable result либо официально изменяются в защите ветки; простое filtering может оставить Pending. Local supported Windows runtime и GitHub campaign не отождествлять.

## 7. Шесть документных walkthrough, не product tests

| Сценарий | Evidence now/future | Не требуется | Результат/граница |
|---|---|---|---|
| Исправить Markdown prompt | Source-bound diff/complete packet/remote readback | Build/Windows/training | DOCUMENTARY_APPLIED, не LEARNED |
| Исправить non-native kNN scorer | Affected law/state/consumer и applicable existing integrity; затем frozen domain/latency contrast на supported host | Full Windows campaign, новый trainer/все домены сразу | Integrity отдельно от utility |
| Восстановить optimizer после crash | Journal/full state/pair identity/ordinary reload/consumed disposition | Новый ID старой пары или update при UNKNOWN | Quarantine до resolution; no-repeat после rollback |
| Добавить domain/vector panel | Provenance/law-sensitive independent families/oracle-predicted mapping/frozen rubric | Coverage proxy, retune после outcome | Valid result или honest INCONCLUSIVE |
| Менять native launcher/security | Exact candidate/target Windows evidence/actual statuses | DPO gain для launcher | Native gate для этого свойства, не всех уроков |
| Публиковать S05 remote artifact | Rights/custody/identity/remote/host-release statuses | Считать S04 synthetic достаточным или переигрывать обучение | Missing permission блокирует публикацию, не создаёт новый урок |

## 8. Handoff и независимость

- **R18:** selected policy rows/AC IDs, actual owner/law/consumer/control/runtime/native edge и separate readiness/result; разрешить собственные bindings, не обходить их и не ждать irrelevant whole-program DONE.
- **R19/R20:** применять policy/полный R17 packet в actual dispatcher/compiler scoped; не возвращать Windows-first/30-review/all-mechanisms-per-bulk defaults. Core changes остаются proposal.
- **R21:** staged seven subjects/extras/laws/families и structurally different second domain с owner/trigger; first lesson не ждёт whole curriculum.
- **R22:** causally affected domain/vector/kNN/DPO/LoRA/role experiments и bounded interactions; **не все механизмы на каждом bulk**. Claim/budget заранее.
- **R23:** full attempted PC/local/external cost, feedback admission, durable no-repeat/recovery/growth/retention; future native owner сохранён.
- **R25:** actual отдельный reviewer exact candidate; эта редакция не Astra Ultra PASS. Requested model не actual invocation identity.
- **S00–S10/DTC/IDE:** original capabilities/release obligations сохраняются с owner/trigger; first lesson не full program.

**Good:** domain before/after/no-update/state integrity для выбранного опыта, Windows на causal native property, bounded interaction для соединения. **Bad:** loss = Graph learned; каждый bulk проверяет всё; unknown mutation = retry той же пары; author = independent reviewer. **Edge:** TIE/BOTH_BAD/UNJUDGEABLE, lawful alternative, missing family, evaluator disagreement, mixed-domain regression, incompatible crossed state, native evidence absent.

**Граница честности:** R17 меняет planning criteria; не обучает Student, не вызывает Teacher/CODEX/Astra, не меняет workflow/branch protection и не доказывает эффективность алгоритмов.
