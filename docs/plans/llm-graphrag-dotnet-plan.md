# LLM и Graph RAG — план перевода на .NET
## Solution: LlmFromScratch.sln — 11 подпроектов

Каждый подпроект — отдельная библиотека (Class Library) или консольное приложение.
Зависимости только между своими проектами — никаких внешних NuGet пакетов кроме TorchSharp для матричных операций.

Порядок реализации соответствует зависимостям: каждый следующий проект использует предыдущий.

---

## Структура Solution

```
LlmFromScratch.sln
├── 01.Tokenizer/              — BPE токенизатор
├── 02.DatasetPipeline/        — подготовка датасета
├── 03.TransformerModel/       — архитектура модели
├── 04.TrainingLoop/           — тренировочный цикл
├── 05.Embeddings/             — векторные представления текста
├── 06.VectorSearch/           — поиск по векторам
├── 07.GraphEngine/            — граф знаний и PageRank
├── 08.GraphRag/               — Graph RAG система
├── 09.Inference/              — генерация токенов
├── 10.LoRA/                   — дообучение без переобучения всей модели
├── 11.DPO/                    — обучение на человеческих предпочтениях
└── Tests/                     — тесты каждого проекта
```

---

## Проект 01 — Tokenizer (BPE токенизатор)

### Что это и зачем

BPE расшифровывается как Byte Pair Encoding — кодирование парами байтов. Это алгоритм который превращает текст в числа (токены) так чтобы частые слова и их части были короткими числами, а редкие — длинными последовательностями. Без токенизатора модель вообще не может работать с текстом — она принимает только числа.

Пример: слово "playing" может быть разбито как ["play", "##ing"] → [4821, 2480]. Это позволяет модели понять что "playing", "played", "player" — родственные слова, потому что они начинаются с одного токена "play".

### Источник для изучения

```
https://github.com/karpathy/minbpe
MIT · Python

minbpe/basic.py      — читать полностью, 150 строк
```

### Что реализовать

```csharp
// Tokenizer/BpeTokenizer.cs

public class BpeTokenizer
{
    // Словарь: индекс → строка байтов
    // Например: 4821 → "play", 2480 → "ing"
    private Dictionary<int, byte[]> vocab;

    // Таблица слияний: пара токенов → новый токен
    // Например: (4821, 2480) → 7301 означает "play"+"ing" = "playing"
    private Dictionary<(int, int), int> merges;

    // ОБУЧЕНИЕ: читаем текст, считаем какие пары байтов встречаются
    // вместе чаще всего, сливаем самую частую пару в новый токен,
    // повторяем пока не достигнем нужного размера словаря
    public void Train(string text, int vocabSize)
    {
        // Шаг 1: разбить текст на байты (каждый байт = токен 0..255)
        // Шаг 2: найти самую частую пару соседних токенов
        // Шаг 3: заменить все вхождения этой пары новым токеном
        // Шаг 4: записать слияние в merges
        // Шаг 5: повторить с шага 2 пока vocab.Count < vocabSize
    }

    // КОДИРОВАНИЕ: текст → список числовых токенов
    // Применяем выученные слияния в том же порядке что при обучении
    public List<int> Encode(string text) { ... }

    // ДЕКОДИРОВАНИЕ: список числовых токенов → текст
    // Обратный словарь: число → байты → строка
    public string Decode(List<int> tokens) { ... }

    // Сохранить/загрузить словарь на диск
    public void Save(string path) { ... }
    public static BpeTokenizer Load(string path) { ... }
}
```

### Как работает внутри

```
Входной текст:  "hello world hello"
Байты:          [104,101,108,108,111, 32,119,111,114,108,100, 32,104,101,108,108,111]

Итерация 1: самая частая пара (108,108) встречается 2 раза
            → объединяем в новый токен 256
            [104,101,256,111, 32,119,111,114,256,100, 32,104,101,256,111]

Итерация 2: самая частая пара (104,101) встречается 2 раза
            → объединяем в новый токен 257
            [257,256,111, 32,119,111,114,256,100, 32,257,256,111]

...и так до нужного размера словаря
```

### Файлы проекта

```
01.Tokenizer/
├── BpeTokenizer.cs     — основной класс
├── VocabEntry.cs       — запись словаря
├── RegexSplitter.cs    — предварительное разбиение текста на слова
└── Tests/
    └── BpeTokenizerTests.cs
```

---

## Проект 02 — DatasetPipeline (подготовка датасета)

### Что это и зачем

Модель не читает файлы кода напрямую. Ей нужен непрерывный поток числовых токенов, нарезанный на куски одинаковой длины. Этот проект читает все исходные файлы, токенизирует их, склеивает в один длинный поток, нарезает на куски по 2048 токенов и сохраняет на диск в формате uint16 массивов.

uint16 означает что каждый токен занимает 2 байта (числа от 0 до 65535). Это компромисс между точностью (большинство словарей меньше 65535 токенов) и размером файла (вместо 4 байт float32 используем 2 байта).

### Источник для изучения

```
https://github.com/karpathy/nanoGPT
MIT · Python

data/shakespeare_char/prepare.py   — читать полностью, ~50 строк
```

### Что реализовать

```csharp
// DatasetPipeline/DatasetBuilder.cs

public class DatasetBuilder
{
    private readonly BpeTokenizer tokenizer;
    private readonly int seqLen;       // длина одного куска (обычно 2048)
    private readonly int shardSize;    // токенов на один файл (10 миллионов)

    // Главный метод: папка с файлами кода → папка с .npy файлами
    // 1. Обходим все .ts, .cs, .py файлы рекурсивно
    // 2. Каждый файл токенизируем
    // 3. Добавляем специальный токен конца файла между файлами
    // 4. Когда накопили shardSize токенов — сохраняем шард
    public async Task BuildAsync(string sourceDir, string outputDir) { ... }

    // Читает следующий батч из шардов для тренировки
    // Возвращает (входные токены, целевые токены)
    // Целевые — это входные сдвинутые на 1 позицию вправо
    // Модель учится предсказывать следующий токен
    public (long[,] inputs, long[,] targets) NextBatch() { ... }

    // Сохраняет массив uint16 в формате NumPy .npy
    // Формат: 128-байтный заголовок + данные
    private void SaveShard(ushort[] tokens, string path) { ... }

    // Читает .npy файл обратно в массив
    private ushort[] LoadShard(string path) { ... }
}
```

