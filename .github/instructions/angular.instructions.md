---
applyTo: "CMI/Web.Clients/**/*.{ts,html,less,css}"
---

# Angular Frontend - Conventions & Rules

Applies to all three Angular clients: `web-core`, `web-frontend`, `web-management`.

## Project structure

| Project | Purpose | npm package |
|---------|---------|-------------|
| `web-core` | Shared Angular library - services, components, models, and integration helpers reused by the other two clients | `@cmi/lesesaal-web-core` |
| `web-frontend` | Public citizen portal for reading room orders and consultation requests | - |
| `web-management` | Internal archivist / admin client for order processing, administration, and operational workflows | - |

`web-frontend` and `web-management` depend on `web-core`. For local development, build `web-core` first then link it:

```bash
# In web-core
npm run build

# In web-frontend or web-management
npm run link   # links ../web-core/dist/@cmi/lesesaal-web-core
```

To consume `web-core` from the CMI-internal MyGet feed instead (package: `@cmi/lesesaal-web-core`):
```bash
npm login --registry https://www.myget.org/F/akrosag-nuget/npm/ --scope=@cmi
npm ci --legacy-peer-deps
```

## Code conventions

### Private members
All private methods, properties, and fields must be prefixed with `_`:
```typescript
// OK
private _userService: UserService;
private _isLoading = false;
private _loadData(): void { }

// Not OK
private userService: UserService;
```

### Business logic
Business logic belongs in **services**, not in components. Components handle only presentation and user interaction.

### Indentation
Use **tabs** (not spaces). Use WebStorm's *Code -> Reformat Code* to normalize existing files before editing.

### Component placement
- New **routable pages** (reachable via `Routes.ts`) -> `/app/component/`
- All other components -> `/client/components/`

### Component CSS
- CSS files must only contain styles for that specific component
- All selectors must be namespaced with a component-specific prefix:
  ```css
  /* OK */
  .order-detail-header { }
  .order-detail-title { }

  /* Not OK */
  .header { }
  h1 { }
  ```

### Service injection
Services injected in the constructor that are also used outside the constructor must be declared as `private` fields directly:
```typescript
// OK
constructor(private _router: Router) { }
// _router is available as this._router throughout the class

// Not OK - redundant field declaration
private _router: Router;
constructor(router: Router) { this._router = router; }
```

### Truthy / falsy
Use TypeScript / Angular truthy/falsy features to avoid unnecessary single-line methods:
```typescript
// OK
if (this._items?.length) { }

// Not OK
if (this._items !== null && this._items !== undefined && this._items.length > 0) { }
```

### TODOs
```typescript
// ToDo: <description of what is still open>
// e.g.: // ToDo: Connect favourites service
```

## Code quality rules (enforced in CI)

- Do not commit commented-out code to `develop` - remove it before merging
- Do not commit leftover `console.log` statements to `develop`
- Nothing is committed that does not build, including lint errors (`npm run lint` must pass)
- Line count must be proportional to the complexity of the code

## Build commands

Run from the relevant client directory:

```bash
npm ci --legacy-peer-deps    # install (use this, not npm install)
npm run build                # dev build - also runs lint
npm run build-prod           # production build
npm run lint                 # lint only
npm run test                 # interactive watch mode
npm run test-github          # single CI run, ChromeHeadless, with coverage
```

Build order dependency: `web-core` must be built before `web-frontend` or `web-management`.

## Testing

- Focus tests on logic in services, not on component rendering details
- Use `npm run test-github` for a single run with coverage (mirrors CI)
- CI uses `karma.github.conf.js` with `ChromeHeadlessCI`

## IDesign rules for the web layer

The Angular clients are the Client layer in IDesign terms:
- Call only one Manager per use case (via the web API)
- Do not contain business logic - delegate to services; services call the API
- Do not call Engine or Access endpoints directly