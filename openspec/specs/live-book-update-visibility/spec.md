## Requirements

### Requirement: Books screen reflects incoming real-time updates
The system SHALL refresh the displayed book information when a relevant book update message is received so the on-screen state matches the latest server state.

#### Scenario: Book update arrives while the books page is open
- **WHEN** the books page receives a real-time message for a book-related change
- **THEN** the visible book data refreshes without requiring manual reload
- **AND** the updated state is shown on the screen shortly after the message arrives

### Requirement: Real-time changes are visibly acknowledged
The system SHALL provide a clear visual indication that a live update was received so viewers can connect the message event to the refreshed UI state.

#### Scenario: Presenter demonstrates a live change
- **WHEN** a real-time book update is processed
- **THEN** the interface shows a noticeable but non-disruptive acknowledgement of the update
- **AND** the acknowledgement does not obscure the primary book information
