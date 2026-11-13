# R01–R25: предметные промпты по восстановленным решениям

**13.10.2026 · CP4 · подготовленные задания, не выполненные Rxx.** Эта редакция уточняет существующие 25 карточек, не создаёт ещё 25 запусков и не перезапускает R01–R03. Сохраняются 10 research + 5 analysis + 5 изменения плана/требований + 5 дополнения. Предыдущие полные LEARNING-WORK-ORDERS и их HYBRID-специализации сохраняются. Новое здесь — адрес получателя DPO, семантический порядок, связность групп пар, конкретные доменные контрасты и перенос поздних находок параллельных исследователей в действия.

## Общая часть каждого исполнимого пакета

**Сборка и область.** Dispatcher выдаёт один effective prompt: общая часть исходного work order, полный назначенный Rxx, его относящаяся HYBRID-специализация и предметная карточка ниже. Повторы сводятся в одно требование; уникальные сведения сохраняются. Axx означают разделы RECOVERED-DESIGN-CONTRACT, Hxx — HYBRID-ALGORITHMS-AND-RUNS. Не загружать все 25 карточек, все A/H и все четыре книги каждому исполнителю. Пользователь не должен самостоятельно соединять тексты.

**Текущая задача:** один назначенный документный Rxx. **Не задача:** продуктовый код, новые tests/coverage/build, Windows/GitHub Actions, trainer, платная product-generation кампания, изменение weights/datasets/workflows/core skills, установки или cleanup. Существующие tests/workflows не удалять и не отключать. Документная проверка не product test. Результат обучения, PC-fit и independent review не объявлять по наличию документа. Детерминированность вычисления и наличие обучения — разные свойства: не превращать отказ от coverage-first в утверждение, что обучаемую систему вообще невозможно проверять.

