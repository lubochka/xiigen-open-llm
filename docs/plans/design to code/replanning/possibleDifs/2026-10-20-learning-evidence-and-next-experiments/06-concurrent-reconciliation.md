# Согласование параллельных дополнений перед публикацией

> **Исторический промежуточный срез.** Окончательный827:01–06 completed, NEXT=LF-03; [текущая сводка](08-final-core827.md). Старые next/counts ниже не управляют запуском.


> Этот документ фиксирует промежуточное согласование planning53cb/core883. Позже опубликован core669: текущие статусы38/5completed/33pending и supersession NG01 описаны в [07](07-core669-status-and-results.md). Прежний NEXT N62-R2 ниже — история, не команда запуска.


Исходный полный аудит закреплён на planning6bc4/core883496. Во время подготовки публикации planning продвинулся до **53cbcabd3926cda714bab428d294eb666301e9e4** двумя commits:9d311be и 53cbcab. Core остался 883496. Этот пакет добавляется поверх 53cb и сохраняет оба новых каталога и изменённый possibleDifs README. Старые документы, опыты, results и production не удаляли и не переписывали.

## 1. Объём дополнительной проверки

Compare6bc4→53cb содержит 87 новых файлов и 1 изменённый; среди них 52 новых Markdown и 1 прежний README. Все 53 Markdown получены и сверены по Git blob. Updated possibleDifs:1400 файлов /799 Markdown до нашего пакета. [Manifest](coverage/concurrent-source-manifest.json) отделён от первоначальных 1313/747 receipts.

Буквальный поиск `AccumulateGrad`, `gradToAdd`, `mappedBIndices`, `GRAD-ACCUM-COPY-01`, `FROZEN-BROADCAST-MAP-01` по этим 53 не дал совпадений. Дополнительно содержательно сопоставлены ROPE, AST, SKNN, BPE-REALIZED и novelty ledger; очередь и output assignments прочитаны по пяти документам, primary/current DPO provenance — по трём. Это адресная проверка пересечений, не заявление о ручном semantic review всех 799 Markdown.

## 2. Что совпадает, а что остаётся отдельным

| Материал concurrent authors | Точное различие с нашими выводами |
|---|---|
| ROPE-TRAIN-TABLE-01 | Повторное получение cos/sin таблиц во время Train; не первое копирование AccumulateGrad и не no-grad backward map |
| AST-SIB-CACHE-01 / SKNN-SIBLING-PREP-01 | Один фактор sibling preparation для actual StructuralKnn.TrainFromSource; SKNN остаётся reference-only alias, без дополнительного run |
| BPE-REALIZED-MERGE-COUNT-01 | Неперекрывающиеся реализуемые merge counts могут менять learned BPE state; не tensor allocation |
| GRAD-ACCUM-COPY-01 | Остаётся новым точным фактором по проверенным корпусам; native AccumulateGrad double copy отличается и от LF-03 |
| FROZEN-BROADCAST-MAP-01 | Остаётся отдельным уточнением RMS-семьи; no-grad map skip сохраняет FlatToIndices, тогда как известный RMS-factor меняет координатные allocations |
| N62-R1 interpretation | Также описан concurrent авторами. Наш corrigendum — независимое подтверждение/readback, не исключительная новая гипотеза и не повод повторять run |

Pinned source links и ближайшие аналоги — в [learning evidence](learning/learning-evidence.md). Новизна ограничена проверенными срезами; математическая уникальность идеи не заявлена.

## 3. Единая очередь и outputs

[Реестр](04-execution-registry.md) содержит 38 ID,4 завершённых и 34 pending. NEXT=N62-R2. Все 36 ID свежего опубликованного реестра сохраняют относительный порядок; новые GRAD и FROZEN стоят после BPE-REALIZED, перед IDENTITY и KN-ROUTER. NG01 сохраняет canonical ID, старое длинное имя — alias. Завершённые 01–04 не повторять.

[Договор](05-executor-contract.md) сохраняет все 32 inherited output assignments и добавляет только AGC1/FBM1. Сверка полей result_root/report/candidate/entry/source_url дала **ноль изменений** у inherited строк. Не вводятся конкурирующие generic claims, преждевременные results, новые attempt IDs или второй runner. AST использует назначенный sibling Planning; Wiki/ROPE/AST/NG01 имена отчётов/кандидатов не унифицируются.

Source identities, resolved ссылки при переносе между пакетами и проверки — [concurrent-merged-registry.json](experiments/concurrent-merged-registry.json). [Первоначальный review](experiments/INDEPENDENT-PACK-REVIEW.md) сохранён с явным banner о draft 35/31; действующая очередь 38/34 определяется root04.

## 4. Original RP01 и перенос kNN

[Дополнение к истории](history/05-concurrent-primary-reconciliation.md) учитывает новый primary-readback и текущий DPO→Units анализ. Root непосредственно загрузил два scientific-result.json на 83590; [summary records](history/evidence/original-rp01-primary-summaries.json) сохраняют поля и Git identities. C# mean margin +0.0012275314331054651, ENRU +0.00011000633239746093 — успех соответствующей метрики, при 11/25 и 9/20 индивидуальных ухудшениях и 0/3,0/4 правильных свободных ответов до/после. Ограничения — в [измерениях](02-results-and-candidates.md).

Receipt переноса 27ae032 показывает experimental structural kNN→production. Конкретная «неуспешная матричная функция→успешная kNN-функция» этим receipt не идентифицирована. Current memory supervision из rejected annotations и отдельный neural StepMany маршрут уточняют причинную карту; эти source findings не являются новым measured run.

## 5. Граница действий

Всё согласование — документация и чтение сохранённых источников. Нового обучения, build, generation, исправления production/test/CI и записи в protected не было. При будущем исполнении действуют уже встроенные default model/skills, русский отчёт, любой полезный metric gain с регрессиями рядом и абсолютный немедленный NEXT/STOP. Согласование параллельных документов не разрешает продолжать завершённый опыт.
