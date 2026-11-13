# N56-02 — read-only аудит защищённой библиотеки экспериментов: охват, корректность, качество

**19.10.2026.** Результат независимой сверки **только чтением**. Запись исключительно в \`xiigen-planning/llm_plan3/.../replanning/possibleDifs\`. **НИ ОДНОГО изменения** \`llm_mvp_core/docs/plans/replanning-possible-diffs/**\`. **Новых native experiences: 0; новых измерений learning quality: 0.** Срез planning до N56: \`790bcf3e396411fb8d5a29504cdadc5dd7ef32fa\`; core: \`eb05219c27663835ce927c7bf97cf1274c07cddb\`.

## 1. Что на самом деле существует (а не «всё проверено»)

- [Защищённая \`experimets/00-README.md\`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/00-README.md) — **исторический** снимок исследования 18.10: 360 рекурсивно просмотренных файлов \`possibleDifs\`, 125 карточек \`P01–P125\`, 470 подслучаев. Это **исследовательские проекты**, а не 470 успешных запусков. Часть созданных позднее 19.10 материалов не могла входить в frozen input 18.10.
- [\`2026-10-19-small-learning-probes/01-method-and-report.md\`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/01-method-and-report.md) — действующая малая постановка: A0/A1/B0/B1, один фактор, default 2 урока/4 новых задачи, сохранение обычных ответов и расходов. **План, не результаты.**
- [\`results/2026-10-18/README.md\`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md) — реальные запускавшиеся опыты (P125, P25, P11, P86 и neural). У них указаны reviewers, точные границы, слепой/same-panel контролируемый материал, actual результаты и failures.
- Верхний уровень protected \`docs/plans/replanning-possible-diffs\` содержит **93 элемента** в просмотренном снимке; это не 93 эксперимента.
- Корневая \`possibleDifs\` до новых N56 содержала **479 непосредственных элементов**, из них **463 файла и 16 каталогов**; это не то же самое, что рекурсивные 360 H-files из прошлого frozen survey.

**Вердикт:** в protected library действительно есть развитые **проекты** корректных малых экспериментов плюс несколько опубликованных нативных результатов. **Не подтверждено**, что все текущие 19-Oct \`possibleDifs\` имеют отдельный эксперимент с доказанно вызываемым actual consumer, или что все 125 карточек исполнены.

## 2. Проверка соответствия обнаруженных деталей экспериментам

| Механизм / вопрос | Уже есть в read-only protected library | Насколько корректно описан? | Что остаётся открытым |
|---|---|---|---|
| Random B0 + DPO вместо исходной языковой компетенции | P02 \`warm-base\`; 04-documentation-neural; P01 recipient | **Design PASS**: equal DPO budget *и* equal total-cost — раздельные arms, actual free outputs. | Native head-to-head with truly trained base and ordinary serving **NOT_RUN здесь**. |
| Предпочтение, качество пары, контраст и length norm | P04, P05, P07, P14, P81 | **Design PASS**: separate pair preference, model-visible hashes/mask, heldout semantic exact obligations; source independent. | Результат конкретного RP01 multi-domain на same panel нет. |
| Нулевой/ошибочный backward, embedding, LoRA topology, optimizer | P01, P06, P10, P11, P12, P86, P87 | **Design PASS**: numeric FD probes/legality, rank/target, state, optimizer vs loss; **native P86/P11 component**, отдельные 8-step results дают 0/24→0/24. | Не приписывать LoRA repair исправление ответа. |
| Tokenizer, vocab identity, pair→actual Student, EOS/framing | P07, P17, P18, P22, P98/P99; новая planning N39 | **PARTIAL**: внутри отдельных карточек есть identity tests; [N39](39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) **нет буквального имени файла** в просмотренных summary 19-Oct. | Нужен отдельный invariant actual **policy+reference tokenizer SHA**, а не только SHA self-consistency materializer; возможен guard на outer runner. |
| kNN generic, hierarchy, similarity, adaptive, confidence | P16, P20–P23, P26–P28, P93, P107 | **Design PASS** generic-level; **PARTIAL** translation-specific: P107 owner \`Generic KnnLogicalUnit\`/k3, а проверяемый реальный Executor consumer использует translation \`k5\`. | N56-C1/C2/C3 native candidate→verifier→Flow micro. |
| POS, parser, syntax, order, semantic roles, operator identity | P16/P26/P27/P28/P43/P44/P45/P51/P55/P77/P83/P84/P85 | **Design PASS по существующим профилям**, например C# AST graph vs перевод порядка; explicit conditional where no Roslyn consumer. | Не переносить успех synthetic E25/E26 на все языки; нужен реальный recipient + feature trace. |
| DPO pair→typed Unit, graph, Flow, orchestration | P19, P39, P88, P92, P94–P97 | **Design PASS** as conditional trace protocols; multi-recipient/learned edges не приравнивают к nearest copy. | Нет единого real after-training ordinary-RP01 path от DPO pair до каждого named Unit, подтверждённого новым run. |
| Partial batch/admission/identity/no-op readiness | P53.1–.5 | **Design PASS**: unknown Unit, all-heldout, duplicate IDs, atomicity; не ново в N56. | Зависимость от конкретного P125 trained checkpoint и false regression exam требует отдельного matched native следа. |
| Reported Accuracy vs measured correctness; mixed direction, baseline state | P57.1–.16; [19-Oct EVAL-DIR / METRIC-STATE](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/17-additions-direction-evaluation-and-policy.md) | **Design PASS**, включая explicit negative A=B, eval not learning, score before/after separation. **Не заявлять новым N56**. | Реальный native запуск этой карточки NOT_RUN в данном аудите; валидно по конкретному evaluator, не neural Student. |
| Cold restore/checkpoint adoption; schema v1→v2, old branch donor | P18, P25 + 19-Oct additions 10–17; P25 actual 24/24 restore | **Design PASS** for identity/cold + **native evidence** лишь конкретного EN→RU. | Gold v1 orchestration / translation legacy manifest → legitimate v2 admission + ordinary consumer остаются условными. |
| Visual score and signed learning in DesignConversion | защищённые \`15-additions-designtocode-learning-semantics.md\`; planning 54 | **PARTIAL**: existence of method, но отсутствует доказательство влияния score на \`AddDelta.Contribution\`; reporter-only score нельзя считать learnable. | Micro score sign-swap / frozen page screenshot / native admitted recipient. |
| Wiki/GraphRAG source authority / replay / source-specific answer | P24, P50, P69/P70/P74/P116–P120 и N49/N51 | **Design PASS conditional** for source-specific consumer, separate replay. | Измеренные ранее компоненты не подтверждают current end-to-end Wiki answer + source citation. |

**Теги:** \`Design PASS\` означает **обоснованная, фальсифицируемая конструкция на прочитанном уровне**, **не** PASS исполнения или независимый reviewer verdict. \`PARTIAL\` — не найден достаточный recipient/scope или неполная связка. \`NOT_RUN\` относится к сегодняшнему анализу; там, где есть native 18-Oct receipt, он указан отдельно.

## 3. Новая проверка полноты *индексов*, НЕ утверждение о 31 забытом эксперименте

Сопоставлены литеральные имена **463 корневых файлов** planning frozen snapshot со следующими прочитанными protected summary: \`experimets/01-full-source-registry.md\`, \`06-source-content-walk.md\`, \`2026-10-19-small-learning-probes/06-findings-map.md\`, \`08-sources-and-study-log.md\` и \`09-additions-44-48.md\` … \`17-additions-direction-evaluation-and-policy.md\`.

- **432/463** имени найдены хотя бы в одном из этих summary/addenda.
- **31/463** имя не найдено **буквально** — это **TITLE_LINK_GAP**, а не доказательство отсутствия смыслового покрытия в 125 карточках, всех 479 элементах или новых 19-Oct документах.
- Среди 31: **8 gold-first / восстановительных master-документов**, **7 документов и результатов группы 38–39**, **5 group 52**, **10 group 54** и **1 скрипт group 55** (группы по префиксам, не 31 независимый root cause).

### Точные 31 missing-by-literal-name (для следующей проверяемой инвентаризации)

\`\`\`text
00-01-PREPARETOGPL-GOLDEN-RESTORE-2026-10-19.md
00-54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19.md
00-DESIGNTOCODE-CLEAN-GOLDEN-FIRST-2026-10-19.md
00-DESIGNTOCODE-CLEAN-GOLDEN-RECOVERY-FIRST-2026-10-19.md
00-DESIGNTOCODE-CLEAN-RECOVERY-FIRST-2026-10-19.md
00-ORCHESTRATOR-EXTENSIONS-GOLD-RECOVERY-FIRST-2026-10-19.md
00-ORCHESTRATOR-EXTENSIONS-GOLDEN-FIRST-2026-10-19.md
00-ORCHESTRATOR-EXTENSIONS-GOLDEN-RESTORE-P0-2026-10-19.md
38-CODEX-ASTRA6-ULTRACODE-EXPERIMENT-AND-INDEPENDENT-REVIEW-WORK-ORDER-2026-10-16.md
38-KIRO-CODEX-FAST-CAUSAL-REPLICATION-PROMPT-2026-10-16.md
38-MASTER-PRIORITIES-POSITIVE-HISTORY-NEW-HYPOTHESES-AND-MICRO-DECISIONS-2026-10-16.md
38-MICRO-MULTIDOMAIN-LEARNING-DPO-KNN-AND-UNIT-EXPERIMENTS-2026-10-16.md
38-PUBLICATION-AND-SOURCE-VERIFICATION-RECEIPT-2026-10-16.json
39-MICRO-TOKENIZER-IDENTITY-DPO-PAIR-TO-STUDENT-EXPERIMENT-2026-10-16.md
39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md
52-GOLD-OR-STATIC-PREFLIGHT.py
52-ORCHESTRATOR-DOMAIN-MICROEXPERIMENTS-2026-10-19.md
52-ORCHESTRATOR-GOLD-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md
52-ORCHESTRATOR-PROBE-ORDER-PROXY-2026-10-19.py
52-ORCHESTRATOR-PROBE-ORDER-PROXY-RESULT-2026-10-19.json
54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md
54-DESIGNTOCODE-CLEAN-EVIDENCE-AND-SKILL-REVIEW-2026-10-19.md
54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md
54-DESIGNTOCODE-CLEAN-FAST-DOMAIN-EXPERIMENTS-2026-10-19.md
54-DESIGNTOCODE-CLEAN-PROXY-RESULT.json
54-DESIGNTOCODE-CLEAN-PROXY.py
54-KNN-SOURCE-EQUIVALENT-PROXY-2026-10-19.py
54-KNN-SOURCE-EQUIVALENT-PROXY-RESULT-2026-10-19.json
54-MECHANISM-COVERAGE-AND-REVIEW-2026-10-19.md
54-MICRO-DOMAIN-CAUSAL-EXPERIMENTS-2026-10-19.md
55-MIXED-DIRECTION-ROUTING-PROXY.mjs
\`\`\`

**Нельзя автоматически добавить 31 новую карточку**: сначала содержательный source claim→existing P.subcase join. Первые owner priorities: \`39\` BPE SHA recipient, \`52\` old orchestration v1 actual consumer, \`54\` signed visual learning, \`55\` direction evaluation/proxy. Для остальных scripts/docs могут быть полностью представленные семантические дубликаты.

## 4. Критические требования к ПРАВИЛЬНОМУ эксперименту и честному before/after

Для каждого уникального claim, **до кода**, заполнить:

1. **Источники с pin SHA+line**; донор gold и current, exact named Unit/Flow/Student, какой user-facing путь действительно вызывается; по возможности Git blob identity/checkpoint/schema.
2. **Контракт признака/учебного изменения**: одна независимая переменная; frozen B0/start state (включая tokenizer, encoder, index, LoRA base/reference, optimizer, RNG, routing), TRAIN source IDs и TEST source-disjoint IDs; no oracle in trainer/selection; разрешённый positive/negative/edge контроль.
3. **A0/A1/B0/B1**: A и B до/после на **одинаковых четырех новых exam** (2 teaching items — старт, не performance guarantee); если сравнивают **scorer**, а не learner, ответ и обучаемое состояние должны быть неизменны, записать \`updates=0\`. Если historical learned state, **до** — реально загруженный checkpoint с исходной функцией, а не dummy random B0.
4. **Предметные показатели**: правильные смысловые обязательства / applicable; correct entire responses / all; неверные принятые / all; coverage/abstain; gains/losses по caseId; раздельно chosen margin, lexical score, accuracy, reward. Пример, когда **0/4→0/4**: честно NO_OBSERVED_GAIN, даже при изменившихся весах.
5. **Рабочие расходы**: секунды/реальный update, полезные tokens/sec, total training time до качественного ответа, candidate latency, memory RAM/VRAM с областью процесса, bytes checkpoint/index, число trainable/total tensors/parameters/layers и Unit/Flow executed depth; unchanged/n/a с основанием.
6. **Отрицательный контроль**: shuffled targets/labels, law-only, no-learning state, metadata route, cold-restore after training — выбирать то, что отличает гипотезу; один отличный score не является выводом о learning.
7. **Трасса и независимая оценка**: raw answer + выбранные neighbor/unit/adapter/Flow branch, actual model-visible token IDs, stop reason, accepted/rejected provenance; blind semantic judge не должен видеть arm и train, отдельные подписи author/reviewer.
8. **Артефакты реального исполнения**: command, exit code, SHA state/corpus, signed stdout/report, before+after table, counts and denominators, confidence interval only if data justify it. Proxy/simulation не переименовывать в native result.
9. **Область доменов:** фиксировать конкретный тип темы + профиль (C# AST vs перевод/order vs SQL roles vs Wiki evidence vs orchestration order vs Figma/HTMLCSS); спорная функция может давать выигрыш в одном и ухудшение в другом.
10. **Применимость:** \`SOURCE_CONFIRMED\` / \`HYPOTHESIS\` / \`DESIGN_ONLY\` / \`NATIVE_RUN\` / \`REVIEW_ACCEPTED\` / \`NOT_REACHABLE\` — разные статусы; не давать PASS только по количеству тестов.

## 5. Качество сопровождающих документов

**Плюс:** защищённые P53/P57/P107/P125 и активный \`01-method-and-report\` в большинстве прочитанных карточек аккуратно разделяют actual learner, score, consumer и oracle, предлагают проверяемый случай и edge-контроль. **Минус обнаруженной воспроизводимости:** некоторые \`cards/P125-executor-memory-learning.md\` и \`cards/P107-lawful-neighbor-aggregation.md\` имеют ссылки формата \`C:/Users/lubao/Documents/...\` на исходники planning; эти URL **непереносимы как публичные GitHub-ссылки**. Не меняя защищённые документы, в **следующем внешнем executor-пакете** использовать ссылки \`https://github.com/lubochka/xiigen-planning/blob/<PIN>/llm%20mvp/.../possibleDifs/<FILE>\`; для protected самого GitHub — \`blob/<PIN>/docs/plans/replanning-possible-diffs/experimets/...\`.

**Итог quality-review:** **METHOD_ACCEPT_WITH_SCOPE_LIMITS** для просмотренных карточек, **GLOBAL_COVERAGE_NOT_PROVEN**, **NATIVE_EFFECT_ONLY_FOR_PUBLISHED_RECEIPTS**. Слова «все гипотезы доказаны экспериментами» и «никакого нового эксперимента не нужно» оснований не имеют. Связывать всю свежую 19-Oct историю с cards только после semantic claim-by-claim join.

**Next:** [N56-03 малые исследования](56-03-FAST-DOMAIN-AB-EXPERIMENTS-AND-EXECUTOR-REVIEW-2026-10-19.md). Protected folder remains unchanged.