### Как устроен .npy файл

```
Байты 0-5:    магическое число "\x93NUMPY"
Байты 6-7:    версия формата (1, 0)
Байты 8-9:    длина заголовка
Байты 10-127: заголовок — описание массива в Python dict формате
              {'descr': '<u2', 'fortran_order': False, 'shape': (10000000,)}
Байты 128+:   данные — uint16 числа подряд без разделителей
```

### Файлы проекта

```
02.DatasetPipeline/
├── DatasetBuilder.cs    — сборка датасета из файлов
├── NpyWriter.cs         — запись .npy формата
├── NpyReader.cs         — чтение .npy формата
├── DataLoader.cs        — подача батчей во время тренировки
└── Tests/
    └── DatasetTests.cs
```

---

## Проект 03 — TransformerModel (архитектура нейронной сети)

### Что это и зачем

Transformer — архитектура нейронной сети опубликованная Google в 2017 году в статье "Attention Is All You Need". Сейчас на ней построены все большие языковые модели — GPT, Claude, Llama, OLMo. Основная идея: вместо того чтобы читать текст слева направо последовательно (как это делали старые рекуррентные сети), каждое слово одновременно "смотрит" на все другие слова и решает какие из них важны для его понимания.

Механизм внимания (Attention) — это математический способ спросить "насколько слово X связано со словом Y в данном контексте". Модель сама обучается понимать эти связи.

### Источник для изучения

```
https://github.com/karpathy/nanoGPT
MIT · Python

model.py   — читать полностью, 300 строк
```

### Что реализовать

```csharp
// TransformerModel/Layers/RmsNorm.cs
// Нормализация значений в слое перед вычислениями
// Без нормализации числа разрастаются и обучение расходится
// RMS расшифровывается как Root Mean Square — квадратный корень
// из среднего квадратов. Проще чем LayerNorm и работает не хуже.
public class RmsNorm : Module<Tensor, Tensor>
{
    // gamma — обучаемые веса масштабирования, один на каждое измерение
    private readonly Parameter gamma;
    private readonly float eps; // маленькое число чтобы не делить на ноль

    // Формула: x / sqrt(mean(x^2) + eps) * gamma
    public override Tensor forward(Tensor x) { ... }
}

// TransformerModel/Layers/RotaryEmbedding.cs
// RoPE — Rotary Position Embedding, кодирование позиции в тексте
// Модель не знает что токен 5 идёт после токена 4 — ей нужно сказать.
// RoPE вращает векторы запроса и ключа на угол зависящий от позиции.
// Это позволяет модели "видеть" расстояние между токенами.
public class RotaryEmbedding : Module
{
    // Получить матрицы косинусов и синусов для заданной длины последовательности
    public (Tensor cos, Tensor sin) GetFrequencies(int seqLen, Device device) { ... }
    // Применить вращение к вектору
    public static Tensor Apply(Tensor x, Tensor cos, Tensor sin) { ... }
}

// TransformerModel/Layers/CausalSelfAttention.cs
// Механизм самовнимания — сердце Transformer
// Causal (причинный) означает что токен может смотреть только назад
// но не вперёд. Иначе модель "читерила" бы при обучении —
// видела бы ответ до того как его предсказала.
//
// Q = Query (запрос) — что я ищу
// K = Key (ключ) — что я предлагаю другим
// V = Value (значение) — что я несу в себе
//
// GQA = Grouped Query Attention — несколько Q голов делят один K и V
// Экономит память в 4-8 раз при небольшой потере качества
public class CausalSelfAttention : Module<Tensor, Tensor>
{
    private readonly Linear q_proj; // проекция запроса
    private readonly Linear k_proj; // проекция ключа
    private readonly Linear v_proj; // проекция значения
    private readonly Linear o_proj; // выходная проекция
    private readonly RotaryEmbedding rope;

    // Вычислить внимание:
    // 1. Спроецировать x в Q, K, V
    // 2. Применить RoPE к Q и K
    // 3. Scores = Q @ K^T / sqrt(head_dim)
    // 4. Маска: будущие позиции = -бесконечность
    // 5. Weights = softmax(scores)
    // 6. Output = weights @ V
    // 7. Спроецировать обратно через o_proj
    public override Tensor forward(Tensor x) { ... }
}

// TransformerModel/Layers/FeedForward.cs
// Двухслойная нейросеть после каждого блока внимания
// Если внимание отвечает на вопрос "на что смотреть"
// то FFN отвечает на вопрос "что с этим делать"
//
// SwiGLU = Swish-Gated Linear Unit
// Более современная функция активации чем ReLU
// gate контролирует сколько информации пропускать
// up расширяет размерность в 4 раза
// down сжимает обратно
public class SwiGLUFeedForward : Module<Tensor, Tensor>
{
    private readonly Linear gate_proj; // ворота — что пропускать
    private readonly Linear up_proj;   // расширение
    private readonly Linear down_proj; // сжатие обратно

    // gate = silu(x @ gate_proj)  — плавный переключатель
    // up   = x @ up_proj
    // return (gate * up) @ down_proj
    public override Tensor forward(Tensor x) { ... }
}

// TransformerModel/TransformerBlock.cs
// Один блок Transformer = нормализация + внимание + нормализация + FFN
// Плюс остаточные связи (residual connections):
// x = x + attention(norm(x))
// x = x + ffn(norm(x))
// Остаточные связи позволяют градиентам течь
// через 32+ блока без затухания
public class TransformerBlock : Module<Tensor, Tensor> { ... }

// TransformerModel/LlmModel.cs
// Полная модель: embedding + N блоков + финальная нормализация + lm_head
// lm_head — Language Model Head, проекция из D измерений в vocab_size
// чтобы получить вероятность каждого токена как следующего
public class LlmModel : Module<Tensor, Tensor>
{
    private readonly Embedding token_embedding; // таблица: токен → вектор
    private readonly ModuleList<TransformerBlock> blocks;
    private readonly RmsNorm final_norm;
    private readonly Linear lm_head; // проекция на словарь

    public override Tensor forward(Tensor tokenIds) { ... }
}
```

