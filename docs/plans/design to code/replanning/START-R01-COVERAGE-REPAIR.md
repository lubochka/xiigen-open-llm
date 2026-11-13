# R01 — продолжить карту всей программы, не начать её заново

**Редакция R01-REPAIR-2026-10-13. Назначение: исправленное готовое поручение тому же R01. Написание этого поручения не завершает R01 и не выполняет обучение.**

> Продолжи опубликованный R01 с его проверенного остатка. Найди и распределись по всей программе, а не только S04: общие задания, WorkLeft/LeftToWork, все обнаруженные RAG и вложенные библиотеки, S00–S10 и дополнительные этапы, кодовые и учебные источники. Результат — карта, по которой следующие исполнители смогут адресно восстановить и реализовать недостающие гибридные алгоритмы, затем измерять обучение. Не выбирай архитектуру за R12, не выполняй R02–R25 и не заменяй исследование новым циклом Windows/coverage.

## 1. Действующий вход, полномочия и сохранённая работа

Planning: `lubochka/xiigen-planning`, branch `llm_plan3`; **D** = `llm mvp/design to code session`; **P** = `D/replanning`. Core: `lubochka/llm_mvp_core`, branch `codex/dpo-implementation`; **K** = `docs/skills` в core. Локальные `C:\Users\lubao\Documents\xiigen` и `C:\Projects\llm_mvp_core` — известные ориентиры, не заявление об их доступности в текущей среде. GitHub-публикация не меняет локальный C: автоматически.

Это развёрнутое поручение R01 согласовано с [START-NEXT-RUN](START-NEXT-RUN.md), [PLAN-25-RUNS](PLAN-25-RUNS.md), общей частью и R01 из [LEARNING-WORK-ORDERS-01-10](LEARNING-WORK-ORDERS-01-10.md), R01 из [RECOVERED-DESIGN-PROMPTS](RECOVERED-DESIGN-PROMPTS.md) и [PARALLEL-R01-R03-JOIN](PARALLEL-R01-R03-JOIN.md). Исходная R01-карточка HYBRID остаётся предметным входом. При следующей редакции сравни только относящуюся дельту; пользователь не должен склеивать несколько prompts вручную. Не загружай все 25 карточек или все повторные PLAYBOOK. Старый `LEARNING-WORK-ORDERS-25.md` не обязательная вторая книга.

**Разрешено:** чтение/search/git/API; разбор деревьев, индексов, CSV/MD/NDJSON и существующих свидетельств; документная проверка; публикация собственных пяти носителей `P/results/R01/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`. Авторизация GitHub уже дана: повторно её не запрашивать. Сначала пользоваться работающим connector; ошибка terminal DNS или одного locator не означает read-only GitHub.

**Запрещено в исследовательском R01:** менять R02/R03, shared `results/*`, live план, core skills, код, тесты, workflows, datasets или веса; запускать новые product tests/build/coverage/Windows/Actions/trainer/платную генерацию, установки или cleanup. Не удалять существующие проверки, не отменять jobs и не объявлять skipped проверку успешной. Ремонт данного стартового документа выполнен отдельным текущим поручением пользователя; последующему research worker это не даёт права переписывать всю программу.

R01, R02 и R03 работают независимо и параллельно. Чужой checkpoint — versioned advisory input, не условие старта и не собственное свидетельство чтения. Каждый пишет только в свой каталог. **J03 входит в сведение R10**, не добавляет автоматический 26-й запуск. Сохраняются 10+5+5+5 сессий; A–D ниже — внутренние части R01, а не четыре новых запуска или независимые рецензии.

### Что уже нельзя повторять без конкретной причины

Прочитанный при ремонте `results/R01/FINDINGS.md`, blob `33b7611caed1fc21912f3e21e8a96b15a884da4b`, содержит F01–F13 и статус IN_PROGRESS. История четырёх RAG→primary переходов есть в `results/R01/RUN-LOG.md`. Это база, не доказательство полного обхода.

