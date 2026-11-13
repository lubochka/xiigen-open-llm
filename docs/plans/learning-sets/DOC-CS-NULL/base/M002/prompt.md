# Создание одной партии DPO в Kiro — CREATE_ONLY

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Наборы создаются только под `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`; инструкции — в соседнем `preparation`. Текущая папка Kiro или `llm_mvp_core` не меняет эти значения. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Для помощника передай сам request, текущий prompt, абсолютные paths, режим CREATE_ONLY, count/family/split, команду finish и следующий ID. После compaction вызови готовую команду `resume` из `commands.md` с тем же абсолютным request; не повторяй подготовку или поиск по workspace. Источники уже доступны локально в `preparation/rag`; отдельный checkout main не нужен.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В коротком отчёте создания (`report.creation.md`, сохраняемом finish) укажи смысл содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит точную команду finish для структурного завершения и resume для продолжения, включая обнаруженный абсолютный `request.paths.python_executable`. Не заменяй её предположением, что `python` доступен в PATH. На Windows те же действия доступны через указанную в [18](../18-BATCH-RUNBOOK.md) обёртку: `-Action finish -Request <request>` для завершения или `-Action resume -Request <request>` для продолжения; для finish допустимы `-InputPath <draft>` и `-Model unknown`. Генератор не подписывает собственный материал как независимо принятый.

## Содержание каждой пары

Используй формат `xiigen.dpo.v1`. JSONL: один объект на строку, без Markdown-ограждения. Каждая строка содержит:

```json
{
  "schema_version": "xiigen.dpo.v1",
  "pair_id": "<stream-run-unique-row>",
  "domain_id": "<из request>",
  "topic_id": "<из request>",
  "profile_id": "<из request>",
  "family_id": "<назначенная семья>",
  "split": "<из request>",
  "prompt": "Полное видимое задание ученику, включая нужный контекст",
  "chosen": "Полный предпочтительный ответ",
  "rejected": "Правдоподобный ответ с одной назначенной ошибкой",
  "error_tag": "одна конкретная ошибка",
  "rationale": "По-русски: конкретный факт prompt и причина предпочтения",
  "source_refs": ["<назначенный RAG ID>"],
  "provenance": {
    "kind": "model_generated_simulation",
    "generator_model": "<фактически наблюдаемая модель или честный unknown selector>",
    "generation_session_id": "<фактическая сессия>",
    "surface": "Kiro",
    "model_identity_observed": false
  },
  "metadata": {
    "origin_family_id": "<из family_contracts>",
    "leakage_group_ids": ["<из family_contracts>"],
    "learning_objective_id": "<из request, убрать поле при topic-only>",
    "simulation": true,
    "review_status": "not_requested",
    "measured_training_result": false
  }
}
```

Это шаблон полей с заполнителями, не готовая строка. Машинная схема и конкретный request включены ниже; все topic/profile/family значения берутся из request, а не из примера. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — сохрани неготовый случай отдельно в `quarantine.jsonl` этой попытки с причиной и укажи недостающий материал. Quarantine не считается созданной парой и не входит в count. При отсутствии source-файла назови конкретный ID/path и используй назначенную карту происхождения; не создавай пустой manifest, фальшивую RAG-выдержку или текст под ожидаемый hash.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: сохраняй известное родство сейчас; последующая проверка скрытых парафразов назначается отдельно и не запускается этой сессией.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Будущая отдельно порученная приёмка поведения использует небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Сейчас ни oracle, ни глобальные production-тесты не запускаются: создаётся кандидат по полному видимому требованию.

## Завершение создания и переход

После сохранения ровно `count` строк выполни готовую команду `finish` из `commands.md`, сохранив абсолютные пути интерпретатора/request/draft и заменяя `unknown` только наблюдаемым именем модели. Отдельный `check` не нужен: finish проверяет JSON/контракт/количество и структурные связи; он не оценивает смысл независимо, не вызывает provider и не обучает. При ошибке исправляй только создаваемый файл с сохранением прежней версии. Не создавай новый run ради обхода структурной ошибки и не выдавай неполный результат за завершённый.

