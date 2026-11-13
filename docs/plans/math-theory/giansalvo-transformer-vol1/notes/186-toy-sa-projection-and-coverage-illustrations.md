---
title: "186. Toy S–A и coverage: точное ортогональное разложение и пределы иллюстрации"
source_pdf_pages: [533, 534]
book_pages: [497, 498]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 186. Toy S–A и coverage: точное ортогональное разложение и пределы иллюстрации

[Индекс](../README.md) · [Источник и редакционная политика](../SOURCE.md)

## Первый опыт — проверяемая алгебра

[Experiment 1](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#section-14-12-1) рассматривает $M(\lambda)=S+\lambda A$, где $S^\top=S$, $A^\top=-A$, $0\le\lambda\le1$. Для вещественных матриц эти подпространства ортогональны по Frobenius inner product. Если $s=\|S\|_F^2$ и $a=\|A\|_F^2$, то

$$
\|M(\lambda)\|_F^2=s+\lambda^2a,
\qquad
\min_{T=T^\top}\|M(\lambda)-T\|_F^2=\lambda^2a.
$$

Минимизирующая symmetric approximation — ровно $T=S$. Полная S–A модель восстанавливает $M$ точно **по построению**. Это полезная иллюстрация учёта компонентов, но не эмпирическое доказательство большей Transformer expressivity или trainability.

## Alpha, relative error и нормировка графика

В главе 14 используется [squared-energy convention](../source/0512-0515-ch14-sa-channels-and-cost-accounting.md#display-0512-squared-alpha):

$$
\alpha(\lambda)=\frac{\lambda^2a}{s+\lambda^2a+\varepsilon},
\qquad
\alpha'(\lambda)=\frac{2\lambda a(s+\varepsilon)}{(s+\lambda^2a+\varepsilon)^2}\ge0.
$$

Для обычной relative Frobenius error symmetric approximation

$$
r(\lambda)=\frac{\lambda\sqrt a}{\sqrt{s+\lambda^2a}},
\qquad r(\lambda)^2=\frac{\lambda^2a}{s+\lambda^2a}.
$$

Без stabiliser $\varepsilon$ alpha равна **квадрату** relative error, а не самой error. При $\varepsilon>0$ она ещё немного меньше. В [Figure 14.4](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#figure-14-4) линии почти совпадают, вертикальная ось названа `normalised score`. Это может означать squared residual или дополнительную нормировку; по одному рисунку восстановить метрику нельзя. Нельзя молча считать совпадение доказательством равенства всех этих определений.

При ненулевом фиксированном $S$ и конечном $A$ ненормированная $\alpha(1)<1$. Верхняя отметка около $1$ на графике может быть следствием выбранной нормировки; для её воспроизведения нужны матрицы, их масштабы и формула plot preprocessing. Если $A=0$, рост отсутствует; если $S=0$ и $\varepsilon=0$, ratio не определён при $\lambda=0$ и постоянен при $\lambda>0$. Поэтому корректное общее утверждение — неубывание при определённом знаменателе, а strict increase требует невырожденных условий.

## Второй опыт — геометрическая иллюстрация allocation

[Experiment 2 и Table 14.10](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#section-14-12-2) задают плотный reciprocal cluster и редкий directional tail. Политика, резервирующая tail slots и штрафующая crowding, имеет reported coverage residual $0.098$ против $0.578$, crowding $0.102$ против $0.255$, tail residual $0.129$ против $1.103$.

По этим значениям относительные снижения составляют приблизительно $83.0\%$, $60.0\%$ и $88.3\%$. Это арифметика опубликованной таблицы, а не новая оценка по данным. Чтобы сравнение было алгоритмически воспроизводимым, нужны точки и веса geography, число представителей, metric и bandwidth, правило выбора uniform representatives, objective rationalised allocation и random seed.

Если политика явно оптимизирует метрики, по которым её затем сравнивают с нетюнингованной эвристикой, улучшение служит демонстрацией механизма, но не универсальной гарантией качества на новых географиях и задачах. Редкий tail может требовать отдельного веса или ограничения, потому что average distortion допускает его игнорирование; см. [179](179-conditional-inheritance-and-empirical-dictionary-threshold.md) и [181](181-task-class-quantisation-and-covering-number-limits.md).

Источник утверждает наличие supplied script; здесь сохранены его результаты и формулировка, но сам эксперимент не запускался. [Figure 14.5 и продолжение на PDF 535](../source/0535-0540-ch14-illustrations-implementations-and-boundaries.md#pdf-0535) и [заметка 187](187-toy-coverage-and-marginality-aware-pruning.md) продолжают coverage example и переходят к marginality-aware pruning.
