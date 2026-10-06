# Domain Model

## Overview

This document describes the core entities of the system, their 
attributes, and the relationships between them — independent of database 
or implementation details (covered in 05-Database.md).

## Entities

### User
Represents a registered person using the platform, either as a seller, 
a buyer, or both.

**Attributes:**
- id
- email (unique)
- password (hashed)
- displayName
- role (`user` | `admin`) — default: `user`
- createdAt

### Listing
Represents an instrument listing published by a user.

**Attributes:**
- id
- title
- description
- price
- category (fixed enum — see Glossary)
- condition (fixed enum — see Glossary)
- status (`available` | `sold`) — default: `available`
- createdAt
- updatedAt

### Image
Represents a single photo attached to a listing. A listing can have up 
to 5 images.

**Attributes:**
- id
- url (Cloudinary URL — see 05-Database.md → Image storage strategy)
- displayOrder (to control image order in the gallery)

## Relationships

- **User → Listing**: one-to-many  
  A user can publish many listings; each listing belongs to exactly one user.

- **Listing → Image**: one-to-many  
  A listing can have up to 5 images; each image belongs to exactly one listing.

```
User (1) ──────< (many) Listing (1) ──────< (many) Image
```

## Glossary (fixed enums)

**Category** (fixed list, not a separate entity):
- Guitars
- Keyboards
- Drums
- Wind Instruments
- Strings
- Other

**Condition** (fixed list):
- New
- Like New
- Good
- Fair
- Poor

**Listing status** (stored lowercase):
- `available` (default)
- `sold`

**User role** (stored lowercase):
- `user` (default)
- `admin` (assigned manually, not selectable at registration)

## Notes on design choices

- **Category and Condition are not separate database tables.** Since 
  they are fixed, small lists that don't change dynamically, they are 
  modeled as enums directly on the `Listing` entity. This avoids 
  unnecessary complexity (no need for joins just to resolve a category 
  name). If the project evolves to support dynamic, admin-managed 
  categories, this would change — noted as a possible future 
  consideration in 09-Roadmap.md.
  
- **Image is a separate entity**, not a simple array field, because each 
  image needs its own identity (e.g. for deletion, reordering, or 
  independent metadata) — this maps naturally to a one-to-many 
  relationship in a relational database.

- **A single `email` field is used** both for login and as the contact 
  reference shown to buyers, keeping the model simpler. If needed in the 
  future, a separate public contact field could be introduced without 
  breaking this structure.