В `results/R01/CHANGE-MAP.csv`, blob `8725f02322a1124c1a040857dc5fbe0d1b375c72`, **уже существуют U01–U15, LF01–LF12 и AD01–AD02**. LF06 уже отдельно требует mutation→persist/reload→ordinary consumer. Поэтому старую фразу остатка «согласовать LF01–LF12 с U01–U14» нельзя понимать как поручение создать эту карту заново. Сверь актуальные строки и новые источники; исправь только выявленную дельту. Наличие строк не означает code trace или измеренное обучение.

Сохрани F12: найден первичный LearningMachine evolution plan. Сохрани F13: неразрешённый locator — не отсутствие архитектуры. Не возвращайся к прежнему snapshot/reset и не повышай статусы по одному новому commit.

## 2. Skills и минимальный контекст исполнителя

Сначала прочитай применимые `D/AGENTS.md`, `P/AGENTS.md` и startup-инструкции фактической среды. Из K загрузи применимые разделы и зависимости; в RUN-LOG запиши actual path/ref/blob/span, `read`/`partial`/`unavailable`, а не только перечень имён.

| Роль | Конкретные навыки | Для чего в R01 |
|---|---|---|
| Текущая задача и коммуникация | `planning-session-startup-SKILL.md`, `architect-behavior-classifier-SKILL.md`, `output-readability-gate-SKILL.md` | Отличить ремонт/исследование от исполнения исторических команд; объяснить результат по-русски |
| Источники и планирование | `reference-inventory-and-source-map-SKILL.md`, `authority-requirement-binding-SKILL.md`, `phase-planning-SKILL.md` | Source identity, первичные решения, версии, обязанности и ограниченные передачи |
| Проверка плана | `plan-review-SKILL.md` | Найти потерянное ожидание, повторную работу, циклическую зависимость и неподтверждённый статус |
| Архитектура | `trainable-architecture-decision-SKILL.md`; применимые разделы `human-readable-architecture-audit-SKILL.md` | Не потерять learned authority, many→many, владельца state и связь блоков |
| Проверка кода | `generated-code-review-SKILL.md`; применимые разделы `code-examination-SKILL.md` | Отличить найденный класс/декларацию от producer→state→consumer; не проводить весь code audit в R01 |
| Промпты | `prompt-patch-authoring-SKILL.md` | Каждый переданный вопрос получает контекст, действия, хороший/плохой пример и способ проверки |

Существование дополнительных путей проверяй по дереву, не угадывай. Не скрывай недоступный обязательный source. Исторические 30/50-review правила и прежние модели из skills не активируются автоматически и не отменяют текущую параллельность. Полную библиотеку и её транзитивные зависимости не объявляй загруженными после excerpts. Отдельного reviewer не имитируй.

Адресные общие источники: [COMMON-CONTRACT](COMMON-CONTRACT.md) — evidence schema; [SOURCE-ROUTES](SOURCE-ROUTES.md) — семейства/корни; [COVERAGE-AND-WORKLOAD-GUARD](COVERAGE-AND-WORKLOAD-GUARD.md) — нагрузка; §4 [LEARNING-EXAMINATION-CONTRACT](LEARNING-EXAMINATION-CONTRACT.md) — LF01–LF12; относящиеся разделы [LEARNING-EVALUATION-CONTRACT](LEARNING-EVALUATION-CONTRACT.md) — метод будущих измерений. Из HYBRID/RECOVERED читать свой R01 и назначенные источники, не все карточки.

## 3. Продуктовый смысл, который должна сохранить карта

Система должна учиться выбирать правильный домен, представление и допустимый путь к решению; сочетать детерминированные операции с обучаемыми решениями; работать компактно на ПК и продолжать обучение по пользовательскому вводу. WorkLeft training/DPO research — исходная база для переиспользования, не задание изобрести второй pipeline.

Три линии не сокращаются: DPO/DynamicGraph; Design-to-Code; IDE/клиенты. DTC сохраняет API, дерево/HTML/CSS/images, несколько разрешений, оценку, БД, пары, обучение, публикацию, ordinary consumer и стоимость пользовательского уровня. IDE и отложенные поставочные обязанности получают владельца/условие возврата, а не исчезают.

