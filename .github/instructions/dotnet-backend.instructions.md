---
applyTo: "CMI/**/*.cs"
---

# .NET Backend - IDesign Rules & Conventions

These rules apply whenever you are writing or reviewing C# code in the `CMI/` backend.

> **Reality check:** The codebase was built by a large team (10+ developers) over many years. IDesign was the target architecture, but not every part of the code adheres perfectly to all rules. You will encounter violations - treat them as known technical debt, not as intentional exceptions that justify repeating the pattern.
>
> **Rule of thumb:** When reading existing code, note violations. When writing new code, always follow the rules below - regardless of what the surrounding code does.

## IDesign Method - Layer responsibilities

| Layer | Encapsulates | Knows | Does NOT know |
|-------|-------------|-------|---------------|
| **Client** (`CMI.Web.*`) | Who interacts | Which Manager to call per use case | Business logic, Engine internals |
| **Manager** | *What* sequence/workflow to execute | When to call Engines and Access | How Engines or Access work internally |
| **Engine** | *How* a business activity is performed | Business rules and algorithms | When it is called, what use case triggered it |
| **Access** | *How* to access a resource | Atomic business verbs for one resource | Which Manager or Engine needs the data |
| **Utilities** (`CMI.Utilities.*`) | Cross-cutting infrastructure | Nothing domain-specific | Nothing about any subsystem |

> Utilities are ubiquitous - any layer may call them. However some are layer-specific: logging and auditing belong at the Manager layer, not in client code.

## Call rules - ALLOWED

- **Client -> Manager** (one Manager per use case; never two Managers in the same use case)
- **Manager -> its own Engine(s)** (synchronous, in-process)
- **Manager -> its own Access component(s)** (synchronous, in-process)
- **Engine -> Access** (Engine may read/write data it needs)
- **Any component -> any Utility**
- **Manager -> another Manager** only via a **queued/async MassTransit message** (never a direct project/assembly reference)

## Call rules - FORBIDDEN

| Forbidden pattern | Why |
|-------------------|-----|
| Client calls an Engine or Access directly | Skips the Manager; business logic migrates into the Client |
| Client calls more than one Manager in a single use case | Client absorbs orchestration; Managers become coupled |
| Manager calls another Manager directly (synchronous) | Couples subsystems; use the message bus instead |
| Manager queues calls to more than one other Manager in the same use case | Use a Pub/Sub Utility instead |
| Queuing a call to an Engine | Engines are always called synchronously |
| Queuing a call to a ResourceAccess | ResourceAccess is always called synchronously |
| Engine calls another Engine | Each Engine encapsulates its own activity completely |
| Access calls another Access | Joins between resources belong inside a single Access component |
| Calling more than one layer down (e.g. Client->Access, Manager->Resource) | Bypasses encapsulation; couples to implementation details |
| Engine or Access publishes events | No knowledge of business context; only Managers publish events |
| Engine, Access, or Resource subscribes to events | Reacting to an event starts a use case; only Clients or Managers subscribe |

## Inter-layer data passing

Only these types may cross layer boundaries:

- Primitives (`int`, `string`, `bool`, `DateTime`, etc.)
- Arrays of primitives
- Data contracts (plain DTOs - data only, **no methods or business logic**)
- Arrays of data contracts

**Never share logic embedded in a data contract across layers.** Each layer provides its own interpretation of the data. "Business Entity" objects carrying behaviour break encapsulation.

## Naming conventions

| Component | Prefix type | Example |
|-----------|-------------|---------|
| Manager | **Noun** | `OrderManager`, `AssetManager` |
| Engine | **Gerund** (verb + "-ing") | `AnonymizingEngine`, `SearchingEngine` |
| Access | **Noun** | `SqlAccess`, `HarvestAccess` |

**Smells:**
- Gerund on a Manager or Access -> functional decomposition signal
- Noun on an Engine -> not encapsulating an *activity*
- Manager, Engine, and Access all with the same name -> entity-based (not volatility-based) decomposition
- Engines are **somewhat rare** - only add one when there is genuinely volatile business logic to encapsulate

## Adding a new component - checklist

### New Manager
1. Create `CMI.Manager.<Subsystem>` project; add to solution
2. Create `CMI.Host.<Subsystem>` project to host it as a Windows Service
3. Put public interfaces and message types in `CMI.Contract.<Subsystem>`
4. Reference only `CMI.Contract.*` projects from other subsystems; never reference `CMI.Manager.*` directly
5. Register MassTransit consumers in the Host's container configurator
6. Engine and Access interfaces stay **inside** the Manager project - they are not public

### New Engine
1. Create inside `CMI.Manager.<Subsystem>` (or its own project if large)
2. Name with gerund prefix: `<Gerund>Engine`
3. No knowledge of which Manager calls it - pure business logic / algorithms
4. May call Access components; must not call other Engines

### New Access component
1. Expose atomic business verbs - never CRUD (`Select`, `Insert`, `Delete`)
2. A single Access may join multiple physical resources when they are always accessed together
3. See `contracts.instructions.md` for operation design rules

## Design smells - watch for these

| Smell | Description |
|-------|-------------|
| Fat Manager | One Manager handling multiple unrelated use cases or mixed subsystems |
| Too many Managers | More than ~5 per subsystem without sub-subsystem split |
| Vertical slice smell | Manager + Engine + Access all named after the same entity |
| Domain in Manager names | If the domain is not volatile, it should not drive component boundaries |
| Code pushed to Client | Business logic accumulating in `CMI.Web.*` controllers |

## Call chain smells

| Smell | Description |
|-------|-------------|
| **The Glove** | Back-and-forth: A calls B, B calls A |
| **Cyclic** | Any cycle in the call graph |
| **Staircase / Fork** | Interaction diagram looks like a staircase or fork - sign of functional decomposition |
| **Big data movement** | Large objects passed up and down the chain |
| **One operation to rule them all** | A single operation does everything; impossible to test or reuse |

## Design review checklist

Before merging new backend code, verify:
- [ ] All identified volatilities are encapsulated in their own component
- [ ] No naming smells (verbs in Manager names, nouns in Engine names)
- [ ] Service count appropriate (~2-5 Managers per subsystem)
- [ ] No forbidden call patterns
- [ ] Inter-layer data uses only primitives and data contracts (no behaviour-carrying objects)
- [ ] Call chains are symmetric across use cases
- [ ] No call chain smells (The Glove, cyclic, staircase/fork)
- [ ] No code pushed into the Client that belongs in a Manager or Engine

## Namespace convention

```
CMI.<Concept>.<Subsystem>
```

- `Concept`: `Contract`, `Access`, `Engine`, `Manager`, `Host`
- `Subsystem`: `Asset`, `Cache`, `DataFeed`, `DocumentConverter`, `ExternalContent`, `Harvest`, `Index`, `Lesesaal`, `Monitoring`, `Notification`, `Onboarding`, `Order`, `Parameter`, `Repository`, `Vecteur`; use `Common` when none fits
- Folder structure mirrors the namespace exactly (each dot = a folder)
- Utilities exception: `CMI.Utilities.<Utility>.<Concept>`