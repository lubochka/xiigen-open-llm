# R22 — что исправлено в исследовании гибридных алгоритмов

**13.10.2026 · RCOR22-R2 · текущий результат: EDITORIAL_REPAIR / DESIGN_CANDIDATE / DESIGN_BINDING_REQUIRED / RUNTIME_BINDING_OPEN / LEARNING_NOT_MEASURED / INDEPENDENT_ASTRA_REVIEW_NOT_PERFORMED.**

Исправлены [действующий каталог](../../R22-CAUSAL-EXPERIMENT-CATALOG.md) и [промпт R22](../../run-prompts/R22.md), а не только формулировка статуса. Предыдущий R22 уже содержал полезные 11 сравнений и реальные ограниченные code traces. Проблема была в неполном согласовании с R18/R13/R21, ложной линейной зависимости от E1, смешении проектной и runtime-готовности и недостаточно различающих деталях отдельных сравнений. **Новые метрики, пары, веса и результаты обучения этой редактурой не получены.**

Предыдущая версия этого файла: P=`e96a727053f6153455e979bf76ab4714d8728aa2`, blob `2b886503233c73768df69ebf51688350e6c0a126`. F22-01…09 сохранены ниже. Старые прямые code reads остаются свидетельством прежнего researcher; текущий интегратор не приписывает их себе как новый полный аудит. Полный исторический текст доступен по указанному commit/path.

## Адресный review и реально применённые исправления

| Repair ID | Найденная проблема | Исправление в действующем R22 |
|---|---|---|
| RCOR22-01 | Каталог/карточка/F22-08/D22-13 продолжали говорить, что R21 не найден | Прямо прочитан опубликованный R21. Приняты его C#→EN→RU progression, семь предметов и Builder; actual data/translator binding не объявлены готовыми. |
| RCOR22-02 | Заголовок каталога требовал выбирать Cxx только после E1 | Убрана общая зависимость. E1 приоритетен, но E2–E4 готовятся/выбираются по собственным prerequisites; R01–R20 independent starts сохранены. |
| RCOR22-03 | R22 ссылался на R18 G1–G6, но G5/G6 означали другое; preconditions включали будущие post-adoption факты | Согласованы G5=consumer, G6=environment/review. Разделены planning, read-only, baseline, author/judge, mutation, isolated exam, promotion, cold adoption и release. |
| RCOR22-04 | Старый/новый code pipeline мог стать скрытым вторым фактором learning comparison | O old-code diagnostic отдельно. Основное N→U сравнение: одинаковый исправленный v2 pipeline, memory/context, corpus и evaluator; только один permitted update. |
| RCOR22-05 | «Полный первый packet» оставлял N/эффект/шум/retention/стоимость будущим владельцам без конкретного выбора | Добавлен decision-ready PILOT-P12 proposal с конечной структурой и отдельное confirmatory предложение. Они НЕ approved profile/данные/ресурсы. Невыбранные policy values честно оставляют DESIGN_BINDING_REQUIRED; owner R13/R18-D, allocation R23. |
| RCOR22-06 | Не было явного сквозного matched-budget deterministic/learned/hybrid comparison | Добавлен C00 с common hard guards во всех arms и полной стоимостью подготовки/обучения/инференса. Недоступный arm ограничивает claim, не блокирует остальные опыты. |
| RCOR22-07 | Fixed nearest-set мог смешиваться с learned geometry; common retrieved set скрывал бы ANN omissions | C03 разделяет reranking общего pool и отдельное geometry comparison. C05 exact/ANN получают собственные sets при одинаковом corpus/score/consumer. |
| RCOR22-08 | Содержательные card prompts не полностью связывались с mode, source/skill excerpts, ordinary owner и bounded handoff | Встроены Q1–Q4 context packs, skill packs, полный child contract, A–E внутреннее разбиение и отдельные C00–C11 good/bad/edges/falsifiers. Нет title-only delegation, нового trainer или новых автоматических Rxx. |
| RCOR22-09 | Ordinary adoption и scientific outcome могли сливаться; одно наблюдение — с bulk/domain mastery | Раздельные isolated utility/promotion/adoption; один урок не bulk; stable operational panel не untouched confirmation; user feedback не ground truth. |