Семь исходных предметов: EN↔RU; оркестрация; Figma→HTML+CSS; C# documentation; Angular documentation; SQL documentation; DPO Builder для предыдущих шести. HE↔EN и другие найденные языки/задачи регистрируются дополнительно. **Domain, intent, language/stack/project, slice, functional role и physical model — разные координаты.** Например, SQL — предмет, BUG — intent; это не два взаимоисключающих домена.

| Роль пользователя | Что найти и сохранить | Кому передать подробную проверку |
|---|---|---|
| `ILearningUnit` | Domain/input-vector/output schema; typed many→many, queryable/candidates, materialization и actual consumer. Один envelope не доказывает одного результата. | R04; training seam — R05 |
| `ILearningFlow` | Отношения Units, преобразование и коррекция input→output, совместимость и смысл ports, обучаемые связи. | R06; межблочные противоречия — R07 |
| `ILearningOrchestrator` | Async fan-out/fan-in и комбинации одного input; выбор пути, cancellation и полная цена ветвей. Первый завершившийся ответ не автоматически лучший. | R06; стоимость среды — R09 |
| `ILearningDynamicGraph` | Соотношения паттернов разных порядков, изменение отношений, memory/arbiter и перенос на новые композиции. | R06/R07; архитектурный выбор — R12 |

Это семантические роли, не заявление, что буквальные C# интерфейсы с такими именами уже существуют. Найди aliases `ILogical*`, generics и producer/consumer. Сохраняемый найденный дизайн: Train у `ILearningMachine`, Graph — специализация L5, а не новая шестая ступень или четыре независимых trainer. Проверяй источники до утверждения о коде. Сохрани TensorCore, shared/project LoRA, stack-before-normalization и semantic compression до явно согласованного изменения.

**Роли DPO закреплены явно:** роль человека при выборе учебной задачи/направления выполняет **CODEX с моделью Astra 6 ultracode**; внешняя открытая китайская модель **создаёт DPO, включая положительные/chosen и отрицательные/rejected кандидаты**; оценку качества этих пар также выполняет **CODEX с Astra 6 ultracode**. Обучается XIIGEN Student. Нельзя возвращать старое ограничение «китайская модель выдаёт только teacher-goal» или делать её окончательным судьёй. Дополнительные маршруты Student-generated candidates не отменяют этого текущего ролевого договора. Requested и actual model/effort фиксируются отдельно; в документном R01 эти вызовы не выполняются. Запрошенная отдельная Astra Ultra экспертиза направления в R15/R25 должна быть реальной, а не переименованной самопроверкой.

## 4. R01-A — восстановить checkpoint и точный остаток

**Цель:** не потерять сделанное и не тратить проход на повторную подготовку. **Skills:** startup/intent, источники, plan-review, коммуникация. **Вход:** пять собственных carriers, START/PLAN/JOIN, относящиеся previous rounds, текущие planning/core refs. **Выход:** один согласованный список сохранённого и остатка в существующих FINDINGS/RUN-LOG; IDs и история сохраняются.

**Промпт исполнителю.**

> Зафиксируй текущие refs и редакцию своего задания. Прочитай собственные пять carriers и сопоставь статус, подтверждающие строки и незавершённые вопросы. Найди результаты под прежними именами; отсутствие ожидаемого файла не доказывает отсутствия работы. Для каждого старого остатка выбери: действительно открыт; уже закрыт опубликованным evidence; требует только новой дельты. Проверь существующие U/LF/AD до повторного mapping. Старые statements в RUN-LOG пометь историческими при расхождении, но не переписывай прежнее наблюдение. Результаты R02/R03 используй лишь с их ref/blob и границей прочитанного. Не жди их завершения.
>
> Прочитай в `previous rounds thinking results.md` относящиеся исходный S04-обзор и последующее признание недостаточного охвата. Сохрани полезные гипотезы и различие механизм/learning/delivery; не реактивируй прежнюю нумерацию задач или sandbox-ссылки. Остаток большого файла регистрируй по смысловым разделам, не называй целиком прочитанным из-за запроса большого диапазона.

