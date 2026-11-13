# Доказательства, новизна и границы аудита PrepareToGPL

Дата: 18 октября 2026, UTC. [Главный список с восстановлением в начале](00-PREPARETOGPL-RECOVERY-FIRST-2026-10-18.md). Цель — собрать различия, способные ухудшить обучение или скрыть его результат, и дать короткие различающие опыты. **Источник не равен результату исполнения; найденный разрыв не объявляется причиной конкретного RP01 без связи с его caller и состоянием.**

## 1. Зафиксированные снимки

| Репозиторий / роль | Ref и SHA | Реальный объём проверки |
|---|---|---|
| llm_mvp_core / хорошая ветка пользователя | PrepareToGPL — **31346d8928be4469b01aaf313780e8f950ff91e2** | Целевые DPO/LoRA/tokenizer/TensorCore/модель; generated Unit/Flow/restore; SemanticCompiler; translation state; первичные E07/E09 |
| llm_mvp_core / проверяемая реализация | codex/dpo-implementation — **a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6** | Сопоставимые классы и вызывающие пути; свежие сохранённые reports/results; оба слоя core-каталога |
| xiigen-planning / исходный каталог | llm_plan3 — **81a0f456ef318d812a55c3d07185c69615fe2fdd** | Existing possibleDifs, RP01 pack, выбранные исходные требования и RAG маршруты |
| xiigen-planning / сверка перед публикацией | llm_plan3 — **a4a3e709f8fb5310baab6daa16501b5d4394bd72** | Дополнительно11 новых Markdown N40–N42, свежий README, изменения применимых инструкций; повторная смысловая дедупликация |
| xiigen-planning / конечный параллельный пакет | llm_plan3 — **004c4a79f3ec0c1dba387ba713abad108b7568fe** | Ещё6 Markdown из2026-10-18-preparetogpl-recovery, повторная проверка точных новых механизмов и сохранение нового README |
| llm_mvp_core / соседняя переводческая ветка | language_translate — **f6032aff81f1f83de7c09cf9b9efe6b058dd12b9** | Полный ContextualDictionary.cs |
| llm_mvp_core / соседняя структурная ветка | external_domains — **3036a01acec938f0b83fa4ffbda26202cc35d639** | Полный StructuralKnn.cs |
| llm_mvp_core / соседняя ветка дизайна | DesignToCode — **8f20ceb8a9bfbadab2ad309226c47dd76e714548** | Полный TranslationMachine.cs |

Другие refs были инвентаризированы, но полный аудит всех файлов всех веток не заявляется. Ключевые утверждения привязаны к immutable blob URLs с SHA, а не к плавающему названию ветки.

**Побайтное контрдоказательство:** 12.SemanticCompiler tree SHA **cae2d9355e803e5b46ec52b7d4fe0f00b524af0f** одинаков old/current; все 27 файлов совпадают. Также совпадают translation trained units/tokenizer и прежний tracked checkpoint. [Git-идентичности](PREPARETOGPL-BLOB-IDENTITY-2026-10-18.json), [объяснение и прямые файлы](PREPARETOGPL-KNN-HISTORY-2026-10-18.md). Три прочитанных sibling source отличаются от PrepareToGPL только пятистрочным лицензионным заголовком.

## 2. Какие документы использованы и как проверялась новизна

Получены все **367 файлов** исходного planning possibleDifs, из них **272 Markdown**. Из core получены **92 верхних Markdown** и **168 вложенных Markdown** из experimets, в том числе 125 карточек P01…P125 и новые отчёты. Итого **627 файлов, 532 Markdown** в локальном корпусе сопоставления.

Эти числа означают получение содержимого и возможность поиска по нему. Они не означают чтение каждой строки каждого документа. Выполнен адресный текстовый поиск по механизмам/символам и смысловая проверка ближайших карточек. Raw JSON/binary деревья nested results были инвентаризированы; целиком скачаны и прочитаны только относящиеся первичные материалы. Название «новая» ниже означает отсутствие найденной **этой конкретной причинной цепи** в данном корпусе, а не гарантию отсутствия похожей идеи у любого автора.

**Обновление перед публикацией:** ветка planning продвинулась на20 commits. Дополнительно получены11 новых профильных Markdown N40–N42; они просмотрены на пересечение механизмов, причём ключевые recovery/novelty главы прочитаны полностью. README берётся с нового head целиком. В N41-T1 уже опубликован разрыв4→6; его первоначальная классификация «новый» в рабочем черновике снята. Core branches при повторной проверке остались на тех же SHA. Свежие corpus-resume/intrabatch, topic-domain erasure, guide schema/admission, metric drift и dual-checkpoint темы присоединены ссылками, без присвоения авторства.