Доказательная основа этих исправлений — current source excerpts, перечисленные в [SOURCES.csv](SOURCES.csv), и exact applied documents. Это **авторский review**, не независимый запуск Astra и не empirical validation.

## F22-01 — материал пары до optimizer: сохранённое статическое наблюдение

**Унаследовано из исходного R22, а не заново трассировано этим интегратором.** K=`3c7f5f8c7809936e25579603c96ea6df8598fc40`: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs:BuildBatch/Compact` создаёт raw passport, но перед Encode сжимает whitespace/обрезает до 480 символов. Это риск потери оценённого различия, не доказательство уже случившейся испорченной учебной попытки или её частоты. R18-A проектирует exact immutable material, separate segments/masks и отказ при incompatibility. R22 сохраняет этот узкий путь, не предлагает просто увеличить 480 и не переписывает tokenizer глобально.

## F22-02 — существующий objective и causal baseline

Прежний R22 читал `11.DPO/DpoTrainer.cs:Step`, где есть policy/reference scores, backward и optimizer; сведения о prompt+answer average reducer были атрибутированы R05/R18. Current R18 v2 выбирает отдельный conditional response logsum и соответствующий backward, оставляя public average scorer. Это подготовленное решение, не текущая выполненная code patch.

**Новое исправление:** code/material/framing/consumer fixes общие для N, C0 и U. Основной вывод об обучении делается по N→U после одной новой пары. O→U, где O исполняет старый pipeline, смешивает correctness repair и learning. Низкий loss, hash или parameter delta не outcome полезности.

## F22-03 — генерация, recovery и действующий R18 v2

Исторический code evidence: `09.Inference/TextGenerator.cs:GenerateCore`, `CodeDocumentationSavedModelAssessmentService` и publication coordinator already-applied/recovery. Эти существующие механизмы — основания reuse, не доказанный ordinary endpoint или cold adoption.

Current R18 v2, прочитанный прямо как **design evidence**, уже проектирует shared generation facade в 14 поверх kernel 09, wrapper 26 и local adapter 25; не обратную dependency 14→26. `owner-input-verbatim-v1` сохраняет x от judgment до ordinary request. Accepted state либо явно разрешённый clean baseline; corrupt checkpoint не silent random start. Конкретный implemented path/loaded keys ещё привязываются.

**Новое исправление:** G5-pre baseline/isolated path предшествует update; G5-post cold adopted/retained observation следует после verdict. Не требовать будущую adoption для написания C или первого разрешённого baseline. `NO_TRAINING_NEEDED` остаётся законной веткой без пары/update, не состоявшимся обучением.

## F22-04 — четыре роли и смысл комбинации сохранены

| Роль | Что должно проявиться в независимой задаче | Различающий контроль |
|---|---|---|
| ILearningUnit | Lawful typed many→many и правильные input→result associations | Fixed connections; отдельно меняется mapping/adapter/ranker конкретного Unit |
| ILearningFlow | Правильная коррекция и доставка payload в следующий Unit | Frozen Units, changed Flow; reciprocal Unit-only control |
| ILearningOrchestrator | Выбор lawful asynchronous combinations при конечном бюджете | Те же branch opportunities/права; fixed versus learned policy; все branches оплачены |
| ILearningDynamicGraph | Новая higher-order composition знакомых примитивов | Frozen lower roles; unseen structural family versus flat/per-primitive comparator |

Train остаётся у ILearningMachine, Graph — L5 specialization; не четыре trainers и не шестая ступень. TensorCore/base/project LoRA, StackDetector-before-normalization, semantic order, BM25+kNN, positive/negative/warning memory, final arbiter и обязательные skills сохраняются. Отсутствующий trainable target — OPEN seam, не «все роли обучились после E1».

**C00 добавляет полный сравнительный вопрос:** D fixed solver/choice, L learned policy без исследуемого optional shortcut, H hybrid при одинаковой информации и ресурсах. Laws/type/rights/arbiter обязательны во всех arms. Correct deterministic result не rejected из-за происхождения. Learned forward может быть детерминированным; random output не доказательство learning.

## F22-05 — dynamic kNN разделён на реальные вмешательства

Унаследованное R06 evidence: exact/ANN пути и first-hit `TOutput` в current KnnLogicalUnit, а k hits ещё не learned aggregation. C01 mapping; C02 representation; C03 features/scoring; C04 k/radius; C05 index approximation; C06 actual neighbor use сохранены.

**Исправлена точность сравнения:** C03-rerank держит общий pool; C03-geometry допускает другие exact neighbors из той же исходной corpus geometry. В C05 собственные exact/ANN retrieved sets обязательны, иначе omission скрыт. Query-dependent score не автоматически допустимая index distance. Dynamic heuristic без update owner/signal не называется обученной функцией. Candidate score/feature policy проверяется законом домена и обычным ответом, не одним cosine.

## F22-06 — DPO role contract, LoRA и projection

Прямо перечитан active WorkLeft-02: CODEX с запрошенной разрешённой конфигурацией Astra UltraCode выбирает case и **отдельно** судит exact final pair/context. Разрешённая внешняя открытая китайская модель пишет ОБЕ final sides. Student weak answer — только разрешённый контекст. TIE/BOTH_BAD/UNJUDGEABLE не forced preference; изменённые bytes не наследуют прежнее judgment. Reference policy, selector, pair judge, Student и post-update evaluator — разные роли. Actual invocations в этом repair: **0**.

Response-conditional DPO sum/masks и consistent backward — конкретный objective первого Student. Arbitrary similarity не стандартная DPO probability. Equivalent sides, contrast destroyed by preparation и initial margin0 различаются. Correct deterministic solution может быть chosen. One new admitted pair — не более одного разрешённого update, UNKNOWN сначала recovery, не новый UUID.

C10 теперь явно отделяет actual project-LoRA adaptation от P_domain typed source→target vector/codebook projection. LoRA не tokenizer сама по себе; нужны отдельный objective/recipient/decoder/version/ordinary consumer и examination. Нельзя требовать, чтобы каждая LoRA matrix обязательно изменилась на первом шаге, или считать один scalar VectorSignal обученной проекцией.

## F22-07 — сравнимые наблюдения, bulk и полная цена

Каталог задаёт same corrected-pipeline N/C0/U, source-family partitions и frozen outcome rules. Законный лучший путь — множество решений и bounded comparator; для больших cases best observed не global optimum. Минимальные interactions representation×neighbors, Unit×Flow, Orchestrator×Graph только при допустимых независимых states/lineage/resources. Disjoint pair families не автоматически randomized/эквивалентные arms.

**Prospective profile уточнён:** PILOT-P12 proposal — 12 independent C# families ×2 cases; 72 основных P/C0/T generations плюс 24 заранее выделенных noise observations, максимум 96 ordinary generations. Это не готовые families и не весь API budget. Один author attempt/new pair/update максимум при допуске; no automatic TIE retry. Existing approved S04 profile имеет приоритет. Отдельный confirmatory proposal (N/meaningful effect/uncertainty) помечен как design candidate, не guaranteed power/owner-approved rule. Actual grants, retention tolerance, precision и whole-PC limits ещё требуют связывания до candidate outcome.

Измерения: per-family/domain law-valid essential claims, mapping, semantic loss, path/Graph transfer, retention, полный wall/phase time, branch/retry/recovery/reindex cost, model/reference/optimizer/activation/KV/LoRA/index/graph/cache simultaneous RAM/VRAM и persistent growth. Отдельные component peaks не measured concurrent peak. Local Student с внешним teacher/judge не fully offline learning.

Один урок — не bulk и не освоение семи предметов. Последующие bulks состоят из новых уроков; operational anchor отделён от untouched final; легче следующая панель не доказательство gain. User feedback проходит rights/truth/conflict/novelty. Isolated IMPROVE может сосуществовать с ADOPTION_NOT_ESTABLISHED; это не delivered improvement. Missing/insufficient evidence остаётся NOT_MEASURED/INCONCLUSIVE, не PASS.

## F22-08 — R21 теперь получен, а не отсутствует

**Предыдущее `R21 absent` superseded для текущей передачи.** Старый S22-28 сохраняется только как bounded historical directory lookup. Current R21 FINDINGS blob `ee3329f6ea4e9310e9b3e8e14c86acd43fbe89da` и DOMAIN-EXPERIMENT-RECIPES §9 прямо прочитаны. Принято: C# first, структурно другой EN→RU second; E2 independently preparable; Builder B0/B1 измеряет downstream Student, не качество JSON.

Семь предметов: EN↔RU; orchestration; Figma→HTML/CSS; C#, Angular, SQL documentation; DPO Builder. Для каждого сохранены laws/typed meaning/candidate kNN/good-hard-novel-TIE-OOD/examination. Остальные languages/intents/code/stored procedures, DTC/IDE и S00–S10/extras не удалены. Corpus/rights/family IDs/ordinary translator остаются unbound; materialized curriculum не выполненное обучение и не whole-library audit.

## F22-09 — что завершено и что остаётся

**Завершено в этом repair:** предметная редактура R22 с current receipts, corrected causal controls, stage-bound G1–G6, C00–C11, model task A–E, quantitative proposal и согласованными собственными carriers. Самопроверка документа не independent acceptance.

**Design binding:** R13/R18-D выбирает approved или предложенный prospective profile со всеми effect/noise/N/retention/allowed-look/call-budget rules; resource owner R23 задаёт реальный allocation. Это конечные owner-bound решения до candidate outcomes, не новое общее исследование и не всё на R24. Draft proposal не объявляется READY.

**Runtime binding:** actual authorized baseline/keys/reference/continuation; teacher/judge/evaluator profiles и grants/rights; lawful source families; implemented ordinary/isolated path; PC/host admission; stage-appropriate required independent review. Post-update observations появятся только после своих действий. Незавершённый общий R13 или R21 не запрещает независимую подготовку, но требуемый конкретному эксперименту контракт нельзя пропускать.

**Следующее назначенное действие:** минимальные R18 A/B/C под отдельным core scope + prospective profile и lawful family binding параллельно. При E1 blocker доступный E2 можно выбрать до outcomes по объяснённой цене знания и собственным prerequisites. Не цикл Windows/coverage, не повтор той же пары. R24 получает ordinary DTC/IDE output expectations; R25 — exact candidate independent examination. Ни один sibling status здесь не повышен.

### Documentary counterexample walkthrough — не продуктовые запуски

1. R21 существует, данных ещё нет → принять curriculum, не заявлять корпус/не блокировать E2 preparation.
2. Trial лучше, но старый code baseline сломан → learning attribution invalid; сравнить N→U на одном pipeline.
3. Cold adoption требуется до update → prerequisite cycle; разделить G5-pre/G5-post.
4. ANN использует exact retrieved set → retrieval misses скрыты; получить собственные sets и сохранить тот же downstream score.
5. Hybrid выигрывает только с большим corpus/teacher hints → не matched causal superiority.
6. Один family/несколько paraphrases/несколько seeds → не несколько independent families и не подтвердившаяся 95% accuracy.
7. Exact pair valid, keys updated, ordinary response не лучше → valid no-gain observation, а не повод повторять пару/ослаблять rubric.
8. Isolated gain есть, ordinary process грузит parent → ADOPTION_NOT_ESTABLISHED, не delivered IMPROVE.

**Фактические действия:** source/skill excerpts и документы; GitHub planning publication. Product training, teacher authorship, inference, tests/build/coverage/Windows/Actions dispatch, code/weights/dataset changes и independent Astra calls: **0**. Автоматическое поведение существующих workflows после GitHub commit здесь не наблюдалось и не выдаётся за отсутствующее или пройденное.
