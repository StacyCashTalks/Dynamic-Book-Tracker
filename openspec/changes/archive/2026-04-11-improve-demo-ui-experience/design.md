## Context

The current client presents a mostly scaffolded experience: the homepage is still the default template, the primary login affordance lives in the shared top bar, and the books screen relies on generic Bootstrap cards. The demo goal is to make the UI feel inviting and deliberate while clearly showing the impact of Web PubSub when book state changes arrive.

This change is confined to the client UX and presentation layer. It should improve first-run guidance for unauthenticated visitors, strengthen the visual identity with a bookshop-inspired theme, keep text legible on projected screens, and make live data updates obvious without changing the core domain model.

## Goals / Non-Goals

**Goals:**
- Give unauthenticated visitors a clear homepage path into sign-in.
- Introduce a cohesive, cosy visual theme that still uses high-contrast, projector-friendly typography and spacing.
- Improve responsiveness for homepage, navigation, and book listing interactions across common demo layouts.
- Make real-time book updates visibly refresh the current screen so the Web PubSub story is easy to follow during a presentation.
- Keep the work focused on UI polish and presentation of existing flows.

**Non-Goals:**
- Redesign or replace authentication providers.
- Introduce new backend APIs or change the book data model unless strictly required by the client refresh flow.
- Add complex analytics, telemetry, or non-demo product features.
- Rework the entire application information architecture beyond the screens directly involved in the demo.

## Decisions

### Decision: Use the homepage as the primary guest entry point
The homepage will become a branded landing screen with a concise explanation of the demo and a prominent login button for unauthenticated users. Authenticated users can instead be directed toward the live books experience.

**Rationale:** This solves the current discoverability problem without requiring users to infer that the small top-row login link is the next step.

**Alternatives considered:**
- Keep the top-row login link only: too easy to miss in a demo setting.
- Redirect all guests directly into login: faster, but weaker as a guided demo narrative and less friendly for first-time viewers.

### Decision: Introduce a shared theme layer rather than isolated page styling
The redesign will use shared layout and app-level styling primitives such as color tokens, typography, spacing, surface styling, and reusable section/card treatments.

**Rationale:** A shared theme keeps the homepage, navigation, notifications, and books page visually consistent and reduces the risk of a one-off redesign that feels disconnected.

**Alternatives considered:**
- Restyle only `Home.razor`: insufficient because the demo experience continues on the books page.
- Pull in a new UI library: unnecessary scope and risk for a targeted polish change.

### Decision: Optimize for projector readability first, ambience second
The palette and typography should evoke a cosy bookshop, but contrast, font sizes, control sizes, and layout density must stay readable from a distance.

**Rationale:** The application is specifically used in demos, so projected readability is a functional requirement rather than a visual preference.

**Alternatives considered:**
- Lean into a dark, highly decorative theme: may look attractive on a laptop but can reduce readability on projectors.
- Preserve the default Bootstrap palette: readable, but not distinctive enough for the desired demo quality.

### Decision: Reflect incoming Web PubSub messages in both status messaging and book data refresh
When a book-related real-time event arrives, the books screen should refresh the displayed data and visually acknowledge that an update happened.

**Rationale:** The current UI shows a notification message but does not make the changed book state consistently obvious. Refreshing the list ensures the page reflects the newest information while a visible cue reinforces the real-time effect.

**Alternatives considered:**
- Show toast messages only: demonstrates activity but not necessarily changed state.
- Fully patch the local book list from message payloads: potentially smoother, but more coupled to message shape than is necessary for this UI-focused change.

## Risks / Trade-offs

- **[Theme becomes too decorative]** → Use accessible contrast, restrained textures, and large readable controls so the ambience does not compromise legibility.
- **[Real-time refresh feels disruptive]** → Limit refresh behavior to relevant events and pair it with lightweight visual confirmation rather than a full-page reload experience.
- **[Responsive layout regressions in existing Bootstrap structure]** → Keep changes layered on current components and validate around the main demo breakpoints.
- **[Homepage and books page styling drift apart]** → Centralize the core theme styles in shared app and layout styles.

## Migration Plan

1. Update shared styles and layout components to establish the theme shell.
2. Replace the placeholder homepage with a guest-aware landing experience.
3. Restyle the books page and responsive interactions.
4. Update the real-time event handling so visible data refresh accompanies notifications.
5. Deploy as a standard client update with no data migration.

Rollback is a normal application rollback to the previous client build if the visual update or refresh behavior causes demo issues.

## Open Questions

None currently. The proposal is specific enough to proceed with implementation.