### Файлы проекта

```
03.TransformerModel/
├── Config/
│   └── ModelConfig.cs       — параметры архитектуры (слои, размеры)
├── Layers/
│   ├── RmsNorm.cs
│   ├── RotaryEmbedding.cs
│   ├── CausalSelfAttention.cs
│   └── SwiGLUFeedForward.cs
├── TransformerBlock.cs
├── LlmModel.cs
└── Tests/
    └── ModelTests.cs
```

---

## Проект 04 — TrainingLoop (тренировочный цикл)

### Что это и зачем

Тренировочный цикл — это процесс обучения модели. Модель делает предсказание, сравнивает его с правильным ответом, вычисляет насколько она ошиблась (Loss — функция потерь), и немного корректирует свои веса в направлении уменьшения ошибки. Этот процесс повторяется миллионы раз на миллиардах токенов.

AdamW — это алгоритм оптимизации весов. Расшифровывается как Adaptive Moment estimation with Weight decay. "Адаптивный" означает что скорость обучения автоматически настраивается отдельно для каждого веса — медленно меняющиеся веса учатся быстрее.

Gradient Accumulation — накопление градиентов. Позволяет имитировать большой батч (например 32 примера) при ограниченной видеопамяти — обрабатываем по 2 примера 16 раз прежде чем обновить веса.

### Источник для изучения

```
https://github.com/karpathy/nanoGPT
MIT · Python

train.py   — читать полностью, 300 строк
```

### Что реализовать

```csharp
// TrainingLoop/Optimizers/AdamW.cs
// Обновляет веса модели после каждого шага
// m — момент первого порядка: куда мы двигались последние шаги
// v — момент второго порядка: насколько нестабильно мы двигались
// weight_decay — штраф за большие веса, предотвращает переобучение
public class AdamW
{
    private readonly float lr;           // скорость обучения (learning rate)
    private readonly float beta1 = 0.9f; // коэффициент сглаживания момента m
    private readonly float beta2 = 0.95f; // коэффициент сглаживания момента v
    private readonly float weightDecay = 0.1f;
    private readonly float eps = 1e-8f;

    // step() вызывается один раз после backward()
    // Обновляет каждый вес: w = w - lr * m_hat / (sqrt(v_hat) + eps) - lr * wd * w
    public void Step(IEnumerable<Parameter> parameters) { ... }
}

// TrainingLoop/Schedulers/CosineWithWarmup.cs
// Планировщик скорости обучения
// Warmup — первые N шагов скорость линейно растёт от 0 до max_lr
//   без этого большие градиенты в начале разрушают случайно
//   инициализированные веса
// Cosine decay — после warmup скорость плавно снижается
//   по форме косинуса от max_lr до min_lr
public class CosineWithWarmup
{
    public float GetLr(int step) { ... }
}

// TrainingLoop/Trainer.cs
// Главный тренировочный цикл
public class Trainer
{
    // Один шаг обучения:
    // 1. Получить батч из DataLoader
    // 2. Forward pass: модель предсказывает следующий токен для каждой позиции
    // 3. Loss: cross-entropy между предсказанием и правильным токеном
    //    Cross-entropy = -log(вероятность правильного токена)
    //    Чем увереннее неправильный ответ — тем больше штраф
    // 4. Backward pass: вычислить градиенты (насколько каждый вес влияет на ошибку)
    // 5. Gradient clipping: ограничить градиенты чтобы не было взрывного роста
    // 6. Optimizer step: обновить веса
    // 7. Обнулить градиенты для следующего шага
    public async Task<float> TrainStepAsync(LlmModel model, DataLoader data) { ... }

    // Сохранить чекпоинт — снимок весов в определённый момент
    // Позволяет продолжить обучение после перерыва
    // или откатиться к предыдущей версии если что-то пошло не так
    public void SaveCheckpoint(LlmModel model, AdamW optimizer, int step, string dir) { ... }

    // Загрузить чекпоинт и продолжить обучение
    public void LoadCheckpoint(LlmModel model, AdamW optimizer, string path) { ... }
}
```

### Файлы проекта

```
04.TrainingLoop/
├── Optimizers/
│   └── AdamW.cs
├── Schedulers/
│   └── CosineWithWarmup.cs
├── Trainer.cs
├── TrainingConfig.cs   — batch_size, seq_len, lr, warmup_steps...
└── Tests/
    └── TrainerTests.cs
```

---

## Проект 05 — Embeddings (векторные представления текста)

### Что это и зачем

Эмбеддинг — это способ превратить текст в вектор чисел так чтобы похожие по смыслу тексты были близки в математическом пространстве. Например вектор слова "кошка" и вектор слова "кот" будут очень близки, а вектор слова "самолёт" — далеко.

В Graph RAG эмбеддинги нужны чтобы по вопросу на естественном языке найти похожие куски кода или документации. Вопрос "как работает аутентификация" превращается в вектор, и мы ищем векторы кода которые близки к этому вектору.

