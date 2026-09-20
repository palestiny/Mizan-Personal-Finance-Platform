# Mizan Engineering Rules

## 1. Roles

### Project Owner

Khaled is the project owner and final decision maker for product direction, scope, business requirements, major architecture decisions, trade-offs, and acceptance.

### Engineering Partner

The AI engineering partner is responsible for analysis, proposals, challenge of assumptions, trade-off analysis, tests, implementation, review, documentation, and risk identification.

Significant product or architecture decisions must not be silently made on behalf of the project owner.

## 2. Source of truth

GitHub is the project source of truth.

Important decisions, requirements, architecture records, checkpoints, risks, and completion evidence belong in the repository.

## 3. Design before implementation

For major capabilities:

**Understand → Map → Design → Trade-offs → Decide → Document → Test → Implement → Review → Verify**

No production code should be introduced merely to discover what the architecture should have been.

## 4. Financial correctness

Financial records are authoritative business data.

The following principles require explicit design and verification:

- Monetary values must not depend on binary floating-point representation.
- Every monetary amount must have an explicit currency context.
- Transfers must not be modeled casually as unrelated income and expense events.
- Financially atomic operations must not leave partial state.
- Retries must be safe against duplicate financial effects.
- Historical financial data requires explicit correction/deletion semantics.
- Balance calculations must have one defined source of truth.
- Rounding and precision rules must be explicit.
- Date/time semantics must be explicit.
- Concurrency and synchronization conflicts must not silently corrupt financial state.

These are engineering principles to be converted into approved invariants through the relevant design gates.

## 5. Root-cause rule

When a defect appears, investigate the underlying cause before applying a local patch.

A fix is not considered complete if the same underlying failure can reappear through another path.

## 6. Verification

Every completed milestone must have:

- Defined acceptance criteria
- Evidence of implementation or analysis
- Relevant automated tests where applicable
- Review of affected areas
- A recorded checkpoint

## 7. Documentation discipline

Documentation must explain decisions and contracts, not become a collection of speculative files.

Do not create documents merely because a folder structure looks professional.

## 8. Scope discipline

Do not introduce features, abstractions, infrastructure, or dependencies before their need is established.

Future capabilities may be recorded as roadmap items without being implemented or architecturally forced into the current design.

## 9. Change control

Major architecture changes require an explicit decision record containing:

- Problem
- Context
- Options
- Trade-offs
- Decision
- Consequences
- Rejected alternatives
- Revisit conditions

## 10. Completion

“Implemented” means more than code exists.

A capability is complete only when its intended behavior is tested, reviewed, documented where necessary, and verified against its acceptance criteria.