При первом обновлении ref GitHub вернул ошибку; повторное чтение обнаружило один новый commit004c4a79 с шестью документами параллельного пакета. Они получены и проверены профильными исследователями на точные пересечения; реестр новизны прочитан основным интегратором. Новые там TranslationModelTrainingDataEvidence, raw PairId против derived Unit IDs и намеренное сохранение attached adapters при legacy base-import не дублируют наши пять цепей. Пакет пересобран поверх нового head с полным сохранением чужих файлов. **ID U-N3 в соседнем пакете означает иной механизм**; здесь использовать PG-11 и полное имя pattern/materializer карточки.

Маршруты к исходному замыслу:

- [RP01 source and architecture](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md), [learning examination](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/LEARNING-EXAMINATION.md), [current code map](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md).
- [Recovery/code audit](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md), [source routes](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/SOURCE-ROUTES.md), [recovered design contract](https://github.com/lubochka/xiigen-planning/blob/81a0f456ef318d812a55c3d07185c69615fe2fdd/llm%20mvp/design%20to%20code%20session/replanning/RECOVERED-DESIGN-CONTRACT.md).
- Исходные «original task.md», «Алгоритм создания ДПО.md» и translation four-level training plan; RAG master использован как маршрутизатор к требованиям. Четыре NDJSON инвентаря 09.09.26 разобраны как данные: DPO/legal 201 строка, design-to-code 140, merge/branches 33, IDE/Continue 30. Статусы RAG не принимались за доказательство выполненного обучения.

Из двух прежних каталогов особенно важны core 02/25/26/41/70/78, P01/P10/P11/P12/P25/P85/P86/P92/P94/P95/P105/P117/P119/P125, planning N23/N26/N27/N33, matrix→KNN и restore/consumer документы. Их ссылки приведены в профильных записках и карте осей главного списка.

## 3. Реестр новой дельты

| Карточка | Что добавлено относительно существующих материалов | Статус и соседняя известная тема |
|---|---|---|
| **PG-01 / legacy 4→6** | Конкретный tracked schema1 checkpoint с blob a4cf… конфликтует с mandatory six-direction runtime; точная пара old/current, источник отказа до state import | **Независимое подтверждение N41-T1, не новая уникальная гипотеза.** Добавлены tracked asset identity и source preflight; P25/P117 — соседние темы |
| **U-N1 / PG-06** | Полная ordinary chain: routed_sub_phase_id в metadata → original UserRequest → latest Unit по role/domain → scorer только Input → одинаковый retrieved output для разных обязанностей | **Новая локализация связи caller→registry→retrieval.** Смежные core26/70/P92/P94 описывают routing/consumer. Сама ambiguity унаследована; current preservation retrieved text делает её заметной |
| **U-N2 / PG-07** | DPO branchPlans против KNN registrationPlans после grouping; общий документ меняет document length и расположение discriminant относительно embedding budget | **Новая гранулярность, уточняющая известный cutoff64.** Не новое открытие prefix alias N23; old duplicate-ID overwrite учтён |
| **DLT-1 / PG-05** | Warm-cache→cold-construction меняет расход общего RNG, включая конструкторы перед перезаписью checkpoint; следующий новый domain получает другое исходное состояние | **Новая caller-level зависимость, условная при отсутствии внешнего scope.** Сам Random42 уже известен core02; HNSW RNG — N33/P22, другой recipient |
| **DLT-2 / PG-09** | Sequential float totals могут стереть представимый policy/reference delta до stable softplus; границы float carriers отмечены | **Новая численная гипотеза, унаследованный механизм.** Не повтор AdamW scale cancellation N26 и clipping overflow P12. Есть numeric proxy, native incidence неизвестна |
| **U-N3 / PG-11** | Готовый learned_pattern имеет приоритет над chosen/rejected feature arrays в нынешнем materializer | **Новая условная диагностика контракта.** Сам API отказывается обещать semantic training; ожидаемую инвариантность нельзя объявить поломкой |
| **DLT-3 / PG-10** | Q/K не влияют на единственный target, если его позиция видит ровно один causal key | **Узкое математическое уточнение P01/P10, не самостоятельное доказательство дефекта/регрессии.** Не включается в число новых доказанных ошибок |
| **U-R1 / PG-04** | Old history-union сопоставлен с current signed corpus и bounded replay; отдельно замечена старая ID-коллизия | **Уточнение N27/P24/P92.** Не выдаётся за новую гипотезу top3 |
| **U-R2 / PG-04** | Старые IsLearned descriptors и manifest несовместимы с current executable-v2 | **Конкретизация известного класса restore.** Реальный успешный old generated bundle ещё требуется; silent fallback не найден |
| **DLT-K1/K2** | AdamW moments C#→ENRU; неподдержанный legacy batch>1 carrier | **Crosswalk известных тем.** Не новые причины RP01, не повод ломать точный continuation |

У всех новых карточек есть domain/recipient, before/after, исходник, условие достижимости, маленький опыт и фальсификатор. **Консервативная новая дельта — пять конкретных механизмов:** U-N1/U-N2/U-N3 и DLT-1/DLT-2. На уровне общих семей контекст, cutoff, материализация и RNG уже исследовались; новизна относится к указанным точным цепям. DLT-3 — дополнительная математическая граница внутри P01/P10. Все они требуют проверки влияния на реальные ответы.

Свежий crosswalk:

- [N41-T1:4→6](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md) — приоритетное известное расхождение, независимо подтверждённое здесь.
- [G40:guide schema/admission/consumer](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md) и [N42:dual checkpoint/metric/branch tests](42-NEW-CAUSAL-HYPOTHESES-AND-DOMAIN-MICROEXPERIMENTS-2026-10-18.md) — включены в PG-01G. Current loader, named checkpoint schema1/train8/patterns15 и пустой admission ledger дополнительно прочитаны напрямую. **Для этого shipped dataset U-N3 не достигается до admission.**
- [N40:corpus identity/intrabatch](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md) — другой resume/lesson-admission механизм, не DLT-1.
- [N41-T3:topic_domain теряется у Translator](41-TRANSLATION-TOPICDOMAIN-INPUT-ERASURE-AND-MAP-COLLISION-2026-10-18.md) — общая семья потери контекста уже известна; U-N1 относится к иному generated-capability caller/selector.
- [N41:LoRA parity](41-LORA-BRANCH-PARITY-AND-REAL-NEURAL-QUALITY-BOUNDARY-2026-10-18.md) — совпадающее контрдоказательство против отката старых внешних адаптеров; в новую дельту не включается.
- [Параллельный пакет на004c4a79](2026-10-18-preparetogpl-recovery/05-sources-novelty-review.md) — ссылки на manifest evidence, raw/derived lesson IDs и legacy base-import intent. Это дополнительные результаты других исполнителей, с которыми проведена последняя дедупликация.

## 4. Положительные доказательства и исправления, которые сохраняются

**Измерения других исполнителей, прочитанные в этом аудите:**

| Источник | Что доказано в его ограниченной области | Что не следует из результата |
|---|---|---|
| [P125 report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) | 8 memory lessons, ordinary Flow, 0/24→19/24; wrong-label control13/24 | Нейронный DPO, широкая переводческая компетенция, правильность всех принятых ответов |
| [P25 report и review](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md) | Реальный новый процесс, современный checkpoint, все 24 поведения сохранены, качество19/24 | Совместимость любого старого schema1 asset; проверка старого четырёхнаправленного checkpoint |
| [Ingress report](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md) | Та же Domain DLL и уроки, доставка mappings:11/24→19/24; восемь новых сочетаний восстановлены | Проверка полного CLI registry; улучшение всех направлений; отсутствие новых ошибочно принятых ответов |
| [Neural eight-lesson review](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/independent-neural-eight-lesson-review/review.md) | Локальные repairs/обновления и сохранённые нулевые результаты коротких панелей | Невозможность нейронного обучения вообще; активное bias обучение в C50, где bias выключен |
| E07/E09 primary JSON на PrepareToGPL | Сохранены отрицательный E07 и положительный E09, конкретные алгоритмы и неравные панели прочитаны | Строгое парное доказательство переноса одной и той же neural matrix функции в KNN |

[Core25 historical neural donor](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/25-historical-neural-learning-donor.md) используется как указатель на отдельную цепь изменённых checkpoints/head и preference receipts. Собственное повторное чтение его binary tensors этим аудитом не выполнено. Число49/50 в нём относится к reference-relative предпочтению, а не49 правильным собственным ответам. Это не основание объявить всю старую нейронную ветку неуспешной или автоматически назвать её donor текущего RP01.

В current отдельно сохранены исправленные live LoRA, правильный знак log-prob gradient, forward-graph reuse, exact response mask/sum, fractional masks, Embedding и Linear bias gradient. [Сопоставленные строки](PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md). Старый RmsNorm/LoRA/backprop дефект не становится полезной архитектурой из-за нахождения успешного отчёта рядом.

## 5. Что выполнено именно сейчас

| Работа | Фактический результат |
|---|---|
| Source-contract preflight checkpoint | [Скрипт](PREPARETOGPL-CHECKPOINT-PREFLIGHT-2026-10-18.py), [JSON](PREPARETOGPL-CHECKPOINT-PREFLIGHT-RESULT-2026-10-18.json): exact API blob match3/3; old missing0, current missing2; C# не исполнялся |
| FP32 accumulation proxy | [Скрипт](PREPARETOGPL-DPO-FP32-PROXY-2026-10-18.py), [JSON](PREPARETOGPL-DPO-FP32-PROXY-RESULT-2026-10-18.json): N64/128/256 теряют выбранный float-input delta; synthetic logP, не модельные logits |
| Code/plan review | Три предметных исследователя и основной интегратор; перекрёстная проверка получателей, математических ограничений, прошлых receipts, novelty и one-factor design |
| Новый native training/inference/Windows/CI | **Не запускались** |
| Внешний Chinese teacher / отдельный CODEX quality judge | **Не вызывались**; в протоколах указаны будущие роли, не фиктивные аттестации |
| Core и защищённый каталог | **Read-only**; не являются адресатом публикации |

JSON preflight содержит локальные пути временных прочитанных копий как provenance наблюдения. Они не являются дополнительными файлами планового репозитория; воспроизведение берёт исходники из закреплённых GitHub SHA и передаёт их через аргументы скрипта. FP32 proxy использует только стандартную библиотеку Python. Оба скрипта выводят JSON и не меняют источники.

## 6. Применённые skills и независимые исправления

Использован указанный пользователем набор из [docs/skills на current SHA](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills):

| Задача навыка | Применённый источник | Что изменил в результате |
|---|---|---|
| Архитектура | human-readable-architecture-audit; trainable-architecture-decision | Обучаемое состояние, его владелец, consumer и измерение ответа показаны отдельно; deterministic calculation не объявлен дефектом |
| Планирование | intent-to-plan; planning-dpo-authoring | Restore-first, конкретный материал урока, роли автора и оценщика, маленькие доменные arms |
| Проверка плана | plan-review, применимые разделы | Предусловия, независимые controls, недостижимые outputs и скрытые вторые факторы; полный247KB файл не заявляется прочитанным целиком |
| Проверка кода | generated-code-review | Сначала actual caller и before/after source; current fixes и guards сохраняются |
| Prompt engineering | prompt-patch-authoring | Точный draft рабочего поручения, раздельные human choice / Chinese author / CODEX judge, без подмены неподтверждённого provider |
| Человеческая коммуникация | human-communication-GLOSSARY | Смысл ошибки и практический опыт объясняются рядом с именем метода; домен отделён от LoRA/KNN/Flow |

Прочитаны также применимые AGENTS исходной planning-сессии и replanning. Для большого plan-review использованы относящиеся к задаче разделы; полнота непрочитанных разделов не утверждается.

**Замечания, исправленные до публикации:**

1. Current BuildChosen одинаков внутри subphase. U-N1/U-N2 и PG-06/07 теперь измеряют raw scope agreement; перестановка одинаковых labels удалена. Обучение разным законам внутри scope оставлено отдельным условным contract.
2. FP32 первый A/B теперь меняет только totals одинаковых float logP; double LSE вынесен отдельно, precision сохраняется через carriers до margin.
3. RNG quality A/B использует конкретные initial states из zero-step, учитывает изменение base и LoRA-A; отдельный base-only вывод требует одинаковых adapters.
4. Bias component proof отделён от live C50, где bias выключен.
5. Из PG-03 удалено обещание восстановить learned fusion без её конкретного donor/caller/arm. Общая fusion тема остаётся ссылкой в карте старых исследований.
6. Современный успешный cold restore отделён от старого topology отказа; маленький tracked fixture не называется богатой пользовательской моделью.
7. После свежего чтения N40–N42 topology4→6 переклассифицирована в независимое подтверждение N41-T1; пять оставшихся конкретных механизмов повторно проверены профильными исследователями. Для U-N3 добавлен фактический admission blocker shipped dataset.

Материальные замечания профильных проверок устранены. На итоговом пакете проверены **110 уникальных диапазонов exact-SHA source**, UTF-8/JSON и существование новых относительных ссылок; ошибок и непрочитанных источников для этих диапазонов нет. Старое содержимое свежего README сохранено целиком как неизменённый суффикс, добавлен только верхний раздел. Подготовленный набор —11 новых файлов и README; публикация ограничена целевым planning possibleDifs.

## 7. Что остаётся установить исполнением

Главное незакрытое обстоятельство — идентичность **той** хорошей PrepareToGPL модели и обычного запроса, на которых пользователь наблюдал успех. Ветка и первичные положительные результаты найдены; это ещё не один универсальный checkpoint для всех доменов.

Дальнейшая проверка идёт по готовым PG-карточкам: сначала legacy load и сохранность прежнего состояния, затем реально работающий recipient и полезные признаки, потом новые scope/granularity/numeric гипотезы. Если PG precondition не выполняется, это сужает причину вместо создания нового произвольного эксперимента. Широкое утверждение «все причины ухудшения окончательно доказаны» этому результату не соответствует.