Успешный finish сохраняет `state.json` и `report.creation.md` (новая версия вместо затирания имеющегося отчёта). `DRAFT_CREATED` означает созданных кандидатов. Сразу переходи к следующему stream из registry; `review_status=not_requested`, независимая смысловая приёмка не выполнена, Student `NOT_RUN`. **Запрещено автоматически создавать blind/review, вызывать отдельного reviewer, делать accept/export или ждать их перед следующим набором.** Сохранённые описания этих этапов используются только по отдельному последующему заданию.

Короткое сообщение по-русски: «Создано N из N кандидатных пар по теме …; файлы …; структурная проверка завершена; следующий …». Если число неполное, назови фактическое число и точную причину. Не добавляй вымышленные оценки смысла, скорость Student, стоимость или обучение. Имя модели/session остаётся unknown, если оно не наблюдалось.

## Контекст и ожидание

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`; точный `next_stream_id` также возвращают finish/resume. Дополнительный поиск runner или модели для перехода не нужен.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.


## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "DOC-CS-NULL/base",
  "topic_id": "DOC-CS-01",
  "objective_id": "DOC-CS-NULL",
  "domain_id": "docs.csharp",
  "profile_id": "base",
  "run_id": "M002",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-CS-NULL/base/F0000001",
    "v3/DOC-CS-NULL/base/F0000003",
    "v3/DOC-CS-NULL/base/F0000004",
    "v3/DOC-CS-NULL/base/F0000006",
    "v3/DOC-CS-NULL/base/F0000007",
    "v3/DOC-CS-NULL/base/F0000009",
    "v3/DOC-CS-NULL/base/F0000011",
    "v3/DOC-CS-NULL/base/F0000012",
    "v3/DOC-CS-NULL/base/F0000013",
    "v3/DOC-CS-NULL/base/F0000014"
  ],
  "family_contracts": {
    "v3/DOC-CS-NULL/base/F0000001": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000001",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000003": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000003",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000004": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000004",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000006": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000006",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000007": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000007",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000009": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000009",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000011": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000011",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000012": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000012",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000012"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000013": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000013",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-CS-NULL/base/F0000014": {
      "origin_family_id": "v3/DOC-CS-NULL/base/F0000014",
      "leakage_group_ids": [
        "v3/DOC-CS-NULL/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 15,
  "sources": [
    "RAG-027",
    "RAG-028"
  ],
  "card": "curriculum/cards/DOC-CS-NULL.md",
  "output_directory": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-CS-NULL/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-CS-NULL.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002\\request.json",
    "prompt": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002\\prompt.md",
    "draft": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002\\draft.jsonl",
    "state": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002\\state.json",
    "commands": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002\\commands.md",
    "resume": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\DOC-CS-NULL\\base\\M002\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-028": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-028.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "DOC-CS-NULL",
  "topic_id": "DOC-CS-01",
  "title_ru": "Документировать подстановку только вместо null",
  "learning_goal_ru": "Различать null, пустую строку и пробельную строку при null-only fallback; не придумывать очистку или проверку длины.",
  "generation_prompt_ru": "Создавай только объяснение null-only fallback для строк. Небольшой полностью показанный метод использует ?? либо явную проверку is null. Никаких БД, async, внешних callee, логирования, маршрутизации или исключений. Для каждого prompt перечисли 3–4 точных входа, обязательно null, \"\" и \" \". Chosen перечисляет точные результаты и правило подстановки. Rejected добавляет ровно одну отсутствующую обработку: empty-as-null, whitespace-as-null либо trim. Сохраняй остальные результаты верными. Пустые и пробельные значения показывай JSON-кавычками, чтобы пробелы были видимы.\n\nРаботай только с данным objective_id и назначенными family_id/split/count из request.json. Пиши одну смысловую пару на назначенное семейство; chosen и rejected должны иметь одинаковый формат и отличаться одной проверяемой ошибкой. Все сведения, версии, исходные данные и договор ответа помещай в prompt: metadata learner не получает. Примеры ниже — открытые авторские reference seeds, не закрытый EXAM, не сгенерированный китайской моделью корпус и не измеренные результаты. Не копируй reference-строки в TRAIN под новым ID. Переименование переменных/таблиц и замена чисел не делают шаблон новым независимым семейством. Для документации объясняй именно показанный код, для генерации возвращай только запрошенный код. Не вставляй в ответы labels, rationale, source ID и предпочтительную позицию. Oracle expected — заранее назначенное ожидание; статус остаётся NOT_RUN, пока соответствующий runtime действительно не выполнен. Runtime oracle сейчас не запускай; сохрани его ожидания как NOT_RUN, не выдавай их за наблюдённый результат. Не переходи к другой теме внутри пакета. При NEXT немедленно используй следующий ID из реестра, при STOP прекрати новые действия. Production и старые эксперименты не менять; отчёт на русском по 08-REPORT-TEMPLATE.md, фактическая текущая Kiro-модель без override.",
  "response_contract_ru": "Русский текст: точные результаты в порядке входов, затем одно предложение о правиле. Без нового кода.",
  "scenario_axes": [
    {
      "axis": "форма",
      "values": [
        "??",
        "явный if (s is null)",
        "тернарный s is null ? fallback : s"
      ]
    },
    {
      "axis": "вход",
      "values": [
        "null",
        "пустая строка",
        "один пробел",
        "обычный текст с краевыми пробелами"
      ]
    },
    {
      "axis": "fallback",
      "values": [
        "литерал",
        "явный аргумент"
      ]
    }
  ],
  "forbidden_mixing": [
    "trim/normalization как дополнительная задача",
    "async/cancellation",
    "побочные эффекты",
    "кодогенерация вместо документации"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-CS-NULL.REF.01",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.literal.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Внутри класса объявлен метод: string Label(string? s) => s ?? \"(нет)\"; Других операций нет. Опиши результаты вызовов Label(null), Label(\"\"), Label(\" \"), Label(\"A\") в таком порядке и правило подстановки.",
      "chosen": "Результаты: \"(нет)\", \"\", \" \", \"A\". Подстановка выполняется только для null; пустые и пробельные строки сохраняются.",
      "rejected": "Результаты: \"(нет)\", \"(нет)\", \" \", \"A\". Подстановка выполняется для null и пустой строки; пробельная строка сохраняется.",
      "error_tag": "empty_treated_as_null",
      "rationale": "Оператор ?? выбирает fallback только при null; дополнительная проверка длины отсутствует.",
      "counterexample": "Label(\"\") возвращает строку длины 0, не \"(нет)\".",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.literal.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.literal.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "input": null,
              "expected": "(нет)"
            },
            {
              "input": "",
              "expected": ""
            },
            {
              "input": " ",
              "expected": " "
            },
            {
              "input": "A",
              "expected": "A"
            }
          ],
          "assertion": "Exact ordinal equality of every returned string; record engine version and exit status."
        }
      }
    },
    {
      "example_id": "DOC-CS-NULL.REF.02",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.parameter.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Метод: string Pick(string? value, string fallback) { if (value is null) return fallback; return value; } fallback всегда не-null. Опиши результаты Pick(null,\"guest\"), Pick(\"\",\"guest\"), Pick(\" \",\"guest\") в этом порядке.",
      "chosen": "Результаты: \"guest\", \"\", \" \". Метод заменяет только null на переданный fallback и не проверяет пробельность.",
      "rejected": "Результаты: \"guest\", \"\", \"guest\". Метод заменяет null и строку из пробела на переданный fallback, оставляя пустую строку.",
      "error_tag": "whitespace_treated_as_null",
      "rationale": "В методе только проверка is null; пробел является полноценным значением.",
      "counterexample": "Pick(\" \",\"guest\") возвращает ровно один пробел.",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.parameter.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.parameter.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "args": [
                null,
                "guest"
              ],
              "expected": "guest"
            },
            {
              "args": [
                "",
                "guest"
              ],
              "expected": ""
            },
            {
              "args": [
                " ",
                "guest"
              ],
              "expected": " "
            }
          ],
          "assertion": "Use ordinal string comparison; whitespace length must remain 1."
        }
      }
    },
    {
      "example_id": "DOC-CS-NULL.REF.03",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.preserve-spaces.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Метод: string Keep(string? s) => s is null ? \"default\" : s; Опиши результаты Keep(\" A \"), Keep(\"\"), Keep(null) в этом порядке. Показанный метод полный, вызова Trim нет.",
      "chosen": "Результаты: \" A \", \"\", \"default\". Ненулевое значение возвращается без изменения пробелов.",
      "rejected": "Результаты: \"A\", \"\", \"default\". Ненулевое значение возвращается после удаления краевых пробелов.",
      "error_tag": "invented_trim",
      "rationale": "Ни одна ветвь не удаляет символы; возврат s сохраняет точную строку.",
      "counterexample": "Для \" A \" длина результата 3, не 1.",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.preserve-spaces.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.preserve-spaces.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "input": " A ",
              "expected": " A "
            },
            {
              "input": "",
              "expected": ""
            },
            {
              "input": null,
              "expected": "default"
            }
          ],
          "assertion": "Compare code points and lengths, not Trim() values."
        }
      }
    },
    {
      "example_id": "DOC-CS-NULL.REF.04",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.empty-default.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Метод: string Value(string? s) => s ?? \"\"; Опиши результаты Value(null), Value(\" \"), Value(\"B\") в таком порядке. Укажи точный текст, не только наличие значения.",
      "chosen": "Результаты: \"\", \" \", \"B\". Null заменяется пустой строкой, остальные значения сохраняются.",
      "rejected": "Результаты: \"\", \"\", \"B\". Null и строка из пробела заменяются пустой строкой, остальные значения сохраняются.",
      "error_tag": "whitespace_collapsed",
      "rationale": "Fallback применяется при null, но пробельная строка не null.",
      "counterexample": "Value(\" \") содержит один пробел; сравнение после Trim скрыло бы ошибку.",
      "source_refs": [
        "RAG-027",
        "RAG-028"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.empty-default.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.empty-default.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "input": null,
              "expected": ""
            },
            {
              "input": " ",
              "expected": " "
            },
            {
              "input": "B",
              "expected": "B"
            }
          ],
          "assertion": "Null input result length 0; whitespace input result length 1."
        }
      }
    }
  ]
}

## Полный контракт строки: не искать другую схему

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "XIIGEN candidate DPO row, creation stage only",
  "description": "One JSON object per JSONL line. Structural conformance does not establish semantic quality, native admission or learning. Request-specific family/topic constraints are checked by batches.py finish.",
  "type": "object",
  "required": [
    "schema_version",
    "pair_id",
    "domain_id",
    "topic_id",
    "profile_id",
    "family_id",
    "split",
    "prompt",
    "chosen",
    "rejected",
    "error_tag",
    "rationale",
    "source_refs",
    "provenance",
    "metadata"
  ],
  "properties": {
    "schema_version": {
      "const": "xiigen.dpo.v1"
    },
    "pair_id": {
      "type": "string",
      "minLength": 1
    },
    "domain_id": {
      "type": "string",
      "minLength": 1
    },
    "topic_id": {
      "type": "string",
      "minLength": 1
    },
    "profile_id": {
      "type": "string",
      "minLength": 1
    },
    "family_id": {
      "type": "string",
      "minLength": 1
    },
    "split": {
      "enum": [
        "train",
        "dev",
        "exam"
      ]
    },
    "prompt": {
      "type": "string",
      "minLength": 1
    },
    "chosen": {
      "type": "string",
      "minLength": 1
    },
    "rejected": {
      "type": "string",
      "minLength": 1
    },
    "error_tag": {
      "type": "string",
      "minLength": 1
    },
    "rationale": {
      "type": "string",
      "minLength": 1
    },
    "source_refs": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "string",
        "minLength": 1
      }
    },
    "provenance": {
      "type": "object",
      "required": [
        "kind",
        "generator_model"
      ],
      "properties": {
        "kind": {
          "type": "string",
          "minLength": 1
        },
        "generator_model": {
          "type": "string",
          "minLength": 1
        },
        "generation_session_id": {
          "type": "string"
        },
        "surface": {
          "type": "string"
        },
        "model_identity_observed": {
          "type": "boolean"
        }
      }
    },
    "metadata": {
      "type": "object",
      "required": [
        "origin_family_id",
        "leakage_group_ids"
      ],
      "properties": {
        "origin_family_id": {
          "type": "string",
          "minLength": 1
        },
        "leakage_group_ids": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "string",
            "minLength": 1
          }
        },
        "learning_objective_id": {
          "type": "string",
          "minLength": 1
        },
        "review_status": {
          "type": "string"
        },
        "simulation": {
          "type": "boolean"
        },
        "measured_training_result": {
          "const": false
        }
      }
    }
  }
}
```