Mean Pooling — усреднение векторов всех токенов предложения в один вектор. Transformer выдаёт вектор для каждого токена, а нам нужен один вектор для всего текста.

### Источник для изучения

```
https://github.com/UKPLab/sentence-transformers
Apache 2.0 · Python

sentence_transformers/models/Pooling.py   — 150 строк
```

### Что реализовать

```csharp
// Embeddings/EmbeddingModel.cs
// Использует обученный Transformer (проект 03) для генерации эмбеддингов
// Отличие от языковой модели: нас интересует не lm_head
// а скрытое состояние после всех блоков
public class EmbeddingModel
{
    private readonly LlmModel encoder;

    // Текст → вектор фиксированной длины
    // 1. Токенизировать текст
    // 2. Прогнать через encoder (без lm_head)
    // 3. Применить mean pooling
    // 4. L2 нормализовать (длина вектора = 1)
    //    Нормализация нужна чтобы cosine similarity работала правильно
    public float[] Embed(string text) { ... }

    // Обработка нескольких текстов за один прогон (быстрее чем по одному)
    public float[][] EmbedBatch(IList<string> texts) { ... }
}

// Embeddings/MeanPooling.cs
// Усреднение векторов токенов с учётом маски внимания
// Маска внимания нужна потому что мы дополняем короткие тексты
// нулевыми токенами до фиксированной длины (padding)
// Нулевые токены не должны влиять на среднее
public static class MeanPooling
{
    // tokenVectors: (batch_size, seq_len, hidden_dim)
    // attentionMask: (batch_size, seq_len) — 1 для реальных токенов, 0 для padding
    // Результат: (batch_size, hidden_dim)
    public static Tensor Pool(Tensor tokenVectors, Tensor attentionMask) { ... }

    // L2 нормализация: вектор / ||вектор||
    // После нормализации все векторы имеют длину 1
    // и cosine similarity = dot product (быстрее вычислять)
    public static Tensor Normalize(Tensor vectors) { ... }
}
```

### Файлы проекта

```
05.Embeddings/
├── EmbeddingModel.cs
├── MeanPooling.cs
└── Tests/
    └── EmbeddingTests.cs
```

---

## Проект 06 — VectorSearch (поиск по векторам)

### Что это и зачем

Когда у нас есть эмбеддинги для тысяч кусков кода, нам нужно быстро найти те из них которые близки к вектору запроса. Можно сравнить запрос со всеми векторами подряд (brute force) — это точно но медленно. HNSW позволяет найти ближайших соседей в тысячи раз быстрее ценой небольшой потери точности.

HNSW расшифровывается как Hierarchical Navigable Small World — иерархический граф навигации в малом мире. Строится несколько слоёв графа: верхние слои редкие (быстрая навигация), нижние плотные (точный поиск). Поиск начинается сверху и спускается вниз.

Cosine Similarity — косинус угла между двумя векторами. Равен 1 если векторы совпадают, 0 если перпендикулярны, -1 если противоположны. Используется потому что нам важно направление вектора (смысл) а не его длина.

### Источник для изучения

```
https://github.com/nmslib/hnswlib
Apache 2.0 · C++

hnswlib/bruteforce.h    — читать первым, 200 строк
hnswlib/hnswalg.h       — читать вторым, 800 строк
hnswlib/space_cosine.h  — формулы расстояний, 30 строк
```

### Что реализовать

```csharp
// VectorSearch/Distances.cs
// Математические функции расстояния между векторами
public static class Distances
{
    // Cosine similarity = dot(a, b) / (||a|| * ||b||)
    // Если векторы нормализованы (длина = 1), то = просто dot(a, b)
    public static float CosineSimilarity(float[] a, float[] b) { ... }

    // Евклидово расстояние = sqrt(sum((a_i - b_i)^2))
    // Геометрическое расстояние в многомерном пространстве
    public static float EuclideanDistance(float[] a, float[] b) { ... }

    // Inner Product — скалярное произведение = sum(a_i * b_i)
    // Для нормализованных векторов совпадает с cosine similarity
    public static float InnerProduct(float[] a, float[] b) { ... }
}

// VectorSearch/BruteForceIndex.cs
// Наивный поиск: сравниваем запрос со всеми векторами подряд
// Медленно O(n) но абсолютно точно
// Использовать для: небольших коллекций (<10000 векторов),
// тестирования правильности HNSW
public class BruteForceIndex
{
    private readonly List<(float[] vector, string id)> items;

    public void Add(float[] vector, string id) { ... }

    // Вернуть topK ближайших векторов и их идентификаторы
    public List<(string id, float score)> Search(float[] query, int topK) { ... }
}

// VectorSearch/HnswIndex.cs
// Иерархический граф для быстрого приближённого поиска
// Строится один раз, поиск работает за O(log n)
public class HnswIndex
{
    // Параметры HNSW:
    // M — количество связей каждого узла (обычно 16)
    //   больше M = лучше качество но больше памяти
    // efConstruction — ширина поиска при построении (обычно 200)
    //   больше = лучше качество но дольше строится
    // efSearch — ширина поиска при запросе (обычно 50)
    //   больше = лучше качество но медленнее поиск
    private readonly int M;
    private readonly int efConstruction;
    private int efSearch;

    // Граф: для каждого узла список соседей на каждом уровне
    // Уровень узла выбирается случайно при вставке
    // Верхние уровни — редкие узлы для быстрой навигации
    // Нижний уровень — все узлы
    private readonly Dictionary<int, List<int>[]> graph;

    // Добавить вектор:
    // 1. Выбрать случайный максимальный уровень для нового узла
    //    (вероятность убывает экспоненциально с уровнем)
    // 2. С верхнего уровня до уровня+1: найти ближайший узел (жадно)
    // 3. На каждом уровне от уровня до 0: найти ef_construction ближайших
    //    и добавить M лучших как соседей нового узла
    // 4. Для каждого добавленного соседа: проверить его список соседей
    //    и возможно заменить дальнего соседа на новый узел
    public void Add(float[] vector, string id) { ... }

    // Найти topK ближайших:
    // 1. Начать с входного узла на верхнем уровне
    // 2. На каждом уровне кроме 0: жадно двигаться к ближайшему соседу
    // 3. На уровне 0: найти ef_search ближайших кандидатов
    // 4. Вернуть topK из кандидатов
    public List<(string id, float score)> Search(float[] query, int topK) { ... }

    // Сохранить индекс на диск и загрузить обратно
    public void Save(string path) { ... }
    public static HnswIndex Load(string path) { ... }
}
```

