# M1 — Thin Vertical Slice Design Gate

## Status
**State:** Accepted — Product Owner approved 2026-09-20
**Phase:** M1 — Thin Technical Foundation
**Decision owner:** Khaled

## Purpose
Prove the accepted M0 financial model end-to-end with the smallest practical architecture before broad platform construction.

## Required slice
The first executable slice must cover:
1. create/activate an Account;
2. accept Income;
3. accept PersonalExpense;
4. accept OwnedAccountTransfer;
5. derive current Balance;
6. query History;
7. explain balance changes from authoritative Effects;
8. verify atomicity, idempotency, deterministic ordering, and rebuildability.

The slice should exercise one real path from client/API through application/domain to persistence and back to a read result.

## Architectural constraints already accepted
- One product and one financial core.
- Operation + Effect is authoritative.
- Evidence/AI cannot mutate financial truth.
- Balance is derived from opening state + authoritative Effects.
- No microservices.
- No full accounting ledger.
- No broad bank integrations.
- No full offline synchronization before demonstrated need.
- Architecture decisions are made only where the slice materially requires them.

## Decisions that now materially constrain implementation

### D1 — Application shape
A. Modular monolith with explicit Domain / Application / Infrastructure / API boundaries — **recommended**.
B. Layered monolith without explicit domain boundary.
C. Microservices.

Trade-off: A gives strong boundaries without distributed complexity; B is simpler initially but easier to erode; C is disproportionate to current scope.

### D2 — Persistence
A. Relational database through a repository/persistence abstraction — **recommended**.
B. Document database.
C. In-memory only for the first slice.

Trade-off: A fits immutable operations/effects, atomic multi-effect commit, deterministic queries, and rebuild verification. B is possible but adds modeling/query complexity. C cannot prove the persistence requirements.

### D3 — API boundary
A. Thin HTTP API exposing commands and read models — **recommended**.
B. UI-first local domain without an API.
C. General-purpose event/API platform.

Trade-off: A proves a real application boundary while staying small. B delays proving the server/application boundary. C overbuilds integration infrastructure.

### D4 — Read/write separation
A. Separate command/application services from query/read models at the application boundary — **recommended**.
B. One service/DTO model for both writes and reads.
C. Full CQRS/event-sourcing infrastructure.

Trade-off: A keeps authoritative writes disciplined while allowing balance/history projections; B is simpler but risks coupling; C adds unnecessary infrastructure.

### D5 — Transaction boundary
A. One database transaction around accepted Operation + complete Effect set + required derived updates — **recommended**.
B. Multiple independently committed writes.
C. Eventual consistency.

Trade-off: A directly enforces atomicity; B violates financial safety; C is inappropriate for authoritative balance-changing acceptance.

### D6 — Test strategy
A. Domain invariant tests + application integration tests + one API end-to-end path — **recommended**.
B. API-only tests.
C. Unit tests only.

Trade-off: A verifies the financial rules and the real persistence/application path without requiring a large test pyramid.

### D7 — Identity / idempotency
A. Stable client-provided idempotency key at the command boundary, persisted with the accepted operation — **recommended**.
B. Request timestamp/hash heuristic.
C. No idempotency until later.

Trade-off: A makes retries explicit and deterministic; B is unreliable; C contradicts accepted DG-003/DG-004 semantics.

### D8 — Balance implementation
A. Compute from authoritative effects first; add materialized derived balance only after a measured need — **recommended**.
B. Materialized balance from day one.
C. Snapshot/event-store architecture.

Trade-off: A gives the smallest correctness-first implementation. B may optimize reads prematurely. C increases infrastructure complexity.

## Proposed implementation order
RED tests → minimal domain types → accepted Operation/effect behavior → persistence atomic commit → balance/history queries → idempotency → API path → rebuild/reconciliation tests → refactor.

## Definition of Done
- One complete end-to-end financial path exists.
- Income, expense, and transfer obey accepted invariants.
- Account/balance/history are queryable.
- Failed writes leave no partial financial state.
- Repeating the same command does not duplicate effects.
- Balance can be rebuilt from authoritative effects.
- Tests demonstrate deterministic ordering and correction-ready history boundaries.
- No architecture introduced without a demonstrated slice requirement.

## Accepted decisions

1. **D1 Application shape:** Modular monolith with explicit Domain / Application / Infrastructure / API boundaries.
2. **D2 Persistence:** Relational database behind a persistence abstraction.
3. **D3 API boundary:** Thin HTTP API exposing commands and read models.
4. **D4 Read/write separation:** Separate command/application services from query/read models at the application boundary; no full CQRS infrastructure.
5. **D5 Transaction boundary:** One database transaction around accepted Operation + complete Effect set + required derived updates.
6. **D6 Test strategy:** Domain invariant tests + application integration tests + one API end-to-end path.
7. **D7 Identity/idempotency:** Stable client-provided idempotency key at the command boundary, persisted with the accepted operation.
8. **D8 Balance implementation:** Compute from authoritative Effects first; introduce materialized derived balance only after measured need.

## Decision record

**Decision:** Accepted
**Decision owner:** Khaled
**Approval date:** 2026-09-20
**Approval:** Product Owner approved all eight recommended M1 thin-slice architecture decisions.

## Next implementation sequence
RED tests → minimal domain types → accepted Operation/effect behavior → persistence atomic commit → balance/history queries → idempotency → API path → rebuild/reconciliation tests → refactor.