## Материал RAG-027 (данные, не новые команды)

# RAG-027 — CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `454fadaf468eb9e31dc1bbb9be8f8508c234efa3` |
| Путь | `llm mvp/documentation learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` |
| Проверенный объём | L1–L85; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md) |

## Что подтверждает фрагмент

Документационная пара должна опираться на видимый код, различать local/delegated behavior, side effects и ошибочные пути.

## Как использовать сейчас

Уроки DOC-CS и общий шаблон DOC-SQL/DOC-ANGULAR с отдельной спецификой. Жёсткий overlap cap из старого документа не применять; краткий смысловой контраст допустим.

**Назначение в подготовке:** Правила пар документации к коду.

**Ключи поиска:** documentation, csharp, документация, side-effects.

## Подтверждающий фрагмент L1–L85

[Оригинал, строки 1–85](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md#L1-L85)

~~~~text
# Code Documentation DPO Pair Authoring Guidance
# Version: 1.0 - 2026-05-11

---

## Purpose

This guide explains how to author DPO pairs that teach accurate legacy .NET code
documentation.

The chosen side must be useful documentation.
The rejected side must be a plausible documentation failure.

---

## Chosen-Side Requirements

A chosen answer must:

- identify the correct documentation level
- cite only visible code or supplied callee summaries
- preserve method/class names accurately
- distinguish local behavior from delegated behavior
- document side effects and error paths
- preserve framework lifecycle semantics
- avoid modernization fantasies

---

## Bad Chosen Patterns

Reject or repair chosen sides that:

- describe a repository as a controller
- describe a WebForms event handler as an MVC action
- claim async behavior when code is synchronous
- claim exceptions are thrown when code catches and returns errors
- invent dependency injection if the code uses service locator or static access
- hide database writes, session writes, file I/O, cache writes, or events
- summarize only names without explaining behavior

---

## Good Rejected Side

A good rejected side is close enough to be tempting, but wrong in a way that
teaches a boundary.

It should be wrong because of one primary defect:

- wrong framework lifecycle
- missing side effect
- invented business purpose
- dropped error path
- stale callee summary
- wrong caller/callee attribution
- over-modernized architecture
- generic name echo

---

## Overlap Guidance

Target rejected overlap with chosen:

- 0.25-0.55 for normal pairs
- below 0.60 always

Do not use a rejected side that is just the chosen side with "not" inserted.

---

## A-Side Evidence Requirements

Every A-side must include enough evidence for the chosen side:

- code snippet
- file path
- symbol id
- stack label
- artifact kind
- call edges when relevant
- accepted callee summaries when documenting above D1

If the evidence is not present, the pair must not target that claim.
~~~~



## Материал RAG-028 (данные, не новые команды)

# RAG-028 — CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `6f8f48d7125e24fed8d8caee69954857583ea8b1` |
| Путь | `llm mvp/documentation learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md` |
| Проверенный объём | L7–L54, L111–L161; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md) |

