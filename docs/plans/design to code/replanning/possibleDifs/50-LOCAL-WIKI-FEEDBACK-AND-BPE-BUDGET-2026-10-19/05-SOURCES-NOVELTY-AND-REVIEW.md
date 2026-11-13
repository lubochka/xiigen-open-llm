# 05. Источники, проверка новизны и границы рецензии

**Статус:** готовы доказательные карточки двух механизмов — LW-FB-01 и LW-BPE-02, с малыми предметными экспериментами. Сравнение исходников и исполненные вычислительные проверки подтверждают описанные механизмы в своих границах. Новое native обучение и восстановление качества RP01 в этом пакете не заявляются.

## 1. Закреплённые источники

| Срез | Репозиторий / SHA | Область |
|---|---|---|
| Рабочая ветка пользователя, donor | `llm_mvp_core@219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` | `local_wiki_planning`: исходники, registry и выбранные исторические DPO receipts |
| Текущая реализация | `llm_mvp_core@4945b8e85c17b5f150eb5b840d16bbdc4b506242` | `codex/dpo-implementation`: проверенные вызывающие пути и состояния |
| Существующий каталог planning | `xiigen-planning@4ca770a2c7354d7985f34c516f087e4b49cd01df` | `llm mvp/design to code session/replanning/possibleDifs` |
| Существующий защищённый каталог | `llm_mvp_core@4945b8e85c17b5f150eb5b840d16bbdc4b506242` | `docs/plans/replanning-possible-diffs`, включая фактически названный `experimets` |

