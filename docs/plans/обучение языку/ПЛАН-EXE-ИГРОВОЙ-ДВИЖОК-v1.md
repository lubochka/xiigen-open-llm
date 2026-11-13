# XIIGen Translation Trap — EXE Smart Game Engine
# Версия: 1.3 — 2026-05-29
# Статус: Планирование

---

## Цель

Превратить текущий HTML-файл игры в закрытый Windows EXE, где EXE — это не просто обёртка вокруг HTML, а **умный игровой движок** на базе `ILearningMachine<StudentDomain>`.

Игра адаптируется к каждому игроку. Следующий вопрос выбирается не случайно — он выбирается из того угла, где игрок слабее всего.

---

## Что уже есть в игре (v17 — текущее состояние index.html)

**Девять игровых режимов — реализованы:**

| Режим | Кнопка | Что делает игрок |
|---|---|---|
| Quiz | `startBtn` | Выбирает правильный перевод из 2 вариантов |
| Quick Round | `quickBtn` | 12 случайных упражнений без настройки |
| Daily Trap | `dailyBtn` | Детерминированный набор 5 упражнений на дату |
| Build (собери) | `buildBtn` | Собирает перевод из тайлов-слов |
| Find (найди ловушку) | `findBtn` | Кликает на слово-ловушку в плохом переводе |
| Memory (карточки) | `memBtn` | Находит пары «плохой ↔ живой перевод» |
| Proverbs (пословицы) | `proverbBtn` | Quiz на пословицах с послойным разбором |
| Study (разбор по слоям) | `studyBtn` | Послойное раскрытие: оригинал → дословно → живой → разбор |
| Mix Session | автоматически | Авто-сессия quiz + find + memory без ручного выбора |

**Дополнительные функции:**
- `reviewBtn` — повтор пропущенных упражнений (записная книжка)
- `trainSavedBtn` — тренировка из сохранённой коллекции ловушек
- Ссылка-челлендж `?daily=ГГГГ-ММ-ДД` — шеринг Daily Trap
- Экспорт ошибок раунда в Anki (`.txt`)
- Share-карточка PNG (canvas) с результатом
- Клавиатура: 1/2 или А/Б для ответа, Enter для продолжения
- Озвучка фраз (Web Speech API)
- Онбординг при первом запуске
- Сюжетная рамка «ты — редактор XIIGen»

**Трекинг в localStorage:**
- `rightBySkill[type]` — верно по каждому типу ошибки
- `wrongBySkill[type]` — неверно по каждому типу
- `bestMode[mode]` — лучший результат по режиму
- `dailyStreak` — дни подряд Daily Trap
- `xp` — накопленный опыт между запусками
- `badges` — 7 достижений (Первая поимка, Серия 10, Калька-бой, ...)
- Уровни: Новичок → Охотник → Продвинутый → Носитель ловушек (по XP)
- Полосы навыков по типам ошибок (накопительная точность %)
- Статистика: сыграно раундов, поймано ловушек, рекорд серии
- `savedTraps` — сохранённые ловушки для повторной тренировки

**Типы ошибок (7 доменов размечены):**
`false_friend` · `literal_translation` · `missing_idiom` · `wrong_register` · `grammar_error` · `wrong_word_choice` · `missing_word`

**Данные:**
- 44 курированные пары с разбором на русском — пул по умолчанию
- 1849 DPO-пар RU↔EN — полный набор за переключателем
- 14 пословиц с фонетикой и послойным форматом

**Что по-прежнему НЕ реализовано (цель EXE-движка):**
- Адаптивный выбор вопроса — Mix Session использует фиксированные пропорции, не данные игрока
- `response_time_ms` — скорость ответа не захватывается
- Thompson Sampling / Beta posterior — нет
- Векторное хранилище — только счётчики rightBySkill/wrongBySkill
- Placement test (определение уровня) — нет
- Tier progression по accuracy gate — уровни сейчас XP-based, не accuracy-based
- Multi-language routing — только RU↔EN

---

## Архитектура EXE-движка

```
┌─────────────────────────────────────────┐
│              EXE (Tauri)                │
│                                         │
│  ┌─────────────┐   ┌─────────────────┐  │
│  │  Game UI    │   │  Smart Engine   │  │
│  │  (HTML/JS)  │◄──│  (Rust/C#)      │  │
│  │             │   │                 │  │
│  │  - вопрос   │   │  ExerciseBank   │  │
│  │  - ответы   │   │  StudentProfile │  │
│  │  - счёт     │   │  Thompson KNN   │  │
│  └─────────────┘   └─────────────────┘  │
│                           │             │
│                    Local storage        │
│                    (игрок сохранён)     │
└─────────────────────────────────────────┘
```

**Что внутри EXE — максимум:**
- 1849 упражнений (ExerciseBank embedded)
- `StudentProfileStore` — локальный файл с векторами ответов игрока
- Thompson Sampling KNN — выбор следующего упражнения по слабым паттернам
- `StudentPersonalRegistry` — прогресс по языковым направлениям
- Tier-продвижение и per-domain gate — локально
- Сохранение/загрузка прогресса — локально (ExportAsync / ImportAsync)

