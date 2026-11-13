# Поздняя дельта аудита: сохранённые доказательства AUDIT-F03

Дата: 12 ноября 2026. Проверенный core snapshot: [f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64](https://github.com/lubochka/llm_mvp_core/commit/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64). Это отдельная проверка позднего DRAFT; исходные результаты A07 и прежние записи аудита не изменялись.

**Два producer-прогона 14/14, review-прогон 13/13 и оба адресных мутационных отказа подтверждены самими named rows TRX.** Разница между 14 и 13 объясняется одним конкретным caller test. Полной связи каждой DLL с точными исходниками финального snapshot в сохранённых квитанциях нет; из этого не следует, что использовалась неправильная DLL.

## Что проверено

Скрипт [codex-late-ownership-evidence.py](codex-late-ownership-evidence.py) читает 23 закреплённых входа: все 19 файлов каталога попытки и четыре C# файла. Git blob SHA и SHA256 проверены до и после разбора. Скрипт проверяет строки результатов, TestDefinitions, TestEntries, имена методов, test IDs и execution IDs; суммы вычисляет из строк и сверяет с Counters. Результат находится в [codex-late-ownership-evidence.json](codex-late-ownership-evidence.json). C#, PowerShell, native tests и обучающие опыты при этом не запускались.

| Сохранённый raw | Именованных строк | Passed / Failed | Реальный состав |
| --- | ---: | ---: | --- |
| [f03-green-01.trx](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-green-01.trx) | 14 | 14 / 0 | 9 ownership + 4 sink + 1 caller |
| [f03-green-02-after-revert.trx](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-green-02-after-revert.trx) | 14 | 14 / 0 | Те же 14 имён и test IDs |
| [f03-mutation-a-red.trx](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-mutation-a-red.trx) | 9 | 8 / 1 | Только ownership |
| [f03-mutation-b-red.trx](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-mutation-b-red.trx) | 9 | 8 / 1 | Только ownership |
| [review-f03-independent.trx](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/review/review-f03-independent.trx) | 13 | 13 / 0 | Все 9 ownership и все 4 sink |

Reviewer не исполнял `DpoStepVectorTimingTests.ObservationWrite_JudgesPublicationByActualRunTargetSoEvidenceStillReachesStorage`, test ID `485584e2-4879-9adf-a4f5-e174cc1d94a0`. Никакой ownership case между producer и reviewer не исчез. Это также не означает проверку тринадцати ordinary callers: 13 здесь — девять новых helper-проверок и четыре прежние helper-проверки.

[Discovery](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/discovery-01.log) перечисляет те же девять ownership методов. Green01 сохраняет полный filter, который согласуется с 14 фактическими строками; green02 сохраняет только placeholder `<F03+accepted+caller>`. Review-log не содержит полного filter command. Для этих двух прогонов состав установлен по named rows, а не восстановлен из неполной командной строки.

## Мутационные отказы и финальный исходник

Мутация A, описанная в log как замена `FileMode.CreateNew` на `FileMode.Create`, краснит только `TwoConcurrentRunsRequestOneAddressAndOnlyOneOwnsItWhileLosersNeverTrain`, ID `8b68016d-b28c-684b-a216-cc75ed608778`. Ошибка: ожидался один победитель, получено восемь. Stack указывает строку 188; в закреплённом финальном source это действительно `Assert.Equal(1, winners);`.

Мутация B, описанная как отключение проверки непустого untracked каталога, краснит только `ExistingUntrackedNonEmptyDirectoryWithoutOwnerMarkerIsNeverOverwritten`, ID `c053689a-2a69-ca35-a66d-9088abb5560d`. Ошибка: ожидаемое `ExperimentAttemptOwnershipRefusedException` не выброшено. Stack указывает строку 305, где в финальном source находится соответствующий `Assert.Throws`.

Все методы из TRX присутствуют в источниках snapshot. Для [ownership tests](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs) проверен blob `963166aa5bbcae4e7c131d4bc649e8f92b1c9603`; для [sink](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/Tests/ExperimentFixes/ExperimentEvidenceSink.cs) — `69372a6a6a3a63c7d62d29dbda0fcd3215715144`. Два других исходника, их hashes и все selected IDs находятся в JSON.

Это подтверждает соответствие названий, отрицательных контролей и конкретных assert текущему source. Сами изменённые mutation snapshots/patches в каталоге попытки не сохранены: точное изменение известно по тексту log. Helpers используют имитацию достижения учебного шага; эти счётчики не доказывают владение до настоящего обучения в каждом ordinary caller.

## Где доказательств не хватает

[DLL receipt](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/dll-identity.txt) состоит из двух строк: DLL SHA256 `35636d57e8d8d85cae69136ec9a8f4765f4843e00a30f70ecae6c78faa60929b` и базовый commit `f089a7d55d359ff103d0cdd9ddfb26eee346a16b`. Нет времени снятия, fingerprints изменённых C# файлов и отдельной привязки хеша к каждому green/mutation/review run. При работе на dirty tree базовый commit допустим как часть идентичности, но одного его недостаточно для точного восстановления собранного состояния.

Из standalone producer build-logs явный успешный terminal есть у 01, 02, 03, 04 и 06. Reviewer build тоже заканчивается `BUILD_EXIT=0`; обе mutation-записи содержат `build_exit=0` и `test_exit=1`. [Build05](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-05-final-test-project.log) заканчивается перечнем проектных outputs: terminal в сохранённых байтах отсутствует. Это неполная квитанция, не установленная неудачная сборка. Build06 завершён в 07:02:31Z, второй green стартует в 07:03:01Z; порядок совместим с rebuild после revert, но не заменяет связь по хешам.

Описание commit говорит о 21 файле улик внутри попытки. Фактический pinned subtree содержит **19**: 12 logs, 4 results TRX, 3 review. Для перечня пакета следует использовать этот состав. Числовое расхождение не отменяет разобранные результаты.

## Адресное усиление приёмки

Следующую доказательную попытку привязать к точным source blobs или dirty diff, hash каждой использованной DLL, полному literal filter, discovery IDs и паре build/run. Мутационные patches и возврат к исходному состоянию должны иметь собственные fingerprints. В review явно фиксировать разность выбранных IDs: здесь один caller пропущен, девять новых ownership контролей покрыты полностью.

При проверке оставшихся ordinary callers отдельно доказывать захват до подготовки/обучения и единое владение всеми writes/streams/TRX; зелёные структурные 9+4 не подменяют эти сценарии. Текущий пакет сохраняет статус DRAFT и подтверждает ограниченный механизм с перечисленными границами.

Повторить только аналитическую сверку можно так:

```bash
python3 codex-late-ownership-evidence.py --core /path/to/f95-core --output /path/to/new-result.json
```

Выход должен быть новым: отказ на уже существующем JSON отдельно проверен, байты сохранены. Исходные raw при сверке открываются только для чтения.

