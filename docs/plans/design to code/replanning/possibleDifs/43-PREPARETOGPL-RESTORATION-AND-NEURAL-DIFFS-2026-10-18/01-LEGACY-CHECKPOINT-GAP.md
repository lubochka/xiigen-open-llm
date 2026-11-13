# N-RESTORE-01. Старое learned state есть, но текущий loader требует ещё два направления

**Приоритет R01 — первым в восстановлении PrepareToGPL.** Сохранённое состояние `translation-v1` совместимо по набору направлений со старым runtime, а с нынешним — нет. Это отдельное различие сверх известных P25 cold restore, cache identity и migration source-language signatures.

**Приоритет обнаружения уточнён перед публикацией:** параллельно появилась [N41-T1](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md) с тем же4→6механизмом. Эта глава — независимое подтверждение и уточнение на реально сохранённом2550-byte checkpoint, не претензия на новую гипотезу.

Последняя параллельная [U-R1](https://github.com/lubochka/xiigen-planning/blob/004c4a79f3ec0c1dba387ba713abad108b7568fe/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-18-preparetogpl-recovery/01-translation-restore-and-unit-lineage.md) также содержит этот archived state. Механизм и существование fixture признаны общим независимо подтверждённым результатом; дополнительная ценность здесь — приложенный exact-byte скрипт и сопоставление с двумя сохранёнными probes.

## 1. Цепочка доказательств

| Звено | До: PrepareToGPL `31346d8928be` | После: current `a392b6be8f8d` |
|---|---|---|
| Формат | `schema_version=1`; loader принимает только1 | Loader по-прежнему принимает только1 |
| Runtime executors | RU_EN, EN_RU, HE_EN, EN_HE | Те же4 + **ES_EN, EN_ES** |
| Сохранённый `translation-v1-checkpoint.json` | Четыре executor records | **Тот же файл, тот же Git blob**, тоже4 |
| Проверка completeness | `runtime.Keys.Except(checkpoint directions)` пусто | Остались EN_ES и ES_EN; source возвращает `EXECUTOR_DIRECTIONS_INCOMPLETE` |
| Когда изменяется live state | Исторический импорт проверял completeness после части мутаций | Нынешний импорт предварительно валидирует всё, затем мутирует; при отказе память не восстанавливается |

Источники: [старый импорт](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L306-L362), [старый runtime](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L555-L613), [нынешний импорт и валидация](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L308-L430), [нынешние шесть направлений](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L714-L780), [контракт schema 1](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationCheckpointContracts.cs#L20-L50).

Исходный checkpoint доступен [в PrepareToGPL](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/models/translation/translation-v1/translation-v1-checkpoint.json) и [в текущем core](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/models/translation/translation-v1/translation-v1-checkpoint.json). Проверены точные bytes, включая BOM/CRLF:

- Git blob: `a4cf054ba3f8a6ee6aa9818edc6c5705e3456c25` в обеих ветках.
- Размер: **2550 bytes**.
- SHA256: `95b5d1c6de1d70a6bba4adbafdf57ffafa0735b7c52d193e79f64acad7e84887`, совпадает с историческим runner-report.
- `version=0.1.1`, `has_structured_training=true`, 1 exact-source запись, 3 KNN triples, 1 history batch, 1 structured identity. Phrase/word maps пусты; новые направления отсутствуют целиком.

Это не случай «файл переобучили и стерли knowledge». Сохранившиеся bytes содержат старую память, но контракт runtime изменился. [Воспроизводимый результат проверки](evidence/legacy-checkpoint-result.json).

## 2. Обычный путь действительно доходит до этой проверки

[`LoadCheckpointAsync`](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationMachine.cs#L611-L650) читает JSON, создаёт `new TranslationMachine()` и вызывает `ImportCheckpoint`. Между созданием шестинаправленного runtime и импортом нет миграции четырёхнаправленного legacy state. Ошибка импорта возвращается вызывающему коду.

[`continue-translation-training`](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain.Tools/Program.cs#L4073-L4099) получает путь checkpoint из manifest, проверяет corpus hash, затем вызывает тот же loader. При `!load.IsSuccess` выполнение прекращается. Проверка CLI полностью в этой сессии не запускалась: утверждение относится к показанному source path, не к наблюдённому процессу пользователя.

Чтение одной лишь `schema_version=1` не различает старую и новую runtime topology. Добавление Spanish executors само по себе полезно; причинный пробел — отсутствие явно поддержанного способа принять конкретный старый state, сохранив строгую проверку неполных/повреждённых файлов.

## 3. Какое историческое поведение есть что восстанавливать

Архивный [runner-report](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/models/translation/translation-v1/runner-report.json) фиксирует один реально обученный исходный пример и два маршрута:

| Probe | Фактический output в retained report | Route |
|---|---|---|
| `add null check ValidateToken` | `Add a null check to ValidateToken.` | `executor-ru_en-exact-source` |
| `add a null check before ValidateToken` | `Add a null check to ValidateToken.` | `executor-ru_en-knn` |

Это узкое положительное свидетельство использования точной и неточной learned memory. Источник написан по-английски и помещён в RU_EN profile: нельзя называть эту единственную запись доказательством общего перевода русского языка. Поле `non_exact_probe_proves_generalization=true` означает здесь неточное извлечение сохранённого ответа. Оно не устанавливает независимое семантическое обобщение на широкий корпус. Старое `accuracy=1/3` — покрытие Units, не точность перевода; старое `reward_margin=0.03` не является измеренным DPO reward.

## 4. Почему успешный P25 от 18 октября не закрывает этот вопрос

В [P25-enru-cold-01](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md) нынешний шестинаправленный state был прочитан нынешним runtime: все 24 наблюдаемых outputs/routes/confidences/verdicts сохранились, качество осталось19/24. Это важный успешный native результат. Он проверяет нынешний **6→6** restore, тогда как N-RESTORE-01 проверяет исторический **4→6** переход. Эти утверждения совместимы.

P25 также не аттестует новый урок после восстановления старого файла, другие направления или neural LoRA restore. Каждый из этих вопросов имеет отдельного state owner.

## 5. Малый причинный опыт R01

**Статус: спроектирован, native НЕ ЗАПУЩЕН.** До обучения вернуть доступ к уже имеющемуся состоянию. Три независимых свежих процесса, без replay/teacher/optimizer:

| Плечо | Runtime и вход | Единственное назначение |
|---|---|---|
| A | Старый owner PrepareToGPL + точный legacy checkpoint | Воспроизвести два retained probes и подтвердить доступность baseline способности |
| B | Текущий owner + те же bytes | Наблюдать predicted refusal и отсутствие мутации |
| C | Текущий owner + явная копия legacy state с двумя пустыми ES executor records | Отличается от B только адаптацией старой topology; проверить возврат прежней памяти |

Для C нужен явно обозначенный migration fixture/host, который принимает конкретный известный old-v1 profile и добавляет **только** пустые `ES_EN`, `EN_ES` records. Не переписывать исходный файл; не ослаблять общий validator; не молча трактовать любой частичный checkpoint как legacy. Корректную production-версию migration следует оформить отдельным контрактом, если причинный опыт подтвердится.

Неизменяемые условия B/C: все четыре прежних executor maps, KNN triples, tokenizer corrections, verifier state, version/readiness, identities и history input одинаковы. Нынешняя нормализация исторического недоказанного reward0.03→0 должна сохраниться в обоих плечах. Именно поэтому побайтный equality всего export с историческим файлом не является правильным критерием: нормализацию метаданных проверять отдельно, learned content — отдельно. Timestamp также технический, но нельзя исключать произвольные поля под предлогом сравнимости.

**Измерить:** load result/error code; counts и digest каждого старого state owner; вывод и route двух архивных запросов; число новых lessons/updates=0; затем export→новый process→load с повтором тех же probes. Два пустых Spanish executors не считать обученными языковыми способностями. Untrained machine того же runtime — контроль того, что ответ действительно пришёл из восстановленной памяти.

**Подтверждение:** A воспроизводит retained memory behavior, B отклоняет exact checkpoint с двумя missing directions, C принимает и возвращает то же поведение старых направлений без новых уроков. Достаточный вывод — восстановлен доступ к этому learned state. Он не равен восстановленному качеству всего RP01.

**Опровержение/остановка:** обязательный реальный caller уже выполняет другую доказанную миграцию; pinned native B неожиданно принимает файл; либо C после успешного импорта не возвращает прежнее поведение. В последнем случае требуется отдельный consumer/representation diff, а не расширение этого опыта несколькими исправлениями сразу.

**Предметная область:** технические инструкции к коду и memory-based translation. Механизм несовместимости не зависит от содержимого; фактический положительный archived probe узок. Новые проверки RU/HE/ES требуют собственного материала. Новое обучение одного урока после C — отдельный следующий опыт с проверкой retention, не скрытая часть миграции.

**Бюджет:** 3 load-процесса, ещё1 для повторного cold restore C, по 2 архивных запроса в успешных плечах; 0 training calls. Измерить ms/load, ms/query, peak process RAM и bytes state. Neural layers/VRAM — N/A.

## 6. Что выполнено в этой сессии

[Скрипт](evidence/legacy_checkpoint_probe.py) проверил Git blob identity двух `TranslationMachine.cs` и одного checkpoint, извлёк направления из реального `CreateRuntime` и вычислил set difference. Это **SOURCE_AND_RETAINED_DATA_COMPARISON**, не заменитель native loader. Результат содержит `nativeLoaderExecuted=false`, `migrationExecuted=false`. Для повторения нужны исходные bytes двух закреплённых worktrees, без преобразования CRLF:

```bash
python evidence/legacy_checkpoint_probe.py --baseline-root /path/to/PrepareToGPL --current-root /path/to/current --output /tmp/legacy-checkpoint-result.json
```

Изменений core и тестов продукта нет. Ни фиктивная успешная загрузка, ни измеренное улучшение качества этому source-only запуску не приписываются.
