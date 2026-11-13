> **CURRENT R24 AFTER DOCUMENTARY RE-EXECUTION (13.10.2026):** [R24-CAUSAL-REPAIR-2026-10-13.md](R24-CAUSAL-REPAIR-2026-10-13.md) is now the expanded causal/source-bound acceptance record. The old §8.7 REEXECUTION_REQUIRED statement is a **historical state** preceding that record; see new §9 below. Current: 20-run semantic intake documented, four-role/seven-domain examination and six bounded packets written; no Student learning, PC-fit, deployed ordinary DTC/IDE or independent R25/Astra review observed.

# R24 — обычные DTC/IDE пользователи должны получить результат выбранного обучения

**13.10.2026 · R24 planning deliverable · DOCUMENTED / RUNTIME ADOPTION NOT OBSERVED / LEARNING NOT MEASURED / INDEPENDENT ASTRA REVIEW NOT PERFORMED.**

## 0. Человеческий результат и граница задания

Обучение имеет смысл не тогда, когда trainer смог сохранить checkpoint, а тогда, когда пользователь на **новом** макете или в **новом** проектном запросе получил более правильный, полезный и достаточно быстрый ответ от обычного клиента, загрузившего именно принятое состояние. В XIIGEN нужно показать этот результат отдельно от технической правильности хранения пары, модели и журнала. R24 задаёт проверяемые переходы и будущие опыты, **не реализует** сервер/UI/VSIX, не вызывает teacher, не обучает модель, не запускает Windows Actions и не объявляет improvement доказанным.

Два неперемешиваемых продукта: **DTC** (Figma/Canva→HTML/CSS через platform/flows/learning) и **IDE** (VS Code/Visual Studio/Continue→проектный контекст→план/разрешённые шаги/полезный ответ). Первый информативный локальный E1 CodeDocumentation из R18/R22 можно выполнить после его собственных G1–G6, допуска и independent review; **полная DTC/IDE поставка не является входным блокером E1**. Но E1 trainer-demo не считается DTC/IDE adoption. Состояние R23 по фактически просмотренной директории results/R01–R22: собственного опубликованного R23 result не наблюдалось; numeric PC/tier budget и actual hardware здесь **OPEN**, не назначены по догадке.

**Разделение ролей без двусмысленности:** CODEX с моделью **GPT-6 Astra 6 UltraCode** выполняет роль человека при выборе разрешённого нового урока. **Допущенная внешняя открытая китайская модель** создаёт **обе** финальные стороны DPO — chosen и rejected — под одним frozen task/context/contract. После этого CODEX с **GPT-6 Astra 6 UltraCode** **отдельным действием** проверяет качество точной неизменённой пары, в том числе возможные TIE/BOTH_BAD/UNJUDGEABLE и pair_usable. Затем Student XIIGEN/ILearningMachine обучается лишь при соответствующих admission/rights/state правилах. После обновления **отдельный независимый evaluator** оценивает обычные ответы; Astra judgement пары и независимая R25 архитектурная рецензия — разные проверки. В R24 никаких реальных вызовов этих ролей не выполнено.

## 1. Сквозной DTC: от макета до следующей конверсии

Primary: D/Обучение создания HTML CSS - 1st level.md, пункты 1–8 и 8а.1–8а.3; D/WorkLeft-COVERAGE-01 и WorkLeft-RAG-02; K/25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs; K/44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs; K/docs/skills/design-conversion-tier-budget-policy-GUIDE.md. Старый WorkLeft coverage считает наличие строки плана, а не deployed external client. Следующие схемы — **target/proposed** поверх реально обнаруженных отдельных API DTO/loop records, не утверждение готового cross-repo хоста.

