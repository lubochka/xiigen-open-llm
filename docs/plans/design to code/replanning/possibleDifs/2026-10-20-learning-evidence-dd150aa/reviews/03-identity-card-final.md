# Независимая проверка IDENTITY-TRAIN-INDEX-01 и финальный readback late delta

Проверяющий: `/root/experiment_audit`. Автор KNN-рецепта — `/root/knn_history`; финальную маршрутизационную поправку внёс `/root`. Проверяющий не автор этих четырёх файлов.

**Вердикт: PASS_BOUNDED_DOCUMENT_REVIEW; необходимая правка NEXT внесена и прочитана в финальном delivery.** Это проверка проекта малого опыта, не аттестация готового host, DLL, результата или product fix.

## Что проверено по содержанию и source

1. Реальное обучение есть: `TranslationMachine.TrainStructuredBatchAsync` вызывает native `ExecutorUnit.TrainOnExample`, затем проверяет identity. Экспериментальный HashSet остаётся именно после Train, поэтому не подменяет известный admission/replay фактор. Предложенные три source изменения сохраняют исходный упорядоченный list/checkpoint и восстанавливают companion index в ImportCheckpoint. [Native writer](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/27.TranslationDomain/TranslationMachine.cs#L107-L145).
2. Материал задан точно: первые 4 прежних содержания, 512 distinct ExampleId/UnitId в128 native batches; затем4 новых содержания по одному batch в A/B. Это8 независимых содержаний, 520 row updates,130 Train calls,96 Flow calls, 2 restore. Не выдано512 повторений за512 независимых знаний. Ожидаемые3 KNN entries на урок соответствуют [трём AddPair](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/27.TranslationDomain/TranslationTrainedUnits.cs#L413-L422); это статическое ожидание, не результат.
3. Оба плеча восстанавливают одни и те же bytes настоящим [LoadCheckpointAsync](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/27.TranslationDomain/TranslationMachine.cs#L611-L650). Требуютсячетыре положительных старых exact ответа, сохранение до/после и усвоение новых четырёх. 0/0 невалидно; gold только post-output. Actual raw, state и quality, а не внутреннее MeasureUnitCoverage, определяют результат.
4. State и ordinary Flow equality проверяются с ограниченными canonical exclusions. Предусмотрены query-no-mutation и frozen-other-owner проверки. Нельзя скрыть изменение ответов за выигрышем стоимости.
5. Построение B index входит в restore; сохраняются restore, Train  и их сумма. Поэтому маленький пакет 4 после H512 может дать Train gain при проигрыше total cost — документ это явно допускает. Peak не складывается; async restore не оценивается неподтверждённым threadcounter. Любое полезное измеренное улучшение своей оси сохраняется с ухудшениями; fixed-order wall/CPU не становится causal speedup.
6. Точные root, команды, входы и действия при конфликте заданы. Полный Program/compare/run ещё не реализован, статус `NEEDS_NATIVE_HOST / NOT_RUN` явно сохранён; prepare внутри Markdown не выдан за выполненную сборку. SDK/модель/проект самостоятельно не подбираются.
7. Default executor, русский отчёт, skills, экспериментальные исключения, protected read-only, кандидат только после результата и немедленный STOP/NEXT присутствуют. Никаких runtime, product edits илиteacher calls для review не потребовалось.

## Единственная необходимая поправка и её закрытие

В первой прочитанной версии CARD/PROMPT текущая точка snapshot `NEXT09` соседствовала с общим «после опыта NEXT», не называя ID после A2. Это могло вернуть будущего исполнителя A2 к №09. В финальном delivery оба файла теперь явно дают ссылку `../../REGISTRY.md`, различают **текущий вход №09** и **после этого ID в полной программе NEXT=IDENTITY-M1-01 (41)**; отдельное поручение одного ID заканчивается `END`. Прямая команда пользователя выше этой последовательности. Правка прочитана, незакрытого конфликта нет.

| Проверенный файл | SHA256 |
|---|---|
| `knn/identity-train-index-01/CARD.md` | `ff446315dbbb3c77b59006ad97764d8d3770985120840d71e72312d5efd4c3f6` |
| `knn/identity-train-index-01/FIXTURE.md` | `65f5f9f224f6b8de3e9fdb17ed7a79c1663def7eee3aab32f174aef81dacbda5` |
| `knn/identity-train-index-01/HOST-BLUEPRINT.md` | `7b4e71b85d07e42fc9e0a89dfc323c40764e94229f75e2517e2346bd0f0f9fe0` |
| `knn/identity-train-index-01/EXECUTOR-PROMPT.md` | `8b665e978c723a21857fd97052b27c7f5290d827deb44ddb85206176c960690e` |

## Late delta: финальная проверка переноса

Семь additions 81141 прочитаны полностью и сверены по Git blob в исходном bounded audit. В delivery текст и JSON побайтно совпадают с завершёнными артефактами проверяющего; их содержание не является независимым review самого автора, а это readback корректного переноса. Документ сохраняет измеренный restore 3/40→4/40 и CONTROL regression, исправляет недоказанные утверждения о первом loss, моментах, универсальном 0/0 и поломке adoption; не выдаёт новые ATTN/JSON рецепты за измеренные. Поздний report не добавил weights 1200 или нового native кода.

| Перенесённый файл | SHA256 |
|---|---|
| `experiment-audit/late-delta-81141.md` | `7e1898a55ff6e4c071b9dbddedc6cf73df83ff0cd4f145dac280d38e7d3e9c8f` |
| `experiment-audit/late-delta-81141.json` | `ee2225ae089e77fc5f3869cc4efb58f0f883b43daa6993afde81f0d86b36edb7` |

Обновлённые README, REQUIREMENTS, SOURCES и REGISTRY отдельно проверены в `review-root-final.md`; они не переименовывают исходные receipts 161+6 в старую проверку 163+8. Старые опыты сохранены; current next 09 и завершение 1–8 обоснованы snapshot. Дополнительных гипотез, запусков и редакций исходных карточек этим readback не создано.
