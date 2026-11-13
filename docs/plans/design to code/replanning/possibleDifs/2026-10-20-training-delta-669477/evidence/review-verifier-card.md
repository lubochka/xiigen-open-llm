# Независимая проверка VT-APOSTROPHE-01

Статус: **DESIGN_VALID / RUNTIME_UNVERIFIED** после адресных уточнений ниже. Прочитаны полные карточка `new-verifier-apostrophe-card.md` и отдельный executor prompt, исходный producer, learner, consumer, tokenizer, контракты и metadata. Это read-only проверка документов и source на core `66947785d1318f7cbbc88118f3156cdc75001e16`; сборка, обучение и изменение production не выполнялись. Семь Git blob SHA source closure независимо пересчитаны по фактическим bytes — **7/7 совпадают с карточкой**.

**Вывод:** это маленький эксперимент над настоящим обучением stateful verifier: native producer создаёт урок, native `TrainOnExample` изменяет память, обычный `Verify` читает эту память. Контракт не подменяет обучение parser-only тестом и не приписывает результат полной TranslationMachine/Flow или neural DPO. Успех пока не измерен; карточка корректно помечена NOT_MATERIALIZED / NOT_RUN.

## Подтверждённые участки исходного пути

| Участок | Что фактически делает source | Значение для опыта |
|---|---|---|
| `27.TranslationDomain/TranslationStructuredBatchBuilder.cs:92–103,131–146` | Из выбранного текста и phrase mappings получает required term; создаёт verifier chosen FAIL с issue `expected '{requiredTerm}' for translated quality marker` | Термин должен пройти настоящий producer, без ручной подстановки правильной метки после парсинга |
| Тот же файл:181–189,258–267,566–567 | `WordRegex` допускает прямой апостроф внутри слова; fallback выбирает первый word | Контракт producer действительно допускает термин с апострофом |
| `27.TranslationDomain/TranslationMachine.cs:107–123` | Обходит не-held-out примеры, вызывает `trainer.TrainOnExample` и записывает identity | Связь unit learner с основным учебным маршрутом доказана; unit-only опыт нельзя называть полным Flow-обучением |
| `27.TranslationDomain/TranslationTrainedUnits.cs:1095–1145` | Для quality-marker issue пишет required term в marker dictionary; signature-rule ветвь специально пропускается | Замер обязан доказать реальные записи marker state, не только счётчик dispatch |
| Тот же файл:1218–1257 | Оба parser метода извлекают `parts[1]` после `Split('\'')`; внутренний апостроф обрезает required term | Source-проблема доказана; изменение качества ещё не измерено |
| Тот же файл:1052–1068 | Читает learned source-marker rule; несколько terms принимаются через `Any` | Marker collision между control/target может скрыть эффект; guard нужен до любого обучения |
| Тот же файл:1265–1290 | Source marker вычисляется tokenizer-ом, сначала ADV, затем подходящий UNKNOWN token | EXAM может отличаться от TRAIN, но для чтения learned rule обязан сохранить нужный marker |

## Проверка дизайна и метрик

| Вопрос | Итог |
|---|---|
| Прямая связь с обучением | Native `ParseJsonl` → `BuildGeneralBatch` → два выбранных verifier lessons → native `TrainOnExample` → checkpoint mutation → native `Verify`. Прямой вызов Unit является тем же learner, который вызывает production Machine; полный Flow не выполняется |
| Один фактор | Только добавленная ветвь распознавания builder frame в `TryParseRequiredTerm`. Namespace A/B меняется симметрично; tokenizer, marker selection, `Contains`, концептный parser и data остаются исходными |
| Выбор actualTrain | Из двух raw pairs builder создаёт 6 rows; learner получает только 2 `:verifier` rows, HeldOut=false. Остальные 4 rows сохранены как evidence, не обучаются |
| Состояние | Два свежих Unit всего, один на плечо; перед Train у каждого пусты все 3 checkpoint коллекции. Нужна реальная мутация state; счётчик calls сам по себе недостаточен |
| Перекрытие marker | First ADV `нежно` для possessive и `горестно` для plain control подтверждён native allowlist. Маркеры различны, plain-term не попадает в OR bucket possessive и не маскирует эффект |
| EXAM | 8 фиксированных записей; Q1/Q2 — учебные тексты, Q3/Q4 — новый глагол с тем же marker, Q5/Q6 — plain control, Q7/Q8 — допустимые варианты. Повторное использование 8 ID до/после даёт сопоставимый readout |
| Gold | 5 positive и 3 negative; Q7/Q8 остаются true, хотя B по статическому прогнозу их отвергает. Правила gold не подстраиваются под механизм |
| Бюджет | На плечо 2 Train,8 Verify до+8 после,2 state exports; суммарно 4 actual learning calls,32 Verify,4 state exports. Neural/DPO updates0, fullFlow0, seeds N/A, repeats0 |
| Цена | Измеряется интервал настоящих 2 TrainOnExample; builder/Verify/export вне него. Allocated bytes не называются RAM, единичный cold timing не доказывает устойчивое ускорение |
| Размещение/исполнение | Source/host/output/launch command и runtime заданы;7 native files ×2 +2 metadata+3 runner files =19 Compile items. Closure проверена чтением, компиляция честно не заявляется |
| Инструкции исполнителю | Русский отчёт, текущая модель, конкретные навыки, pinned source, STOP/NEXT, сохранение старых attempts, read-only protected, запрет production/test/CI repairs. NEXT указан `RMS-ROW-REDUCE-01` |

