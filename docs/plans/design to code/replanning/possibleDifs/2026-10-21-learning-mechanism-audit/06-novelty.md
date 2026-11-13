# Новизна после согласования с последним чужим коммитом

**Финальный вывод: одна новая самостоятельная гипотеза — POS-QUERY-SET-ONCE-01.** Наш первоначальный SiLU-DENOM совпал с уже опубликованным другим агентом SILU-SIGMOID-REUSE-01 и переведён в review alias. Новая конкретная привязка framing к N63C-PROBE сохраняется как evidence известной P07-семьи, без нового execution ID.

Исходный адресный поиск выполнен по 883 primary Markdown possibleDifs@fc0dd и 335 protected текстам core@d962. Перед публикацией получены все 26 добавленных Markdown planning@8346, прочитаны ближайшие новые карточки и добавлена адресная core256 проверка. Это поиск и смысловой разбор конкретных механизмов, не утверждение о полном перечтении каждого документа или мировой уникальности. [Исходный и финальный receipt](data/novelty-search.json).

## 1. POS-QUERY-SET-ONCE-01 — новая конкретная cost-гипотеза

В native `12.SemanticCompiler/Translation/PatternClassifier.cs::Predict/SequenceDistance` query bigrams одного запроса повторно строятся для каждого обученного соседа. При Train128 и панели 32 это 4096 query sets; один набор на Predict оставил бы 32. Это арифметика source, не измеренная экономия.

Предлагается один фактор: подготовить query HashSet после empty-memory fallback, передать в distance; не менять training-row sets, Jaccard, union0, k3, stable ties, голосование, labels или Train. [Карточка](cards/POS-QUERY-SET-ONCE-01.md) задаёт native160/128/32 fixture, 1GenerateExamples,2Train/256 предъявлений/128Predict, exact source, короткийhost, единыйowner, stageclaims, команды, finitebudgets и русский report.

| Ближайший известный материал | Отличие |
|---|---|
| N50-POS-02 / POS2 | Меняет различимость singletons/пустых множеств. Новый фактор сохраняет тот же математический результат |
| Set/multiset и глобальный порядок | Меняет признаки/меру. Новый фактор не возвращает потерянную семантику порядка |
| E25 learned classifier вместо проекта 29 | Меняет consumer wiring. Новый опыт не переключает fusion |
| AST-SIB-CACHE | Подготовка AST во время TrainFromSource, другой learner и состояние |
| Новая HNSW-QUERY-NORM-REUSE-01 | Повтор query cosine norm внутри Add/поиска в HNSW. Здесь POS HashSet внутри trained Predict, без HNSW и его обучения |

Полезный эффект пока не измерен. Участие допустимо через чтение реально выученной памяти, а не private-distance benchmark; приоритет последний. Точный фактор не найден ни в исходном корпусе, ни в 26 новых planning Markdown. Это ограничение новизны доступными источниками, не знание неопубликованных действий всех агентов.

## 2. SiLU объединён, без второго опыта

[SILU-SIGMOID-REUSE-01](../2026-10-20-learning-causal-audit-fb9175cc/cards/SILU-SIGMOID-REUSE-01.md) уже опубликован в 8346: тот же denominator между forward/backward, исходные деления сохранены. Название SIGMOID не означает другой алгоритм. Наш [SILU-DENOM review](cards/SILU-DENOM-REUSE-01.md) сохраняет проверку и guard `RequiresGrad≠training`, но **не создаёт второй ID или SD01**.

Первоначальное отсутствие известного H512/full before не следует расширять до «невозможен любой SiLU опыт». Теперь дополнительно прочитан native H64 LEARN-SANITY-PROBE:400updates дают 1/2 полных TRAIN и 1/2 EXAM ответов. Это минимальный ненулевой signal, но constant-label baseline, не усвоение условного знака. Canonical карточка задаёт bounded400 preparation+4+4updates; host остаётся AUTHOR_SETUP_REQUIRED. Отсутствующий checkpoint не выдаётся за полученные веса.

## 3. N63C: новые свидетельства, известные семейства

На d962 конкретная late-error ternary `error is null ? 1 : 0` скрыла выполненные DPO1+SFT1. Старый filename mismatch уже был сообщён пользователем. До нашей публикации другой пакет также разобрал их, а core256 внёс документные поправки. Поэтому эти пункты — **согласованные подтверждения**, не новые уникальные гипотезы нашего финального пакета.

Новый completed N63C-PROBE — чужой сохранённый положительный результат; его candidate уже есть в required-changes. Мы сохраняем gain, регрессии и source/result mismatch. Его авторство не присваивается.

[Фрейминг N63C](09-n63c-framing.md) даёт конкретную same-model цепочку, которой прежде требовал P07 crosswalk: SFT Prompt+LF+Response → реальные updates → exact-DPO carrier безLF в оценке. Новизна здесь — доказанная привязка к этому probe и оговорка об интерпретации 22/23; общий механизм давно известен. Влияние на обучение и точная ошибочная позиция не измерены; отдельный experimentID ради переименования не добавлен.

## 4. Отклонённые дубликаты

| Идея | Уже существующее покрытие |
|---|---|
| Взвешенные повторяющиеся BPEchunks | BPE-WEIGHTED-CHUNKS-01 |
| HNSW reverse-prune / потерянная достижимость | N28-H1/P20; не новая RP01-причина |
| SourceLang отсутствует в ключе tokenizer corrections | N26-LANG |
| Пустые POS bigrams / потеря кратности | N50-POS-02, P51; не включать в POS-query cost |
| Замена learned E25 правилами 29 | P43/17-positive-pos, родительские планы; подтверждение в 01 |
| SFT scheduler warmup0 / cosine послеTotalSteps | N33/LT-N06 |
| Sparse AdamW global parameter age | Прежний sparse-learning audit |
| Response-mask, SFT/DPO order и moments | P07/P87/E-SFT/E-DPO семьи; не присваивать новое имя |
| Embedding/bias/RMS/SiLU backward отсутствует | В current исправлено; старый broken graph не возвращать |

## 5. Что действительно покрыто protected экспериментами

| Деталь | Состояние на core256 | Наше действие |
|---|---|---|
| N63C filename/counters/consumed | Исправлено в документации; raw и старый runtime сохранены | Отметить закрытие документного конфликта, не повторять entry |
| Пригодный before и достаточный бюджет | Protocol30 требует;15 карточек получили requires-validation, но budget в теле прежний | Не аттестовать всё готовым; авторская квалификация до расхода |
| N63C positive partial probe | Raw и candidate опубликованы | Сохранить 1-token/NLL/margin gains и все регрессии; whole/EXAM цели открыты |
| N63C same-model framing | ОбщаяP07 семья известна; exact carrier выбран, SFTLF сохранён | Добавить source-traced конкретизацию 09; не утверждать, что framing A/B уже выполнен |
| SiLU reuse | В protected отдельного рецепта нет; canonical уже в planning8346 | Alias к canonical; бездубликата |
| POS query-set reuse | Отдельного рецепта в проверенном protected/planning не найдено | Новый nativeTrain→Predict паспорт+prompt, последний приоритет |

Требование «все детали уже правильно оформлены в защищённых экспериментах» нельзя отметить PASS. Папка остаётся read-only по прямому заданию; в possibleDifs сохранены точные разрывы и предложения. Все старые эксперименты и результаты остаются.
