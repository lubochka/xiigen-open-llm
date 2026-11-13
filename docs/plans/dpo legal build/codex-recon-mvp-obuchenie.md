# Рекон: Обучение языку (RU) — метод и сеты

Под-вопрос: «Обучение языку (RU): метод/сеты». Сбор evidence для плана ILearningDynamicGraph
(авто-построение DPO для ILearningUnit/Flow/Orchestrator/DynamicGraph). Это НЕ разрешение на реализацию.

## Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/обучение языку/`. Это набор research-логов + готовая
браузерная обучающая игра «Ловушка перевода» (XIIGen Smart Tutor), собранная из DPO-пар переводчика.

Ключевые файлы:
- `обучение языку/1st game/build_dataset.py` — сборщик датасета (флэттенинг DPO-прогонов переводчика → `game-pairs.json`).
- `обучение языку/1st game/curated_ru.py` — 44 курированные пары с ручным разбором на живом русском (пул по умолчанию).
- `обучение языку/1st game/game-pairs.json` — 1849 очищенных DPO-пар, схема `{id,dir,src,ok,bad,type,why}` (~640 KB).
- `обучение языку/1st game/README.md` — описание метода/каналов; авторитетен сам `index.html`, не скрипты.
- `обучение языку/1st game/build_v2.py`, `build_v3.py`, `game_template.html` — ранние сборщики/шаблон базовой версии.
- `обучение языку/1st game/anki-deck.txt` — те же 1849 пар как колода Anki (второй канал охвата).
- `обучение языку/1st game/index.html` и `translation-game.html` — готовая игра (single-file, офлайн); содержит ещё `PROVERBS` (14 public-domain пословиц EN→RU, поля `src,phon,wfw,ok,bad,source,rule`). (НЕ прочитаны построчно — большие/прод-артефакт; описание из README + build_v2.py.)
- `обучение языку/Третий ресеарч.txt` — первоисточник метода и состава сетов (см. ниже).
- `обучение языку/Второй ресеарч.txt` — research открытого кода игр MIT/Apache-2.0 (game loop, проверка ответа, генерация accepted/rejected) — для license-gate.
- `обучение языку/Первый ресеарч.txt` — почти целиком маркетинг (Steam/itch/Reddit/ASO, бренд XIIGen), к методу/сетам нерелевантно.
- Прочее: `ПЛАН-АДАПТИВНЫЙ-МЕНЕДЖЕР-ПРОГРЕССА-v1.md`, `ПЛАН-EXE-ИГРОВОЙ-ДВИЖОК-v1.md`, zip-архивы итераций, `image.png`, `.jpg` — НЕ открывались (вне под-вопроса; защита от перегруза).

## Как устроено/работает (кратко)

Метод обучения = «поймай ошибку перевода» на готовых DPO-парах, без модели и бэкенда на v1:
1. Источник — пары DPO переводчика (chosen/rejected). У каждой пары уже есть: фраза-источник, верный
   перевод (`chosen_translation`→`ok`), правдоподобный неверный (`rejected_translation`→`bad`), тип
   дефекта (`defect_type`→`type`) и разбор (`defect_description`→`why`). То есть «правильный + ловушка + объяснение» уже написаны.
2. `build_dataset.py`: glob по `runs/translation-runs/translator-*/batch-*/translation-pairs-train-50.jsonl`,
   фильтр строк, где есть source+chosen+rejected, дедуп по ключу `(source,chosen,rejected)`, сортировка по
   `(dir,id)`, запись `game-pairs.json`. Перезапуск на новых батчах → обновлённый датасет.
3. Состав сета (Третий ресеарч, стр. 102): сведены 38 батчей (translator-en-ru + translator-ru-en) →
   1849 заданий после дедупликации: 1000 RU→EN и 849 EN→RU; 7 типов ошибок: неверное слово
   (`wrong_word_choice`), калька/дословно (`literal_translation`), грамматика (`grammar_error`), идиома
   (`missing_idiom`), регистр/стиль (`wrong_register`), пропуск/добавление (`omission`), ложный друг (`false_friend`).
4. Игровой цикл (детерминированный, в HTML/JS): выбор направления и типов ошибок → показ source + двух
   вариантов (shuffle ok/bad) → мгновенная проверка → разбор `why` + тег навыка → неверные кладутся в
   очередь повтора (`retry`, due=idx+3); очки/серия/итог со «слабым местом» (макс по `wrongBySkill`).
5. Каналы охвата с нулевой инфраструктурой: HTML на GitHub Pages/Netlify/itch.io; колода на AnkiWeb.
6. Известные ограничения v1 (Третий ресеарч, стр. 114): разборы (`why`) в основном на английском (для
   учащих русский их надо перевести — мелкая задача через свой же переводчик); дистрактор всегда один и
   тот же неверный вариант пары (для повторов можно подмешивать чужие неверные переводы как доп-ловушки).

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- DPO-данные: это конкретный, уже существующий формат «chosen/rejected + тип дефекта + объяснение» —
  прямой кандидат-вход для авто-построения DPO в ILearningDynamicGraph. Поля `src/ok/bad/type/why`
  отображаются на DPO-триплет + ярлык причины + человекочитаемый разбор.
