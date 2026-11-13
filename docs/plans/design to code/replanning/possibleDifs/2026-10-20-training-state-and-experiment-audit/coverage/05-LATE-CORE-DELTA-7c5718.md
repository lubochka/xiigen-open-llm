# Финальная дельта core: три из четырёх прежних пробелов покрытия уже закрыты карточками

**Этот документ имеет приоритет для состояния на `7c57183670ccaf554a47c0a4f44cabadde8c303d`.** Во время аудита core изменился. Старый подробный реестр `62bcc8f...` сохранён как замороженное доказательство, но его вывод «четыре фактора ещё не представлены в protected» больше не является текущим: **RETENTION-M1-01, IDENTITY-M1-01 и LC-REF-CACHE теперь имеют точные protected-карточки**. Из этой конкретной четвёрки только KN-ROUTER canonicalization остаётся без такого рецепта. Покрытие карточкой не означает выполненный эксперимент или готовый host.

## Срез и счётчики

Сравнение [62bcc8f → 7c5718](https://github.com/lubochka/llm_mvp_core/compare/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39...7c57183670ccaf554a47c0a4f44cabadde8c303d) содержит148 изменённых файлов:145 modified и3 added, все внутри protected документации. **Нативные исходники не изменились.** Поэтому адресные findings и source-copy hashes на62bcc8f сохраняют применимость к7c5718, а уже исправленные документные пробелы отозваны ниже. Latest protected tree: `40b4f0750e9a44edbf26e416a20ae39492d66d34`; API вернул `truncated=false`.

| Область | Freeze62bcc8f | Latest7c5718 | Причина |
|---|---:|---:|---|
| Markdown protected |286|289|Две новые карточки и один preregistration |
| Card-файлы |126|128|Добавлены N61-reference и N61-Mode1 |
| P-семейства / P-подслучаи |125 /470|125 /470|P-family механизмы не менялись этой дельтой |
| Промпты P-карточек |133|133|Общие уточнения правил без нового P-подслучая |
| Секции prompt-labelled всего, results исключены |180|182|+1N61-reference +2N61-Mode1 −1дублировавшийся N60-KU |
| Из них содержат поручение |179|181|Прежний пустой heading файла11 остаётся |
| Промпты отдельных N-карточек |1|4|N-KU плюс три новых исполнительских поручения |
| Секции промптов малого пакета |28|27|N60 теперь ссылается на единственный N-KU passport |

Delta-счёт получен из полного списка148 файлов и всех доступных patches, а две новые карточки, актуальные N60/N-KU и preregistration прочитаны полностью и сверены с Git blob SHA. Это адресное завершение аудита, не повторная независимая сертификация всей текущей библиотеки. `late-head-delta.json` хранит счётчики, hashes и current coverage. Старые `catalog.json`/`cards.tsv`/`prompts.tsv` относятся строго к62bcc8f; не подписывать их как полный machine-readable snapshot7c5718.

## Текущий статус четырёх prior factors

| Factor | Current protected coverage | Readiness и измерительные границы |
|---|---|---|
| RETENTION-M1-01 | **Покрыт точно:** [N61-Mode1,100–173][mode1-retention] | Три single-pair lessons наarm;6Train/16Generate total. A0/B0 — после двух общих подготовительных уроков, A1/B1 — после третьего. Отдельно retentionRQ1 и acquisitionRQ2: агрегат может скрыть потерю. Exact source projection/csproj/paths/commands есть; Program.cs ещё инженерное задание, сборка и обучение не выполнены. |
| IDENTITY-M1-01 | **Покрыт точно:** [N61-Mode1,175–250][mode1-identity] | Только same-FlowRef filter финального retrieval, writer/confidence прежние.4Train/12Generate, два независимых пустых исходных состояния. Совпадение body и flow оценивается вместе; один TRAIN-recall запрос обозначен как recall. Cold restore намеренно исключён из сокращённого бюджета. Program.cs не реализован; нельзя называть эксперимент выполненным. |
| LC-REF-CACHE | **Покрыт точно:** [N61-reference][reference] |2пары повторяются на2updates/arm,4updates total,4EvaluateMany,0Generate; измерение actual reference costs включает lookup и cold misses. EvaluateMany — по TRAIN, не heldout accuracy. Host ещё не имеет полного source-linked dependency/capability контракта; карточка прямо требует автора его дописать до передачи runtime исполнителю. Это техническая подготовка, не отсутствие разрешения. |
| KN-ROUTER learned-key canonicalization | **По-прежнему без точного protected-рецепта** | Иной writer `RoutingProfileTrainer` и reader `BaselineKeywordCandidateGenerator`, C#/.NET→csharp/dotnet. Новые N61 карточки не относятся к нему. Prior planning источник и двухурочный опыт сохранены; новый ID текущего пакета не назначается. |

Эти три покрытые механизма остаются находками предыдущих агентов. Текущий аудит не переименовывает их в новые открытия. Переиспользовать последнюю конкретную N61 карточку; не соединять бюджет40/24 ответов старого planning-проекта с новым16/12, не назначать повтор старых архивных результатов.

## N60-E1 остаётся конкретным неисправленным входным материалом

Актуальный [N60-E1:187–219][e1] по-прежнему использует «быстро/quickly» и «тихо/quietly». В native-коде62→7c изменений нет: эти источники не достигают intended ADV marker writer по указанным в correction доказательствам. Новый overlay «спокойно/calmly», «громко/loudly» остаётся актуальным. Это новая конкретизация входов **известного** admission/N57 механизма, а не новая гипотеза.

[N60-E1-native-host-preparation.md](../corrections/N60-E1-native-host-preparation.md) фиксирует11 исходных файлов, extraction blocks, пути, setup script и один selected batch consumer. Статус — `HOST_PREPARATION_REQUIRED`, а не READY. Script поставлен как подготовительный материал и не выполнялся; источник исходников остаётся62bcc8f, потому что live delta только документная.2batch/11Unit calls/16Verify — будущий бюджет, не измеренный результат.

## N-KU-01: новая рубрика и сохранённый старый run passport

Актуальная [полная N-KU карточка][nku] заменила EXAM-gold `ADD_METHOD` на **NO_EXTRA_LOOP**: непустой ответ, отличный от ADD_LOOP. Это точнее отражает вопрос «не советовать второй цикл»; null не считается успехом. По-прежнему прямо указано постоянное majority приk3/four lessons, поэтому даже ожидаемые1/4→3/4 не являются доказательством структурного обобщения. Новые вопросы из executor logs закрыты точными ответами: не импортировать StackDetector/Facade/OperationResult, не вводить reflection/свой entry/compare mode, не пересчитывать Label наEXAM, не удалятьbin/obj. Секция N60-KU заменена ссылкой на единственный полный passport.

Добавлен [preregistration старой подготовки][preregistration]. Он **не является receipt выполненного обучения**. В нём другая уже зафиксированная схема `VariantA/VariantB`, `runner/`, нейтральный warmup и product-copy control после измерений, другие роли. Новая карточка прямо запрещает автоматически мигрировать или повторять уже начатый опыт из-за новой редакции. Эти схемы нельзя смешивать: если именно прежний run уже поручен и начат, сохраняется его версия и собственный бюджет; новый ещё не начатый вариант следует полной актуальной карточке. Historical preregistration не выдаётся за измеренный gain, его extra warmup/control не исчезают из отчёта по старой версии.

## Дополнения пользовательских правил уже внесены в source

В145 изменённых документах добавлены/уточнены правила экспериментальной оболочки и сохранения старых материалов. Для маленького glue допускаются `throw`, `try/catch`, `void`, простые DTO и локальный log format; не требуется перестраивать скопированный алгоритм ради product OperationResult/comment/CI. Ошибка записывается как ошибка с nonzero exit, а не как0% accuracy. Старые source/results/bin/obj не удаляются, не переносятся и не перезаписываются. Новая версия карточки сама по себе не назначает повтор или миграцию уже начатого опыта. Эти исправления уже выполнены внешней документной сессией; не описывать их как нынешние неустранённые blockers.

Остальные выводы карты9 правил остаются в силе с учётом этой дельты: source-copy перед production, useful metric success без обязательной accuracy, конкретный learner либо доказанная data/restore/consumer dependency, protected folder read-only в нашем поручении. Current protected coverage не означает разрешение нынешней аудит-сессии писать туда результаты.

[mode1-retention]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L100-L173
[mode1-identity]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md#L175-L250
[reference]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md
[e1]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L187-L219
[nku]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md
[preregistration]: https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/preregistration.md
