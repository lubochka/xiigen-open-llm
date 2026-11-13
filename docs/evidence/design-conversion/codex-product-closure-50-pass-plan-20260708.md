# План закрытия продукта DesignConversion - 50 проходов

Текущий вердикт: продукт не завершен. Это план исполнения в работе, а не claim о ProductReady, cloud-ready, deploy-ready, public endpoint, CI, PR или production readiness.

Граница доказательств Pass 1/50:
- текущий проход является recovery pass после потерянного worker'а;
- этот файл обновлен как плановый артефакт, без правок продуктового кода, без тестов, без build, без git commit/push/merge/checkout/reset/clean;
- вложения и выжимка из них используются как evidence text: они говорят, что 0/10 исходных требований закрыты end-to-end, R1-R10 не реализованы в задачном смысле, M1-M4 остаются partial;
- испорченный текст вложений не копируется в план; смысл пересказан чистым русским текстом;
- 50 проходов не заявлены: этот файл содержит только Pass 1/50.

## Текущий продуктовый вердикт

Продуктовая цепочка пока не доказана как работающая от пользовательского submit до durable server reload. Есть отдельные части и локальные поверхности, но нет принятого end-to-end доказательства по маршруту: platform submit -> server routing -> real render QA -> visual judge -> improvement loop -> persistence DB -> DPO build -> training/held-out -> promoted checkpoint/server wiring -> R9/R10 user surfaces -> upload/deploy gate.

Поэтому текущий статус: NOT COMPLETE / execution plan in progress / implementation authorization не выдан. Любые более сильные формулировки должны быть понижены до этого статуса, пока отдельные execution/QA/governance проходы не дадут свежие доказательства.

## 50-pass ledger

### Pass 1/50

requested_model: gpt-5.5
requested_reasoning_effort: xhigh
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

Роль прохода: восстановить первый плановый артефакт после потерянного worker'а и зафиксировать карту фаз, приоритетные пробелы, выбор маршрута, очередь governance repair и фокус Pass 2. Этот проход не чинит продуктовый код и не заявляет готовность продукта.

Что принято как вход:
- AGENTS.md и перечисленные skills прочитаны в нужном для этого узкого прохода объеме;
- два attachment-файла коротко прочитаны как evidence text, а основная содержательная выжимка взята из текущего задания;
- claim из другого model/review принят только как evidence text, не как готовое proof по repo.

Ключевой вывод: продукт надо вести как execution plan с фазовой очередью, а не как readiness summary. Самый высокий риск сейчас - P0 training contour: обучение не соединено с production route, Route A отсутствует или не подключен, Route B имеет 11.DPO math только как кандидат и должен быть проведен через production, held-out, checkpoint и server runtime.

Downgrade: да для product readiness. Нет для узкой цели Pass 1 после проверки файла: плановый артефакт обновлен.

### Pass 2/50

requested_model: gpt-5.5
requested_reasoning_effort: xhigh
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

Роль прохода: проверить P0 training contour и Route A/B decision по file:line grounding. Это плановый проход: продуктовый код, тесты, build, git commit/push/merge/checkout/reset/clean не выполнялись.

skill_load_status:
- `AGENTS.md`: found/read, релевантные правила: First Law, 50-pass evidence, no ProductReady overclaim, no approval prompt, human-readable/no-mojibake gate.
- `docs/skills/planning-session-startup-SKILL.md`: found/read top/relevant gates, включая Pass172 startup, current-ledger pass evidence, strict readiness.
- `docs/skills/plan-review-SKILL.md`: found/read relevant Pass172 and trainable/checkpoint/held-out gates.
- `docs/skills/claim-verification-SKILL.md`: found/read method/pass evidence, claim downgrade and skill-load gates.
- `docs/skills/output-readability-gate-SKILL.md`: found/read Russian readability and mojibake gates.

