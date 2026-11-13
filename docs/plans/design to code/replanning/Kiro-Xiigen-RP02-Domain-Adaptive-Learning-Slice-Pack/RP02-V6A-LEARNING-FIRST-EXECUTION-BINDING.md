# RP02 V6A — первый реальный E2-урок и проверка трёх доменов
**14.10.2026 · ACTIVE дополнение существующего V6, 17 карточек; документация, НЕ исполненное обучение.** Этот файл не создаёт новый слайс или дополнительный gate. Единственный исполнитель RP02 — `Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack`; одноимённый Retrieval-пакет — REFERENCE_ONLY. Если старые V2–V5/QUICK-START требуют C06 перед C03 либо Windows/coverage для обучающей системы, они исторические. Требования к лицензии, tenant, source rights, безопасности и будущей Windows-поставке сохраняются.
## 1. Подтверждённые по коду K исходные факты (проверить HEAD заново)
* `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs::ExecuteAsync` вызывает `Search(query,_k)`, но выдаёт `hits[0]` как типизированный `TOutput`; это memory-example consumer, а не доказательство POS/next-token predict. `Train` добавляет в индекс каждый элемент, а в dictionary пишет ключ `ExampleId.Trim()` — повторяющиеся ID требуют расследования.
* `18.CapabilityTraining/ReplayFusionRanker.cs::Rank` использует статические коэффициенты 0.45/0.35/0.20 и тип `GeneratedCapabilityCheckpointManifest`. **Нельзя** автоматически обучать этот тип парами документов `KnnLogicalUnit`. Прежде чем менять w, доказать совпадение типа кандидата, действительного обычного consumer и обновляемого состояния.
* `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs::NormalizeRoots` использует `[^a-z]+`, то есть не сохраняет кириллицу в **этом** encoder. Выяснить, использует ли его настоящий EN→RU consumer. BPE+predicate KnnLogicalUnit и 60-slot TrainingVectorBuilder — разные пространства, даже если размеры когда-либо совпадут.
* `DomainClassifierUnit` считает `CODE_Q/BUG/PLAN/ARCH` видами задач; `CODE_Q` НЕ означает subject=C#. Домейн должен иметь независимые subject, intent, language direction, dialect/version, stack, UNCERTAIN/MIXED.
## 2. Самый короткий путь к подтверждаемому обучению
**D0, независимые запускаемые NOW без кода:** 00A трасса K→caller→ordinary answer; 00B1 C# AST/nullable source-family exam; 00B2 EN→RU polarity/POS/coreference и проверка Unicode path; 00B3 SQL NULL/LEFT JOIN третьего домена (не блокирует первый); 01D semantic law feature source; 02B1/S найти настоящий typed scorer; 03C preregister hybrid 2×2. Каждое задание читают через WorkLeft-RAG-MASTER→usage→matching MD+NDJSON row ID→реальный первоисточник G/F/V, а не только R01–R25 или S04. R07=REPAIR_REQUIRED при отсутствии исправленного доказательства. Старт без новых продуктовых тестов.
**L1, первый обучающийся E2:** RP02.02B1/I **только** когда найденный нормальный consumer реально принимает один `domain-law-eligible` тип кандидатов, есть лицензированная same-query preference `(q,z+,z-)`, согласованы K effect/rights и ограниченный бюджет. Pairwise scorer `w_d`: сохранить до/после checksum, finite bounded optimizer step, учёт единственного применения пары и cold loader. Если нет существующего mutable recipient, при специальном K grant добавляется минимальное обучаемое состояние ЗА РЕАЛЬНЫМ CALLER; без grant `NO_TRAINABLE_RECIPIENT` с точным методом, но не новый generic trainer и не фиктивная тренировка. **02A/C06 НЕ обязательное условие 02B1**. RP01 Student DPO/LoRA тоже выполняется независимо.
**E, сразу после L1:** независимый 03A сравнивает P=старое поведение; C0=исправленный код, но НЕТ обновления обучаемого состояния; T=тот же код/config/candidates, но одно настоящее изменение состояния. C0−P инженерная поправка, T−C0 кандидат на эффект обучения. Если нет реального update или обычного output, `NOT_TRAINED/NOT_WIRED`, не успешное обучение.
**Параллельный предметный интеллект:** 01D→02D: условные POS/probability, syntax/dependency, same-vs-different referent/gender, negation scope, C# AST/operator/nullable, SQL 3VL/dialect, `φ_d` version и `k_d(context,entropy,density,law_risk,time/RAM)`. k=0,1, many или ABSTAIN. Жёсткий запрет только по установленному праву/закону; неоднозначная грамматика мягкая. Для EN→RU реально устранить потерю кириллицы только если подтверждён relevant caller и выполнить versioned reindex. 02A C06 typed multi-hit — **отдельный опциональный инженерный эксперимент**; 01A/C01,01B/C02,02B2/C04,02C/C05 — собственные однофакторные сравнения.
**Междоменная проверка:** первый C# ИЛИ EN→RU пилот; затем C# и EN→RU отдельными слепыми семьями; SQL третий до заявления о трёх доменах; B0→B1→B2 для retention. Если нет 3 независимых доменов — PILOT_ONLY, а не multi-domain success. RP03 владеет `ILearningUnit` typed many→many и `ILearningFlow` обучаемыми коррекциями; RP04 — async `ILearningOrchestrator` и higher-order `ILearningDynamicGraph`. RP02 не дублирует их до первого урока.
## 3. Формальная цепь deterministic + learned
```text
raw user/context
 → map (subject, intent, direction, dialect/version, uncertainty)
 → φ_d (role/POS, dependency, referent, polarity, AST, SQL-law, coordinate_version)
 → tenant/rights/source/type/schema eligibility
 → exact/ANN candidate pool
 → ESTABLISHED hard-law veto, unknown means alternatives/abstain
 → learned domain rank w_d + calibrated conditional grammar score
 → dynamic k_d(ctx, law risk, margin, entropy, density, CPU/RAM)
 → type-correct reducer/decoder → ORDINARY consumer
 → lawful consented pair → once optimizer update / checkpoint / cold reload
 → independent blind heldout domain-law exam and full PC resources
```
**Не смешивать:** RP01 response DPO reference-policy/real Student LoRA ≠ RP02 pairwise retrieval ranking objective ≠ static hard domain arbiter. LoRA matrices не становятся semantic token без actual projection/decoder/index loader. 03C 2×2 проверка взаимодействия только при существовании обеих arms.
## 4. Минимальное тестирование и научная приёмка
| Работа | Новые code tests | Как установить пользу |
| --- | --- | --- |
| READ_ONLY/документы/изучение кода/проведение слепого экзамена | **0** | первичный текст, exact K caller/consumer, raw answers |
| Без изменения deterministic boundary | **0** | научное per-law heldout и p50/p95, full-PC cost |
| Изменение rights/tenant/type/law/version/journal/cold loader/finite-state | переиспользовать ближайшие существующие проверки, добавить лишь отсутствующие положительную+отказную *по каждому реально изменённому риску* | контракт не пропускает запрещённое состояние |
| Обучение w/LoRA/k-policy | НИКАКИХ % coverage или blanket Windows workflows | P/C0/T, источник пары, изменяемый вес, cold consumer, independent domain families, confidence/abstain/retention |
| Будущая release/build Windows-среда | только действительно требуемые native/security/release checks | поставка; НЕ метрика обучения |
Нельзя подгонять SEALED rubric к результату, менять параллельно candidates/Student/decoder, выдавать один положительный случай за статистическую обобщаемость. Примеры в [workbook](RP02-V6A-THREE-DOMAIN-EXAM-WORKBOOK.md) ОТКРЫТЫ и не могут быть SEALED. Записывать accuracy по доменным законам, false hard veto, lawful-best-path regret, p50/p95 wall, peak RAM/VRAM, disk/index/rebuild/teacher/judge/failed branches, интервал неопределённости, B0/B1/B2 retention. Ноль полезного прироста и доменное нарушение — валидный результат эксперимента, не причина чинить тест.
## 5. Действующий компактный промпт на ОДИН запуск
```text
ROLE: small-model RP02 executor. TASK: ONLY RP02.<ID>, current V6A front matter of tasks/RP02.<ID>.md. NOT TASK: all 32 slices, R01–R25 relaunch, global Windows/coverage, generic model retrain.
READ: pin P llm_plan3 / K codex/dpo-implementation exact HEAD/blob; selected card in full; task-scoped WorkLeft RAG MD+NDJSON→G/F/V primary; real K file+callers+normal user consumer; actual K/docs/skills architecture, planning, plan-review, code-review, prompt, human as stage requires, record sections READ.
EXECUTE: follow the task's specific numbered objective, one learned/deterministic factor; do not invent APIs, effects, model calls or skill loads. Freeze source family/rights and GOOD/BAD/EDGE. S source (READ_ONLY) / I one authorized code effect / E independent blind exam only when actual scope demands, each child gets inherited full task+outcome+why+failure/repair.
VERIFY: 0 new product tests if read-only; nearest narrow touched deterministic invariants if code changed; stochastic ability = actual frozen independent ordinary P/C0/T results, law/accuracy/time/full PC, no sealed leakage.
RETURN: input→method→caller→normal output or NOT_WIRED; candidate type; rights, before/after state SHA or NOT_TRAINED; GOOD/BAD/EDGE+falsifier; code diff/no-change and narrow checks; independent exam metrics/NOT_MEASURED; exact branch SHA/readback if written; one next owner.
```
No product-code change, training, PC benchmark, reviewer invocation or gains are implied by publishing this **plan**.