**Хорошо:** LF06 уже mapped, поэтому требуется только новый consumer-source, а не ещё одна матрица LF. **Плохо:** последний абзац старого FINDINGS автоматически становится новым backlog, хотя CHANGE-MAP уже содержит результат. **Края:** разная схема CSV; одинаковые IDs разных workers; незавершённая публикация нескольких carriers; новая редакция prompt при старом source snapshot. Сохраняй актуальную схему, квалифицируй `(run_id, local_id, source_ref)` и не превращай конфликт статусов в DONE.

**Проверка результата сейчас:** для каждого перенесённого пункта есть конкретная строка evidence либо точный недостающий вопрос; no-reset и write ownership соблюдены. **Передача:** R01-B получает существующий visited set и открытые корни, а не пустую карту.

## 5. R01-B — закончить рекурсивный инвентарь, не прочитать весь архив подряд

**Цель:** ни один существенный root/пакет/тип источника не исчезает за S04. **Skills:** источники/authority, planning, code-review для границы metadata/code. **Вход:** результат A, реальные root trees обоих repos, SOURCE-ROUTES, WorkLeft INDEX/REDATE/MASTER/usage. **Выход:** конкретные строки SOURCES и source-owner-capacity карта; неизвестные поверхности явно видны.

**Промпт исполнителю.**

> От корней обоих репозиториев перечисли деревья и относящиеся листья, включая источники вне D. Сохрани фактические repo/ref/path/blob/size/type, роли версий и область чтения. Отдельно различай GitHub `truncated=true` и обрезанное представление ответа инструмента: даже `truncated=false` не означает, что ты получил и обработал все строки. Для неполного ответа пройди поддеревья/страницы или меньшие диапазоны; запиши необработанный frontier. Не объявляй exhaustive по первым показанным записям.
>
> Ищи case-insensitive `*workleft*`, `LeftToWork*`, RAG/local-RAG, MD/NDJSON/JSON, индексы, source/context manifests, общие prompts, исходные tasks и slice packs. Поиск по имени — начальный фильтр, не граница охвата: перейди по относящимся ссылкам. Учитывай архивный источник, если он единственный носитель живого решения. Не превращай собственные generated replanning results в новые первоисточники и не обходи циклы бесконечно. Одинаковое имя в двух roots не дубликат; одинаковый blob может иметь разные назначения.
>
> Сверь общую `Kiro-Xiigen-Original-Slice-Queue.md`, человеческую карту и реальные S00–S10/дополнительные stages, включая версии S03/S04/S06, вложенные SOURCE-AND-RAG-INDEX/context-manifest в S05–S10 и работы вне packs. Для каждой версии укажи область и lineage; v2/свежая дата не доказывают замену всего предшественника. Не определяй выполненность кода по наличию папки.

**Обязательные стартовые маршруты, не закрытый список:** `llm mvp/dpo legal build`; `llm mvp/Prepare to merge`; `llm mvp/visual studio & visual studio code continue` → `llm mvp/llm dynamic flow`; исходные задания ведут также в `Small model racional`, `language translate learning`, `local wiki learning`, `documentation learning`, extensions и `Planning sessions/session-start-extracted`. Проверяй actual path/ref. Корни **`extensions  - orchestration based on our data`** и **`extensions reasearch`** находятся в корне planning, не под D; два пробела и исходное написание существенны.

**Общие постановки для регистрации и передачи R02:** `original task.md`, `task prompt.md`, `task prompt.txt`, `goal of the round.md`, `Изменения в плане работы - цель.md`, `Алгоритм создания ДПО.md`, `Обучение создания HTML CSS - 1st level.md`, `Исполнение кода без внешней модели.md`, `planning of testing.md`, `Claude session decisions.md`; дополнительно найденные `Original flow to scripts algorithm.md`, `DPO into tasks.md` и их реальные преемники. Не ограничивайся шапками при определении назначения.

