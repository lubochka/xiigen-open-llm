---
title: "243. PAC identification: порог eigenvalues, generalisation accuracy и необоснованный lower bound"
source_pdf_pages: [626, 628]
book_pages: [590, 592]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 243. PAC identification: порог eigenvalues, generalisation accuracy и необоснованный lower bound

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Gap для одного теста и gap всего stopping rule

Формула (16.48) минимизирует расстояние $|\lambda_j-\theta_w|$ только по собственным значениям, удовлетворяющим $|\lambda_j-\theta_w|<\theta_w$. Если множество пусто, минимум требует соглашения. Если оно непусто, контроль operator error строго меньше gap сохраняет соответствующие eigenvalue-threshold comparisons по Weyl. Равенство error=gap уже может привести к попаданию точно на порог, поэтому переход от строгого условия к нестрогому в (16.50) требует запаса. [Relevant gap](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#equation-16-48), [sample-size condition](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#equation-16-50).

Сохранность этих сравнений не равна сохранности **Frobenius residual stopping**, использованного ранее. Редакторский пример для той прежней PSD-модели:

$$
B=\operatorname{diag}(0.4,0.4),\quad
\widehat B=\operatorname{diag}(0.35,0.35),\quad\theta_w=0.5.
$$

Здесь $\|\widehat B-B\|_2=0.05<\Delta_{\rm rel}=0.1$, все eigenvalues у обоих операторов меньше порога. Однако $\|B\|_F\approx0.5657>0.5$, а $\|\widehat B\|_F\approx0.4950<0.5$. Для первого нужна одна дефляция, для второго остановка возможна сразу. Поэтому мост от «all comparisons correct» к $\widehat K^*=K^*$ в Theorem 16.35 требует другого margin: до порога **остаточной нормы на каждом нужном шаге**, плюс контроля eigenvectors и event implementation. [PAC theorem и proof](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#theorem-16-35).

## Два разных требования к выборке

Формула (16.51) контролирует operator-estimation condition. Она не содержит заданной точности $\varepsilon$, а proof лишь объявляет achieved error $\varepsilon=O(K^*\sqrt{\log d/m})$. Если $\varepsilon$ — заранее выбранная малая точность, необходим также соответствующий sample-size constraint с $1/\varepsilon^2$, нормами и loss constants. Если это просто имя достигнутой величины, его нужно явно определить; без этого фраза «generalisation error ≤ empirical + ε» двусмысленна.

Обычная схема объединения двух корректных гарантий потребовала бы $m$ не меньше **максимума** sample requirements для идентификации архитектуры и требуемой generalisation accuracy. Универсальное доминирование первого над вторым из приведённых условий не следует. К тому же оценка класса функций из [242](242-pac-function-class-rademacher-and-estimation-conditions.md) имеет собственные пробелы.

## Что не доказывает Le Cam sketch

Theorem 16.36 задаёт два разных средних оператора, но не строит два распределения наблюдений с контролируемой statistical distance. Разность матричных ожиданий сама по себе не ограничивает total variation сверху: два детерминированных распределения на разных наблюдениях различаются одним sample, даже если их operator means близки. Для lower bound нужен специально построенный трудный класс распределений и корректный расчёт KL/TV. [Нижняя граница и proof sketch](../source/0625-0627-ch16-pac-estimation-and-architecture-selection.md#theorem-16-36).

В показанной one-eigenvalue perturbation вычисление приводит к $B_X^4/(8\Delta_{\rm rel}^2)$. Множитель $d$ затем добавлен словами «after accounting for the d-dimensional structure», без многомерной конструкции или зависимости $B_X$ от $d$. Это не доказательство $\Omega(d/\Delta_{\rm rel}^2)$ и не подтверждает заявленную tightness. Здесь оценивается достаточность приведённого proof, а не доказывается невозможность каких-либо lower bounds для более точно заданной модели.

## Проверка численной таблицы

В Table 16.5 ratios арифметически согласованы: $320000/24\approx13333>10^4$, $67349/48000\approx1.4031$. Но не приведены $B_X$, $\rho_{\max}$ и конкретный $\theta_w$, необходимые для пересчёта $m_{\rm PAC}$; установка $C_{\rm PAC}=1$ — авторская калибровка, не доказанная универсальная константа. [Table 16.5](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#table-16-5).

Соседняя фраза, что SST-2 требует «more heads», расходится с этой же таблицей: 142 против 191 у CoV-2. Если подразумевалась другая нормировка или comparison baseline, она не указана. Наличие обучающей выборки больше заявленного числа не является проверкой корректности теоремы, покрытия доверительного уровня или независимости observations. В пакете выполнен пересчёт табличной арифметики, а не повторное обучение.