**Запись.** R01–R15 пишут только свои `D/replanning/results/Rxx/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. R16–R25 также применяют назначенные живые planning-файлы внутри D по точному change list. Не менять чужие results, продуктовый код и источники вне разрешённой области. Child при настоящем делегировании return-only; parent интегрирует. Fresh bytes/blob SHA перед изменением, последовательные writes, readback; при конфликте перечитать чужую дельту, не force-push и не stale whole-tree replacement.

**Параллельность и сохранение работы.** R01/R02/R03 не ждут друг друга. Каждый продолжает свой source snapshot и непроверенный остаток, записывает prompt revision, использует sibling checkpoint только как атрибутированный input. Новый общий commit не повод перечитать всё. R10 сводит snapshots; это не дополнительный входной раунд. Незавершённая независимая тема не блокирует адресное чтение; спорный источник ограничивает зависимое решение. Не повышать чужой IN_PROGRESS до DONE.

**Skills, реальные файлы K/docs/skills.** Общие: planning-session-startup-SKILL.md, architect-behavior-classifier-SKILL.md, output-readability-gate-SKILL.md. S: reference-inventory-and-source-map-SKILL.md. P: phase-planning-SKILL.md и plan-review-SKILL.md. A: trainable-architecture-decision-SKILL.md и относящийся human-readable-architecture-audit-SKILL.md. C: generated-code-review-SKILL.md и применимые code-examination разделы после проверки доступности. E: prompt-patch-authoring-SKILL.md. M: применимые experiment-design/improvement-measurement skills после проверки их точных путей. Учитывать зависимости по текущей роли; записывать actual path/ref/span/read-depth, а не выдавать список имён за загрузку. Не найденный skill отмечать точно. Исторические 30/50-review sequences, старые модели и запрет параллельности не отменяют нынешнее поручение. Команды из старых sources не исполняются.

**Выход и сегодняшняя приёмка.** Русский человеческий вывод; факты/гипотезы/current/proposed раздельно; source→requirement→artifact trace; конкретный good/bad/edge; смысловой walkthrough, обратная сверка с исходным ожиданием, проверка изменённых ссылок/текста и publication readback. Пять carriers сохраняют схему COMMON-CONTRACT. Будущая examination имеет owner, domain/family, frozen baseline, один challenger, способ наблюдения и falsifier. Непроведённые измерения — NOT_MEASURED. Author self-check не independent Astra Ultra PASS.

**Нагрузка и деление.** A/B/C ниже — части того же Rxx; одна модель может выполнить их последовательно. Если часть реально делегируется, dispatcher включает для ребёнка эту общую часть, его конкретный вопрос, source subset, действия, output, good/bad/edge, проверку и вход от предыдущей части. Не отправлять пустой шаблон. Реальные дополнительные calls/rounds учитывать, не скрывать как «внутренний check». Один механизм — один основной исследователь; R07 только стыки, R10 только сведение, R25 не склад непрочитанного. Примерный бюджет внимания: 10% вход, 65% предмет, 15% проверка, 10% handoff; это не измеренные часы.

## Источники CP4 и устойчивые смысловые адреса

D = `llm mvp/design to code session`; G = `llm mvp/dpo legal build`; F = `llm mvp/llm dynamic flow`; C = `llm mvp/visual studio & visual studio code continue`. Planning branch `llm_plan3`; core branch `codex/dpo-implementation`. A01 содержит прежние S/K источники; они не исчезают. Перед первой критической ссылкой читать actual source и фиксировать ref/blob/span. Найденный locator не равен прочитанному тексту.

| ID | Источник и опора | Что передаётся; граница доказательства |
|---|---|---|
| P14 | G/active-codex-execution-plan-07-dpo-builder.md, commit `9ab76cac434331a407aa0a01cf642b6543baf287`, §§3.1–3.2a, строки 195–438; blob `d6e37ab55a2bfd4deb057c28376fbb1620f7cf08` | Прямо прочитанный плановый 14-field canon, OrderedPatternIds, reward, N-candidate fan-out. NEW/нет кода — исторический статус, не сегодняшнее отсутствие реализации. |
| N01 | `extensions  - orchestration based on our data/ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md`, тот же commit, 1–90; blob `5bd4a8fc43a1cdcaf4ceda1cd1e764901ac5690d` | Прямо прочитанное противоречие: правило одного дефекта и образец ORCH-03+ORCH-01. Корень вне llm mvp, два пробела в имени существенны. |
| C01 | Core/00.Core/LearningMachineContracts.cs, подтверждённая ветка на `a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3`, 1–105; blob `7f605d49762e3fc0c1b4d4ca490596bc9668cdd5` | Прямой контракт: LearningTrainingItem имеет четыре поля без адреса unit. Это вопрос к внешнему transport, не доказательство отсутствующего bridge. |
| H01 | results/R01/FINDINGS.md F09–F13; прочитанный blob `33b7611caed1fc21912f3e21e8a96b15a884da4b` | Карта core, отдельный composition-contract project и primary `docs/plans/ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md`. Дальнейший reader читает primary адресно. Историческое изменение ответа не равно gain. |
| H02 | results/R02/FINDINGS.md §§6.1–6.5; прочитанный blob `4bf7525e831ed9df2e543259317bc2265698156d` | Позднее сведение 52 требований и recipient/order/reward/fan-out. Старые §7.4/7.7 относятся к прежнему тексту, сохранённому на `334d53438db6e7e7d347dbc9a0570f0151237345`; не использовать эти номера с новой веткой. |
| H03 | results/R03/FINDINGS.md, именованные R03-F11–F16, редакция CP05 | Атрибутированные находки: learned reasoning node в одной L5-машине, raw-word/TaskSignal несовместимость, query-planning, domain-law-feature, thin host. Свой primary read обязателен перед новым code/design claim. |

Для H03 конкретные primary маршруты: C/PLAN-VS-VSCODE-CONTINUE-v1.md §§1.4.1/1.4.7/P3-B/P3-C; `llm mvp/language translate learning/TRANSLATION-TOKENIZER-GUIDE-v1.md`; `llm mvp/orchestrator extension/PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`; `llm mvp/external-sources/03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md`. Версии/span брать из текущего R03 SOURCES, не угадывать номер строки в перемещённом документе. Недоступный primary ограничивает конкретный вывод, не доказывает отсутствие архитектуры.

## R01 — продолжить карту программы, не перезапустить её

**Цель:** все значимые источники и решения получают конкретного читателя. **Skills:** S/P. **Контекст:** A01–A02, H01, свои results, SOURCE-ROUTES, WorkLeft INDEX/REDATE/MASTER, реальные деревья planning/core. R02/R03 advisory, не prerequisites.

**Промпт A.** Продолжи именно непрочитанный остаток рекурсивной карты. Сохрани WorkLeft/LeftToWork/RAG/local-RAG families, S00–S10/дополнительные stages, общие prompts, все три продукта и дополнительные языки. В каждой строке укажи repo/ref/path/blob/span, роль, subject/intent, unique decision, code-search seed, read-depth, следующий переход и принимающий Rxx. Не считай файл с 100 заголовками прочитанным по дереву. Отдельно учти H01: LearningMachine evolution, composition contracts и HE↔EN.

**Промпт B.** Сопоставь P14/N01/H02/H03 с уже существующими source IDs, не создавая дублей требований. Распредели большие смешанные тела по вопросам: representation R04, pair/update R05, routing/graph R06, domain data R08. Проверь literal source roots и архивные единственные носители живого решения. Для unresolved locator сначала сравни parent tree/path/ref; не повторяй тот же 404 и не заключай «архитектуры нет».

**Good:** один большой документ имеет отдельные реальные spans и владельцев; H01 primary назначен адресно. **Bad:** новый HEAD обнуляет R01 либо всё назначено R07. **Edge:** одинаковый basename, два пробела в корне, moved source, RAG cycle, новый домен вне семи.

**Приёмка/выход:** source-owner-capacity карта с явной глубиной и остатком, сверенная с реальным деревом. R04–R08 получают вопрос и span, не «прочитать всё». Будущее качество проверяют R13/R22, а не количество строк карты.

## R02 — сохранить восстановленные решения и точный смысл идентификаторов

**Цель:** поздние находки попадают в прежние 52 requirement rows, а не остаются отдельной заметкой. **Skills:** S/P/A. **Контекст:** свои SOURCES/CHANGE-MAP/DECISIONS/FINDINGS, A01–A09, H02, P14, original/task prompts и WorkLeft owner decisions. R01/R03 не условия старта.

**Промпт A.** Продолжи source→existing requirement mapping. Сохрани typed many→many, Flow correction, async combinations, Graph=L5, Machine training owner, TensorCore, stack-before-normalization, shared/project LoRA, semantic compression, BM25+kNN и quality memory. Внеси точную разницу: NodeOrEdgeId — поле планового recipient; ChosenUnitId/RejectedUnitId — сравниваемые routing alternatives; LearningTrainingItem не содержит UnitId. Не выводи actor ownership из общего суффикса Id.

**Промпт B.** Привяжи семантический порядок, N−1 попытки, teacher-goal Student negatives и правильный reward к прежним строкам. Отдели действующее one-pair/no-repeat от старого replay70/30; сохраняй роли CODEX-selector/pair-judge, admitted generator и Student. Subject/intent/language/project/physical model остаются разными координатами. Старые 95%, 20×, 2 MB и +8% не текущие measurements.

**Good:** строка старого требования дополнена P14 и конкретным counterexample неправильного recipient. **Bad:** удалить graph learning из-за нижней тройки DpoPair или создать 52 новых дубля. **Edge:** два допустимых пути, warning без DPO, source conflict, именованный узел не владелец параметров.

**Приёмка/выход:** обратная трасса source→requirement ID→R04/R05/R06/R12→future observation; актуальные смысловые ссылки вместо старых §7.4/7.7. R16 получает допустимую миграцию нормы; R02 не меняет sibling results и не удостоверяет реализацию.

## R03 — первичные маршруты к восстановленным решениям

**Цель:** завершать visited traversal по доказанному остатку, не повторять известные маршруты. **Skills:** S/P/A. **Контекст:** свой CP05/SOURCES, A01/S04, WorkLeft RAG01–04 MD/NDJSON/DELTAS/MASTER, TASK-PROMPTS-CHRONICLE/COVERAGE, H03. Не ждать R01/R02.

**Промпт A.** Продолжи переходы repo/ref/path/blob: DPO plan-01/02/03/04/06/07/09/10/11/12 по критическим вопросам; RAG04→C/RAG06→F/amendment/DECISIONS; translation/IDE/DTC/merge и найденные nested roots. Сохрани distinction index record/primary read/code trace. Прочитанные 404 записи верхнего индекса не называй 404 прочитанными primary документами.

**Промпт B.** Передай F11–F16 предметно: ORM-43 как learned node одной L5-машины и временный scaffold; несовместимые raw-word/TaskSignal координаты; N01 дефект negative; investigation path шире kNN; law→feature→criterion; thin host/core ownership. Для каждого дай точный primary span, current/historical, вопрос и принимающего Rxx. Историческое edge wording не расширяет unit-level pair без решения.

**Good:** реальная nested RAG ссылка разрешена от её корня и доведена до решения. **Bad:** наличие имени класса или старый READY заменяют source trace. **Edge:** цикл, отсутствующий locator, архив единственный носитель решения, несколько пересказов одного источника.

**Приёмка/выход:** обратная проверка критических утверждений и сохранённый visited/unread graph. R04–R08 получают пригодные spans; R12 — lineage конфликтов. Полнота обхода не выводится из статуса R02; новые документы не считаются выполненным обучением.

## R04 — представление, пространство признаков и typed many→many

**Цель:** установить, что реально получает/возвращает Unit и где сохраняется смысл. **Skills:** S/A/C/P. **Контекст:** A02/A03/A06, S00–S03, K01/K02, H01/H03, WorkLeft code/branch/VERIFY, R02 representation requirements.

**Промпт A — вход.** Проследи actual raw input→StackDetector/parser→normalizer/tokenizer→features/encoder→projection→consumer. Извлечение stack markers должно предшествовать их возможному удалению. Раздели domain/intent, text tokens, typed symbols, vectors, optional codebook и output schema. Для каждой границы запиши input/output meaning, размерность/единицы/версию, сохраняемое/теряемое, state owner и downstream criterion. Отдельно проверь H03 raw-word `oauth` versus TaskSignal `security_auth_flow_task`: lifecycle можно переиспользовать, семантическую совместимость весов нельзя вывести из одинаковой shape. Выход A — source/method/coordinate map.

**Промпт B — выход.** На immutable A проследи IQueryable<GraphNodeCandidate>→OutputRefs→payload requested type→ordinary consumer. Проверь many inputs/many outputs, ленивость, повторное перечисление, diversity versus clones, fan-in, partial/invalid refs. По K02 найди настоящий evaluator comparator; ExpectedResponse сам по себе не доказывает string equality. Семантический порядок OrderedPatternIds не сортировать вместе с порядком показа кандидатов. Выход B — port/materialization/evaluator contract.

**Good:** разные допустимые переводы сохраняют отрицание; semantic paths остаются упорядочены; index сообщает совместимое пространство. **Bad:** одинаковая длина вектора доказывает совместимость или разные входы усредняются без сохранения смысла. **Edge:** mixed stack/script, OOV, negation/units, stale codebook, duplicate/empty outputs, two valid renderings.

**Приёмка:** обратная трасса от payload и evaluation до исходного требования; точная граница собственно прочитанного кода. **Будущий опыт R13/R22:** frozen versus одна representation/projection change на тех же semantic contrasts; task correctness/retained facts/bytes/time, стоимость re-embedding и index compatibility. R05 получает projection boundary, R06 — coordinate constraints. Новые interfaces/fixtures здесь не создавать.

## R05 — точный recipient пары → objective → update → обычный ответ

**Цель:** установить минимально недостающий учебный стык на существующем механизме. **Skills:** S/A/C/P/M. **Контекст:** A02/A05/A06/A09, S04–S06, H02 §§6.1–6.5, P14 §§3.2–3.2a, C01/K02–K06, WorkLeft teaching/INTEGRATION01/TRAINING. Старые R02 §7.4/7.7 не адресовать на свежую ветку.

**Промпт A — поля и смысл.** Найди current producer и trace GraphDpoPairCandidate/Pair→projection→LearningTrainingItem/DpoPair→admission→consumer. Для всех 14 плановых полей укажи kept/transformed/out-of-band/not applicable/missing с основанием. NodeOrEdgeId при Level=unit — recipient, не новая универсальная графовая сущность. Сопоставь winner.UnitId из P14 pseudocode, routing candidate IDs и реального владельца обучаемого выбора. Дай один контраст: solver-A и solver-B сравниваются, selector-S выбирает; чьи параметры должен менять именно этот урок и где передаётся S? Не называй отсутствие поля в C01 отсутствием внешнего transport. TeachingVector/Context/domain/intent/representation version и exact-pair verdict тоже проследить до mutation. GetRow versus Pair-only Shuffle/indexer проверять только по достижимым callers. Выход A — field/address matrix и первый доказанный gap.

**Промпт B — содержимое и градиент.** Проследи SerializeCandidate, OrderLevel/ConsumesPatternIds/CapabilityTags, semantic OrderedPatternIds, policy/reference sequence log-probabilities, masks/reduction/beta, loss/backward, optimizer parameter set/LoRA registration, admission, snapshot, promotion и ordinary inference. DPO reward = beta*(policyLogP-referenceLogP); judge score не его замена. Для N кандидатов различи source rows, pair attempts, admitted lessons, optimizer steps и promotions. Teacher-goal losers проверить как Student-origin; не переписать authorship. One-pair/no-repeat не отменяется fan-out.

**Good:** unit address/context перенесён внешним carrier и прочитан до update; A/B-параметры selector-S используются последующим inference. **Bad:** добавить UnitId в DTO и объявить цепь готовой; update solver-A по уроку о selector-S; судейский 0.9 подставить вместо log-prob reward. **Edge:** legacy admission=null, swapped sides, обе стороны равны, reference меняется, unreachable gradient, неоднозначный serializer, pending update, index drift.

**Приёмка/выход:** method/ref/producer/consumer у каждого утверждения, literal schema отдельно от логического recipient, минимальный missing seam вместо нового trainer. **Будущий опыт R13/R22:** same-panel before/after плюс no-update, качество независимых новых families и retention, все failed-attempt costs. R12 получает ownership/objective решения, R18 — bounded implementation seam; actual gain NOT_MEASURED.

## R06 — домены, dynamic kNN и графовая власть выбора

**Цель:** отделить ошибку домена, retrieval и выбора пути; определить один следующий challenger. **Skills:** S/A/C/P/M. **Контекст:** A02–A04/A08, S07–S10, R03 primary routes/H03, F/DECISIONS D04/D07/D10/D13/D15, R04 coordinates, R05 state constraints; H01 composition-contract project.

**Промпт A — retrieval.** Проследи request→subject/intent/stack→eligible pool→features/normalization→metric/similarity→k/radius→positive/negative/warning history→final assessment. Сохрани BM25+kNN как восстановленный дизайн, не доказанный optimum. Для каждого домена запиши candidate distance и основания её признаков; query-dependent weights/k должны иметь signal, trainable owner и consumer, иначе это fixed policy. Отдельно разложи learned metric, memory insertion и adaptive selection: это разные причины улучшения. Approximate index сравнивать с exact search на ограниченном том же pool; query-dependent/неметрическая distance может нарушить предпосылки выбранного индекса. Не вводить новый движок только ради сравнения.

**Промпт B — граф.** На immutable A проследи learned authority, reasoner ORM-43 и цепь IntentDecomposer→GuideRetriever→PlanComposer→RiskAssessor, насколько она подтверждена current sources/code. Graph остаётся L5 specialization; временный scaffold не финальная способность. Проверь пять relation kinds, multi-output, fan-out/fan-in, bounded correction/return loop, async/partial/cancel и ordinary consumer; ParallelEligible не новый edge kind. Investigation path из H03 F14 должен собирать нужные evidence, не только ближайший текст. Выход B — role/port/authority map и конкретный fixed-versus-learned contrast.

**Good:** SQL×BUG выбирает правильный проект, видит write/caller effects; warning не превращается в positive. **Bad:** один cosine/k объявлен лучшим везде; nearest text признан полным планом; graph size признан интеллектом. **Edge:** unknown/mixed/sparse domain, stale negative, wrong project, incompatible encoder/index, combinatorial growth, late correct branch.

**Приёмка/выход:** отдельно actual versus diagnostic oracle-domain, retrieval adequacy и solver/path quality. **Будущий опыт R13/R22:** per-domain confusion/unknown handling, retrieval utility, constraint violations, solution quality, all-branch latency/memory/reindex. Oracle input eval-only, не скрытый production shortcut. R07 получает лишь непрояснённые межкомпонентные связи.

## R07 — причинная связь четырёх ролей и адрес учебного сигнала

**Цель:** понять, какой механизм мог изменить решение и какой урок ему адресовать. **Skills:** A/C/P/M. **Контекст:** A02/A05/A08, immutable R04–R06 maps, P14/H02/H03, WorkLeft MECH01/RESEARCH03.

**Промпт A.** Собери одну прослеживаемую цепь request→domain→representation→neighbors→Unit/Flow/Orchestrator/Graph→result→weakness→pair→owner update→ordinary consumer. Для каждого перехода назови реальные transport, types/coordinate versions, cardinality, meaning, allowed information loss, deterministic constraints и learned final authority. Не исследуй заново весь код; работай с несовпадениями карт. Отдельно разбери winner.UnitId versus recipient authority, и learned reasoning node versus hollow adapter. Выход A — связная карта механизмов и нерешённых seams.

**Промпт B.** Заполни отдельные вмешательства: fixed Units/change edge policy; fixed connections/change Unit projection; same solver/change domain router; same solver/validity rules/change async route policy; flat patterns/one higher-order composition. Для ordered paths сохраняй prerequisite order, но меняй представление массива кандидатов как bias probe. Объясни credit: какое конкретное различие доступно target unit и почему global path reward не разрешает обновить все рёбра. Только при явной interaction-гипотезе добавь одно 2×2, например projection×metric. Выход B — интервенции, frozen state и falsifiers.

**Good:** при фиксированных solvers правильная связь найдена чаще, качество сохранено и полная цена ниже. **Bad:** новый prompt+retrieval+LoRA одновременно назван чистым LoRA gain; все branches получают общий reward. **Edge:** верный route/неверный solver, одинаковый content с разными roles, cycle, полезный поздний ответ, отсутствующий gradient channel.

**Приёмка/выход:** solved architecture block с selected/rejected alternatives, source seams и мысленным прохождением конкретного input/lesson. R12 получает выбранные connection/credit решения, R13 — минимальное различающее наблюдение. Диаграмма без producer/consumer не закрывает R07; code/tests не писать.

## R08 — доменные законы, данные и содержательные DPO-контрасты

**Цель:** дать каждому предмету наблюдаемую способность, а не только label и accuracy. **Skills:** S/A/P/M. **Контекст:** A03/A05/A07/A08, WorkLeft RESEARCH01/REVIEW20/RESEARCH02, planning of testing, P14/N01/H02/H03 и R02 additional scope.

**Промпт A.** Переиспользуй существующее исследование. Для baseline семи и найденных дополнительных направлений заполни domain passport: subject/intent/language/stack/project; source/version/type закона (invariant, approximate assumption, user policy, expert preference, unknown); input/output; важные symbols/features/relations; pool/metric/k candidates; available data/provenance/permissions; independent families/split; confounders и перенос. Нельзя назвать привычный формат корпуса законом мира или 100 парафраз одного задания 100 независимыми families.

**Промпт B.** Разверни следующие иллюстративные cases в точные исследовательские задания с actual sources. Это не заранее допущенные training rows и не закрытый экзамен.

| Домен | Сохраняемое различие / chosen смысл | Правдоподобный rejected | Что измерять и край |
|---|---|---|---|
| EN↔RU | «Do not delete rows with NULL» сохраняет отрицание и NULL | Гладкий перевод разрешает удаление | Semantic adequacy/critical negation error, отдельно направления; два верных перевода = TIE. |
| Оркестрация | Зависимые шаги идут после своих prerequisites | Те же шаги в нарушающем зависимость порядке | Valid path/completion/full cost; независимые перестановки могут быть равноценны. |
| Figma→HTML/CSS | Сохранены элементы и поведение на узком/широком viewport | Один красивый render скрывает пропавший элемент | Structure/visual/functional criteria по разрешениям; missing image нельзя выдумывать. |
| C# documentation | Документ отражает действительные calls/exception/side effects | Пропущен один доказанный write effect | Source-grounded precision и полнота обязательных фактов отдельно; отсутствие source признаётся. |
| Angular documentation | Component/template/state/request relation соответствует коду | UI-событие ошибочно описано как HTTP request | Верные relations и версии; async/conditional path. |
| SQL documentation | Указаны NULL/dialect/transaction/caller effects конкретного source | Сходный запрос описан как read-only при наличии write | Семантика result/effects и impact path; неоднозначный dialect требует ограничения вывода. |
| DPO Builder | Сопоставимые стороны различаются целевым причинным дефектом, recipient верен | Лёгкий мусорный negative либо урок не тому selector | Pair validity/quality/admission yield отдельно от Student gain и total cost; BOTH_BAD/UNJUDGEABLE не forced preference. |

**Промпт C — отрицательные примеры и lineage.** N01 single-error правило и ORCH-03+ORCH-01 образец разнеси: один контраст меняет только learned authority на hardcode при одинаковом правильном порядке; другой меняет только semantic order при неизменном authority. Совместный дефект — отдельно помеченная interaction case, не single-factor success. `[WRONG:...]`, judge rationale и source/order IDs проверять как потенциальные label cues; выводить диагностические сведения в разрешённый carrier, не менять schema/projection самовольно. P14 N−1 попытки сохраняют одну исходную lineage и не раскидываются между train/certification. Teacher-goal и synthetic-negative ветви не смешивать без explicit provenance/policy решения.

**Good:** закон→feature→контраст→ожидаемая ошибка→metric и family. **Bad:** одинаковый cosine и порог95% всем, marker раскрывает сторону, style-only negative выдаётся за логику. **Edge:** tie, lawful abstention, both bad, false law, contradictory feedback, rare/mixed/unseen domain.

**Приёмка/выход:** заполненные паспорта/контрасты с sources, независимые семейства и явные data gaps; R13 получает первый пригодный panel plan, R21 — curriculum без повторного исследования. Исторические 20 reviews не новые Rxx; новые пары здесь не генерируются платным провайдером и не обучаются.

## R09 — стоимость процесса и действительная роль Windows

**Цель:** снять лишние зависимости первого урока по фактам. **Skills:** S/P/C/A. **Контекст:** A08/A09, WorkLeft test-batching/TEST-BATCHES/DEC01/02/04/MATRIX/REPORT, существующие workflows/logs/commits read-only, actual dependencies R04–R06.

**Промпт A.** Для ограниченной объяснимой выборки повторов выпиши original expectation→production change→assertion/fixture diff→workflow/job→queue/execution/retries→новое evidence→зависимый учебный шаг. Раздели исправление неправильного fixture, изменение контракта и ослабление ожидания под реализацию. Сам факт смены expected value не доказывает подгонку. Не выводи рабочие часы человека из длительности CI. Выход A — cost/cause ledger с измеренным и неизвестным.

**Промпт B.** Классифицируй gate: нужен для разрешённого корректного update; нужен конкретному installed Windows consumer; release-only; дублирует evidence; unrelated первому опыту. Дай bounded alternate path через существующую Machine, не bypass admission/state. Foreground/idle learning/отдельная кампания различаются. GitHub write permission уже задан, не задавай прежний вопрос об одобрении.

**Good:** перенесён конкретный host gate, не используемый первым domain route; поздний owner сохранён. **Bad:** все Windows runs объявлены лишними либо все обязательны по привычке. **Edge:** неизвестный exit, retry мог повторить update, скрытая teacher assistance, нет timestamps.

**Приёмка/выход:** keep/reuse/defer/remove-from-critical-path с доказательством/риском/owner R17/R19. Не запускать новые jobs ради аудита. R11 получает causal dependencies, R23 — hardware/cost evidence; unknown не равно нулю.

## R10 — свести evidence без повторной разведки

**Цель:** передать анализу достаточную общую карту и точные неизвестности. **Skills:** S/P/A. **Контекст:** A01, frozen packets R01–R09, P14/N01/H01–H03, source/workload maps.

**Промпт A.** Сведи source→requirement→mechanism graph. При разных snapshots сравни только относящиеся blobs/spans. Включи поздние recipient/order/reward/fan-out, raw-word/TaskSignal, reasoning-node/query-path и negative conflict. Разные пересказы одного primary не независимые подтверждения; индекс/контракт/код/наблюдение не один уровень доказательства.

**Промпт B.** Для каждой существенной гипотезы запиши fact→cause→alternative→falsifier→owner. Проверь scope: original prompts, WorkLeft/RAG, все slices, четыре роли, domain/kNN/DPO/LoRA, данные/права, ПК/feedback, DTC/IDE. Непрочитанное адресовать тематическому owner с вопросом/объёмом/capacity, не «дочитать всё в R10». Замерь только доступную цену; не превращай черновую оценку в measurement.

**Good:** первичный конфликт one-pair/replay передан с lineage, а старые R02 section links разрешены по смыслу. **Bad:** sibling IN_PROGRESS переименован DONE для таблицы. **Edge:** поздняя смена источника, unavailable primary, несовместимые гипотезы.

**Приёмка/выход:** граница у каждого вывода, owner у critical gap, обратная сверка original scope. R11–R15 получают относящиеся evidence subsets. Полная программа не объявляется исследованной при gaps, но независимый достаточный first-experiment packet не блокируется чужой темой.

## R11 — причинные prerequisites первого урока

**Цель:** сократить время до содержательного обучения без удаления продукта. **Skills:** P/A/S. **Контекст:** A08/A09, R09/R10, R04–R07 actual dependencies, текущие roadmap/queue.

**Промпт A.** Построй отдельно DAG first learning и DAG full DTC/IDE/Windows delivery. Каждое «сначала» обоснуй data/state/permission/recipient/consumer зависимостью и конкретным неуспехом без неё. Исторический порядок, review counter и banner недостаточны. Определи реальный reuse и first missing seam; effort known/unknown отдельно.

**Промпт B.** Выбери минимальную последовательность: существующий baseline/solver, корректный pair-owner transport, изолированный update и возврат принятого state в ordinary consumer, сопоставимое observation. Полный domain sweep/IDE packaging отложи только с owner/trigger. Не вставляй все R18 B/C задачи перед первым уроком без зависимости. Содержательная неизвестность owner/objective не переносится скрыто в implementation.

**Good:** первый опыт на реальной Machine не ждёт неиспользуемого UI-порта. **Bad:** отдельный demonstrator trainer или снятие всех gates без понимания потребителей. **Edge:** first route действительно Windows-dependent; данных нет; разрешённый teacher недоступен.

**Приёмка/выход:** у снятого prerequisite сохранён поздний consumer, у необходимого — causal reason. R12 получает варианты, R14 — новый порядок. Ускорение пока estimate/NOT_MEASURED, если нет фактических затрат.

## R12 — согласовать гибридную архитектуру и функцию обучения

**Цель:** выбрать минимальный дизайн без потери восстановленных решений. **Skills:** A/P/C/M. **Контекст:** A02–A06/A09, R02/R03 decisions, R04–R07 traces, R11, P14/N01/H01–H03.

**Промпт A.** Для domain mapping, representation, dynamic retrieval, edge/reasoning authority, DPO/LoRA и state выбери существующего owner и один первый challenger. Запиши selected/rejected alternatives, реальные interfaces/signatures, deterministic admissibility, learned authority, signal/objective, serialization/coordinate version, checkpoint/index compatibility и ordinary consumer. Сохрани Graph=L5, TensorCore/shared-project design. LoRA — конкретная матрица/проекция; codebook/tokenizer не её синоним.

**Промпт B.** Реши предметно recipient ambiguity P14 winner.UnitId: является ли он в выбранной ветви authority target, и как этот адрес доходит через C01 без поля UnitId? Сохрани exact 14-field canon; расширение только explicit migration. Для OrderedPatternIds выбери проверяемую сериализацию с сохранением семантического порядка и без label/provenance shortcut; candidate-array invariance не semantic sorting. Определи probabilistic policy/reference/objective. Если доступен лишь произвольный similarity/ranking, назови самостоятельный метод и сравнение, не «стандартный DPO».

**Промпт C.** Разреши one-pair/replay, N−1 attempts versus actual update, teacher-goal/student-negative versus synthetic-negative policy, intent/subject и campaign/idle conflicts. Условия TIE/BOTH_BAD/UNJUDGEABLE не forced preference. N01 образец разделить либо явно обозначить interaction. Если нынешний unit-level owner не способен учить нужное отношение, сохрани требование и дай точный migration design, не удаляй graph learning. Не делегировать выбор loss/owner/schema/permission исполнителю R18.

**Good:** selector-S получает адресованный урок о выборе solver-A/B, endpoint после update использует его state. **Bad:** reward=judge score, update winner solver без причинного объяснения, шестой уровень из alias. **Edge:** нет sequence probabilities, два checkpoint owners, конфликт правил, несовместимые координаты, оба пути правильны.

**Приёмка/выход:** walkthrough input→path→pair→target mutation→inference; solved design либо конкретный blocker, empirical superiority не обещается. R13 получает зафиксированный сравниваемый вариант; R16/R18 — выбранные изменения и rejected alternatives.

## R13 — самый ранний различающий учебный опыт

**Цель:** определить, как обнаружить gain, regression, неопределённость или негодные evidence. **Skills:** M/A/P. **Контекст:** A07–A09, R08 families/passports, R12 chosen owner/design, LEARNING-EVALUATION-CONTRACT, actual evaluation seam R04.

**Промпт A.** Выбери первый домен по пригодности baseline/данных/стоимости, не по свежести S04. До результатов зафиксируй state refs, target capability, changed factor, train/dev/certification family split, panel E, unit of analysis, sample-size rationale, rubric, severe-error boundary, practically meaningful quality delta и quality tolerance при проверке экономии. Значения брать из owner goals либо явно обоснованного recipe; не подбирать после outcomes. N−1 fan-out одного request — одна связанная family, не N−1 независимых observations. Никаких invented actual scores.

**Промпт B.** Определи before/after на одинаковой E и no-update контроль с тем же context/memory/compute access; отдели warm/cold cache и внешнюю помощь. Record raw outputs, domain/path decisions, time stages, all branches/failed attempts, RAM/VRAM/state bytes, retention. Дай способ оценить uncertainty на уровне independent families и различать изменение ответа от качества. Operational repeated panel пригоден для решений разработки, но не «untouched certification»; closed certification не отдаётся selector/generator и не превращается в бесконечный способ выбора лучшего checkpoint.

**Промпт C.** Запиши решение заранее: IMPROVE — доменная полезность улучшилась достаточно либо цена снизилась при сохранённом качестве/ограничениях; REGRESS — существенное ухудшение или нарушение обязательного свойства; INCONCLUSIVE — валидных данных недостаточно; INVALID_EVIDENCE — leakage, несовпадающие условия, неверная lineage/версия. Не путать отсутствие статистически различимого выигрыша с доказанным равенством. Accept/rollback остаётся действующей политикой; неопределённость не автоматическое promotion. Выбери второй существенно иной домен и unseen composition для следующего расширения, не требуй их завершить до первого опыта.

**Good:** два состояния сравниваются на тех же независимых задачах; корректная но более дорогая модель не названа компактнее. **Bad:** разные сложности bulk сравниваются как gain или changed hash доказывает обучение. **Edge:** мало families, разногласие judges, rare regression, OOM, contamination, valid no-gain.

**Приёмка/выход:** готовый first recipe без выбора rubric/objective на ходу; exact implementation dependencies; NOT_MEASURED поля отдельно от порогов. Передать R18 сразу по достаточности; R22 расширяет, а не задерживает первый recipe.

## R14 — learning-first последовательность всей программы

**Цель:** первый урок, развитие алгоритмов и конечная поставка связаны только реальными prerequisites. **Skills:** P/A/S. **Контекст:** R11 DAG, R12 design, R13 recipe, R02/R10 full scope, A02/A07–A09.

**Промпт A.** Построй capability milestones: usable baseline/state→минимальный dynamic seam→первый допустимый lesson/measurement→иной домен→learned connections/higher-order→широкий curriculum→DTC/IDE delivery. Каждая строка содержит original requirement ID, reuse/new delta, dependency, owner, observable outcome и cost/unknown. Не заменяй отсутствие causal progress количеством commits.

**Промпт B.** Сопоставь все S00–S10/дополнительные tasks: keep/reuse/defer/supersede-with-reason. API/render/DB/publication/ordinary consumer/user-tier cost/Windows acceptance сохраняют поздний trigger. Проверить, что first lesson не ждёт полного grid всех kNN/LoRA/graph alternatives либо независимых R01–R25 outputs. Дополнительный реальный round — только конкретный новый вопрос, источники, capacity и учтённая цена, не автоматический boilerplate.

**Good:** reuse существующего learner и bounded semantic seam; будущий IDE endpoint сохранён. **Bad:** S04 объявлен done ради допуска или прежние цели удалены. **Edge:** shared component, parallel owners, stale version, необходимая поздняя data dependency.

**Приёмка/выход:** backward trace конечных результатов, no artificial cycles, один понятный first path и rejected sequencing alternatives. R15 получает direction candidate, R19 — применяемый roadmap diff; runtime readiness не заявляется.

## R15 — независимая Astra Ultra экспертиза направления

**Цель:** предметно опровергнуть слабый дизайн до дорогого исполнения. **Skills:** A/C/P/M; отдельный настоящий reviewer. **Контекст:** exact R10–R14 candidate refs, A02–A09, P14/N01, выбранные primary spans, доменные examples и first recipe.

**Промпт.** Проследи четыре роли/owner и один path domain→representation→kNN→graph, затем pair→recipient→objective→mutation→ordinary consumer. Проверь NodeOrEdgeId versus solver ID, semantic versus presentation order, one-pair versus fan-out, generator versus judge, raw-word versus typed coordinate compatibility. Попытайся объяснить обещанный gain памятью без обучения, leakage/WRONG markers, облегчёнными negatives, extra compute, сменой rubric/panel, teacher intervention или потерей редких cases. Для каждого возражения укажи source, scope, различающий counterexample, минимальную repair и owner. Раздели design blocker от empirical unknown с готовым опытом.

**Good:** reviewer обнаружил неправильный recipient или смешение двух ошибок и дал bounded исправление. **Bad:** PASS по красивой схеме, списку checks либо строке requested model. **Edge:** reviewer недоступен, actual model/effort отличаются, нет primary evidence, вердикт низкой уверенности.

**Приёмка/выход:** actual invocation/model/effort/identity насколько доступны, exact reviewed candidate, независимый аргумент и scope verdict. Нет отдельного вызова — NOT_PERFORMED, не авторская независимость. Рецензия не заменяет experiment и не запускает trainer. R16–R20 получают адресные repairs; R25 проверит судьбу замечаний.

## R16 — применить требования и точную схему адресации

**Цель:** восстановленный дизайн работает как действующая норма, не новая записка рядом. **Skills:** P/A/E/S. **Контекст:** R02 existing IDs, R12 decisions, R15 remarks, A02–A09, P14/N01 и active requirements consumers в D.

**Промпт A.** Составь exact change list и внеси source→old→new→examination mapping в действующие requirements. Сохрани four roles, many→many, domain mapping/representations/kNN, DPO/LoRA, full PC/feedback, конечную95%goal и дополнительные language/product obligations. Уточни буквальное NodeOrEdgeId и смысл recipient transport, ordered serialization, lineage и три независимых вопроса admission/pair quality/Student utility. Не создавать новую параллельную 52-row матрицу.

**Промпт B.** Примени только выбранные R12 policy/ownership/schema changes. 14-field canon и nested provenance не расширяются случайным diagnostic полем; out-of-band transport указан явно. N01 single-factor versus interaction должен быть отражён в активных examples внутри D; первичный файл вне D не переписывать этой задачей, дать точный неприменённый patch/owner. Старый replay/старую role норму сохраняй исторически с ясным successor. TensorCore/stack/shared-project/Graph=L5 не отменять удобством автора.

**Good:** source requirement дошло до активного consumer и имеет наблюдаемый falsifier. **Bad:** оставить «DPO последним» в рабочем входе, опубликовав только новый отчёт. **Edge:** parallel edits, primary outside D, unresolved R12 policy, изменившийся schema owner.

**Приёмка/выход:** fresh bytes/SHA, смысловой diff по requirements, реальные changed files/readback, вне-scope неприменённое отдельно. R18/R19 получают действующие нормы, не рекомендации; продуктовые DTO не меняются в R16.

## R17 — learning examination вместо coverage-first приёмки

**Цель:** качество подтверждается поведением обучения, не ростом checks. **Skills:** P/A/E/M. **Контекст:** R09 evidence, R13 recipe/thresholds, R15 remarks, A05/A08/A09, active acceptance/playbooks в D.

**Промпт A.** Для снимаемой ближайшей зависимости укажи original gate, actual purpose, consumer и почему он не нужен именно first scenario. Сохрани реальные state/provenance/permission/recipient boundaries. Software integrity, learned usefulness и installed Windows delivery — разные вопросы и доказательства. Не выводи «все tests ненужны» из недетерминированности.

**Промпт B.** Измени planning exit criteria: допустимость/качество exact pair; reachable update/consumer; same-panel per-domain quality/time/full resources/retention; отдельно четыре решения R13. Технический hash/shape/counter не learning result. Не требуй буквальный golden answer там, где несколько семантически правильных outputs. Существующие product tests/workflows не отключать, новые tests не писать и не запускать. Внешние annotations не становятся подсказывающим оценочным target.

**Good:** два корректных перевода допустимы, потеря отрицания различима. **Bad:** переименовать fixture в examination, сохранив подгонку ответа; skipped job назвать PASS. **Edge:** legacy comparator, inconsistent checkpoint, утечка operational/certification, нет независимых source families.

**Приёмка/выход:** criterion→capability→observation, deferred gates имеют owners/triggers. R19/R20 получают применённые правила и exact remaining restrictions; новое learning evidence не выдумывается.

## R18 — полные future implementation-поручения для минимальных связей

**Цель:** следующий coding worker получает узкую решённую задачу, не новый research project. **Skills:** A/C/P/E/M. **Контекст:** R04–R07 traces, R12 selected design, R13 first recipe, R15 remarks, R16/R17 applied rules, A02–A06/A09, P14/H02/H03.

**Промпт A — первый урок.** Выбери доказанно первый missing seam на actual pair→target owner→objective/update→ordinary consumer. Полностью заполни future packet: current files/classes/methods/signatures; exact code delta и non-goals; recipient transport/manifest; ordered payload и versions; deterministic/learned boundary; chosen objective/reference; admission/error/recovery/snapshot/one-pair; first examination steps и ожидаемые типы наблюдений. Seeds K02–K06 заменить адресами R05. Пример результата: selector-S получает contextual preference между A/B, изменяются его разрешённые параметры, ordinary request читает принятое состояние. Не обещай gain до измерения. Если seam уже реализован — reuse и examination, не новый TrainGraph.

**Промпт B — representation/retrieval.** На immutable A подготовь отдельный полный future packet для одного выбранного R12 изменения: input features/token schema, actual projection/LoRA parameters и consumer, domain pool/metric/k, version/index compatibility, TensorCore/reuse, baseline/falsifier, reindex/full footprint. Raw-word→TaskSignal не weight-copy по одинаковой dimension. При выборе нового k/index объясни reachability и signal. Если B не prerequisite A, это явно subsequent task.

**Промпт C — connection.** На R12 ports подготовь отдельный полный future packet первого отсутствующего Flow/Orchestrator/Graph механизма: actual producer/consumer, many→many, allowed edges, authority unit/recipient, reasoning-node state, semantic path order, async fan-in/out/partial/cancel/budget, credit signal и fixed-versus-changed connection experiment. «Первый завершившийся» не означает лучший правильный путь. Если связь готова, выпусти только reuse/experiment packet.

Каждый A/B/C при реальном делегировании получает свои concrete context/skills/RAG/ref, code delta, selected design, actions, output, good/bad/edge, сегодняшнюю проверку полноты prompt и будущую learning examination. Не оставлять типы/loss/owner/policy TODO; конкретный нерешённый design вернуть владельцу R12. Это не разрешение coding/training здесь.

**Good:** один исправляемый transport/consumer и затем первый сопоставимый lesson; B/C не искусственные blockers. **Bad:** одновременно новая tokenization+kNN+LoRA+graph либо абстрактное «улучшить интеллект». **Edge:** pending update, legacy payload, swapped sides, owner mismatch, stale index, late/cyclic branches.

**Приёмка/выход:** walkthrough будущего исполнителя без новых архитектурных решений; first packet+recipe передать сразу по достаточности, остальные отдельно. Никаких новых product tests/build/run в R18, implemented status не повышать по тексту prompt.

## R19 — применить learning-first roadmap к живым очередям

**Цель:** продолжение действительно выбирает новый critical path. **Skills:** P/E/S/A. **Контекст:** R14 roadmap, R16–R18 applied requirements/work orders, actual slice/WorkLeft consumers R01–R03, A09.

**Промпт A.** Перечисли active entrypoints/roadmaps в D и source requirement каждого. Примени bounded dependency/next-action/reuse/deferred-trigger updates ко всем относящимся текущим очередям, сохраняя S00–S10/extra stages. Исторические transcripts не переписывать; старые code-absence/status claims не переносить как сегодняшние. Source section renumbering разрешать по именованному решению и blob, а не старой цифре.

**Промпт B.** Проследи «continue→dispatcher→first R18 packet→R13 recipe» и отдельно поздний DTC/IDE путь. Проверь, не вернулся признанный unrelated Windows prerequisite через вторую очередь. Реально необходимый host/data/state blocker не прятать. B/C improvements не должны автоматически стоять перед A.

**Good:** next-action ведёт к пригодному минимальному пакету с реальными prerequisites. **Bad:** новый roadmap рядом со старым START либо объявленный whole-program DONE. **Edge:** два pack aliases, concurrent edits, executable overlay core outside scope.

**Приёмка/выход:** propagation matrix, actual changes/readback; вне-scope consumers и будущий owner явно не обновлены. R20 получает launch surfaces, R24 — preserved delivery path. Не заставлять независимую работу ждать всех25.

## R20 — один достаточно подробный model packet без повторной загрузки

**Цель:** модель понимает конкретную задачу, её контекст и доказательство результата. **Skills:** E/P/S/A. **Контекст:** START/AGENTS/COMMON, designated original/HYBRID/RECOVERED orders, R18/R19, A01 и только относящиеся sections.

**Промпт A.** Примени в настоящие starting/continuation prompts внутри D current task/non-task, exact refs/spans и смысловые locators, actual skill load, RAG roots, authority/evidence distinction, allowed writes/commands, concrete actions/output/good/bad/edge, present verification/future examination. R02 §6.1–6.5 либо explicit историческая опора заменяют недействительные §7.4/7.7. Убери обязательное дублирующее чтение, сохрани уникальное требование и его источник.

**Промпт B.** Разверни три полностью заполненных documentary dispatch examples: продолжающийся R03 без sibling DONE; R05 с C01 четырёхполевым payload и неизвестным external recipient transport; R18 с already implemented seam, которому нужен reuse/examination вместо второго trainer. В каждом назови exact source subset, outputs и counterexample. Это не три вызванных агента. Для настоящего child разверни его A/B/C целиком, а не список fields для заполнения им.

**Good:** worker продолжает CP, читает только changed relevant spans и знает, что будет измеряться после разрешённой реализации. **Bad:** каждый получает все25 карточек/весь архив либо сам user должен склеить документы. **Edge:** context limit, missing skill, changed blob, desired model alias versus actual runtime identity.

**Приёмка/выход:** однозначный dispatch trace, meaningful complete packet, readback; historical30/50/Windows не переопределяют scope. R21–R25 используют новый вход, без нового автоматического planning cycle.

## R21 — curriculum с настоящей шириной и самостоятельным Builder

**Цель:** новые уроки учат новым различиям и композициям, не размножают один шаблон. **Skills:** M/A/P/S. **Контекст:** R08 passports/cases, R13 recipe, R16 requirements, A03/A05/A07/A08, unique existing RECIPES/PLAYBOOK sections, P14/N01/H02.

**Промпт A.** Для семи baseline предметов заполни последовательность simple relations→near-miss→mixed/unknown→unseen composition→transfer. Каждая ступень: sources/family, law/features, representation/metric candidates, target owner, positive/negative/TIE/BOTH_BAD, mastery observation и retention. HE↔EN/server/client/stored procedures и прочие исходные задачи имеют собственный маршрут. Новые combinations и domain-law profiles, а не число pair files, задают breadth.

**Промпт B — Builder packet.** Вход: конкретная weakness Student, разрешённый domain/source subset и target owner. Действия: отделить admission от semantic quality; выбрать законный producer branch; получить сопоставимые варианты с одним причинным дефектом; сохранить exact text/authors/context/recipient/lineage; передать exact pair CODEX/Astra examiner; не присваивать train-ready самому. P14 teacher-goal branch не превращать в teacher-vs-teacher; синтетический injected-negative specimen не считать автоматически разрешённым v1 update. Source N−1 попытки не новые независимые families. `[WRONG:]`/reason держать в назначенной диагностической роли по R12, не подсказкой preferred label в consumer text.

**Good:** negative ошибается в prerequisite order, а остальные свойства сопоставимы; quality и downstream Student utility оценены отдельно. **Bad:** мусор, другая задача, style-only отличие, N01 multi-error под single-error правилом. **Edge:** повтор source family, lawful refusal, both wrong, private/unlicensed source, copied exam rationale.

**Приёмка/выход:** заполненные curriculum/Builder work order и family split, R08 primary reuse без выдуманного corpus. Размеры/последовательность обоснованы и не блокируют первый опыт; R22 получает конкретные experiments, R23 — growth model. Пары не генерируются и не обучаются этим документным R21.

## R22 — различающий рецепт для каждого выбранного алгоритма

**Цель:** разложить качество, скорость и компактность по причинам. **Skills:** M/A/P/C. **Контекст:** R12 selection, R13 fixed decision rules, R18 actual future entrypoint, R21 curriculum, A03–A08, existing recipes и P14/H02/H03.

**Промпт A.** Заверши first recipe: baseline/challenger snapshots, identical panel E и permissions, one changed state, no-update branch с той же доступной памятью/контекстом, raw outputs/time/resources, family-level comparison, retention и четыре решения R13. Учёт времени: domain/representation/retrieval/path execution плюс generation/judging/update/evaluation/promotion/reindex и неуспешные попытки; отдельно offline preparation и foreground. Bulk — группа новых lessons при действующей one-pair policy. Сравнивать разные panel difficulties как improvement нельзя.

**Промпт B.** Заполни отдельные bounded recipes только для назначенных hypotheses:

| Механизм | Заморожено / изменение | Различающий результат |
|---|---|---|
| Domain mapping | Те же solver/representation; actual routing versus eval-only oracle label | Доля domain-caused ошибок, mixed/unknown handling; downstream quality отдельно. |
| kNN metric | Тот же pool/encoder/k; fixed versus domain-conditioned metric | Полезные соседи и task gain; отдельно approximate versus exact search на том же ограниченном pool. |
| Dynamic k | Те же pool/metric/solver; fixed versus selected k/radius | Качество при сопоставимом cost и abstention на слабом соседстве, не только hit@k. |
| BM25/kNN/history | То же представление и learner; один fusion/history change | Retrieval/memory gain, а не автоматически gradient learning; warning не positive. |
| LoRA/representation | Те же data/task/allowed downstream conditions; конкретная matrix change | Сохранение смысла, downstream quality и полная стоимость; совместимость index учитывается как отдельная цена. |
| Flow/edge authority | Units фиксированы; меняется одна connection policy | Корректная коррекция/маршрут и причинно адресованный owner; не общий reward всем edges. |
| Orchestrator | Те же valid candidates/solvers; fixed versus learned combination/stopping | Success/quality с ценой всех branches; быстрая ошибка не победитель. |
| Higher-order graph | Те же primitive patterns; одна композиционная способность | Новые compositions решаются лучше; candidate shuffle инвариантен, semantic step order сохраняется. |
| DPO/Builder | Одинаковая weak capability/admission; exact pair и выбранный update | Pair quality отдельно от post-update generalization и retention; fan-out family не псевдонезависима. |

Для каждой выбранной строки выпиши полный model-ready subpacket: task/non-task, named skills, exact context/code/data refs, frozen/changed state, sequential actions, artifacts, good/bad/edge, decision/falsifier, budget/stopping и handoff. Эти строки не приказ выполнить полный Cartesian sweep; оформить только реально выбранные R12/R13 опыты. При проверенной interaction-гипотезе одно 2×2, например projection×metric, вместо смешения причин.

**Good:** улучшение retrieval без weight update так и названо; semantic order не теряется при candidate permutation probe. **Bad:** train на certification, перепутанный recipient, одна суммарная accuracy скрывает доменный регресс. **Edge:** small effect, invalid panel, OOM, metric/ANN mismatch, coordinate drift, arbiter overridden by similarity.

**Приёмка/выход:** каждый recipe исполним без нового выбора objective/команды; есть опровергающее наблюдение, не только success condition. Control samples не training targets. R25 проверяет design, не выдаёт будущие опыты за проведённые.

## R23 — полный ПК-budget и продолжение на feedback

**Цель:** измеримо проверить весь цикл на одной локальной машине. **Skills:** A/P/C/M. **Контекст:** R09 actual environment/cost, R12 ownership, R18 transport, R21 growth, A06/A08/A09, H01/H03 lifecycle.

**Промпт A.** По разрешённым evidence установи actual hardware/runtime; прежняя конфигурация не сегодняшнее measurement. Budget: base/model/shared-project adapters, optimizer/activations, tokenizer/codebook, kNN index, graph, cache, snapshots и all branches; отдельно RAM/VRAM/disk. Учти switching, re-embedding/reindex, cold/warm и рост данных; compact adapter не равен compact system. Сравни full quality-constrained cost и foreground interference. Unknown значения снабди будущим способом измерения, не нулём и не выдуманным fit.

**Промпт B.** Заполни feedback work order: pre-update inference/observation→correction admission→exact pair/target→isolated candidate state→evaluation→promotion или restore→ordinary inference. Назови actual owners/versions/operation IDs, no-repeat, bounded queue/backpressure/cancel/pending resolution, personal/global isolation, permissions и retention. Старый raw-word state нельзя импортировать в TaskSignal coordinates лишь потому, что loader принял shape. Lifecycle reuse и semantic weight transfer различаются. Полный snapshot охватывает нужные weights/optimizer/RNG/index/graph/version связи; irreversible costs и disclosure rollback не отменяет.

**Good:** после interruption сохраняется известный update state без повторного gradient; пользовательский request видит согласованную принятую версию. **Bad:** restore толькоweights при другомindex; пользовательская коррекция автоматически истина. **Edge:** power loss, unknown-running update, poisoning/conflicting feedback, private data, OOM, foreground during training.

**Приёмка/выход:** stateflow walkthrough, future foreground p95/total cost/retention plan с family/data volume, exact unknowns и mode boundaries. Никаких hardware benchmarks/trainer сейчас. R24/R25 получают проверяемую границу local claim, не обещание PC-fit.

## R24 — ordinary consumer и сквозной пользовательский результат

**Цель:** полезное изменение проявляется в продукте, а не только в trainer report. **Skills:** A/P/E/S. **Контекст:** R02 full scope, R19 roadmap, R21–R23, A02/A07/A09, H01/H03, original DTC/IDE prompts.

**Промпт A.** Заполни две отдельные истории. DTC: tree/HTML/CSS/images→API/machine→multi-resolution renders→semantic/visual assessment под user-tier budget→DB→допущенная пара→Student update→publication→ordinary consumer. IDE: project index→domain/intent→evidence plan/retrieval→Unit/Flow/async graph→output→weakness→feedback lesson→новая offline task. У каждого перехода actual owner/version и future observation. Thin host вызывает core; не реализует заново domain selection/DPO в каждом клиенте.

**Промпт B.** Проследи GetOrchestratorAsync/ModelServer либо актуального successor до принятого learned state, не bootstrap из другого cache. Несколько допустимых outputs и личные preferences не схлопывай в единственный golden answer. Общая/личная модели, export/import/continue/restore и несовместимые coordinates описаны явно. Полный installed-host path остаётся delivery obligation с owner, а не доказательством ещё не измеренного learning gain.

**Good:** новая независимая задача использует принятое состояние и имеет доказанный domain result; external assistance раскрыта. **Bad:** screenshot/JSON/changedhash или copied supporting-files выданы за обученный consumer. **Edge:** missing image, RTL, offline, client/model mismatch, stale cache, субъективное предпочтение.

**Приёмка/выход:** все три продукта/дополнительные языки и tasks имеют endpoint, source trace и future evidence owner; сначала понятное человеку поведение, затем технический carrier. R25 получает приёмочные сценарии без ложного runtime readiness.

## R25 — независимая финальная передача без автоматического R26

**Цель:** проверить применённый план и передать ближайшую исполнимую работу. **Skills:** A/P/C/E/M, отдельный actual reviewer. **Контекст:** exact final candidate refs, R15 issues и их R16–R24 repairs, A02–A09, P14/N01/H01–H03, первый R18 packet и R13/R22 recipe.

**Промпт A.** Независимо проследи original expectation→primary→current requirement→chosen algorithm/owner→bounded implementation task→domain examination→ordinary consumer. Проверь previously lost details: four roles/many→many/correction/async/higher order, Graph=L5, learned reasoning node, TensorCore, stack retention/shared-project LoRA, coordinate compatibility, domain-specific metric/k/BM25/history, recipient/order/reward/fan-out, no-repeat/full state и все product lines. Не требуй готового продукта для приёмки плана; не прячь нерешённый design как «потом проверим».

**Промпт B.** Проверь активное распространение R15 repairs, отсутствие stale R02 anchors, сохранение parallel R01–R03 результатов и отсутствие return-to-S04-only. Собери ближайший future implementation prompt+first recipe с exact refs, actions, named sources/skills, good/bad/edge, verification/budget/decision. Неопределённые measurements остаются будущими; смысловая неопределённость owner/objective не маскируется READY. Дополнительный round только с доказанным новым объёмом/причиной/capacity, не автоматические R26/30/50.

**Good:** reviewer принимает exact bounded scope либо указывает один реальный seam и минимальную repair. **Bad:** авторский self-check назван Astra Ultra PASS, 25 headings названы 25 исполненными sessions. **Edge:** reviewer unavailable/model mismatch, final content changed после review, late source касается одной requirement.

**Приёмка/выход:** actual independent evidence и scope, source/prompt/content refs, публикация/readback, готовый следующий packet, measured/not-measured отдельно. Нет отдельного reviewer — NOT_PERFORMED. Конечный результат — ясный путь к разрешённой реализации и исследованию, не бесконечное добавление guards.

## Проверка маршрутизации содержания

Архитектура/representation: R01–R04→R12→R16/R18. Recipient/order/reward/DPO/LoRA: R02/R05→R07/R12/R13→R16/R18/R22/R23. Domain/kNN/reasoning/investigation path: R03/R06/R08→R12→R18/R21/R22. Negative quality/lineage: R08/R12→R16/R21/R22. Процесс/Windows: R09/R11→R14/R17/R19/R20. Ordinary consumers: R02/R10→R14/R19/R23/R24. Независимая экспертиза: R15/R25, не новые фиктивные вызовы.

Эти карточки уточняют прежние полные work orders. Проверка их полноты — документная: цель/non-task, context/skills/sources, actions/output, good/bad/edge, current verification/future observation и handoff. Она не означает выполнения R01–R25, code readiness, learning gain или PC-fit. Автор этого CP4 не меняет carriers параллельных R01–R03.