**WorkLeft:** перечисли конкретные файлы решений, teaching, RESEARCH, INTEGRATION, TASK-PROMPTS всех групп, COVERAGE, MECH, VERIFY, DOCDEBT и отчётов. Владелец учёта семейства из SOURCE-ROUTES не означает, что он один обязан читать каждую тему мегабайтного тела: предметные spans распределяются в D ниже. Существующие training/quality/speed исследования становятся baseline.

**Хорошо:** 1 MB WorkLeft имеет выделенные representation/update/graph sections, историю и точных читателей. **Плохо:** строка «все WorkLeft → R07» либо «остальное вне S04 не относится». **Края:** API limit, длинный NDJSON, двусмысленный root, symlink/submodule, moved source, недоступный бинарный источник, несколько текущих версий. Не применяй OCR/конверсию без необходимости; отсутствие прочитанных bytes остаётся видимым.

**Проверка сейчас:** обратная сверка реестра с полученными tree entries и frontier, включая все найденные slice/RAG/WorkLeft families; каждое исключение обосновано. Передача не повышает read depth. **Передача:** C получает новые RAG-ветви/locators; D — источники по предметам и реальные размеры.

## 6. R01-C — проверить навигацию к первичным решениям без дублирования R03

**Цель:** не потерять архитектуру за индексом или ошибочным адресом. **Skills:** source inventory/authority, архитектура, plan-review. **Вход:** A/B, usage/MASTER, собственные прежние primary traces, versioned R03 findings. **Выход:** source-root→record→primary→successor routes в SOURCES/FINDINGS, явный остаток R03 и владельцы использования.

**Промпт исполнителю.**

> Для каждого обнаруженного текущего RAG-носителя установи его библиотеку, формат, index/NDJSON pair, область и status. Четыре WorkLeft семьи DPO/DTC/merge/IDE — минимум, не весь граф. Сохрани уже опубликованные четыре успешных primary перехода; повторяй только изменённый source или новый вопрос. Для новых существенных ветвей открой навигационный раздел и конкретный source route; полный обход записей и содержательная проверка обязательного подграфа принадлежат R03. Любой утверждаемый тобой новый факт требует собственного primary span, а не только ссылки на чужой отчёт.
>
> Разрешай `source.file` относительно root, объявленного самой библиотекой. Сохраняй section/span и blob: старые номера строк нельзя переносить в консолидированное тело. При 404 сначала открой точного родителя, проверь буквальный path/ref/case и доступ; не повторяй тот же неудачный адрес и не делай вывод «такого алгоритма нет». Циклы обрабатывай visited set. Историческая запись с единственным действующим решением не отбрасывается по дате.
>
> Адресно включи восстановленные источники: core `docs/plans/ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md` (R01 F12, §2; опубликованный прочитанный диапазон 85–210); `00.Core/LogicalContracts.cs`; `00.Core/LearningMachineContracts.cs`; `19.DynamicGraphComposition.Contracts`; `docs/SUBPROJECT-MAP.md`. Это seeds, не доказательство твоего нового code trace. Сверь только относящиеся late deltas из RECOVERED/START: DPO recipient/order/reward/fan-out, raw-word/TaskSignal coordinates, reasoning-node, query-planning, skill-selection и existing LoRA. Не начинай вместо владельцев полное исследование алгоритмов.

**Устойчивые ссылки:** P14 = `llm mvp/dpo legal build/active-codex-execution-plan-07-dpo-builder.md`, §§3.1–3.2a; N01 = `extensions  - orchestration based on our data/ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md`; C01 = core `00.Core/LearningMachineContracts.cs`. Их refs/blobs/прочитанные spans брать из текущей таблицы «Источники CP4» RECOVERED, а затем подтвердить относящийся primary. `SRC-R01/SRC-R02/SRC-R03` — источники результатов; `HYBRID:Hxx` — разделы другого документа; `RECOVERED-CONTRACT:Axx` — третье пространство. Не передавай двусмысленный bare H01. Старые R02 §7.4/7.7 не адрес нынешнего §6.1–6.5.

