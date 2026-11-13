# R06 — решения, учебные сравнения и готовые промпты передачи

**13.10.2026 · checkpoint C · R06-REPAIR-1.** Исправлены действующие E6/H07/H12/H13 и приоритет первого опыта по R05 CP2. Сохранены D01–D08, E1–E6 и H07/H12/H13, их предметные ограничения и уникальные примеры; D09/D10 — адресная дельта. Это исследовательские предложения, не разрешение на обучение, model calls, core changes или CI. Рецепты не выполнены; efficacy/PC fit — **NOT_MEASURED**. Они не добавляют шесть обязательных сессий к 10+5+5+5.

Опора: [FINDINGS.md](FINDINGS.md), прежние exact ref/blob/spans — [SOURCES.csv](SOURCES.csv), fresh review receipts — [RUN-LOG.md](RUN-LOG.md), раздел R06-REPAIR-1. Полная редакция B сохранена в Git на planning commit `64a5e745eecce129d2a52a0a1ac307da22e248da`; её H07 про «два соединения» больше не является универсальным описанием остатка до первого урока. В results/R06 остаются пять carriers, не отдельные новые файлы на каждый опыт. Действующий запуск R06 — [самостоятельный prompt](../../run-prompts/R06-PARALLEL6.md).

## 1. Решения R06 и их статус

| ID | Решение/предложение | Статус и владелец |
|---|---|---|
| R06-D01 | Сохранить deterministic eligibility/type/law checks, exact search и bounded execution. Семантическая полезность маршрута принадлежит обучаемому owner, не позиции кандидата или хэшу. | Сохранение hybrid требования; scoped migration старого skill-helper ban — R12 |
| R06-D02 | Начинать с одной weak family и существующего machine-owned learning path, устраняя его реальный первый разрыв. Полные S08/S09/Windows release не prerequisites локального урока. | Предложение R12/R13; снять только действительно не нужную этому пути зависимость |
| R06-D03 | Переиспользовать найденные search/runtime/training механизмы. Для graph path проверить checkpoint→semantic L2 recipient→ordinary L3; для CodeDocumentation — его собственный опубликованный learner→обычный consumer. | Исправленная передача R07/R12/R18; не принуждать любой первый урок проходить graph |
| R06-D04 | Dynamic k требует правила выбора k, owner/state и настоящего потребителя дополнительных соседей. Current copy-first output не голосование k соседей. | Исследовательский контракт; один implementation choice — R12 |
| R06-D05 | Разделять parameter learning, vocabulary/example learning, рост памяти, recall индекса, изменение tokenizer и compute. | R13 выбирает минимально достаточные controls; нет полного Cartesian sweep |
| R06-D06 | Zero margin не означает универсальный zero update. Улучшение каждой пары и движение каждого веса также не обязательны. | Замечание к WorkLeft P4-1; R05/R12 сверяют objective, R17 меняет зависимый критерий |
| R06-D07 | CODEX selection → Chinese final chosen/rejected → CODEX exact quality review. Pair validity, admission и Student utility различаются. | Сохранение действующего контракта, не новые model calls |
| R06-D08 | Semantic order, input-output associations, recipients, branch conflicts и полный бюджет сохраняются через четыре роли. | Original task/R03; предметная examination E5 |
| R06-D09 | Оценённое различие пары должно дойти до фактических tensors и объявленного response objective; raw fingerprint этого не доказывает. | R05 CP2 receipt принят в E6/H12/H13; существующий token/objective seam передан R18, не исправлен в core |
| R06-D10 | Training capability, semantic recipient, winning solver и runtime arbiter не взаимозаменяемы. LlmModel weights не являются автоматически state kNN или typed selector. | Исправленный H07 и граница DPO candidate; точный bridge — R07/R12/R18 |

## 2. Общий контекст каждого опыта E1–E6

**Роль:** исследователь learning-системы, не автор нового test framework. **CURRENT TASK:** различить одну гипотезу на существующем механизме и получить предметный результат. **NOT TASK:** подбирать ожидаемый ответ под код, максимизировать coverage, запускать все combinations, удалять guards или заново строить библиотеку.

**Skills:** `docs/skills/experiment-design-SKILL.md` — causal comparison; `trainable-architecture-decision-SKILL.md` — owner/state; `code-examination-SKILL.md` — actual caller/consumer; `context-package-design-SKILL.md` — полный bounded packet; `human-readable-architecture-audit-SKILL.md` и `output-readability-gate-SKILL.md` — смысл и пределы вывода. Перед исполнением загрузить относящиеся sections на current K; записать actual ref/span. Наличие имени в списке не evidence чтения. Старые replay/test/pass-count routines не разрешены этим поручением.

**Права:** R06 готовит дизайн, не исполняет experiments. Будущее фактическое исполнение требует применимого разрешения на данные, модель, бюджет и mutation. Возобновлённая попытка не делает повторный update уже применённой пары. Работать только в назначенных владельцу files, не менять siblings/shared dispatcher под видом собственного исследования.

