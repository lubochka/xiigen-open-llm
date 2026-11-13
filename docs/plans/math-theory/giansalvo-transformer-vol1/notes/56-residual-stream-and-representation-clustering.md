---
title: "56. Residual stream, вклад головы и кластеризация представлений"
source_pdf_pages: [243, 245]
book_pages: [207, 209]
supplementary_source_pdf_pages: [242]
batch: "005"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 56. Residual stream, вклад головы и кластеризация представлений

[Индекс библиотеки](../README.md) · [Полный текст: residual regimes](../source/0239-0244-ch07-information-flow-and-residual-stream.md#pdf-0243) · [Table 7.2 и продолжение](../source/0245-0250-ch07-head-gauge-and-layer-geography.md#pdf-0245) · [Предыдущая карточка](55-information-flow-and-rank-along-layers.md) · [Следующая карточка](57-head-gauge-and-channel-identifiability.md)

## Что означает сумма приращений

Формула (7.6)

$$
X^{(\ell)}=X^{(0)}+\sum_{j=1}^{\ell}\Delta^{(j)}
$$

точна как телескопическое тождество, если **определить**
$\Delta^{(\ell)}=X^{(\ell)}-X^{(\ell-1)}$ в одной и той же системе координат и точке извлечения. Но при post-norm архитектуре такое приращение не равно автоматически сумме сырых выходов attention и MLP. [Формула 7.6 и её пояснение](../source/0239-0244-ch07-information-flow-and-residual-stream.md#equation-7-6).

Например, для схемы

$$
Z=\operatorname{LN}_1(X+\operatorname{MHA}(X)),\qquad
X'=\operatorname{LN}_2(Z+\operatorname{FFN}(Z))
$$

точное приращение — $X'-X$. Оно включает действие обеих нормировок. LayerNorm нелинеен и не распределяется по отдельным головам как линейная сумма их вкладов. Для pre-norm residual stream аддитивная запись относится к другой точке вычислительного графа; эти варианты нужно различать при сопоставлении формул и измерений. Утверждение о накоплении «информации» также не следует из одного телескопического равенства.

## Сначала вернуть output projection

Пусть для одной головы

$$
A_h\in\mathbb R^{n\times n},\quad
X\in\mathbb R^{n\times d},\quad
W_{V,h}\in\mathbb R^{d\times d_v},\quad
W_{O,h}\in\mathbb R^{d_v\times d}.
$$

Её вклад в общее пространство перед residual/LN:

$$
O_h=A_hXW_{V,h}W_{O,h}=A_hXN_h,
\qquad N_h=W_{V,h}W_{O,h}\in\mathbb R^{d\times d}.
$$

В тексте и Figure 7.8 $W_O$ пропущен, хотя сначала рассматривается output **до** проекции. Поэтому $X(I+W_V^{(h)})$ вообще не определён при обычном $d_v\ne d$. Для изолированной головы и residual без последующей нормировки корректна запись $X+O_h$. [Три режима на PDF 243](../source/0239-0244-ch07-information-flow-and-residual-stream.md#pdf-0243), [Figure 7.8](../source/0239-0244-ch07-information-flow-and-residual-stream.md#figure-7-8).

| Attention prototype | До output projection | После output projection, до residual/LN |
| --- | --- | --- |
| Open: $A_h=I_n$ | $V_h=XW_{V,h}$ | $XN_h$ |
| Directional: $A_h=\mathbf1e_i^\top$ | $\mathbf1v_{i,h}^\top$ | $\mathbf1v_{i,h}^\top W_{O,h}$ |
| Closed: $A_h=\mathbf1\mathbf1^\top/n$ | $\mathbf1\mu_h^\top$, $\mu_h=n^{-1}\sum_i v_{i,h}$ | $\mathbf1\mu_h^\top W_{O,h}$ |

Это прямое перемножение матриц. Оно согласуется с точными definitions главы 6: [open/closed](39-open-and-closed-gates.md), [directional](40-directional-gates.md).

Для open residual одной головы получается $X(I+N_h)$. Произвольная $N_h$ не обязана быть кососимметричной или задавать ортогональный оператор, поэтому это не обязательно **вращение**. Если $A_h$ лишь близка к I, равенство заменяется приближением; например,

$$
\|(A_h-I)XN_h\|_F
\le\|A_h-I\|_{\mathrm{op}}\|XN_h\|_F.
$$

## Названия трёх режимов не совпадают с каталогом

На PDF 243 источник называет open/diagonal attention “directional regime”, а общий broadcast $\mathbf1e_i^\top$ — “instance gate”. В каталоге главы 6 это **directional gate** Definition 6.15. **Instance gate** Definition 6.31 имеет одну Dirac-строку j, а остальные строки uniform. В английском source оба обозначения сохранены; в проекте их следует различать по формулам. [Definition 6.15](../source/0194-0200-ch06-directional-cluster-and-contextual-gates.md#definition-6-15), [Definition 6.31](../source/0201-0206-ch06-pointwise-gates-composition-and-rank.md#definition-6-31).

Транспонированная запись $\mathbf1^\top v_i$ в тексте и рисунке также не даёт нужную $n\times d_v$ broadcast-матрицу при стандартном соглашении о столбце $v_i$. Здесь нужна $\mathbf1v_i^\top$. Ранг такого вклада равен 1 при ненулевом value и 0 при нулевом; после $W_O$ он может дополнительно обнулиться.

Кластерный gate с постоянными весами блока в общем случае создаёт **два** типа выходных строк — локальное и глобальное среднее. Он не сводится к одному одинаковому смещению всех строк. Общее смещение в broadcast-случае зависит от входа X; это не обязательно постоянный обучаемый bias-параметр. [Точный cluster output](../source/0194-0200-ch06-directional-cluster-and-contextual-gates.md#equation-6-15), [карточка 41](41-cluster-gates.md).

## Closed gate не означает нулевого вклада

Фраза “the closed gate contributes nothing” и правый блок Figure 7.8 неверны в общем случае. Uniform attention усредняет values, а не зануляет их. Вклад исчезает до output projection при $\mu_h=0$, после неё — при $\mu_h^\top W_{O,h}=0$; другие специальные компенсации могут зависеть от остальных голов и нормировки. Из самой формы $A_h$ малость этого вектора не следует. Масштабирование $W_{V,h}$ может сделать его сколь угодно большим при неизменной attention map.

Поэтому “inert”, “small constant shift” и “pass-through” — дополнительные описания авторского режима, а не универсальные следствия uniform softmax. Даже одинаковый сдвиг всех строк может изменить следующий LayerNorm и downstream logits. Присутствие st в верхнем слое не доказывает, что голова не влияет на классификацию. [Источник, пункты (iii) и подпись Figure 7.8](../source/0239-0244-ch07-information-flow-and-residual-stream.md#figure-7-8), [условия closed output](39-open-and-closed-gates.md).

## Что сообщает кластерный эксперимент

Автор приводит пять строк Table 7.2, приписывая их Ghione (2022). Эти числа перенесены как **авторская эмпирика**, без независимого воспроизведения:

| Точка извлечения | Число кластеров | ARI |
| --- | ---: | ---: |
| Input embeddings без CLS | 8 | 0.291 |
| Output embeddings без CLS | 14 | 0.885 |
| CLS последнего encoder layer | 8 | 0.978 |
| Classifier до softmax | 8 | 0.979 |
| Classifier после softmax | 8 | 0.981 |

Источник: [Table 7.2](../source/0245-0250-ch07-head-gauge-and-layer-geography.md#table-7-2). Здесь речь о классификации **восьми вариантов** SARS-CoV-2. Глава 3 отдельно различала её и бинарный SARS/non-SARS протокол; объединять их метрики в один эксперимент нельзя. [§3.4.4](../source/0131-0139-ch03-head-symmetry-pgop-and-case-study.md#section-3-4-4).

Арифметическое различие между CLS output и post-softmax ARI действительно равно $0.981-0.978=0.003$. Но пять выбранных extraction points не измеряют ARI **каждого** encoder layer. Кроме того, токенные embeddings и CLS — разные представления и потенциально разные единицы наблюдения. По этой таблице нельзя доказать монотонность одной и той же кластерной статистики по всем слоям.

ARI сравнивает два разбиения с поправкой на ожидаемое случайное совпадение. Его нулевой baseline — ожидание при соответствующей случайной модели, а не запрет отрицательных значений. Сам по себе ARI не является accuracy классификатора. Чтобы воспроизвести числа, нужны состав выборки, правило её представления, DBSCAN metric/параметры и обработка noise points; полный протокол на этих страницах отсутствует.

Утверждение о шести приблизительно равных principal components также не доказывает поглощение двух из восьми кластеров в outliers. Для восьми выходных вероятностей simplex имеет affine dimension не более 7; шесть существенных PCA-направлений совместимы с восемью и большим числом различимых кластеров. Число классов, число значимых компонент и размерность нелинейного многообразия — разные характеристики. [Пояснение после Table 7.2](../source/0245-0250-ch07-head-gauge-and-layer-geography.md#pdf-0245).

Наконец, cone index агрегирует токены отдельной последовательности, а оценка разделения классов требует распределения примеров. Из сходства двух графических рассказов не следует, что это «две дуальные меры одной динамики». Для проекта полезно хранить обе группы наблюдаемых, явно различая входную популяцию и единицу измерения, и проверять их связь на данных.
