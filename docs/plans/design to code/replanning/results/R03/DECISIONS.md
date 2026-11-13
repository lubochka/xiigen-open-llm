# R03 — решения ремонта и готовые предметные поручения

**13.10.2026 · CP08. Документный ремонт; полный R03 остаётся IN_PROGRESS.**

Система должна научиться выбирать предмет, представление и полезный путь решения, сочетая точные ограничения с обучаемыми решениями. R03 восстанавливает первичные основания этой архитектуры и передаёт их исполнителям алгоритмов. Его результат — не число прочитанных названий, а сохранённое решение с точным источником и следующим полезным действием.

D01–D21, H01–H06 и Q1–Q3 сохранены и уточнены; добавлены D22–D24. Полный прежний текст доступен в [снимке P6 до ремонта](https://github.com/lubochka/xiigen-planning/blob/20df95a38c5138639bda97f33b4c044d86f8f70e/llm%20mvp/design%20to%20code%20session/replanning/results/R03/DECISIONS.md). Подготовленный prompt не считается выполненным исследованием, экспериментом или независимой экспертизой.

## 0. Готовый старт следующего продолжения R03

Ниже один исполнимый вход для следующего назначенного продолжения R03. Он не запускает работу сам и не требует оператору собирать задание из нескольких книг. Общий dispatcher остаётся START-NEXT-RUN; эта карточка передаёт конкретный сохранённый остаток, а не создаёт новую очередь.

> **Задача.** Продолжи только R03 от опубликованного CP08: восстанови первичное решение и его преемника по одному незакрытому содержательному семейству, затем опубликуй пригодный для следующего алгоритмического исполнителя результат. Не начинай заново R01, не перепланируй все25, не ограничивай проект S04 и не выполняй продуктовую разработку.
>
> **Репозитории и собственный результат.** Planning=`lubochka/xiigen-planning`, branch=`llm_plan3`; core=`lubochka/llm_mvp_core`, branch=`codex/dpo-implementation`. Рабочий каталог `llm mvp/design to code session/replanning/results/R03`. Начальные evidence pins: planning P6=`20df95a38c5138639bda97f33b4c044d86f8f70e`; core K2=`a5de1a893db8e44e3dce0f4f54cdae2a4d06d1a3`. Сохрани source pins прежнего чтения; свежий HEAD нужен для дельты и безопасной записи, а не для полного restart. Прочти текущие собственные RUN-LOG, незакрытые строки SOURCES/CHANGE-MAP и относящиеся FINDINGS. Соседние R01/R02 — advisory, не prerequisite.
>
> **Сначала полезный вопрос.** Если более свежий собственный checkpoint ещё не закрыл его, начни с T11: какое решение `ORCHESTRATION-MACHINE-TRAINING-PLAN-v20`/Phase39 является преемником P22 и как сохраняется обучаемый выбор skills без подмены его fixed winner? Прямые входы: `llm mvp/visual studio & visual studio code continue/RAG-11-extensions-research.md` → repo-root `extensions reasearch/PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md`, §0 и named successor link. P22 на P5=`10dfbfd8a46f67b9cf43ce827d00d1dbf6d18078`,1–90, blob `a6df21e9b7a170381a01ea6b2944fa364c06f3c5` уже прочитан; переиспользуй его, открой непрочитанный преемник по реальному locator. Не угадывай путь v20 по basename и не называй IMPLEMENTED banner code proof. Если вопрос уже закрыт — следующий непокрытый T06/T08 transition/order из таблицы Q2, не повтор T11 ради нового commit.
>
> **Skills.** В core `docs/skills` прочти применимые секции `reference-inventory-and-source-map-SKILL.md` для root/ref/primary lineage; `trainable-architecture-decision-SKILL.md` для helper/learned/state owner; `phase-planning-SKILL.md` и `plan-review-SKILL.md` для посильного результата и контрпримера; `code-examination-SKILL.md` для границы source/code claim; `context-package-design-SKILL.md` для полного контекста решения; `output-readability-gate-SKILL.md` для понятного результата. Найди нужные named sections, запиши actual spans. Начальные адресные ориентиры K2: inventory110–205, architecture110–220, planning140–235, plan-review200–280; code-examination1–68, context-package1–90, readability1–78. Это инструкция будущему worker, а не утверждение, что он уже прочитал их. Применимые зависимости загрузи адресно; старые30/50-review routines и запреты parallel не отменяют нынешнее поручение.
>
> **Нельзя потерять архитектуру.** Unit получает domain/vector/output schema и выдаёт typed many→many; Flow учит отношения/коррекцию units; Orchestrator асинхронно комбинирует пути; DynamicGraph учит отношения паттернов разных порядков. Training owner — ILearningMachine; Graph=L5 specialization. Реальные ILogical*/generic names проверяй по core, не создавай четыре trainer из пользовательских aliases. Для восстановленного решения покажи deterministic часть, learned authority, изменяемое состояние, objective/signal, owner и ordinary consumer; неизвестное оставь неизвестным. Domain, intent, stack и physical model не один enum. Сохраняются DPO/DynamicGraph, DTC, IDE, S00–S10/дополнительные и семь базовых направлений WorkLeft с дополнительными языками/задачами.
>
> **Роли уже решены.** Открытая китайская модель пишет окончательные chosen/rejected. CODEX `astra 6 ultracode` сначала выбирает одну разрешённую учебную задачу, затем отдельно оценивает точную полученную пару. Primary: `llm mvp/design to code session/Kiro-Xiigen-S04-Luna-xhigh-v2-Pack/overlay/.kiro/xiigen-slices/luna/s04/RUN-CONTRACT.md`, P6:40–95, §§2.1–2.3, source P23. Слабый Student response — разрешённый отдельный контекст, не обязательный final rejected. Не переоткрывай авторство в R12, не переписывай пары под Chinese provenance, не отправляй закрытые targets автору, не выдавай requested model за actual call. One-pair/no-repeat/full-state accept-or-rollback сохраняются.
>
> **Работа.** Сформулируй конкретные unanswered questions выбранного семейства. Разреши только необходимые locator/root/version переходы Q1, сразу прочти относящиеся primary/successor sections Q2. Для каждой находки запиши exact repo/ref/path/blob/span, исходное ожидание, что действительно установлено и что не доказано, связь с механизмом/consumer, один основной downstream owner. Если адрес уже известен, не жди проверки остальных201+140 адресов. Остальные T-ветви и общий адресный остаток остаются явно открытыми, не объявляются irrelevant или DONE. Для T11 передай R06 механизм, R12 только scope исторического helper ban; полный актуальный code audit принадлежит R06, а не этому source переходу.
>
> **Понятный хороший результат.** «Первичный преемник сохраняет learned skill selection; конкретный helper только формирует допустимый пул. Есть или отсутствует подтверждённая связь с владельцем обучаемого выбора. R06 получает точные sections и проверяет механизм; будущий опыт сравнит полезность/полную цену на одинаковых новых задачах». Если primary утверждает другое, передай именно обнаруженное противоречие с обеими сторонами, не выдумывай желаемый вывод.
>
> **Плохой результат.** «Нашли v20, значит всё реализовано»; «RAG состоит из12 файлов — все пройдены»; «написано learned — результат лучше»; «проверим все старые адреса прежде любого алгоритмического вопроса»; «correct fixed answer должен быть rejected». Последний случай — ошибка оценки: pedigree алгоритма не заменяет наблюдаемое качество пары.
>
> **Края.** Root `extensions reasearch` находится в корне repo и содержит именно эту опечатку; другой root `extensions  - orchestration based on our data` содержит два пробела. Одноимённый файл не successor без lineage. Цикл переиспользует visited key; moved source требует mapping; недоступный/усечённый ответ не доказывает отсутствия реализации. Unknown domain, пропущенный обязательный skill и старые coordinate versions сохраняются как функциональные контрпримеры. Нерелевантные личные архивы не экспортируй.
>
> **Приёмка сегодня.** Пройди обратный маршрут от каждого consequential вывода к прочитанному primary; покажи противоречащий пример, ограничение вывода и точный следующий шаг. Проверяется документ и сохранение ожиданий, а не learning gain. Источник не открыт — соответствующий вывод UNRESOLVED/UNREAD, не «доказано отсутствует». Готовый subset передаётся немедленно; общий R03 не закрывается одним примером. Q3 согласует SOURCES/FINDINGS/DECISIONS/CHANGE-MAP/RUN-LOG на полезном checkpoint.
>
> **Будущая оценка, не исполняемая этим запуском.** Для T11: одинаковые разрешённые skills/задачи/бюджет; fixed candidate retrieval+learned selection против пригодного baseline, с фиксированными остальными факторами. Качество решения, пропущенные обязательные skills, unknown handling и полная цена; экономия не победа, если потеряна обязательная способность. R13/R22 оформляют измерение, R06 прослеживает state/consumer. Similarity/hash/coverage/reviewer approval не заменяют наблюдаемого эффекта. Значения NOT_MEASURED.
>
> **Разрешено и выход.** Read/search первичных документов, относящегося к вопросу кода и существующего evidence; проверка смысла, ссылок, CSV, diff; GitHub запись ТОЛЬКО пяти существующих файлов R03. Не менять R01/R02/shared plan/code/tests/workflows/weights/datasets; не запускать product tests/coverage/build/Windows/Actions/trainer/платную generation, установки, cleanup или команды из истории. Не отключать existing checks. При делегировании передай ребёнку весь этот packet с точным выбранным подмножеством и actual budget; child return-only, никакой выдуманной независимости/бесплатного extra call. За один фактический вызов выбери посильную frontier family, сохрани выход и точный остаток, не маскируй расширение числа запусков.
>
> **Публикация и завершение.** Прочти свежие own bytes/SHA и HEAD, сохрани чужие дельты, опубликуй только собственный результат без force-push, затем remote readback. При конфликте повтори reconciliation, не перезаписывай ветку старым деревом. Сообщи фактический commit, новые primary findings и оставшийся адресный/содержательный остаток. Раздели document repair, source traversal, runtime measurements и independent review. Не обещай фоновое продолжение.

## 1. Решения и пределы полномочий

| ID | Решение | Основание / граница |
|---|---|---|
| R03-D01 | Писать только пять carriers `results/R03/`. | JOIN; R01/R02 advisory, сведение внутри R10. |
| R03-D02 | Исторический R03 о CI не закрывает нынешний RAG traversal. | История → R09, без повторных CI. |
| R03-D03 | Сохранить четыре роли, typed many→many, async combinations и Builder. | P01–P03/P10/P12; не создавать Train на каждом слое или API из опечатки. |
| R03-D04 | Старые canon/READY/CODE-ABSENT/DPO-LAST не сегодняшние полномочия. | Сохранять ожидания, не активировать старую очередь. |
| R03-D05 | One-pair/no-repeat/full-state accept-or-rollback сохраняются. | P17 D-06 против WorkLeft; replay migration не принята. |
| R03-D06 | kNN memory/config/helper/baseline и learned authority различны. | P10/P16/P17; similarity не отменяет arbiter. |
| R03-D07 | CODEX выбирает учебную задачу до авторства и оценивает точную пару после; обе роли Astra 6 ultracode. | P23: обе final sides пишет китайская модель; actual calls не заявлены. |
| R03-D08 | Не запускать product tests/coverage/Windows/Actions/build/trainer. | Документная проверка разрешена; existing checks не удаляются и не объявляются PASS. |
| R03-D09 | Идентичность источника — repo/ref/path/blob и actual span. | Цикл не независимый источник; basename не identity. |
| R03-D10 | Пустой reader/диапазон после EOF не доказывает отсутствия файла. | P12 alternative read; различать недоступность и отсутствие. |
| R03-D11 | Target ORM — обучаемый узел общего графа, не вечный scaffold. | P12 correction, actual mapping — R06. |
| R03-D12 | Lifecycle reuse не доказывает перенос весов между координатами. | P12/P13; actual projection/consumer — R04/R05. |
| R03-D13 | Negative должен сохранять различимость причины ошибки. | P19; взаимодействие дефектов — отдельный опыт, не single-factor. |
| R03-D14 | T09 читать только по архитектурным вопросам. | Нерелевантные личные темы не экспортировать. |
| R03-D15 | REQ25–27 связаны с F17–19 и H02/H05/H06. | Сохранено исправление рассинхронизации CP07. |
| R03-D16 | Semantic order стороны отличен от порядка перечисления candidates. | P11; не сортировать осмысленную последовательность в bag. |
| R03-D17 | Pair judgment, objective и Student utility — отдельные стадии. | P11/P23; judge score не DPO loss. |
| R03-D18 | N−1 source-row attempts не N−1 независимых допущенных уроков. | Family/split/duplicates/ties/admission/no-repeat. |
| R03-D19 | Сохранить goal skill selection, согласовать исторический широкий helper ban. | P22 §0; нынешняя hybrid постановка не отменена. |
| R03-D20 | C11 LoRA reuse после проверки подключения. | Projection не доказывает optimizer/consumer/tokenizer/PC gain. |
| R03-D21 | Q1–Q3 — внутренние части R03. | Не новые R26–R28 и не три выполненных model calls. |
| R03-D22 | Авторство final chosen/rejected закрыто первичным решением P23. | Старое Student-only/teacher-teacher eval-only не active alternative R12; история не перелицовывается. |
| R03-D23 | Q1 адресует выбранный critical question; Q2 не ждёт полного Q1. | Полный адресный остаток сохраняется; Q3 на каждом полезном checkpoint. |
| R03-D24 | Не учить предпочтение ярлыка learned вместо результата. | Правильный deterministic ответ не negative по происхождению; механизм проверяется отдельным вмешательством. |

**Остаются открытыми:** replay70/30 против no-repeat; scope historical semantic-helper ban; unit recipient против solver identity и higher-order payload; actual serializer/objective/reference/coordinate compatibility; фактическая применимость старого admission-margin порога. **Авторство обеих final sides к этому списку больше не относится.** Решающие архитектурные вопросы — R12, разрешённые плановые изменения — R16. Независимый адресуемый вопрос не блокируется несвязанным конфликтом.

## 2. Общий контракт адресных H/Q packets

Planning D/G/C/F и core — как в готовом старте. P3=`9db9b84b71d90a3ba28153befd139132988d931c`; P4=`9ab76cac434331a407aa0a01cf642b6543baf287`; P5=`10dfbfd8a46f67b9cf43ce827d00d1dbf6d18078`; P6/K2 — выше. Source IDs разрешаются по собственному SOURCES.csv. Путь с alias D означает путь относительно planning repo, не буквальный каталог D.

H01–H06 — предметные дополнения к одному existing effective prompt соответствующего Rxx, не новые конкурирующие задания. Dispatcher включает содержание нужного H, собственный Rxx и относящуюся специализацию, без повторного чтения всех25/всей библиотеки. При делегировании общий контекст и конкретный вопрос/выход/examples/verification передаются ребёнку полностью; child return-only, parent интегрирует. Реальные дополнительные calls учитываются, последовательное self-review не называется independent review.

**Skills будущего worker:** source investigation и architecture/planning/review из §0; для code seams — code-examination/generated-code-review; для authoring/evaluation — context-package-design/prompt-patch-authoring и относящиеся experiment-design/improvement-measurement секции. Записывать фактические spans и зависимости. В этом CP08 реально прочитанные секции перечислены в RUN-LOG и SOURCES/CP08-K01–K06; список будущей загрузки не подменяет чтение.

**Единица возврата:** вопрос и исходное ожидание; exact source/ref/blob/span и read depth; primary fact/interpretation/hypothesis; producer→state→consumer либо первое оборванное звено; good/bad/edge; документная приёмка; будущий различающий опыт; один основной owner и consumers; точный остаток. Нельзя выдавать interface/hash/pair count за learning. Разрешения §0 действуют для R03; downstream Rxx работает в своём разрешённом scope, не получает продуктовых полномочий из H.

## H01 → R06: домен, динамический kNN и окончательная оценка

**Контекст/источники.** P16 F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1:1–110; P17 D-04/D-10/D-15; P10:81–150; P22:1–90 для historical helper ban. R08 получает domain/data контракт, не повторяет механизм. Skills: source/architecture/code/measurement по §2.

**Промпт.** Проследи request→domain/intent/stack→versioned features→допустимый pool→retrieval/ranking→learned choice→final evaluation→ordinary consumer. Для каждого выбора укажи состояние, objective/signal, training owner и наблюдаемое влияние. Отдели пополнение памяти, смену config k и обученную policy. Для домена выпиши значения/единицы признаков, scaling, distance/score, k/radius, positive/negative/warning weighting, uncertainty/empty handling, coordinate versions. Какие constraints fixed, какие параметры учатся, как выбирается следующий вариант и сколько стоит reindex? Не назначай universal cosine/0.50/optimal k по старому примеру. Scope helper запрета передай R12 с фактическим местом подмены либо сохранения learned authority.

**Выход/приёмка сегодня.** Source/method/state/consumer trace и отдельные mapping/retrieval/solver gaps, не утверждение измеренного gain. **Хорошо:** близкая запрещённая регистрация — warning, а не автоматический accept. **Плохо:** highest similarity = final truth; dynamic = новый if. **Края:** SQL×BUG, unknown/mixed, contradictory memory, nonmetric score, empty/wrong pool, stale encoder/index.

**Будущий опыт R13/R22.** Known-domain против predicted-domain диагностирует routing; затем fixed/dynamic kNN при фиксированных encoder/solver. Одинаковые task families/panel и бюджет; admissible quality, полная цена всех кандидатов, память, retention. Смена одновременно tokenizer/router/data не позволяет приписать gain одному kNN. R07 получает только неразрешённый стык.

## H02 → R05: правильная пара, правильный recipient и реальный update

**Контекст/источники.** P23 P6:40–95 §§2.1–2.3 — действующие роли; P11:40–118/195–458 — schema/order/objective/fan-out; WorkLeft N07:1–50 и P06/P07 update sections; P17 D-06; C11 K2 `10.LoRA/LoraLinear.cs`:1–145. Skills: architecture/code/source/prompt/measurement. Старые данные — baseline, не новый разрешённый корпус.

**Промпт.** Проследи existing accepted state и разрешённую сохранённую диагностику → CODEX `astra 6 ultracode` выбирает ОДНУ eligible teaching task → замораживаются source/material/context bindings → полный разрешённый packet со слабым ответом Student на отдельную учебную задачу/историей → китайская модель пишет final chosen/rejected → CODEX `astra 6 ultracode` оценивает точную пару → отдельный admission → full pre-pair snapshot → serializer/objective/reference → mutation → сопоставимая оценка → accept/rollback → следующий ordinary inference. Выбор учебной задачи не переносить после авторства; pair assessment не подменять code review или Student baseline/trial scoring.

Назови actual LoRA/optimizer/index/graph/RNG owners. Сохранённая/sealed/structurally valid пара не автоматически optimizer-eligible. Changed pair/context требует оценки именно новой identity; CODEX не переписывает стороны под Chinese provenance; semantic rejection не создаёт новый authoring permit. Сохрани реальные origins исторических Student-negative пар, но не навязывай их как текущий запрет китайскому автору обеих final sides. Авторство НЕ передаётся R12 на новое решение.

Проследи unit recipient отдельно от выбранных solver IDs, ordered content внутри стороны отдельно от graph trace и enumeration order. Нижний четырёхпольный DTO не доказывает потери адреса без проверки envelope/consumer; старый четырнадцатипольный канон не разрешает произвольное новое поле. Не добавляй Train каждому Unit/Flow. Дойди до полного serialized policy/reference objective. Judge score не loss. Если используется иной pairwise/vector objective, назови его честно и укажи его learner, а не объявляй arbitrary similarity стандартным DPO.

N−1 attempts принадлежат одной family; admission каждой допустимой пары не даёт разрешения на minibatch/replay. Unknown-running update после сбоя не безопасно повторять или откатывать одновременно с продолжающейся мутацией. Найди actual consumer старого positive pre-margin admission из CP5 D2; при pi=pi_ref m=0 не доказывает семантический дефект первой пары. Не утверждай runtime bug без consumer и не выбирай новый порог наугад.

**Выход/приёмка.** Одна actual producer→recipient→update→consumer трасса либо точное первое отсутствующее звено и bounded work order. **Хорошо:** новый ordered output достигает scorer, применённая один раз пара восстанавливается через допустимую оценку/rollback. **Плохо:** JSON сохранён — обучение выполнено; judge0.9 вставлен в loss; N−1 применены автоматически. **Края:** missing reference/serializer, partial snapshot, pending mutation, shared winner, TIE/BOTH_BAD, domain regression, incompatible index/adapter/tokenizer.

**Будущий опыт R13/R22.** No-update/update на сравнимых состояниях и задачах при фиксированных остальных факторах; новый user input оценивается до обучения на нём. Pair quality отдельно от Student utility. Время generation/judgment/update/evaluation/recovery отдельно и суммарно, качество и retention. Operational panel не untouched certification; отсутствие gain не исправляется ослаблением рубрики.

## H03 → R04, затем R05: токены, признаки, проекция и LoRA

**Контекст/источники.** P12 §P3-B/§P3-C; P13 translation tokenizer:1–82; P17 D-03/D-05/D-07/D-10; C11:1–145. WorkLeft CODEDOC-00-tokenization-research сначала разрешить по actual tree, не выдумывать тело. Skills: architecture/code/source/measurement.

**Промпт R04.** Проследи raw input→parser/tokenizer IDs→domain semantic features/signals→encoder/projection→compatible retrieval coordinates→typed output/materialization. Для звена назови schema/version, cardinality/association, meanings/units и consumer. Не смешивай слова, typed signals, tensor и output decoding. StackDetector-before-normalization и stack/version markers сохраняют смысл. Lifecycle reuse не доказывает совместимость raw-word и signal weights.

**Продолжение R05.** От установленной проекции найди adapter installation в активном Student, parameter groups/freeze policy, update и ordinary inference. C11 содержит A/B forward, но не доказывает эти связи. Не делай второй tokenizer под видом LoRA, не копируй исторический ±eta эскиз как проверенный DPO. Изменение координат требует версии индекса/состояния и полной цены миграции.

**Выход/приёмка.** Actual shapes/methods/consumers и смысловые distinctions, gaps отдельно; нет круга tokenizer=LoRA. **Хорошо:** отрицание, единица, SQL relation и identifier доходят до результата; many→many outputs сохраняют связь с inputs. **Плохо:** shape совпала — смысл доказан; adapter2MB — PC fit. **Края:** Hebrew/Cyrillic/Latin, OOV, потерянный stack marker, one→zero/many→many collapse, duplicate enumeration, stale checkpoint/reindex memory spike.

**Будущий опыт R13/R22.** Одно representation изменение при фиксированных остальных факторах; затем отдельное adapter вмешательство. Семантическая fidelity, mapping, downstream accuracy, total memory и inference/update/evaluation/reindex time. Наличие матрицы/картинка векторов не доказательство полезности.

## H04 → R06/R24: DTC-поставка и тонкие IDE-хосты

**Контекст/источники.** P05 D/task prompt.md; P12 §1.7; P20 July8 report:1–90. Skills: architecture/source/code/readability. Минимальный урок ускоряется без удаления конечного результата.

**Промпт.** Восстанови tree/HTML/CSS/images→API→несколько разрешений→visual quality и user-tier budget→БД входа/результата→Builder/negative→accepted weights→publication→ordinary consumer. Назови исходные product owners/repos и current source либо unknown. Недоступность чтения не равна отсутствию реализации. IDE сохраняет manifest/trigger/thin entrypoint/core-call/trace; semantic selection/training не дублируются в каждом host. Раздели implementation dependency и release-only Windows/IDE obligation с условием возврата; supporting-copy рядом со старым dispatcher не migration.

**Выход/приёмка.** Original step→owner→current source/unknown с сохранёнными зависимостями и минимальным первым уроком. **Хорошо:** local lesson измерим, API/DB/IDE поставка остаётся явной. **Плохо:** один CodeDocumentation урок закрывает весь DTC. **Края:** разные разрешения, недостоверные изображения, free/pro cost, reload предыдущей модели, host-side fixed semantic winner.

**Будущая оценка R24.** Одни пользовательские сценарии до/после, visual/semantic quality, цена и использование принятого состояния. JSON-only не доказывает desktop host; desktop release proof не становится gate каждого документного RAG прохода.

## H05 → R08: доменные законы, positive/negative и качество пары

**Контекст/источники.** P19 P5:1–105; P14 translation guidance:1–94; P18 domain map:1–90; WorkLeft P06/P07; P11:315–458; P23 current roles. Skills: source/architecture/prompt/experiment design. Это протокол, не authoring campaign.

**Промпт.** Для каждого исходного домена восстанови task classes, versioned law/constraint/preference, допустимые альтернативы и эталон. Свяжи domain→feature/token→near-miss defect→наблюдаемое последствие→criterion. Раздели ошибки routing, path и answer. Корректность первична; компактность сравнивается среди допустимых решений. Семь WorkLeft направлений — EN↔RU, orchestration, Figma→HTML/CSS, C#, Angular, SQL documentation и Builder для шести — сохраняются вместе с дополнительными языками/intents, а не становятся исчерпывающим hardcoded enum.

Китайская модель пишет обе окончательные стороны. CODEX `astra 6 ultracode` выбирает урок, отдельно оценивает точные стороны и context binding; внешний автор не self-judge. TIE/BOTH_BAD/unknown/обоснованное abstention не превращать в принудительную пару. N−1 attempts сохраняют одну family/split и реальные side origins. Историческое Student-only правило не active alternative; действующее авторство P23 не переоткрывается в R12. Pair validity не Student utility.

**Образец A — только ошибка зависимости, иллюстрация, не training row.** Контекст: «Нужен план запроса к БД; актуальная схема ещё не получена, mapping зависит от неё». Допустимый порядок: `получить схему → построить mapping → выполнить запрос → оценить результат`. Близкий плохой порядок: `построить mapping → получить схему → выполнить запрос → оценить результат`. Дефект — преждевременная зависимая операция, не стиль или provenance. Остальные действия сохранены. Коды ORCH/WRONG и rationale находятся в audit envelope, не автоматически в candidate text; actual serializer/leakage проверяет R05.

**Образец B — проверка authority, НЕ готовая DPO-пара.** Fixed selector и learned selector могут дать одинаковый корректный порядок. Тогда слово fixed не основание назвать один ответ rejected. Для проверки обучаемого механизма R06/R22 фиксируют units, меняют только допустимое learned relation state и наблюдают обобщение на новые сочетания; отдельно сравнивают fixed baseline и цену всех ветвей. Нет установленного качественного/стоимостного различия конкретных допустимых сторон — TIE/нет preference. Если actual result теряет обязательный шаг или уступает по заранее определённой цене, причиной preference становится этот наблюдаемый дефект, не ярлык реализации. Этот контраст исправляет CP07 authority-only negative; авторские illustrative bytes не выдаются за Chinese-generated dataset.

**Выход/приёмка.** Для каждой заявленной task family есть primary/пример и law-feature-pair-criterion цепь; near-miss действительно нарушает критерий, single-factor не содержит второй дефект. **Хорошо:** SQL×BUG negative теряет ровно одну обязательную caller/write/transaction связь, оставаясь правдоподобным. **Плохо:** бессмыслица, пять ошибок, неверное маркирование обязательного отказа, предпочтение слова learned. **Края:** два верных пути разной цены, оба неверны, спорный закон, contradictory feedback, multilingual negation, near-duplicate другого split, unknown domain.

**Будущая оценка R13/R21/R22.** Blind pair quality/label stability с abstention, затем no-update/update Student result на независимых families. Generator/Builder не видят closed certification targets. Shared operational panel сравнивает состояния; certification отдельно. Accuracy, time-to-valid-pair, full lesson cost, retention и неопределённость; мнение Astra дополняет, но не заменяет измерение.

## H06 → R06, затем R07: четыре роли и обучение отношений

**Контекст/источники.** P10:1–150; P12 §1.4.1/§1.4.7; прежний C10 DynamicGraphContracts:250–379 как carrier evidence; P11:195–458 для order. Current code names/consumers проверяются на fresh core. Skills: architecture/code/measurement.

**Промпт.** На одном сквозном случае покажи множество typed inputs/outputs Unit, relation/correction Flow, async fan-out/fan-in Orchestrator и higher-order DynamicGraph. Запиши port meanings, materialization, alternatives/conflicts, cancellation/partial results, state owner, final semantic authority. Отсутствие Train у узла не доказывает отсутствия learned state: update через machine owner и inference узла различаются.

Сохрани target ORM reasoning state; v1 scaffold не финальный полый adapter. Различи unit identity, ordered semantic payload и evidence-only graph trace. Перестановка списка candidates не даёт positional bias; порядок внутри side сохраняет зависимости. Найди serializer/consumer прежде вывода о потерянном канале. Если нынешний owner/payload не обучает нужные отношения, передай exact gap R12, не стирай требование.

R07 после R04–R06 исследует только неразрешённые связи: что меняется при том же input, какой selector получает credit, согласованы ли coordinate/graph/state versions. Выпиши fixed units/change relation и обратное вмешательство fixed relation/change units. Global reward всем edges не объясняет причинность; полный повтор source audits не нужен.

**Выход/приёмка.** Четыре роли→current owner/state/consumer, transient/target отдельно, отсутствующее звено явно. **Хорошо:** те же units/task, изменён один learned relation selector, допустимые пути сравниваются по результату и полной цене. **Плохо:** first-in-list = learning; sequence превращена в bag; ORM навсегда пустой. **Края:** partial async, conflicting fan-in, цикл, одинаково хорошие paths, many→one collapse, unseen composition, stale coordinates.

**Будущий опыт R13/R22.** Fixed graph против одного learned relation/order вмешательства, затем отдельно interactions. Admissible quality прежде cost, новые композиции и retention. Loss/hash/isLearned и количество edges не доказывают эту способность.

## 3. Внутреннее продолжение R03: Q1–Q3 без массового входного барьера

Это три вида работы в одном R03, не автоматически три новых вызова или R26–R28. При действительном делегировании dispatcher разворачивает общий контракт §0/§2 и конкретную карточку ниже полностью. Одного названия Q или ссылки ребёнку недостаточно. Собственные пять carriers остаются единственным местом записи; child return-only.

### Q1 — адресные receipts для выбранного вопроса и полный явный остаток

**Роль/skills/вход.** Source investigator; inventory/planning/review/readability. N00–N05, WorkLeft-RAG-00 usage, SOURCE-ROUTES по нужному root, четыре MD/NDJSON и actual trees/archive mappings. Прежние201/140/33/30 records и X01/X02 для63 адресов — prior evidence, не fresh parse. Известные адреса сразу доступны Q2; полный Q1 не prerequisite.

**Полный предметный prompt.** Возьми unanswered question из Q2. Если его primary locator уже подтверждён и relevant blob не изменился, используй receipt; не проверяй сначала весь архив. Иначе открой соответствующую index record, declared root, current MD metadata и реальный tree/mapping; сохрани row/key и resolved repo/ref/path/blob. Статус: resolved, relocated-with-primary-mapping, cyclic, unavailable или outside-scope с причиной. Одинаковый basename не автоматический successor. После разрешения нужного пути сразу передай его Q2.

Параллельно по смыслу работы, без обязательного второго worker, веди отдельный полный адресный остаток DPO201/DTC140 и ранее заявленных63. Для реальной сверки группы получи записи без усечения и проверь actual counts; прежний blob/parse не выдавай за новое независимое исследование. Archive target может быть единственным носителем живого решения. DELTAS:1–57 и master:1–70 предупреждают об историческом NDJSON и текущей MD metadata; offsets не переносить автоматически. Не делай массовые guessed GET. Обрабатывай одну declared-root группу и сохраняй checkpoint, не перекрывая содержательную работу. Unresolved receipt содержит испробованный маршрут, последнего рабочего родителя, вопрос, одного owner и одну следующую операцию.

**Выход/приёмка.** SOURCES с прежней девятиколоночной схемой; каждый реально разобранный адрес имеет воспроизводимый receipt, суммы статусов относятся только к разобранному subset. Body-unread не повышается до primary-read. Остаток не исчезает из-за приоритета критического вопроса. **Хорошо:** архив найден по mapping, body пока unread; уже известный P22 сразу доступен Q2. **Плохо:**341 CSV rows =341 primary reads;404=кода нет; все адреса blocking gate. **Края:** encoded/two spaces, case mismatch, directory target, truncated tree, ambiguous successor, concurrent source delta.

**Передача.** Q2 — конкретный путь/неразрешённый адрес; R01 — advisory inventory delta, не повтор его работы. Q1 сам learning gain не измеряет.

### Q2 — первичные решения и преемники по одному семейству вопросов

**Роль/skills/вход.** Source+architecture investigator, code/prompt/readability по §2. Подтверждённый relevant subset Q1 ИЛИ прямые T/P locators, own FINDINGS/CHANGE-MAP, текущий source route. Не ждать Q1-DONE/R01-DONE/R02-DONE. Выбрать посильную frontier family и её exact questions; остальные остаются в таблице.

| Группа | Конкретный вопрос и вход | Граница / получатель |
|---|---|---|
| T01/merge | Compare-branches integration/verification primary и P21 convergence: какое обязательство живо? | Dependency/authority sections, не все commit logs; R04 relevant seam, не историческая mergeability. |
| T02/T03/T07 | Query-planning, router, source-registry, examination: что ещё не покрыто P13–P15/P18? | Named contract→successor; R06 mechanism, R08 data/laws; closed targets не раскрывать. |
| T06/T08 | Исходный transition/order design и его successor? | Actual T34/SecurityArbiter paths из индекса/дерева; BuildPackets/Evaluate и successor declaration; full current audit — R06. |
| T09 | Уникальное авторское архитектурное решение сверх прочитанных primaries? | Named relevant spans RAG-09; личные темы исключить без экспорта. |
| T10 | Machine/v2 successor меняет ли one-error/DPO/authority P19? | Exact successor sections; R08/R12; P23 final authorship не переоткрывать. |
| T11 | v20/Phase39 successor P22 и связь с43.SmartSkillsDomain? | Resolve exact link/named section; banner не код. R06 mechanism; R12 helper-ban scope. |
| T04/T05/T12 | Цикл к прочитанному D/G/core или уникальная поправка? | Reuse visited key; новая decision delta, не новый canon из supporting copy. |
| WorkLeft/P12 | Уникальные four-role/order/representation/consumer decisions в DELTAS/CHRONICLE/COVERAGE/catalogue? | Named question прежде span; прежние P12§1.4.1/1.4.7/P3-B/P3-C/1.7 не перечитывать без дельты; не назначать1.56MB одним вопросом. |

**Полный предметный prompt.** Для выбранной строки назови точные unanswered questions. Через relevant locator открой первичный span, проверь реальный successor и запиши source→исходное ожидание→correction/conflict→algorithm/state/consumer→owner. Primary fact, авторская интерпретация, current-user requirement и empirical hypothesis должны быть различимы. Один хороший пример не закрывает все вопросы строки. Сохраняй semantic order, domain/intent/stack, scoped kNN, separate tokenizer/projection, ordinary consumer, current P23 roles и no-repeat.

Останови конкретное чтение, когда вопрос имеет primary trace либо точный unresolved/unavailable receipt с одной следующей операцией и owner. Unresolved остаётся unresolved: формальная передача не доказательство решения. Непрочитанные файлы не объявляй irrelevant без причины. Если объём раскрывает несколько независимых contracts, раздели по вопросам и source spans; каждой реально отдельной задаче передай полный packet со своим result/good/bad/edge/verification и фактическим бюджетом. Сначала переиспользование и распределение внутри25; дополнительные реальные calls учитывать явно, не прятать их в A/B и не объявлять все завершёнными.

Готовый subset R04/R05/R06/R08 передай сразу, не жди несвязанного архива. Полный R03 остаётся открытым, пока обязательный адресный/содержательный остаток не исследован либо явно и посильно распределён с сохранением статуса. Сам R10 не становится владельцем всего недочитанного.

**Выход/приёмка.** SOURCES actual spans; FINDINGS только новая/исправленная мысль; CHANGE-MAP сохраняет expectation→primary→one owner→examination; DECISIONS обе стороны нерешённого конфликта. **Хорошо:** P22 goal сохранён, v20 найден по lineage, область helper запрета не угадана. **Плохо:** все12 RAG пройдены по первым50 строкам; newest report автоматически прав; непрочитанный источник признан отсутствующим. **Края:** отсутствующий successor, provenance сомнителен, changed blob, no code bytes, повторный отчёт, пустой reader, цикл.

**Будущая оценка.** H01–H06 передают R13/R22 mapping, DPO quality/update utility, controlled kNN/LoRA/order contrasts, качество/время/память/retention. Q2 не исполняет опыт и не назначает выдуманные thresholds или measured gain.

### Q3 — согласование собственного результата на полезном checkpoint

**Роль/skills/вход.** Own-result integrator; source inventory, plan review, prompt/readability/publication. Свежие собственные пять carriers, фактический прочитанный subset Q1/Q2, JOIN и relevant source/sibling delta. Q3 не ждёт полного окончания всех ветвей.

**Полный предметный prompt.** Проверь собственную цепь F/REQ→primary→H→owner и честность evidence epoch. Исправь dangling pointers/расхождения CSV/человеческого текста/журнала, сохрани IDs и уникальные решения. REQ25–27 ведут к F17–19 и актуальной поправке F22, а не phantom CP05 log. REQ10/27 больше не зовут R12 решать authorship. История не переписывается и self-review не становится независимостью.

Пройди документно: (1) SQL×BUG domain/features/kNN/четыре роли/path/result; (2) meaningful sequence сохраняется при перестановке candidate array; (3) CODEX teaching selection→Chinese final pair→CODEX exact assessment→admission→update/evaluation/ordinary consumer без replay; (4) одинаковый правильный fixed/learned ответ не искусственная negative pair; (5) уже адресуемая Q2 ветвь продолжается при открытом unrelated Q1. Это проверка описания, не исполнение runtime.

Перед записью reread own bytes/blob и HEAD. Публикуй только свою дельту с проверкой SHA/lease без force. Если HEAD изменён соседями, сохрани их дерево; если изменён own file, перечитай и согласуй, а не перетри. После публикации readback actual bytes/commit. Атомарность заявляй только для действительно единого ref update; последовательные content updates не атомарная пятёрка. Не выдумывай self-SHA.

**Выход/приёмка.** Нет необъяснённых потерь IDs/смысла, актуальный handoff и точный unread/unresolved остаток. Отдельные статусы document_review_repair, source_traversal, runtime_measurements, independent_review. **Хорошо:** REQ→primary+F+H+owner, family split сохранён, действующая роль передана содержанием. **Плохо:** repair переименован в R03_DONE; Astra reviewed без вызова. **Края:** concurrent own edit, partial publication, late source delta, совпавшие local IDs разных runs.

## 4. Передача и окончание документного ремонта

R04 получает P10/P12/P13 и semantic order; R05 — P23 current roles, P11 serialization/objective/fan-out и C11 projection; R06 — P10/P12/P15/P16/P17/P22 graph/domain/kNN/skills; R08 — P14/P18/P19/P23 и исправленные meaningful contrasts. R12 получает только реально открытые policy/architecture вопросы, не повтор принятого author decision. R10 объединяет versioned checkpoints, не выполняет вместо R03 весь source backlog.

**Следующее собственное действие:** адресуемый незакрытый successor вопрос T11/v20 либо T06/T08 по готовому старту §0; Q1 вызывается по его locator-потребности, широкий адресный остаток остаётся явным. Это не разрешение пропустить другие домены/roots и не требование ждать полного R03 перед использованием проверенных subsets. R13 готовит различающий опыт; R18 — smallest implementation packet по общей очереди. Сам этот repair не запускает code/training.

Learning measurements — **NOT_MEASURED**. Отдельная Astra Ultra review — **NOT_PERFORMED**. CODEX selection/assessment в продукте — **NOT_PERFORMED этим ремонтом**. Документные проверки не product test coverage и не Windows gate.
