---
title: "Chapter 14: conditional inheritance and static geography closure"
source_pdf_pages: [518, 522]
book_pages: [482, 486]
batch: "011"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 14: conditional inheritance and static geography closure

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 516–517](0516-0517-ch14-subsumption-and-algebraic-instances.md#section-14-6-4) · [Next: PDF 523–526](0523-0526-ch14-design-recipe-and-static-interface.md#pdf-0523)

> Editorial note: Full transcription of PDF 518–522 (printed pages 482–486). The opening sentence continues §14.6.4 on PDF 517. Unnumbered displays remain unnumbered; the original task-class bound, assignment argument, residual estimate, and local-convergence proof are preserved without mathematical correction. Separate analysis is in notes 179–182.

<a id="pdf-0518"></a>
## PDF 518 · Book 482

assumptions also applies to the rationalised template under the same assumptions, provided the degeneration is allowed. This gives a conditional inheritance statement.

<a id="corollary-14-13"></a>
**Corollary 14.13 (Conditional inheritance of standard approximation results).** Assume that a class of standard Transformer blocks satisfies a universal approximation theorem on a domain $\Omega$ under architectural assumptions $\mathcal A$. If the rationalised template is allowed to use the degenerate parameterisation of Proposition 14.10 within the same assumptions $\mathcal A$, then the rationalised class inherits the same approximation property.

*Proof.* The standard class is contained in the rationalised class by Proposition 14.10. Density of a subclass implies density of the containing class in the same function space, under the same assumptions. $\square$

This corollary is intentionally conditional. It does not provide a new universal approximation theorem for every constrained variant of the rationalised block. If additional constraints are imposed, for example fixed low rank, hard channel separation, or a small dictionary, the approximation result must be rechecked under those constraints.

<a id="section-14-6-5"></a>
#### 14.6.5 What is and is not claimed

The chapter therefore makes four explicit claims.

1. Standard MHA+FFN is recovered exactly as a degenerate case of the rationalised template.
2. Prototype-gated projection is an implementation instance when its atoms, gates, and residual bounds are chosen from the chapter's diagnostics.
3. A geometrically allocated multi-head block is an implementation instance when its head assignment, channel roles, crowding penalty, and residual budgets satisfy the rationalised constraints.
4. Universal approximation results are inherited only conditionally, through the embedded standard class and under the assumptions of the relevant theorem.

The chapter does not claim unrestricted subsumption of all Transformer variants, nor does it claim that every constrained rationalised instance has the full approximation power of an unconstrained standard Transformer. This resolves the subsumption issue at the level required here: identities are stated as identities, approximation is stated as conditional inheritance, and architectural variants are treated as instances only when their constraints match the rationalised template.

**Takeaway.** *The Rationalised Transformer is expressive enough to contain the standard block, concrete enough to admit projection-based and geometrically allocated instances, and cautious enough not to turn algebraic identities into unsupported universal approximation claims.*

<a id="section-14-7"></a>
### 14.7 Static control of the signature geography

The purpose of the rationalised Transformer is not merely to expose diagnostics. It must use them to close architectural choices. This section gives the static resolution of the three main quantities left by the preceding chapters: dictionary coverage, orbit crowding, and residual amplification. The results are deliberately stated at the correct scale. They are not universal dominance theorems over every possible trained Transformer. They are constructive closure

<a id="pdf-0519"></a>
## PDF 519 · Book 483

results: once a layer geography, a tolerance, and an allocation class are fixed, the block has a well-defined prescription.

<a id="section-14-7-1"></a>
#### 14.7.1 Empirical dictionary threshold

Let $\mu_\ell$ be the empirical distribution of signatures at layer $\ell$, supported in a compact metric signature space $(\Sigma,d_\Sigma)$. For a tolerance $\rho^\star>0$, define

<a id="display-0519-dictionary-threshold"></a>
$$
K_\ell^\star(\rho^\star)=\min\left\{K:\exists\,a_1,\ldots,a_K\in\Sigma\text{ such that }\int\min_{1\le k\le K}d_\Sigma(\sigma,a_k)^2\,d\mu_\ell(\sigma)\le\rho^\star\right\}.
$$

This is the empirical dictionary threshold used in the chapter. It is not postulated. It is the quantisation number of the measured signature geography at tolerance $\rho^\star$. In a finite diagnostic sample $\{\sigma_i\}_{i=1}^N$, it becomes the smallest $K$ for which the empirical $K$-medoids or $K$-means distortion is below the same tolerance:

<a id="display-0519-empirical-dictionary-threshold"></a>
$$
\widehat K_\ell^\star(\rho^\star)=\min\left\{K:\frac1N\sum_{i=1}^N\min_{1\le k\le K}d_\Sigma(\sigma_i,a_k)^2\le\rho^\star\right\}.
$$

<a id="proposition-14-14"></a>
**Proposition 14.14 (Operational dictionary closure).** For a fixed layer $\ell$, a fixed diagnostic metric $d_\Sigma$, and a fixed tolerance $\rho^\star$, the smallest number of effective signature representatives required by the rationalised block is $K_\ell^\star(\rho^\star)$. Any static allocation with fewer than $K_\ell^\star(\rho^\star)$ effective representatives fails the coverage tolerance, while an allocation with $K_\ell^\star(\rho^\star)$ representatives can satisfy it by construction.

*Proof.* This is an immediate consequence of the definition of $K_\ell^\star(\rho^\star)$. The lower bound follows because no set of fewer than $K_\ell^\star$ representatives attains distortion at most $\rho^\star$. The upper bound follows because the defining minimum is attained, or approximated arbitrarily well in the empirical case, by $K_\ell^\star$ representatives. The statement is therefore an operational closure result, not an asymptotic universality theorem. $\square$

The dictionary threshold problem is therefore resolved in this chapter by using the empirical signature measure as the object to be covered. A deeper task-uniform lower bound may be studied elsewhere, but it is not needed to define the static architecture.

<a id="section-14-7-2"></a>
#### 14.7.2 Crowding-controlled assignment

Coverage alone is insufficient: many heads may cover the same atom while leaving other regions uncovered. Let $z_{h,k}\in\{0,1\}$ assign effective head $h$ to atom $k$, and let $m_k=\sum_hz_{h,k}$ denote the occupancy of atom $k$. A simple layer crowding penalty is

<a id="display-0519-occupancy-crowding"></a>
$$
C_{\mathrm{occ}}^{(\ell)}=\sum_{k=1}^{K_\ell^\star}\max(0,m_k-1)^2.
$$

A static rationalised layer chooses assignments by minimising

<a id="display-0519-allocation-objective"></a>
$$
J_{\mathrm{alloc}}=\rho_\ell+\lambda_C C_{\mathrm{occ}}^{(\ell)}-\lambda_V\sum_hV_{\mathrm{marg},h}+\lambda_G\Gamma_{\mathrm{res}}^{(\ell)},
$$

subject to the available parameter budget and to the required S–A channel split. This turns crowding reduction into an explicit optimisation problem.

<a id="pdf-0520"></a>
## PDF 520 · Book 484

<a id="proposition-14-15"></a>
**Proposition 14.15 (Crowding reduction by assignment).** Assume that at least $K_\ell^\star(\rho^\star)$ effective heads are available and that each dictionary atom can be reached by at least one admissible head parameterisation. Then there exists a static assignment with $\rho_\ell\le\rho^\star$ and $C_{\mathrm{occ}}^{(\ell)}=0$. Moreover, any minimiser of $J_{\mathrm{alloc}}$ with $\lambda_C$ larger than the maximal possible marginal-value gain from duplicating an already occupied atom selects a non-crowded assignment whenever such an assignment is feasible.

*Proof.* Feasibility gives one admissible representative for each of the $K_\ell^\star$ dictionary atoms, hence the coverage tolerance can be met with one occupied head per atom and zero occupancy crowding. If a candidate assignment duplicates an occupied atom while some required atom remains unrepresented, replacing the duplicate by an admissible representative of the uncovered atom decreases the crowding penalty by at least $\lambda_C$ and cannot increase the coverage term beyond the tolerance construction. When $\lambda_C$ exceeds the largest possible marginal-value advantage of the duplicate, the objective strictly favours the non-crowded assignment. The result is a finite assignment argument. $\square$

This proves the internal crowding claim needed by the chapter. It does not assert that any optimiser trained from random initialisation automatically finds this assignment. The static architecture, however, is now well specified: crowding is not merely diagnosed; it is penalised by the allocation objective.

<a id="section-14-7-3"></a>
#### 14.7.3 Residual amplification as a constrained quantity

Let $\Gamma_{\mathrm{res}}^{(\ell)}$ denote the residual amplification diagnostic inherited from the orbit analysis. In the rationalised block it enters as a constraint or penalty:

<a id="display-0520-residual-constraint"></a>
$$
\Gamma_{\mathrm{res}}^{(\ell)}\le\Gamma^\star\qquad\text{or}\qquad\lambda_G\Gamma_{\mathrm{res}}^{(\ell)}\subset J_{\mathrm{alloc}}.
$$

This closes the architectural question: residual energy is not allowed to remain an invisible side effect of residual addition.

<a id="proposition-14-16"></a>
**Proposition 14.16 (Static residual control).** Suppose the attention and feed-forward residual branches are multiplied by gates satisfying $0\le g_h\le g_{\max}$ and suppose that each branch output is normalised so that $\|Y_hW_O^{(h)}\|_F\le B_h\|X_\ell\|_F$. Then the layer update satisfies

<a id="display-0520-residual-update-bound"></a>
$$
\|X_{\ell+1}-X_\ell\|_F\le\left(\sum_hg_hB_h+B_{\mathrm{ff}}\right)\|X_\ell\|_F
$$

up to the Lipschitz factor of the normalisation map on the considered compact region. In particular, bounding the gates and penalising $\Gamma_{\mathrm{res}}^{(\ell)}$ gives a finite, monitorable upper control on residual amplification.

*Proof.* The estimate follows from the triangle inequality applied to the gated residual branches and from the assumed branch bounds. Layer normalisation is Lipschitz on compact subsets away from zero variance; the corresponding factor multiplies the bound. The proposition does not require a special training trajectory. It is a static control statement for the block map. $\square$

The residual amplification quantity $\Gamma_{\mathrm{res}}$ is therefore resolved architecturally by two mechanisms: explicit monitoring and bounded gated injection into the residual stream. The stronger empirical question of whether this always beats a standard Transformer at equal compute is separate from the construction and is tested experimentally rather than assumed.

<a id="pdf-0521"></a>
## PDF 521 · Book 485

<a id="section-14-7-4"></a>
#### 14.7.4 A task-class lower bound on $K^\star$

Proposition 14.14 closes the dictionary threshold problem at the level of a single empirical $\mu_\ell$. A natural strengthening replaces the layer measure by a class of layer measures induced by a class of tasks. The strengthening is partial: it gives a lower bound that depends on the task class but not on a particular run, without claiming a universal task-uniform bound (which is left as an outlook item in §14.15.5).

<a id="proposition-14-17"></a>
**Proposition 14.17 (Task-class lower bound on $K^\star$).** Let $\mathcal T$ be a class of tasks with associated layer signature measures $\{\mu_\ell^{(\tau)}:\tau\in\mathcal T\}$ on a common compact metric space $(\Sigma,d_\Sigma)$. Define the class supports $S_\ell(\mathcal T)=\overline{\bigcup_{\tau\in\mathcal T}\operatorname{supp}\mu_\ell^{(\tau)}}$, and let $\mathcal N_{\rho^\star}(S_\ell(\mathcal T))$ denote the $\rho^\star$-covering number of $S_\ell(\mathcal T)$ in $d_\Sigma$. Then

<a id="display-0521-task-class-bound"></a>
$$
\sup_{\tau\in\mathcal T}K_\ell^\star(\rho^\star;\mu_\ell^{(\tau)})\le\mathcal N_{\rho^\star}(S_\ell(\mathcal T))^{1/2}\cdot(1+o(1))\qquad\text{as }\rho^\star\to0,
$$

and a uniform allocation with at least $\mathcal N_{\rho^\star}(S_\ell(\mathcal T))$ representatives covers every $\mu_\ell^{(\tau)}$ in $\mathcal T$ at tolerance $\rho^\star$.

*Proof.* The upper bound follows because every $\mu_\ell^{(\tau)}$ is supported in $S_\ell(\mathcal T)$, so any $\rho^\star$-cover of $S_\ell(\mathcal T)$ is a $\rho^\star$-cover of $\mu_\ell^{(\tau)}$ for every $\tau$. Hence $K_\ell^\star(\rho^\star;\mu_\ell^{(\tau)})\le\mathcal N_{\rho^\star}(S_\ell(\mathcal T))$ for each $\tau$. The square-root factor is a standard concentration improvement that arises when the diagnostic measure assigns probability bounded away from zero to a positive fraction of the cover atoms; see Niyogi–Smale–Weinberger for the analogous bound in manifold sampling. The constant $1+o(1)$ absorbs the standard deviation of the empirical sampling. $\square$

This proposition resolves the task-class question at the level needed for the chapter: if the architecture is to serve a class of tasks $\mathcal T$, the head budget must be at least the covering number of the union of their supports. The bound is not task-uniform in the strong sense (which would require a single covering valid for arbitrary classes), but it is uniform within $\mathcal T$.

<a id="remark-14-18"></a>
**Remark 14.18 (On the square-root improvement).** The $\sqrt{\mathcal N_{\rho^\star}}$ factor is sharp when the class measures concentrate on a low-dimensional sub-manifold of $\Sigma$. In the worst case, when the measures fill $S_\ell(\mathcal T)$ uniformly, the bound becomes $\mathcal N_{\rho^\star}$ without the square root. Both regimes are useful: the architecture can be designed for the worst case if the task class is broad and unstructured, and for the square-root case if the task class is structured.

<a id="section-14-7-5"></a>
#### 14.7.5 Local convergence to a non-crowded allocation

Proposition 14.15 shows that a non-crowded allocation exists when the budget is at least $K_\ell^\star$. It does not assert that gradient-based training reaches one. The next proposition gives a local convergence result: started from any allocation in a sufficiently small neighbourhood of a non-crowded allocation, gradient flow on $J_{\mathrm{alloc}}$ remains in a non-crowded basin and converges. The result is local rather than global, and it is parameterised by the gap between the marginal-value gain of crowding and the crowding penalty $\lambda_C$. The full convergence-from-random-initialisation question is deferred to Chapter 15 (Hypothesis H1 there).

<a id="proposition-14-19"></a>
**Proposition 14.19 (Local convergence of $J_{\mathrm{alloc}}$ to a non-crowded basin).** Let $z^\star\in\{0,1\}^{H\times K_\ell^\star}$ be a feasible non-crowded assignment of $H\ge K_\ell^\star$ heads to dictionary atoms (one head per atom for $K_\ell^\star$ atoms; the remaining $H-K_\ell^\star$ heads assigned to the channel split). Suppose:

1. $J_{\mathrm{alloc}}$ is twice-differentiable in a continuous relaxation $\widetilde z\in[0,1]^{H\times K_\ell^\star}$ of the assignment, with bounded Hessian;

<a id="pdf-0522"></a>
## PDF 522 · Book 486

2. $\lambda_C>2\Delta V_{\max}$, where $\Delta V_{\max}$ is the maximum marginal-value gain attainable by duplicating an already occupied atom.

Then there exists a neighbourhood $U$ of $z^\star$ in the relaxed assignment space such that gradient flow on $J_{\mathrm{alloc}}$, starting from any $\widetilde z(0)\in U$, remains in a non-crowded basin and converges to a stationary point $\widetilde z^\star$ with $C_{\mathrm{occ}}^{(\ell)}(\widetilde z^\star)=0$ and $\rho_\ell(\widetilde z^\star)\le\rho^\star$.

*Proof.* The objective $J_{\mathrm{alloc}}$ is convex in the coverage and crowding penalties when these are written in terms of the relaxed assignment $\widetilde z$, with strict convexity contributed by $\lambda_CC_{\mathrm{occ}}^{(\ell)}$ for $\lambda_C>0$. The marginal-value term is concave (a sum of bounded scalars), but its concavity is dominated by the crowding term whenever $\lambda_C>2\Delta V_{\max}$: the second-derivative bound on $J_{\mathrm{alloc}}$ is then bounded below by a positive constant in the directions that would lead to crowding.

Local convexity of $J_{\mathrm{alloc}}$ around $z^\star$ follows. A standard gradient-flow argument (Lyapunov function $J_{\mathrm{alloc}}-J_{\mathrm{alloc}}(z^\star)$, which is non-negative and decreasing) gives convergence to a stationary point in the same basin. The basin is non-crowded by the strict-convexity gap, and the coverage tolerance is preserved because $z^\star$ achieves it. $\square$

<a id="remark-14-20"></a>
**Remark 14.20 (Limitations).** The proposition is local: it does not address random initialisation, non-convex sub-regions of the loss landscape, or saddle-point behaviour. It does, however, give a non-trivial guarantee: once the optimiser reaches a neighbourhood of any non-crowded allocation, it stays there. The full global convergence question, together with the choice of initialisation that places $\widetilde z(0)$ in such a neighbourhood, belongs to the stochastic-approximation framework of Chapter 15. Hypothesis H1 of that chapter combines the present local result with a tracking argument to give global convergence on the slow time scale.

<a id="section-14-7-6"></a>
#### 14.7.6 Resolved geography criterion

The static geography criterion used by the rationalised Transformer is therefore

<a id="display-0522-geography-criterion"></a>
$$
\rho_\ell\le\rho^\star,\qquad C_{\mathrm{occ}}^{(\ell)}\le C^\star,\qquad\Gamma_{\mathrm{res}}^{(\ell)}\le\Gamma^\star,\qquad\sum_hV_{\mathrm{marg},h}\text{ maximal under budget.}
$$

It closes the design loop left by the diagnostic chapters. Coverage fixes the number of representatives, crowding fixes their distribution, marginal value fixes which representatives should be preserved, and residual amplification fixes the amount of update allowed to enter the stream.

**Takeaway.** *The chapter resolves static geography control by replacing informal architectural preference with an explicit finite allocation problem. The threshold $K_\ell^\star$ is the empirical covering number; crowding is controlled by assignment; residual amplification is bounded by gated residual injection.*

<a id="section-14-8"></a>
### 14.8 A concrete static design recipe

The previous sections define the block and its control quantities. This section turns them into a concrete static recipe. The recipe is intentionally conservative. It can be applied to a small model without changing the training objective radically, and it produces diagnostics that can be compared with a standard baseline.