### Файлы проекта

```
06.VectorSearch/
├── Distances.cs
├── BruteForceIndex.cs
├── HnswIndex.cs
├── IndexEntry.cs       — запись: id + вектор + метаданные
└── Tests/
    └── SearchTests.cs  — проверить что HNSW находит то же что brute force
```

---

## Проект 07 — GraphEngine (граф знаний и PageRank)

### Что это и зачем

Граф знаний хранит связи между сущностями кода — классами, методами, файлами. Узел графа — это сущность (например метод ValidateToken), ребро — это связь (например "вызывает" или "определён в"). Граф позволяет найти не только прямое совпадение по тексту, но и структурный контекст — всё что связано с найденной сущностью.

PageRank — алгоритм ранжирования узлов графа изобретённый Ларри Пейджем для ранжирования веб-страниц. Узел важен если на него ссылаются другие важные узлы. В контексте кода: метод который вызывается из многих мест — важнее метода который вызывается редко.

Персонализированный PageRank — вариант где мы задаём начальные "веса интереса" для некоторых узлов (тех что нашли через векторный поиск), и алгоритм распространяет важность по графу от этих узлов.

### Источник для изучения

```
https://github.com/networkx/networkx
BSD 3-Clause · Python

networkx/algorithms/link_analysis/pagerank_alg.py   — 80 строк
```

### Что реализовать

```csharp
// GraphEngine/Graph.cs
// Направленный взвешенный граф
public class KnowledgeGraph
{
    // Узлы: id → сущность
    private readonly Dictionary<string, GraphNode> nodes;
    // Рёбра: from → list of (to, weight, relation_type)
    private readonly Dictionary<string, List<GraphEdge>> edges;

    public void AddNode(GraphNode node) { ... }
    public void AddEdge(GraphEdge edge) { ... }

    // Получить всех соседей узла (входящие и исходящие)
    // hops — на сколько шагов в глубину
    public IEnumerable<GraphNode> GetNeighbors(string nodeId, int hops = 2) { ... }

    // Все входящие рёбра — кто ссылается на этот узел
    public IEnumerable<GraphEdge> InEdges(string nodeId) { ... }
    // Все исходящие рёбра — на кого ссылается этот узел
    public IEnumerable<GraphEdge> OutEdges(string nodeId) { ... }
}

// GraphEngine/PageRank.cs
// Power iteration алгоритм:
// Начинаем с равномерного распределения важности (1/N для каждого узла)
// На каждой итерации: каждый узел отдаёт свою важность соседям
// пропорционально весу рёбер
// Повторяем пока распределение не стабилизируется
public class PageRankCalculator
{
    // damping — коэффициент затухания (обычно 0.85)
    //   Означает: с вероятностью 0.85 переходим по ссылке
    //   с вероятностью 0.15 телепортируемся в случайный узел
    //   Предотвращает ситуацию когда всё рейтинг уходит в изолированные кластеры
    private readonly double damping = 0.85;

    // Вычислить PageRank без персонализации
    public Dictionary<string, double> Compute(KnowledgeGraph graph, int iterations = 50) { ... }

    // Персонализированный PageRank:
    // personalization — словарь id → вес для узлов-якорей
    // Телепортация происходит не равномерно а в соответствии с personalization
    // Итог: важность распространяется от якорных узлов по графу
    public Dictionary<string, double> ComputePersonalized(
        KnowledgeGraph graph,
        Dictionary<string, double> personalization,
        int iterations = 50) { ... }
}

// GraphEngine/Models/GraphNode.cs
public record GraphNode
{
    public string Id          { get; init; } = "";
    public string Name        { get; init; } = "";
    public string Type        { get; init; } = ""; // Class, Method, File, Interface
    public string FilePath    { get; init; } = "";
    public string Signature   { get; init; } = ""; // полная сигнатура метода/класса
    public string RawText     { get; init; } = ""; // исходный код
    public float[] Embedding  { get; init; } = []; // вектор из проекта 05
    public double PageRankScore { get; set; }
}

// GraphEngine/Models/GraphEdge.cs
public record GraphEdge
{
    public string From         { get; init; } = "";
    public string To           { get; init; } = "";
    public string RelationType { get; init; } = ""; // Calls, Defines, Inherits
    public double Weight       { get; init; } = 1.0;
}
```

### Файлы проекта

```
07.GraphEngine/
├── KnowledgeGraph.cs
├── PageRankCalculator.cs
├── GraphSerializer.cs   — сохранение/загрузка графа в JSON
├── Models/
│   ├── GraphNode.cs
│   └── GraphEdge.cs
└── Tests/
    └── PageRankTests.cs
```

---

## Проект 08 — GraphRag (система Graph RAG)

### Что это и зачем

Graph RAG расшифровывается как Graph-based Retrieval Augmented Generation — генерация с извлечением информации на основе графа. Суть: перед тем как ответить на вопрос, система находит релевантный контекст из кодовой базы и добавляет его в запрос к модели. Без этого модель отвечала бы только на основе обучающих данных и не знала бы ничего о конкретном проекте.

