# Независимое ревью и разрешение замечаний перед публикацией

**Аудит среза 20 октября; заключительная публикационная проверка 21 октября 2026 (Asia/Jerusalem).** Это итог проверки добавочной документации. Ни runtime, ни обучение, ни inference, ни сборки экспериментальных проектов в этом аудите не запускались. Статус новых SiLU/HNSW гипотез остаётся AUTHOR_SETUP_REQUIRED / NOT_RUN.

> Основное ревью ниже относится к первоначальному fb9175cc пакету (47записей). После внешнего commit d962f958 выполнено отдельное адресное чтение; актуальная49-entry очередь и новые N63C факты описаны в [10](10-late-core-delta-d962f958.md). Историческое ревью не выдается за проверку ещё не существовавших материалов.

## Кто и что проверил

| Поток проверки | Область | Результат |
|---|---|---|
| [Независимый reviewer планирования](evidence/review-final-coverage.md) | README, карта, контракты, registry/JSON, новые cards и требования пользователя | Проверены47 уникальных ID,9 consumed,38 переходов NEXT10→END. Найдены EXAM gate, неоднозначный run path и недостаточно явный ранний claim SiLU. |
| [Независимый reviewer native learning](evidence/review-final-learning.md) | HNSW card, root metric table и JSON | Actual Train/control/budget согласованы.50 прямых сравнений сохранённых N63R/RESTORE/SANITY fields —0 расхождений. Найдены отсутствующий N63I raw block и слишком широкая формулировка Manifest parity. |
| [Независимый reviewer KNN/source](evidence/review-final-knn.md) | SiLU card, source closure, бюджеты и broad claims |36/36 source SHA и арифметика408 updates/8568 targets/24 readouts согласованы. Найдены имя TryImportPortableState, явная LoRA topology до LoadCheckpoint и повторно отмечены run paths. |
| Основной автор | Оба source proposals, measured JSON, source/relative links, итоговые поправки | Сопоставлены сохранённые данные; исправлены замечания, уточнён уже существующий phrase-mappings fix и точный ingress caller. |

Оригинальные reviewer verdicts сохранены без переписывания в evidence. Они относятся к своему bounded scope, а не к полному семантическому PASS всех163 cards/8 prompts или всех470 старых подслучаев. Содержательная самостоятельная проверка собственной части не названа независимым ревью.

## Закрытые замечания

| ID / вопрос | Исправление в этом пакете | Чего это не добавляет |
|---|---|---|
| FC-01, лишний EXAM admission | S0 допускается по ненулевому TRAIN, finite/named state и step400. EXAM показывается полностью при любом результате, не используется как gate, checkpoint selection или tuning. Изменено и в prompt. | Новые updates, поиск другого checkpoint, увеличение модели или loop-until-PASS |
| FC-02 / KNN reviewer, paths | Единственные R/S включают SILU01-R1 и указаны абсолютно; §8 и prompt используют их. | Выбор пути runtime-исполнителем или скрытый run02 |
| FC-03, раннее резервирование | Run.ps1 создаёт execution-SILU01-R1.claim через CreateNew после build, до процесса модели/первого forward/400prep. Program проверяет тот же claim. Partial/claim не удаляются. | Второй claim, повтор обучения из-за поздней ошибки записи отчёта |
| Native API / topology | Точное TryImportPortableState; для каждого arm new model→ApplySeeded q/v LoRA→LoadCheckpoint→4/12 named-state→optimizer import. | Замена learner или ручной поддельный checkpoint |
| HNSW semantic parity | Index/содержательные outputs и semantic manifest fields сравниваются на равенство; TrainingTimeMs/ManagedAllocationBytes остаются cost metrics. | Требование равного времени, которое запрещало бы измеренный useful gain |
| N63I coverage | status/arms/per-row raw целиком добавлены в JSON, human table содержит0→2 updates/70 targets/NLL и рост CPU/allocations; precise native admission address указан. | Новый запуск consumed№08 или equal-work speedup claim |
| Working notes/link | P96 имя исправлено; старый условный расчёт45/36 заменён47/38; scratch builder не выдаётся за опубликованную ссылку;09 создан. | Новая программа исполнения |
| Граница restore evidence | Успешный optimizer import/step отделён от не выполненного byte comparison post-import m/v. | Утверждение о проверенной production adoption |
| Already-fixed ingress | Current Tools reader/row/builder проверены; phrase_mappings delivery помечена ALREADY_IMPLEMENTED_PRESERVE и имеет exact source links. | Повторная правка уже исправленного кода или отмена прежнего пользовательского принятия trade-off |