**Общие входы:** immutable parent checkpoint; разрешённый source-family split; domain/intent/stack/project taxonomy; representation/index versions; candidate universe; rubric; actual update owner и state group; hardware/budget; no-update control. Недостающий вход имеет owner и значение UNKNOWN, не фикцию. Можно закончить документный дизайн с ограничением; реальный опыт не мутирует состояние без допуска.

**Порядок A/B:** A — выбрать одну intervention и frozen components, источники, данные, наблюдения и цену. B — в отдельно разрешённом исполнении получить baseline/candidate outputs, оценить по той же рубрике и сравнить. Две части одного автора не являются независимыми reviewer/model calls. Подзадача для другого исполнителя получает полный общий блок, exact source subset, свою A/B часть, входы/выходы/good/bad/edge/приёмку, return-only права; не только имя E6.

**Разделение данных:** learning и evaluation families независимы с учётом общего источника; перефразы и N−1 варианты одного задания — не независимые families. Учитель/выборщик урока не получает закрытые exam answers или их близкие версии. Operational diagnostic panel можно переиспользовать для наблюдения, но после раскрытия она не untouched certification. Final panel остаётся отдельной. Пользовательский feedback сначала оценивается на текущем state; он не автоматически истинный label.

**Выход:** одна существующая запись опыта с parent/child/state/representation/pair identities; per-case результат/ошибка; агрегаты по предметам/семействам; wall time, compute, memory, стоимость teacher/judge/update/evaluation и всех попыток. Training entry, completed update, accepted checkpoint и ordinary consumer use — отдельные поля. Статус сейчас NOT_MEASURED; будущий вердикт IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE. Старые IMPROVED/REGRESSED означают те же категории, не новые измерения.

**Общая examination:** сравнимые inputs, одинаковая рубрика, допустимы разные правильные пути. Quality/законы — ограничения; компактность/скорость оцениваются при сопоставимом качестве, не покупают нарушение смысла. Порог существенного эффекта, precision/family sample rationale и stopping rule фиксируются R13 до результата. Нельзя менять их ради PASS. Маленькая выборка/неоднозначная truth могут дать INCONCLUSIVE. Рост state/hash или число зелёных checks не являются learning gain.

## 3. Dynamic kNN и LoRA: конкретный алгоритм вместо названия

Предлагаемая схема, **не готовая реализация**:

`input → (subject, intent, stack, project, uncertainty) → authorized compatible universe → eligible/related-domain pool → representation φ_v → neighbors/proposals → consumer policy → valid route → arbiter/result`.

Mixed subject и разрешённый bridge могут использовать несколько явно допустимых пулов. Права, tenant isolation и schema compatibility не смягчаются similarity. Unknown subject имеет knowledge-edge/abstention путь, а не forced label. Warning в current KnnLogicalUnit не доказывает, что caller действительно воздержался.

Для первого кандидата R12 выбирает **один** score/consumer change. Возможная гипотеза: `s_d(q,z)=sum_j w_dj(q)*phi_dj(q,z)` с заданными предметными признаками; `k_d(q,state,budget)` либо radius ограничены ресурсами. Записать actual или proposed parameter group, signal, objective, owner и потребителя. Formula не считается принятой реализацией или доказанным лучшим вариантом.

Query-conditioned/asymmetric score не обязательно metric. Не считать существующий HNSW автоматически пригодным после смены distance/projection. Возможны exact finite-pool scoring или fixed-metric proposals + один reranker; во втором случае отдельно измерить candidates lost before reranking. Existing BruteForceIndex — cosine/in-memory, не arbitrary-score engine или готовый persistent publication bundle.

**Baseline в current KnnLogicalUnit:** Train обучает saved BPE и сохраняет example memory/index; Execute возвращает `bestExample.Output` первого hit. Остальные hits дают evidence IDs. Это настоящее vocabulary/example learning, но не demonstrated neural-DPO update. До dynamic k выбрать реальный consumer: compatible-candidate selector, evidence-aware reranker **или** предметно допустимую агрегацию. Не внедрять все три. При fixed exact ranking больший k не меняет copy-first output; при ANN первый hit может измениться из-за approximation — не путать эффекты.

Для DPO-trained selector нужны compatible alternatives, conditional distribution, reference и actual owner/gradient route. Scalar distance или judge confidence не заменяют log-probability. Нормализация придуманного score сама по себе не подключает существующий LlmModel trainer. Иной ranking/metric-learning objective допустим как явно названная гипотеза, но не должен называться уже реализованным DPO. Не создавать четыре training stacks и не объявлять selection capability получателем любого reward.

Версия вектора включает tokenizer/codebook/normalization/domain schema/projection/adapter/index coordinates, не только длину. LoRA — место адаптации конкретного слоя, не универсальный vector→token converter. Указать input/output meanings и actual forward consumer: меняется query embedding, stored embedding, output projection или policy score? Если выбран output representation, измерять decoded/typed semantic output и сохранение law/association, а не только размер матрицы. Если меняются index coordinates, учесть reindex/cache invalidation/rollback. TeachingVector в нынешнем StepVector — pair weighting, не целевой output vector; его нельзя без trace объявить этим adapter seam.