Двухуровневый поиск:
- Локальный (local search) — векторный поиск по конкретным кускам кода. Быстро отвечает на вопросы типа "что делает метод X"
- Глобальный (global search) — обход графа с персонализированным PageRank. Отвечает на вопросы типа "как данные текут через систему аутентификации"

### Источник для изучения

```
https://github.com/gusye1234/nano-graphrag
MIT · Python

nano_graphrag/base.py   — читать полностью, 400 строк
```

### Что реализовать

```csharp
// GraphRag/Indexer.cs
// Индексирует кодовую базу: парсит файлы → строит граф → создаёт эмбеддинги
public class RagIndexer
{
    private readonly KnowledgeGraph graph;
    private readonly HnswIndex vectorIndex;
    private readonly EmbeddingModel embedder;

    // Полный pipeline индексации одного файла:
    // 1. Распарсить файл (через Roslyn или ts-morph subprocess)
    // 2. Извлечь сущности: классы, методы, интерфейсы
    // 3. Извлечь связи: вызовы, наследование, зависимости
    // 4. Добавить узлы и рёбра в граф
    // 5. Для каждой сущности создать эмбеддинг (название + сигнатура + код)
    // 6. Добавить эмбеддинг в векторный индекс
    public async Task IndexFileAsync(string filePath) { ... }

    // Вычислить PageRank для всего графа после индексации
    // Сохранить ранги в узлах графа
    public void ComputeRanks() { ... }
}

// GraphRag/LocalSearch.cs
// Быстрый поиск конкретных фактов по векторному индексу
public class LocalSearch
{
    private readonly HnswIndex index;
    private readonly KnowledgeGraph graph;

    // Найти topK узлов наиболее похожих на запрос по смыслу
    // Вернуть их исходный код как контекст
    public List<SearchResult> Search(float[] queryEmbedding, int topK = 10) { ... }
}

// GraphRag/GlobalSearch.cs
// Структурный поиск через обход графа с персонализированным PageRank
public class GlobalSearch
{
    private readonly KnowledgeGraph graph;
    private readonly PageRankCalculator pageRank;

    // На основе найденных локальным поиском узлов:
    // 1. Создать персонализацию: найденные узлы получают высокий вес
    // 2. Вычислить персонализированный PageRank
    // 3. Обойти граф на N шагов от топовых узлов
    // 4. Вернуть связанный контекст отсортированный по PageRank
    public List<GraphNode> Search(
        List<SearchResult> localResults,
        int hops = 2,
        int topK = 20) { ... }
}

// GraphRag/HybridRetriever.cs
// Объединяет локальный и глобальный поиск в один контекст для модели
public class HybridRetriever
{
    private readonly LocalSearch local;
    private readonly GlobalSearch global;
    private readonly EmbeddingModel embedder;

    // Полный pipeline запроса:
    // 1. Создать эмбеддинг запроса
    // 2. Локальный поиск → конкретные совпадения
    // 3. Глобальный поиск → структурный контекст
    // 4. Дедупликация и ранжирование по PageRank
    // 5. Собрать текстовый контекст для отправки в LLM
    public async Task<RetrievalContext> RetrieveAsync(string query) { ... }
}
```

### Файлы проекта

```
08.GraphRag/
├── Indexer.cs
├── LocalSearch.cs
├── GlobalSearch.cs
├── HybridRetriever.cs
├── Models/
│   ├── SearchResult.cs
│   └── RetrievalContext.cs
└── Tests/
    └── RetrieverTests.cs
```

---

## Проект 09 — Inference (генерация токенов)

### Что это и зачем

Инференс — это использование обученной модели для генерации текста. Модель принимает последовательность токенов и предсказывает следующий. Мы выбираем один токен, добавляем его к последовательности, и снова спрашиваем модель. Так повторяется пока не получим нужный результат.

KV-cache (Key-Value cache) — кэш промежуточных вычислений. При генерации каждого нового токена, Key и Value векторы для всех предыдущих токенов не меняются. Кэшируя их мы избегаем пересчёта и ускоряем генерацию в десятки раз.

Temperature, Top-K, Top-P — способы управлять "случайностью" генерации. Temperature = 0.0 даёт всегда одинаковый детерминированный вывод (greedy). Temperature = 1.0 — стандартное поведение. Temperature > 1.0 — более случайный вывод.

### Источник для изучения

```
https://github.com/karpathy/nanoGPT
MIT · Python

model.py GPT.generate()   — авторегрессия с кэшем

https://github.com/ggerganov/llama.cpp
MIT · C

src/llama-sampling.cpp    — все алгоритмы сэмплинга
```

### Что реализовать

