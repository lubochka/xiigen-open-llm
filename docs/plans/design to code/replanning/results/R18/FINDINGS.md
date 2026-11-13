# R18 — результаты: точный урок, допустимые стадии и обычный ответ

**13.10.2026 · RCOR-2026-10-13. Текущий результат согласован с редакцией2 основного [work order](../../R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md), не только с прежним CP2.** Current status: `RESULT_RECONCILED / EDITORIAL_REPAIR_PUBLISHED_DRAFT / DESIGN_BINDING_REQUIRED / RUNTIME_BINDING_OPEN / LEARNING_NOT_MEASURED`. Статусы относятся к указанным ниже границам, не общему запрету независимой подготовки. Independent review NOT_PERFORMED.

Первоначальные F18-01…08, их source IDs и code evidence сохранены. Новые F18-09…12 — **интеграторское чтение опубликованной редакции2**, не новые собственные core traces. Полная прежняя версия результата: `18d91cba0365c3adb66c7b929ced690f2159a224`, blob `4d5aa9963ccd4c48cc3b35c069ee5dda12b4db79`. Архив основной редакции1: `R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.v1.md`, blob `af505592907a43dc2022e22854e2b71f23a7e276` по свидетельству редакции2. Её существование здесь прочитано из current work order, не новый собственный byte-equality audit архива.

## Итог человеческим языком

Существующий CodeDocumentation Student, trainer, попытки, checkpoints, recovery и TextGenerator переиспользуются. Проект исправляет exact material→conditional objective→разрешённые параметры→isolated candidate examination→выбранное состояние→обычный пользовательский ответ. Готовые A–D — поручения будущим исполнителям, не четыре выполненных model calls.

**Теперь results отражают также исправления редакции2:** один exact OwnerInput, shared generation facade без project-reference cycle, допустимая явно разрешённая initial-baseline ветка и стадийные G1–G6. Не требовать будущий response ID до authoring или post-update adoption до самого update. Не обходить реальные права, состояние и критерии.

Продуктовый код/модели/обучение/CI/Windows/измерения/независимая Astra-экспертиза ни этой RCOR-коррекцией, ни прочитанной редакцией2 не выполнены. Стоимость/точность/память/learning gain остаются NOT_MEASURED.

## Сохранённые code findings первоначального R18

### F18-01. Exact raw admission не гарантирует faithful token material

K02 `BuildBatch` токенизирует Compact: whitespace схлопывается, каждая часть ограничивается480 символами. Содержательный suffix может исчезнуть после raw assessment. Это bounded code observation/risk, не измеренная доля испорченных пар.

**Current design:** immutable segmented-byte-BPE-v2; exact prompt/response encoding, byte roundtrip, masks и capacity проверяются до effects. Overflow — явный отказ либо отдельный новый assessed lesson, не скрытая обрезка старого. Новая редакция дополнительно фиксирует x как OwnerInput (§F18-09). Actual implementation не заявляется.

### F18-02. Full-sequence mean отличается от выбранной response sum

K04 real-token masks и K05 masked mean/count-backward включают prompt из K02. Public EvaluateAverageNextTokenLogProbability использует общий helper.

**Current design:** отдельная versioned response-only sum ветка forward/backward; public average scorer не меняется. Prompt остаётся conditioning и участвует в вычислении hidden state; исключаются только прямые prompt targets. Никакого all-ones fallback при missing response masks. Изменения кода общие всем экспериментальным arms, их совокупный эффект не приписывается одному градиентному шагу.

### F18-03. TeachingVector в этом consumer не semantic output projection

K05 VectorSignal считает число активных координат и множитель1–2; gradient использует vectorWeight*vectorSignal, reported loss только vectorWeight. Перестановка активных координат не меняет множитель. Не утверждается отсутствие других vector uses.

**Current design:** weighting зафиксирован, raw и реально оптимизируемый weighted loss различены; отдельный weighting/projection опыт не смешивается с E1. LoRA и смысл source→target representations имеют разные контракты.

### F18-04. Actual recipient связан с установленными ключами, не названием LoRA

K05 optimizer использует actual RequiresGrad; K02/K06 задают adapter topology/independent reference. Полный actual selected-parent trainable key set не был наблюдён.

**Current design:** project-LoRA-only E1, installed A/B keys сверяются с manifest/allowlist; base/reference frozen, storage/optimizer/RNG compatibility. Existing freeze/install/filter переиспользуется; actual receipt G2 получают через допустимую inspection stage до мутации. ADAPTER_ONLY_CONFIRMED не ставится по одной конфигурации. Равные weights при разных dropout/training modes не гарантируют равный stochastic score.

### F18-05. Existing lifecycle сохраняется

K07 already-applied/resume/baseline/executor/trial-reopen и K01 logical request показывают существующие механизмы, не successful learning run. Reuse, versioned material/objective compatibility и полный applicable state; UNKNOWN update → reconcile, не новый step или переименование пары. Изолированная оценка trial предшествует принятому serving state.

### F18-06. Generation kernel найден; ordinary service остаётся предлагаемым

K09 AssessAsync вызывает TextGenerator; K10 GenerateCore выполняет real prefill/decode/cache/sampling/seed/cancellation. K11 routing даёт route/stages, K12 прочитанная map assessment/training/preflight не подтверждает готовую обычную выдачу.

**Уточнённый design C:** shared facade в project14 поверх kernel09; wrapper в26, local adapter в25; assessment и ordinary route вызывают вниз общий kernel, не импортируют26 в14. Это **предлагаемый** bridge, не найденный готовый endpoint. Input contract/source scope, selected-state handle/cache/response identity, trial isolation и G5-pre/G5-post явны. Не строить второй generator или выдавать routing receipt за текст.