| ID / переход | Вход → выход, версии и владелец | Что наблюдать / остановка; честный статус |
|---|---|---|
| **D1 Source-client intake** | FigmaToCode-design-gen или CanvaToCode создаёт SourceDesignRef, original tree, screenshot/export refs и generated HTML/CSS; owner client connector, source/schema/version/license/tenant/request id | Сверить origin + tree + renders + rights; malformed tree/empty HTML/unauthorised tenant отклонить. Исходные client repos названы; их deployed request→response **NOT VERIFIED** |
| **D2 Platform API** | xiigen-platform-dot-net-angular и xiigen-flows-dot-net-angular принимают запрос, устанавливают entitlement, idempotency, tenant, source content digests и передают typed envelope ModelServer; owner platform API | Реальный authenticated cross-repo caller, response/schema/limits и trace ID. K DesignConversionModelServerApiRequest имеет DesignTree/OriginalTree/Html/Css/Screenshots/DesignRenders/Budget, но внешняя platform API/adoption **NOT VERIFIED** |
| **D3 Learning/Unit and representation** | Platform-bound job→домен дизайна, input vector/token law (не терять geometry/order/constraints, text/RTL, units/breakpoints)→typed many-to-many candidates, stable association IDs; owner 44.DesignConversionDomain + Machine | Каждому candidate — original input association, declared model/index/codebook version. Pseudocode mapper или fixed k не объявлять trained. При неполном source return gap, не перепутать layout с CSS токенами |
| **D4 Render at viewports** | Candidate HTML/CSS + assets→Chromium/QA Automation screenshots минимум по заранее заданным source-relevant viewport families; owner render service/host | Output: RenderSet refs, viewport DPR/font/browser/version, job id, timing. Failure: missing asset, script injection, browser unavailable, responsive collapse. Не подменять реальную отрисовку static image similarity |
| **D5 Visual/semantic assessment** | Source visual refs + actual RenderSet + DOM/structured constraints→geometry/typography/structure/responsiveness/required accessibility issue list, uncertain/TIE and provenance; owner authorized external visual examiner + deterministic checks | Сравнивать при одинаковых viewports/cost; visual AI judgement допустим только согласно separate provider approval/privacy policy. Один «процент похожести» не покрывает смысл/взаимодействия. Actual evaluator response **NOT VERIFIED** |
| **D6 Bounded improvement** | Findings + candidates + tier policy→новая допустимая версия HTML/CSS либо best-effort/stop/ask consent; owner improvement coordinator, server tier governor | K ImprovementLoopRequest имеет MaxAttempts и MeasurementGate; K tier DTO имеет MaxRenderQaRuns/BudgetUsd/MaxTeacherCalls. Сервер должен фиксировать каждый attempt, фактические расходы/латентность и stop; free/paid/pro имеют отдельные заданные политики, НЕ выдуманные здесь численные лимиты |
| **D7 Persist lawful observations** | Original tree, generated code, target/renders, judge discrepancies, tier/time/cost, versions/rights, consent and provenance→versioned DB evidence record; owner platform store; learning service только по разрешённому экспорту | Durable record и удаление секретов/внешних restricted assets. Данные для visual repair ≠ автоматический допуск в общую тренировку. Нет утверждения, что app DB уже подключена к обучению |
| **D8 Local DPO and assessment UI** | Локально открывается разрешённая DB/corpus→новый урок/закон domain→CODEX/Astra case selection→open Chinese both final pair texts→separate CODEX/Astra exact pair judgement→pair_usable + usefulness panel; owner local legal DPO service/UI | DPO negative должен быть реалистичным и материально хуже при том же frozen context; correct deterministic code может быть chosen. Показать chosen/rejected, pair validity, expected utility hypothesis, provenance, uncertainty, charge; TIE/BOTH_BAD не обучать. UI presentation original 8а.2 **OPEN** |
| **D9 Training, selection and next ordinary request** | Authorized lesson→existing Machine/DPO owner→trial project adapter/weights→separate parent vs trial evaluation→accept or full rollback→local selected state→future S05 versioned remote delivery→next Figma/Canva conversion cold ordinary client | E1/other early local lesson не требует S05 remote. Для DTC product adoption доказать actual platform caller reload/deployed model identity (base/LoRA/tokenizer/index/graph manifests), response uses accepted state; stale cache/incompatible codebook fail closed. После remote publish проверить реальное adoption and replay-idempotency, не только upload. **NOT OBSERVED** |

**Контракт перехода для любого D1–D9:** request_id, immutable source family id, tenant/rights, domain/intent, input/output schema id, association ids/OutputRefs scope, model_version+adapter_digest+tokenizer/codebook/index/graph_version, caller, contract_version, budget_rule_version, evidence refs, decision and failure code, work/time/cost, reversible vs irreversible effects. Требуемое поле отсутствует → documented contract gap/typed refusal, не неподтверждённый happy path. Product state-changing action не возникает из evaluation observation автоматически.

**DTC правильный пример (иллюстрация, не результат модели).** Новая Figma-форма имеет 3 breakpoints, RTL подписи, два связанных поля и доступную клавиатуру. Parent после реальной отрисовки сохраняет порядок визуально, но ломает focus order при 320 px; trial на независимом макете сохраняет DOM semantics и responsiveness в той же тарифной цене. Экзамен отдельно проверяет click/focus/labels и screenshot family; только после cold ordinary loaded selected state можно назвать improvement в DTC.

**DTC неправильный пример.** Совпала одна картинка 1440 px на старом шаблоне, а страница на 375 px горизонтально скроллится, часть текста заменена изображением; другой judge видел teacher answer; API/UI не вызваны. Verdict INVALID_EVIDENCE/NOT_DEPLOYED, даже если DPO loss уменьшился.

**DTC крайние случаи:** исходный tree и screenshot противоречат; один видимый рисунок допускает несколько валидных DOM; CSS зависит от внешних шрифтов; JS/external content запрещён; пользователь отменил после записанного costly render; DB запись закончилась при сетевом сбое; модель выбрана, но platform cache держит старый manifest; в input доступны секреты; несовместимый codebook или source license. Для каждого owner фиксирует отказ/деградацию, повторы только по idempotency и без дублирования оплаченных эффектов.

## 2. Сквозной IDE: обычный запрос, законность действий, новый ответ

Primary: D/WorkLeft-COVERAGE-01 §4 (6 шагов); D/WorkLeft-RAG-04→V/LeftToWork-CONTINUE-M1/M2/M4 и V/PLAN-VS-VSCODE-CONTINUE-v1; D/WorkLeft-PLAN-HUMAN-01. V older progress explicitly plan/port-only, не installed acceptance. Hosts/SDK/core хранят разные responsibilities; решение о native installed квалификации принадлежит host owner.

