# Фактический NEXT: №08 завершён, следующий №09

**№01–08 имеют сохранённые результаты. Следующий — №09 `N63-AR-CAP`, в фазе авторской подготовки.** Это документное согласование, не разрешение запускать обучение в этой сессии. Ограничение learning panel завершённого опыта не возвращает его в очередь и не разрешает повтор, дополнительный attempt или продолжение обучения.

## 1. Почему старый индекс нельзя использовать как фактический статус №08

Первичный [N63I/results/arms.json](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/arms.json) уже существует на исходном core `dd150aa5216e42b8a8bba14955337398ec5f8397`. Git blob — `68a4f304f5f5cfb4e1be12b9f1c7e1aab718351e`; на позднем `81141d57b847bc050881c2fabba8302ac18c0f8a` он тот же. Устарел статус protected execution-index, а не отсутствует выполненный эксперимент. Предварительное документное ревью нашего пакета повторяло этот старый NEXT08; поздняя проверка raw исправила ошибку до публикации.

| Показатель №08 N63-INGRESS-LENGTH | A-full | B-bounded |
|---|---:|---:|
| Статус | `TRAINING_EXAMPLE_CONTEXT_EXCEEDED` / `FAILED_NO_STATE_CHANGE` | `COMPLETED` |
| Подано строк | 3 | 2 |
| Реальные optimizer updates | 0 | 2 |
| Обработано target positions | 0 | 70 |
| Изменился state модели | Нет | Да |
| Отложено строк | 0 | 1 — `train-long` |
| CPU в интервале попытки | 62,5 ms | 93,75 ms |
| Allocations в интервале попытки | 81 992 B | 3 330 408 B |
| Elapsed в интервале попытки | 279,9091 ms | 111,3761 ms |

Полезный эффект — короткие учебные строки действительно достигли learner и обновили состояние, несмотря на длинный непригодный материал. A и B выполнили разный объём работы: это **не equal-compute сравнение ускорения обучения**. Рост CPU/allocations показан рядом; один elapsed исход не доказывает ускорение одинакового update.