```csharp
// Inference/KvCache.cs
// Кэш K и V тензоров для всех предыдущих токенов
// Структура: слой → (K тензор, V тензор)
public class KvCache
{
    // cache[layer_idx] = (keys, values)
    // keys shape: (batch, n_kv_heads, seq_len_so_far, head_dim)
    private readonly (Tensor keys, Tensor values)?[] cache;

    // Получить кэшированные K,V для слоя и добавить новые
    public (Tensor keys, Tensor values) Update(
        int layerIdx,
        Tensor newKeys,
        Tensor newValues) { ... }

    // Очистить кэш перед новым запросом
    public void Clear() { ... }
}

// Inference/Sampler.cs
// Выбор следующего токена из распределения вероятностей
public class Sampler
{
    // Greedy — всегда берём токен с максимальной вероятностью
    // Детерминированно, но монотонно
    public int SampleGreedy(Tensor logits) { ... }

    // Temperature scaling — делим логиты на temperature перед softmax
    // Низкая temperature (0.1) = уверенные предсказания = монотонно
    // Высокая temperature (2.0) = размытые предсказания = разнообразно
    public int SampleWithTemperature(Tensor logits, float temperature) { ... }

    // Top-K — обнуляем все токены кроме K самых вероятных
    // Предотвращает выбор совсем неуместных токенов
    public int SampleTopK(Tensor logits, int k, float temperature = 1.0f) { ... }

    // Top-P (Nucleus sampling) — берём минимальный набор токенов
    // чья суммарная вероятность >= P
    // Адаптивнее чем Top-K: при уверенном предсказании набор маленький
    public int SampleTopP(Tensor logits, float p, float temperature = 1.0f) { ... }
}

// Inference/TextGenerator.cs
// Авторегрессивная генерация текста
public class TextGenerator
{
    private readonly LlmModel model;
    private readonly BpeTokenizer tokenizer;
    private readonly Sampler sampler;

    // Сгенерировать продолжение текста
    // Возвращает IAsyncEnumerable чтобы стримить токены по мере генерации
    public async IAsyncEnumerable<string> GenerateAsync(
        string prompt,
        int maxNewTokens = 512,
        float temperature = 0.7f,
        int topK = 50,
        float topP = 0.9f,
        CancellationToken ct = default)
    {
        // 1. Токенизировать промпт
        // 2. Инициализировать KV-cache
        // 3. Цикл генерации:
        //    a. Forward pass только для нового токена (с KV-cache)
        //    b. Взять логиты последней позиции
        //    c. Сэмплировать следующий токен
        //    d. Если EOS токен — остановиться
        //    e. Декодировать и вернуть
        //    f. Добавить в последовательность и повторить
    }
}
```

### Файлы проекта

```
09.Inference/
├── KvCache.cs
├── Sampler.cs
├── TextGenerator.cs
└── Tests/
    └── GeneratorTests.cs
```

---

## Проект 10 — LoRA (дообучение без переобучения всей модели)

### Что это и зачем

LoRA расшифровывается как Low-Rank Adaptation — адаптация низкого ранга. Полные веса языковой модели занимают 14+ ГБ. Переобучать все веса на своих данных требует огромного объёма памяти и данных, а результат легко уничтожит исходные знания модели.

LoRA добавляет маленькие матрицы адаптации поверх существующих весов и обучает только их. Вместо матрицы W размером 4096×4096 (16М параметров) обучаем две матрицы A (4096×16) и B (16×4096) — всего 131К параметров. Это в 120 раз меньше.

Ранг (rank) — это размер промежуточного измерения (16 в примере выше). Чем выше ранг — тем больше параметров и выше качество адаптации, но больше память.

### Источник для изучения

```
https://github.com/microsoft/LoRA
MIT · Python

loralib/layers.py   — читать полностью, 200 строк
```

### Что реализовать

```csharp
// LoRA/LoraLinear.cs
// Линейный слой с LoRA адаптацией
// Заменяет обычный Linear в моделях внимания
public class LoraLinear : Module<Tensor, Tensor>
{
    private readonly Linear baseLayer;  // замороженные базовые веса
    private readonly Parameter loraA;   // матрица A (in_features × rank)
    private readonly Parameter loraB;   // матрица B (rank × out_features)
    private readonly float scale;       // alpha / rank — масштаб вклада LoRA
    private readonly float dropout;

    // A инициализируется случайно (малые значения)
    // B инициализируется нулями чтобы при старте вклад LoRA = 0
    // Это важно: в начале fine-tuning модель ведёт себя как исходная

    // forward: baseLayer(x) + (x @ A^T @ B^T) * scale
    // Базовые веса заморожены: requires_grad = false
    // Обучаются только loraA и loraB
    public override Tensor forward(Tensor x) { ... }

    // Слить LoRA в базовые веса для инференса без оверхеда
    // W_merged = W + B @ A * scale
    // После слияния можно удалить A и B
    public Linear MergeWeights() { ... }
}

// LoRA/LoraApplicator.cs
// Применяет LoRA ко всем нужным слоям модели
public class LoraApplicator
{
    // Заменить Linear слои в attention на LoraLinear
    // Обычно заменяем только q_proj и v_proj — этого достаточно
    // k_proj и o_proj можно оставить замороженными
    public static void Apply(LlmModel model, LoraConfig config) { ... }

    // Получить только обучаемые параметры (loraA и loraB)
    // Передаётся оптимизатору вместо всех параметров модели
    public static IEnumerable<Parameter> GetTrainableParams(LlmModel model) { ... }

    // Сохранить только LoRA адаптер (~50-100 МБ вместо 14 ГБ)
    public static void SaveAdapter(LlmModel model, string path) { ... }

    // Загрузить адаптер поверх базовой модели
    public static void LoadAdapter(LlmModel model, string path) { ... }
}

// LoRA/LoraConfig.cs
public record LoraConfig
{
    public int   Rank        { get; init; } = 16;    // размер адаптации
    public float Alpha       { get; init; } = 32f;   // масштаб = alpha / rank
    public float Dropout     { get; init; } = 0.05f; // регуляризация
    public string[] TargetModules { get; init; } =   // какие слои адаптировать
        ["q_proj", "v_proj"];
}
```

### Файлы проекта

```
10.LoRA/
├── LoraLinear.cs
├── LoraApplicator.cs
├── LoraConfig.cs
└── Tests/
    └── LoraTests.cs
```

---

## Проект 11 — DPO (обучение на человеческих предпочтениях)

### Что это и зачем

DPO расшифровывается как Direct Preference Optimization — прямая оптимизация предпочтений. После базового обучения модель умеет генерировать текст, но не обязательно полезный или правильный. DPO дообучает модель так чтобы она предпочитала "хорошие" ответы "плохим" используя пары примеров (chosen — выбранный ответ, rejected — отклонённый).

