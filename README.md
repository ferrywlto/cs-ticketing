# Customer Support Ticketing System

## Requirements

- [ ] Ticket Creation: Allow creation of new support tickets with subject and
description.
- [ ] Ticket List View: Display a list of unresolved support tickets with their subject, username, user ID and placeholder avatar.
- [ ] Ticket Detail View: Allow viewing the full details of a ticket, including a
chronological thread of replies.
- [ ] Reply to Ticket: Support agents should be able to add replies to a ticket.
- [ ] Status Updates:
    - [ ] A ticket starts as “Open”.
    - [ ] It becomes “In Resolution” when an agent replies.
    - [ ] It can be manually marked as “Resolved” by the agent.

### Backend
- Use .Net 8 for the backend.
- Expose a RESTful API to manage tickets and replies.
- Store data using SQLite or in-memory storage.
- Apply clean code principles and design patterns (e.g., Repository, Service Layer, Dependency Injection).
- Include unit tests for core logic (status updates, reply handling, etc.).

### Frontend
- Use any modern JavaScript framework of your choice: Blazor, React, Vue, or Angular.
- Implement the UI with:
    - Ticket list
    - Ticket detail + replies thread
- Forms to reply and create new tickets
- Interact with the backend using proper API calls.
- Demonstrate effective state management

## How to run
TBD

## Requirement Analysis
- There are the entities in the system:
    1. Customer (Player) - Who file customer service ticket.
    2. Customer Service Agent (Agent) - Who handle the customer support tickets.
    3. Ticket - Initiated by a player of what happened. 
    4. Reply - Initiated by either a player or an agent.
    5. Thread - The list of messages from player and agents for a particular ticket.
- Each message contains:
    - What - The message content
    - Who - A player / an agent
    - When - The time
- Each thread contains:
    - Ticket - The initiating ticket from player
    - Messages - A list of messages from both player and agents, sort by message date in ascending order.
    - Status - Reflect the ticket status
    - CreatedDate - The timestamp that the thread created.
    - LastUpdateDate - The timestamp that the thread created, the last message created in that thread or when the ticket is resolved.
- Each player and agent is a person that:
    - Has an email as identifier.
    - Has a name
    - Has an avatar
    - Player has an additional field: player number
- Each ticket contains:
    - Creator - Player
    - Status - from ["Open", "In Resolution", "Resolved"], the initial status is open. When ever an agent replies the ticket status will change to in resolution if it is in open status. When an agent click resolve, and the ticket is in resolution status, it will change to resolved.
    - Title - Ticket title
    - Description - Message content
    - Created date
    -  

## Assumptions
- Once a message is send, it cannot be edited for integrity.
- All CS Agents share the same avatar and share the same name "CS Agent".
- The list of tickets shows only "open" and "in resolution" tickets.
- The buttons of creating a new ticket and resolving a ticket is limited to player.
- When a player sign-in, they can only see their own tickets on the ticket list.
    - Player can see all tickets on ticket list, including resolved tickets, in reverse chronological order.
- When an agent sign-in, they can see all tickets that are open or in-resolution.
- There will be two sign-in pages:
    - One for player
    - One for agent
- Ticket can be handled by any agent, no assignment required.
- Assume user will refresh the page to load latest ticket status.

## Technical Decisions
- Using In-memory database due to keeping the project as simple as possible without external dependencies, this make sure user can run on their machine regardless of platform. As data persistency is not a mandatory in requirements.
- Using Entity Framework is not mandatory, as a clear data access layer should abstract the infrastructure and underlying implementation details. The same data access API should work with different storage provider interfaces.
- Basic username and password are used as authentication mechanism as the project is not aim for secure production use, in real world we should use more secure approach like OAuth + MFA. 
- Due to the same reason the agent or user account management is not included in this project.


## Design
- We will use .NET Aspire to orchestrate the following projects:
    - Domain Model
    - Infrastructure - EF Core, repositories, external services. 
    - Application - Orchestration of business logic and domain objects.
    - API - REST API endpoints from Web API / Minimal API.
    - Web - Blazor WASM front-end client.
    - UnitTests
    - Integration Tests
    - E2E Tests - Selenium for testing UI behavior
- It is arguably that we should have different independently deployable services for authentication user and ticket management. However for this project scale it is a bit over-engineering. Instead we will focus on separating concerns into isolated service and controller classes.
- Due to the lack of user management, user and agent records are seeded during application startup.
- We will use JWT with short exp for session management. 
- We will use Redux style dispatch and mutate pattern for frontend state management.
- We will use CDN Bootstrap styles for frontend UI styles for simplicity.

## Possible enhancements
- Use better authentication mechanism like OAuth and MFA.
- Use real database for persistent data storage.
- APIs for user management.
- Queuing mechanism to fetch and processing ticket requests to avoid overwhelm infrastructure.
- Separate APIs by concern into individual deployable units.
- CI/CD pipeline with test automation
- Cloud deployment
- Caching mechanism for to improve frequent reading tickets loading time.  
- Using Signalr for ticket update notifications.

## Change Log

### Version 1.2.0 (Current)
- **CustomerServiceApp.Web v1.2.0**: Removed navigation bars from login pages
  - Applied `TicketLayout` to both `/player/login` and `/agent/login` pages
  - Implemented full-screen login interface with centered vertical alignment
  - Enhanced login forms with shadow effects and better responsive design
  - Added demo navigation links to respective ticket views
  - Improved form accessibility with unique IDs for player/agent forms

### Version 1.1.0
- **All Projects v1.1.0** (Domain, Application, Infrastructure, API, Web, AppHost): Major architecture changes
- **BREAKING CHANGE**: Separated ticket views for players and agents
  - Renamed `/tickets` to `/player/tickets` for player view
  - Added new `/agent/tickets` for CS agent view
  - Updated navigation menu with separate links
- **Feature**: Created dedicated `TicketLayout.razor` without navigation bars
  - Removed left sidebar navigation menu from ticket pages
  - Removed top header bar from ticket pages
  - Implemented full-screen ticket interface
- **Feature**: Agent ticket view improvements
  - Shows only unresolved tickets from all players
  - Removed "New ticket" button for agents (agents cannot create tickets)
  - Auto-selection handling when tickets are resolved
- **Feature**: Player ticket view improvements
  - Shows only current player's tickets (filtered by player ID)
  - Maintains "New ticket" functionality for players
  - Player replies marked correctly with player avatar
- **Enhancement**: Added sample data to demonstrate multi-player scenarios
- **Enhancement**: Improved ticket filtering and status management

### Version 1.0.0
- **All Projects v1.0.0**: Initial project setup with clean architecture
- Basic ticket interface with sample data
- Bootstrap 5.3 UI styling with icons
- Two-panel layout (ticket list + details)
- Basic message threading functionality

**Note**: ServiceDefaults project remains at v1.0.0 as it contains cross-cutting concerns that haven't required changes.
