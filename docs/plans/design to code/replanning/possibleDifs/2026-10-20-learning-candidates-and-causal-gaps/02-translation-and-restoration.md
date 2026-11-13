# Успешное обучение памяти, доставка subunits и сохранность результата

## Вывод и статус кандидатов

**Переводческая память действительно научилась на восьми уроках; отдельное исправление транспорта частей урока улучшило результат на тех же данных.** Это реальные learning paths без neural weight updates. Нулевое число нейронных обновлений не превращает изменение Executor/KNN memory в отсутствие обучения.

Три сохранённых результата отвечают на разные вопросы:

| Результат | Вопрос | Статус в этом пакете |
|---|---|---|
| P125-enru-memory-01 | Приобретает ли свежая машина способность через native Train, читается ли она обычным Flow? | Успех существующего обучения памяти |
| translation-memory-ingress-01 | Доходят ли предусмотренные части урока через настоящий corpus reader/builder до обучаемого Executor? | Успешный кандидат **уже применён**; новое разрешение/повторная правка не требуются |
| P25-enru-cold-01 | Сохраняются ли приобретённая память и ответы после native load в новом процессе? | Успех сохранения результата обучения, без нового accuracy gain |

Этот документ заново читает сохранённые отчёты и текущий код. Он не является новым прогоном и не меняет исходные результаты. Любое измеренное улучшение полезного показателя считается успехом этого показателя и домена; остальные измеренные дельты, включая ухудшения, остаются рядом.

## 1. P125: собственная обучаемая память

В сохранённом сравнении fresh untrained A даёт **0/24**, B после **8 native lessons** — **19/24**, контроль C с переставленными метками — **13/24**. Путь проходит через `TranslationMachine.TrainStructuredBatchAsync` в память Executor и общий KNN, затем через `GetOrchestratorAsync().RunAsync`. Это приобретение и чтение изменённого learned state; не подстановка готового ответа scorer в Student. [Первичный P125 report][p125]; [независимая проверка][p125-review]

Панель различает восемь точных уроков, три близкие формулировки, восемь новых сочетаний знакомых action/object и пять краевых случаев. После B правильны **8/8, 3/3, 8/8 и 0/5** соответственно. Приняты22/24 ответа, строго правильны19/22 принятых. Эта проверка не означает владение произвольным EN→RU, новой лексикой, другими языками или нейронным RP01 adapter.

Контроль с неправильными метками важен: состояние и поведение зависят от материала обучения, а не только от факта вызова Train. При этом 13/24 в C не доказывает универсальную чувствительность ко всем перестановкам и не заменяет отдельную проверку каждой ветви получения ответа.

Оригинальный `accuracyAfter=0` нельзя читать как «не научилось»: в этом контексте receipt использовал `MeasureUnitCoverage` с SemanticTokenizer/ExecutorRuEn/QualityVerifier, а наблюдаемая панель относится к ExecutorEnRu. Поле coverage и предметная accuracy19/24 — разные измерения. [P125 report][p125]; [cold report с разбором поля][cold]

## 2. Потерянная доставка phrase mappings: подтверждённое влияние

### Причина в исходном коде

В обеих версиях входной корпус уже содержал **16 соответствий частей**. Старый `ReadTranslationCorpusRowsAsync` их не переносил в row, а `BuildTranslationContinueExample` создавал `ExecutorOutput` без них. В результате после восьми уроков были восемь точных записей и ноль обученных частей. Исправленный transport передал optional `phrase_mappings`: восемь точных записей плюс16 частей. Сам Domain DLL, Executor search/Flow/verifier/confidence/пороги в этом сравнении не менялись. [Ingress report][ingress]

Текущий адрес уже внесённой правки:

- `27.TranslationDomain.Tools/Program.cs`, `ReadTranslationCorpusRowsAsync`: чтение поля в row. [Строки4442–4468][ingress-reader]
- `BuildTranslationContinueExample`: передача `row.PhraseMappings` четвёртым аргументом `ExecutorOutput`. [Строки4682–4689][ingress-builder]
- `TranslationContinueCorpusRow`: optional member с `JsonPropertyName("phrase_mappings")`. [Строки5140–5147][ingress-row]

Это точная связь **данные → материализация → Train → память → обычный Flow**. Проверка не является посторонним тестом JSON и не требует превращать её в новый neural update. Исполнялись настоящие private reader/builder через reflection; полный CLI с registry/manifest не запускался. Отдельный `ResearchCheckpointEnRuAdapter` RP01 этим исправлением не изменён.

### Сопоставимые результаты

| Показатель после одинаковых восьми уроков | Старый transport A | Исправленный transport B | Дельта |
|---|---:|---:|---:|
| Exact recall | 8/8 | 8/8 | 0 |
| Близкая формулировка | 3/3 | 3/3 | 0 |
| Новые композиции знакомых частей | 0/8 | 8/8 | **+8 правильных** |
| Краевые случаи | 0/5 | 0/5 | 0 |
| Правильные / все | 11/24 =45.83% | 19/24 =79.17% | **+33.33 п.п.** |
| Принятые / все | 12/24 =50% | 22/24 =91.67% | +41.67 п.п. |
| Правильные / принятые | 11/12 =91.67% | 19/22 =86.36% | **−5.30 п.п.** |
| Принятые strict failures | 1 | 3 | **+2 ошибки** |
| Exact memory entries | 8 | 8 | 0 |
| Phrase memory entries | 0 | 16 | +16 |
| Shared KNN entries | 24 | 24 | 0 |