| ID / переход | Вход → выход, версии и владелец | Проверка / failure; статус |
|---|---|---|
| **I1 Connect/project permission** | Host (VS Code/Visual Studio/Continue) выбирает root/repo/branch/permissions; core получает workspace identity и allowed capabilities | Проверить trusted root, read-only, tenant, versioned protocol. Forbidden repo/path returns refusal. Installed host acceptance **NOT OBSERVED** |
| **I2 Evidence/indexing** | Allowed local files/calls + project/stack/language/dialect/version→source-cited index/vector and document pointers; owner local index/42.ProjectWikiDomain, not host UI | Index must retain symbol/order/nullability/SQL dialect/Angular version, stale-index policy, provenance, permission filtering, and typed domains; local raw project code не отправлять teacher без разрешения |
| **I3 Task + mapper** | User request in language/intent + project coordinates + source evidence→domain/intent/stack/project mapping and task constraints | Compare predicted-domain vs oracle-domain on ambiguous C#, SQL, Angular, mixed repo, wrong-domain cases; Hebrew/unsupported-tokenizer path blocks training where required rather than fabricated translation |
| **I4 Plan and flow** | Tasks→ILearningUnit typed candidate answers, ILearningFlow output-ref corrections, ILearningOrchestrator dependencies/async alternatives→bounded staged plan; DynamicGraph composes prior patterns | Record route, rejected lawful routes, association IDs, cancellation/fan-in work, per-step budget. Same candidate pool for selector contrast; solver quality vs selector improvement separate. User sees plan and status |
| **I5 Authorized execution** | User approval + narrowed tool grants→stepwise edits/read-only answer/commands; owner host execution broker, core policy not raw OS privilege | Check actual permission before tool action and after stale state; cancellation after irreversible write does not equal rollback. Future native Win/process/security verification **only** at actual host operation/release edge |
| **I6 Useful output + observation** | Answer/patch/plan + source evidence + actual chosen state ID→user-visible result, diff, error/weakness and feedback record | Real task correctness/usefulness, lawful action, terminal refusal and debug status; training console output alone invalid. If user says «нет» or spec changed, feedback not automatic truth |
| **I7 Permitted feedback→next normal response** | Valid rights+novelty+evidence lesson→separate CODEX human role, Chinese author, CODEX pair judgement, Machine single permitted update→accepted compatible local checkpoint→new ordinary IDE request via installed host | Compare same host+workspace snapshot+memory + fixed panel before/after; reload selected adapter/index. Remote/portable distribution later, host version compatibility separate. If exact installed consumer unavailable: ADOPTION_NOT_OBSERVED |

**IDE правильный пример (иллюстрация):** в незнакомом Angular/C# проекте пользователь спрашивает, почему клиент вызывает endpoint после cancellation. Orchestrator строит план с ссылками на исходники и без записи файлов до одобрения; frozen parent пропускает отмену, challenger на **другом проектном семействе** корректно выбирает lawful read-only route и показывает source-backed explanation. Проверять отдельно mapper, route, output и latency.

**IDE неправильный пример:** модель демонстрирует улучшенный JSON в evaluator, но расширение не установлено; использована другая ветка или host не запросил разрешения, а «успех» проверен парсингом JSON. Это не IDE product acceptance. **Edge:** offline workspace, corrupted index, missing symbols, guarded secrets, mixed SQL dialects, host cancellation after git write, stale local adapter, read-only forbidden mutation, divergent VSIX/extension.ts SDK protocols.

## 3. Гибридная архитектура участвует по роли, а не названием

| Роль / причинный объект | DTC / IDE ordinary consumer | Deterministic law → learned variable → observation / falsifier |
|---|---|---|
| **ILearningUnit** | D3/D9 typed candidates; I2/I4 docs/code evidence/result typed many→many | Domain/schema/type/association eligibility → trained candidate score/project-LoRA where installed → correct multiple bound outputs on unseen families; Zip/reorder losing input IDs fails |
| **ILearningFlow** | D5→D6 corrections and D8 signal; I4 plan/source-ref adjustment | Versioned output-ref resolver, exact port types, constraints → learned correction policy if identified → downstream correction with frozen Unit; unresolvable/stale ref fails |
| **ILearningOrchestrator** | D4/D5/D6 render/judge/retry budgets; I4/I5 async branches and approvals | DAG dependencies, cancellation, allowed effect/tier ceilings → learn route/schedule among admissible paths → better lawful route/outcome per full consumed cost, count abandoned branches; no global-optimum claim |
| **ILearningDynamicGraph** | D3 semantic layout/DOM/higher-order relations; I4 cross-module dependency patterns | Ordered pattern/graph version/index compatibility and rights → trainable higher-order relation state if actual owner found → unseen composition transfer with frozen lower roles; static graph registry alone not learning |

**Common retrieval/representation factor grid.** Exact identity/source law first; then oracle-domain versus predicted-domain, frozen representation vs domain-specific vector/token representation, fixed exact neighbors vs adaptive eligible k/radius, fixed pool/top1 vs multi-neighbor aggregation/reranking, approximate index vs exact comparator, and learned selector versus frozen solver. A domain law cannot be «fixed» by changing k after looking at sealed exam. DTC preserves geometry/DOM order/responsive rules/RTL; IDE preserves syntax/AST/stack/version/SQL NULL and permission/cancellation. **LoRA** is parameter-efficient model update, **not** the tokenizer/codebook or automatic source→target vector projection. If learned projection is proposed, name P_domain source space, target typed tokens, objective, actual trainable keys, consumer and independent semantic preservation experiment. A lower-level speed improvement is not automatically a learned Unit/Flow/Graph gain.

## 4. Examination: качество новой семьи, причинность, время и вся цена ПК