**Хорошо:** IDE RAG ведёт к `llm dynamic flow`; новая relation authority получает первичный source и R06, а не теряется как «IDE-only». **Плохо:** прочитан MASTER, значит все RAG проверены; три отчёта повторили одно мнение, значит есть три независимых доказательства. **Края:** смещённые lines, пустой target, конфликт старого replay с one-pair, недоступный primary, найденная только декларация. Статусы UNREAD/UNRESOLVED_LOCATOR/HISTORICAL/PRIMARY_TRACED различны.

**Проверка сейчас:** пользователь может повторить каждый заявленный переход; непройденные ветви имеют вопрос и владельца R03, но не помечены проверенными. **Передача:** D получает точные первичные адреса и вопросы, R03 — только новые или спорные маршруты, не приказ повторить свой обход.

## 7. R01-D — распределить алгоритмические вопросы и подготовить исполнимую передачу

**Цель:** каждый последующий проход делает свой полезный кусок и приближает первый реальный урок. **Skills:** planning/plan-review, архитектура, code-review для evidence boundaries, prompt authoring, коммуникация. **Вход:** сохранённые U/LF/AD, A–C, текущие тематические поручения R04–R09. **Выход:** source→expectation→question→owner→bounded packet; no blanket blockers и no megafile handoffs.

**Промпт исполнителю.**

> Для каждого механизма найди источники deterministic операций, learned authority, state/objective/training owner, input/output meaning и обычного consumer. В R01 не заполняй неизвестный дизайн догадками: дай точный исследовательский вопрос и различающий будущий опыт. Сопоставь с existing U/LF/AD, не создавай параллельную систему требований. Один механизм — один основной исследователь; другие исследуют свои конкретные подключения. Проверенный subset можно передавать до завершения всех независимых тем.
>
> В большом документе сначала определи смысловые заголовки и lineage, затем точные ranges. Разделяй по вопросу, а не по случайному числу страниц. У передачи должны быть source/ref/blob/span, уже доказанное, недостающее, ожидаемые bytes/sections или число новых seams/conflicts, один основной owner и конкретный результат. Не пиши «остальное в R10». R07 — не повтор R04–R06; R10 объединяет, не исследует весь архив повторно. Непомещающийся обязательный объём явно перераспредели в существующих ролях; дополнительный реальный запуск требует объяснения и учёта, не фиктивной внутренней проверки.