Все числители, исходные ответы и frozen оценщик доступны в [первичном report][ingress]. Начальные canonical states совпали при исключении только ExportedAtUtc; уроки, их порядок и gold сохранены. Для строк без optional поля материализация проверена без дополнительных уроков и совпала побайтно в исходном runtime validation.

Ухудшения имеют предметный смысл. `do not delete the file` стало принятым `удали файл` — потеря отрицания. `did you delete the file` стало принятым `удали файл` — вопрос о прошлом превратился в команду. `validate the file and the cache` по-прежнему не проходит strict gold; вариант `проверь файл кэш` сохраняет оба объекта, но неоднозначен. Не нужно превращать эти наблюдения в условие, отменяющее уже измеренный успех composition. Нужно сохранить их рядом с ним.

### Затраты не скрыты

| Наблюдаемая величина | A | B | Граница интерпретации |
|---|---:|---:|---|
| Один native batch8, ms | 38.5685 | 34.5068 | Локальная меньшая стоимость; один A/B, JIT/order не уравнены, устойчивый speedup не доказан |
| Сумма24 ответов, ms | 291.0535 | 52.9254 | Не чистый benchmark одной операции; состав выбранных routes изменился |
| Process peak working set, bytes | 56 909 824 | 55 107 584 | Отдельный process metric |
| Максимум наблюдённой суммы RAM process+descendants, bytes | 66 539 520 | 82 214 912 | **Вырос**, sampler всего5/2 snapshots; не полный пик дерева |
| Saved learned state, bytes | 7 515 | 8 954 | **+1 439 bytes** — цена добавленных частей |

Таблица сохраняет локальные измеренные изменения и не обещает перенос стоимости на другие машины. Неизмеренные whole-PC RAM, авторские tokens/$ и all-language quality не подставляются как нули. [Полные costs/commands/provenance][ingress]

### Кандидат уже внедрён

`required-changes.md` фиксирует решение пользователя «Вноси именно так как есть» и правку транспорта. Текущий pinned source действительно содержит чтение и передачу поля. Следовательно, повторная правка или новое подтверждение уже принятого действия здесь не нужны. Это статус **SUCCESS_RECORDED / SOURCE_ALREADY_PRESENT**, а не «ещё исправить». [Решение и статус][required]

## 3. P25: результат обучения сохранился без новых уроков

Сравнивались сохранённые B_after observations P125 и новый OS process после `TranslationMachine.LoadCheckpointAsync` из копии прежнего обученного checkpoint. Выполнены load1 и24 ordinary Flow calls; **Train0, lessons0, neural updates0**. Это валидная проверка сохранности приобретённого состояния, непосредственно относящаяся к результату обучения. Новый Train только ради формального слова «обучение» исказил бы вопрос. [Cold report][cold]; [независимый review][cold-review]

Результат:

- **24/24** output/route/confidence/verdict совпали с retained observations; строгая правильность **19/24→19/24**, coverage22/24.
- Canonical state source, loaded и after-inference совпал: `8b03a483d5d32385fb22efdd164f633efc2ae92ca88813ac1f50a88f0c573305`. Исключён только ExportedAtUtc; не исключались массивы, веса памяти, version, readiness, identities и history.
- Сохранились exact8, phrases16, shared KNN24, identities8/history1, version0.1.1. Исторические counts не переименованы в новые уроки.
- Сохранились и три прежних strict failures, включая две явные смысловые ошибки. Restore не исправил качество и не заявил нового улучшения.

Native load занял97.9842ms и allocated1 021 096bytes. Это измерение загрузки без сопоставимого speed baseline, не доказательство ускорения. `Cold` здесь означает новый процесс/владелец памяти: OS disk cache не сбрасывался.

Current importer не предоставляет tokenizer/corpus content-hash rejection contract. Поэтому добавить произвольное поле с неправильным hash и ожидать отказ было бы выдуманным тестом несуществующей функции. Внешний integrity guard самого опыта не приписан продукту. [Ограничения P25][cold]

Этот успех не закрывает старую четырёхнаправленную schema1, LoRA/optimizer resume, другие snapshots, все языки, restart ОС или current OrchestrationReasoning admission. Конкретные несовпадения старых активов и current readers перечислены в [карте соседних веток](01-axes-and-neighbor-evidence.md).

## 4. Правило для отчёта и будущего исполнителя

Текущая задача — документация. Результаты P125, ingress и P25 уже есть; их нельзя заново запускать только потому, что старый общий отчёт называл отсутствие neural updates «не обучением» или требовал accuracy gain для restore. Перед любой будущей новой проверкой должен быть указан иной, ещё не отвеченный вопрос.

**Промпт для документационного исполнителя:** «Сохрани эти три результата с их собственными получателями, панелями и исходными receipts. Любое измеренное улучшение полезного показателя — успех этой метрики и предметного домена; рост точности не обязателен. Покажи все ухудшения и неизмеренные величины. Не запускай обучение/генерации и не меняй production. Ingress уже внесён; P25 подтверждает сохранение конкретного learned state. Не распространяй эти результаты на отдельный neural RP01 adapter или другие домены без свидетельств».

[p125]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[p125-review]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/independent-review/review.md
[ingress]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[ingress-reader]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/27.TranslationDomain.Tools/Program.cs#L4442-L4468
[ingress-builder]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/27.TranslationDomain.Tools/Program.cs#L4682-L4689
[ingress-row]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/27.TranslationDomain.Tools/Program.cs#L5140-L5147
[required]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/required-changes.md#L7-L19
[cold]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[cold-review]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/independent-review/review.md