Positive/negative/warning memory хранит причины и область применимости; отрицательный сосед не становится target ответа. Не удалять предупреждения как неудобные; не приписывать усвоение закона появлению ответа из exam в памяти.

## 4. Ограниченные учебные опыты

### E1 — правильно ли найден предмет и почему сломался путь

**Контекст/источники:** FINDINGS F04/F12; K05/K06/K21; P05/P06 и original task; R04 channels. **Вход:** одна разрешённая family, например SQL documentation, соседние C#/Angular cases, mixed/unknown; известный project/dialect; один compatible candidate universe и frozen solver. Общие права/skills/данные из §2 обязательны.

**Prompt A:** «Раздели subject, intent, stack и project. Построй predicted membership и diagnostic independently established membership. Какие кандидаты потерялись из-за mapping, а какие были в пуле и проиграли ranking? Не используй diagnostic oracle как runtime shortcut или label из закрытой панели».

**Prompt B:** «Сравни одинаковый downstream solver при predicted и diagnostic-correct domain. Запиши lawful success, per-domain confusion, mixed-domain coverage, unknown rejection, abstention coverage и время. После отдельно разрешённого domain-owner update используй новые source families, не обучай solver одновременно».

**Выход/приёмка:** attribution mapping/pool/ranker/solver, не общий accuracy вместо причин. Оба solver плохи и разница мала — не успех router. **Хорошо:** корректный dialect pool возвращает допустимый путь; неверный intent виден отдельно. **Плохо:** общий score скрывает пустой SQL pool или уверенный неправильный fallback. **Edges:** короткий prompt, смена предмета в контексте, смешанные языки, несколько правильных memberships, нет truth для unknown. **Owner:** R08 данные; R12 owner; R13 включает лишь если domain routing влияет на выбранный первый опыт. E1 не обязательный gate каждого урока.

### E2 — предметная метрика, представление и реальное действие k

**Контекст:** F01/F07/F11/F18; K01/K10/K20; primary ARBITER-KNN; R04 coordinate receipt. **Вход:** одинаковый permitted pool, frozen representation version, hard near misses, fixed-k/copy-first baseline и один выбранный score/consumer candidate.

**Prompt A:** «Отдели exact neighbors от ANN approximation. Укажи source-grounded law features, отличающие похожие, но неверные варианты. Выбери metric при fixed k ИЛИ k policy при fixed metric для первого contrast; второй нужен только для оставшейся гипотезы. Параметры выбирай на development subset. Покажи, как дополнительный neighbor влияет на результат; при copy-first не выдумывай voting».

**Prompt B:** «Сравни exact и ANN при одинаковом score там, где они реально поддержаны. Query-dependent reranker сопоставь с полным eligible pool, чтобы видеть truncation cost. Раздельно оцени neighbor relevance и downstream lawful utility, время, память, число кандидатов, warnings/abstention и редкие ошибки. Изменение encoder/LoRA projection исследуй отдельно при fixed downstream и совместимом index».

**Выход/приёмка:** заполненная algorithm card из §3, один baseline/challenger, frozen/changed components, owner/signal, semantic counterexample, full cost/falsifier. Shape или cosine без task gain не подтверждают output semantics. **Хорошо:** близкий несовместимый dialect/unit отвергнут с причиной. **Плохо:** k+tokenizer+memory меняются вместе, выигрыш назван dynamic k. **Edges:** пустой pool, меньше k примеров, ties, одинаковые token counts при значимом order, changed coordinate space, unreliable relevance, duplicate neighbors. **Owner:** R12/R13; R04 уточняет реальный channel. Optimal universal k не объявляется; неоднозначный опыт INCONCLUSIVE.

### E3 — BM25, kNN или их сочетание

**Контекст:** F02/F11; K01–K03/K19/K20; DECISIONS-LOCKED D10. **Вход:** одинаковые document IDs/права/domain/source families; BM25 sparse и vector representations названы отдельно, а не выданы за одно пространство.

**Prompt A:** «Найди существующий fusion consumer выбранного product path; при отсутствии в прочитанном subset назови один missing call. Подготовь BM25-only / kNN-only / один fusion при одинаковом eligible universe и сопоставимом retrieval budget. Не приравнивай local/global HybridRetriever к BM25+kNN. Не строй новый search engine».

**Prompt B:** «Покажи exact-identifier ошибки против смысловых перефразировок при одном downstream consumer. Учти index build/update и количество предложенных кандидатов; normalization/weights зафиксируй до закрытого сравнения. Сообщи отрицательный эффект на редких symbols или другом языке, не только среднюю пользу».