Связь с обучением адресная: [Program.cs:70–89](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/Program.cs#L70-L89) формирует admission, [132–140](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/Program.cs#L132-L140) вызывает настоящий `IncrementalTrainer.TrainAsync`; [205–235](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/Program.cs#L205-L235) записывает статус и результаты. Будущий рассматриваемый admission fix — отложить только несовместимую строку до вызова batch learner, сохранить её ID и причину; не стирать её и не считать обученной. Производственный перенос здесь не выполнен и требует решения пользователя.

## 2. Что действительно изменилось в учебной панели

[per-row.json](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/per-row.json) содержит24 записи:20 измерены,4 строки `train-long` отмечены `NOT_SCORED_OVER_CONTEXT`, а не нулевой loss. Ниже пересчитаны только существующие данные B; новых forward не было.

| Показатель B | До двух updates | После двух updates | Вывод |
|---|---:|---:|---|
| TRAIN response NLL, взвешенная по40 target tokens | 6,595570459138105 | 6,592119856314251 | Полезное снижение примерно0,003451 |
| EXAM response NLL, взвешенная по43 target tokens | 6,615796678265578 | 6,61468287730819 | Полезное снижение примерно0,001114 на этой панели |
| TRAIN правильные response tokens | 0/40 | 0/40 | Нулевой результат |
| EXAM правильные response tokens | 0/43 | 0/43 | Нулевой результат |
| Полностью правильные teacher-forced TRAIN responses | 0/2 | 0/2 | Требование ненулевого полного результата не выполнено |
| Полностью правильные teacher-forced EXAM responses | 0/2 | 0/2 | Улучшение ответа не доказано |

Формула NLL в этой таблице: сумма `responseNllSum` / сумма `responseTargetCount` по указанному split. Это не невзвешенное среднее двух строк разной длины. [status.json](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/results/status.json) фиксирует `MEASURED_REQUIRES_METRIC_REVIEW`,2 train attempts,20 scoring forwards,0 DPO/Generate/teacher calls. Ненулевые updates сами по себе не превращены в quality verdict.

**Кандидат для решения пользователя:** сохранить admission/update и NLL gains как успех соответствующих показателей домена коротких C# уроков; одновременно отметить нулевую learning panel. Не объявлять опыт успешным доказательством полного task answer и не обесценивать измеренные полезные эффекты. На текущем шаге правится документная трактовка, не запускается переделка законченного опыта.

## 3. Согласование с параллельно опубликованным пакетом

Во время подготовки публикации planning продвинулся с `0ea8f0b0094e234ffe7734adfa95e2a9e53203f5` до `2d1741261e9e17c0e4dec94a480b5711f5ad47d3`:36 добавленных файлов,28 из них Markdown. Все28 получены и сверены по Git blob. Изменения другого агента сохраняются; наш Git commit должен иметь свежего родителя и добавлять только собственную директорию.

[Его REGISTRY.md](https://github.com/lubochka/xiigen-planning/blob/2d1741261e9e17c0e4dec94a480b5711f5ad47d3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-dd150aa/REGISTRY.md) уже согласует42 существующих ID: ATTN39, JSON40, прежние dependent consumers IDENTITY41/KN42. Два его новых предложения — `DPO-SHIFTED-CONTEXT-01` и `IDENTITY-TRAIN-INDEX-01` — сохраняются как A1/A2 и не выдаются за новые находки нашего прохода.

Наш **SFT-EXP-REUSE-01 — отдельное предложение A3**. Оно проверено на отсутствие точного аналога в базовом корпусе1248Markdown и поздних дельтах5core+28planning. Итог —1281полученный и сверенный Markdown; это полнота поискового корпуса, а не1281полное смысловое ревью. Нельзя заключать из него, что рассмотрены все бинарные артефакты или все соседние ветки.

Добавление A3 сохраняет порядок остальных ID:09–38 → ATTN39 → JSON40 → A1 → A2 → авторская подготовка A3 → IDENTITY41 → KN42. Номера41/42 — сохранённые номера существующих ID, а не позиция в полной последовательности с предложениями. **42существующих +3предложения =45записей;8завершены,37остаются в документном плане.** Не все37 готовы к исполнению. [Полный добавочный реестр](07-additive-registry.md), [точный dispatch JSON](data/resolved-dispatch.json).

## 4. Восстановленная модель — полезный отдельный результат

RESTORE-MINIMAL-SET не является повтором №08. Его [raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json) уже существовал наdd150aa; поздний commit добавил отчёт. SaveCheckpoint сохраняет donor **на step1000, до200continuation updates**, а не наstep1200. После restore есть полезные TRAIN gains3/40→4/40tokens и NLL4,766696→4,711598, при ухудшении CONTROL NLL5,807963→5,857194.

Полный TRAIN exact по-прежнему0/2; «две строки с хотя бы одним правильным токеном» не равны двум правильным ответам. H16state нельзя автоматически подставить в H512/H64карточки. Весовой hash и optimizer import/step подтверждены raw; побайтная equality m/v и uninterrupted control не представлены. [Подробный разбор и кандидат исправления отчёта](11-latest-core-delta-and-restore.md). Ни повтор, ни универсальная resume equivalence не заявлены.

## 5. Переходы и граница исполнения

Текущая сессия только документирует. Никаких новых training, inference, builds, product tests, правок protected карточек или production. Выполненные01–08 сохраняются независимо от ограничений результата. Прямые NEXT/STOP пользователя немедленно прекращают работу над предыдущим опытом: без дополнительного report, review, hash, retry, Git или continuation. При отдельном поручении одного ID после него NEXT=END. Следующий назначенный ID в общей программе — **09 N63-AR-CAP**, а его готовность проверяется по конкретному авторскому рецепту, не по одному наличию строки в registry.
