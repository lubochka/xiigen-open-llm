# P1 SOURCE-INTAKE rerun report

## Что было прочитано

Прочитан актуальный файл `xiigen-file-analysis-state.json` как очередь ссылок на источники (`source refs`), а не как готовый датасет. Файл читался потоково и проверялся повторным прогоном для idempotence.

- SHA256 source JSON: `CDE864671769700FC006CC198E97104E772C24ACEE946B96DE89E6C037B7932E`
- Размер source JSON: `320391713` bytes
- mtime UTC source JSON: `2026-06-14T07:29:10Z`

## Что было создано

- `source-ledger.jsonl`: `2,951` строка только для `state=done`.
- `backlog-summary.json`: агрегированные счетчики по состояниям и sample `todo` только как `file_name` + `path_sha256`.
- `intake-rerun-report.json`: машинный отчет о двух прогонах и проверке idempotence.
- `intake-rerun-report.md`: этот человекочитаемый отчет на русском.

## Точные счетчики P1

- total rows: `1,103,977`
- done: `2,951`
- todo: `1,101,026`
- ledger rows: `2,951`
- missing done source files: `0`
- duplicate source path rows: `9`
- DPO rows created: `false`

## Почему `done` только candidate

Строки `state=done` являются только кандидатными ссылками на источники. Они подтверждают, что source ref можно рассматривать как evidence для следующих фаз, но не делают документ gold-разметкой, DPO-строкой, training-ready материалом или доказательством semantic quality.

Каждая строка ledger остается в статусе `candidate_only_not_training_ready`. Перед любым обучающим использованием нужны отдельные проверки: material quality review, universal-vs-stack decomposition, source-disjoint split, ablations и parent review.

## Почему `todo` не материализован

Строки `state=todo` являются только backlog. Raw `todo` paths не выгружались в репозиторий и не дублировались в артефактах, чтобы не создавать приватный raw path dump. В `backlog-summary.json` сохранены только агрегированные counts и ограниченный sample в форме `file_name` + `path_sha256`.

`todo` rows не являются DPO rows, не являются training candidates и не должны использоваться как источник для обучения в P1.

## Проверка повторяемости

- ledger sha256 match: `true`
- backlog summary sha256 match: `true`
- counts match: `true`
- source JSON stable after runs: `true`
- idempotence passed: `true`

## Явные не-claims

Semantic quality не заявлялась. DPO rows не создавались. Training не запускался. `dotnet test` и build не запускались. Code changes не делались. Commit, push и branch switch не выполнялись.
