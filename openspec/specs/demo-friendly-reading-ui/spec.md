## Requirements

### Requirement: UI uses a cohesive demo-ready visual theme
The system SHALL use a consistent bookshop-inspired visual style across the homepage, navigation, layout chrome, and primary book interactions.

#### Scenario: User moves between key screens
- **WHEN** a user navigates between the homepage and books experience
- **THEN** both screens use shared visual tokens and styling patterns
- **AND** the interface feels intentionally branded rather than scaffolded

### Requirement: UI remains readable on projected displays
The system SHALL preserve strong readability through high-contrast colors, sufficiently large typography, and easily distinguishable interactive controls.

#### Scenario: Demo is shown on a projector
- **WHEN** the application is displayed from a distance
- **THEN** primary headings, status indicators, and call-to-action controls remain legible
- **AND** state distinctions such as availability and connection status are visually clear

### Requirement: Key screens adapt responsively for demo layouts
The system SHALL adapt homepage sections, navigation regions, and book content layouts for narrow and wide viewports without losing access to core actions.

#### Scenario: Viewport size changes during use
- **WHEN** the application is viewed on smaller or constrained layouts
- **THEN** key actions remain visible without horizontal scrolling
- **AND** book cards and supporting controls reflow into usable stacked layouts
- **AND** the layout preserves clear spacing and hierarchy
