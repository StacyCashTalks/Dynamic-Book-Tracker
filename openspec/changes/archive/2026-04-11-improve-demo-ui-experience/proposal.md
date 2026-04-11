## Why

The current UI does not present a polished demo experience: unauthenticated users are not clearly guided to log in, the visual design lacks a distinctive bookshop feel, and the layout is not optimized for responsive projector use. Tightening the UX and making live book updates more visible will better demonstrate the value of Web PubSub and make the application more compelling to use in front of an audience.

## What Changes

- Add a clear login call to action on the homepage for unauthenticated visitors.
- Refresh the client visual design with a cosy bookshop-inspired theme that still maintains strong projector readability and contrast.
- Improve responsive behavior so the main flows remain usable across demo screen sizes.
- Surface book update events in the UI so incoming messages visibly refresh the relevant book information without manual interaction.
- Improve overall demo ergonomics so the application better highlights real-time responsiveness and feels more polished to present.

## Capabilities

### New Capabilities
- `guest-homepage-guidance`: Guide unauthenticated visitors from the homepage into the sign-in flow.
- `demo-friendly-reading-ui`: Present a responsive, projector-friendly bookshop-style interface for the demo experience.
- `live-book-update-visibility`: Reflect incoming book update messages in the UI quickly and clearly so the real-time behavior is obvious during a demo.

### Modified Capabilities

None.

## Impact

- Affected code: `Client/` homepage, layout, styling, responsive components, and real-time update presentation logic.
- Affected systems: client authentication entry points and UI handling of Web PubSub-driven book updates.
- Dependencies: existing client build pipeline and current real-time messaging flow.
