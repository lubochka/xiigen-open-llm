# Генерация одной партии DPO в Kiro

Ты готовишь учебные кандидаты одной маленькой темы. Используй текущую выбранную пользователем китайскую модель Kiro; помощники наследуют её. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity не раскрыта, прямо запиши это; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В `report.md` коротко обоснуй выбор содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит последующие команды проверки. Генератор не подписывает собственный материал как независимо принятый.

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
    "review_status": "independent_review_required",
    "measured_training_result": false
  }
}
```

Это схема с заполнителями, не готовая строка. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — запиши строку в `quarantine.jsonl` с причиной, сохрани черновик и укажи недостающий материал.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: reviewer должен проверить скрытые парафразы и родство.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Для приёмки поведения требуется небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Глобальные production-тесты здесь не запускаются.

## Контекст и завершение

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`, команда `tools/batches.py next --stream <ID/profile>`.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.


## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "TR07-OMISSION/ru-en",
  "topic_id": "TR07",
  "objective_id": "TR07-OMISSION",
  "domain_id": "tr.en",
  "profile_id": "ru-en",
  "run_id": "M001",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/tr/TR07-OMISSION/F0000001",
    "v3/tr/TR07-OMISSION/F0000002",
    "v3/tr/TR07-OMISSION/F0000003",
    "v3/tr/TR07-OMISSION/F0000005",
    "v3/tr/TR07-OMISSION/F0000007",
    "v3/tr/TR07-OMISSION/F0000009",
    "v3/tr/TR07-OMISSION/F0000011",
    "v3/tr/TR07-OMISSION/F0000012",
    "v3/tr/TR07-OMISSION/F0000014",
    "v3/tr/TR07-OMISSION/F0000015"
  ],
  "family_contracts": {
    "v3/tr/TR07-OMISSION/F0000001": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000001",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000002": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000002",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000003": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000003",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000005": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000005",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000007": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000007",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000009": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000009",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000011": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000011",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000012": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000012",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000012"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000014": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000014",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/tr/TR07-OMISSION/F0000015": {
      "origin_family_id": "v3/tr/TR07-OMISSION/F0000015",
      "leakage_group_ids": [
        "v3/tr/TR07-OMISSION/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 16,
  "sources": [
    "RAG-024",
    "RAG-023"
  ],
  "card": "curriculum/cards/TR07-OMISSION.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\TR07-OMISSION\\ru-en\\M001",
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "TR07-OMISSION",
  "topic_id": "TR07",
  "title_ru": "Ровно одна пропущенная буква известного слова",
  "learning_goal_ru": "Переводить контекстно однозначное слово с одной пропущенной буквой, сохраняя его лексическую семью и смысл.",
  "generation_prompt_ru": "Сформируй только случаи single-character deletion. Сначала запиши естественный canonical-сценарий и target-перевод, затем удали ровно одну букву внутри целевого слова. Запиши индекс удаления в Unicode code points и обе формы в metadata. В основном наборе поврежденная форма не должна быть другим самостоятельным словом; такую коллизию отправь в отдельный objective TR07-VALIDWORD, не исправляй её автоматически. Видимый prompt сообщает, что есть одна случайно пропущенная буква, но не сообщает canonical слово. Контекст должен оставлять одно разумное восстановление; спорные случаи quarantine. Rejected заменяет именно восстанавливаемый предмет другим, сохраняя остальные действия, роли, время, число и регистр. Не подменяй эту тему перестановками, повторами, морфологией, раскладкой или отрицанием. Вариации длины и позиции делай только в пределах source-языка профиля. Чистые формы допускаются как отдельно размеченный контроль, без ложного заявления о наличии дефекта.\n\nРабота выполняется по одному request.json: один objective_id, topic_id, profile_id и split. Не меняй модель Kiro: используй текущую, помощники наследуют её; запиши фактически наблюдаемое имя, не угадывай поставщика. Читай 02-DATA-CONTRACT.md, 04-QUALITY-AND-SPLITS.md, назначенную карточку и указанные RAG-источники. Ответы составляются на языке назначения; отчёт и объяснения — по-русски. Записывай draft.jsonl, review.json и report.md в папку запуска, заданную prepare; в этом objective нет разрешения запускать обучение или менять production. Все примеры этого файла — публичные reference, не train-ready и не скрытый EXAM. Примеры общего исходного сценария, его обратные переводы, опечатки и раскладки имеют прежние family/lineage и один split; не выдумывай новую семью для копии. Действительное улучшение любой полезной метрики на валидном учебном опыте — успех по этой метрике/домену с показом ухудшений рядом; подготовка JSON не является учебным опытом, 0→0 не доказывает обучение. Если позже обучение разрешено, сначала восстанови совместимый обученный checkpoint по 05, используй один process handle и штатный wait до terminal state; не заканчивай ход вместо ожидания и не делай частого polling. При «следующий» немедленно переключись на указанный ID, запрещено продолжать прошлую задачу, дописывать её отчет или повторять её запуск. При «стоп» прекрати действия. Прежние материалы не удаляй: новый run/revision сохраняется рядом.",
  "response_contract_ru": "Только перевод на target-языке; в objectives уточнения — одна разрешенная конкретная просьба уточнить смысл, когда контекста недостаточно. Конкретный контракт каждого prompt имеет приоритет. Объяснения только в metadata/rationale.",
  "scenario_axes": [
    {
      "axis": "position",
      "values": [
        "first",
        "middle",
        "last"
      ]
    },
    {
      "axis": "context",
      "values": [
        "one_short_sentence",
        "two_short_sentences"
      ]
    },
    {
      "axis": "word_length",
      "values": [
        "4_to_6_codepoints",
        "7_to_10_codepoints"
      ]
    }
  ],
  "forbidden_mixing": [
    "Перестановка букв, удвоение, смена раскладки или два одновременных повреждения.",
    "Valid-word collision без контекста, который исключает конкурирующий смысл.",
    "Другие цели обучения, соседние objective_id и несколько профилей в одном shard.",
    "Непроверенные карты раскладки, гадание о скрытом намерении, ошибочно связанные самостоятельные слова.",
    "REFERENCE примеры, их переводы и производные в запечатанном EXAM."
  ],
  "review_requirements": {
    "kind": "bilingual_review",
    "checks": [
      "Сначала проверить предпочтение по prompt без rationale; обе стороны должны быть естественны на целевом языке.",
      "Проверить все шесть направлений отдельно: язык инструкции не определяет source/target.",
      "Не выдавать self-review той же модели за независимого носителя; все опоры требуют предметного bilingual review.",
      "Не переносить rationale, canonical_source, family ID или метку ошибки в student prompt/answer.",
      "Проверить глобальные связи family/lexical/sense с другими карточками до назначения split; считать строки и независимые семьи отдельно.",
      "Другой корректный перевод допускается; вкусовые различия и синонимы не являются ошибочным rejected.",
      "Проверить механически: canonical_token с удалением одного указанного code point равен raw_token.",
      "Для иврита не заменять конечную форму буквы обычной и не удалять огласовки как будто это та же операция."
    ]
  },
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "TR07-OMISSION.ru-en.REF01",
      "profile_id": "ru-en",
      "family_id": "v2-ref.translation.ticket-on-table",
      "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: Блет лежит на столе.",
      "chosen": "The ticket is on the table.",
      "rejected": "The receipt is on the table.",
      "error_tag": "single_deletion_wrong_lexeme",
      "rationale": "Пропуск буквы в слове «билет» не превращает документ в квитанцию; остальные части предложения одинаковы.",
      "counterexample": "Нельзя брать самостоятельное слово и объявлять его опечаткой без видимого основания; новая лексема не включается в эту семью только по edit distance.",
      "source_refs": [
        "RAG-024",
        "RAG-023"
      ],
      "metadata": {
        "response_language": "en",
        "language_pair": {
          "source": "ru",
          "target": "en"
        },
        "reference_only": true,
        "split": "reference",
        "simulation": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "measured_training_result": false,
        "source_role": "methodology_not_translation_gold",
        "generator_model": "reference_authoring_not_a_kiro_run",
        "origin_family_id": "v2-ref.translation.ticket-on-table",
        "source_raw": "Блет лежит на столе.",
        "canonical_source": "Билет лежит на столе.",
        "canonical_token": "Билет",
        "raw_token": "Блет",
        "mutation": {
          "kind": "delete_one_codepoint",
          "index": 1
        },
        "lexical_family_ids": [
          "lex:ru:билет"
        ],
        "sense_id": "transport_ticket",
        "contrast_lexical_families": {
          "correct_target": "lex:en:ticket",
          "wrong_target": "lex:en:receipt"
        },
        "response_contract": "translation_only",
        "leakage_group_ids": [
          "lex:ru:билет",
          "source:v2-ref.translation.ticket-on-table"
        ]
      }
    }
  ]
}