**Выход/приёмка:** lawful downstream output и полная цена, не сумма несопоставимых retrieval scores. **Хорошо:** lexical канал сохраняет API-имя, semantic — paraphrase; final authority не подменена helper. **Плохо:** fusion видит вдвое больше знаний/compute и объявлен бесплатным. **Edges:** пустой lexical hit, шкалы, stale index, mixed language, mandatory skill outside top-k. **Owner:** R07 — только неизвестный consumer; R13/R22 — выбор нужного опыта. E3 не стартовый gate.

### E4 — parameter learning или просто рост памяти

**Контекст:** F05/F07; P05 memory; WorkLeft P14/P15 one-pair; R05 actual owner/objective receipt. **Вход:** parent state, frozen representation/solver, разрешённая новая пара, одинаковые prompt/context volumes и выбранный настоящий learner.

**Prompt A:** «Раздели (a) memory fixed: разрешённый policy update против no-update и (b) weights fixed: разрешённая новая memory против прежней. Для первого опыта выбери достаточный contrast, не Cartesian sweep. Отдели BPE/example learning от neural parameters. Старые pair IDs повторно в optimizer не отправляй; новый ID тому же материалу не создаёт новый урок».

**Prompt B:** «На новых независимых families покажи ответ, neighbor/warning reason, route и score-independent quality. Если обе ветви улучшились только от добавленного в memory ответа, не приписывай это gradient. Если updater не получает различие сторон из-за F15/F16, это invalid evidence для DPO-benefit, не отрицательный результат всей гибридной архитектуры. Отрази rejected/no-update уроки и их цену».

**Выход/приёмка:** причинно различимые memory/parameter эффекты, а не обещанный рост после каждой пары. **Хорошо:** policy лучше использует прежнюю negative-memory причину на новом случае. **Плохо:** контроль получает меньше контекста/старую библиотеку. **Edges:** contradiction, stale law, duplicate identity, memory growth, project switch, неверный semantic recipient, insufficient families. **Owner:** R05 actual update, R13 comparison. Это не разрешение replay buffer или повторных gradient trials без допуска.

### E5 — Flow, асинхронность и higher-order Graph

**Контекст:** F03/F08–F10/F17; K11–K17; original four roles; R03 F17; S07.04A/B/C. **Вход:** frozen primitives, permissible relationships, input-output types, один budget, новая комбинация знакомых primitives. Если выбрана graph policy, H07 сначала устанавливает actual semantic recipient/ordinary path; CodeDocumentation solver update не заменяет его.

**Prompt A — Flow:** «Заморозь Unit parameters/outputs, измени только доступную learned relation policy. Проследи input→Unit output→next input, association/recipient/rationale. Сохрани many→many и альтернативные допустимые outputs. Обратный frozen Flow/changed Unit contrast используй только при необходимости различить причины».

**Prompt B — Orchestrator:** «При одном input/candidate universe сравни baseline composition с одним learned candidate. Запиши start/finish/cancel, partial failures/conflicts и merge decision всех ветвей. Latency победителя не скрывает compute остальных. Быстрый недопустимый ответ не выигрывает; completion order не меняет semantic dependencies».

**Prompt C — Graph:** «С теми же primitives исследуй перенос higher-order structure на новые source families. Presentation rename имеет явную биекцию IDs и не меняет authorization/dependency order. Покажи actual L2 decision→L3 call→output. IsLearned, route ID lookup и новая перестановка знакомых названий не доказывают compositional transfer. Укажи admission/reuse/pruning relations и полный growth cost».

**Выход/приёмка:** lawful solution rate, bounded-set path regret, executed steps/full cost, unseen-composition quality. Не заявлять globally best path без oracle. Reward не размножается на все edges; credit hypothesis и её основание направляются R07/R12. **Хорошо:** prerequisites предшествуют effect, независимые анализы параллельны, конфликт сохранён. **Плохо:** alphabetic/shortest-path даёт недопустимый порядок; одновременно обучены solver и graph. **Edges:** cycle, missing prerequisite, duplicate/late branch, timeout, conflicting outputs, distinct equally useful paths, graph budget exhausted. **Owner:** R07 concrete graph links; R12 одна intervention; R13/R22 budget. A/B/C — возможное разбиение, не три автоматически обязательных вызова.

### E6 — доходит ли смысл пары до ученика и окупается ли обучение

**Контекст:** §5; F06/F13/F15–F18; R05 CP2 F03/F05/F09–F14 и C01/C11/C12; WorkLeft teaching/INTEGRATION-01/P4-1; S06 campaigns; S07 Builder .07A/.08D. Прямой источник текущей дельты: K `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` BuildBatch/Compact 2260–EOF, blob `cb8744fcbbcff99821852e789dc92163976c0ad7`; `11.DPO/DpoTrainer.cs` 530–730, blob `9c358978075db8c22fe0125eaea9e89e831d5635` при K `3c7f5f8c7809936e25579603c96ea6df8598fc40`.

**Вход:** diagnosed weakness, compatible immutable parent, permitted teacher profile/context, одна exact pair и CODEX verdict, independently evaluated families, actual semantic recipient/state group, hardware и budget. Уже известные coordinator/optimizer/publication не проектируются заново.

