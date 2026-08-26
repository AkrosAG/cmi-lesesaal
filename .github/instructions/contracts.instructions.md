---
applyTo: "CMI/Contract/**/*.cs"
---

# Contract Design - IDesign Rules

Applies when writing or reviewing code in any `CMI.Contract.*` project.

## What belongs in a Contract project

`CMI.Contract.<Subsystem>` contains **only**:
- Public service interfaces (what the Manager exposes to Clients and the message bus)
- Message types used on the MassTransit bus
- Shared DTOs that must cross subsystem boundaries

**Does NOT contain:**
- Engine interfaces (these stay inside the Manager project - they are not public)
- Access interfaces (same - internal to the subsystem)
- Business logic, helper methods, or extension methods

## The basic element of reuse is the contract, not the service

Design contracts as if they will be reused across multiple systems. The contract defines the interaction surface; the service is an implementation detail behind it.

## Operation count per contract

| Operations | Assessment |
|-----------|------------|
| 1 | Red flag - investigate; a single-operation contract is suspect |
| 2-3 | Fine with care |
| **3-5** | Optimal range |
| 6-9 | Acceptable, drifting |
| 12+ | Very likely poor design - look for factoring opportunities |
| **20+** | Reject immediately |

## Contract design rules

### Expose behaviour, not state
Avoid property-like operations (getters/setters on data). Good contracts are behavioural:

```csharp
// OK - behavioural, business-meaningful
Task<OrderDto> GetOrder(int orderId);
Task SubmitOrder(SubmitOrderRequest request);
Task CancelOrder(int orderId, string reason);

// Not OK - property-like; implies state and implementation detail
Task<string> GetOrderStatus(int orderId);
Task SetOrderStatus(int orderId, string status);
```

### Use atomic business verbs for ResourceAccess contracts
Access contracts must expose verbs that are meaningful at the business level, not CRUD operations:

```csharp
// OK
Task<UserDto> GetUser(string userId);
Task UpdateUserContactDetails(UpdateContactRequest request);
Task DeactivateUser(string userId);

// Not OK - exposes that a database is behind the abstraction
Task<UserDto> Select(string userId);
Task Insert(UserDto user);
Task Delete(string userId);
```

### Limit contracts per service
- **1-2 contracts per service** is the norm
- If a service has 3+ independent contracts, it may be doing too much
- Each contract (facet) should stand alone and operate independently

### Data types in contracts
Only use:
- Primitives
- Arrays of primitives
- Data contracts (DTOs - plain data, no methods)
- Arrays of data contracts

Never expose domain objects, Entity Framework entities, or objects carrying behaviour.

## Contract factoring techniques

When a contract grows too large or feels wrong, apply one of:

### Factor Down (Base Extraction)
Extract a base contract when some operations are not universally applicable:
```csharp
// Before: one contract with optional operations
// After: base contract + extended contract
public interface IOrderReader { Task<OrderDto> GetOrder(int id); }
public interface IOrderManager : IOrderReader { Task SubmitOrder(...); Task CancelOrder(...); }
```

### Factor Sideways (Separating Concerns)
Split logically unrelated operations into independent contracts:
```csharp
// Not OK - unrelated operations bundled
public interface ICommonManager { Task<UserDto> GetUser(...); Task<OrderDto> GetOrder(...); }

// OK - separated by concern
public interface IUserManager { Task<UserDto> GetUser(...); }
public interface IOrderManager { Task<OrderDto> GetOrder(...); }
```

### Factor Up (Contract Hierarchy)
Create a shared base when identical operations appear in multiple unrelated contracts - avoids duplication.

## MassTransit message types

Message types used on the bus live in `CMI.Contract.Messaging`. Rules:
- Messages are plain C# classes with properties only (data contracts)
- Use `interface`-based messages where MassTransit convention requires it
- Separate **command** messages (imperative: `SubmitOrderCommand`) from **event** messages (past tense: `OrderSubmittedEvent`)
- Only **Managers** publish events; only **Clients or Managers** consume events

## Naming

- Contracts follow the same IDesign naming as their host service: `I<Noun>Manager`, `I<Noun>Access`
- DTOs: suffix with `Dto` (e.g. `OrderDto`, `UserDto`)
- Request/response pairs: suffix with `Request` / `Response` (e.g. `SubmitOrderRequest`)
- Event messages: past tense (e.g. `OrderSubmittedEvent`, `UserCreatedEvent`)
- Command messages: imperative noun phrase (e.g. `SubmitOrderCommand`, `NotifyUserCommand`)