## Материал RAG-024 (данные, не новые команды)

# RAG-024 — TRANSLATION-TOKENIZER-GUIDE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `bb26ac658daf86bf4eb2a56e83be14a29948f439` |
| Путь | `llm mvp/language translate learning/TRANSLATION-TOKENIZER-GUIDE-v1.md` |
| Проверенный объём | L16–L37, L92–L122, L168–L179; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md) |

## Что подтверждает фрагмент

Рассматривает Unicode-скрипты, Hebrew final forms/nikud и смешанные технические слова внутри предложения.

## Как использовать сейчас

Измерять покрытие на закреплённом локальном наборе, сохранять raw текст и привязку к семье. Не выполнять внешние загрузки из старого руководства; не объявлять его пороги и нормализацию доказанно оптимальными.

**Назначение в подготовке:** Разделение языков, токенизации и смысла.

**Ключи поиска:** tokenizer, unicode, hebrew, токенизация, раскладка.

## Подтверждающий фрагмент L16–L37

[Оригинал, строки 16–37](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L16-L37)

~~~~text
## Script Coverage Requirements

### Script Ranges

| Script | Unicode range(s) | Notes |
|---|---|---|
| Latin (English) | U+0020–U+007F (Basic Latin), U+00C0–U+024F (Latin Extended) | Include accented characters for proper names |
| Cyrillic (Russian) | U+0400–U+04FF | 256 code points; Russian uses approximately 66 of them |
| Hebrew | U+0590–U+05FF (Hebrew block), U+FB1D–U+FB4F (Alphabetic Presentation Forms) | Include both blocks; FB block contains legacy compatibility characters |