## Что подтверждает фрагмент

Маршрутизация учитывает runtime/framework/version и уровень D1–D4; обобщение caller требует видимых callee summaries.

## Как использовать сейчас

Уроки DOC-CS: не путать .NET Framework и .NET Core, AngularJS и Angular; не выдумывать hidden callee. Source version фиксируется до генерации.

**Назначение в подготовке:** Версионность стека и доказательство переходов вызовов.

**Ключи поиска:** documentation, version, call-graph, версии, csharp, angular.

## Подтверждающий фрагмент L7–L54

[Оригинал, строки 7–54](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L7-L54)

~~~~text
## Purpose

This guide closes the gap between broad "document this project" routing and the
actual documentation behavior needed for legacy C# systems.

The documentation flow must route by:

- server/client side
- .NET runtime family
- target framework version
- inferred C# language version
- web/UI framework and version when present
- artifact kind
- documentation level D1-D4
- bottom-up call transit state

It is not enough to choose "C# server documentation." A .NET Framework 4.6
WebApi2 controller with AngularJS 1.4 client code must not be documented as an
ASP.NET Core 3.1 controller, and an ASP.NET Core 3.1 controller must not be
documented as a WebForms page.

---

## Required Version-Aware Routing Fields

Every source row, request, DPO pair, held-out row, and report must include:

```json
{
  "app_side": "server | client | shared | mixed",
  "runtime_family": "NET_FRAMEWORK | NET_CORE | NET_STANDARD | UNKNOWN",
  "target_framework": "net46 | netcoreapp3.1 | netstandard2.0 | ...",
  "target_framework_source": "csproj | packages.config | project.assets.json | inferred",
  "csharp_language_version": "CSharp5 | CSharp6 | CSharp8 | explicit:<value> | inferred_unknown",
  "language_version_confidence": "exact | inferred_high | inferred_review_required",
  "project_style": "legacy_non_sdk | sdk_style | mixed",
  "web_framework": "WebForms | WebApi2 | ASP.NET Core MVC | ASP.NET Core WebApi | ASMX | none | unknown",
  "web_framework_version": "System.Web.Http 5.2.3 | Microsoft.AspNetCore.App 3.1 | ...",
  "frontend_framework": "AngularJS | React | WPF | WinForms | none | unknown",
  "frontend_framework_version": "1.4.8 | ...",
  "stack_label": "NETFX_WEBAPI | NETCORE_WEBAPI | DATA_ACCESS_EF | ...",
  "artifact_kind": "method | controller_action | helper_method | repository_method | page_event | workflow",
  "documentation_level": "D1 | D2 | D3 | D4"
}
```