| Предмет / требования | Конкретный вопрос для владельца | Основной владелец / потребитель | Будущая функциональная examination, не тест в R01 |
|---|---|---|---|
| Domain mapping и законы; U08/U12, LF01/LF02 | Где subject/intent/stack разделяются? Какие ограничения действуют, при каких единицах/исключениях и с каким источником? | R06; данные R08, решение R12, протокол R13 | Одинаковый solver с predicted и допустимым reference domain; mixed/unknown domains; неверно отсечённые допустимые решения |
| Tokenization/vectorization/Unit; U01/U08, LF03/LF07 | Parser/tokenizer/feature encoder/embedding/projection/decoder; raw-word vs TaskSignal; queryable→OutputRefs→typed consumer; версии координат | R04; R05/R06 | Семантически разные близкие inputs, negation/types/units; несколько inputs/outputs; collisions, downstream correctness и bytes, не только shape |
| DPO construction; U06/U15, LF05 | Одна задача/контекст, chosen и правдоподобный rejected; provenance/lineage; literal recipient vs winning candidate; semantic order vs presentation order; N−1 families | R05 — transport/owner; R08 — качество данных; R12 — конфликты | CODEX/Astra 6 ultracode проверяет качество китайских пар; TIE/BOTH_BAD/unknown не принуждаются; Student utility измеряется отдельно |
| DPO/update/LoRA; U09/U10, LF06/LF08/LF10 | Policy/reference/objective→конкретная matrix/state mutation→persist/reload→ordinary consumer; GraphDpoPair vs lower DTO; frozen base/adapters и compatibility | R05; representation seam R04, стыки R07, выбор R12 | Parent/candidate/no-update; frozen/updated/wrong-domain adapter; реальная задача, сохранение навыков, update cost. LoRA не tokenizer |
| Dynamic kNN и memory; U07, LF04 | Domain pool/features/scaling/metric/k/radius/weights; обучаемая policy vs фиксированный switch vs memory insertion; ANN/index compatibility; positive/negative/warning memory и final arbiter | R06; R08/R13 | Fixed/adaptive при равном бюджете; sparse/noisy/drifted/unknown domains; качество retrieval и решения, latency/RAM/reindex cost отдельно |
| Flow/Orchestrator/Graph; U02–U05, LF07 | Producer/consumer ports; learned edge authority; fixed Units и изменённая связь; async branches и паттерны разных порядков; кто получает credit | R06; только остаточные interactions R07; решение R12 | Fixed Units/change edge и обратное вмешательство; новые композиции; корректность/допустимость раньше цены пути; цена всех branches |
| Compact hybrid на ПК; U05/U10, LF09 | Что реально нужно до первого урока? Полный model/runtime/adapters/optimizer/index/graph/cache/snapshots footprint; какие Windows-зависимости причинные, а какие поставочные | R09; R05/R13/R23 | Пригодные deterministic-only/learned-only/hybrid baselines на одинаковых задачах; quality/time/peak memory/disk, не размер одного adapter |
| Bulk/feedback; U10/U11, LF10/LF11 | Оценка нового input до обучения, admission, один разрешённый update, accept/full rollback, следующий ordinary request; bulk как группа новых уроков | R08 с R05; протокол R13, исполнение R22/R23 | Сопоставимые family-aware панели, per-domain accuracy/time deltas и uncertainty; retention; шумные corrections; без скрытого replay |
| Независимая оценка направления; LF12 | Какая гипотеза различается опытом, что её опровергнет, какие первичные источники и actual results передать reviewer | R15/R25; метод R13 | Реальная отдельная Astra Ultra examination exact candidate; IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE, не verbal PASS вместо измерений |

**Доменный пример передачи:** для SQL NULL/трёхзначной логики R08 ищет источники законов и трудный near-miss, R04 — представление predicate/operator/context, R06 — метрику соседей с учётом различия смысла, R05 — обучение нужного owner. Это иллюстрация будущего вопроса, не утверждение о нынешней реализации или уже созданная учебная пара. Для перевода сохраняются negation/направление языка; для Figma→HTML/CSS — структура/геометрия на нескольких разрешениях; для orchestration — зависимости/ресурсы/порядок; для documentation — API/version/type semantics. Builder проверяется на способности создавать хорошие пары для всех шести, а не только на синтаксис JSON. R08 должен получить отдельный law→feature→near-miss→metric маршрут каждого базового домена и найденных дополнительных.

**Хорошо:** один документ делится на representation R04, update R05, routing R06; R07 получает только несовместимый port/credit вопрос; R13 получает различающий опыт. **Плохо:** «универсальный cosine оптимален», «более короткий неправильный путь лучше», «красивый rejected доказывает Student gain», «LoRA имеет правильную dimension, значит смыслы сохранены», «всё должно дождаться R10».

**Края:** два алгоритма одновременно изменены и вклад неотделим; domain router скрыл хороший solver; память выросла без обученной policy; новый encoder сделал старый index несовместимым; warm latency улучшилась ценой неприемлемой RAM; accepted state сохранён, но consumer остался bootstrap. Для каждого назови будущий контраст и владельца, не запускай опыт здесь.

**Проверка сейчас:** пройди source→U/LF→packet и обратный путь от каждого исходного ожидания. Для общего механизма нет двух назначений полного повторного анализа. Полный downstream-аудит и измерения не являются условиями закрытия навигационной задачи R01.

