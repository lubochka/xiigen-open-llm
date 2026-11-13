# Кандидаты улучшения метода: контекст и обучаемая память

## Выбор

**Рекомендую два случая, с разными названиями результата.** T-TRAIN-CONTEXT показывает пользу более подробного учебного представления. KDP01 показывает пользу ручных предметных признаков при чтении уже обученной памяти. Второй опыт не следует называть улучшением усвоения новых уроков. E18 оставлен историческим резервом; остальные пять не предлагаются в основной ряд.

HCL исключён из этой подборки: улучшение после исправления неправильного локального индекса реально, но это исправление реализации. Такое сравнение не доказывает превосходство нового метода обучения.

Текущий код сопоставлен с `75107af930a1d4c55d60fa8a4ef7c01643f13cb0`. Работа ограничена чтением источников и арифметическим разбором сохранённых результатов. Новых обучений, инференса, тестов и сборок не было. Основные пользовательские артефакты не редактировались.

| Кандидат | Решение | Самый полезный вывод | Главная граница |
|---|---|---|---|
| T-TRAIN-CONTEXT | Основной |2/4→4/4; уникальные 1/3→3/3; дополнительных 256B выделений на чтение|Одно слово, два значения; POS задан человеком|
| KDP01 | Основной с названием «чтение памяти» |На S1: RU5/8→8/8, HE6/8→8/8, OR5/8→8/8; новых ошибок 0|Успешные функции ручные; ресурсной A/B нет; CS/EN без прироста|
| E18 | Исторический резерв |33/40→38/40 для памяти комбинаций признаков|Нет построчных ошибок/потерь; шаблоны не отделены; текущий путь другой|
| E25 | Не основной |Само наполнение памяти даёт 11/32→28/32|26/32 повторяют TRAIN; на новых 2/6→2/6; lookup+rules тоже 28/32|
| E40v2 | Не основной |Собственный V2baseline14/21→21/21|14 повторяемых шаблонов;0→100 между V1/V2 нечистое сравнение; текущий scorer иной|
| E22 | Не основной |MacroF1 .4709→.5646|Автоматические labels, неполный manifest, нет баланса исправлений и потерь|
| E13B | Не основной |Извлечение знакомых формул и отказ от неизвестных|100%на TRAIN;0/10 решений новых формул; нет matchedKNN→lookup качества|
| KDR01 | Поддержка KDP, не отдельный главный пример |Корректный B→C:24/46→28/46 с 4 правильными отказами в обеих цифрах|A-fixed всегда C#—искусственный контроль ошибки;RU/HE0/8|

## Почему цена TCTX не исключает полезный результат

Раньше я слишком механически отнёс любые дополнительные выделения к существенному ухудшению. Здесь речь о **1024 байтах на всю панель из четырёх чтений**, то есть 256B на ответ. За эту цену словарь на показанных входах начинает различать два значения. Это разумный кандидат для объяснения метода. При этом 4B в отчёте — рост сериализованного JSON уроков, а не измеренная память модели. Размер удерживаемых массивов и стабильное время не установлены.

## Подробный разбор

### T-TRAIN-CONTEXT-01 — Сохранять контекст вместе с переводом, чтобы память различала значения одного слова

**Решение:** `recommended_main`.

**Старый способ.** Две правильные пары book→книга и book→бронировать записываются без левого и правого POS-контекста. Это корректно работающий словарь по слову с недостаточно подробным представлением; при равных расстояниях и приоритетах он возвращает первую запись.

**Новый способ.** В те же две записи сохраняются части речи слева и справа. Для книги — определитель слева и глагол справа; для бронировать — местоимение слева и существительное справа. Сам AddEntry, расстояние, порядок строк и читатель не меняются.

**Что действительно училось.** Список из двух DictionaryEntry, добавленных реальными AddEntry. Значения перевода и контекст приходят из уроков. Нейронные веса, LoRA, DPO и k-соседское голосование не обучаются; поле k читателем не используется.

**Почему это не следует смешивать с исправлением HCL.** Опыт сравнивает две полноты учебного представления. Он не исправляет неправильную адресацию или сломанный AddEntry: AddEntry в обеих версиях правильно сохраняет переданную запись. Фактический продуктовый писатель, якобы терявший POS, не установлен; такой баг здесь не заявляется.

**Базовый метод.** Читатель и код обучения одинаковы. Отсутствие POS — заявленное ограничение контрольного word-only представления, а не нарушение его контракта. На сбалансированных четырёх вопросах постоянный перевод одного book не может дать больше 2/4; при подсчёте трёх уникальных контекстов лучший постоянный ответ мог бы дать 2/3, тогда как фактический A даёт 1/3.

**Измерения и потери.**

```json
{
  "train_entries_per_arm": 2,
  "exam_observations_per_stage": 4,
  "unique_reader_inputs": 3,
  "query_ids_duplicate": [
    "Q1",
    "Q3"
  ],
  "before_A": 0,
  "before_B": 0,
  "after_A": 2,
  "after_B": 4,
  "unique_after_A": 1,
  "unique_after_B": 3,
  "corrected": [
    "Q2",
    "Q4"
  ],
  "regressed": [],
  "answers": {
    "A": {
      "Q1": "книга",
      "Q2": "книга",
      "Q3": "книга",
      "Q4": "книга"
    },
    "B": {
      "Q1": "книга",
      "Q2": "бронировать",
      "Q3": "книга",
      "Q4": "бронировать"
    }
  },
  "train_order_same": true,
  "reader_same": true
}
```