- Teacher-provenance (КРИТИЧНО для Legal Teacher / Provenance Gate скила): пары происходят из
  собственных прогонов переводчика проекта (`runs/.../translator-*`), а НЕ из внешнего teacher напрямую
  здесь. НО как именно эти пары были изначально сгенерированы (какой движок/модель построил
  `chosen/rejected`, провайдер, лицензия, terms) — в этой области не зафиксировано. Это обязательный
  провенанс для решения, можно ли переиспользовать сет как обучающие DPO-строки (поля
  `teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked/closed_proprietary_model`).
  Если первичный teacher окажется closed proprietary (Claude/OpenAI/Gemini), вердикт по скилу —
  `DATASET_REBUILD_REQUIRED` для DPO/дистилляции.
- License-gate внешнего кода: Второй ресеарч даёт кандидатов открытых игровых движков/паттернов
  (Osmosmjerka Apache-2.0, Game2Text Apache-2.0, Acorns Apache-2.0, dutchfordevelopers MIT и др.) с
  предупреждением, что код MIT/Apache ≠ медиа/словари MIT/Apache. Полезно для R5 (модели/сеты в repo) и
  external-source license-gate, но это код-источники, не метод обучения RU.
- Оркестрация/граф/измерение: в этой области ОТСУТСТВУЮТ. Нет ребёр графа, fan-out/fan-in,
  arbiter/reprocessing, ILogical*-контрактов, чекпойнтов, метрик обучения, export/import. Игра —
  детерминированная подача готовых пар; «обучение» здесь = педагогика для человека, не обучение модели.

## Классификация (уровень ILearning* vs детерминистический хелпер)

Артефакты в этой области — НЕ trainable-архитектура; это данные + детерминированный фронтенд:
- `build_dataset.py`, `build_v2.py`, `build_v3.py` — внутренний детерминистический хелпер: ETL/флэттенинг
  и дедуп DPO-строк. Роль по скилу `trainable-architecture-decision-SKILL.md`: corpus labeling/fixture
  generation (Decision Procedure шаг 6) — eval/dataset-prep, не runtime/trainable authority. Вердикт-аналог: `OK_INTERNAL_DETAIL`.
- `game-pairs.json`, `curated_ru.py`, `anki-deck.txt`, `PROVERBS` — датасет/ассеты (банк упражнений), не
  механизм. Для скила это вход «set or row» в Required Dataset Table; пригодность к обучению зависит от
  teacher-provenance (см. выше): без провенанса — `KEEP_FOR_EVAL_ONLY`/кандидат, не `KEEP_FOR_TRAINING`.
- Игровой цикл/проверка ответа в `index.html`/JS (shuffle, сравнение с `ok`, очередь повтора, счёт) —
  внутренний детерминистический хелпер (правило/сравнение), НЕ ILogicalUnit/Flow/Orchestrator/Machine/Pair.
  Никакого learned state, scorer/router/KNN/trained-весов нет; это прямой lookup/сравнение по данным.
  Допустимо только как helper/baseline; не может быть semantic/trainable authority. Вердикт-аналог: `OK_INTERNAL_DETAIL`.
- Уровни ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair в
  этой области НЕ присутствуют. (Decision Procedure §1-9; правило: всё, что не один из allowed levels с
  lifecycle-evidence, — implementation detail.)

## Пробелы / что доисследовать

1. Teacher-provenance первичных пар: где и чем сгенерированы `chosen/rejected` в `runs/.../translator-*`
   (движок, провайдер, model_id, лицензия, terms). Решающее для допуска сета в DPO-обучение. В этой области не найдено — искать в репо-прогонах/раннере переводчика (вне данной папки).
2. Точная схема сырых JSONL: имена полей `source/chosen_translation/rejected_translation/defect_type/
   defect_description/direction/id` подтверждены по `build_dataset.py`; сами файлы
   `translation-pairs-train-50.jsonl` и каталог `runs/translation-runs/` в этой области отсутствуют (надо найти источник батчей).
3. `index.html`/`translation-game.html` построчно не верифицированы (большие single-file артефакты);
   доп-режимы (Собери перевод, Найди ловушку, Память, Микс, XP/уровни, TTS, экспорт Anki) описаны по
   README — нужно прямое чтение, если план будет опираться на конкретную логику режимов.
4. `ПЛАН-АДАПТИВНЫЙ-МЕНЕДЖЕР-ПРОГРЕССА-v1.md` и `ПЛАН-EXE-ИГРОВОЙ-ДВИЖОК-v1.md` не открыты — могут
   содержать намёк на adaptive/learning-логику (ближе к ILearning*); доисследовать при необходимости.
5. Лицензии медиа/ассетов (`image.png`, `.jpg`, шрифты Google Fonts, пословицы) отдельно от кода — для
   полного license-gate (Второй ресеарч прямо предупреждает про расхождение код vs медиа).
6. Связь с языковыми сетами проекта: соотнести с памятью проекта (ru-en-general/level-1/batch-001,
   формат `[WRONG:]`, 50-pair DPO) — совпадает ли источник этих 1849 пар с основным конвейером RU_EN DPO.
