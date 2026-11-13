# R03 — восстановленные решения и результаты ремонта

**13.10.2026 · CP08 · документный ремонт R03. Полный обход источников: IN_PROGRESS / SOURCE_TRAVERSAL_NOT_COMPLETE.**

Текущая команда — **«Please repair R3»**. Исправляются только пять существующих носителей `results/R03/`; R01/R02, общая очередь, код и обучение этим проходом не выполняются. Главная свежая поправка: R03 больше не отправляет уже решённое авторство DPO на повторное решение R12 и не ставит весь адресный аудит перед содержательным чтением.

## Что исправлено сейчас, а что сохранено

В CP07 уже были полезные алгоритмические находки: semantic order, objective, fan-out, learned skill selection и конкретный LoRA seam. Утверждение «R03 совсем не рассматривал алгоритмы» было бы неверным. Но CP07 сохранил устаревший открытый вопрос о Student-only rejected, неоднозначный порядок selection/generation и последовательность «сначала все адреса, затем первичные решения». В CP08 исправлены именно эти действующие передачи. Дополнительно исправлен учебный контраст, который мог объявлять правильный deterministic результат отрицательным лишь по происхождению.

F01–F21 и все прежние requirement/source IDs сохраняются. F22/F23 — адресные выводы этого ремонта, не ещё один общий аудит. Полный прежний текст и его формулировки доступны в [неизменяемом снимке P6 до ремонта](https://github.com/lubochka/xiigen-planning/blob/20df95a38c5138639bda97f33b4c044d86f8f70e/llm%20mvp/design%20to%20code%20session/replanning/results/R03/FINDINGS.md). Более ранний CP05/P5 сохранён в Git на `10dfbfd8a46f67b9cf43ce827d00d1dbf6d18078`.

CP07 устранил прежнюю рассинхронизацию: REQ25–27 находились в SOURCES/CHANGE-MAP, но не были раскрыты в FINDINGS/H packets, а журнал заканчивался CP04. Это исторический результат CP07; перенос его описания сюда не считается новым чтением первоисточников.

## Версии, корни и глубина доказательства

P2=`1cb2d868d71ba7041290adf5b0695418565a5de8`; P3=`9db9b84b71d90a3ba28153befd139132988d931c`; P4=`9ab76cac434331a407aa0a01cf642b6543baf287`; P5=`10dfbfd8a46f67b9cf43ce827d00d1dbf6d18078`; **P6=`20df95a38c5138639bda97f33b4c044d86f8f70e`**. Core K=`731d2e55728caf601f05daaca7ab3fc370dd0240`; **K2=`a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3`**. Перед публикацией проверяется актуальный HEAD; source pin не даёт права перезаписать чужие изменения.

D=`llm mvp/design to code session`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; C=`llm mvp/visual studio & visual studio code continue`; W=`09.09.26WorkLeft-`. Pxx/Txx/Cxx — локальные `R03-Pxx/Txx/Cxx` из [SOURCES.csv](SOURCES.csv), не источники соседних запусков. HYBRID:Hxx и DECISIONS/Hxx также разные namespaces.

Различаются найденный индекс, разрешённый адрес, прочитанный primary span, прослеженный code seam и наблюдение runtime. Runtime-наблюдений здесь нет. F01–F21 опираются на указанное прежнее чтение; свежие чтения CP08 отдельно отмечены P23/V07/V08/CP08-*. Старые строки с FRESH относятся к своему checkpoint, не автоматически к нынешнему автору. Один blob в нескольких отчётах не создаёт независимых подтверждений.

## Сохранённые находки F01–F21

### R03-F01 — четыре роли и комбинации одного входа

P01/P02/P03 — исходные постановки G/`active-codex-execution-original-task.md`, D/`original task.md`, D/`Алгоритм создания ДПО.md`; P10 — G/`active-codex-execution-06-ilearningdynamicgraph-architecture-brief.md`, P3:1–150. Unit получает домен, строение векторов и выхода и возвращает typed many→many; Flow задаёт отношения и коррекцию; Orchestrator исполняет асинхронные комбинации; DynamicGraph представляет отношения паттернов разных порядков. Собственный Builder остаётся обучаемой способностью формирования полезных пар.

Один CodeDocumentation unit не заменяет весь контракт. Исходные `Iquearable`/`IlearnyngFlow` не доказывают готовую C# сигнатуру или query provider. R04 проверяет порты, кратность, association/materialization; R06 — отношения и комбинации. Train остаётся у ILearningMachine, Graph — специализация L5, а не дополнительная ступень или четыре независимых trainer. Semantic order F17 не отменяет текущую unit-level границу адресации.

### R03-F02 — детерминированное вычисление не равно власти выбора

P04: G/`active-codex-execution-plan-12-no-hidden-determinism.md`, P2:1–105; P10:81–150. Индекс, вычисление расстояния, преобразование формата и явные ограничения могут помогать гибридной системе. Они не доказывают обучаемую semantic authority, но их нельзя удалять только ради названия learning. Обученная функция также может вычисляться воспроизводимо.

R06 исследует фактический selector/router и изменяемое состояние; R12 согласует границу helper/learned authority. Будущее сравнение fixed/learned/hybrid требует одних задач, явного вмешательства и полной цены. Правильный fixed результат не становится rejected по слову fixed: способность механизма учиться проверяется отдельно от предпочтения конкретной пары. Более строгий исторический запрет skill-плана — F20. Выигрыш NOT_MEASURED.

### R03-F03 — domain-scoped kNN и три роли соседей

Маршрут W-RAG04 → C/RAG06 → F/`ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` (P16, P3:1–110) → F/`DECISIONS-LOCKED.md` (P17, P4). Сначала задаётся допустимый доменный пул, затем ранжируется близость. Память хранит positive/negative/warning и причины прежних решений. Близость и бюджет не отменяют окончательную оценку допустимости.

Пример: недопустимый дублирующий аккаунт не становится допустимой регистрацией из-за высокого similarity. Исторические 0.50/E51 не назначаются оптимальными k/metric. R06 разделяет pool, feature meanings, metric/score, k/radius, память и learned policy; R08 получает доменные контрасты. Ошибки domain mapping, retrieval и solver оцениваются раздельно, включая mixed/unknown/empty/stale-coordinate случаи.

### R03-F04 — replay и one-pair конфликтуют по существу

P17 D-06 содержит исторический `70/30 Replay Buffer`; WorkLeft P06/P07/N07 сохраняют одну новую пару, no-repeat, полное состояние и accept/rollback. Bulk наблюдений не разрешает replay, повторный gradient или новый ID старому материалу. Retrieval старых примеров как контекста не равен повторному обучению на них.

Текущая one-pair/no-repeat policy сохраняется. R12 разбирает реальное противоречие, R16 применяет только разрешённую миграцию. Это не универсальное утверждение о непригодности replay, а запрет незаметно заменить действующий проектный контракт.

### R03-F05 — предмет, намерение и композиция не один enum

P17 D-04/D-12 перечисляет CODE_Q/BUG/PLAN/ARCH/REFACTOR; WorkLeft задаёт семь учебных направлений; P18 дополнительно организует topology/routing/translation/execution. Это разные координаты. SQL×BUG сочетает предмет и намерение. R06/R08 сохраняют domain, intent, язык/стек/версию и уровень композиции; смешанные и неизвестные случаи нельзя исключать ради удобной классификации.

### R03-F06 — learning-first не удаляет конечную DTC-поставку

P05: D/`task prompt.md`, прежнее чтение исходной задачи. HTML/CSS + дерево + изображения → API/машина → рендеры нескольких разрешений → качество и user-tier cost → БД → Builder/содержательный rejected → обучение → публикация → ordinary consumer.

Сохраняются исходные `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`, библиотеки Small model racional, language translate learning, local wiki learning, documentation learning и extensions. Устаревший порядок «сначала весь код, затем DPO» не перебивает нынешнюю цель раннего урока; API, БД, изображения и consumers не исчезают. R06/R24 проверяют соответствие поставке; полный актуальный код этих продуктов здесь не исследован.

### R03-F07 — статус библиотеки не является свежим фактом кода

C/RAG06/RAG08 и G/library-index содержат исторические readiness-overlays. Цепь доказательства: индекс → первичное решение → актуальная code trace → runtime-наблюдение. Старое CODE-ABSENT и найденное похожее имя не закрывают соответствие.

Прочитанный CP07 T08 на P5:1–80 остаётся отчётом о майских T34/SecurityArbiter и июльском cutover. Исходные C# экспериментов тогда не были перечитаны. `MultiSubPhaseRouterUnit` нельзя приравнять к `EdgeAuthorityUnit` без содержательной проверки R06. CP08 не повышает эту глубину.

### R03-F08 — WorkLeft research остаётся базой

P06/P07: W-RESEARCH-03:1–115 и активная часть W-RESEARCH-01 уже задают семь доменов, before/after, стоимость генерации/обновления/оценки, попытки до улучшения и состояние. Их нужно дополнить различающими опытами, не заменить параллельным трактатом. R08 использует предметное исследование; R05/R12 — update contract. Никаких сегодняшних измерений из прежнего текста не выводится.

### R03-F09 — прежний номер запуска не подтверждает нынешний

`previous rounds thinking results.md` хранит историю подготовки. CP08 V08, P6:1–44/1204–1250, подтверждает различие 25 документных сессий и обучения/Actions, whole-program область и нынешний R03: RAG к primary. Историческое CI-исследование пригодно R09, но не закрывает нынешний обход. S04-only обзор не становится whole-program обзором по переименованию. Подготовленные prompts не равны исполненным запускам.

### R03-F10 — компактность считается для полной системы

P17 содержит исторические заявления 2 MB/project LoRA, 20× compression и BM25+kNN gain. Они не воспроизведены. PC budget включает base model/runtime, encoder/codebook, adapters, optimizer, index, graph, cache, snapshots, reindex, обучение и оценку. Размер adapter не доказывает fit или выигрыш. Узкая кодовая опора F21 не заменяет полный профиль памяти и времени. Всё это NOT_MEASURED.

### R03-F11 — reasoning-узел не должен навсегда стать пустым адаптером

W-RAG04 запись18 → P12 C/`PLAN-VS-VSCODE-CONTINUE-v1.md`, §1.4.1/§1.4.7, P2-01-DOC. Прежний alternative contents reader получил непустой blob `ca1e423c5285f88dcada241358eab6d76b9bd18d`; пустой ответ другого reader не доказывал отсутствия файла.

Целевой замысел — одна L5-машина-граф; ORM-43 — обучаемый reasoning-узел внутри, не вторая независимая L5 и не окончательно полый адаптер. IntentDecomposer → GuideRetriever → PlanComposer → RiskAssessor; OPM-42 сохраняет доменную роль и lifecycle/integrity reuse. V1 scaffold и минимальное обучаемое ребро — промежуточная, не финальная цель. R06 ищет изменяемое reasoning/edge состояние и consumer. Старые NodeOrEdgeId/edge слова не расширяют современный payload автоматически.

### R03-F12 — перенос lifecycle не означает перенос весов в другой словарь

P12 §P3-B/§P3-C различает raw-word prompt weights и typed TaskSignalEnvelope: «oauth» и `security_auth_flow_task` — не одинаковые координаты. Reuse checkpoint/lifecycle/памяти/истории pair IDs не доказывает эквивалентность весов. TokenToSignalAssociationUnit — исходный замысел, не установленная сегодняшняя реализация или проверенная формула update.

P13 `TRANSLATION-TOKENIZER-GUIDE-v1.md`:1–82 задаёт письменности/byte fallback/словарь; исторические квоты и vocab-size не назначаются сегодняшними параметрами. R04 прослеживает parser/tokenizer → domain features → projection → retrieval space → typed consumer; R05 — адаптер и состояние. Одинаковая shape не равна смыслу. Нужны контрасты отрицания, чисел, единиц, identifiers и смешанных письменностей с reindex/snapshot cost.

### R03-F13 — отрицательный пример должен иметь понятный содержательный дефект

T10 → repo-root `extensions  - orchestration based on our data/ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md` (P19). CP07 на P5:1–105 подтвердил правило «ровно одна» ошибка и собственный образец ORCH-03 + ORCH-01: hardcode и неверный порядок одновременно; blob `5bd4a8fc43a1cdcaf4ceda1cd1e764901ac5690d`.

Это дефект учебного примера, не доказанная причина регресса Student. CP08 уточняет DECISIONS/H05: order-only контраст остаётся примером предпочтения; fixed-versus-learned контраст — проверка механизма, а не автоматически positive/negative пара. Для rejected нужно наблюдаемое ухудшение по заранее выбранному критерию. Равные допустимые результаты без установленного преимущества дают TIE/нет пары. P14 translation guidance сохраняется как дополнительный источник.

Маркеры `[WRONG]`, коды ошибок и судейские rationale не должны автоматически попадать в обучаемую сторону; текущая утечка не установлена. TIE/BOTH_BAD/unknown, два правильных пути и обоснованное воздержание не превращаются в принудительное предпочтение. Роли — по P23/F22: обе final sides пишет китайская модель, CODEX выбирает учебную задачу и отдельно оценивает точную пару. Продуктовых вызовов здесь нет.

### R03-F14 — правильный путь решения шире nearest-neighbor lookup

T02 → P15 `llm mvp/orchestrator extension/PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`, P3:1–200: проект/домен → план доказательств → retrieval по зависимостям → ответ → проверка достаточности. Сохраняются версии, виды evidence и wrong-project/stale/missing handling.

Для изменения поля требуются readers/writers, DTO, validator, mapper, persistence и внешняя граница; для SQL procedure — параметры, таблицы, result shape, транзакции/side effects и .NET caller. Это исходные ожидания, не команда менять SQL. R06/R08 различают mapping, достаточность investigation path и ответ. Самый короткий путь, пропустивший запись в БД, не является лучшим; цену сравнивают среди допустимых решений.

### R03-F15 — законы домена должны доходить до признаков и критериев

T07 → P18 `llm mvp/external-sources/03-OPEN-SOURCE-KNOWLEDGE-DOMAIN-MAP-v1.md`, P4:1–90. Карта связывает source, task type и arbiter; функциональные оси дополняют семь направлений, не заменяют их.

Рабочая связь R08: versioned law/constraint → сохраняемый token/feature → plausible negative с конкретной потерей смысла → критерий. SQL×BUG различает чтение/запись и caller→procedure→result; negative теряет одну обязательную связь. SourceId не доказывает правильного отображения домена. Исторические лицензионные пометки не создают нынешнее разрешение импортировать corpus; права на новую кампанию здесь не устанавливались.

### R03-F16 — хосты не должны повторно реализовать семантический алгоритм

P12 §1.7: manifest, trigger, thin entrypoint, один core-call, trace/honesty. Semantic selection, DPO/update и common/adaptive состояние принадлежат core. P20 `llm mvp/extension development/CURRENT-EXTENSION-DEVELOPMENT-REPORT-2026-07-08.md`:1–90 отличает supporting copies от рабочей цепи.

Первый локальный урок отделяется от упаковки всех IDE, но интеграции не удаляются. Копия README/skills рядом со старым dispatcher не доказывает применения нового порядка. R20/R24 доводят его до реальных входов; июльские host-ready числа не становятся октябрьскими измерениями.

### R03-F17 — unit-level не запрещает учить смысловой порядок

Primary CP07: P11, P5:195–280/281–356, blob `d6e37ab55a2bfd4deb057c28376fbb1620f7cf08`. Сохранено раскрытие REQ25.

Unit identity определяет обучаемую цель; `OrderedPatternIds` внутри chosen/rejected — смысловой выход; `graph_path_ids` в trace/sidecar — происхождение сравнения. Историческое решение сериализует ordered sequence внутри сторон, а `OrderLevel`, `ConsumesPatternIds`, `CapabilityTags` — в структурном входе. Это не новое поле пары и не доказанная современная сериализация.

Перестановка массива candidates не должна давать позиционное предпочтение. Но сортировка meaningful sequence как множества уничтожила бы учебный сигнал о зависимостях. Контраст: получить схему → построить mapping → выполнить запрос против перестановки зависимых действий. R04 — представление; R05 — complete serialized side; R06 — relation selector; R12 — только реальное несогласование. Whole-path trainer не выводится из одного старого имени.

### R03-F18 — оценка CODEX и DPO objective не одна величина

Primary CP07: P11, P5:315–390. План определяет reward полной стороны как `Beta * (policyLogP(side|input) - referenceLogP(side|input))`, margin — разность rewards chosen/rejected. Это восстановленное определение плана, не доказательство реализации training stack.

Оценка пары, loss/margin, уверенность судьи и Student accuracy — разные наблюдения. Score CODEX не подставляется вместо policy/reference terms. Неизвестный full-sequence scorer нельзя скрыть train-ready флагом; rationale не становится автоматически training text. R05 прослеживает producer→projection→objective/reference→mutation→ordinary consumer; R08 получает pair quality, R13 — отдельную оценку Student utility.

### R03-F19 — N кандидатов дают попытки пар, не автоматические N−1 updates

Primary CP07: P11, P5:397–458. Winner-vs-each-loser задаёт N−1 source-row/pair attempts с одной rejected стороной в каждой. IDs/content hashes связывают одну family; candidate-order hash не является confidence feature. Это не N−1 независимых семейств, не гарантированно годные пары и не разрешение на дополнительные updates. TIE/BOTH_BAD, duplicates, общий winner и split-family leakage требуют отдельного admission; one-pair/no-repeat сохраняется.

**Поправка CP08 к прежнему выводу об авторстве:** историческое teacher-goal правило Student-only losers/teacher-teacher eval-only больше НЕ открытая альтернатива для R12. P23 §§2.1–2.2 прямо устанавливает китайское авторство обеих окончательных сторон. Старые записи сохраняют своё реальное происхождение и не перелицовываются. R05 проверяет фактическое соблюдение текущей политики и admission; R08 — материал; R16 — разрешённые плановые consumers. R12 получает только нерешённые schema/owner/objective/replay вопросы, не повторное разрешение авторства. Генерация обеих сторон не отменяет material rights, exact-pair quality, one-call/one-pair ограничения и novelty.

### R03-F20 — замысел выбора skills и чрезмерный исторический запрет helpers

Маршрут CP07: T11 C/`RAG-11-extensions-research.md` → repo-root `extensions reasearch/PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md`, P22, P5:1–90, blob `a6df21e9b7a170381a01ea6b2944fa364c06f3c5`.

Цель — выбирать правильные skills обучаемым слоем, не только экономить токены. Старый Hook 2 заменяется, упаковка hooks переиспользуется. Но §0 запрещает BM25/AST/Jaccard даже при семантической генерации candidates/context — шире запрета подмены learned final authority.

Нынешняя гибридная постановка не отменяется историческим текстом. R12 согласует scope по механизму R06, сохраняя goal. Будущий контраст: approved skills/задачи/бюджет одинаковы; bounded fixed retrieval + learned selection против baseline; task success, missing mandatory skills, unknown handling и полная цена. Это не принятое runtime решение.

IMPLEMENTED/Phase39/`43.SmartSkillsDomain` — указатели, не code audit. `ORCHESTRATION-MACHINE-TRAINING-PLAN-v20` ещё требует точного successor trace R03. Нельзя заявить весь каталог прочитанным или активировать историческую 50-pair кампанию.

### R03-F21 — LoRA-проекция существует; её доменная полезность ещё не доказана

Code seam CP07: K2 `10.LoRA/LoraLinear.cs`:1–145, C11, blob `1b321e99408c961b896896983d002e3b26425b85`. `LoraLinear : Module, IAttentionProjectionAdapter` регистрирует A/B и вычисляет `base(x) + Scaling * ((dropout(x) @ A^T) @ B^T)`. Комментарий конструктора о не-владении/не-мутации base projection не доказывает freeze всей training stack; это вопрос optimizer owner.

Арифметика inference не отрицает обучаемости A/B, но отрывок не устанавливает tokenizer features, установку adapter в Student, update и употребление новых весов следующим обычным ответом. R05 продолжает эту цепь, не переписывает класс по умолчанию. LoRA не tokenizer/codebook; размер адаптера не PC fit. CP08 сохраняет эту прежнюю кодовую опору, не приписывает себе новый полный audit.

## Свежие выводы CP08

### R03-F22 — авторство и две CODEX-роли уже определены первичным контрактом

**Свежий primary:** P23, P6:40–95, `D/Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md`, §§2.1–2.3, blob `6e0e47a06a568cf7fef2e7c73239ac2e3d6a5fb3`.

CODEX `astra 6 ultracode` сначала выполняет человеческую роль выбора ОДНОЙ разрешённой учебной задачи по сохранённой допустимой диагностике. Затем открытая китайская модель получает полный разрешённый пакет и пишет окончательные chosen/rejected. После этого CODEX `astra 6 ultracode` отдельно оценивает семантическое качество именно этих неизменённых сторон, контекста и provenance. Поддерживаемое имя GPT-6-Astra / ultra уже разрешает alias; искать ещё один preset не требуется.

Слабый ответ Student относится к отдельной разрешённой учебной задаче и входит в контекст автора. Он не обязан стать final rejected. Закрытые экзаменационные ответы не передаются selection/generator/judge для этой задачи. CODEX не переписывает стороны под Chinese provenance. Изменение content/context делает старую оценку неприменимой; отказ не создаёт новый retry budget. Сохранённая/структурно валидная пара, semantic acceptance и optimizer eligibility — разные состояния.

Это исправляет F19, D07/H02/H05, REQ10/27 и журнал. Актуальная цепь имеет selection ДО authoring и quality ПОСЛЕ. Проверка наличия binding/consumer в runtime остаётся R05; actual model calls здесь не выполнялись. Старое происхождение исторических пар не меняется.

### R03-F23 — полезное исследование не должно ждать всего адресного аудита

P6 current R03 packet (CP08-004) требует содержательного перехода к primary; WorkLeft master V07:1–70 предлагает выбирать RAG по вопросу. Прежний DECISIONS ставил Q1 с 201+140 адресами перед Q2 и называл его следующим обязательным действием даже при уже известных primary routes. Это риск повторения прежней низкой отдачи, не измеренная стоимость всех запусков.

CP08 меняет порядок на «критический unanswered question → недостающий locator → primary/successor → содержательный вывод/передача». Q1 обслуживает адресную потребность выбранной ветви и ведёт полный отдельный остаток; Q2 использует уже доступные адреса без общего Q1-DONE; Q3 согласует каждый полезный checkpoint. Все обязательные roots и непрочитанные вопросы сохраняются; один пример не закрывает весь R03.

Второй связанный дефект — плохой метод предпочтения: исправленный CP07 authority-only пример мог обучать выбор ярлыка learned вместо лучшего решения. H05 теперь разделяет контентный order contrast и причинное вмешательство fixed/learned в механизм. Для пары нужен предметный дефект или установленная разница цены при допустимом качестве. Для доказательства обучаемости нужен наблюдаемый эффект изменения нужного состояния на новых задачах. Ни один из этих результатов не доказывается coverage или общим экспертным одобрением. Это авторский анализ примера; новый корпус/опыт не выполнен.

## Адресуемость и оставшаяся работа

CP05 сообщил 404 индексные записи: DPO201, DTC140, merge33, IDE30; также 30/30 IDE и33/33 merge address matches. CP08 сохраняет это как prior evidence, не новый parse и не 404 primary reads. Полная построчная сверка 201+140 адресов по-прежнему не выполнена. Она не блокирует чтение уже адресуемого критического документа, но остаётся открытым обязательством R03.

| Ветка | Первичный корень | Сохранённое / точный остаток |
|---|---|---|
| T01 compare branches | `llm mvp/compare branches` | Прежний index read; primary integration/verification решения ещё прослеживаются. |
| T02 orchestrator extension | `llm mvp/orchestrator extension` | P15:1–200; только уникальные protocol/source successors, без повторения прочитанного. |
| T03 translation | `llm mvp/language translate learning` | P13/P14; domain router, registry, examination successor вопросы открыты. |
| T04 DTC | D | Цикл к уже известным источникам; P05 цепь поставки сохранена. |
| T05 DPO | G | P10/P11 prior primary; P23 current roles прочитан CP08; runtime projection/admission — R05. |
| T06 dynamic flow | F | P16/P17; уникальные topology/examination переходы не все прочитаны. |
| T07 external sources | `llm mvp/external-sources` | P18:1–90; provenance/successor links не равны правам на corpus. |
| T08 experiments | `llm mvp/orchestrator experiment` | RAG:1–80; T34/SecurityArbiter primary/successor trace открыт. |
| T09 sessions | repo-root `claude-sessions` | Только архитектурно-релевантные named decisions; личные темы исключаются без пересказа. |
| T10 orchestration | repo-root `extensions  - orchestration based on our data` | P19:1–105 prior; machine/v2 successors остаются. Два пробела в root значимы. |
| T11 skills/extensions | repo-root `extensions reasearch` | P22:1–90; v20/Phase39 successor; actual43 mechanism — R06. |
| T12 extension development | `llm mvp/extension development` | P20; supporting copies не новый canonical runtime root. |

DELTAS V03:1–57 и master V07 сохраняют различие исторических NDJSON и текущей Markdown metadata. Архивные offsets нельзя механически переносить в консолидированное тело; нужны named section и actual span. Пустой ответ после EOF не означает отсутствия файла.

Продолжение — [DECISIONS](DECISIONS.md), готовый старт и Q1–Q3. Следующий полезный результат: адресный successor вопрос T11/v20 или T06/T08 о transition/order; неизвестный locator разрешается внутри этой работы. Общий адресный остаток не удаляется и не сваливается в R10. R04/R05/R06/R08 могут потреблять достаточные проверенные subsets сейчас. Все S00–S10 и дополнительные stages остаются у R01/R04–R06; эти findings не выдают себя за аудит каждого слайса.

## Граница результата

Документный ремонт не равен полному RAG traversal, работающему runtime или доказанному улучшению. Product tests/coverage, Windows Actions, build/trainer, paid generation не запускались; code/tests/workflows/weights/datasets не менялись. Accuracy, learning gain, latency, memory и PC feasibility — **NOT_MEASURED**. Отдельная Astra Ultra рецензия — **NOT_PERFORMED**. Запрошенные продуктовые CODEX-роли не выдаются за выполненные вызовы.
