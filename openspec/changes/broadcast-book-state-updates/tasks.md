## 1. Realtime event publishing

- [x] 1.1 Review the current borrow and return Web PubSub publishing paths and identify where global state-sync events must be emitted
- [x] 1.2 Update API-side borrow and return publishing so connected clients receive enough realtime information to refresh book state broadly

## 2. Client refresh and filtering

- [x] 2.1 Update the books page realtime handling so borrow and return events always refresh visible book state for connected users
- [x] 2.2 Add client-side relevance checks so only owners, watchers, or the active borrower see visible acknowledgement for borrow/return events

## 3. Behavior validation

- [x] 3.1 Verify owner, borrower, watcher, and unrelated-user flows to ensure state refreshes correctly and only relevant users see visible notifications