**Prompt A — от оценки до update:** «Назови learner owner, objective и parameter group. Проследи exact reviewed prompt/chosen/rejected → фактическое domain-sensitive представление → response boundary/mask → policy/reference forward/backward → параметры. Проверь, осталось ли именно различающее условие. Raw fingerprint и разные длины строк не доказательство сохранённого смысла. Prompt должен оставаться conditioning context; loss mask response отделяется от attention/context semantics, padding, next-token shift и EOS. Length normalization не менять молча. Не ломай public average inference scorer при исправлении training objective».

**Prompt A, ограниченный implementation handoff:** «Прими R05 уже локализованные BuildBatch/Compact и response scorer как first repair candidates для R18. Если input выходит за поддерживаемый token budget, явно отклони либо сформируй самостоятельный новый разрешённый урок с новым exact assessment; не обрезай смысл под старым verdict. Не учи DPO на одинаково collapsed alternatives. Не переписывай весь trainer или контроль состояния. Перечисли только изменяемые методы, зависимые consumers и наблюдаемое поведение».

**Prompt B — от update до следующего ответа:** «Сравни before/after с no-update при одинаковой памяти и frozen rubric. Покажи, что принятый child либо сохранённый parent реально выбрал ordinary caller, а не только trial evaluator. Для graph-selection preference обновляется semantic selector, не автоматически winning solver. Для первого document learner не требуй неиспользуемый L2/L3. Сохрани полный pre-pair state, ранее применённую пару не повторяй. На новых bulk окнах покажи качество, время и весь temporary/persistent footprint».

**Builder subdivision, только когда выбран Builder:** «Заморозь/сопоставь downstream Student start и разрешённое число новых уроков. Измерь полезность создаваемой пары отдельному Student, не self-score/JSON size. Собственный Builder остаётся целевой обучаемой способностью программы, но нынешний Chinese-authored external teacher route не подменяется непроверенным self-authoring. Вердикт качества пары не гарантирует её downstream utility».

**Выход:** исправленный или точечно переданный source→objective seam; exact parent/child/pair/recipient identities; независимое качество и retention; state фактического ordinary consumer; раздельные validity/admission/update/acceptance/utility; all-attempt cost. **Документная приёмка:** другой исполнитель может найти владельца/метод, отличить semantic loss от genuine no-gain и получить одно bounded implementation поручение. **Будущая examination:** quality/time/PC cost на unseen families, не checksum или training accuracy.

**Хороший контрпример для разбора:** chosen/rejected отличаются существенным условием только после первых 480 нормализованных символов; их raw fingerprints разные, но существующий Compact может дать одинаковые sequences. Это иллюстрация, не реально созданная Teacher pair и не запуск теста. Другой contrast — ответы разной длины при одинаковом prompt: явно проверить объявленный conditional objective, не приписывать его нормировку kNN.

**Плохо:** checksum изменился — значит научились; TeachingVector density — значит semantic decoder; judge confidence вместо DPO margin; LoRA small file — значит PC fit. **Edges:** обе стороны правильны, обе плохи, tie, near-duplicate, lost whitespace/negation, zero margin, unchanged weights, representation collapse, wrong recipient, insufficient sample, evaluator version changed. Нулевой/отрицательный эффект допустим; потеря обучаемого различия даёт INVALID_EVIDENCE для заявленного DPO-сравнения, не универсальный verdict о гибридном подходе.

**Owner:** R05/R18 — token/objective seam; R07 — ordinary semantic consumer; R12 — owner/objective choice; R13 — minimal recipe; actual PC profile — владелец ресурсов. Это не шесть новых checks или требование подождать завершения всех доменов.

## 5. Точный DPO-контракт и model-ready prompts

**Роли неизменны:** CODEX **astra 6 ultracode** выполняет человеческий педагогический выбор разрешённой задачи; **внешняя открытая китайская модель** создаёт обе окончательные стороны **chosen/rejected**; CODEX **astra 6 ultracode** отдельно оценивает exact pair в exact permitted context перед optimizer. Project alias — `GPT-6-Astra`/`ultra`; requested configuration и actual model invocation evidence различаются. Здесь product calls не выполнялись. Pair reviewer, runtime graph arbiter, independent Student evaluator и DPO reference — разные роли.

### Prompt CODEX-выборщику урока

> По разрешённым пользовательским входам и диагностике слабого результата выбери один урок, который проверяет конкретную способность. Раздели subject, intent, stack/project и composition level. Назови diagnosed weakness, допустимый контекст/primary laws, input-output schemas, semantic recipient и почему этот урок предпочтительнее ближайшей альтернативы по ожидаемой полезности и цене. Не копируй закрытые exam cases. Предыдущие применённые pairs могут быть разрешённым context_only материалом, не новыми optimizer rows. Не обещай gain и не создавай final chosen/rejected вместо назначенного автора. Неизвестный recipient/permission отрази явно, не заполняй случайным UnitId.