1. **Freeze before outcome:** accepted parent N (no new lesson), candidate U (exactly one admitted new lesson), same corrected material/response objective/generation/loader pipeline, same model/source context, same memory/index unless measured factor, same compute envelope, same evaluator+routing policy. Legacy L→N engineering delta separate, never count it as learned gain N→U. Sealed CERT source-family-disjoint from TRAIN and operational DEV; frozen rubrics and numeric thresholds chosen by authorized exam owner before U. R23 hardware/caps/entitlement still OPEN.
2. **Per-domain panels:** DTC grouped by design-family, designer/tool, viewport/RTL/layout pattern; measure DOM/geometry/structure, responsive breakpoints, semantic controls/accessibility where required, real render/asset success, speed and *tier-wide* costs. IDE grouped by repository/project, task family, stack/dialect, permission mode; measure evidence-grounded correctness/usefulness, mapping, lawful refusal/action, plan path quality, output correctness, side-effect outcome, p50/p95 latency and full cost. Do not count paraphrases/screenshots as independent family samples.
3. **Hybrid ablations staged:** oracle vs predicted domain; semantic representation under unchanged neighbor law; exact vs approximate index + retrieval loss; learned rerank/k/radius only when multiple neighbors influence answer; frozen Unit→changed Flow, frozen Flow→changed Orchestrator, flat primitives→higher-order Graph. Each contrast needs actual mutable state owner and ordinary consumer; missing factor → NOT_MEASURED, not ZERO_GAIN.
4. **DPO Builder quality:** exact same task/input/output schema for both sides, correct source rights and non-contaminated provenance; CODEX Astra lesson choice, open Chinese authors BOTH, separate CODEX exact-pair assessor; preferred_side distinct pair_usable; side-by-side acceptance for plausible/material contrast; no bad deterministic answer solely for being rule-based. Builder JSON validity/pair preference ≠ downstream Student improvement. Measure useful admitted-pair yield, invalid/tie/rejected distribution, independent Student delta and dollars/time per *useful new lesson*.
5. **State and learning retention:** compare U vs N on old-domain sealed families plus new, measure cold ordinary reload exact accepted digest, checked adapter/base/encoder/index/graph compatibility. UNKNOWN_STATE quarantine, no repeat optimizer with same pair after timeout; full rollback includes optimizer/RNG/counters and applied identities; no hidden post-response training. R23 must inventory peak simultaneous CPU/RAM/VRAM, disk and checkpoint/index growth, snapshot/reindex/reload/teacher/judge/evaluator, retries, failed and cancelled paths; report measured versus unknown.
6. **Judgement:** independent source-based examiner runs against ordinary outputs, logs confidence/uncertainty and blind side assignment where possible; CODEX Astra independent architectural examination/review (R25) must critique causal assumptions and wrong-direction signals, **not replace direct observation**. Verdict for each family and overall is IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE; if no run NOT_MEASURED. Set evidence-quality floor, practical margin, regression tolerance and time/cost ceiling **before** new outcome, not to force improvement. A correct refusal can be better than a plausible but unauthorized answer.
7. **Longitudinal BULK:** each bulk contains distinct admitted new lessons (not repeated old pairs); compare N and U in the **same** predeclared panel for that bulk, track task-family mix separately across bulks, report time per correct/usable new family, changes in worst-domain performance, memory/disk growth, no-update control and old-domain retention. Increased corpus size or decreased training loss alone is not quality evidence.

**Report formula** (no fake numerical thresholds): coverage = evaluated unique families / predeclared eligible families; exact law accuracy = law-valid outputs / independently judged outputs; ordinary adoption rate = observed correct loaded-state responses / eligible cold requests; path regret = quality/cost gap against best enumerated lawful route when finite, otherwise label best-observed fixed-budget comparator; full cost per useful family = all learning+evaluation+inference+retries / verified new-family successes. Report confidence intervals/paired differences as appropriate and avoid optimizing on final CERT. No global claim «20× cheaper» from workflow-elapsed alone.

### Пример короткого отчёта пользователю (целиком вымышленный, не наблюдение)

> Учили сохранение порядка полей в адаптивном макете. Frozen parent и trial получили одинаковые новые Figma-family задачи, одинаковые 320/768/1440 viewports и одинаковый тарифный предел. Trial исправил DOM-порядок, но увеличил время рендера; IDE C# на сохранённом retention panel осталось без значимого изменения. Вердикт: INCONCLUSIVE до независимой оценки и полного resource ceiling. Новые веса не объявлены принятыми; следующий вопрос — полезно ли learned rerank при неизменном Unit? Ни одна цифра здесь не является результатом запуска.

**Четыре допустимых исхода:** (a) IMPROVE с нижней границей эффекта и без недопустимой деградации по старым доменам/бюджету → допустимое принятие состояния, только если все state/effect permits подтверждены; (b) NO_GAIN/INCONCLUSIVE → parent остаётся, меняем **новую** гипотезу без повторения той же пары; (c) REGRESS → полный rollback с retention check и durable consumed pair_id; (d) INVALID_EVIDENCE (leakage, differing pipeline, wrong manifest, unreachable client, unapproved judge, missing legal grant) → никакой claim об улучшении, исправить протокол и сохранить audit trail; UNKNOWN update сначала reconciliation, не повтор.

## 5. Product milestones без круговой блокировки

