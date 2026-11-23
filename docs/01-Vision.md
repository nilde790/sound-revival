# Vision

## The problem

People who own musical instruments they no longer use (private sellers or 
small shops) usually end up selling them on generic marketplaces (e.g. 
Subito.it, eBay), where musical instrument listings are mixed with 
thousands of unrelated categories. This makes it harder both to publish 
a well-categorized listing and, from the buyer's side, to search for a 
specific instrument in a focused way.

## Proposed solution

A vertical marketplace dedicated exclusively to buying and selling musical 
instruments between users, with categories and search filters designed 
specifically for this domain (instrument type, brand, condition, price).

## Target users

- Private users who want to sell one or more musical instruments
- Small shops/resellers looking for a simple online channel
- Musicians and enthusiasts searching for a specific instrument at a fair price

No distinction is made at the role level between "private seller" and 
"shop": every registered user can publish and search listings in the same way.

## What this project is NOT

This project is a **technical case study**, not a product meant to compete 
in the real market. Its primary goal is to demonstrate full-stack 
development skills (authentication, relational data management, APIs, 
frontend, deployment), not to innovate over existing marketplaces.

## Personal & professional goals

Beyond the product itself, this project exists to support my growth as a 
developer. Specific goals include:

- Practicing the full lifecycle of a software project: from requirements 
  to architecture, implementation, testing, and deployment
- Gaining hands-on experience designing a relational database with 
  non-trivial relationships
- Building and consuming a REST API with proper authentication
- Writing clean, maintainable frontend code with sensible state management
- Learning to document technical decisions clearly (ADRs), as done in 
  professional environments
- Producing a project that is genuinely presentable in a professional 
  portfolio, not just functionally complete

## v1 scope (MVP)

**Included:**
- User registration and authentication
- Creating, editing, and deleting listings, and marking them as sold
- Searching and filtering listings by keyword, category, brand, 
  condition and price, sorted by newest or by price
- Homepage displaying listings from all users
- Simple contact between users: the seller's e-mail is shown to 
  logged-in visitors only (no in-app chat in v1)

**Planned for v2 (committed, after v1 — see 09-Roadmap.md):**
- Real-time in-app messaging between users
- Simulated payments (test mode, no real money)
- Admin moderation dashboard, dynamic categories

**Excluded:**
- Real payments
- Shipping management

## Why this domain

Choosing a vertical marketplace, while not an original idea, allows work 
on a set of technical skills relevant to the software development 
industry: authentication, CRUD with non-trivial relationships, search/
filtering, image upload, and listing state management (available/sold). 
The goal is not product originality, but implementation quality.