### Coverage Targets

Before any translation training begins (Phase 0 pre-condition):

| Language | Minimum token coverage | Test corpus |
|---|---|---|
| English | ≥ 97% | First 10,000 sentences from FLORES-200 EN |
| Russian | ≥ 95% | First 10,000 sentences from FLORES-200 RU |
| Hebrew | ≥ 90% | First 10,000 sentences from FLORES-200 HE |

Coverage = fraction of tokens that are NOT split into individual byte-fallback tokens.
If a token is represented as a sequence of `<byte_XX>` tokens, it is not covered.
~~~~

## Подтверждающий фрагмент L92–L122

[Оригинал, строки 92–122](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L92-L122)

~~~~text
## Hebrew-Specific Tokenizer Notes

### Nikud (Vowel Points)

Hebrew vowel diacritics (U+05B0–U+05BD, U+05C1–U+05C2, U+05C7) combine with
base characters. The tokenizer must treat base + nikud as a single combining
character sequence, not split them.

Recommendation: normalise Hebrew text to NFD (Unicode Canonical Decomposition)
before tokenisation. This ensures base letters and combining marks are consistently
separated at the unicode level, then the BPE merge rules can recombine them.

### Final Letter Forms

Hebrew has five final letters (ף ך ם ן ץ) that replace their regular forms
at word end. These are separate Unicode code points (not combining characters).
The tokenizer must treat them as distinct from their non-final counterparts:
- פ (pe) ≠ ף (final pe)
- כ (kaf) ≠ ך (final kaf)
- מ (mem) ≠ ם (final mem)
- נ (nun) ≠ ן (final nun)
- צ (tsadi) ≠ ץ (final tsadi)