If a value is inferred, the evidence must name the file and field that produced
the inference.
~~~~

## Подтверждающий фрагмент L111–L161

[Оригинал, строки 111–161](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L111-L161)

~~~~text
## Bottom-Up Call-Transit Gate

Documentation must be built like a compiler pass.

### Stage T1 - Leaf Method Summary

Document methods whose behavior is locally visible and does not require a known
callee summary. The output becomes an accepted `CalleeSummary`.

### Stage T2 - Helper Or Component Summary

Document a helper or component only after the methods it calls are documented or
included in the A-side. The output must separate:

- local control flow
- delegated behavior
- parse/validation/error decisions inherited from callees

### Stage T3 - Caller / Entry-Point Summary

Document controller actions, ASMX endpoints, page events, and workflow entry
points only after all important helper summaries are available. The caller may
claim callee behavior only by citing the accepted `CalleeSummary`.

### Stage T4 - Workflow / Module Summary

Document the user-visible workflow only after the caller chain is documented.
The workflow summary must preserve data flow, side effects, error paths, and
framework lifecycle constraints.

### Stage T-F - Framework Lifecycle Support

Small customer projects may have almost no project-defined business call graph.
ASP.NET Core template-like projects are still valid documentation targets, but
their transit evidence is the framework lifecycle:

- `Program.Main` -> host builder -> `Startup`
- `Startup.ConfigureServices` -> service/controller activation
- `Startup.Configure` -> middleware and endpoint routing
- endpoint dispatch -> controller action
- response serialization -> expression-bodied properties or model getters

Rows for this case must be marked with:

- `artifact_kind`: `framework_lifecycle_surface`
- `source_root_role`: `framework_lifecycle_root`
- `documentation_level`: usually `D4` for host/startup/endpoint propagation

They must not invent product/domain workflow. If the only code is the default
weather forecast template, the correct documentation is framework lifecycle plus
visible response construction, not a fictional game-server domain.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\request.json' --input 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\DOC-CS-NULL\base\M002\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-CS-NULL/base`. Не запускать review, accept, export, oracle или обучение.
