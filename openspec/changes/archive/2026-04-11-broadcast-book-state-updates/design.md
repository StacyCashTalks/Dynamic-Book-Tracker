## Context

The current real-time flow mixes two concerns: keeping every open client in sync with book state and drawing a user's attention to updates that matter to them. In practice, borrow/return events are not reaching every connected client consistently, and the UI behavior treats received events as visible notifications even when the user is not watching or borrowing the affected book.

This change spans both API-side event publishing and client-side event handling. The goal is to keep the shared book list accurate for all connected users while reserving toast-style acknowledgement for people who are directly affected by the change.

## Goals / Non-Goals

**Goals:**
- Ensure borrow and return events provide enough realtime information for all connected users to refresh stale book state.
- Differentiate between background sync events and attention-worthy notifications.
- Preserve visible notifications for relevant users such as owners, watchers, or the current borrower.
- Keep the client behavior calm for unrelated users while still making the book list accurate.

**Non-Goals:**
- Redesign the books UI again beyond what is necessary to support quieter refresh behavior.
- Replace Web PubSub or the existing notification payload model.
- Introduce a new persistence model for tracking subscriptions beyond the current user and watchlist data.

## Decisions

### Decision: Separate state-sync delivery from user-facing notification rules
The system will treat borrow/return events as two layers: an event stream broad enough for all connected clients to refresh state, and a presentation rule that decides whether the current user should see a visible acknowledgement.

**Rationale:** This preserves accurate shared state without forcing every event to behave like a toast notification.

**Alternatives considered:**
- Keep a single event type and always show a toast: simple, but too noisy for users unaffected by a change.
- Only notify owners and watchers: quieter, but leaves unrelated clients stale until manual refresh.

### Decision: Publish borrow/return updates broadly for connected clients
Borrow and return changes should reach all connected clients, not just owners or watchers, so the library grid can stay correct everywhere.

**Rationale:** Availability is shared state in this application, and all users benefit from seeing correct borrowability immediately.

**Alternatives considered:**
- Force polling after every action: would update state, but undermines the realtime demo.
- Depend only on watcher groups: insufficient because non-watchers still need fresh availability.

### Decision: Let the client decide whether an event is visible or silent
The payload already identifies the affected book and event type, so the client can decide whether to show a toast based on local relevance such as ownership, current loan, or watchlist membership.

**Rationale:** Client-side filtering avoids multiplying server-side event variants while keeping user-facing behavior flexible.

**Alternatives considered:**
- Encode separate noisy/silent message channels on the server: possible, but adds avoidable complexity to a small demo app.
- Suppress all notifications for borrow/return events: too quiet for people who are directly impacted.

## Risks / Trade-offs

- **[More events reach every client]** → Keep payloads small and limited to borrow/return state changes so fan-out remains lightweight.
- **[Client filtering logic becomes inconsistent]** → Base visibility rules on existing local context: owner ID, borrowed-by user ID, and watchlist membership.
- **[Silent sync events feel invisible during demos]** → Preserve visible acknowledgement for directly affected users while still updating all lists in the background.

## Migration Plan

1. Update realtime publishing so borrow/return state updates can reach all connected clients.
2. Adjust client event handling to always refresh book state from these events.
3. Restrict visible acknowledgement to relevant users only.
4. Validate borrow and return flows for owner, borrower, watcher, and unrelated user sessions.

Rollback is a standard application rollback to the previous client and API build if the broader event fan-out causes issues.

## Open Questions

None currently.
