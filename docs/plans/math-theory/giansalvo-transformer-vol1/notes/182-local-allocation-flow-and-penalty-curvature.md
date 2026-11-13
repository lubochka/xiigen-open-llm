---
title: "182. Локальный allocation flow: почему bounded gains не доказывают устойчивость"
source_pdf_pages: [521, 522]
book_pages: [485, 486]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 182. Локальный allocation flow: почему bounded gains не доказывают устойчивость

[Индекс](../README.md) · [Источник и редакционная политика](../SOURCE.md)

## Что утверждается локально

[Proposition 14.19](../source/0518-0522-ch14-static-geography-closure.md#proposition-14-19) предполагает дважды дифференцируемый relaxed objective с ограниченным Hessian и $\lambda_C>2\Delta V_{\max}$. Из этого заявлены инвариантная окрестность любого допустимого non-crowded assignment, сходимость к стационарной точке, нулевой crowding и сохранение coverage tolerance. [Remark 14.20](../source/0518-0522-ch14-static-geography-closure.md#remark-14-20) правильно отделяет эту локальную задачу от достижения такой окрестности из случайной инициализации. Но локальный вывод тоже требует доказательства.

## Ошибки перехода к выпуклости

Функция $(m-1)_+^2$ плоская при $m<1$ и не является дважды дифференцируемой в $m=1$. Даже на гладких участках occupancy зависит только от сумм столбцов: перераспределения, сохраняющие $m_k$, невидимы для штрафа. Поэтому положительный $\lambda_C$ не делает objective строго выпуклым по всем assignment coordinates.

Ограниченность значений marginal reward не задаёт знак Hessian и не ограничивает его нужной величиной. Например, $f(t)=\varepsilon\sin(\omega t)$ ограничена по модулю $\varepsilon$, но $\|f''\|_\infty=\varepsilon\omega^2$. Каждая выбранная частота даёт конечный bounded Hessian; условие на амплитуду всё равно не доминирует его отрицательные направления. Coverage и residual term также не объявлены выпуклыми в предпосылках. Фраза «a sum of bounded scalars» не доказывает concavity.

## Явный гладкий пример положительного crowding

Рассмотрим естественную relaxation двух assignment rows с суммой $1$:

$$
Z(a,b)=\begin{pmatrix}1-a&a\\b&1-b\end{pmatrix},
\qquad 0\le a,b\le1.
$$

Тогда $m_1=1-a+b$, $m_2=1+a-b$, и **точно** $C_{\mathrm{occ}}=(a-b)^2$. На этом допустимом пространстве штраф гладкий. Обозначим $u=a-b$ и выберем гладкую coverage relaxation

$$
\rho(u)=\frac12\left(\frac{(|u|-1/4)_+}{3/4}\right)^4.
$$

Она равна нулю при $|u|\le1/4$ и $1/2$ при $|u|=1$. На бинарных assignments это согласуется с точным distortion двух равновесных атомов на расстоянии $1$: полный словарь имеет ошибку $0$, дубликат — $1/2$, так что при допуске $\rho^*=0.1$ имеем $K^*=2$. Функция $\rho$ дважды дифференцируема с bounded Hessian; положительная часть в четвёртой степени устраняет излом второй производной.

Пусть residual постоянен, а суммарный marginal reward равен $\varepsilon(1+a-b)$ при $\varepsilon>0$. Reward неотрицателен и ограничен; прирост при дублировании второго атома относительно non-crowded assignment равен $\Delta V_{\max}=\varepsilon$.

При $\lambda_V=1$ и $\lambda_C=3\varepsilon>2\Delta V_{\max}$ цель в полосе $|a-b|\le1/4$ равна

$$
J(a,b)=3\varepsilon(a-b)^2-\varepsilon(1+a-b).
$$

Она имеет bounded Hessian. В точке $Z(0,0)=I$ crowding нулевой, но градиент не нулевой. При **projected gradient flow в евклидовых координатах $(a,b)$**, сохраняющем $a,b\in[0,1]$, из $(0,0)$ получаем $b(t)=0$ и

$$
\dot a=\varepsilon(1-6a),\qquad
a(t)=\frac{1-e^{-6\varepsilon t}}6\longrightarrow\frac16.
$$

Предельная точка стационарна и имеет $C_{\mathrm{occ}}=1/36>0$. Таким образом, даже гладкий выпуклый пример удовлетворяет числовому gap condition и не даёт заявленного zero-crowding conclusion. Для обычного непроецированного flow начальная производная $\dot b=-\varepsilon$ сразу выводит trajectory из допустимого box, что подчёркивает необходимость явно задать constrained dynamics.

Вся траектория лежит в полосе нулевой coverage error. Выбранная relaxation совпадает с геометрическим distortion на всех бинарных assignments; Proposition не задаёт единственного способа её продолжения на дробные assignments. Если требуется иное конкретное продолжение, его необходимо включить в предпосылки и проверить отдельно. Механизм примера общий: конечный квадратичный штраф допускает малое нарушение, если reward имеет ненулевую производную в сторону нарушения.

## Какие условия действительно нужны

Из $z^*$ feasible не следует $\nabla J(z^*)=0$ или $J(z)-J(z^*)\ge0$. Поэтому предложенная в proof Lyapunov function не обоснована. Нужны stationarity/KKT, определённая constrained dynamics, инвариантность области допустимости и количественная кривизна всей цели. Для строгого zero crowding можно вводить hard constraint или подходящий exact penalty; bounded scalar gain не заменяет эти условия. Coverage tolerance в одной точке также не сохраняется автоматически в её окрестности и вдоль flow.