### Prompt внешнему китайскому автору

> Получи lesson_id, одну задачу, предмет/версию законов, input-output schemas, diagnosed weakness, фиксированный semantic recipient и permitted context. Создай обе стороны для этой же задачи и получателя. Chosen соблюдает смысл и ограничения; rejected — правдоподобный near miss с существенной предметной ошибкой, а не мусор/другой домен/слабое оформление. Допустимы разные хорошие пути; не выдавай полезный deterministic результат за плохой только по происхождению. Не копируй закрытый экзамен и не назначай другого recipient. Верни стороны, основание предпочтения, различающее правило и uncertainty. Model/request/provenance IDs присоединяет приложение из реального вызова; не выдумывай их. Не гарантируй Student improvement.

### Prompt CODEX-оценщику пары

> Оцени сохранённые exact sides и exact permitted context. Проверь одинаковую задачу/получателя, применимость закона/версии, существенность отличия, сложность negative, отсутствие leakage, сохранение many→many/order/единиц. Не переписывай стороны под чужим authorship. Разрешены accept, reject, tie, both_bad, insufficient_evidence. Два корректных ответа можно предпочесть один другому только по заранее объявленной существенной utility, включая цену при сопоставимом качестве; forced labels по стилю/происхождению недопустимы. Привяжи verdict к exact pair/context. Pair validity, permission admission и ненаблюдавшийся downstream gain — разные заключения. Подготовка tensors должна сохранить различающее условие; новый материал требует применимой новой оценки.

Эти prompts параметризованы для будущих допущенных вызовов, не являются evidence их выполнения.

| Family | Хорошая конструкция | Плохая конструкция | Examination |
|---|---|---|---|
| Flow / предусловие | Задано validate→persist; chosen соблюдает, rejected сохраняет schemas, но выполняет effect раньше проверки | Несвязанный текст или тот же путь с новыми ID | Semantic dependency и effects, не formatting |
| Перевод / отрицание | Один source и направление; near miss грамматичен, но меняет отрицание/число/роль | Стороны переведены из разных исходников | Source-grounded adequacy и допустимые варианты |
| Domain kNN / закон | Одинаковый input; chosen применяет совместимую версию, rejected — похожую, но неподходящую | Любой fixed ответ помечен rejected | Applicability и final utility, не происхождение |
| Encoding fidelity | Существенное отличие после длинного общего начала должно остаться в input objective | Different raw hashes объявлены достаточным основанием без проверки consumer | Exact reviewed distinction → tokens → response objective |

**Таблица — иллюстрации, НЕ реальные Teacher outputs и НЕ train-ready данные.**

Для заявленного standard DPO сверяется `m = β[(log πθ(chosen|x)−log πref(chosen|x))−(log πθ(rejected|x)−log πref(rejected|x))]`, `L = −log σ(m)`. Это не новый trainer. При m=0 `∂L/∂m=−1/2`; parameter update зависит от Jacobian, optimizer и разрешённых параметров. Judge confidence не заменяет likelihood; arbitrary ranking loss не называется DPO без явного определения. Current full-sequence mean scorer отличается от отдельно заявленного response objective: E6 сохраняет это как конкретный вопрос реализации, а не считает формулу доказательством корректности кода. Primary reference: https://arxiv.org/abs/2305.18290.

Каждая новая pair identity применяется не более одного раза в разрешённом цикле. Bulk — окно новых уроков, не replay. Полный rollback включает фактически изменённые weights, optimizer/RNG/counters, pending/accepted state, memory/index/projection; неизменные компоненты не обязаны дублироваться бессмысленно. Snapshot одного optimizer не доказывает восстановление всех graph/index/project состояний. Откат state не отменяет потраченные ресурсы. Неопределённый результат попытки требует recovery, не второго update.

## 6. Предметная широта без ожидания полной матрицы

Канонические семь subjects и extras восстанавливаются из WorkLeft, не из имён namespaces. Семейства ниже не требуют 28 моделей. Один первый lesson не удаляет остальные обязательства.

| Семейство | Что representation/алгоритм должны сохранить | Near miss и наблюдение |
|---|---|---|
| EN→RU и RU→EN | Направление, отрицание, entities, числа/единицы, роли | Грамотный перевод с изменённым смыслом; adequacy/constraints отдельно |
| C# documentation | Фактические symbols/types/contracts, nullability, async/resource semantics по проекту | Документ другого overload/режима; source-grounded correctness |
| Angular documentation | Template/component связи, binding direction, lifecycle/version | Похожий API или обратное направление данных; relations, не text match |
| SQL documentation | Dialect/schema и применимые NULL/duplicate/cardinality semantics | Лексически близкий, но неподходящий query/description |
| Design-to-Code | Hierarchy/geometry/units/constraints/viewport; HTML/CSS/images | Хороший desktop, сломанный mobile; реальные browser renders и независимая visual assessment |
| Orchestration / Graph | Prerequisites, semantic order, typed ports, permitted effects | Быстрый короткий недопустимый путь; actual execution/full cost |
| DPO Builder | Same-task preference, recipient, provenance, meaningful negative | Удобная judge, но бесполезная Student пара; отдельная Student utility |