Статический прогноз арифметически согласован: initial5/8correct,3/3falseaccept,0/5falsereject; A_after6/8,2/3,0/5; B_after6/8,0/3,2/5. Это **не измерение**: table описывает ожидаемые выходы исходного substring consumer. Снижение false accepts при той же общей accuracy и росте false rejects должно быть названо полезным успехом по первой оси и регрессией по второй. Term fidelity измеряется в настоящем learned state и имеет доказанный downstream consumer; это допустимая учебная метрика, а не произвольный форматный score.

## Адресные замечания и исправления

1. Исправлена неточная фраза `2 fresh empty dictionaries`: теперь два Unit instances всего и три пустые checkpoint коллекции **у каждого**.
2. Before-Train guard раскрыт конкретно:6 generated IDs,2 selected IDs в T1/T2 порядке, HeldOut=false, chosen Pass=false, два точных issue, разные firstADV markers и явный Q→marker mapping. Счётчик completed увеличивается после успешного возврата native Train; entered/failed отдельно. При нарушении предусловия PRECONDITION_FAILED, без выбора новой фразы.
3. Указаны абсолютный `launch.ps1`, одна внешняя команда, build120s/A30s/B30s, controlled process-tree termination, no retry. Перед материализацией сохраняются любые существующие source/host roots; existing attempt не перезаписывается и не заменяется новым номером.
4. Уточнено: pre-Train hash guard может читать сырые bytes `gold.json` только для SHA256 без разбора labels; сами labels читаются post-output. Это устраняет буквальное противоречие между запретом раннего чтения gold и проверкой его неизменности.
5. Уточнено: при timeout использовать только уже сохранённые actual counters; отсутствие durable receipt — null/«не измерено», а не 0 и не плановые 2. Дополнительный запуск ради получения счётчика запрещён.

Все пять уточнений перепроверены после изменения карточки и prompt. Открытых замечаний, препятствующих публикации **дизайна**, не осталось. Алгоритм, данные и бюджет не расширены. Никаких дополнительных вычислительных тестов для данного документного review не требуется.

Финальный readback source-рецепта: автор добавил проверку `issue.Length >= prefix.Length + suffix.Length` перед выделением term. Она не даёт одной общей кавычке malformed frame одновременно удовлетворить prefix/suffix и породить отрицательную длину `Substring`; такой вход сохраняет прежний fallback. Оба закреплённых native TRAIN issue проходят эту проверку, поэтому заданные A/B-фактор, прогноз и бюджет не изменены. Guard просмотрен статически; дополнительных cases/runs не выполнялось.

## Границы вывода

Карточка проверяет literal ASCII target term внутри одного точного frame native builder и двух фиксированных marker families. Ухудшение на допустимых Q7/Q8 открыто задано до запуска. Не заявляется универсальное качество перевода, корректность всех parser inputs, полнота других языков, генерация кандидата самим Flow либо преимущество над neural DPO. Metadata Machine/Flow содержит только родные константы и не заменяет learner; DomainTag сверён по Machine:15, FlowId по TranslationRoutingFlow:26.

На появившемся core `827a1acdbef95d220886e54f9d36bce0b405c8cd` весь production subtree `27.TranslationDomain` совпадает с 669, что подтверждено Git tree SHA. Гипотеза не исчезла. Подробная отдельная квитанция — `latest-core-learning-source-delta.json`; новая source pin не нужна. Runtime readiness, успешный эксперимент и разрешение переноса в продукт этот review не устанавливает.