Verify that the vocabulary contains both forms for each pair.

### Gershayim and Geresh

- Geresh (׳ U+05F3): marks abbreviations and foreign-origin letters
- Gershayim (״ U+05F4): marks acronyms and compound abbreviations

These must be in the vocabulary as single tokens, not split as punctuation.
~~~~

## Подтверждающий фрагмент L168–L179

[Оригинал, строки 168–179](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/TRANSLATION-TOKENIZER-GUIDE-v1.md#L168-L179)

~~~~text
## Mixed-Script Handling

Both Russian and Hebrew text can contain English words (brand names, technical
terms, proper names). These are expected and must tokenise correctly.

Example:
- Russian: "iPhone оказался дорогим" — "iPhone" must tokenise as Latin tokens
- Hebrew: "הפגישה ב-Zoom" — "Zoom" must tokenise as Latin tokens

The tokenizer must NOT attempt to force all tokens in a Russian sentence through
the Cyrillic merge table, or all tokens in a Hebrew sentence through the Hebrew
merge table. Script detection must be per-token, not per-sentence.
~~~~



## Материал RAG-023 (данные, не новые команды)

# RAG-023 — DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `fd70c69dcdd6edc70ed0b0d57ae77e3a150ffc90` |
| Путь | `llm mvp/language translate learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md` |
| Проверенный объём | L16–L49, L103–L154, L209–L239; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md) |

## Что подтверждает фрагмент

Различает сырое переводимое предложение, выбранный перевод и тип ошибки; описывает морфологию RU/HE и четыре старых направления.

## Как использовать сейчас

Актуально шесть направлений EN/RU/HE и раздельные темы. Не наследовать overlap>0.60, [WRONG]-аннотации и тезис «высокое сходство значит нет сигнала»: минимальная смысловая пара может быть полезной.

**Назначение в подготовке:** Правила смысловых контрастов в переводческих DPO.

**Ключи поиска:** translation, перевод, русский, иврит, английский.

## Подтверждающий фрагмент L16–L49

