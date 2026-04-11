## MODIFIED Requirements

### Requirement: Books screen reflects incoming real-time updates
The system SHALL refresh the displayed book information when a book borrow or return update is received so the on-screen state matches the latest shared server state for all connected users.

#### Scenario: Borrow or return update arrives while the books page is open
- **WHEN** the books page receives a real-time message for a borrow or return change
- **THEN** the visible book data refreshes without requiring manual reload
- **AND** the updated state is shown on the screen shortly after the message arrives
- **AND** connected users who can view the library receive enough event information for the refresh to occur even if they are not following the affected book

### Requirement: Real-time changes are visibly acknowledged
The system SHALL show a visible acknowledgement for a borrow or return update only when the current user is directly affected, while still allowing unrelated users to refresh silently in the background.

#### Scenario: Relevant user receives a borrow or return update
- **WHEN** a real-time borrow or return update is processed for a user who owns, watches, or currently has the affected book checked out
- **THEN** the interface shows a noticeable but non-disruptive acknowledgement of the update
- **AND** the acknowledgement does not obscure the primary book information

#### Scenario: Unrelated user receives a borrow or return update
- **WHEN** a real-time borrow or return update is processed for a user who does not own, watch, or currently have the affected book checked out
- **THEN** the interface refreshes the affected book state silently
- **AND** the interface does not show a toast or similar visible acknowledgement for that update
