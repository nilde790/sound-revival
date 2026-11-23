# User Stories

Each story has a stable id (`US-n`), the requirements it covers (see 
[02-Requirements.md](02-Requirements.md)) and **acceptance criteria** in 
Given / When / Then form. A story is done when all its criteria pass 
(see the Definition of Done in 
[dev/ENGINEERING-STANDARDS.md](dev/ENGINEERING-STANDARDS.md)). 
Acceptance criteria are also the starting list of test cases.

## Authentication

**US-1** — As a **visitor**, I want to register with my email and 
password, so that I can create and manage my own listings. 
*Covers*: FR-1, FR-5, FR-17
- Given valid data, when I register, then my account is created with 
  role `user` and I receive a token (I am logged in immediately).
- Given an email that is already registered (in any letter case), when I 
  register, then I get `400` and no account is created.
- Given an invalid email, a too-short password or an empty display name, 
  when I register, then I get `400`.
- Given any registration, then the stored password is a hash and it 
  never appears in the response.

**US-2** — As a **registered user**, I want to log in with my 
credentials, so that I can access my account and listings. 
*Covers*: FR-2
- Given correct credentials (email in any letter case), when I log in, 
  then I receive a token, my user id, display name and role.
- Given a wrong password or an unknown email, when I log in, then I get 
  `401` with the same message in both cases.
- Given too many failed attempts in a short time, when I try again, then 
  I get `429` (NFR-11).

**US-3** — As a **registered user**, I want to log out, so that I can 
secure my account on a shared device. 
*Covers*: FR-3
- Given I am logged in, when I log out, then the frontend discards my 
  token and protected pages redirect me to the login page.
- Given I logged out, when the old refresh token is presented again, 
  then the refresh fails with `401` (revoked on the server, ADR-012, 
  phase 3).
- Given I am logged in and reload the page, then I stay logged in (the 
  frontend gets a new access token through the refresh cookie).

**US-4** — As a **registered user**, I want to view and edit my profile, 
so that my information stays up to date. 
*Covers*: FR-4
- Given I am logged in, when I open my profile, then I see my email and 
  display name.
- Given a valid new display name, when I save, then it is updated; given 
  an empty one, then I get `400`.

## Listings management

**US-5** — As a **registered user**, I want to create a listing with 
title, description, category, condition, price and optionally the brand, 
so that I can sell an instrument I no longer use. 
*Covers*: FR-6, FR-8, FR-16
- Given valid data, when I create a listing, then it is saved with 
  status `available`, I am its owner, and I get `201` with the created 
  listing.
- Given a category or condition outside the allowed lists, a negative or 
  zero price, or a missing required field, then I get `400`.
- Given I am not logged in, then I get `401`.

**US-6** — As a **listing owner**, I want to upload up to 5 photos per 
listing, so that buyers can clearly see the instrument's condition. 
*Covers*: FR-19
- Given my listing has fewer than 5 images, when I upload one, then it is 
  stored and shown in the listing's gallery in the chosen order.
- Given my listing already has 5 images, when I upload another, then I 
  get `400`.
- Given a listing I don't own, when I upload or delete an image, then I 
  get `403`.

**US-7** — As a **listing owner**, I want to edit my own listing, so that 
I can correct or update information after publishing it. 
*Covers*: FR-7
- Given my listing, when I send valid changes, then they are saved and 
  `updatedAt` changes.
- Given a listing I don't own (and I'm not an admin), then I get `403`; 
  given a listing that doesn't exist, then I get `404`.
- The edit cannot change the status, the owner or the creation date.

**US-8** — As a **listing owner**, I want to delete my own listing, so 
that I can remove it once I no longer wish to sell it. 
*Covers*: FR-7
- Given my listing, when I delete it, then it and its images are removed 
  and I get `204`.
- Given a listing I don't own (and I'm not an admin), then I get `403`.

**US-9** — As a **listing owner**, I want to mark my listing as sold, so 
that buyers stop contacting me about an unavailable item. 
*Covers*: FR-8, FR-9, FR-10
- Given my `available` listing, when I set its status to `sold`, then it 
  disappears from default search results but stays reachable by its link, 
  marked as sold.
- Given a status value outside the allowed list, then I get `400`.

## Browsing & search

**US-10** — As a **visitor**, I want to see a homepage with available 
listings, so that I can browse without needing an account. 
*Covers*: FR-11, FR-10, FR-22
- Given I am not logged in, when I open the homepage, then I see the 
  first page (20 items) of `available` listings, newest first.
- Given more listings than one page, then I can move between pages; a 
  page size above 100 or a page below 1 is rejected with `400`.

**US-11** — As a **visitor**, I want to filter listings by category and 
condition, so that I can quickly find the type of instrument I'm looking 
for. 
*Covers*: FR-12
- Given a category and/or condition from the allowed lists, then only 
  matching listings are returned; given a value outside the lists, then 
  I get `400`.

**US-12** — As a **visitor**, I want to search listings by keyword, so 
that I can find a specific instrument. 
*Covers*: FR-13
- Given a keyword, then listings whose title or description contain it 
  (case-insensitive) are returned.
- Given a keyword containing `%` or `_`, then those characters are 
  matched literally, not as wildcards.

**US-13** — As a **visitor**, I want to filter listings by price range, 
so that I can stay within my budget. 
*Covers*: FR-14
- Given a minimum and/or maximum price, then only listings within the 
  range (inclusive) are returned; given minimum > maximum, then I get 
  `400`.

**US-14** — As a **visitor**, I want to filter listings by brand, so that 
I can find instruments from a maker I like. 
*Covers*: FR-20
- Given a brand text, then listings whose brand contains it 
  (case-insensitive) are returned.

**US-15** — As a **visitor**, I want to sort results by newest or by 
price, so that I can see the most relevant listings first. 
*Covers*: FR-21
- Given no sort option, then results are newest first; given 
  `price_asc` / `price_desc`, then they are ordered by price; given any 
  other value, then I get `400`.
- Listings with the same sort value always appear in the same order 
  across pages (no duplicates or gaps).

**US-16** — As a **visitor**, I want to view a listing's full details, so 
that I can evaluate the instrument before contacting the seller. 
*Covers*: FR-15, FR-10
- Given an existing listing (available or sold), when I open it, then I 
  see all its fields, its images, and the seller's display name.
- Given a listing id that doesn't exist, then I get `404`.

## Contact

**US-17** — As a **logged-in visitor**, I want to see the seller's 
contact email on a listing, so that I can reach out directly to 
negotiate a purchase. 
*Covers*: FR-15
- Given I am logged in, when I open a listing, then the seller's email 
  is included.
- Given I am not logged in, then the email is not included in the 
  response (not just hidden in the UI), and the page invites me to log 
  in to contact the seller.

## Roles & moderation

**US-18** — As an **admin**, I want to edit or delete any listing (not 
just my own), so that I can remove inappropriate or spam content. 
*Covers*: FR-18
- Given I have role `admin`, when I edit or delete any listing, then the 
  operation succeeds as it would for the owner.
- Given role `user`, then the same operation on someone else's listing 
  returns `403`.

Moderation beyond edit/delete (flagged listings, user management, a 
dedicated dashboard) is v2 scope, see 
[09-Roadmap.md](09-Roadmap.md).