## 8. Формат каждой реально передаваемой подзадачи

A–D может выполнить один worker последовательно. Если часть действительно делегируется, parent передаёт **полный общий блок §§1–3, конкретную карточку A/B/C/D, вход предыдущей части и её source subset**, а не «смотри весь replanning». Child работает return-only; parent интегрирует. Это не создаёт независимый review и не скрывает дополнительных вызовов.

Каждая адресная передача R04–R09 содержит: понятную цель; current/not-the-task; actual skills и контекст; exact RAG/primary refs/spans; current→target; последовательные действия; ожидаемый output; хороший/плохой пример; edge case; документную проверку сегодня; будущую examination с owner; разрешённые действия/запись; объём и принимающую роль. Не оставляй placeholders и не поручай пользователю дописать промпт. Архитектурные и измерительные решения по-прежнему принимаются в назначенных R12/R13, а не заранее в R01.

## 9. Пять носителей и честное закрытие

Сохраняй существующие schema/IDs. Дополнительные сведения, не помещающиеся в CSV без миграции, размести связанным разделом FINDINGS, не создавай вторую несовместимую таблицу.

| Носитель R01 | Что дополняется |
|---|---|
| `SOURCES.csv` | Конкретные источники и identity, реальная глубина, первичные маршруты, следующие spans/owners; frontend truncation/frontier видны |
| `FINDINGS.md` | Подтверждённые source факты отдельно от hypotheses, root/slice/algorithm navigation, устранённые stale residues и адресные пробелы |
| `DECISIONS.md` | Только решения организации R01 и неразрешённые конфликты с владельцами; не новая product policy |
| `CHANGE-MAP.csv` | Дельта к существующим U01–U15/LF01–LF12/AD01–AD02, source→expectation→owner→future examination |
| `RUN-LOG.md` | Snapshot/prompt revision, фактические прочитанные spans/skills, публикация и remaining frontier; не фиктивные 25 записей |

**Приёмка R01 сегодня — документный и source walkthrough, не новые продуктовые tests.** Должны быть видимы все три линии, S00–S10/дополнительные, общие prompts, конкретные WorkLeft и все обнаруженные RAG-корни. Для полноты inventory сверь полученные деревья, явные исключения и frontier; для содержательных выводов — primary spans. Каждый обязательный источник имеет читателя либо конкретный неразрешённый capacity gap. Все четыре роли и U/LF ожидания имеют маршрут и адресный следующий packet. Четыре старых primary перехода не выдаются за полный RAG audit. Проверь примеры A–D, source aliases, changed links, CSV integrity и свежую удалённую публикацию.

Нельзя закрывать R01 только по числу файлов/строк/commits. Если обязательная навигация остаётся неполной, статус **IN_PROGRESS/PARTIAL с точным остатком**; достаточные независимые packets уже пригодны для следующих владельцев. Полный semantic RAG-аудит R03, code traces R04–R07 и результаты обучения не присваивай R01. Learning measurements остаются NOT_MEASURED, пока опыта нет; independent review — NOT_PERFORMED без реального reviewer.

Перед каждой записью перечитай свежие bytes/blob целевого файла и актуальный ref. Пиши поверх актуального дерева, не force-push и не stale replacement. При конфликте сохрани чужую дельту и повтори документную проверку своего candidate. После write прочитай exact commit/blob и текущую ветку; если branch продвинулся, проверь сохранность своей версии в новом состоянии, а не откатывай чужие commits. Ответ операции, не обещание, подтверждает commit. `[skip ci]` — не PASS и не изменение branch rules.

**Форма ответа пользователю:** что добавлено к уже сделанному R01; какие части теперь можно исследовать/реализовать без повторного чтения; что осталось и какой именно вывод ограничено; точные опубликованные пути/commit. Не заканчивай формулой «теперь нужно начать R02»: R02/R03 могут уже идти. Не выдавай подготовленный prompt, source map или отдельную пару за завершение всей программы либо доказанный выигрыш hybrid обучения.
