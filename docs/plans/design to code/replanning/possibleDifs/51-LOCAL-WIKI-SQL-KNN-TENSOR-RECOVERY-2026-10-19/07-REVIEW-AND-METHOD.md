# Независимая проверка и метод исследования

19 октября 2026. [Оглавление](README.md) · [Приоритет восстановления](00-LOCAL-WIKI-RECOVERY-FIRST.md).

Исследование состояло из сопоставления исходников, архивных результатов и уже написанных исследований. Три отдельных направления — neural/TensorOps, KNN/context и SQL/Units — выполнялись независимо и затем сведены главным автором. **Отдельный reviewer проверял доказательства и расчёты после авторов**, возвращал замечания, не меняя их файлы и не выполняя обучение. Его окончательный отзыв на публикационный пакет — [independent-final-review.md](evidence/independent-final-review.md); перечень проверенных файлов и SHA256 — [reviewed-files.json](evidence/reviewed-files.json).

## Что исправлено после независимой проверки

| Замечание | Почему существенно | Исправление в публикации |
|---|---|---|
| В черновике роли CODEX и автора пары были описаны двусмысленно | Выбор темы не даёт права незаметно переписать финальный rejected | В05 явно разделены выбор задачи/материала, внешний автор обеих сторон, отдельный exact-pair judge и слепой examiner ответов |
| NK-03 можно было прочитать как открытие scope-before-topK | Общий принцип уже есть в P104.1 и прежнем15.03 | В03/06/README новизна ограничена конкретным KnnSkillFilter, confident foreign hits и успешным пустым результатом |
| В SQL prose утверждалось, что appended evidence в fixture полностью обрезан | Финальный выполненный JSON содержит false для append-only collision | Независимо пересчитаны surviving tokens43/48. В02 и evidence README исправлено: prefixes различаются, но полный write list/version не помещаются |
| «Один heldout» мог означать те же вопросы с разными candidate targets | Current generator преобразует TRAIN и HELDOUT; score разных экзаменов не изолирует обучение | Representation intervention только TRAIN; EXAM exact triples/IDs/masks/scorer фиксируются отдельно и остаются одинаковыми |
| Возможный выигрыш от evidence мог быть назван новым усвоением | Полезный контекст способен улучшить ответ при нуле updates | В02/05 добавлены A_before/B_before и после обучения; различаются эффект входа и разность приростов |
| SQL scope был слишком широк | Условный chosen блок действует в signal/stored-procedure ветви, focus curriculum — другой selector | Scope уточнён; counts в current сохранены. Результат не распространяется на все формы SQL-ответов |
| Некоторые ссылочные anchors SQL вели к другой части нужного файла | Правильный файл сам по себе не гарантирует удобную проверку конкретной строки | Добавлены отдельные ссылки selector/shapes/chosen/fixtures/BuildContext/defaults/prefix; общие aliases ведут на файл |
| Четыре плеча по5 эпох выглядели обязательным первым опытом | Слишком рано смешивались вопросы representation и curriculum | Сначала0-update preflight и A00/A10 по1 эпохе/10 updates;2×2 — следующий отдельный вопрос |
| Default training ingress подменил бы авторство нового Q | Wiki ToTrainingBatch выдаёт DeterministicGenerator admission, default pipeline сам строит пары | В05 назван отдельный repository-owned вход с exact pair correspondence; необходимый integration seam отмечен, готовая CLI не заявлена |
| Raw cached files иногда имеют дополнительный LF | Нельзя назвать их exact Git bytes без сверки | Нормализация допущена только при совпадении ожидаемого Git blob SHA; источники206 и corpus1191+7+10 повторно проверены при сборке |

В исходном черновике число47/50 не было приписано correction111; reviewer независимо подтвердил эту важную границу. Сохранены также failed summary106, continuation111/112, current admission dependency и отсутствие ordinary neural reader в рассмотренном deterministic composer path. Исправления не меняют исторических JSON и не объявляют любой receipt ложным. [SQL-досье](02-NEW-SQL-UNIT-GAPS.md).

## Что проверено расчётом

Авторский tensor proxy и независимый NumPy/finite-difference эталон дали один математически правильный MatMul результат [10,20,300,400], dA=[10,10,100,100], dB=[3,7]. Нынешняя modulo формула даёт [10,200,30,400], dA=[10,100,10,100], dB=[4,6]. Для Mean [2,3]→[2] после удаления оси выявлено несовпадение числа backward координат с rank. При этом проверены работающие формы actual RP01 и сохранён статус conditional P2. [Независимые числа](evidence/independent-numeric-source-checks.json), [подробный механизм](04-TENSOR-CONDITIONAL-GAPS.md).

