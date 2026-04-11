## ADDED Requirements

### Requirement: Guest homepage presents a clear sign-in path
The system SHALL present unauthenticated visitors with a homepage that explains the demo and includes a prominent sign-in call to action without requiring them to discover navigation chrome first.

#### Scenario: Unauthenticated visitor opens the homepage
- **WHEN** an unauthenticated visitor navigates to `/`
- **THEN** the page presents demo-focused introductory content
- **AND** the page shows a prominent login action above the fold
- **AND** the login action routes to the configured authentication flow

#### Scenario: Authenticated visitor opens the homepage
- **WHEN** an authenticated visitor navigates to `/`
- **THEN** the page acknowledges their signed-in state
- **AND** the page provides a prominent route into the live books experience

### Requirement: Homepage communicates the demo value clearly
The system SHALL describe the real-time demo purpose on the homepage so viewers understand that the application showcases responsive UI updates powered by messaging.

#### Scenario: Presenter introduces the application from the homepage
- **WHEN** the homepage is displayed during a demo
- **THEN** the content explains that the application demonstrates real-time book updates
- **AND** the explanation is concise enough to scan quickly on a projected display