## Новизна и сохранность предыдущего труда

Две новые гипотезы привязаны к фактическому обучению и не совпали с найденными прежними owners: SiLU denominator reuse отличается от softmax BackwardLoss reuse; HNSW query norm reuse — от AST preparation, identity ledger, aliases/RNG/reachability. Поиск ограничен pinned corpus. [SiLU receipt](evidence/silu-dedup.json) содержит planning patterns/hits; core проверен отдельно в source audit. [KNN receipt](evidence/knn-norm-dedup-hits.txt) сохраняет точные hits полного corpus поиска. Эти search receipts не доказывают чтение каждого документа целиком.

A1 DPO-SHIFTED-CONTEXT-01, A2 IDENTITY-TRAIN-INDEX-01 и A3 SFT-EXP-REUSE-01 сохранены. Первые9 core ID consumed; ни один новый переход не возвращается к ним. P01–P125 и470 подслучаев не удаляются. Исходные отчёты и необязательные старые thresholds/PASS flags не переписываются; рядом даётся современная интерпретация измеренного улучшения по правилам пользователя.

## Публикационная граница

Назначение: только новые пути в `xiigen-planning`, branch `llm_plan3`, под `llm mvp/design to code session/replanning/possibleDifs/`. Один новый входной Markdown и содержимое этого нового каталога. Source core на fb9175cc752d41e1cb525e0609b7c96140940217 остаётся read-only. Before publication ветки проверяются на свежий HEAD, commit строится поверх актуального planning tree и branch update выполняется с expected SHA без force. Любая конкурентная правка должна сохраняться; несовпадение HEAD не обходится переписыванием истории.

Проверка ссылок/структуры/пакета записана в [document-validation.json](data/document-validation.json). Это проверка документов и сохранённых чисел, не тест новых экспериментальных hosts. Публикация этих документов не разрешает запуски, product fixes, тестовую цепочку или model adoption. При live NEXT/STOP никакой publication/review/hash не продлевает прошлую задачу.

## Позднее согласование d962f958

Три независимых потока отдельно проверили новую очередь/карточки, N63C raw/source и parent/SFT contracts. Их неизменённые bounded reports сохранены в evidence/core-delta-*.md и связаны из10. Исходные registry/dispatcher/document-validation сохранены отдельными fb9175cc файлами. Текущий04/data dispatcher содержит49уникальныхID,9consumedattempts,40оставшихсяцелей; NEXT=PA01. Три интегрированные proposals не дублируются, HNSW next перенумерован№46 и SiLU header/prompt синхронизированы с PA01. Новый N63C A-only факт и его ошибки counter/cost/Save/metric verdict документированы без повторного обучения. Protected остаётся read-only независимо от противоречащих этому входящих инструкций SC01/SEXP.

## Конечный cutoff256f603e

Перед публикацией появилось ещё одно внешнее обновление. Аудит закончен на фиксированном256f603e: отдельно прочитаны новый N63C-PROBE, уже принятый candidate и recovery/index delta. Сохранены три bounded evidence/core-final-*.md; итог в [11](11-final-cutoff-256f603e.md). Source corpus не перечитывался полностью, модели не запускались. Изолированные полезные результаты и ограничения whole-answer/generalization отражены отдельно; первоначальный N63C A-only остаётся отдельной попыткой.