В контексте кодовой документации: chosen — правильно написанный JSDoc с точным описанием параметров и поведения, rejected — неточный или пустой JSDoc. Модель обучается различать их и генерировать chosen-качество.

Разница с RLHF (Reinforcement Learning from Human Feedback — обучение с подкреплением на основе человеческой обратной связи): DPO не требует отдельной reward model и не использует алгоритм PPO (Proximal Policy Optimization — оптимизация проксимальной политики). Математически эквивалентен но в 10 раз проще в реализации.

### Источник для изучения

```
https://github.com/eric-mitchell/direct-preference-optimization
MIT · Python

trainers.py   — читать полностью, 300 строк
```

### Что реализовать

```csharp
// DPO/DpoTrainer.cs
public class DpoTrainer
{
    private readonly LlmModel policy;      // модель которую обучаем
    private readonly LlmModel reference;   // замороженная копия исходной модели
    private readonly float beta = 0.1f;    // сила KL-дивергенции от reference
    // KL-дивергенция — мера того насколько далеко policy ушла от reference
    // beta регулирует: высокий beta = держимся ближе к исходной модели

    // DPO Loss — функция потерь
    //
    // log_prob(chosen | policy)   — вероятность хорошего ответа по нашей модели
    // log_prob(rejected | policy) — вероятность плохого ответа по нашей модели
    // log_prob(chosen | reference)   — то же по исходной модели
    // log_prob(rejected | reference) — то же по исходной модели
    //
    // rewards_chosen  = log_prob(chosen|policy)  - log_prob(chosen|reference)
    // rewards_rejected = log_prob(rejected|policy) - log_prob(rejected|reference)
    //
    // loss = -log(sigmoid(beta * (rewards_chosen - rewards_rejected)))
    //
    // Смысл: максимизировать разницу между тем насколько модель
    // предпочитает chosen по сравнению с reference
    public float ComputeDpoLoss(
        Tensor chosenLogProbs,
        Tensor rejectedLogProbs,
        Tensor refChosenLogProbs,
        Tensor refRejectedLogProbs) { ... }

    // Вычислить log вероятность последовательности ответа при данном промпте
    // Нужно для chosen и rejected, и для policy и reference (4 прогона)
    public Tensor GetLogProbs(LlmModel model, Tensor promptTokens, Tensor responseTokens) { ... }

    // Один шаг обучения на батче пар (промпт, chosen, rejected)
    public async Task<DpoMetrics> TrainStepAsync(DpoBatch batch) { ... }
}

// DPO/DpoDataset.cs
// Датасет пар предпочтений
public class DpoDataset
{
    // Загрузить пары из JSON файла
    // Формат: [{"prompt": "...", "chosen": "...", "rejected": "..."}]
    public static List<DpoPair> LoadFromJson(string path) { ... }

    // Токенизировать и упаковать в батчи
    public IEnumerable<DpoBatch> GetBatches(int batchSize) { ... }
}

// DPO/Models/DpoPair.cs
public record DpoPair
{
    public string Prompt   { get; init; } = ""; // вопрос или контекст
    public string Chosen   { get; init; } = ""; // предпочтительный ответ
    public string Rejected { get; init; } = ""; // нежелательный ответ
}
```

### Файлы проекта

```
11.DPO/
├── DpoTrainer.cs
├── DpoDataset.cs
├── Models/
│   ├── DpoPair.cs
│   ├── DpoBatch.cs
│   └── DpoMetrics.cs
└── Tests/
    └── DpoTests.cs
```

---

## Порядок реализации и зависимости

```
01.Tokenizer
    ↓
02.DatasetPipeline  (использует 01)
    ↓
03.TransformerModel (не зависит от других)
    ↓
04.TrainingLoop     (использует 02, 03)
    ↓
05.Embeddings       (использует 01, 03)
    ↓
06.VectorSearch     (не зависит — чистая математика)
    ↓
07.GraphEngine      (использует 05, 06)
    ↓
08.GraphRag         (использует 05, 06, 07)
    ↓
09.Inference        (использует 01, 03)
    ↓
10.LoRA             (использует 03, 04)
    ↓
11.DPO              (использует 04, 10)
```

---

## Solution файл

```xml
<!-- LlmFromScratch.sln -->
Project("...") = "Tokenizer",        "01.Tokenizer\Tokenizer.csproj"
Project("...") = "DatasetPipeline",  "02.DatasetPipeline\DatasetPipeline.csproj"
Project("...") = "TransformerModel", "03.TransformerModel\TransformerModel.csproj"
Project("...") = "TrainingLoop",     "04.TrainingLoop\TrainingLoop.csproj"
Project("...") = "Embeddings",       "05.Embeddings\Embeddings.csproj"
Project("...") = "VectorSearch",     "06.VectorSearch\VectorSearch.csproj"
Project("...") = "GraphEngine",      "07.GraphEngine\GraphEngine.csproj"
Project("...") = "GraphRag",         "08.GraphRag\GraphRag.csproj"
Project("...") = "Inference",        "09.Inference\Inference.csproj"
Project("...") = "LoRA",             "10.LoRA\LoRA.csproj"
Project("...") = "DPO",              "11.DPO\DPO.csproj"
```

---

## Итоговая таблица по времени

| Проект | Название | Дней |
|---|---|---|
| 01 | BPE токенизатор | 3–4 |
| 02 | Подготовка датасета | 2–3 |
| 03 | Архитектура Transformer | 7–10 |
| 04 | Тренировочный цикл | 4–5 |
| 05 | Эмбеддинги | 2–3 |
| 06 | Векторный поиск | 4–5 |
| 07 | Граф и PageRank | 3–4 |
| 08 | Graph RAG | 4–5 |
| 09 | Генерация токенов | 2–3 |
| 10 | LoRA адаптация | 3–4 |
| 11 | DPO обучение | 3–4 |
| **Итого** | | **37–50 дней** |