| Веха / владелец | Required evidence, trigger | Не является предпосылкой |
|---|---|---|
| **M0 first local E1** — R18 A–D + S04 local owners | Legal/new pair identity, G1 accepted source state, G2 actual keys, G3 role/provider grant, G4 independent panels, G5 R23 finite budget, G6 cold ordinary CodeDoc caller + snapshot/recovery, independent review | Remote S05, Figma client deployment, VSIX, full graph/language curriculum, Windows all-suites |
| **M1 DTC local candidate evaluation** — 44/25 + local judge | source-family panel; actual D1–D8 local stub/executable seams, no external product claim; domain visual law and tier rule | Installed VS host, S05 publication |
| **M2 S05 remote selection/adoption** — artifact publisher/platform owner | Same compatible accepted model manifest served via actual platform API, verified remote loaded-state and cache invalidation, DB/query rights | Does not retroactively create M0 local learning |
| **M3 DTC product delivery** — platform/flows + Figma/Canva client owners | Real D1–D9 authenticated ordinary journey in correct tier, visual/dom evidence, UI pair usefulness/consent, end-to-end output with selected model | Not a blanket first lesson gate |
| **M4 IDE local/host delivery** — V Track C core/SDK + installed VS Code/VS host owner | Actual root→index→plan→authorized execution→visible answer→state adoption in installed extension, per-host protocol/native tests only when touching those boundaries | Not a prerequisite for DTC learner causal experiment |
| **M5 broad domains** — curriculum S06–S10 + R21/R22 owners | Translation/C#/Angular/SQL/Builder and Graph/Flow per-domain real causal receipt and PC envelope; local feedback admission | No all-domains mandatory pass before single first lesson |

A single endpoint status flag, route mapped, a successful core build, plan approval or a stored adapter does **not** establish M3/M4. Old V M1/M4 plans document port proposals; current host/client implementation must be audited on its own code ref. If a formerly missing consumer now exists on newer code, reuse it with source proof instead of duplicate bridges. R23 not yet observed; it must supply finite physical/cost limits before any effect.

## 6. Полные model-ready дочерние work orders — не дополнительные вызовы Rxx

Следующие задания являются **частями одного R24**, не тремя независимыми research runs и не полномочием запускать child. При отдельном разрешённом исполнении дочерний agent получает полный bounded packet, а не «прочитай всё». Parent R24 сводит межрепозиторные outputs; все children return-only.

### R24-A: DTC ordinary journey / schema and adoption

**CURRENT TASK:** source-trace D1–D9 and produce one complete DesignToCode scenario with current/proposed status, typed contracts, owner/version/budget/refusal, client→platform→44/25→DB→DPO UI→selected ordinary consumer. **NOT THE TASK:** realize APIs/renderers, train, run teacher, invent tier thresholds. **EVIDENCE-ONLY:** historical WorkLeft statuses, R18/R22, K API/loop signatures until actual caller established. **Skills:** trainable-architecture-decision + phase-planning + plan-review + code-examination + prompt-patch-authoring + output-readability-gate; before implementation read whole changed code, callers/consumers and current agent instructions. **Required context/RAG:** D/Обучение создания HTML CSS - 1st level.md items 1–8+8a; D/WorkLeft-COVERAGE-01 §2; D/WorkLeft-RAG-02→source originals; K/25.ModelServer/DesignConversionModelServerApi.cs, DesignConversionModelServerAspNetHostStartup.cs, K/44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs and tier-budget policy guide; actual platform/flows/Figma/Canva code only if connector reveals it. **Algorithm:** source tree/DOM typed semantics→deterministic law→learned candidates with association→real render→comparative judgement→tier governor→persisted observation→lawful pair→selected-state normal conversion. **Bounded work:** (A1) trace cross-repo API and mark unobserved callers; (A2) fill D1–D9 schema/IDs and typed failures; (A3) simulate valid responsive+RTL scenario and malformed tree/incompatible codebook; (A4) propose matched parent/trial viewport-family examiner; (A5) return gap IDs and staged milestone gates. **Good:** real tree refs, two independently designed responsive families, selected digest in ordinary API. **Bad:** one screenshot score or training file said to be deployed. **Edge:** external fonts unavailable; duplicated request after publish timeout, malformed tree, private material. **Allowed:** read refs, create own assigned planning artifact only if explicitly granted; **forbidden:** core/host edits, teacher/model calls, Actions, fake promotion, other results. **Verification NOW:** source-to-D1..D9 completeness, owner/consumer and failure for each step, source-excerpt honesty, current/proposed separation. **Future verification:** actual browser/viewports and source-disjoint blind human/visual/DOM exam with total cost. **Return schema:** source pin/read-depth; exact steps; owner/schema; good/bad/edge; implementation gap/consumer; document-only verdict; resource/role limits; claim_scope; independent evidence count=0 unless real separate examiner.

### R24-B: IDE ordinary journey / installed host evidence