**Что остаётся на сервере (XIIGen API) — минимум:**
- Генерация новых упражнений сверх базовых 1849
- Продвинутые объяснения ошибок (AI-powered)
- Cross-player analytics и обновления модели

Игра полностью работает offline. Сервер только расширяет возможности.

---

## Что меняется в игровой логике

| Сейчас | После (умный движок) |
|---|---|
| Выбор вопроса: фиксированные пропорции в Mix Session | Thompson Sampling: (упражнение × режим) с наименьшим posterior |
| `rightBySkill/wrongBySkill` — счётчики | `StudentResponseVector` с `response_time_ms` и `exercise_signature` |
| `bestMode` — лучший % по режиму | Posterior Beta per `(error_type × game_mode)` — система учится |
| Прогресс в localStorage (только этот браузер) | `StudentProfileStore` — SQLite, персистентный между сессиями |
| Один маршрут для всех в Mix | Персональный маршрут по слабым паттернам (из профиля) |
| Нет различия внутри домена | KNN различает структурные паттерны внутри одного error_type |
| Уровни XP-based (Новичок → Носитель) | Tier 1 → 2 → 3 по per-domain accuracy gate ≥ 0.80 |

**Что остаётся без изменений:** весь UI всех девяти режимов, XP/badges/уровни/streak, daily challenge, записная книжка missed, сохранённые ловушки, share/Anki-экспорт, послойный разбор, пословицы — всё это уже хорошо сделано и не переписывается.

---

## Игровые режимы как измерение в exercise_signature

Движок не назначает режим упражнению. Движок выбирает пару `(упражнение, режим)` через Thompson Sampling — одновременно.

```
exercise_signature = hash(
  error_type       +   // calque, grammar, false_friend...
  phrase_structure +   // NP+VB, SUBJ+VB...
  sentence_length  +
  game_mode            // quiz | memory | wordsearch
)
```

Для каждой комбинации `(упражнение, режим)` накапливается свой `Beta(correct+1, wrong+1)` posterior. Thompson Sampling сэмплирует из всего пространства и выбирает с наименьшим сэмплом.

Результат: система обнаруживает для каждого конкретного игрока, какой режим работает на каком паттерне. Для одного игрока quiz + calque работает отлично. Для другого — только memory + calque. Данные решают, не правила.

Сессия из 10 упражнений может оказаться любой комбинацией. Никакой предустановленной пропорции Quiz/Memory/WordSearch.

**Все режимы — готовы в HTML, нужно подключить к engine:**

| Режим | Статус HTML | Что захватывает engine |
|---|---|---|
| Quiz | ✅ Готов | is_correct + response_time_ms |
| Find (найди ловушку) | ✅ Готов | is_correct + response_time_ms |
| Build (собери из тайлов) | ✅ Готов | is_correct + response_time_ms |
| Memory (карточки) | ✅ Готов | is_correct + response_time_ms |
| Mix Session | ✅ Готов (фикс. пропорции) | → заменить на Thompson Sampling |

Формат ответа одинаков для всех режимов — один `StudentResponseVector`. UI существующих режимов не трогаем — только добавляем перехват `response_time_ms` и hook к sidecar.

---

## Технический путь

### Шаг 1 — Tauri wrapper (EXE без изменения логики)

Обернуть текущий `index.html` в Tauri → Windows `.exe`.

Что получаем:
- Закрытый продукт, HTML не виден в браузере
- Работает offline
- Branding XIIGen

Ворота: игра запускается как `.exe`, все 1849 упражнений работают.

---

### Шаг 2 — Smart Engine: замена статичной выборки на Thompson Sampling KNN

Вынести логику выбора вопроса из HTML в Rust-backend Tauri (или в embedded .NET через sidecar).

Что добавляем:
- `ExerciseBank` — читает встроенный JSONL
- `StudentResponseVector` — запись каждого ответа: exercise_signature, is_correct, response_time
- `StudentProfileStore` — локальный файл (SQLite или JSON)
- Thompson Sampling по KNN posterior — следующий вопрос из слабейшего паттерна

Ворота: после 20 ответов игра начинает выбирать из паттернов, где игрок ошибался чаще всего.

---

### Шаг 3 — Placement + Tier progression

Добавить:
- Первые 10 вопросов = placement (стратифицированная выборка)
- После placement → `StudentCapabilityProfile` → начало адаптивных сессий
- Per-domain gate: домен mastered когда accuracy ≥ 0.80 за последние 3 батча
- Tier promotion когда все домены mastered

Ворота: новый игрок проходит placement и получает персональный маршрут.

---

### Шаг 4 — StudentPersonalRegistry + multi-language

Добавить:
- `GetOrchestratorAsync(language_direction)` — роутинг по языку
- Отдельные `StudentProfileStore` для RU_EN, EN_RU, RU_FR
- Игрок видит прогресс по каждому языку отдельно

Ворота: игрок учит два языка, прогресс сохраняется независимо для каждого.

---

### Шаг 5 — Дистрибуция

