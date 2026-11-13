# Как читать и повторить механические доказательства

Это артефакты исследовательской проверки, **не веса модели, не новый учебный corpus и не native receipts обучения**. Скрипты не меняют core. Они воспроизводят выбранные формулы или операции carrier из C# и помогают построить следующий маленький native опыт. Проверка source anchor не доказывает полную эквивалентность Python и .NET.

## Три семейства probes

| Каталог | Что выполнено | Результат и граница |
|---|---|---|
| [routing](routing/probe_results.json) | Формулы target grouping/equality и global cutoff/eligibility | Same-target unique sources отвергнуты; allowed candidate исчезает после чужого high score. Raw scores — аналитический диапазон, не native measurement |
| [prompt](prompt/prompt_pattern_result.json) | Current aliases извлечены из source; nearest score/top2 margin на constructed pattern state | Duplicate margin0; 2/6→6/6 на ровно шести ASCII fixtures. Это ручной expected action, не DPO judge |
| [autonomous](autonomous/autonomous_source_probe_result.json) | Восемь weight/replay scenarios; 11 source blobs проверены | Weight4→1 в одном Domain; после quality reject cohort остаётся, следующий batch содержит2 replay. Конкретные `.NET Random` индексы не эмулированы |

### Routing

Из каталога `evidence/routing`:

```bash
python probe_learning_delivery.py
```

Скрипт записывает `probe_results.json` рядом с собой. Источники и refs — [source_manifest.json](routing/source_manifest.json). Он автономно повторяет формулы, не скачивает и не исполняет C#; [reference_check.json](routing/reference_check.json) — отдельная проверка существования cited source ranges. Новый native драйвер должен использовать настоящие compiler/provider/router и поля результата из карточек KU.

### PromptToTask

Нужна read-only копия трёх файлов из `llm_mvp_core@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`: `43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs`, `PromptToTaskMachine.cs`, `XiigenExtensionApiService.cs`. Пути должны сохраняться внутри выбранного source root. Точные Git blob и SHA256 находятся в receipt.

```bash
python prompt_pattern_probe.py --source-root /path/to/pinned-current-core --out prompt_pattern_result.json
```

Извлекаются только literal AddAliasIf mappings и проверяются указанные anchors. Unicode-механика не объявлена эквивалентной: все выполненные fixtures ASCII. Composite action в probe сокращает C# payload; native B обязан учитывать все semantic fields и SplitId, сохранять provenance и конфликтные записи.

### Autonomous

Для **повтора формул без новой source verification**:

```bash
python autonomous_source_probe.py --output autonomous_source_probe_result.json
```

Для повтора вместе с проверкой Git bytes:

```bash
python autonomous_source_probe.py --source-root /path/to/pinned-autonomous-sources --output autonomous_source_probe_result.json
```

Этот source root содержит `current/` и `external/` с путями из [manifest](autonomous/source_manifest.json), взятыми по указанным commit SHA. Сам скрипт выполняет только локальное чтение. Проверка bytes требует исходных байтов Git, включая корректное завершение файла; нельзя массово нормализовать line endings ради прохождения hash.

## Остальные реестры

- [History manifest](history/downloaded-source-manifest.json): 84 полученных source texts; это не счётчик запусков.
- [Novelty ledger](novelty/novelty-ledger.json): regex, совпадения, excerpts и refresh. Исходный корпус661, после10 новых Markdown и2 replacement versions —671 unique.
- [Corpus manifest](novelty/corpus-manifest.json): SHA256 локально сохранённого текста для фиксации поискового корпуса, не аттестация полного чтения и не всегда Git blob hash.
- [Независимая PT source review](review/prompt_to_task_independent_review.md): отдельная проверка трёх remote blob SHA и тех же формул; повторный receipt не считается новой выборкой.
- [Итоговый independent content review](review/final-candidate-review-addendum.md): точный candidate, закрытые замечания и область принятия исследовательской документации. [Первая стадия](review/final-candidate-review.md) сохранена вместе с уточнениями.

Для любых quality claims нужен дополнительный native consumer, исходное сохранённое состояние, независимая заранее фиксированная предметная оценка и raw before/after outputs. В этом пакете такие claims отсутствуют.

## Окончательный срез новизны

[Core delta ledger](novelty/core-final-delta-ledger.json) фиксирует ещё 13 Markdown версий, 2 новых пути и итог 689. [Адресный независимый review](review/core-late-delta-review.md) различает KU-02/NK-03 и признаёт prior generic lead у PT-01. Итог пакета: пять новых конкретных гипотез и одна новая локализация; прежние receipts сохранены как этапы.
