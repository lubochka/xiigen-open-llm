# Фиксированный материал IDENTITY-TRAIN-INDEX-01

Это определение будущего материала; новых внешних DPO и генерации моделью нет. Источник — core commit `dd150aa5216e42b8a8bba14955337398ec5f8397`, каталог `docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/`. Никакие поля InputJson/ChosenOutputJson/RejectedOutputJson/semanticSignature/SourceId/UnitId не сочиняются исполнителем.

| Файл | Git blob | Назначение |
|---|---|---|
| training-batch-B.json | `0c2a3d9a0da31b7277c0d215fb79ddebda49aa6c` | 8 исходных уроков |
| eval-inputs.json | `585bf30cad84b57b2e1f3fe8c00f171f2348c313` | 24 вопроса; 8 exact, 3 near, 8 composition, 5 edge |
| panel-gold.json | `af73d89cfd5b42f954380437bc5596b46dae9498` | Только post-output scorer |
| score.cjs | `e8b20d471667da397e8d5c59ac63b4e7f947540b` | assess/gold неизменны; обе arm-list ABC сократить до AB |

## Содержания уроков, заданные до опыта

| ID | Английский вход | Выбранный русский ответ | Явные части | Фаза |
|---|---|---|---|---|
| tr01 | validate the file | проверь файл | validate → проверь; the file → файл | warm |
| tr02 | delete the cache | удали кэш | delete → удали; the cache → кэш | warm |
| tr03 | copy the report | скопируй отчёт | copy → скопируй; the report → отчёт | warm |
| tr04 | move the config | перемести конфигурацию | move → перемести; the config → конфигурацию | warm |
| tr05 | upload the image | загрузи изображение | upload → загрузи; the image → изображение | главный пакет |
| tr06 | download the package | скачай пакет | download → скачай; the package → пакет | главный пакет |
| tr07 | save the draft | сохрани черновик | save → сохрани; the draft → черновик | главный пакет |
| tr08 | archive the log | архивируй журнал | archive → архивируй; the log → журнал | главный пакет |

Во всех 8 исходных строках: UnitId=`executor-en_ru`, SourceId=`simulation`, SemanticSignature=`SVO`, HeldOut=false. SourceLang=`en`, TopicDomain=`technical`. WordMappings=null. Остальные поля и их JSON-кодирование сохранять из pinned файла, не пересериализовывать внутренние JSON без причины.

## Точный warm generator

Порядок 128 пакетов: g=0..127; в каждом уроки tr01,tr02,tr03,tr04 в данном порядке. Для строки j=0..3 задать r=4*g+j и **ExampleId=`IDX01-W{r:D4}::{originalExampleId}`**. BatchId=`IDENTITY-TRAIN-INDEX-01-warm-{g:D3}`. Первые IDs: `IDX01-W0000::tr01`, `IDX01-W0001::tr02`, `IDX01-W0002::tr03`, `IDX01-W0003::tr04`; последние: `IDX01-W0508::tr01` … `IDX01-W0511::tr04`.

На каждой итерации настоящим A `TranslationMachine.TrainStructuredBatchAsync` обучить пакет 4. Все 512 пар `(ExampleId,UnitId)` различны. Это 512 предъявлений, **4 независимых содержания**. Нельзя вместо этого вставить identities непосредственно в checkpoint, обойти TrainOnExample или повторить старые four IDs.

После W ожидаются 512 identities, 128 training-history записей, exactSourceMap=4, phraseMap=8, KNN entries=1536. Последнее следует из трёх AddPair на каждую строку и append-семантики; это ожидаемые структурные счётчики, не выданный за факт запуска результат. Сохранить native checkpoint в `C:/xgi/idx01/run01/result/W/warm.checkpoint.json`. Если реальные счётчики расходятся, записать расхождение и не продолжать подменённый опыт.

## Главный пакет A/B

Из frozen batch взять только tr05,tr06,tr07,tr08 в исходном порядке. Их ExampleId и все поля без изменения. Новый BatchId одинаков в A и B: `IDENTITY-TRAIN-INDEX-01-new04`. Из одного и того же warm файла оба процесса вызывают настоящий LoadCheckpointAsync. В каждом: 24 before Flow calls; один Train четырёх строк; 24 after Flow calls. После ожидаются identities=516, history=129, exact=8, phrase=16, KNN=1548. Ни один шаг генерации не читает gold.

## Scope и неизмеренное

Ни checkpoint, ни build, ни native call этим документом не созданы. Если warm даёт менее четырёх правильных exact ответов, fixture невалиден для заявленного учебного сравнения. Будущий отчёт должен показать реальные raw до/после; нельзя присвоить ожидаемые здесь счётчики запуску или считать повторные содержания независимыми знаниями.
