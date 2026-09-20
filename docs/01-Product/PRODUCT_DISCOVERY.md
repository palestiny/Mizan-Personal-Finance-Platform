# Product Discovery — Mizan

## Status

**Discovery document — not a final product specification**

This document records hypotheses and questions that feed the product/strategy gates. It is intentionally broader than an expense tracker while keeping the first release bounded.

## Product direction

Mizan is intended to become a Personal Financial Operating System that progressively helps a person:

Capture → Understand → Explain → Predict → Recommend → Act

The first product must earn trust through accurate financial state before higher-order intelligence becomes authoritative or action-oriented.

Mizan is global by design. Egypt and the initial personal-use context are validation conditions, not product boundaries.

## Initial problem hypothesis

People's financial information is fragmented across cash, bank accounts, wallets, receipts, messages, memory, spreadsheets, and finance applications. The product opportunity is to reduce the effort required to turn that fragmented information into a trusted financial picture and useful next actions.

This is a hypothesis to validate, not a claim that the market has already been proven.

## Candidate initial user

Individual personal-finance user

This remains the working MVP hypothesis. Household/shared finance and professional variants are intentionally deferred unless discovery evidence changes the boundary.

## Candidate first wedge

Trust + effortless capture + immediate understanding

The MVP does not need every capture channel. It needs a trustworthy core and at least one capture path that can be measured against the user's current baseline.

## Candidate minimum trustworthy loop

Capture → Interpret → Validate → Commit → Explain

AI may participate in interpretation and explanation. The domain remains authoritative for validation and commitment.

## Candidate core jobs

1. Record money received.
2. Record money spent.
3. Move money between owned accounts.
4. See current and historical financial state.
5. Correct a financial event without silently corrupting history.
6. Understand why a balance changed.
7. Reduce the effort required to keep financial records current.
8. Later, use trusted history to anticipate and evaluate financial decisions.

## Candidate economic scenarios

The discovery process should test whether users need distinctions between:
- personal expenses;
- owned-account transfers;
- payments on behalf of others;
- recoverables/reimbursements;
- advances/shared expenses;
- obligations/expected future payments;
- informational events with no balance effect.

These scenarios should be promoted into MVP behavior only when evidence and domain gates justify them.

## Candidate product qualities

Evaluate the product against:
- financial correctness;
- reliability;
- privacy/security;
- recoverability;
- usability and capture effort;
- explainability;
- accessibility;
- maintainability;
- observability;
- auditability;
- portability;
- testability.

## Discovery questions

### User and value
- Which user segment experiences the problem frequently enough to return?
- Which capture methods actually reduce effort?
- What explanation/insight is useful enough to create recurring value?
- Which pain points justify payment?

### Financial scope
- Which account types are essential?
- Is multi-currency needed at launch?
- Are credit cards, liabilities, recurring transactions, budgets, or goals essential to the first validated use case?

### Economic semantics
- How often do reimbursements, shared payments, advances, or obligations occur?
- Which of these need first-class behavior versus relationships/metadata?

### Trust and control
- What corrections do users expect?
- What must remain immutable/auditable?
- Which AI proposals are acceptable without confirmation?
- Which actions require explicit authorization?

### Commercial validation
- What measurable value is created per week/month?
- Which capability drives retention?
- What would users pay for?
- Which usage limits or premium capabilities are perceived as fair?

### Globalization
- Which requirements are truly universal?
- Which are country/provider-specific and should live at integration/localization edges?
- Which currencies, languages, tax/legal requirements, or data-access constraints are market-specific?

## Foundation non-goals

The foundation should not implement:
- advanced forecasting;
- autonomous financial decisions;
- broad bank integrations;
- household collaboration;
- investment management;
- full accounting/ledger infrastructure;
- microservices;
- every AI capability in the vision.

These remain future capabilities or validation targets, not reasons to overbuild M0.

## DG-001/DG-013 acceptance evidence

Before the product strategy/scope gates close, the repository should contain:
- target problem and user hypothesis;
- minimum trustworthy loop;
- bounded MVP;
- explicit non-goals;
- measurable validation outcomes;
- major constraints;
- open questions with owners/status;
- initial commercial hypotheses and evidence plan.