**CURRENT TASK:** trace I1–I7 from editor workspace through local project index, mapper, async orchestration, permissioned execution, feedback to new ordinary result; enumerate VS Code, Visual Studio, Continue consumer variants without conflating them. **NOT THE TASK:** port branch, modify VSIX or execute unsafe host tool. **EVIDENCE-ONLY:** V M1/M2/M4 are historical port-map/plan; CORE docs do not prove host installation. **Skills:** same six, plus reference-inventory-and-source-map and claim-verification for uncertain repository boundaries. **Context/RAG:** D/WorkLeft-COVERAGE-01 §4; D/WorkLeft-RAG-04→V/LeftToWork-CONTINUE-M1/M2/M4+V/PLAN-VS-VSCODE-CONTINUE-v1 (re-fetch selected actual spans), D/WorkLeft-PLAN-HUMAN-01, K live SDK/adapters when located; R21 domain passports/R22 C07–C09. **Algorithm:** domain law/permission and source indexing→typed Unit associations→Flow corrections→async Orchestrator route→Graph higher-order links→approved execution→state/feedback loop. **Bounded work:** (B1) one C#/Angular/SQL mixed repository task and actual authority; (B2) map SDK/host protocol and installed requirement; (B3) route/solver/domain-mapping separated frozen-factor comparisons; (B4) simulate read-only forbidden-write, offline/stale index and cancellation after effect; (B5) return local/remote/native milestone distinction. **Good:** new project family, source-backed output with explicit no-write until consent, actual installed host loads selected manifest. **Bad:** evaluator JSON or hypothetical VSIX treated as editor delivery. **Allowed:** own planning docs and read-only evidence; **forbidden:** host/process tests unless explicit native implementation/release task separately authorizes; no code/training/workflow changes. **Verification NOW:** each I1–I7 has trigger/owner/schema/caller/failure, all host variants with status. **Future verification:** installed client observations with recorded permissions, domain correctness, p50/p95 and full cost. **Return schema:** actual/unknown host; source refs; journey; good/bad/edge; law/learning/evaluation/next gap; independent evidence count.

### R24-C: shared consumer and learning examiner

**CURRENT TASK:** integrate A/B outcomes without treating source inventory as read, prepare independent normal-consumer before/after examination matrix, report format and specific first E1→DTC/IDE deferred adoption. **NOT THE TASK:** invent numerical PC limits; label R15/R25 or any Astra run completed; retry an applied pair. **Skills:** phase-planning, plan-review, trainable-architecture-decision, code-examination, output-readability-gate, prompt-patch-authoring, experiment-design, improvement-measurement. **Context/RAG:** R13 causal protocol, R17 evidence policy, R18 E1, R21 curriculum, R22 experimental catalog + own R22 result, R23 exact recorded result **if it exists at invocation**, DTC/IDE primary and updated state manifests. **Algorithm:** frozen corrected parent N/challenger U with identical ordinary client; independent sealed source-family panel, component law, state adoption, separate domain/selector/solver/Flow/Graph factors; cost/retention/uncertainty and truthful final verdict. **Bounded work:** (C1) define independent panel per product; (C2) predeclare domain-specific rubrics and budget source or mark OPEN; (C3) four verdict walkthroughs; (C4) map selected checkpoint to ordinary endpoint and unresolved G1–G6; (C5) return R25 source-bound acceptance and precise next implementation step. **Good:** no gain at higher cost → parent retained and new hypothesis; **bad:** training loss/JSON accuracy/coverage as product improvement. **Edges:** leakage, unexpected multi-domain regress, UNKNOWN_STATE, wrong accepted digest, mixed policy versions. **Allowed:** read and assigned R24 planning writes. **Forbidden:** DPO authoring calls, weights, model updates, CI/Windows loops, R25 independent PASS. **Verification NOW:** explicit unmeasured cells, causal comparability and all consumer roles; **Future:** separate authorized blinded evaluator and CODEX/Astra architecture review with actual response refs, cost and state. **Return schema:** per-product family matrix; frozen objects; four outcomes; doubts/falsifiers; funding/permission gaps; next work order; exact self-check vs independent status.

## 7. Sources and truthful status

DIRECT CONTENT this R24: R24 prompt + COMMON; D/Обучение создания HTML CSS - 1st level.md whole; D/original task.md opening (four roles); D/WorkLeft-COVERAGE-01 targeted §2/§4; D/WorkLeft-PLAN-HUMAN-01 relevant summary; D/WorkLeft-INTEGRATION-01 current initial sections; R21/R22 findings; R23 prompt; R18 work order opening and algorithm; V/LeftToWork-CONTINUE-M1 first part, M2 and M4 excerpts; D/WorkLeft-RAG-02 and RAG-04 indexes. CORE DIRECT CODE EXCERPT: K/25.ModelServer/DesignConversionModelServerApi.cs, ...AspNetHostStartup.cs, K/44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs, K/09.Inference/TextGenerator.cs; CODEDOC saved assessment and publication details from R18/R22 remain **ATTRIBUTED**, not first-hand fully re-audited. K docs/skills architecture/planning/review/code/prompt/human and tier guide read in relevant sections. The very large previous-rounds report and V main plan were **not** fully re-audited; V main plan returned no normal text body through this connector, so do not claim its 6065 lines were read. RAG directories and other repositories were not fully crawled. Exact files, read depths, SHA and inherited limits are in results/R24/SOURCES.csv.

**CURRENT DOCUMENT ACCEPTANCE:** source-linked D1–D9 and I1–I7 transitions; explicit role/caller/schema/failure/unknown; matched evaluation and four verdicts; local E1 vs remote S05 vs installed native host gates; first child packets filled with scope/skills/refs/good/bad/edge/verification; five own carriers + plan pointer published and read back. **NOT CLAIMED:** model gains, deployed clients, trained DPO pair, exact teacher/CODEX call, R23 PC-fit, or R25 independent review.

**Next:** R25 must take this observable ordinary-user acceptance together with R22 E1 and actual R23 resource receipts (or OPEN), run an *actual different* independent reviewer or mark NOT_PERFORMED, then dispatch smallest authorized implementation of existing R18 A/B/C to enable first cold normal CodeDocumentation learning comparison. Product DTC/IDE owners subsequently close M1–M4 without delaying this local first-learning path.

## 8. R24 REPAIR — parallel R01–R20 intake and causal hybrid acceptance