**Цена.** Дополнительные 1024B на четыре чтения —1KiB на всю панель и 256B на ответ. Это небольшой абсолютный расход для показанного качества, а не автоматическое основание исключить опыт. Рост 31% относится к маленькому участку. +4B относится только к сериализованному JSON двух уроков: четыре int имеют 16B полезных числовых данных плюс накладные расходы массивов, размер состояния отдельно не измерен. Разницу кучи 7968B нельзя целиком приписать модели. По времени выигрыша/ухудшения не установлено: одинаковая работа до обучения разошлась на 5.46ms, больше последующей разницы 0.94ms.

**Обобщение.** Все три различных POS-входа содержат дополнительные теги относительно уроков; это узкое различение контекстных вариантов двух значений одного слова. Нет проверки новых слов, автоматического POS, полных фраз, грамматического рода, морфологии или субъектно-объектных ролей. Уникальных входов всего 3, повторов запуска нет.

**Текущее состояние.** В HEAD75107 механизм уже есть в 12.SemanticCompiler/Translation/ContextualDictionary.cs: AddEntry хранит поля, TranslateWithContext ими пользуется. Это подтверждение существующего механизма и лучшего состава урока, не найденный новый перенос кода/исправленный продуктовый писатель.

**Простая формула.**

\[
\hat t=\operatorname{translation}\!\left(\arg\min_e d(c_q, c_e)\right)
\]

На этой панели верный сохранённый контекст даёт расстояние 0, чужой 3; две пустые записи контроля дают одинаковое расстояние 2. Сведения о соседях разрывают ничью.

**Допустимая краткая формулировка:** На двух уроках сохранение POS-контекста подняло ответы с 2/4 до 4/4 при дополнительных 256B выделений на чтение; это локальный пример представления обучаемой памяти.

**Первичные источники:**

- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md:1–65](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md#L1-L65) — primary report and explicit limits.
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Program.cs:29–91](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Program.cs#L29-L91) — one-factor training inputs and actual reader.
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/raw.json:1–189](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/A/raw.json#L1-L189) — raw A; answers, counts and costs.
- [experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/B/raw.json:1–197](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/B/raw.json#L1-L197) — raw B; answers, counts and costs.
- [12.SemanticCompiler/Translation/ContextualDictionary.cs:13–27](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/12.SemanticCompiler/Translation/ContextualDictionary.cs#L13-L27) — current learned-state fields and writer.
- [12.SemanticCompiler/Translation/ContextualDictionary.cs:67–117](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/12.SemanticCompiler/Translation/ContextualDictionary.cs#L67-L117) — current reader and exact distance.

### KNN-DOMAIN-PRINCIPLES-01 — При ранжировании памяти учитывать время текущего предложения и уже выполненные этапы

**Решение:** `recommended_only_as_manual_readout_method`.

**Старый способ.** Штатный поиск использует тот же обученный индекс и память, объединяя близость вектора, BM25-подобное совпадение и фиксированную структурную метку с весами 0,45 / 0,35 / 0,20. Он выбирает максимальную оценку; отдельного признака времени текущего предложения нет.

**Новый способ.** К прежней оценке тех же кандидатов добавляется ограниченная оценка доменных признаков. RU/HE сравнивают время последнего предложения префикса, OR — последние завершённые этапы. Ответ берётся из памяти. Функции не генерируют окончание и не добавляют кандидатов.

**Что действительно училось.** S0 содержит четыре урока, S1 — восемь. Штатный TransformerEmbeddingKnnLogicalUnit.Train строит embeddings, HNSW и таблицу примеров; веса LLM не обновляет. Стенд создаёт один кодировщик new LlmModel на профиль: один слой, скрытая размерность 16; загрузки предобученных весов или нейронного обучения перед этим не показано. BPE заранее обучается на восьми TRAIN-входах, включая будущие T+, но без EXAM. Функции времени и этапа и их веса заданы человеком. Частотная функция зависит от статистики TRAIN, однако ни одного ответа не улучшила.

**Почему это не следует смешивать с исправлением HCL.** Baseline корректно реализует исходный общий retrieval и совпадает с native-модулем 40/40. Изменение вводит новый предметный признак/предпочтение чтения, а не чинит индекс, переданный не той таблице. Поэтому это улучшение метода ранжирования; называть его улучшением усвоения новых уроков нельзя.

**Базовый метод.** Сравнивать A-empty с B на одном S0 или одном S1, при общем списке candidates и native scores. Не смешивать с KDR always-C#; это иной искусственный baseline. Все функции и fixtures были заданы до запуска по первичному отчёту; независимая внешняя проверка не установлена.

**Измерения и потери.**

```json
{
  "profiles": [
    {
      "profile": "CS",
      "S0": {
        "control": 4,
        "all_functions": 4,
        "total": 8,
        "corrected": [],
        "regressed": []
      },
      "S1": {
        "control": 6,
        "all_functions": 6,
        "total": 8,
        "corrected": [],
        "regressed": []
      }
    },
    {
      "profile": "EN",
      "S0": {
        "control": 8,
        "all_functions": 8,
        "total": 8,
        "corrected": [],
        "regressed": []
      },
      "S1": {
        "control": 8,
        "all_functions": 8,
        "total": 8,
        "corrected": [],
        "regressed": []
      }
    },
    {
      "profile": "RU",
      "S0": {
        "control": 5,
        "all_functions": 8,
        "total": 8,
        "corrected": [
          "E02",
          "E07",
          "E08"
        ],
        "regressed": []
      },
      "S1": {
        "control": 5,
        "all_functions": 8,
        "total": 8,
        "corrected": [
          "E02",
          "E07",
          "E08"
        ],
        "regressed": []
      }
    },
    {
      "profile": "HE",
      "S0": {
        "control": 6,
        "all_functions": 8,
        "total": 8,
        "corrected": [
          "E02",
          "E08"
        ],
        "regressed": []
      },
      "S1": {
        "control": 6,
        "all_functions": 8,
        "total": 8,
        "corrected": [
          "E02",
          "E08"
        ],
        "regressed": []
      }
    },
    {
      "profile": "OR",
      "S0": {
        "control": 4,
        "all_functions": 8,
        "total": 8,
        "corrected": [
          "E02",
          "E04",
          "E05",
          "E06"
        ],
        "regressed": []
      },
      "S1": {
        "control": 5,
        "all_functions": 8,
        "total": 8,
        "corrected": [
          "E02",
          "E05",
          "E06"
        ],
        "regressed": []
      }
    }
  ],
  "rows_per_profile": 8,
  "total_S0_A": 27,
  "total_S0_all_functions": 36,
  "total_S1_A": 30,
  "total_S1_all_functions": 38,
  "native_copy_equivalence": "40/40",
  "restore_equivalence": "40/40",
  "interpretation": "S0 and S1 are different memory sizes; compare arms within each state. 336 raw arm/state/exam observations are not 336 independent test inputs."
}
```

**Цена.** Есть только суммарные 7.948s и 73,809,920B пика процесса для 496 логических ответов, построения/чтения состояний и всех 21 плеча. Раздельных A/B задержек, аллокаций, размера памяти и marginal cost функций нет. Кандидаты переиспользовались всеми плечами, поэтому общую скорость нельзя выдавать за стоимость обычного serving. Отсутствие новых смысловых ошибок установлено; отсутствие ресурсного ухудшения — нет.

**Обобщение.** Восемь коротких вопросов на профиль, окончания из четырёх известных ответов и близкие перефразировки уроков. RU: Вчера Олег → писал. Это не проверка согласования рода; HE вся форма 3-го лица мужского рода единственного числа указана в инструкции заранее. У HE штатный S0/S1 читатель воспроизводит 3/4 уроков, приобретение новой дозы не показано. CS/EN прироста нет. Весь эффект функций существует уже на S0, поэтому ускоренного обучения нет.

**Текущее состояние.** Код конструктора и функций существует как Kdp01Probe в experiments-v2/RP01-MICRO/KDP01. В проверенном текущем native-owner18.CapabilityTraining/LlmIngredientLogicalUnits.cs ScoreCandidates остаётся.45/.35/.20, без массива принципов. Перенос в общий продуктовый путь не подтверждён; наличие эксперимента не выдаётся за интеграцию.

**Простая формула.**

\[
s_1(q, e)=s_0(q, e)+\frac{0.2}{m}\sum_{j=1}^m f_j(q, e),\quad f_j\in[-1,1]
\]

Для проведённых плеч веса функций равны 1. Неприменимый признак даёт 0, остальные ограничены[-1,1]. Поэтому оценка кандидата меняется максимум на 0.2, а разрыв двух — максимум на 0.4. В RU-E02 родной перевес неверного ответа 0.0023065 исчезает: пишет 0.646003−0.2=0.446003; писал 0.643696+0.2=0.843696.

Условие формулы: General API multiplies by Descriptor.Weight and does not normalize their sum: the .2/.4 bound assumes the actual experimental weights all equal1. Correct hints may help; wrong/non-discriminative hints need not, as CS shows.

**Допустимая краткая формулировка:** Ручные признаки времени и этапа улучшили выбор ответа из прежней памяти в RU, HE, OR; на той же панели не появилось новых ошибок. Это изменение чтения, не доказательство лучшего обучения.

**Первичные источники:**

- [experiments-v2/RP01-MICRO/KDP01/results/attempt-01/report.md:1–237](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/report.md#L1-L237) — primary report with all profiles, qualifications and costs.
- [experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json:1–14802](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json#L1-L14802) — 336 raw observations; parsed for all arm/state/profile gains and regressions.
- [experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json:6433–6463](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json#L6433-L6463) — raw native RU-E02.
- [experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json:7030–7065](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/results/attempt-01/raw.json#L7030-L7065) — raw time-feature RU-E02.
- [experiments-v2/RP01-MICRO/KDP01/DomainKnnLearningModel.cs:80–118](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/DomainKnnLearningModel.cs#L80-L118) — constructor and equal per-function scale.
- [experiments-v2/RP01-MICRO/KDP01/DomainKnnLearningModel.cs:125–214](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/DomainKnnLearningModel.cs#L125-L214) — native fusion, clamp and final ranking.
- [experiments-v2/RP01-MICRO/KDP01/Principles.cs:280–337](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/Principles.cs#L280-L337) — manual time-frame extraction.
- [experiments-v2/RP01-MICRO/KDP01/Principles.cs:441–467](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/Principles.cs#L441-L467) — learned frequency scorer, zero result.
- [experiments-v2/RP01-MICRO/KDP01/Principles.cs:500–565](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDP01/Principles.cs#L500-L565) — manual stage/prerequisite comparison.
- [experiments-v2/RP01-MICRO/KDP01/Program.cs:61–109](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/experiments-v2/RP01-MICRO/KDP01/Program.cs#L61-L109) — model initialization, BPE exposure and actual memory training.
- [experiments-v2/RP01-MICRO/KDP01/Program.cs:437–510](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/experiments-v2/RP01-MICRO/KDP01/Program.cs#L437-L510) — arms and fixed language marker lists.
- [experiments-v2/RP01-MICRO/KDP01/Program.cs:555–613](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/experiments-v2/RP01-MICRO/KDP01/Program.cs#L555-L613) — RU/HE lessons, exam and fixed-gender instruction.
- [18.CapabilityTraining/LlmIngredientLogicalUnits.cs:98–168](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L98-L168) — native Train changes index/examples, not neural weights.
- [18.CapabilityTraining/LlmIngredientLogicalUnits.cs:184–244](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L184-L244) — current native readout without principle array.

### E18-DOMAIN-CLASSIFIER — Комбинации признаков и память меток вместо первого найденного ключевого слова

**Решение:** `historical_reserve_not_main`.

**Старый способ.** Фиксированный keyword baseline сначала проверяет BUG_FIX, затем CODE_CHANGE, ORCHESTRATION, ARCHITECTURE; иначе CODE_QUESTION.

**Новый способ.** Семь групп слов превращаются в множество предикатов. Память хранит 160 размеченных профилей; пять ближайших по Жаккару голосуют за метку. Это комбинации заданных признаков и обученная память меток.

**Что действительно училось.** Список пар (предикаты, метка). Train очищает список и заполняет его примерами. Метки заданы авторами 25 синтетических шаблонов, а не получены от baseline, несмотря на старый комментарий auto-labelled.

**Почему это не следует смешивать с исправлением HCL.** Сравнение приоритета ключевых слов с памятью соседних размеченных примеров сопоставляет два допустимых метода. Отдельную прежнюю замену C#-normalizer, дававшего пустые признаки английским строкам, нельзя выдавать за достижение обучения. Положительный результат 33/40 → 38/40 относится уже к пригодному английскому extractor.

**Базовый метод.** Baseline соответствует своей простой эвристике, но сравнение слабое: новый метод получает комбинации признаков и размеченную память. Преимущество над более сильным классификатором на тех же признаках не установлено.

**Измерения и потери.**

```json
{
  "train": 160,
  "validation": 40,
  "templates": 25,
  "correct_baseline": 33,
  "correct_memory": 38,
  "enhanced_by_class": {
    "CODE_QUESTION": "5/7",
    "BUG_FIX": "6/6",
    "CODE_CHANGE": "8/8",
    "ORCHESTRATION": "11/11",
    "ARCHITECTURE": "8/8"
  },
  "gain_pp": 12.5,
  "per_row_regressions": "not recorded; can range0..2 from aggregates",
  "historical_later_different_path": {
    "baseline": 0.825,
    "enhanced": 0.85
  }
}
```

**Цена.** 14,9 мс на 40 классификаций — около 0,373 мс на строку простым делением. Это небольшой абсолютный бюджет. Ноль у baseline округлён; делить на него и заявлять бесконечное замедление нельзя. Аллокации, стоимость обучения и устойчивые временные измерения не опубликованы.

**Обобщение.** Шаблонные семейства не разделены между обучением и проверкой. Построчных запросов и ID в результате нет. Не установлены точное пересечение, качество на новых семействах или отсутствие отдельных регрессий. Из агрегатов следует только пять чистых дополнительных успехов; ухудшений могло быть от нуля до двух.

**Текущее состояние.** Нынешний experimental runner использует SemanticTeachingPromptCompiler и включает в признаки compiled.Domain. Compatibility-класс делегирует DomainClassifierUnit и иначе задаёт baseline. Старые 95% нельзя присвоить этому пути. Поздний сохранённый результат содержит 85%, но менялось несколько факторов.

**Почему не в основном ряду.** Исторический резерв, не основной пример отсутствия существенных потерь: нет построчных результатов, семейства шаблонов не отделены, нынешняя реализация другая.

**Первичные источники:**

- [35.ExperimentsV2/results/E18-DOMAIN-CLASSIFIER/results/run-2026-05-01-002.json:1–18](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/results/E18-DOMAIN-CLASSIFIER/results/run-2026-05-01-002.json#L1-L18) — positive historical aggregate.
- [35.ExperimentsV2/Experiments/E18DomainClassifier.cs:15–162](https://github.com/lubochka/llm_mvp_core/blob/530c1e0581fa854574bd8608a7576a80236f2ebd/35.ExperimentsV2/Experiments/E18DomainClassifier.cs#L15-L162) — historical actual representation, training and labelled templates.
- [35.ExperimentsV2/Infrastructure/DomainClassifier.cs:16–67](https://github.com/lubochka/llm_mvp_core/blob/530c1e0581fa854574bd8608a7576a80236f2ebd/35.ExperimentsV2/Infrastructure/DomainClassifier.cs#L16-L67) — historical keyword baseline and learned-memory reader.
- [35.ExperimentsV2/Experiments/E18DomainClassifier.cs:75–147](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/35.ExperimentsV2/Experiments/E18DomainClassifier.cs#L75-L147) — current changed representation.
- [35.ExperimentsV2/Compatibility/DomainClassifier.cs:26–83](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/35.ExperimentsV2/Compatibility/DomainClassifier.cs#L26-L83) — current wrapper and changed baseline.
- [35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E18-DOMAIN-CLASSIFIER/results/run-2026-05-07-002.json:1–18](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E18-DOMAIN-CLASSIFIER/results/run-2026-05-07-002.json#L1-L18) — later result85%, different code path.

### E25-PATTERN-CLASSIFIER — pos_bigram_memory_vs_restricted_rules

**Решение:** `exclude_main`.

**Старый способ.** Небольшой набор правил по позиции первого глагола и существительного либо местоимения. Он не охватывает все восемь меток генератора.

**Новый способ.** Сохранить 128 POS-последовательностей с метками; сравнивать множества соседних POS-пар по Жаккару; три ближайших примера голосуют.

**Что действительно училось.** Память POS-последовательностей и меток. Метрика и k фиксированы; веса LLM не обучаются.

**Почему это не следует смешивать с исправлением HCL.** KNN-память — иной метод, а не ремонт адресации. Но слабость baseline и повторение учебных последовательностей объясняют основной результат. Обобщение этим не подтверждается.

**Измерения и потери.**

```json
{
  "train": 128,
  "exam": 32,
  "historical_baseline": 0.3438,
  "historical_memory": 0.875,
  "verified_later_same_generator_raw": {
    "before": 11,
    "after": 28,
    "seen_rows": 26,
    "seen_correct_before": 9,
    "seen_correct_after": 26,
    "unseen_rows": 6,
    "unseen_correct_before": 2,
    "unseen_correct_after": 2,
    "regressions_zero_based": [
      11,
      21
    ]
  },
  "stronger_baseline": {
    "method": "exact lookup in the same saved memory; original saved rule answer on a missing key",
    "correct": 28,
    "total": 32,
    "status": "calculated from saved records, no classifier/model run",
    "train_conflicting_sequence_labels": 0
  },
  "historical_binary_attestation": "Original May JSON has no binary SHA;26/32 is directly confirmed for laterPQ fixture using same seed42/perPattern20 and matching counts, not claimed as recovered May execution bytes."
}
```

**Цена.** Историческая дополнительная задержка 15 мс на 32 вопроса — около 0,469 мс на строку. Абсолютный бюджет может быть приемлем, но замер один и холодный. Поздний PQ снижает затраты чтения, сохраняя ответы; он не создаёт дополнительного выигрыша качества.

**Обобщение.** В более поздней сохранённой панели PQ 26 из 32 вопросов повторяют учебные последовательности. На шести новых точность 2/6 до и 2/6 после. Два новых входа исправлены, два ранее верных потеряны. Точный поиск по той же памяти с прежним ответом правил при отсутствии ключа также даёт 28/32 по сохранённым данным.

**Текущее состояние.** POS-bigram KNN существует в проекте 12, теперь с кэшем запроса. Одноимённый класс проекта 29 для fusion — другая фиксированная эвристика. 87,5% нельзя переносить на любой компонент с тем же названием.

**Почему не в основном ряду.** Не основной: весь чистый прирост относится к знакомым последовательностям; есть два регресса на новых; более сильный корректный lookup-baseline даёт ту же точность.

**Первичные источники:**

- [35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json:1–18](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L1-L18) — May aggregate.
- [35.ExperimentsV2/Experiments/E25PatternClassifier.cs:20–68](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L20-L68) — split, baseline and trained classifier.
- [experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/A/arm.json:1–946](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/A/arm.json#L1-L946) — saved memory and all before/after answers; exact overlap and lookup calculation.
- [experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md:58–124](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/PQ01/results/attempt-0001/report.md#L58-L124) — same generator, repeated/novel strata and losses.
- [12.SemanticCompiler/Translation/PatternClassifier.cs:38–129](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/12.SemanticCompiler/Translation/PatternClassifier.cs#L38-L129) — current memory, baseline, metric and templates.

### E40V2-TRUST-SCORER — feature_memory_plus_evaluation_split_change

**Решение:** `exclude_main`.

**Старый способ.** Внутри V2 простой baseline: свой или официальный источник получает HIGH, остальные — MEDIUM.

**Новый способ.** Нормировать семь признаков, сохранить 79 векторов с метками; пять евклидовых соседей голосуют за LOW, MEDIUM или HIGH.

**Что действительно училось.** Размеченные векторы источников. Веса расстояния и нейронная модель не обучаются.

**Почему это не следует смешивать с исправлением HCL.** V2 против его собственного baseline сравнивает методы. Однако переход от E40-v1 к V2 меняет разбиение так, чтобы классы попали в обе части. История 0% → 100% не является чистым улучшением алгоритма обучения.

**Измерения и потери.**

```json
{
  "correct_baseline": 14,
  "correct_memory": 21,
  "validation": 21,
  "train": 79,
  "rows": 100,
  "source_templates": 14,
  "enhanced_by_level": {
    "LOW": "4/4",
    "MEDIUM": "8/8",
    "HIGH": "9/9"
  },
  "v1": {
    "validation": 20,
    "all_validation_low": true,
    "baseline": 0,
    "enhanced": 0
  }
}
```

**Цена.** 2,9 мс на построение векторов и 21 чтение — небольшой наблюдённый абсолютный бюджет. Нет устойчивого тайминга, аллокаций и размера состояния. Главная причина исключения — постановка проверки, а не отношение к округлённому нулю baseline.

**Обобщение.** Сто строк выбираются с повторениями из 14 фиксированных шаблонов признаков. Целые семейства источников между обучением и проверкой не отделены; новые типы источников не проверены.

**Текущее состояние.** Нынешний TrustScorer проекта 16 использует фиксированную взвешенную сумму с порогами 0,70 и 0,30. В нём нет этой памяти, k=5 или Train. Результат экспериментального KNN не относится к такому scorer автоматически.

**Почему не в основном ряду.** Не основной: 14/21 → 21/21 валидны внутри V2, но повторная выборка 14 шаблонов и неэквивалентный перенос делают пример слабее TCTX и KDP. Переход 0% → 100% показывать нельзя.

**Первичные источники:**

- [35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json:1–18](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json#L1-L18) — V2 aggregate.
- [35.ExperimentsV2/Experiments/E40TrustScorerV2.cs:31–147](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs#L31-L147) — stratification, correct baseline, KNN and14templates.
- [35.ExperimentsV2/results/E40-TRUST-SCORER/results/run-2026-05-01-002.json:1–18](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/35.ExperimentsV2/results/E40-TRUST-SCORER/results/run-2026-05-01-002.json#L1-L18) — different V1 validation panel.
- [16.AutonomousLearning/TrustScorer/TrustScorer.cs:26–103](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/16.AutonomousLearning/TrustScorer/TrustScorer.cs#L26-L103) — current fixed scorer.

### E22-DATASET-ROUTER — multilabel_knn_on_automatically_labelled_predicates

**Решение:** `exclude_main`.

**Старый способ.** В положительном позднем опыте baseline возвращает три самых частых раздела из обучения. Старая шапка «все разделы» не соответствует этому сравнению.

**Новый способ.** Запомнить 360 наборов предикатов с шестью булевыми метками; три ближайших по Жаккару примера голосуют отдельно по каждому разделу.

**Что действительно училось.** Пары «предикаты → набор разделов». Метки задаёт AutoLabel по текстовым условиям, а не независимый эксперт.

**Почему это не следует смешивать с исправлением HCL.** Это иной метод маршрутизации. Положительный поздний результат не изолирует причину раннего неуспеха: менялись корпус и labels. Внутри него можно сравнить только top3freq с KNN.

**Измерения и потери.**

```json
{
  "examples": 450,
  "train": 360,
  "validation": 90,
  "baseline_macro_f1": 0.4709,
  "enhanced_macro_f1": 0.5646,
  "enhanced_per_dataset": {
    "Generic": 1.0,
    "CSharp": 0.99,
    "TypeScript": 0.0,
    "BugFix": 0.82,
    "Orchestration": 0.57,
    "Tests": 0.0
  },
  "corrected_or_lost_rows": "not stored in reviewed receipt;supports/precision/recall not recoverable"
}
```

**Цена.** Дополнительные 100,4 мс на 90 строк — примерно 1,116 мс на строку. Нельзя автоматически отвергать опыт по отношению к округлённым 0,1 мс baseline. Но размер памяти и устойчивость времени не установлены.

**Обобщение.** Цель проверки — воспроизведение автоматической разметки, а не независимая оценка полезности разделов. Упорядоченный список 450 принятых файлов не восстановлен. Построчный баланс исправлений и потерь, precision, recall и support не сохранены в прочитанном результате.

**Текущее состояние.** PredicateDatasetRouter с памятью есть в проекте 22, runner его использует. Это не доказательство улучшения публичного маршрута или последующего нейронного обучения.

**Почему не в основном ряду.** Не основной: небольшой прирост proxy-F1, неполные данные ошибок и потерь, невосстановленный корпус и отсутствие независимого эталона.

**Первичные источники:**

- [35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json:1–18](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json#L1-L18) — positive aggregate.
- [35.ExperimentsV2/Experiments/E22DatasetRouter.cs:27–128](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/35.ExperimentsV2/Experiments/E22DatasetRouter.cs#L27-L128) — actual top3 baseline and evaluation.
- [22.AgentAlgorithms/Context/PredicateDatasetRouter.cs:25–93](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L25-L93) — learned memory, majority and auto-label source.

### E13B-EXPRESSION-PREFIX-COMPLETION — exact_learned_retrieval_and_abstention

**Решение:** `exclude_main`.

**Старый способ.** Первый baseline всегда возвращает самое частое завершение Total.

**Новый способ.** Ранний опыт обучает KNN на 30 соответствиях «префикс → завершение» и проверяет те же 30. Поздний путь использует нормализованный точный lookup и отказ от неизвестного ключа.

**Что действительно училось.** Таблица соответствий определяется уроками. Вычисление значения новой формулы не обучается.

**Почему это не следует смешивать с исправлением HCL.** Это работающая предметная память, а не исправление адресации. Но проверка на тех же 30 строках не доказывает превосходство метода обобщения.

**Измерения и потери.**

```json
{
  "early_train": 30,
  "early_evaluation_same_train": 30,
  "early_baseline_pct": 6.7,
  "early_retrieval_pct": 100,
  "later_train": 20,
  "later_unseen": 10,
  "later_train_correct": 20,
  "later_unseen_answers_correct": 0,
  "later_unseen_correct_abstentions": 10,
  "norm_variants": "3knownformatvariantsmatched;1unknownA+Bcorrectlyrejected"
}
```

**Цена.** Парной оценки цены KNN и доменного lookup нет. Ранние 142,4 мс обучения не являются стоимостью позднего механизма.

**Обобщение.** В поздней таблице Correct=True для десяти новых формул означает правильный отказ. Правильных завершений новых формул — 0/10. Нормализация пробелов и регистра не равна выводу алгебраического правила.

**Текущее состояние.** В HEAD ExpressionCompletionCore проекта 23 содержит Train и lookup[Normalise(prefix)]=completion. При конфликте ключа последний урок заменяет предыдущий. Подтверждён экспериментальный потребитель, а не общая генерация RP01.

**Почему не в основном ряду.** Не основной: 100% — извлечение учебных соответствий; новых решённых формул нет. Парного качества до и после перехода KNN → lookup не найдено.

**Первичные источники:**

- [experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/run-2026-05-07-001715.md:1–52](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/run-2026-05-07-001715.md#L1-L52) — early train retrieval report.
- [experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/heldout-2026-05-07-075333/e13b-heldout-validation.md:1–36](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/results/heldout-2026-05-07-075333/e13b-heldout-validation.md#L1-L36) — heldout refusals, not completions.
- [experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/runner/E13BExpressionPrefixCompletionRunner/Program.cs:15–96](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/experiments-v2/E13B-EXPRESSION-PREFIX-COMPLETION/runner/E13BExpressionPrefixCompletionRunner/Program.cs#L15-L96) — same rows trained and evaluated.
- [23.SemanticCompilerAdapter/ExpressionCompletionCore.cs:43–107](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/23.SemanticCompilerAdapter/ExpressionCompletionCore.cs#L43-L107) — current exact learned lookup.

### KNN-DOMAIN-CONSTRUCTOR-ROUTE-01 — learned_router_and_model_registry_plus_manual_readout

**Решение:** `exclude_main_but_keep_B_vs_C_support`.

**Старый способ.** A-fixed всегда выбирает модель C#. Первичный отчёт прямо называет её контролем ошибки. Это намеренно неподходящая модель для остальных профилей, а не сильный корректный baseline маршрутизации.

**Новый способ.** Обученный маршрутизатор выбирает профиль, реестр разрешает состояние и функции. B-domain использует обычное чтение, C-domain-functions добавляет функции KDP. Новый урок записывается в выбранную память.

**Что действительно училось.** 24 урока маршрутизатора с профильными метками: 12 исходных и 12 дополнительных. Доменные состояния взяты из KDP. Отдельный урок break увеличивает C#-память с восьми до девяти записей; ответ throw меняется на break и сохраняется после загрузки. Веса LLM не обновляются.

**Почему это не следует смешивать с исправлением HCL.** Связка маршрутизатора и памяти может быть архитектурным методом. Однако сравнение 10/46 → 24/46 специально устраняет выбор чужой C#-модели. Превосходство над корректной альтернативной маршрутизацией не показано. Чистый эффект функций — B → C.

**Измерения и потери.**

```json
{
  "S1_B_correct_answers": 20,
  "S1_C_correct_answers": 24,
  "S1_B_correct_outcomes_with4refusals": 24,
  "S1_C_correct_outcomes_with4refusals": 28,
  "per_profile_S1": {
    "CS": [
      "8/8routed",
      "6/8B",
      "6/8C"
    ],
    "EN": [
      "8/8routed",
      "8/8B",
      "8/8C"
    ],
    "RU": [
      "0/8routed",
      "0/8B",
      "0/8C"
    ],
    "HE": [
      "0/8routed",
      "0/8B",
      "0/8C"
    ],
    "OR": [
      "8/8routed",
      "5/8B",
      "8/8C"
    ]
  },
  "routing_thresholds": {
    "minimum_score": 0.35,
    "minimum_margin": 0.05
  }
}
```

**Цена.** 7,853 с и пик 78 639 104 B относятся ко всему опыту. Это не парная стоимость выбора профиля или добавления функций. Расход пяти моделей и индексов отдельно не измерен.

**Обобщение.** Шестнадцать из сорока профильных запросов RU/HE не проходят маршрутизатор. Новый break воспроизводится только как один приобретённый урок, без проверки неизвестных аналогов. Правильные отказы считаются отдельно от ответов. Квалификация S0 считает TopProfile: часть верных меток имеет отрыв ниже 0,05, поэтому узнавание класса не означает допуск запроса к модели.

**Текущее состояние.** ProfileRouter и DomainModelFactory есть в эксперименте KDR01. Подключение к общему продуктовому пути не подтверждено; repair 455398 относится к другому FIX-KR.

**Почему не в основном ряду.** Не отдельная основная карточка: baseline A намеренно ошибочный; чистый B → C повторяет KDP и добавляет полезную проверку полного пути, но RU и HE остаются на 0/8.

**Первичные источники:**

- [experiments-v2/RP01-MICRO/KDR01/results/attempt-01/report.md:1–226](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/report.md#L1-L226) — primary report and explicit A-fixed error control.
- [experiments-v2/RP01-MICRO/KDR01/results/attempt-01/raw.json:1–7060](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/raw.json#L1-L7060) — 276raw route/arm/state records; answers vsrefusals calculated separately.
- [experiments-v2/RP01-MICRO/KDR01/results/attempt-01/router-qualification.json:1–394](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/router-qualification.json#L1-L394) — router labels and margins.
- [experiments-v2/RP01-MICRO/KDR01/results/attempt-01/cs-lesson.json:1–66](https://github.com/lubochka/llm_mvp_core/blob/846144464a856cd3f118bcfc0165c8117b06c167/experiments-v2/RP01-MICRO/KDR01/results/attempt-01/cs-lesson.json#L1-L66) — one new lesson andretention.
- [experiments-v2/RP01-MICRO/KDR01/ProfileRouter.cs:24–118](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/experiments-v2/RP01-MICRO/KDR01/ProfileRouter.cs#L24-L118) — current experimental router and rejectionthresholds.
- [experiments-v2/RP01-MICRO/KDR01/DomainModelFactory.cs:93–155](https://github.com/lubochka/llm_mvp_core/blob/75107af930a1d4c55d60fa8a4ef7c01643f13cb0/experiments-v2/RP01-MICRO/KDR01/DomainModelFactory.cs#L93-L155) — current experimental model/functionregistry.

## Особо важный контраст E25: память точных примеров уже объясняет итог

Из сохранённого PQ JSON прочитаны 128 учебных строк и 32 экзаменационных ответа. В памяти нет противоречащих меток у одной POS-последовательности. Для 26 вопросов есть точное совпадение с правильной меткой. На шести остальных сохранённый старый ответ правил верен дважды. Следовательно, **точный поиск по памяти с прежним правилом при отсутствии ключа даёт 26+2=28 из 32**. Это вычисленный из данных контрпример сильному выводу о преимуществах KNN; новый классификатор не запускался, скорость гипотетического lookup не измерялась.

При этом у KNN на шести новых входах два улучшения и два ухудшения: итог остаётся 2/6. Потому 87,5%нельзя использовать как доказательство обобщения синтаксического порядка. Теорема книги о потере порядка также не доказывает преимущество KNN именно в этойпанели.

## Действительно прочитанные навыки

Полностью прочитаны архитектурное объяснение, словарь человеческой коммуникации, исследование кода, перевод намерения в план и промпт-патчи. В plan-review прочитаны относящиеся к этому заданию разделы об актуальном поручении, точной области доказательства, загрузке навыков и недопустимости выдавать точную память за обобщение. Весь большой plan-review не объявляется прочитанным. Правила исполнения и запуска моделей не применялись, поскольку разрешён только исследовательский разбор.

- `human-readable-architecture-audit-SKILL.md` — read_full; строки 1–442.
- `human-communication-GLOSSARY.md` — read_full; строки 1–47.
- `code-examination-SKILL.md` — read_full; строки 1–376.
- `intent-to-plan-SKILL.md` — read_full; строки 1–225.
- `prompt-patch-authoring-SKILL.md` — read_full; строки 1–242.
- `plan-review-SKILL.md` — read_relevant_sections_not_full; строки 1–240, 2080–2165.

## Проверка и оставшиеся границы

JSON разобраны; арифметика TCTX, пересечение PQ, все 42 группы KDP и 6 групп KDR пересчитаны по сохранённым данным. Диапазоны строк каждого источника сверены с локальным файлом. Это проверка исследовательского пакета, а не повторный эксперимент.

Не восстановлены бинарники старых майских запусков и полные списки данных E18/E22. Нет проверки долгосрочного обобщения и ресурсных A/B для KDP/KDR. Наличие файла в HEAD не означает подключения ко всем потребителям. Эти границы сохранены в выводах.