В SQL отдельно сверены24 архивных blob и33 записи primary ledger, включая повторное использование одного исходника в разных assertions. Пересчитаны heldout37/47/47/47/44, chosen-diffs20/0/100,42→12→0 противоречивых read-only строк и состав focus/general. Это не повторное обучение, а независимый разбор исходных receipts. [Независимый SQL результат](evidence/independent-sql-checks.json).

После интеграции главный автор повторно исполнил все три публикуемых Python scripts. Их JSON семантически совпали с результатами авторов; source-verified режим TensorOps проверил4 файла, SQL34. **Native .NET/С#/Roslyn, тренировки, импорт модели и новые free-answer экзамены не запускались.** В окружении не найдены dotnet/csc; Python не использовался для заявления о native исполнении.

## Как использованы запрошенные навыки

Проектные материалы взяты из docs/skills на C4945b8e8; точные пути и hashes включены в [sources.json](evidence/sources.json). Навыки применены к этой исследовательской задаче, а не превращены в требование внедрять следующий PR или запускать чужую кампанию.

| Навык/материал | Применение |
|---|---|
| human-readable-architecture-audit, learning-path-audit | Конкретный learner, изменяемое состояние, вход/выход, ordinary consumer и before/after |
| phase-planning | Разделение восстановления рабочего W, проверки конкретной оси и необязательного последующего расширения |
| plan-review | Отдельный reviewer, falsifier, source-family split, независимость EXAM и замкнутый путь до ответа |
| generated-code-review | Проверка реальных caller/shape, градиентов, default injection/admission, контроль silent fallbacks |
| prompt-patch-authoring | Готовое недвусмысленное задание опыта и точные роли авторства/оценивания |
| experiment-design | Matched state/data/budget, одна ось, baseline и контроли, раздельные механизм/качество |
| human-communication glossary | Домен отделён от языка/направления, доказанный факт — от гипотезы, метрика — от содержания ответа |

Большие phase-planning и plan-review, как и RAG indexes, прочитаны по применимым sections и адресным ссылкам. Полное прочтение каждого большого файла или всего репозитория не заявляется. Родительские RAG guide/master/conformance использованы для поиска первичных документов; фраза из индекса не принималась за доказанный факт реализации.

## Версии и пределы покрытия

Источник текущего кода C4945b8e85c17b5f150eb5b840d16bbdc4b506242 сравнен с W219d7ba89dc3ef43e03e17f1461ad96154d3dd7a. Planning corpus взят на4ca770a2c7354d7985f34c516f087e4b49cd01df. Проверен subsequent core649b9343cfda5b81cc54396a3840ad5db5e0b179: один commit,7 файлов документации,2 новых пути. Программный код не изменился; свежие главы09/10 и карта06 вошли в дополнительную дедупликацию. [Сравнение](evidence/latest-core-compare.json).

Перед записью также получены10 файлов planning delta до4151916fac86becea8b7c2fd8cb6e73abb49cd79, выполнена точная SHA-проверка и отдельная содержательная дедупликация N50/N51. Новые pair/Pos/lookup материалы не переобъявлены нашими. Главный index собран добавлением нового prefix поверх свежего README с сохранением его прежних bytes. [Planning delta](evidence/latest-planning-compare.json), [свежий crosswalk](06-COVERAGE-AND-NOVELTY.md).

Соседние положительные E09/E26/E43 и восстановленные translation memory результаты учитываются через их адресные прежние досье; этот пакет не выдаёт их за новые native runs. Все прошлые и новые известные механизмы связаны в [общей карте06](06-COVERAGE-AND-NOVELTY.md). Точный исторический исполнявшийся бинарник, полная product DI graph условных KNN и все возможные tensor callers остаются за пределами доказанного. Назначены короткие проверки именно этих границ; утверждение «весь алгоритм обучения проверен и полностью исправлен» не делается.

## Область публикации

Публикуются только этот самостоятельный пакет, его evidence и добавление ссылки в начало possibleDifs/README.md в xiigen-planning, ветка llm_plan3. Прежние материалы сохраняются. Product code, исторические checkpoint и запрещённый для изменений llm_mvp_core/docs/plans/replanning-possible-diffs не меняются. Результат этой работы — проверяемые документы и планы, не выполненные исправления обучения.