**13.10.2026 repair authority.** This section supersedes any weaker R24 wording where R24 could be executed as a downstream DTC/IDE documentation exercise from a stale snapshot. R24 is still one planning run, but at invocation it must consume the **currently observable semantic deltas of R01–R20** without waiting for all twenty runs to become DONE. Missing/partial sibling output blocks only the claim that depends on it and is recorded as `RESULT_NOT_OBSERVED@checkpoint`; it does not create a global wait cycle.

### 8.1 Mandatory preflight before narrowing

R24 must begin with a bounded evidence ledger, not with DTC/IDE prose:

1. Read current `PARALLEL-R01-R20-CONTRACT.md`, `PARALLEL-R01-R03-JOIN.md`, `COMMON-CONTRACT.md`, `SOURCE-ROUTES.md`, the current R24 prompt and this file.
2. Snapshot all observable `results/R01`…`results/R20` identities/statuses. For each run, record only semantic findings relevant to: original scope, RAG/WorkLeft coverage, representation, DPO, domain mapping, kNN, LoRA/state, four learning roles, ordinary consumer, evaluation, PC cost, Windows/test-placement, plan/application ownership. Do not convert sibling PARTIAL into COMPLETE.
3. Recover the original user/product result from `original task.md`, `task prompt.md/.txt`, `goal of the round.md`, applicable `*WorkLeft*` / `LeftToWork*`, and RAG→primary routes. R24 may reuse R01–R03 receipts, but any consequential claim must still name the primary/owner decision it depends on. **S04-only or slice-4-only execution is invalid.**
4. Refresh the three delivery lines separately: DPO/DynamicGraph, Design-to-Code, IDE/VS/VS Code/Continue. A narrow first CodeDocumentation lesson is an early experiment, not permission to discard the other delivery lines.
5. Before publication refresh R01–R20 once. Ingest only semantic deltas that change R24 decisions. Late contradictions reopen the affected row and descendants, not the entire R24.

Required ledger row:
`origin_run/local_id → repo/ref/blob/span → meaning + limitation → affected R24 mechanism/journey → accepted|contested|deferred → owner/next action`.

### 8.2 The four learning roles are mandatory causal objects

For **each** applicable ordinary-user journey, R24 must explicitly trace these roles instead of merely naming them:

| Role | Deterministic law that must remain true | Learned state / signal to identify | Mandatory ordinary-consumer evidence | Falsifier |
|---|---|---|---|---|
| **ILearningUnit** | domain/input/output schema, rights, type, identity, stable many→many associations | actual candidate/rerank/adapter/index state owned by the Unit learner | unseen-family typed outputs with original associations preserved | state changes but output utility/selection does not, or IDs/order are lost |
| **ILearningFlow** | compatible ports, OutputRef resolution, semantic constraints, dependency preservation | relation/correction preference for lawful Unit→Unit transitions | frozen Unit + changed Flow changes downstream correction/path on new cases | same output under changed Flow, or correction violates invariant |
| **ILearningOrchestrator** | DAG legality, permission, cancellation, side-effect and budget ceilings | route/order/stop/branch policy among admissible paths | better lawful path/outcome at **full consumed branch cost**, including abandoned branches | apparent gain disappears when equal candidate pool/budget is used |
| **ILearningDynamicGraph** | ordered pattern identity, graph/version/index compatibility, hard domain law | higher-order relation/composition state with a real update owner | transfer to unseen compositions with lower roles frozen | static registry or solver improvement mislabeled graph learning |

The training owner remains the actual `ILearningMachine`/existing trainer path. These four roles are **learning targets/consumers**, not four invented trainers.

### 8.3 Domain mapper, token/vector law and dynamic kNN are part of R24 acceptance

R24 must consume the current R21/domain passports when available and preserve at least the baseline domains plus any source-backed extras. For every domain family used in R24, record:

- real-world law/invariants and authoritative source;
- domain/intent/language/stack/project identity and unknown/mixed-domain behavior;
- input tokenization/feature law and what information must not be collapsed;
- vector/embedding/projection/codebook/index versions as separate objects;
- exact-neighbor baseline and approximate-index behavior;
- **domain-conditioned dynamic-k policy hypothesis**, not a universal fixed `k`;
- whether multiple neighbours actually affect the consumer result;
- learned selector/reranker state and its update owner;
- the ordinary consumer that uses the selected neighbour/path;
- an examination that can falsify the claimed improvement.

Dynamic-k candidates must be expressed as a family such as
`k_domain = f(domain, eligible-neighbour density, distance/margin distribution, uncertainty, admissibility, cost ceiling)`.
Do **not** choose a numerical formula after looking at sealed outcomes. Compare at minimum: exact/fixed-k baseline, adaptive-k with the same representation, and—only where supported—learned rerank/selector with the same eligible pool. `k>1` with a first-hit consumer is not multi-neighbour learning.

### 8.4 DPO pair construction and LoRA/vector crosswalk

For every R24 learning claim, keep the active role firewall:

1. CODEX/Astra in the human-delegate role selects an eligible new lesson.
2. The approved open Chinese model authors **both** final chosen and rejected outputs under one frozen task/context/contract.
3. CODEX/Astra separately judges the exact immutable pair: preferred side, `pair_usable`, TIE/BOTH_BAD/UNJUDGEABLE, rights/provenance and domain-law validity.
4. XIIGEN Student receives the exact admitted material once; unknown update outcome is reconciled before another mutation.
5. A separate outcome evaluator measures the ordinary consumer.