### F18-07. EOS нельзя выдумать по последнему vocabulary ID

K03 EosToken=VocabCount−1, K10 legacy stop; dedicated special-token contract в прочитанном scope не установлен. v2 не добавляет guessed EOS; detailed overload использует manifest-verified stop либо explicit LENGTH_LIMIT. Legacy overload сохранён. Кодовый migration общий всем arms, не доказательство эффекта DPO.

### F18-08. Protected local host и GitHub Windows gate различны

K12 owner-pinned C:\Projects\llm_mvp_core нельзя объявить готовым на произвольном clone/OS. Native evidence — по actual protected property; local constraint не вводит broad CI перед каждым lesson. Документы не изменяют workflows/rulesets и не доказывают отсутствие required statuses. Старые количественные R09 агрегаты теперь source-qualified, не основание экономии.

## Новые принятые передачи редакции2 — не новая code-execution история

### F18-09. Один exact input предотвращает несовпадающее conditioning

Редакция2 §3.1 выбирает `owner-input-verbatim-v1`: x — immutable OwnerInput без добавленного после assessment шаблона/пробела. Domain/source/association context связан до authoring; metadata не является невидимым conditioning для Student. Proposed input carrier и versioned routing adapter предотвращают два независимо меняемых CodeText/OwnerInput; mismatch — отказ, не guessed rendering.

Сохраняется segmented-byte-BPE-v2, p=Encode(x), a=Encode(y), explicit prefix/loss masks. Это проектное решение current packet, не свидетельство уже реализованного публичного входа. Исходные finite capacity/rights/judgment правила не ослаблены.

### F18-10. Shared generation должен исключать project-reference cycle

Редакция2 §2/packet C сообщает прямо прочитанные её автором зависимости:26→14 и14→09, без14→26. Принята предложенная архитектура facade14/kernel09/wrapper26/adapter25. Эти csproj reads **атрибутированы автору редакции2**, не добавлены как собственные core reads интегратора. Actual implementation/dependency verification остаются будущим code scope.

### F18-11. Допустимый initial baseline не обязан иметь выдуманный accepted manifest

Редакция2 сохраняет ветки existing StartService: verified accepted selection либо clean missing + применимое explicit cold-start authorization. Повреждённый/несовместимый checkpoint не становится silent random fallback. Если конкретный S04 запрещает cold start, эта ветка не допускается. Initial baseline получает честный source-kind/identity, не поддельный accepted-parent passport. Достаточный baseline допускает NO_TRAINING_NEEDED без teacher/pair/update.

### F18-12. G1–G6 — зависимости стадий, не круговой общий барьер

Сохранены ID с разделением предпосылок и последующих наблюдений:

| ID | До соответствующего действия | После действия, не его собственная предпосылка |
|---|---|---|
| G1 | Допустимая baseline selection/config/tokenizer/identity и applicable start authority | Actual loaded state receipt; не выдуманный accepted manifest initial root |
| G2 | Actual keys/freeze/storage/optimizer/RNG/recovery из допустимой inspection | Observed mutation/weights/journal только после разрешённого update |
| G3 | Actual expected profile/source rights/grant/expiry/budget до call | Teacher/judge request IDs и returned bytes после call; exact pair admission до optimizer |
| G4 | Выбранные panel/families/rubric/effect/noise/retention/N/looks/budget до outcomes | Scores/uncertainty после examination; missing numerical policy — DESIGN_BINDING_REQUIRED |
| G5 | Реализованный callable input/facade/isolated path и допустимая pre-update baseline observation | G5-post cold selected-state use после verdict, не prerequisite первого update |
| G6 | Applicable host/resource/review/native authority соответствующей стадии | Observed full peak/cost/adoption после effects; no invented independent-review receipt |

Read-only inspection, baseline observation, authoring/assessment, one mutation, isolated examination, promotion/adoption и remote release имеют разные permissions. Эта схема не выдаёт ни одного grant. Future response IDs и измеренная post-state usefulness не должны препятствовать действию, которое впервые их создаёт. Отсутствующая числовая design-policy блокирует dependent scientific run/decision, а не независимую подготовку A/C.

## Сохранение широкой программы

Исходные WorkLeft/S00–S10/extras, S06.08B/S06.09C, DPO/Graph/DTC/IDE, семь domains и дополнительные языки/intents сохранены. Unit many→many associations, Flow correction, async Orchestrator и higher-order Graph различаются; E1 не закрывает их learning. TensorCore/shared base/LoRA, stack-before-normalization, semantic order, BM25+kNN, positive/negative/warning memory и final arbiter остаются. Никакого скрытого historical70/30 replay или solver gain→selector credit.

Dynamic kNN ranking/k/neighbor use, law-sensitive representation и отдельная semantic projection требуют своих causal contrasts и actual consumers; small adapter не full PC budget. Bulk — новые lessons со stable anchors и independent families; mixed-domain/retention/allattempt-cost отдельно от средней точности.

## Текущая приёмка и остаток

R12 conditional design и R18 kernel/patch design уже существуют — их не исследуют повторно из-за старой надписи. R13 full compiler и numerical/runtime bindings, implementation A–C, реальные profile/source/grants/keys/caller и applicable independent review ещё не закрыты. G4 missing numerical decisions нельзя назвать только runtime metadata; реальные returned request IDs не invent заранее.

**RCOR выполнен:** согласованы сами пять carriers с действующей редакцией2, первоначальные IDs/evidence/history сохранены и отделены от нового attributed handoff. Результат не FIRST_LESSON_EXECUTION_READY, не выполненное обучение, не признание алгоритма лучшим и не независимый Astra PASS. Документный repair, underlying code work, admissible real observation и measured gain — разные состояния.