R08/R21 указывают primary law sources, версии/применимость и независимые families. Метка самой системы не эталон предмета. Один generic cosine/text-length не заменяет разные законы, но общий reusable encoder разрешён, когда сохраняет нужный смысл. Для каждый найденной дополнительной области — requirement/owner/trigger, не автоматическое удаление. HE↔EN, IDE/wiki/audit taxonomies и прочие реальные потребители S09 сохраняются.

Для S07–S10 R06 передаёт source-qualified карту: exact obligation → primary section → current consumer → missing observation/implementation → owner/trigger. Human map/task list — не прочитанный task body. S08 сохраняет API/БД/history/user-tier cost и следующий ordinary answer, S09 — private index/editor journeys, S10 — whole-program acceptance. Полный audit внешних клиентов здесь не заявлен и не назначается prerequisite одного урока.

## 7. Bulk-to-bulk и компактность ПК

Строка наблюдения: `bulk_id; subject/direction/intent/project; new_lesson_count; optimizer_entries; applied_once; accepted/rejected/ambiguous; parent/selected_child; semantic_recipient; representation/index; independent_family_count; quality_before/after; uncertainty; abstention_coverage; latency; total_compute; peak_RAM/VRAM; persistent_bytes; full_lesson_cost; ordinary_consumer_state`.

Отдельные curves: quality/domain mapping; ordinary inference time; время/цена нового урока; memory/index/adapter/graph size; retention на прежних предметах, не использованных для update. Changed rubric/data/evaluator/config отмечаются как несопоставимость; новый более простой bulk не считается gain. Перефразы одной семьи не увеличивают effective sample count. Не исключать slow failures/rejected lessons. Accuracy после роста abstention оценивается вместе с coverage, иначе сравнение вводит в заблуждение.

PC footprint включает shared base, encoder, active adapters, reference, optimizer, index, graph/cache, runtime buffers и snapshots; teacher/reviewer residency/cost указываются отдельно. Cloud teacher не делает student-only VRAM полной ценой обучения. Учитывать cold load, warm inference, update, reindex, evaluation, все отменённые/невыбранные branches и amortized maintenance. Меньший adapter/rank/token count сам по себе не доказывает fit или semantic gain. При равном качестве сравнивать time-to-quality и полную цену, не количество checks.

**Исправленный минимальный приоритет R13:** один E6/необходимый E4 на ближайшем существующем learner с R05 CP2 token/objective repair и ordinary-consumer наблюдением. E1 добавляется, только если mapping влияет на его результат; E2 — только нужная retrieval/representation диагностика. E3/E5 и полные domain cards развиваются по конкретной гипотезе R22, не как обязательный барьер первого урока. Existing S06.08B/S06.09C campaigns/curves переиспользуются: S04 one-update-total не вечный предел проекта, но no-repeat каждой уже применённой пары сохраняется. Никакие новые coverage targets/Windows matrices здесь не назначены.

## 8. Готовые bounded handoff prompts

### H07 — R07: semantic recipient и ordinary consumer выбранного пути

**Current task:** закрыть только непрослеженные связи, не повторить R04/R05/R06. **Role/skills:** architecture + code examination + context-package design + plan review, exact sections/refs в source ledgers. **Контекст:** R06 F06–F12/F15–F18; R05 CP2 F03/F05/F09–F14, C01/C11/C12; R04 channels; для graph — K08–K17, P05/P06/P11 S07.04A/B.

**Prompt A — выбрать фактический путь:** «Зафиксируй одну учебную задачу и semantic recipient. Если это document learner, возьми существующий R05 coordinator/executor/PolicyModel route; если graph/route preference — установи именно learned selector/state owner. Не назначай winner solver или LogicalDpoTrainingUnit.Id получателем по совпадению имени. Покажи representation/objective/state compatibility; произвольный LlmModel checkpoint не является BPE/example state kNN. R05 token/objective defect передай его R18 packet, не исследуй весь trainer заново».

**Prompt B — проследить оставшийся consumer:** «Для document path проследи accepted publication→ordinary request model/adapter selection→output, не заменяя его trial evaluator reload. Для graph path используй уже найденные resolver/pipeline/L3 calls: admitted state→trusted preload→вызванный ILogicalUnit<EdgeAuthorityInput,EdgeAuthorityDecision>→association→DynamicGraphRegistryL3SelectedTargetExecutor→actual output. Не навязывай graph path document уроку. Для каждого действительно missing hop дай producer/consumer/type/version/owner/minimal delta. При неполном sibling результате выполняй только нужный bounded direct trace, не жди его DONE».

