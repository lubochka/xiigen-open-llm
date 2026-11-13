---
title: "88. Выпуклая смесь values и касательный motor softmax"
source_pdf_pages: [323, 324]
book_pages: [287, 288]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 88. Выпуклая смесь values и касательный motor softmax

[Индекс библиотеки](../README.md) · [§9.8](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#section-9-8) · [Proposition 9.9](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-9) · [Предыдущая карточка](87-feedforward-gates-and-layernorm-motors.md) · [Следующая карточка](89-qkv-paths-and-headwise-pullback.md)

## Две выпуклые оболочки относятся к разным пространствам

Для одной головы источник использует row-конвенцию:

$$
q_m=y_mW_Q,\quad K=YW_K,\quad V=YW_V,\quad
s_m=q_mK^\top/\sqrt{d_k},\quad
a_m=\operatorname{softmax}(s_m).
$$

Выход до output projection равен

$$
o_m=a_mV=\sum_j a_{mj}v_j.
$$

Поскольку $a_{mj}\ge0$ и $\sum_j a_{mj}=1$, $o_m$ принадлежит $\operatorname{conv}\{v_j\}$. Это точное утверждение [Proposition 9.8](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-8).

| Оболочка | Элементы | Что ограничено |
|---|---|---|
| Value hull | Векторы $v_j\in\mathbb R^{d_v}$ одного входа | Выход одной головы $o_m$ |
| Canonical hull | Матрицы канонического attention-словаря | Проекция всей attention map $S$ и её остаток |

Из выпуклости value-смеси нельзя заключить, что attention map находится в канонической оболочке или что её остаток равен нулю. Обе геометрии участвуют в вычислении, но описывают разные переменные. Это различие прямо оговорено на [PDF 323](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#pdf-0323).

Условие суммы весов существенно. Формула относится к указанному softmax-выходу, до дополнительных изменений весов и residual-суммы. Линейная output projection переносит смесь в оболочку **спроецированных** values; она не сохраняет исходную оболочку в прежних координатах.

## Softmax убирает свободу общего сдвига logits

Для $a=\operatorname{softmax}(s)$

$$
J=\operatorname{diag}(a)-aa^\top,\qquad
J\mathbf1=0,\qquad \mathbf1^\top J=0.
$$

Поэтому и вариация вероятностей, и covector, прошедший через $J$, имеют нулевую сумму компонент. Это дифференциальная форма инвариантности $\operatorname{softmax}(s+c\mathbf1)=\operatorname{softmax}(s)$. [§9.8.1 и Proposition 9.9](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#section-9-8-1).

Редакторская проверка положительной полуопределённости:

$$
u^\top Ju=\sum_i a_i u_i^2-\left(\sum_i a_i u_i\right)^2
=\operatorname{Var}_{i\sim a}(u_i)\ge0.
$$

При конечных logits все $a_i>0$, поэтому для $n>1$ kernel состоит ровно из постоянных векторов; image — касательное пространство $\{v:\mathbf1^\top v=0\}$. В предельной насыщенной или маскированной конфигурации дополнительные направления могут подавляться, и фактическое активное пространство надо уточнять.

## Почему слово «проекция» требует оговорки

В подписи [Figure 9.6](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#figure-9-6) Jacobian «projects variations» в касательное пространство. Это верно как описание **области значений**, но $J$ обычно не является идемпотентной ортогональной проекцией.

Например, при равномерном $a=\mathbf1/n$,

$$
J=\frac1n\left(I-\frac1n\mathbf1\mathbf1^\top\right)=\frac1nP,
\qquad J^2=\frac1nJ.
$$

Касательные направления ещё и масштабируются. При неравномерном $a$ коэффициенты различаются по направлениям; следовательно, одного исключения $\mathbf1$ недостаточно для описания norm amplification или anisotropy.

Это также объясняет ошибку масштабирования в [Figure 9.2](../source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#figure-9-2-searchable-transcription): если $b$ уже является нормированным logit-вектором, $\partial a/\partial b=J$. Для $b=u/\sqrt{d_k}$ производная по $u$ равна $J/\sqrt{d_k}$, с одним общим множителем.

## Выпуклость сама по себе не доказывает anisotropy

Стрелка “CONVEX HULL $\Rightarrow$ ANISOTROPY” в большом графе — слишком сильное сокращение. Геометрия оболочки может быть сильно анизотропной, но факт нахождения в выпуклой оболочке не задаёт ни распределение выходов, ни спектр их covariance. Для такого вывода нужны свойства values и весов.

В данном проходе точные результаты — выпуклость смеси и касательное ограничение. Связь с геометрией данных и спектром motor остаётся отдельным вопросом, а цепь $\delta L\mapsto\delta S\mapsto\delta\Sigma$ требует ещё дифференцируемого извлечения сигнатуры; см. [карточку 91](91-signature-differential-chain-and-sa-motors.md).