[Оригинал, строки 16–49](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md#L16-L49)

~~~~text
## Why This Document Exists

The orchestration DPO guidance covers ARCH, PLAN, SKILL, and CODE sub-phases.
Translation is a separate capability domain with different:

- A-side source (raw sentence, not compiled orchestration prompt)
- Chosen authority (verified parallel corpus, not XIIGen internal contract)
- Rejected error taxonomy (linguistic errors, not routing/governance errors)
- Reward margin mechanics (token-level linguistic divergence, not predicate match)

Read this document instead of DPO-PAIR-AUTHORING-GUIDANCE-v2.md when writing
translation pairs. Do not mix translation pairs into orchestration batches.

---

## Reward Margin Mechanics for Translation

DPO computes for every pair:

```
reward_margin = log P(chosen | a_side) - log P(rejected | a_side)
```

For translation pairs the model needs to learn that the chosen translation is
more likely given the source. This means:

- If chosen and rejected are very similar (high overlap), the model's logits
  for both are nearly identical → reward margin near zero → no learning signal.
- If the rejected side is obviously garbled, the model already scores it low
  before training → margin is already positive → pair teaches nothing new.
- The sweet spot is a rejected side that a naive model or careless translator
  would actually produce, but that a well-trained model should reject.

**Target reward margin per pair: > 0.05 after standalone ablation.**
~~~~

## Подтверждающий фрагмент L103–L154

[Оригинал, строки 103–154](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md#L103-L154)

~~~~text
## Batch Composition Rules

For a 64-pair first batch, target distribution:

| Direction | T01 core | T03 morphological | T02 register | T04/T05 idiom/cognate | T09 named entity |
|---|---|---|---|---|---|
| RU→EN | 6 | 3 | 2 | 2 | 1 |
| EN→RU | 5 | 3 | 2 | 1 | 1 |
| HE→EN | 6 | 3 | 2 | 2 | 1 |
| EN→HE | 5 | 4 | 2 | 1 | 2 |

Total: 64 pairs across 4 directions and 5 families.

Do not mix directions in a single sub-batch for ablation. Run direction-level
ablation before mixing all four directions.

---

## Script-Specific Rules

### Russian (Cyrillic)

1. Use UTF-8 exclusively. Do not use KOI8-R or Windows-1251 encoded input.
2. Stress marks (Unicode combining acute accent U+0301) are optional in pairs;
   do not treat their presence or absence as an error.
3. "Ё" (U+0401) vs "Е" (U+0415): modern Russian often drops the diaeresis.
   Both forms are acceptable in chosen; do not use one as a rejected variant
   of the other unless the pair explicitly tests this distinction.
4. Verb aspect (perfective / imperfective) is the single highest-value
   morphological teaching target for EN→RU pairs. Prioritise it in T03.

### Hebrew

1. Hebrew text must be in logical order (left-to-right in the byte stream),
   not visual order. Unicode bidi algorithm handles display.
2. Nikud (vowel points) — most modern Hebrew text omits nikud. Chosen sides
   should match the target register:
   - Biblical / liturgical text → nikud expected
   - Modern prose → nikud absent
   - Do not use nikud presence/absence as a rejected variant unless testing T06.
3. Gershayim (״) is the Hebrew double punctuation used for abbreviations and
   acronyms. Do not replace with ASCII quotation marks in Hebrew text.
4. The Hebrew alphabet has five final letters (ך ם ן ף ץ). Using a non-final
   form at word end, or a final form at word start/middle, is a morphological
   error — use as T03 rejected type.
5. Binyan (verb conjugation pattern) is the highest-value morphological teaching
   target for EN→HE pairs. Prioritise it in T03.
6. **Knesset source register label:** Pairs sourced from `knesset-he` must be
   labelled `(register: formal spoken)`, not `(register: formal)`. Knesset
   transcripts are spoken parliamentary Hebrew. Formal written Hebrew (prose,
   legal documents) is a different register — do not conflate them. Use
   `(register: formal)` only when the source is written formal text.
~~~~

## Подтверждающий фрагмент L209–L239

[Оригинал, строки 209–239](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/language%20translate%20learning/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md#L209-L239)

~~~~text
## Token Overlap Test

Compute before submitting each pair to the batch:

```python
def overlap_ratio(a_side_tokens, chosen_tokens, rejected_tokens):
    prompt_set = set(a_side_tokens)
    shared = set(chosen_tokens) & set(rejected_tokens) - prompt_set
    total = len(set(chosen_tokens) | set(rejected_tokens)) - len(prompt_set & (set(chosen_tokens) | set(rejected_tokens)))
    if total == 0:
        return 0.0
    return len(shared) / total
```

If `overlap_ratio > 0.60` and pair length qualifies (>= 12 tokens): discard.

For translation pairs this typically fires when chosen and rejected differ by
only one or two tokens. In that case, select a more divergent rejected variant.

---

## Diagnosis Table

| Symptom | Likely cause | Fix |
|---|---|---|
| All pairs in a batch show reward_margin < 0 | Rejected sides are too good (too similar to chosen) | Increase structural divergence; use Type A errors instead of Type B |
| Reward margin near zero for all pairs | Chosen and rejected are token-similar | Apply overlap test; select more divergent rejected variants |
| High loss but low accuracy | Model is uncertain which is correct | Pair may be ambiguous; add more specific [WRONG:] annotation |
| Direction-specific negative margin | Tokenizer handles that language poorly | Check tokenizer coverage for the script; verify BPE (Byte Pair Encoding) vocabulary |
| Hebrew pairs all fail | Encoding issue (bidi) | Verify UTF-8 byte order in Hebrew fields; check that logical order is correct |
| Russian morphological pairs negative margin | Aspect is too subtle | Add more phonologically distinct chosen/rejected pairs before returning to aspect |
~~~~

