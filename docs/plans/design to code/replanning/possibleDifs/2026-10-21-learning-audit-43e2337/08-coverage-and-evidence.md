# Что проверено и как воспроизвести выводы аудита

> **Позднее уточнение источников:** основной аудит ниже относится к core `43e2337`. После его публикации появился `0cf3634`: LC-REF-CACHE уже имеет сохранённые результаты, общий реестр содержит53 ID, а для S0 назначены canonical shared families. Для текущего статуса и путей приоритет имеет [дополнение09](09-late-core-0cf3634-update.md); прежние выводы оставлены как датированный срез, а не текущая инструкция повторять опыт.

## 1. Снимки и границы записи

- Core: [43e2337d68dad0e4ab8d7fbb0cea38406b592b61](https://github.com/lubochka/llm_mvp_core/commit/43e2337d68dad0e4ab8d7fbb0cea38406b592b61), ветка `codex/dpo-implementation`.
- Planning до добавления этого пакета: [1212d2d6a8d6cf9d9755a83e0e53f7768dd62e44](https://github.com/lubochka/xiigen-planning/commit/1212d2d6a8d6cf9d9755a83e0e53f7768dd62e44), ветка `llm_plan3`.
- Оба head повторно прочитаны перед финальной сборкой документов и совпали с исходными pins. Это срез источников, не обещание неизменности веток после публикации.
- Новые files только в `llm mvp/design to code session/replanning/possibleDifs`. Старые файлы не удаляются. Записей в core, защищённую папку, production code, tests или model state этой работой нет. Native runs/builds — **0**.

## 2. Объём получения, поиска и чтения — разные величины

| Слой | Проверено | Что не утверждается |
|---|---|---|
| Исходный possibleDifs |994 Markdown, полный текст получен и доступен для поиска, Git blob SHA сверены | Не994 ручных семантических review |
| Выбранный защищённый корпус |374 Markdown: документы, карточки, промпты и выбранные отчёты; полные тексты/SHAs | Не все1464 blobs защищённого дерева; бинарники и каждый raw scaffold не читались |
| Всего manifest |1368/1368 exact Git blobs, missing0,mismatch0 | Совпадение bytes не доказывает правоту содержания |
| Активная очередь |50 ID,46 отдельных файлов; наличие/якоря/NEXT, требования и содержательные заметки по каждому ID | Не50 выполненных/успешных экспериментов и не50 проверенных сборок |
| RP01/родительский RAG |15 выбранных источников:8полностью,7по указанным spans;15/15 exact blobs | Большие прочие WorkLeft/R01–R25/RP02–05 каталоги не объявлены полностью прочитанными |
| Сохранённые результаты | Raw для ключевых спорных N63C/WT/WM/N60/N-KU/BPE/restore и12 исторических receipts в evidence | Повторные документы об одном запуске не считаются независимыми испытаниями |
| Новый native neural host source |36-файловый closure RD01@66947785 совпадает по Git blobs с current43e2337 | Это source compatibility, не построенный runner или доступный qualified S0 |

Полный список исходных путей/ref/blob/size: [materialize-manifest.json](evidence/materialize-manifest.json). Проверка: [full-harvest-receipt.json](evidence/full-harvest-receipt.json) и [независимое совпадение](evidence/learning-code-materialization-verify.json). История/выбранные primary источники: [history-evidence.json](evidence/history-evidence.json). Cache paths внутри provenance JSON описывают временные локальные копии исследователей; для чтения результата они не нужны — authoritative repo/ref/path и ссылки сохранены.

## 3. Как проверялась новизна

Для каждого фактора выполнен полнотекстовый поиск по1368 Markdown и адресное чтение ближайших совпадений. [Neural dedup](evidence/learning-code-dedup.json) хранит regex, hits и решения; [static-field dedup](evidence/static-field-novelty.json) — source paths/lines и охват. Длинные повторные цитаты старых документов из публикуемого static-field JSON исключены; пути и строки сохранены.

| Новый фактор | Ближайшие прежние записи | Граница новизны |
|---|---|---|
| Static-field false-positive label | N48/NG01 — nested/receiver/numeric conditions подавляют нужный guard; N-KU — do-while | Новый конкретный контрпример в известной семье ActionLabelGenerator; не новая root-cause family |
| Mean backward coordinate arrays | RMS-ROW-REDUCE — forward SumAlongAxis; RMS-COORD — MultiplyBroadcast; known keepDims=false correctness | Новый точный ресурсный фактор живого keepDims=true backward; не повтор старого shape fault |
| Attention Softmax initial copy | N62-R2/LF-02 — exp scratch в DpoTrainer.BackwardLogProbs | Другой метод/проход; не уменьшение числа массивов и не доказанное ускорение |

Вердикт: `NO_EXACT_FACTOR_DUPLICATE_FOUND_IN_SEARCHED_MD`. Это ограниченная проверка конкретного корпуса. Она не гарантирует, что подобного предложения нет в непрочитанном бинарном материале, другом неопубликованном разговоре или всех возможных ветках. Найденные известные риски LoRA, SFT moments, codec/tie-break и restoration не переименованы в новые.

## 4. Навыки и независимая проверка

Использованы запрошенные архитектурные, планировочные, review, code-examination, prompt-authoring и human-communication материалы. Для объёмных текущих skills прочитаны относящиеся к задаче разделы; полная загрузка всего исторического корпуса skills не заявляется. Присланные краткие версии учитывались как исторические источники; актуальные условия в карточках имеют точные существующие filenames. `experiment-design-SKILL.md` дополнительно проверен целиком: его auto Tier escalation/SCALE_UP и старые acceptance thresholds разрешены явно в [контракте](06-execution-contract.md) по текущим правилам пользователя.

Архитектурная проверка требовала writer→learned state→reader. Проверка планов требовала точный host, бюджет, source, dataset, baseline, запуск и NEXT. Проверка кода отделяла source facts от runtime. Промпты фиксируют default model, язык, отсутствие полной production-цепочки и поведение STOP/NEXT. Человеческая коммуникация сохраняет успех и ограничения рядом, не заменяя учебную цель зелёным статусом.

Работали три параллельных исследовательских подпрохода: current neural code, карточки/raw, история/RAG/kNN. Их выводы сведены root; повторное согласие не названо новым экспериментальным evidence.

### Исправлено в новых карточках после независимого review

1. FSR entry перенесён в wrapper с guard **до build/log redirect**; `dotnet run` не мог бы обеспечить этот порядок.
2. FSR common qualification требует обе группы3/3 и TrainingSize6. Ненулевой total сам по себе пропускал бы empty constant ADD_METHOD.
3. Для static predicate назначены per-row SemanticModel, единственное syntax tree, metadata reference и OutputKind; unresolved S1–S3 теперь `BLOCKED_SEMANTIC_BINDING`, а не ложный negative result.
4. RMS получил явный runtime switch и включение только для B TRAIN; A0/B0 и after-readout исходные. Обе нейронные карточки получили exact SDK OutputPath и build argv, соответствующие DLL commands.
5. Проверены счётчики: FSR5Train/36переданных пар/55Predict; каждая neural pair8updates/8backward/24forward; AdamW400→404. В будущем реальные счётчики должны подтвердить рецепт, но здесь запусков нет.

Независимый финальный [readback21проверка](evidence/final-new-card-readback.json) имеет DOCUMENT_READBACK_PASS и0открытыхblockers в назначенном объёме. Окончательная проверка связности карточек/промптов/реестра и локальных ссылок фиксируется в [publication-review.json](evidence/publication-review.json). Она проверяет документацию; не выдаётся за проверку компиляции или фактического качества обучения.

## 5. Незакрытые существенные вопросы

Точный matrix→kNN donor quartet (состояние, lessons, target codec, matched EXAM) для всего воспоминания пользователя не восстановлен. Проверенные E09/E40/E34/E36 показывают конкретные успехи и ограничения. Наличие sibling source не доказывает ready-to-load trained state.

Полный causal ответ «какая доля деградации RP01 вызвана какой правкой» требует отдельных сопоставимых learning A/B. Ряд источников уже исправлен между историческим experiment commit и нынешним code snapshot. Статические доказательства не показывают величину эффекта, а похоже названные метрики могут иметь разные знаменатели. Новые карточки не скрывают этих ограничений и не отправляют исполнителя неопределённо искать недостающий набор/модель/инструмент.