```
Шаг 1-2 готов
  → itch.io: XIIGen Translation Trap — Windows Demo (downloadable)
  → PR: "AI-powered language trap game, adapts to your mistakes"

Шаг 3-4 готов
  → Steam Coming Soon
  → Steam demo
  → Steam Next Fest
  → YouTube / Reddit
```

---

## ILearningMachine<StudentDomain> как движок

EXE IS the `ILearningMachine<StudentDomain>` client:

```
Level 5  ILearningMachine<StudentDomain>
           TrainBatchAsync → обновить StudentProfileStore после сессии
           EvaluateAsync   → placement и per-domain метрики
           GetOrchestratorAsync(language) → Thompson Sampling orchestrator
           ExportAsync / ImportAsync → save/load игрового профиля

Level 4  StudentOrchestrator
           RunAsync → следующие 10 вопросов для текущего игрока

Level 2  PlacementUnit, SessionComposerUnit, ProgressVerifierUnit
           — все работают локально внутри EXE —
```

Никакого серверного вызова для выбора следующего вопроса. Интеллект внутри EXE.

---

## Технические решения (принятые)

### C# проект — 30.StudentDomain

`ILearningMachine<StudentDomain>` реализуется в новом проекте **30.StudentDomain** в решении `llm_mvp_core`.

Соседство в solution:
```
27.TranslationDomain   ← источник упражнений (ExerciseBank читает экспорт)
28.SemanticNlp
29.SemanticCompiler
30.StudentDomain       ← новый проект
```

30.StudentDomain содержит:
- `ILearningMachine<StudentDomain>` — реализация контракта
- `StudentProfileStore` — локальное хранилище профиля игрока
- `ExerciseBankAdapter` — читает тренировочные пары из 27.TranslationDomain
- `ThompsonSamplingEngine` — Beta posterior + KNN по exercise_signature
- `PlacementUnit`, `SessionComposerUnit`, `ProgressVerifierUnit`

---

### Мост Tauri ↔ C# — .NET sidecar

Tauri управляет окном и упаковывает HTML/JS. C# логика живёт как **sidecar** — отдельный `.exe`, который Tauri запускает при старте игры.

Коммуникация: **stdin/stdout JSON** (простейший протокол без сетевого стека).

```
Tauri (Rust)
  → запускает 30.StudentDomain.Runner.exe при старте
  → отправляет JSON-запрос в stdin: { "method": "NextExercise", "studentId": "..." }
  ← получает JSON-ответ из stdout: { "exercise": {...}, "mode": "quiz" }
```

Почему sidecar, не Rust:
- Thompson Sampling KNN уже будет написан на C#
- Не нужно переписывать на Rust
- Sidecar bundled внутри EXE через Tauri — пользователь не видит

---

### Хранение профиля игрока — SQLite

Формат: **SQLite** (`Microsoft.Data.Sqlite` NuGet — разрешён как non-ML utility).

Почему не JSON:
- Beta posterior хранится по каждой паре (exercise_id × game_mode) — при 1849 × 4 режимах = 7396 записей
- JSON одним файлом медленно обновляется; SQLite даёт O(1) UPDATE по ключу

Путь к файлу:
```
%LOCALAPPDATA%\XIIGen\Translation Trap\profile.db
```
Стандарт для Windows desktop (itch.io, Steam): не требует прав администратора, не теряется при обновлении игры.

---

### Защита данных — Windows DPAPI

`StudentProfileStore` шифруется через **Windows Data Protection API (DPAPI)**.

```csharp
// Шифрование: привязано к учётной записи Windows
ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser)

// Расшифровка:
ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.CurrentUser)
```

Почему DPAPI:
- Нативно для Windows (target платформа — itch.io Windows demo)
- Ключ привязан к Windows-аккаунту пользователя — другой пользователь на той же машине не читает профиль
- Нет зависимости от внешней библиотеки шифрования

---

## Что защищено, что нет

Tauri упаковывает HTML/JS. Технически можно извлечь ассеты.
Главная интеллектуальная часть XIIGen:
- Thompson Sampling KNN — в Rust backend, не в JS
- StudentProfileStore — локальный зашифрованный файл
- Логика gate и Tier — в backend

Если XIIGen API подключён — продвинутые объяснения и новые упражнения остаются на сервере полностью.

---

## Зависимости

| Зависимость | Статус |
|---|---|
| `index.html` (1849 упражнений, 9 режимов, XP/levels/badges/streak/skills) | ✅ Готово (v17) |
| Tauri toolchain | Установить |
| **30.StudentDomain** (C# проект в llm_mvp_core) | Нужно создать |
| `ILearningMachine<StudentDomain>` контракты | В плане (ПЛАН-АДАПТИВНЫЙ-МЕНЕДЖЕР-ПРОГРЕССА-v1.md) |
| Thompson Sampling KNN реализация | Нужно реализовать |
| Steam аккаунт ($100 fee) | Нужно открыть |

---

*Создан: 2026-05-29*
*Версия 1.3 — обновлено по анализу index.html (v17, 1st game.zip)*
*Автор плана: Claude Sonnet 4.6*
