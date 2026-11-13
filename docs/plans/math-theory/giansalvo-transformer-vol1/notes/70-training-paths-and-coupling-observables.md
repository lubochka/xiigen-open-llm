---
title: "70. Наблюдаемые transport, траектории обучения и условия дифференцируемости"
source_pdf_pages: [278, 280]
book_pages: [242, 244]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 70. Наблюдаемые transport, траектории обучения и условия дифференцируемости

[Индекс библиотеки](../README.md) · [Transition observables](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-38-4) · [Training paths](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-39) · [Метрика](69-hellinger-wasserstein-layer-geometry.md)

## Какие части перехода зависят от coupling

Для coupling $\pi_\ell$ двух распределений источник определяет

$$
\Phi_\Lambda^{(\ell)}
=\int(\Lambda'-\Lambda)\,d\pi_\ell,
\qquad
\Phi_\rho^{(\ell)}
=\int(\rho'-\rho)\,d\pi_\ell.
$$

Первая величина показывает рост и уменьшение средних regime weights, вторая — изменение среднего residual. [§7.38.4, PDF 278 / книга 242](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-38-4).

Однако **оптимальность coupling для этих двух чисел не нужна**. Из условия на его marginals немедленно следует

$$
\Phi_\Lambda^{(\ell)}
=\bar\Lambda_{\ell+1}-\bar\Lambda_\ell,
\qquad
\Phi_\rho^{(\ell)}
=\bar\rho_{\ell+1}-\bar\rho_\ell.
$$

Это редакторское раскрытие определений, а не изменение исходных формул. Оно показывает ограничение: нулевой translocation vector не означает нулевое Wasserstein distance. Распределение может измениться при сохранении средних.

Например, в двух regime coordinates при одинаковом residual сравним

$$
\mu=\tfrac12\delta_{e_1}+\tfrac12\delta_{e_2},
\qquad
\nu=\delta_{(e_1+e_2)/2}.
$$

Средняя $\Lambda$ у мер одинакова, но сами меры различны и $W_2(\mu,\nu)>0$. Первый слой состоит из двух чистых типов heads, второй — из смешанных heads. Данный пример проверяет наблюдаемое на пространстве сигнатур; реализуемость конкретных points конкретным словарём и обученной моделью остаётся отдельной задачей.

Упомянутый автором **spread change** требует зафиксировать определение центра и dispersion. Arithmetic mean координат, Fréchet barycentre по выбранной метрике и mean attention matrix — разные объекты. В §7.38.4 конкретной формулы spread нет, поэтому её нельзя молча достраивать как единственную авторскую.

## Matching не является наследованием head identity

Figure 7.16 изображает геометрическое сопоставление heads соседних слоёв. Source прямо называет его pseudo-identity. Оптимум зависит от dictionary, метрики, $\alpha$, набора входов и возможных ties. Он отвечает на вопрос «какие сигнатуры лучше сопоставляются по выбранной стоимости», а не «какая голова передала функцию какой голове». [Figure 7.16](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#figure-7-16).

При одинаковых весах и числе heads существует оптимальный assignment. Каждый выбранный match при этом не обязан быть индивидуально ближайшим среди всех heads: глобальное ограничение one-to-one может заставить назначить одному head не самого близкого партнёра. Слова источника “closest” следует читать как глобально оптимальное сопоставление, а не независимый nearest-neighbour search.

## Длина траектории и прямое смещение

Для фиксированных входа, слоя и head Definition 7.41 вводит

$$
A=\sum_{k=0}^{T-1}
d_\Sigma^{(\alpha)}(\Sigma(t_k),\Sigma(t_{k+1})),
\qquad
D=d_\Sigma^{(\alpha)}(\Sigma(t_0),\Sigma(t_T)),
\qquad
\delta_{\rm geo}=A-D\ge0.
$$

Неотрицательность — следствие triangle inequality. Если два heads пришли к одной конечной сигнатуре, их $A$ могут сильно различаться. [Definition 7.41](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#definition-7-41).

Но это **длина по наблюдаемым checkpoints**. Редкие checkpoints могут пропустить петли и быстрые изменения между ними. Более частое разбиение увеличивает или сохраняет сумму distances, если добавленные точки лежат на той же фактической траектории.

Название “geodetic deviation” не превращает $D$ в длину внутренней геодезической. В использованной Hellinger metric расстояние является хордой на square-root sphere. Даже движение по кратчайшей дуге может давать положительное превышение суммы хорд над одной конечной хордой. Поэтому $\delta_{\rm geo}$ корректно читать как **избыток дискретной длины над конечным metric displacement**.

## Scalar speed и векторная производная

Definition 7.42 пишет

$$
\dot\Sigma(t_k)\approx
\frac{d_\Sigma^{(\alpha)}(\Sigma(t_k),\Sigma(t_{k+1}))}
{t_{k+1}-t_k}.
$$

Правая часть — неотрицательное **скалярное значение скорости** в метрике. Оно не содержит направление изменения. Нужны строго возрастающие $t_k$ и одинаковые единицы времени: optimizer steps, examples или seconds дают разные величины. [Definition 7.42](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#definition-7-42).

В следующем §7.39.1 тот же символ используется уже как производная координат полной сигнатуры:

$$
\dot\theta=-\nabla_\theta\mathcal L,
\qquad
\dot\Sigma=
D\Sigma_8(S)\,
D\operatorname{softmax}(L)\,
DL(\theta)\,
\dot\theta.
$$

Здесь $L$ — logits, $\mathcal L$ — loss. Это цепное правило для дифференцируемого composite map, а не тождество со scalar finite-difference speed. [§7.39.1](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-39-1).

## Почему стабильной active face недостаточно

Источник утверждает differentiability при стабильной active face проекции. Для **всей** сигнатуры этого условия недостаточно: residual norm может иметь излом в нуле, а выбор коэффициентов требует собственных условий.

Простой редакторский контрпример использует допустимый фиксированный одноточечный hull $K=\{U\}$, где $U$ — равномерная stochastic matrix. Возьмём ненулевую матрицу $E$ с нулевыми суммами строк и

$$
S(t)=U+tE
$$

при достаточно малых положительных и отрицательных $t$, сохраняющих неотрицательность entries. Тогда

$$
\Pi_K(S(t))=U,\qquad
\rho_K(S(t))=|t|\,\|E\|_F.
$$

Active face всё время одна и та же, но $\rho_K$ не дифференцируема при $t=0$. Следовательно, общее обоснование через стабильность грани не покрывает точку нулевого residual. Аналогичная оговорка нужна у пересечения canonical hull в transverse direction; конкретная ситуация зависит от выбранного словаря.

Рабочая локальная версия цепного правила требует дифференцируемости всех используемых звеньев: logits/model map, softmax, projection на рассматриваемой области, выбранных $\Lambda$ и residual coordinate. При ненулевом residual евклидова норма гладкая; при нулевом безопаснее отдельно работать с squared distance или направленными производными. Hellinger square-root coordinates дополнительно требуют осторожности у $\Lambda_j=0$.

Это не отменяет continuity самой projection/residual: они остаются устойчивыми объектами при фиксированном закрытом выпуклом hull. Но continuity и differentiability — разные свойства.

## Что означает связь с динамикой обучения

Измерять скорость и crowding полезно для обнаружения головы, которая ещё меняет роль, и головы, которая уже стабилизировалась рядом с другими. Source также требует учитывать functional marginality. Совпадение или медленное изменение signatures не доказывает redundancy и не устанавливает замедление обучения. [PDF 280](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#pdf-0280).

