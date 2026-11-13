# Исполнение переносов и точность опытов после P125: аудит и адресный ремонт

Дата: 3 ноября 2026. Научный/кодовый срез core [`9d4d79d27f18`](https://github.com/lubochka/llm_mvp_core/commit/9d4d79d27f18066ce3a346fac2a5eea156304f62), planning [`bd37b3a653ee`](https://github.com/lubochka/xiigen-planning/commit/bd37b3a653eed19ba07c3c5b7690dd8153c8ed82). Документ связывает требования possibleDifs с текущим продуктом и сохранёнными результатами. Это не новый опыт и не переписывание raw.

## Вывод и что сделано

**Наличие правки и теста ещё не означает принятого переноса.** В последних механизмах найдены ошибки подтверждения сохранения кандидата, привязки владельца к фактическим весам и поздней отмены оценки. В опытах встречаются обязательные проверки, которые лишь записываются; контроли, не доходящие до рабочего потребителя; недостаточный учёт стоимости и слишком сильное толкование точного запоминания.

Подтверждённые улучшения сохранены: staged CAS, поздняя отмена training, длинный номер версии, однозначный framing хэшируемых полей, внутрифайловые пары E43, доверенный routing gate, отдельный actual-memory helper, opt-in polarity API и доставка geometry evidence. У каждого назван остаток и уровень доказательства. Продуктовые `.cs`, тесты, сырьё, исторические отчёты и веса этим документным этапом не менялись.

**Исправлены 14 связанных карточек в этом репозитории и22 текущих экспериментальных входа в core.** Новое описание содержит конкретный хороший вход, соседнюю различающую подмену, реального потребителя, обязательное влияние проверки на итог и конечную область расхода. Общие переносные договоры02/03/11 и навигация уточнены отдельно. Полный [аудит142](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-142-post125-transfer-and-experiment-audit-2026-11-03.md), [план143](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-143-post125-addressed-repair-plan-2026-11-03.md), [карта изменений144](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-144-post125-card-coverage-2026-11-03.md), [дополнение codex45 о доказательствах](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/codex-45-post125-evidence-consumer-contract-2026-11-03.md).

## Наиболее важные остатки продукта

| Найденный дефект | Почему прежней проверки недостаточно | Адресное исправление и приёмка |
|---|---|---|
| F125-01: candidate store хэширует UTF-8 без BOM, пишет с BOM | Файл может существовать, но Save возвращает `CANDIDATE_PERSISTENCE_FAILED` без подтверждённого locator. Тест в исходнике не является пройденным тестом | Одни канонические bytes для hash/write/read; настоящий late-conflict, реальная ссылка, загрузка проигравшего состояния без Train. Использовать уже написанные candidate tests |
| F125-02: content identity ошибочно используется как live recipient identity | Два независимых объекта одного checkpoint имеют один reference hash/keyset. У capabilities разные ledgers; обычный issuer передаёт пустого owner | Trusted project→actual recipient перед mutation. A/A проходит; A/B и второй owner через другую capability над A отказывают, сохраняя weights/optimizer/RNG. Собственный B/B проходит. Законное sharing — по существующему договору |
| F125-03: поздняя отмена Evaluate не останавливает публикацию | Token проверен при входе, но после publish barrier не проверен; отменённый ответ может стать ошибкой accuracy | Barrier вызывает Cancel; обычный вход возвращает CANCELLED и сохраняет version/accuracy/history/state. Не повторять уже исправленную late-cancel training |
| Остаток CAS provenance | После проигранного CAS CurrentVersion может уже принадлежать третьему writer | Сохранять actual observed CAS version либо честно переименовать поле; v2 конфликтует, live v3 остаётся v3 |
| F125-13: составной E43 key неоднозначен | `(a :: b,c)` и `(a,b :: c)` становятся одной строкой и дают ложный duplicate | Структурный tuple; два разных потока проходят, одинаковый отказывает. Это ограниченный multi-project API случай, не повтор уже устранённого межфайлового шва |

Источники и точные методы — [продуктовый разбор](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/product-findings.md); [воспроизводимый статический результат](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/product-counterexamples.json). Сравнение .NET UTF-8 подтверждено официальной реализацией и документацией, ссылки приведены там. Статический Python-контрпример не объявлен запуском C#.

## Эксперименты: что подтверждено и что надо исправить

| Опыт | Полезное наблюдение | Ошибка постановки/исполнения/приёмки и конкретный ремонт |
|---|---|---|
| P115/P117 | P115 сохраняет ответы при снижении лексической работы; P117 открывает1/5 строк доступ к памяти при прежних3/5 правильных | Новый finalizer не дополняет старую попытку отсутствующими квитанциями. P117 экспортирует expected journal, actual проверяет только count; нужен live export и независимое полное сравнение |
| P118 | Первый режим использует выученное тело плана, второй теряет его; экспериментальная правка возвращает payload | Семантическая рубрика принимает запрет обеих операций и обратный порядок. Проверить тот же oracle на контрастах смысла; обязательные copy/blindness/calibration/cold outcomes должны менять final verdict |
| P119 | Learning rate даёт примерно втрое больший относительный сдвиг весов; ответы остаются2/4 | Первая неуспешная попытка сохранила VALID/SUPPORTED. Обе оплачены:16 updates/160 предъявлений суммарно. Один итог связывает реальные обязательные проверки, бюджет, статусы и exit |
| P120 | Законные переходы2→4; literal correctness2/4→3/4 с названным дефектом gold | 18 прямых+18 вложенных Predict=36 при cap20. PP+24,2682968272% относится к разным vocab/потокам; не объявлять её общим ухудшением одной задачи. Считать вызовы на нижней границе, фиксировать общий eval-domain до сравнения |
| P121 | Правильный адрес обновления0/1→1/1 в своём локальном реестре | Восемь optimizer/RNG fingerprints пусты. Содержательная сериализация, actual before/after и обязательный отказ UNKNOWN. Ответы `+,+,-,-`,2/4 до/после; это не новая предметная способность |
| P122 | Различение пар1/10→10/10; сохранность символов0/10→9/10; скалярный вклад вырос | `over32` состоит из неизвестных roots и занимает лишь2 фиксированных slots. Pure capacity fixture должен доходить до32/33; scientific sharedVocab/фактор не менять. Same-count roots ограничивают вывод о семантике |
| P123 | На четырёх признаках A2/4→B4/4, C0/4; законный перенос координат сохраняет representation | NaN проверяется отдельным AllFinite, а рабочий oracle возвращает Pass. Provenance проверяет answer, не raw geometry. Один checker проверяет raw→bridge→features/gold с согласованным self-hash; original B:e1 принимается |
| P124 | A5/6→B6/6, C5/6; прирост на exact новом урокеe3 | Aggregator sensitivity сравнивает REFUSED→REFUSED; labelSensitive не входит в validity. Начать с полного ACCEPTED, порча даёт REFUSED. A уже обучено двум урокам, e4 идёт от nearest neighbour r1, поэтому слово «необученное/default» исправлено |
| Q125 = P25-LEARNED-RESTORE-02/P12501 | Реальный новый процесс:0/24→19/24→19/24,24/24 literal/route/confidence/verdict parity;1batch8lessons72Flow | Частичный comparer игнорирует topic/full KNN/provenance; его controls вызывают другие функции. Полный independent comparer exact/phrase/KNN с кратностью принимает оригинал и отказывает каждой порче; итог зависит от него. Run относится к старой DLL, не свежему HEAD |

Точные первичные числа, единицы, временные области, source links и пересчёт: [таблица и разбор](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/experiment-findings.md), [P123/P124](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/negation-geometry-findings.md), [машинные результаты](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/experiment-recount.json). В P123 оплачены обе попытки:4Train/16lessons/32KNN/4scaffold. Нельзя назвать первый полный запуск бесплатным из-за ошибки финализатора. Пики памяти разных процессов не складываются как одновременные.

### Последняя поправка по отрицаниям сохраняется

В Q125 TRAIN не содержит запретов и вопросов. Эти края показывают наблюдённые ответы со снятым отрицанием/вопросительностью, но **не измеряют способность выучивать** такие случаи. Одинаковая0,99 у19 правильных и3 неправильных ответов в каждой trained/restored панели — отсутствие различения на этой панели, не универсальная калибровка.

Снятое владельцем предложение продуктового отказа по наклонению не возвращено в очередь. Новый опыт про отрицание другим архитектором здесь не назначен. Polarity API в `12.SemanticCompiler` честно отделён: новый opt-in путь написан, действующий learner не подключён, нулевая цена обучения не измерена. Geometry bridge тоже засчитан именно как evidence path; продуктовый обученный critic геометрию пока не потребляет.

## План исполнения: одна существующая очередь

План143 содержит13 адресных пакетов, но их номера — ссылки на остатки, **не новая очередь**. Первый текущий перенос/Q01 — существующий `135-R01-CONCURRENCY`: ремонт candidate byte-confirmation, provenance и назначенной приёмки с поздней отменой оценки. После него переход по «Далее» в [01-registry](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next), без самовольного возобновления приостановленных кандидатов. Прямой ID пользователя имеет приоритет.

Работы T121-OWNER/BINDING/SHARING остаются привязаны к своим владельцам. Ошибки финализаторов и независимых comparer сначала чинятся чтением сохранённых доказательств. Для каждой проверки нужен полный честный positive и одна адресная порча; оба идут через тот же working checker. Нет необходимости вновь обучать модель, чтобы проверить потерянное поле JSON, cap builder, смысловый контрпример или зависимость exit от false.

Состояние, отсутствующее в старом evidence, называется UNKNOWN. Новый accepted derived assessment хранит исходные SHA и не меняет raw. Нативная приёмка текущего продукта должна выполняться по конкретному назначенному host/fixtures/cap; в этой среде нет dotnet/pwsh, и зелёный C# прогон не заявлен. Существующие нативные тесты кандидата переиспользуются, а не дублируются новым набором похожих проверок.

## Какие проектные карточки усилены

| Карточка | Собственная проверка |
|---|---|
| [03-DPO-PAIR-AND-GRADIENT.md](03-DPO-PAIR-AND-GRADIENT.md) | SAME-CHECKPOINT-DIFFERENT-RECIPIENT |
| [07-CONTINUAL-REPLAY.md](07-CONTINUAL-REPLAY.md) | ACTUAL-STREAM-PROVENANCE |
| [10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | RESTORE-ACTUAL-CHECKPOINT |
| [22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | LIVE-KNN-EXPORT-IS-NOT-EXPECTED-JOURNAL |
| [27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md](27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md) | NESTED-PREDICT-AND-COMMON-METRIC |
| [27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | NESTED-PREDICT-AND-COMMON-METRIC |
| [34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md](34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md) | ACTUAL-STREAM-PROVENANCE |
| [34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md](34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md) | PAIR-TUPLE-AND-ORIGIN |
| [35-MICRO-NEGATION-WORDMAP-CONFIDENCE-FACTORIAL-2026-10-16.md](35-MICRO-NEGATION-WORDMAP-CONFIDENCE-FACTORIAL-2026-10-16.md) | RESTORE-ACTUAL-CHECKPOINT |
| [35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md) | RESTORE-ACTUAL-CHECKPOINT |
| [39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md](39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) | FRAMED-BYTES-AND-CONSUMED-MATERIAL |
| [54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) | RAW-GEOMETRY-TO-WORKING-ORACLE |
| [59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md](59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md) | ACCEPTED-TO-REFUSED-AND-TRAINED-BASELINE |
| [59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md](59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md) | LR-FACTOR-CAMPAIGN-ACCOUNTING |

В14 planning-файлах добавлены только новые верхние уточнения. В17 core-карточках заменён прежний текущий prompt, в5 добавлен новый; исторические паспорта сохранены. После независимого review дополнительно исправлены real RawButton fields, semantic mutation с согласованным digest, same-checkpoint acceptance outcomes, multiplicity checkpoint и реальные границы будущих карточек.

## Проверка пакета

371 уникальный адресный source/result файл сверен по Git blob SHA и SHA256. Три Python-прохода повторно выполнены корневым аудитором: результаты совпали, новых native/model/train calls0. [Опись](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/source-manifest.json), [команды воспроизведения](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/README.md), [итоговая проверка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/validation.json). Это адресный аудит, не заявление о проверке всего репозитория или всех130 опытов.

Основная научная область этого документа заканчивается P125 на9d4d79d. Новые публикации учитываются отдельной дельтой при сохранении их данных и актуального NEXT; они не присваиваются этому аудиту как заново принятые результаты.

## Публикационная дельта: Q126 уже записан

До записи этого аудита ветка продвинулась с9d4d79d до [3eed42a80a4f](https://github.com/lubochka/llm_mvp_core/commit/3eed42a80a4fc202bfe281349c08435097664bb1). ДобавленP12601 и обновлена его авторская строка очереди. Эти файлы и строка сохранены, основной кодовый/научный срез142 не переназначен задним числом. Теперь **126/130 записанных**, следующий незаписанный **Q127/KNN-ORDER-PERMUTATION-01**, без разрешения запуска.

Адресно прочитаны опубликованные отчёты, arm результаты, runtime/restore и паспорт. В attempt-0002 и0003 выполнено по14updates, суммарно28; attempt-0001 остановился на сборке. Исходная буквальная гипотеза о новых правильных ответах не выполнена: обученный старт уже стоял на потолке. Сохранение ответов и рост margin — полезные наблюдения. Использованы DLL от16 октября и factor copies, не свежая сборка продукта. Подробная отдельная [проверка публикационной дельты](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-03-post125-audit/p126-publication-delta.md) различает эти факты и остатки бюджета/финализатора; полный новый научный аудитP126 не заявлен. Ошибки finalizer относятся к уже назначенному классуF125-04, без новой очереди и без повторного Train.

## Согласование публикации с5a885d6c

Следующая параллельная дельта [5a885d6c9037](https://github.com/lubochka/llm_mvp_core/commit/5a885d6c90378fcc38f66338d80e652f3316f12c) добавила правила native acceptance, обновления навыков, правку тестового вызова и отдельную DESIGNED_NOT_RUN карточкуP131. Три общих документа объединены автоматически, в индексе и тестовом договоре сохранены обе совместимые части после адресного разрешения конфликта. Чужие source/артефакты и строкаQ126 не перезаписывались. Научный pin9d4d79d и отдельный P126pin3eed42a остаются прежними; это не новая аттестация изменённого теста или всех навыков.

Актуальный счёт теперь **126/131**, следующий обычный шаг **Q127**. P131 следует послеQ130 либо по прямому назначению пользователя; этот аудит его не запускает и не возвращает снятый продуктовый отказ по отрицанию. Основной договор45 требует native discovery/build/tests для уже разрешённой реализации; наше дополнениеcodex45 и план143 согласованы с ним. NATIVE_UNRUN допустим как граница этого документного аудита, но не закрывает последующую порученную реализацию.