**Вход/выход:** actual source/checkpoint pins → один связанный trace и конкретные gap rows в собственных carriers R07. **Examination:** состояние принадлежит тому же semantic recipient и видно в ordinary caller; call expressions/versions, не filenames/IsLearned. **Good:** выбранный child используется обычным consumer; graph selector меняет relation при frozen solver. **Bad:** второй trainer, подключение любых weights, полный повтор S04–S10 или неиспользуемый L2 prerequisite. **Edges:** stale state, untrained composition, incompatible schema, unresolved target, evaluator-only reload, wrong recipient. **Handoff/stop:** R12 получает architecture choice, R13 — достаточный input, R18 — конкретный missing implementation; research grant не исполняет обучение. Старый H07 «два соединения» сохранён только как graph subset этой задачи.

### H12 — R12: одна минимальная гибридная архитектура

**Current task:** выбрать owner и одну минимальную intervention, не весь новый framework. **Skills:** trainable architecture, plan/code review, experiment design, readable architecture audit. **Контекст:** D01–D10; F07/F08/F13–F18; original four roles; R04 coordinates; R05 CP2; old skill-helper ban P16 versus P06 D10/PARALLEL6. Точные source spans из соответствующих ledgers; неизвестные утверждения не считать подтверждёнными.

**Prompt A:** «Выбери первый реальный learner/semantic recipient, его objective, allowed state group и ordinary consumer. Отличи уже работающий memory mechanism от gradient policy. Учти lossless-pair/response-objective seam R05; E1/E2/полный graph не prerequisites, если первый route их не использует. Назови baseline и ОДИН challenger. Если выбран kNN challenger — задай actual neighbor consumer, feature meanings, score/k policy, signal/update owner и coordinate budget. DPO требует conditional policy/reference; никакой judge-score вместо likelihood».

**Prompt B:** «Адресно разреши helper-ban и zero-margin requirement с primary versions, действующей областью и точными consumers. Не удаляй history и полезные deterministic helpers; learned final authority не подменяй helper. Конфликт, не затрагивающий первый выбранный route, получит owner/trigger, а не общий стартовый барьер. Не создавай четыре trainers; shared base/project adapters и четыре behavioral роли сохраняются. Раздели first seam от поздней DTC/IDE/release поставки».

**Вход/выход:** scoped evidence → owner/state/port decision, minimal delta, supersession map и один R13 recipe. **Examination:** никакого висящего Train owner/несовместимых coordinates/необоснованного recipient; у решения falsifier и rejected alternative. **Good:** существующий learner исправляет свой input/objective и используется ordinary caller. **Bad:** «обучить dynamic kNN» без состояния/потребителя; все algorithm эксперименты перед первым уроком. **Edges:** memory-only достаточно для задачи, no gain, mixed domains, два корректных пути, insufficient bridge evidence. **Stop:** решение не повышает readiness; конкретный неразрешённый call передаётся named owner, не новой общей серии ревью.

### H13 — R13: один первый измеримый урок/сравнение

**Current task:** собрать один исполнимый learning-examination packet, не unit-test plan. **Skills:** experiment design, planning, context-package design, human-readable reporting. **Контекст:** выбранный R12 owner; actual R04/R05/R07 seams; E6/необходимый E4 и лишь обусловленные E1/E2; current one-pair/admission; S06 campaign/curve reuse.

**Prompt A:** «Выдай исполнителю одну weak family, immutable parent, actual hardware/budget, domain-law primary refs, representation/index version, exact teacher/selection/review route и semantic recipient. Обозначь first code repair из R05 CP2 или уже подтверждённое исправление — не fake readiness. Покажи forward context и response-loss boundaries, ordinary consumer, no-update control, same-memory condition, fixed rubric, independent family sample rationale, meaningful-effect threshold и stopping rule. Не используй закрытый exam для Teacher и не предполагай выдачу budget».

**Prompt B:** «Задай наблюдения до/после нового урока и следующий обычный ответ: parent/child/pair/recipient/encoder identities, lawful quality, mapping/abstention coverage где применимо, retention и full time/PC cost. Покажи IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE, условия accept/rollback и неопределённого результата. Отдели pair quality от observed Student gain. Для Builder сравни одинаковый downstream Student start. Не повторяй pair до положительного score и не ждите полной матрицы доменов/Windows/IDE, если они не участвуют в пути».

**Вход/выход:** согласованные actual inputs → reproducible minimal packet с current→target behavior и точными implementation methods/callers, а не набор ссылок для пользователя. **Examination:** другой исполнитель понимает, что менялось/было frozen, где сохраняется смысл пары, кто обновлён, где ordinary use и когда rollback. **Good:** реальный отрицательный результат честно завершает опыт, но не доказывает utility; потерянный target различается с no gain. **Bad:** guaranteed PASS, changed checksum вместо качества, judge-only report без Student outputs/cost. **Edges:** cold/warm mismatch, collapsed sides, zero margin, unchanged weights, insufficient families, changed evaluator, feedback conflict. **Handoff/stop:** implementation — адресный R18 seam; deployment/full product acceptance не объявлены завершёнными. Design-ready и real-run-admitted различаются.
