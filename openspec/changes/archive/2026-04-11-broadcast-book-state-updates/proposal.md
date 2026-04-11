## Why

Borrow and return events currently do not update every open client consistently, which leaves some users looking at stale book availability. At the same time, broadcasting visible toast-style notifications to everyone would create noise for users who are not following or currently borrowing that book.

## What Changes

- Broadcast borrow and return state changes broadly enough that all connected clients can refresh book data promptly.
- Distinguish between background state-sync events and user-facing notifications so the UI can quietly refresh for unrelated users.
- Preserve visible notifications for users who are directly affected by the change, such as watchers, owners, or the active borrower.
- Clarify the real-time client behavior so borrow/return events keep the shared library view accurate without unnecessary distraction.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `live-book-update-visibility`: Expand real-time update handling so all users receive enough information to refresh book state, while only relevant users receive visible acknowledgement.

## Impact

- Affected code: `Api/` real-time publishing for borrow/return events and `Client/` real-time event handling on the books page.
- Affected systems: Web PubSub event fan-out, client refresh behavior, and notification presentation rules.
- Dependencies: existing `live-book-update-visibility` spec and the current borrow/return event payload shape.