A correct deterministic solution may be chosen. A rejected side must be plausible and materially worse for a domain-law reason; nonsense negatives and different inputs are invalid. Pair validity, optimizer admission, DPO loss and downstream Student gain are separate measurements.

When LoRA participates, R24 must provide a **LoRA→representation→consumer crosswalk**:
`changed parameter group / adapter digest → affected hidden/output representation or score → token/vector schema expected by downstream consumer → ordinary behavior measured`.
LoRA is not tokenizer, codebook, projection or evidence that a source token became a correct target vector. If a learned projection is proposed, name source space, target typed space, objective, mutable keys, compatibility/versioning and an independent semantic-preservation examination.

### 8.5 Functional examination replaces coverage as learning evidence

R24 acceptance must be an experimental examination, not test coverage.

Mandatory contrasts where the factor exists:
- deterministic-only lawful baseline;
- learned-only candidate where meaningful and safe;
- **hybrid deterministic + learned** path;
- no-update frozen parent N vs one-update challenger U on the same corrected pipeline;
- oracle-domain vs predicted-domain;
- fixed representation vs domain-specific representation;
- fixed/exact kNN vs adaptive/learned neighbour policy;
- frozen Unit→changed Flow; frozen Flow→changed Orchestrator; flat primitives→Graph higher-order composition;
- retained old-domain panel after each new-domain bulk.

Per bulk measure at least: domain-mapping accuracy, hard-law failures, path/route quality, ordinary output usefulness/correctness, uncertainty, retention, p50/p95 where meaningful, **total** CPU/RAM/VRAM/disk/checkpoint/index/branch cost, and time/cost per verified useful new family. Every bulk contains distinct new admitted lessons; it is not replay of old pairs under new IDs.

Technical checks remain only for deterministic integrity/risk properties—permissions, provenance, serialization/shape, no-repeat, rollback/recovery, native process/security at the affected boundary. **Green tests, coverage percentage, Windows Actions, a lower training loss, a changed route ID or a new weights file are never learning evidence.**

### 8.6 Required model-ready R24 execution packet

The R24 executor prompt is invalid unless it contains all of the following, explicitly and with exact current paths:

- **CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY**
- active authority and R01–R20 snapshot/refresh rule;
- exact context files and source routes;
- required skills: `authority-requirement-binding`, `reference-inventory-and-source-map`, `phase-planning`, `plan-review`, `code-examination`, `trainable-architecture-decision`, `prompt-patch-authoring`, `human-readable-architecture-audit`, `output-readability-gate`; add experiment/improvement skills for causal measurement;
- RAG→primary traversal obligations and WorkLeft/LeftToWork roots;
- the four-role causal matrix;
- domain/token/vector/dynamic-k/LoRA/DPO requirements above;
- DTC, IDE and DPO/DynamicGraph ordinary-user journeys;
- good examples, bad examples and edge cases;
- bounded subtasks with their own inputs/outputs and return schema;
- verification now vs future runtime examination;
- exact forbidden substitutions: S04-only scope, coverage/Windows as learning proof, trainer/checkpoint as user benefit, universal k, LoRA=tokenizer, sibling summary=primary read, self-review=independent review.

### 8.7 Repair status

The R24 artifacts published before this repair remain **historical bounded evidence**. They are useful DTC/IDE design input but **do not satisfy the repaired R24 by themselves**, because they did not perform the mandatory parallel R01–R20 semantic intake/refresh and did not make every hybrid factor/domain representation/dynamic-k/LoRA/DPO causal row a completion requirement.

Current status after this document repair: **R24_REPAIR_APPLIED_TO_PROMPT_AND_GOVERNING_DOCS / R24_REEXECUTION_REQUIRED / LEARNING_NOT_MEASURED / INDEPENDENT_REVIEW_NOT_PERFORMED.**

<!-- R24-CURRENT-CAUSAL-REPAIR-2026-10-13 -->
## 9. R24 execution after the governing §8 repair

**CURRENT_DOCUMENTARY_REEXECUTION_APPLIED / REAL_LEARNING_NOT_MEASURED / INDEPENDENT_ASTRA_NOT_PERFORMED.** [R24 causal repair](R24-CAUSAL-REPAIR-2026-10-13.md) is the **current full R24 deliverable**, not a mere link to an unexecuted prompt. It includes direct snapshot/refresh of R01–R20 semantic results (§2), R23 as **present** with unknown measured PC fit, four causal learning role mappings (§3), seven domain-law/vector/kNN passports (§4–5), CODEX→Chinese both-side→CODEX exact judgement→Student→independent ordinary evaluator (§6), DTC D1–D9 + IDE I1–I7 + ordinary DPO/DynamicGraph loop (§7), controlled bulk-to-bulk examination (§8), and six bounded subtask prompts A–F (§9). Five R24 result carriers are updated in [results/R24](results/R24/).

The earlier §§1–8 remain design/evidence inputs. In particular §8.7's R24_REEXECUTION_REQUIRED describes the moment **before** this current documentary re-execution. It is not a runtime acceptance: no external agent, teaching pair, weight update, native Windows qualification, installed IDE or platform client was executed. First CodeDocumentation E1 still requires R18 current G1–G6 source/runtime/cost grants and proper no-update comparison; S05 remote/DTC and VS/Continue product adoption remain separate. R25 must provide a genuinely separate candidate-bound Astra review or explicitly NOT_PERFORMED.
