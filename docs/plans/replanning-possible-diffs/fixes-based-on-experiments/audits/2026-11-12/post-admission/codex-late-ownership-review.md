# Поздняя проверка реализации владения попыткой

Дата: 12 ноября 2026. Reviewer: Codex, исследователь исполнения попытки. Проверенный commit: [f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64](https://github.com/lubochka/llm_mvp_core/commit/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64), DRAFT AUDIT-F03.

**Новый sink действительно исправляет прежний беспрепятственный захват занятого untracked каталога и замену одного имени через свои `WriteTextExactly`/`WriteBytesExactly`. Старое описание sink на f089 больше не описывает текущую реализацию.** При этом новый механизм ещё не обеспечивает захват до обучения каждого ordinary caller, запрет всех записей после завершения и непроходимую границу write root. Ниже четыре конкретных остатка; это продолжение B05, а не отмена полезного частичного ремонта.

Проверены точные bytes нового sink и ownership tests. P58 дополнительно прочитан по immutable URL нового commit и совпал с прежним blob; Dpo caller прочитан из закреплённого файла нового snapshot. [Manifest источников](codex-late-ownership-source-manifest.json), [четыре ограниченные наблюдения](codex-late-ownership-counterexamples.json), [программа](codex-late-ownership-counterexamples.py). Здесь не исполнялись C#, PowerShell или модель. Старые f089 контрпримеры и их pins не изменены.

## Что новый код уже делает

В [ExperimentEvidenceSink.cs](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/Tests/ExperimentFixes/ExperimentEvidenceSink.cs) factory отдельно проверяет занятость (283–312); `DescribeOccupiedAttempt` отказывает маркеру и непустому каталогу без маркера (625–655); `TryCaptureAttemptOwnership` создаёт marker через `FileMode.CreateNew` (685–733). Это полезная атомарная резервация имени. Handle закрывается при выходе из capture; существование постоянного marker сохраняет резервацию, но само по себе не различает активный и умерший процесс.

Записи через `WriteSingleShotBytes` создают отдельный временный файл и используют `File.Move` без разрешения замены (503–549). Обычная попытка второй записи того же имени получает явный отказ. `WriteOwnerStatus` и `AppendJournalLine` выделены как отдельно изменяемые записи (439–475). Проверка относительного пути появилась во всех этих методах. Уже принятые published redirect/refusal и byte fidelity не следует возвращать в статус отсутствующих.

Сохранённые native результаты разобраны отдельным worker: [проверка raw AUDIT-F03](codex-late-ownership-evidence.md). Producer дважды получил 14/14 = девять новых ownership, четыре прежних sink и один Dpo publication caller; reviewer получил ровно те же девять плюс четыре, 13/13. Это не прогон тринадцати ordinary callers. Обе адресные мутации действительно краснят свои именованные проверки. Границы связи исходников с DLL и различие 19 фактических raw файлов против 21 в описании commit сохранены в том подотчёте.

## Четыре точных остатка

### L1. Регистр в guard разрешает запись в соседний каталог на case-sensitive FS

Строки 599–618 нормализуют путь и затем используют `StringComparison.OrdinalIgnoreCase` независимо от файловой системы. Для root `/.../attempt` относительное имя `../ATTEMPT/foreign.json` становится `/.../ATTEMPT/foreign.json` и проходит это сравнение. На проверенной case-sensitive POSIX FS `attempt` и `ATTEMPT` — разные каталоги; файловая проекция создала файл именно в соседнем. Законное `nested/legal.json` принимается, обычный `../foreign/file.json` отклоняется; различающий case нужен дополнительно.

Это конкретное нарушение обещанной границы, а не утверждение о такой же коллизии на каждой Windows FS. Исправление должно учитывать реальные правила сравнения путей на поддерживаемых FS. Отдельно определить политику symbolic links/reparse points: одного строкового префикса для физической границы недостаточно. Native контроль: case-sensitive sibling, законный вложенный путь и обычный выход вверх, Train=0.

### L2. `CompleteAttempt` не закрывает последующие записи этого owner

`CompleteAttempt` (484–487) лишь создаёт одноразовый файл завершения. `WriteSingleShotBytes` (503–549), `ResolvePath` (392–404), `WriteOwnerStatus` и `AppendJournalLine` не проверяют completion или состояние owner. Поэтому последовательность «CompleteAttempt → WriteTextExactly с новым именем» остаётся разрешённой. Отказ повторной записи *того же имени* completion не доказывает закрытия попытки.

Файловая проекция демонстрирует этот точный остаток, сохраняя положительное свойство: повторное имя completion отклоняется, но новый raw после completion создаётся. Она моделирует успешную ветвь для отсутствующего target; внутренности `File.Move` и native lifecycle не исполнялись. Приёмка ремонта должна проверять единый state gate для всех каналов записи и отсутствие изменений bytes после закрытия. Внешний финализатор по-прежнему отвечает за фактический child exit; локальный completion не становится его заменой.

### L3. Выданный путь позволяет обойти однократную запись

`ResolvePath` (392–404) возвращает путь существующего raw без reservation и без отказа для уже созданного имени. Поэтому после законного `WriteTextExactly("frozen-raw.json", original)` вызов `File.WriteAllText(sink.ResolvePath("frozen-raw.json"), replacement)` заменяет raw. Тот же accessor принимает имя owner marker. `EnsureWriteRootDirectory` (411–420) отдаёт весь root и не ограничивает последующие операции вызывающего.

Это воспроизводимый обход публичного API, а не утверждение, что текущий ordinary тест уже перезаписал конкретный frozen raw. Существование реального канала обходных writers подтверждается [DpoStepVectorTimingTests.cs](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/Tests/DpoStepVectorTimingTests.cs): `NewRoot` получает root через sink (298–315), а другой Fact пишет через `File.WriteAllBytes`/`File.WriteAllText` (127–132) и передаёт root следующему runner. Зелёный четырнадцатый publication case — другой метод и не проверяет эту цепь на повторную запись или закрытие.

План ремонта: перечислить реальные path/root consumers и провести их записи через тот же owner/state/immutable договор. Сохранение escape hatch требует явно ограниченного доверенного контракта; его нельзя одновременно считать доказательством однократности всех raw. Native отрицательный контроль должен идти через accessor и реальный способ caller write, а не только ещё раз вызывать `WriteTextExactly`.

### L4. В ordinary P58 marker по-прежнему появляется после setup Train

В [P5801CorrectiveLessonCounterExampleArmsTests.cs](https://github.com/lubochka/llm_mvp_core/blob/f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs) sink остаётся `Lazy` (145–146); единственный `Evidence.Value` находится внутри `WriteEvidence` (160). Для свежего класса/процесса, запускающего основной Fact, cold panel читается на 593, setup обучается на 612, повторная панель читается на 613, и лишь первая запись на 619 вызывает factory. Следовательно, новый внутренний factory claim не переносит ordinary entry перед подготовкой и обучением.

Новый helper `OwnThenTrain` в ownership tests (123–131) сам явно вызывает factory перед synthetic счётчиком. Его зелёный результат полезен, но не доказывает такой же порядок в неизменённых callers. Этот остаток уже подтверждён чтением конкретного ordinary метода; он точнее формулировки «все 13 regression пока UNRUN». Claim надо выполнить в entry/fixture до первой операции модели и проверить проигравший/ошибочный вход с нулевыми setup/model counters. Затем назначить ограниченный ordinary regression с заранее известной дозой. FIX-P56 и Learn07 не менялись этим commit и требуют отдельного охвата.

## Семь оговорок DRAFT: что с ними делать

| Оговорка commit | Проверенный смысл и граница |
|---|---|
| Launcher stdout/TRX ещё не имеют общего owner с C# writer | Новый sink использует process-static run ID/переменные окружения, но не подключает lifetime существующего launcher. Сохранить единый executor как владельца внешнего результата; marker — частичный механизм, не новый terminal. |
| Проверена гонка потоков одного процесса | В тесте восемь `Thread` и `Barrier`, строки 143–184. Отдельные OS processes не запускались этим контролем. Следующий маленький native case должен различать процессы и их run IDs. |
| Шесть связывающих полей marker не утверждены | Payload содержит source/dirty/build/project/filter и run identity (744–759); tests главным образом утверждают существование marker. Поля seeded чужих marker не являются проверкой producer payload. Проверить expected values и связь с фактическим child/build. |
| Прежние callers становятся неповторяемыми на незакоммиченном каталоге | Второе одноимённое доказательство через тот же sink теперь отказывает. Новый factory с default redirect может получить новый root; не следует утверждать, что любой повтор обязательно падает. Проверить адрес и политику каждого caller, не ослабляя сохранность старых bytes. |
| Два каталога под training-artifacts требуют разбора | Compare содержит две цепи по три файла, включая owner marker. Это уже опубликованная история; этот аудит ничего не удаляет. Нужна явная классификация назначения этих файлов, а не автоматическая уборка ради чистого отчёта. |
| Permission failure не обёрнут | В `WriteSingleShotBytes` ловится только `IOException` (530); `UnauthorizedAccessException` не получает этой обёртки, а создание parent dir стоит до try. Отказ остаётся видимым, но диагностика и уборка не покрыты общей ветвью. |
| Не выполнены regression тринадцати callers | 13/13 reviewer относятся к helper tests, а producer добавляет только один publication caller. Полный список/доза нового selected regression должны быть назначены до запуска; L4 уже показывает отдельный source дефект порядка, который одними counts не закрыть. |

## Как продолжать и что сохранить

R01 остаётся первым, но его актуальное содержание изменилось: сохранить реализованные marker reservation, occupied checks и non-replacing sink writes; закрыть L1–L4, передать единый контекст существующего executor фактическим writers и различить active/closed/abandoned. Следом R02 связывает реальный child, required IDs, stdout/stderr/TRX, build identity и окончательный исход. B14/P50 остаётся следующим предметным шагом. Принятое A07 этой дельтой не затронуто.

Аналитический повтор:

```bash
python3 codex-late-ownership-counterexamples.py --source-dir /path/to/pinned-flat-sources --output /path/to/new-f95-result.json
```

Три исполняемых входа перечислены в программе и manifest. Четыре записи L1–L4 — filesystem/source projections, не четыре C# tests и не новая приёмка. Повтор дал побайтно тот же JSON; попытка занять существующий output отказала до проб, bytes сохранены. [Проверка инструмента](codex-late-ownership-tool-validation.json).