Проверенные файлы для grounding: `11.DPO/DpoTrainer.cs`, `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`, `25.ModelServer/DesignConversion/DesignConversionModelServerTrainingIntakeHandler.cs`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs`, `44.DesignConversionDomain/DesignConversionTrainingRunner.cs`, `44.DesignConversionDomain/DesignConversionMachine.cs`, `44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`, `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs`, `44.DesignConversionDomain/DesignConversionPromotion.cs`, `Tests/DesignConversionModelLifecycleTests.cs`, `Tests/DesignConversionDpoStorageTests.cs`, `Tests/DesignConversionModelServerTrainingIntakeTests.cs`, `LlmFromScratch.sln`, `25.ModelServer/ModelServer.csproj`, `44.DesignConversionDomain/44.DesignConversionDomain.csproj`, `Tests/LlmFromScratch.Tests.csproj`. Заданный путь `44.DesignConversionDomain/DesignConversionDomain.csproj` не найден; фактический project file в repo: `44.DesignConversionDomain/44.DesignConversionDomain.csproj`.

| Вопрос Pass 2 | Evidence | Узкий verdict |
| --- | --- | --- |
| Route A внутри `44.DesignConversionDomain` | `DesignConversionTrainingPipeline.RunAsync` строит dataset и вызывает `machine.TrainBatchAsync` после count gate: `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:189`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:216`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:241`. `DesignConversionMachine.TrainBatchAsync` строит local learned-weight deltas и меняет локальное состояние: `44.DesignConversionDomain/DesignConversionMachine.cs:67`, `44.DesignConversionDomain/DesignConversionMachine.cs:84`. Checkpoint save/load есть в домене: `44.DesignConversionDomain/DesignConversionMachine.cs:295`, `44.DesignConversionDomain/DesignConversionMachine.cs:339`. Runtime flow может использовать loaded local weights: `44.DesignConversionDomain/DesignConversionMachine.cs:283`. Но код сам маркирует это как local checkpoint feature adjustment, not trained model authority: `44.DesignConversionDomain/DesignConversionMachine.cs:17`, `44.DesignConversionDomain/DesignConversionMachine.cs:19`, а boundary decision не заявляет real training/checkpoint/promotion: `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:313`. | partial. Route A присутствует как локальный domain lifecycle/checkpoint contour и уже ближе к product-connected path, но это scaffold/local feature-adjustment, не доказанная trained model authority. |
| Route B через `25.ModelServer -> DesignConversionDpoTrainingHandoff -> 11.DPO/DpoTrainer` | `DpoTrainer` содержит реальный DPO loss/optimizer: `11.DPO/DpoTrainer.cs:31`, `11.DPO/DpoTrainer.cs:54`, vector path: `11.DPO/DpoTrainer.cs:169`, `11.DPO/DpoTrainer.cs:209`. Но trainer сохраняет только metric json, не checkpoint: `11.DPO/DpoTrainer.cs:323`. Handoff принимает rows, models, tokenizer, checkpoint directory и вызывает `LogicalDpoTrainingUnit`: `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs:39`, `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs:88`, `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs:97`. В разрешенных файлах единственные hits `DesignConversionDpoTrainingHandoff` находятся в самом handoff file; product intake route вызывает `DesignConversionTrainingRunner`, а не handoff: `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:772`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:866`. | real math, but product-connected save/load/use not proven. Route B нельзя принять как default execution route без явного adapter slice и checkpoint contract. |
| Откуда product path берет DPO rows | Approved rows идут из local artifact DB: `QueryApprovedTrainingCandidatesAsync` фильтрует approved candidates: `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:549`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:561`; handoff export берет только approved runtime candidates: `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:573`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:586`; training intake persist/readback идет через `training-intake-requests.jsonl`: `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:608`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:646`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:653`. Tests подтверждают local JSONL DB и approved rows: `Tests/DesignConversionDpoStorageTests.cs:49`, `Tests/DesignConversionDpoStorageTests.cs:97`, `Tests/DesignConversionDpoStorageTests.cs:362`, `Tests/DesignConversionDpoStorageTests.cs:408`. | product path rows are durable/current local artifact DB rows after approval/intake. Synthetic/fixture rows есть в tests, но они не являются доказательством real product data. |
| Held-out evaluation | Dataset builder делает train/held-out split and count gate: `44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs:432`, `44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs:439`, `44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs:446`. Pipeline передает held-out row ids в local continue-training probe: `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:448`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:454`. Measurement gate исполняется только если есть request and continue-training pass: `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:611`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:621`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:649`. `DesignConversionMachine.EvaluateAsync` возвращает scaffold note and `PassesGate: false`: `44.DesignConversionDomain/DesignConversionMachine.cs:112`, `44.DesignConversionDomain/DesignConversionMachine.cs:118`. Test показывает `HeldOutRecordUsageCount` equals 0 in continue proof: `Tests/DesignConversionModelLifecycleTests.cs:54`, `Tests/DesignConversionModelLifecycleTests.cs:64`. | real product-connected held-out evaluation not proven. Есть split/count/measurement-gate scaffold, но не доказан actual held-out evaluator on product route. |
| Promoted checkpoint in server runtime | Loader fresh-loads local checkpoint and builds promoted model only from local receipt: `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:33`, `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:109`, `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:123`, `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:134`. It can create internal service from promoted machine: `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:166`, `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs:175`. `DesignConversionModelServerApi.CreateMachineBackedLocal` uses `machine.CreateRuntimeRoutingFlow`: `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:143`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:159`. `SubmitAndEvaluateAsync` builds result with `TrainedModelAuthority: false` and `ProductReady: false`: `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:172`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:539`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:546`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:554`. Test manually constructs service/API with promoted model: `Tests/DesignConversionModelLifecycleTests.cs:96`, `Tests/DesignConversionModelLifecycleTests.cs:100`, `Tests/DesignConversionModelLifecycleTests.cs:105`. | fresh-load into an in-process manually constructed API is proven by tests. Automatic product server startup/load/use is not proven. |

Route verdict: default execution route is unresolved after Pass 2. Route B remains a real-math candidate but is not the current product-connected route. Route A is the current local product-connected fallback contour, but it is partial and explicitly not trained authority. Until a later pass chooses and reconciles a single spine, implementation backlog must not claim Route B default, ProductReady, deploy/cloud readiness, or trained model authority.

Next concrete product slices after the 50-pass plan phase:
- Slice 1: route decision repair - choose one product spine in the plan: either wire Route B into training intake and checkpoint lifecycle, or formalize Route A as the bounded local training spine and mark Route B as later.
- Slice 2: approved intake to training - prove local artifact DB approved rows flow through `CreateTrainingIntakeAsync` -> `ResolveTrainingIntakeRecordsAsync` -> runner with source-disjoint enough rows.
- Slice 3: held-out gate - replace fixture/manual held-out metrics with product-connected evaluator or explicit accepted local measurement provider, including source-disjoint ids.
- Slice 4: checkpoint lifecycle - save checkpoint, fresh-load, continue-training, then attach manifest/hash to a runtime service path without manual test-only construction.
- Slice 5: server runtime use - start a fresh `SubmitAndEvaluateAsync` route from the loaded/promoted machine and prove output/score/choice changed because of the checkpoint, while keeping `ProductReady=false` until cloud/deploy/CI/public endpoint evidence exists.

Downgrade: да для product readiness and trained-model authority. Нет для узкой цели Pass 2 после проверки файла: route audit and backlog update are recorded in this plan artifact.

## Карта фаз

| Фаза | Что закрывает | Главный риск сейчас | Что должно стать acceptance evidence |
| --- | --- | --- | --- |
| Intake/platform submit | Пользователь отправляет html/css/source tree/original tree/screenshots/design renders. | Default submit может быть мертвым из-за пустого Flow47 BaseUrl и FLOW47_BASE_URL_REQUIRED. | Fresh submit proof с валидной конфигурацией или понятной recoverable ошибкой до принятия job. |
| Server routing to model | Платформа передает задачу в model/server route. | Нет доказанной worker/queue durability; один synchronous resolution может вести к quarantine. | Durable job id, retry/idempotency/status readback или явно принятый synchronous SLA. |
| Render QA | Сервер делает реальные render screenshots в нужных разрешениях. | Production DI может давать metadata render, а не Chromium/browser pixels; MaxRenderQaRuns=1 недостаточен. | Две или больше нужных resolution, nonblank pixel evidence, browser-backed route, matching manifest. |
| Visual judge | Сравнение source/design/generated screenshots через visual judge. | Judge может быть echo constant/local stub или недоступный production vendor. | Принятая provider policy, score, threshold, cost/redaction, defects and reason. |
| Improvement loop | Система меняет candidate до threshold/cost stop. | Loop может быть отсутствующим или no-op в user path. | Attempt 1 below threshold -> findings -> changed candidate -> persisted attempt -> accepted stop reason. |
| Persistence DB | Результаты и learning evidence сохраняются. | Special DB не production default; может быть opt-in temp/hash-only. | Durable DB decision, restart survival, migration/version, query/readback for DPO. |
| DPO build | Из DB строятся chosen/rejected pairs. | Пары могут быть weak/synthetic; source-disjoint и high-quality negative не доказаны. | Approved rows, machine-internal vector, source/result fields, held-out split, no synthetic shortcut. |
| Training/held-out | Тренировка действительно улучшает held-out. | Тренер test-only, starts from zero, train-and-discard, held-out не подключен. | Train from approved rows, save checkpoint, held-out Evaluate, before/after numeric gate. |
| Promoted checkpoint/server wiring | Новые веса попадают в server runtime. | SaveCheckpoint/fresh load/runtime use могут быть не соединены. | Manifest/hash, promotion/register, server fresh-load, runtime route uses promoted checkpoint. |
| R9/R10 user surfaces | Пользователь видит DPO usefulness/control/durability. | Display/control surfaces и durability still missing или local-only. | UI/API proof for current DB rows, metrics, approval/blocker state, rollback/load status. |
| Upload/deploy gate | Материализация, package restore, CI/deploy/public endpoint. | Нельзя смешивать local proof с cloud/deploy claim. | Только после authority: clean package/consumer restore, CI/deploy/public endpoint evidence. |
| Governance/skills repair | Правила не дают повторить false stop и mojibake. | Human gate, no-packet handling, plan-only vs product execution confusion. | Отдельные правки skills/guides/agent files с readback, marker scan и enforcement checks. |

## Приоритетная таблица пробелов

| Phase | Gap | Why it matters | Suspected files | Acceptance evidence | Category |
| --- | --- | --- | --- | --- | --- |
| Training/held-out | P0 training contour не соединен с production route. Pass 2 уточнил: Route A partial local contour, Route B real math but not product-connected. | Без single spine обучение может не менять пользовательский outcome. | `11.DPO/DpoTrainer.cs:31`, `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs:97`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs:241`, `44.DesignConversionDomain/DesignConversionMachine.cs:283`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs:546` | File:line-backed proof: approved DB rows -> chosen route -> train -> checkpoint saved -> fresh load -> held-out -> runtime score/choice changes in `SubmitAndEvaluateAsync`. | product code, QA |
| DPO build | Route decision unresolved after Pass 2: do not default Route B yet; Route A is fallback/current local scaffold. | Нельзя выбрать implementation path без single-valued route decision. | `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:772`, `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs:866`, `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs:39`, `11.DPO/DpoTrainer.cs:323` | Route decision with exact refs, one product spine, no hidden parallel architecture, no train-and-discard. | architecture, product code |
| Visual judge | Quality ceiling 0.60: learned adjustment +/-0.2 over echo score 0.40 below 0.80-0.90. | Даже обученный scorer может не пересечь acceptance threshold. | `DesignConversionMachine.cs`, `QaProviders.cs`, visual QA policy files | Numeric before/after on held-out data reaches threshold, or scoring architecture is repaired and remeasured. | product code, QA |
| Intake/platform submit | Flow47 BaseUrl empty / FLOW47_BASE_URL_REQUIRED. | Default request can die before reaching model route. | Platform/FLOW47 configuration and submit handler files | Default config proof or actionable startup/submit failure before job acceptance. | product code, QA |
| Server routing to model | No worker/queue; one resolution and MaxRenderQaRuns=1 cause quarantine. | Product route can stall or always quarantine valid work. | ModelServer improvement loop handler, render QA policy/config | Durable job/status or accepted synchronous SLA; required resolution count in product path. | product code |
| Render QA | Production DI metadata render, not Chromium. | Visual QA without pixels is not visual proof. | Local service composition, browser QA providers | Product route metadata and artifacts prove browser-backed nonblank screenshots. | QA |
| Visual judge | Echo constant or unreachable production vendor judge. | Score cannot be trusted and threshold cannot prove quality. | Visual judge/provider abstraction files | Accepted local policy or real provider execution with threshold/cost/redaction evidence. | product code, QA |
| Improvement loop | Missing or no-op improvement in user path. | One-shot output does not satisfy iterative quality improvement. | Improvement loop coordinator, model server handler | Below-threshold run produces changed candidate and persisted second attempt. | product code, QA |
| Persistence DB | Special DB not production default; opt-in temp/hash only. | Learning data may disappear or never be used by product. | Local artifact DB, DPO storage, training pipeline storage | Durable DB policy, restart proof, current rows visible to DPO/training route. | product code |
| Training/held-out | Training weak/synthetic or starts from zero; held-out not production connected. | Test-only training cannot justify trained readiness. | DPO dataset builder/storage, training pipeline, held-out evaluation | Approved source-disjoint rows, held-out manifest, saved checkpoint, no replay or synthetic shortcut. | QA, product code |
| Promoted checkpoint/server wiring | Checkpoint not uploaded/loaded by server runtime. | Training may be train-and-discard. | Model repository, checkpoint manifest, server startup/runtime route | Server fresh-loads promoted checkpoint by hash/manifest and route uses it. | product code, QA |
| R9/R10 user surfaces | Display/control surfaces and durability missing. | User cannot inspect usefulness, control training, or trust durability. | DPO usefulness API/UI, control endpoints, status surfaces | Current DB rows visible with usefulness, approval/blocker state, training controls, load/rollback status. | product code, UX |
| Upload/deploy gate | ProductReady/cloud/deploy claim not supported. | Local proof is not public endpoint, CI, PR, deploy or provider readiness. | Packaging/materialization/CI/deploy docs and scripts | Authorized clean package restore, CI/deploy/public endpoint proof only when actually run. | QA, release |
| Governance/skills repair | Mojibake/human gate did not prevent unreadable Russian. | Human-readable plans and statuses can become unusable while marker-only scans pass. | `AGENTS.md`, `docs/skills/output-readability-gate-SKILL.md`, startup/review skills | Changed-line readback in readable Russian plus marker scan for corrupted text. | governance |
| Governance/skills repair | False stop after governance and no-packet handling remain risky. | Missing worker packet or governance-only fix can be mistaken for product progress. | Startup, plan-review, code-execution, generated-code-review skills | Final/status rejected unless next product slice/pass is named or true external blocker is proven. | governance |

## Pass 2 route audit

Pass 2 changed the route decision from "Route B default candidate" to "route unresolved". Evidence boundary:
- Route A is not absent: domain pipeline calls `DesignConversionMachine.TrainBatchAsync` and can save/load local checkpoints, but code labels the result as local checkpoint feature adjustment and keeps trained authority false.
- Route B is not wired as the product route in the allowed files: `DesignConversionDpoTrainingHandoff` has real DPO math behind it, but the training intake route runs `DesignConversionTrainingRunner` with `DesignConversionMachine`.
- Product rows are approved local artifact DB rows and training-intake records, not a direct synthetic shortcut; real product data remains not proven.
- Held-out evaluation is split/gate/probe level, not a product-connected evaluator proof.
- Promoted checkpoint use is proven only for a manually constructed in-process test service/API, not as automatic server startup/runtime loading.

## Route choice for execution

Recommended default route after Pass 2: unresolved. Do not claim Route B as the execution default yet.

Near-term planning rule: the 50-pass plan phase must choose one single spine before product execution:
- Option A fallback/current spine: continue from `44.DesignConversionDomain` local training lifecycle because it already touches training intake and runtime flow, while explicitly preserving "not trained authority" until held-out/checkpoint/runtime proof closes.
- Option B bridge: wire `DesignConversionDpoTrainingHandoff` into the approved training-intake route with a checkpoint save/load contract and prove `SubmitAndEvaluateAsync` uses the result.

Acceptance for any chosen route:
- approved current DB rows feed dataset build;
- source-disjoint train/held-out rows exist and are counted;
- training saves a checkpoint and fresh-load proves compatible state;
- held-out evaluator runs in the same product-connected route;
- promoted checkpoint changes runtime output/score/choice through `SubmitAndEvaluateAsync`;
- `ModelUploadReady`, deployment, public endpoint, CI and cloud remain false until those exact gates run.

## Governance repair queue

| Queue | Repair needed | Why | Acceptance evidence |
| --- | --- | --- | --- |
| GQ-1 | Mojibake/human-gate repair. | Russian plans/statuses must be readable, not merely marker-clean. | Direct changed-line readback plus scan for the configured mojibake marker set; corrupted attachment text must be labelled as source corruption, not copied. |
| GQ-2 | False stop after governance repair. | Rule presence or no-change governance result is not product execution. | Status/final must name next product/review slice or true external blocker. |
| GQ-3 | Sub-agent packet/no-packet handling. | Lost, interrupted, silent or no-output workers are zero accepted work. | Packets require `UNKNOWN_IN_SUBAGENT` when id is hidden, skill_load_status, changed_files, verification, count boundaries. |
| GQ-4 | Plan-only vs product execution confusion. | This file is a plan artifact; it cannot close product code, QA, cloud or deploy. | Active artifacts keep NOT COMPLETE until execution/QA/deploy routes actually run. |
| GQ-5 | Exact 50-pass evidence. | One ledger or one worker cannot count as 50 passes. | Each pass has exactly one section, model/effort, worker identity, verification, no conflict markers, no mojibake. |

## Next pass focus

Pass 3/50 should close the route-decision design gap without editing product code. It should pick the smallest plan-safe spine for execution backlog: Route A fallback/current local lifecycle or Route B bridge into training intake. It should update the plan with one route owner, exact files to edit after the 50-pass phase, acceptance evidence, and rollback if the chosen route cannot satisfy held-out/checkpoint/runtime proof.

Question for Pass 3: which single route should be carried into the execution backlog, and what exact product slices prove it without ProductReady/cloud/deploy overclaim?

Expected Pass 3 output: one route owner, explicit non-owner route status, exact backlog slices, acceptance evidence, and no product execution claim.