Точные пути и Git blob SHA представлены в [catalog-inventory.json](evidence/catalog-inventory.json). Источники кода новых механизмов: [donor selector](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs#L60-L170), [current selector](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs#L63-L115), [donor tokenizer factory](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L862-L869), [current tokenizer factory](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L4054-L4061). Подробные цепочки caller/state/consumer находятся в карточках01/02.

При подготовке материалов защищённый каталог использовался только для чтения. В этом документе нет утверждения, что SHA одной ветки подтверждает поведение любой последующей ветки.

## 2. Что действительно покрыла проверка новизны

Метаданные двух каталогов содержат **1577 Git blobs**: 499 planning и 1078 core. Полный лексический поиск выполнен по **630 Markdown-файлам, 12 413 387 байтам**: 361 planning и 269 core; последние включают 92 корневых документа и 177 документов вложенного `experimets`. В корпус входят все карточки P01–P125 и сохранённые Markdown-отчёты опытов. Каждый из этих 630 текстов совпадает со своим закреплённым Git blob SHA. [Инвентарь](evidence/catalog-inventory.json); [поисковые запросы и совпадения](evidence/novelty-receipts.json).

Отдельно получены и сверены с Git 32 верхнеуровневых Markdown RP01, 11 родительских/RAG носителей и два skill-файла: всего 675 точных текстовых копий. Инвентарь содержит 1624 записи метаданных вместе с дополнительными источниками; это число не означает 1624 прочитанных документа. Не все не-Markdown файлы были прочитаны, и все подкаталоги RP01 вне обозначенной области не аттестованы.

Выполнен полный поиск точных идентификаторов и смысловых сочетаний, затем адресно прочитаны реестры, совпадения и ближайшие существующие темы. **Построчное смысловое прочтение всех 12,4 МБ не заявляется.** Отсутствие совпадения — ограниченный результат по двум pinned каталогам; оно не доказывает глобальную уникальность во всей истории репозиториев, чужих переписках или более поздних публикациях.

### Отдельная проверка новых публикаций planning перед выпуском

GitHub compare `4ca770a2… → 05ba7dfeec56ac054b17c8d22806f2b649ad3247` показал пять новых commits и ровно пять добавленных Markdown в `possibleDifs`; старые файлы в этой дельте не изменены. Все пять получены, полностью прочитаны и повторно проверены теми же 19 точными/семантическими запросами. Их **74 411 байт** совпали со всеми пятью Git blob SHA. Это отдельная дельта к **630 базовым Markdown**, а не подмена SHA всего исходного корпуса. [Полный freshness receipt](evidence/freshness-planning.json).

| Добавленный источник на `05ba7dfe…` | Проверенное пересечение |
|---|---|
| [Positive restore](https://github.com/lubochka/xiigen-planning/blob/05ba7dfeec56ac054b17c8d22806f2b649ad3247/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/00-EXTERNAL-DOMAINS-POSITIVE-RESTORE-P0-2026-10-19.md) | E09/E26/E43 и качество внешнего CODE_TO_DOC корпуса; другой механизм |
| [Success restoration](https://github.com/lubochka/xiigen-planning/blob/05ba7dfeec56ac054b17c8d22806f2b649ad3247/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md) | E25/POS, domain learner и consumer; не accepted/mastery selector и не BPE seed |
| [Causal experiments](https://github.com/lubochka/xiigen-planning/blob/05ba7dfeec56ac054b17c8d22806f2b649ad3247/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-EXTERNAL-DOMAINS-CAUSAL-EXPERIMENTS-2026-10-19.md) | DPO-контраст, template diversity, AST/POS/replay; иной причинный фактор |
| [E25 POS seams](https://github.com/lubochka/xiigen-planning/blob/05ba7dfeec56ac054b17c8d22806f2b649ad3247/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-EXTERNAL-DOMAINS-E25-NOVEL-POS-SEAMS-2026-10-19.md) | Ordinal enum migration, пустые POS bigrams, очистка памяти; enum ordinal не равен давности BatchId |
| [Pair identity and inverted preferences](https://github.com/lubochka/xiigen-planning/blob/05ba7dfeec56ac054b17c8d22806f2b649ad3247/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md) | Один общий поисковый hit `curriculum_tier/rejected_type` относится к генератору DPO; consumer иной |

Точных совпадений двух новых механизмов в этой дельте не найдено; семантическое чтение пяти документов дублирование также не выявило. Содержательные утверждения этих новых глав здесь не переаттестовываются как доказанные эксперименты.

### Отдельная проверка изменений защищённого core-каталога

Донорский трек получил и просмотрел семь указанных изменённых Markdown на core `649b9343cfda5b81cc54396a3840ad5db5e0b179`: `02-programming`, `03-translation`, `06-findings-map`, `08-sources-and-study-log`, `09-additions-44-48`, `10-additions-language-restore-49` и README в `experimets/2026-10-19-small-learning-probes`. Полные пути, Git blob SHA, поисковые запросы и семантическое сопоставление сохранены в [freshness-core.json](evidence/freshness-core.json). Перечень дельты предоставлен сборщиком; этот трек не выдаёт его за собственное повторное сравнение всего source code.

Точных идентификаторов обоих механизмов в семи текстах нет. Ближайшие темы — перенос parent policy, обучение lexical Router и его ordinary consumer, LR scheduler, известный prefix64 и tokenizer identity. Они не описывают accepted-aware выбор следующего Unit или расход BPE merges искусственными 512 номерами. Единственное совпадение числового запроса `256` оказалось частью слова `SHA256`, что сохранено в receipt. Совпадений причинного механизма не найдено.

**Итоговая граница:** 630 базовых Markdown на исходных refs, плюс отдельно пять новых planning Markdown и семь повторно просмотренных изменённых core Markdown. Это не 642 гарантированно разных новых документа и не перебазирование исходных экспериментальных receipts. Сравнение donor/current исходников по-прежнему закреплено на `219d7ba… / 4945b8e…`.

### Последняя отдельная дельта planning до `4151916f…`

Перед публикацией проверен compare `05ba7dfe… → 4151916fac86becea8b7c2fd8cb6e73abb49cd79`: семь commits, шесть файлов. Полностью прочитаны два новых Markdown (малые E25/E26/E09/E43 опыты и N51 о сохранённом коде/preloaded E26), новый `50-EXTERNAL-DOMAINS-CORPUS-PROBE.py` и полные patches трёх изменённых Markdown (30 добавленных строк). Все три новых файла сверены с Git blob SHA; Python-файл только прочитан. Повторены те же 19 запросов и смысловое сопоставление: accepted-aware выбор следующего Unit и искусственные numeric seeds в BPE не описаны. E25 ordinal label mapping остаётся иной темой, чем BatchId-recency контроль. [Точные источники, patches и результаты](evidence/freshness-planning-final.json).

Это дополнение сохраняет прежнюю границу **630 базовых текстов + отдельные пять planning additions и семь core rereads**. Новые файлы и patches не объявлены повторным чтением всего каталога или новым обучением. Финальная проверка новизны planning ограничена `4151916fac86becea8b7c2fd8cb6e73abb49cd79`; дальнейшее движение веток в этот пакет не включено.

## 3. Две новые гипотезы и ближайшие прежние темы

| Карточка | Проверенные точные идентификаторы | Ближайшая существующая область | Почему новая карточка не повторяет её |
|---|---|---|---|
| **LW-FB-01** | `SelectNextTrainingAction`, `ActionableWeakSignalScore`, `SelectRegistryWeakUnit`, `recentRejected`: 0 совпадений в 630 Markdown | P48 — targets/order/curriculum E44/E45; прежний 49 donor delta — publication/recipient; ссылка на NEXT-TRAINING-ACTION | Проверяется удаление accepted/mastery и ограниченного окна истории из конкретной формулы следующего Unit, при одном и том же registry |
| **LW-BPE-02** | `semantic_seed_`, `stableMergeSeed`, `BuildTokenizer`: 0 совпадений | P17 — re-fit/order/identity; P98 — capacity; N23 — prefix64 | Причина — именно 512 искусственных номеров занимают часть 256 merge-операций при создании generated capability |

Для обеих областей дополнительно проверены семантические запросы: искусственный/synthetic padding с BPE/merge; accepted/mastery и curriculum/priority; история слабых/отклонённых попыток. Ненулевые общие совпадения сохранены и рассмотрены в [novelty-receipts.json](evidence/novelty-receipts.json); пустой поиск имени метода сам по себе не служил единственным критерием.

**Контроль внутри LW-FB-01:** donor берёт восемь лексикографически наибольших `BatchId`. Это не гарантирует временную давность. Смена имени при неизменных исходах и времени может менять выбранное окно. Точные и смысловые запросы по этой связи не нашли прежнего описания в corpus; ближайшие P48.2 и G-W07 касаются других consumers. Этот контроль нужен, чтобы не объявить старый filename shortcut обязательным правильным алгоритмом. **Третьей самостоятельной гипотезы и отдельного нового ID здесь нет.**

Проверка новизны не доказывает, что новый механизм объясняет RP01 `0/3` или `0/4`, и не доказывает улучшение после исправления. LW-BPE-02 унаследован от donor и не объясняет готовый RP01 tokenizer629. Прямое возвращение donor selector также требует контроля качества accepted evidence и временной семантики.

## 4. Что сознательно не выдано за новые находки

При сверке сняты, в частности: E09 `kindMap` по отдельному файлу, E09 разные знаменатели и переходы между файлами, E13 различный cutoff methods/pairs, `maxLexical`-нормировка от набора кандидатов, FP32 потеря точности суммы logP, нечётный RoPE headDim, динамика LoRA A при B=0, AdamW second-moment overflow, reset policy/reference и потери при проекции DPO в Unit. Они уже представлены в двух каталогах.

Полная механическая карта доступна в [03](03-MECHANISM-COVERAGE-AND-KEPT-FIXES.md), а точный индекс 125 карточек — в [prior-card-index.md](evidence/prior-card-index.md) и [машиночитаемом варианте](evidence/prior-card-index.json). Эти ссылки сохраняют принадлежность прежних находок; они не являются новой проверкой всех их нынешних статусов.

## 5. Проверка карточек другим исследовательским треком

Сохранена [независимая содержательная рецензия](evidence/independent-review.md). В ней повторно просмотрены обе реализации selector, validators и caller/consumer. Python-перенос селектора независимо воспроизведён; compact registry projections сверены со всеми исходными entries в исходном порядке. Проба BPE просмотрена против конкретных C# merge/splitter операций и повторно исполнена с полным совпадением результирующего JSON. Рецензия отдельно пересчитала четыре исторических pair-diagnostics набора; архивное измерение осталось архивным свидетельством.

| Проверено сейчас | Чего это не доказывает |
|---|---|
| Разные рекомендации двух правил на одном нынешнем registry; контроль accepted и rejected; отказ несовместимого donor registry в нынешнем validator | Автоматический запуск нового урока, длительную динамику обучения или причинность ухудшения RP01 |
| Разный BPE merge recipe и prefix64 на четырёх фиксированных примерах; сохранён отрицательный результат оркестрации | Паритет с запущенным .NET tokenizer, качество KNN-ответа, DPO или обучения весов |
| Архивные пары, cardinality и диагностика выбранных donor запусков | Новый native запуск, свободную генерализацию, cold restore или успех на всех доменах |
| Точные текущие изменения LoRA/DPO/autograd из03 | Полную корректность всех kernels и всех конфигураций модели |

Рецензия относится к указанным материалам и срезам. Она не является новой оценкой DPO человеком или CODEX: таких пар в этой работе не генерировали и не принимали как train-ready. Исторические данные не переобъявлены соответствующими новому контракту ролей без отдельной проверки provenance.

## 6. Применённые проектные skills и предел чтения

В треке каталога применены следующие запрошенные проектные skills, закреплённые на current SHA:

| Skill | Git blob | Честная область чтения |
|---|---|---|
| [phase-planning-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/phase-planning-SKILL.md) | `27ba212c829148158511c7a5a69bc418abfc276c` | Файл получен целиком; последовательно прочитан по частям, строки 1–1232 |
| [plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/plan-review-SKILL.md) | `b99f4ce280f48d0242a35f2ba421a2dc1996399f` | Полное тело получено; просмотрен весь индекс заголовков и адресно разделы 1–140, 1581–1628, 2291–2370. Полное смысловое чтение 4200+ строк не заявляется |

В независимом нейронном треке применены [code-examination](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/code-examination-SKILL.md), [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/generated-code-review-SKILL.md) и [learning-path-audit](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/learning-path-audit-SKILL.md); область проверки этого трека изложена в приложенной рецензии. Это заявление об областях реально выполненной работы, а не о полном аудите каждого skill-файла каждым участником.

Сборщик пакета сообщил полное чтение [human-communication-GLOSSARY](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/human-communication-GLOSSARY.md), [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/prompt-patch-authoring-SKILL.md), [dpo-triple-format](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/dpo-triple-format-SKILL.md), [learning-signal-capture](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/learning-signal-capture-SKILL.md), [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/human-readable-architecture-audit-SKILL.md) и [conversation-intent-and-self-reflection](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/conversation-intent-and-self-reflection-SKILL.md). Их практическое применение в этом пакете: назвать конечное предметное действие, сохранить exact DPO contrast и роли моделей, отделить сигнал обучения от отчёта, связать состояние с потребителем и объяснить пользователю проверенные результаты без объявления proxy новым обучением. Это сведения об отдельном треке; автор каталога не приписывает себе повторное чтение этих файлов.

Донорский трек прочитал первые 240 строк [architect-behavior-classifier](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/architect-behavior-classifier-SKILL.md) и [trainable-architecture-decision](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/trainable-architecture-decision-SKILL.md), а [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/skills/experiment-design-SKILL.md) — целиком. В карточке01 это выразилось в явном разделении рекомендации `plan_only` и работающего learner, цепочке Unit→Flow→state→consumer, сравнении на одних 421 entries и измеримых контролях. Полное чтение первых двух skills не заявляется.

Применимые требования: привязка claim/review к точному SHA; текущая задача важнее исторических шаблонов; requirement → mechanism → state → consumer → опровергающий опыт; конкретные близкие отрицательные примеры и source-independent вопросы; явная проверка leakage/fallback/confidence; пределы каждого вывода. Специальная [Minimal verification policy](https://github.com/lubochka/xiigen-planning/blob/4ca770a2c7354d7985f34c516f087e4b49cd01df/llm%20mvp/design%20to%20code%20session/replanning/LEARNING-FIRST-MINIMAL-VERIFICATION-POLICY-2026-10-14.md#L9-L56) задаёт для данного source/plan-аудита ноль новых product tests. Старые упоминания Windows matrix, иных моделей и многопроходных шаблонов не превращены в новые требования к этому документационному пакету.

**Роли следующего эксперимента по актуальному поручению:** CODEX с `astra 6 ultracode` выполняет человеческий выбор и независимую оценку качества; внешняя открытая китайская модель только генерирует DPO. Историческое разрешение `ASTRA_SIMULATED_TEACHER` в RP01 не переопределяет эту границу. Низкоуровневые Python fixtures из02 не являются DPO-уроками и не заменяют этот процесс.

## 7. Переносимый evidence и оставшиеся границы

**Поздняя параллельная публикация перед записью.** На planning `2ef186330c032b3aba8476f71e3f21113003d432` появился [пакет51 SQL/KNN/Tensor](https://github.com/lubochka/xiigen-planning/tree/2ef186330c032b3aba8476f71e3f21113003d432/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19). Просмотрены объявленные новые механизмы и предметная глава KNN, выполнен точный поиск идентификаторов двух наших цепочек в добавленном diff; совпадений нет. Его SQL conditioning/curriculum, document-frequency при повторной индексации, AST query-role, skill eligibility и условные tensor shapes относятся к другим операциям и получателям. Они остаются находками своего пакета. Этот поздний контроль не является повторным аудитом всех его первичных источников; исходные630 и прежние freshness receipts сохраняют свои границы. Новый общий README сохранён при публикации целиком.

| Файл | Назначение |
|---|---|
| [catalog-inventory.json](evidence/catalog-inventory.json) | Repo/ref/path/blob SHA, размер, факт проверки exact text copy; 1577 catalog blobs и 47 дополнительных записей метаданных |
| [novelty-receipts.json](evidence/novelty-receipts.json) | Точные и семантические запросы, counts, совпавшие строки с источниками, ручное сопоставление и классификация «две гипотезы + внутренний контроль» |
| [prior-card-index.md](evidence/prior-card-index.md), [JSON](evidence/prior-card-index.json) | P01–P125 с подслучаями, закреплёнными ссылками и blob SHA в JSON |
| [freshness-planning.json](evidence/freshness-planning.json) | Отдельная дельта пяти добавленных Markdown на `05ba7dfe…`: compare metadata, exact blob SHA, повтор запросов, ручное сопоставление |
| [freshness-planning-final.json](evidence/freshness-planning-final.json) | Финальная дельта `05ba7dfe… → 4151916f…`: три полных новых файла и полные patches трёх изменённых Markdown |
| [freshness-core.json](evidence/freshness-core.json) | Отдельное чтение семи изменённых Markdown защищённого каталога на `649b9343…`, сопоставление связанных механизмов без смены исходной source-пары |
| [independent-review.md](evidence/independent-review.md) | Что другой трек действительно перепроверил и чего не запускал |
| [selector-receipt.json](evidence/selector-receipt.json), [seed_probe_results.json](evidence/seed_probe_results.json) | Результаты вычислительных проверок; inputs/scripts связаны из01/02 |

Эти файлы не зависят от абсолютного пути scratch. Полные многомегабайтные исходные registry здесь не дублируются: для selector приложены точные проекции прочитанных полей, а оригиналы определяются закреплёнными path/blob SHA. Git SHA подтверждает тождественность источника, но не достоверность всех содержащихся в нём утверждений.

Основные оставшиеся проверки — native паритет обоих малых механизмов, достижение фактического learner/consumer, независимые предметные ответы при равном бюджете и fresh load. Planning-добавления и последняя дельта проверены отдельно до `4151916fac86becea8b7c2fd8cb6e73abb49cd79`, указанные изменения core-каталога — до `649b9343cfda5b81cc54396a3840ad5db5e0b179`; более поздние публикации не наследуют эту проверку автоматически. При позднем совпадении правильный статус — независимое подтверждение с ссылкой на прежнюю карточку.
