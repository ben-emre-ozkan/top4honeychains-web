# 2. Getting started {#getting-started}

## Signing in {#sign-in}

1. Open the platform address and click **Sign in** on the landing page, or go straight to `/Account/Login`.
2. Enter your **Username** (your user name, not your e-mail address) and your **Password**.
3. Tick **Remember me** if you want to stay signed in after closing the browser. Leave it empty on a shared computer.
4. Click **Log In**.

<!-- SHOT: sign-in -->

After signing in you land on **My work**, the list of tasks waiting for you.

**Who can sign in.** You need either the platform administrator role or at least one active membership in an organization. A platform administrator gives you the membership (see [Members](#admin-members)).

**Common messages**

- "Invalid user name or password. After too many failed attempts the account is locked for a while." The user name or password is wrong, or the account is locked. After **5 failed attempts the account is locked for 15 minutes**. Wait and try again, or use **Forgot your password?**.
- "This account does not have access to the web panel. …" Your account has no active membership. Ask a platform administrator to add you to your organization.

To sign out, open the menu under your avatar (top right) and choose **Logout**.

## Forgot your password {#password-reset}

1. On the sign-in page click **Forgot your password?**.
2. Enter the **Email** address of your account and click **Reset**.
3. The page always answers "If an account with this e-mail address exists, password reset instructions have been sent to it." (It does not reveal which addresses have accounts.)
4. Open the e-mail "Reset your TOP4HoneyChains password" and click the link. The link is valid for one day.
5. Enter the **New Password** twice and confirm. You are sent back to the sign-in page.

Passwords need at least 6 characters. If the e-mail does not arrive, check the spam folder, then ask a platform administrator, who can set a new password for you (see [Users](#admin-users)).

<!-- SHOT: forgot-password -->

## Your profile {#profile}

Open the avatar menu and choose **Profile**.

- **Username** and **Email** are shown but cannot be changed here. Ask a platform administrator.
- **Phone Number** can be changed. Click **Save**.
- **Change Password** opens a dialog: enter the **Current Password**, the **New Password** and **Confirm Password**, then click **Update Password**.

The avatar menu also has **Lock Screen**, which hides the page until you enter your password again. It is a privacy screen only; it does not sign you out.

<!-- SHOT: profile -->

## Choosing the language {#language}

The platform speaks Turkish, English, Spanish, Polish and Latvian. Turkish is the default.

- In the panel, use the flag button in the top bar. (On a narrow phone screen the flag button is hidden; switch the language on the sign-in page instead.)
- On the sign-in pages, use the **TR EN ES PL LV** links at the top right.
- On the public pages, use the language links in the top bar.

The choice is remembered in your browser for one year. Names from reference data (honey types, control points, roles) follow the same language; where a translation is missing, the English text is shown.

## The top bar and your role {#top-bar}

The top bar shows, from left to right:

- the menu button (on phones) and the TOP4HoneyChains logo, which opens the public landing page;
- the **organization · role chip**: the organization and the role you are working in now;
- the language button;
- a full-screen button;
- your avatar, with **Profile**, **Lock Screen** and **Logout**.

<!-- SHOT: top-bar-role-chip -->

The chip shows a check mark when your organization is verified and an exclamation mark when it is not. While an organization is not verified, its members can read its data, and its organisation admin can upload documents, but **nobody can enter data**. A banner says: "Your organization is not verified yet. …"

### Several memberships {#memberships}

One person can hold several memberships: in different organizations, or several roles in the same organization (for example **Quality approver** and **Organisation admin** at an association). You always work in one membership at a time.

To switch:

1. Click the chip. A list headed **Your roles** shows every active membership; the current one is ticked.
2. Click another membership. The platform opens **My organization** in the new membership.

The menu, the tasks in **My work**, the records you can see and the actions you may take all follow the active membership. The choice lasts until you close the browser; after that you start again in your first membership (alphabetically by organization name). **My organization** also has an **Act as** list with a **Switch** button when you have two or more memberships.

<!-- SHOT: role-switch -->

## My work {#my-work}

**My work** is your home page. It lists everything that is waiting for you in the active membership, grouped by kind, oldest first. Each group has a tile at the top; click it to jump to the group.

<!-- SHOT: my-work -->

| Group | What it means | Who gets it |
|---|---|---|
| **Values to enter** | A control point (CCP) of a lot or batch needs values | The roles that enter that CCP |
| **Values to approve** | A CCP record, or a CCP 10 storage record, was submitted and needs a second person | The roles that approve that CCP |
| **Finalize requests** | Someone requested finalizing a batch | Quality approver of the batch owner |
| **Samples to receive** | A sample was sent to your laboratory | Analyst, Lab manager |
| **Lab results to enter** | A received sample has no report yet, or the report was returned | Analyst |
| **Lab reports to approve** | A report was submitted | Lab manager |
| **Shipments to confirm** | A shipment to your organization is waiting | Staff, Organisation admin of the receiver |
| **Organizations to verify** | An organization uploaded documents and waits for verification | Platform administrator |
| **Registry numbers to verify** | A beekeeper's enterprise registry number waits for verification | Platform administrator |
| **Corrections to decide** | Another administrator requested a correction | Platform administrator |

Each item shows a title (for example "CCP 0 · #20070" and the word **lot**, **batch** or **storage**), how long it has waited (items older than three days get an amber mark) and a **New** chip if you have not seen it before. A lab item can also show why it must wait, for example: "CCP 0 (primary audit) of this lot is not completed yet; lab results are entered and approved only after it."

The **My work** link in the menu carries a badge with the number of waiting items. The page checks for new work every minute while it is open; when something new arrives, the link glows once and a small notice lists the groups that grew. When nothing is waiting, the page says "Nothing is waiting for you." and offers links to the pages you can open.

## The menu {#menu}

Organization members see a short menu. Each entry appears only if your organization has the capability and your role has the right:

| Menu entry | Opens | Shown when |
|---|---|---|
| **My work** | Your tasks | Always |
| **My organization** | Your organization's lots and batches | Always |
| **Deliveries (intake)** | Recorded deliveries | Processing organization; roles with intake read right |
| **Samples and lab** | Samples sent and received, lab reports | Processing or laboratory organization; roles with sample read right |
| **Shipments and stock** | Batches you hold, incoming and outgoing shipments, storage records | Processing, packing, wholesale or retail organization; roles with shipment read right |
| **Beekeeper pages** | Consents and public beekeeper pages | Association; roles with storefront read right |

Platform administrators see the full administration menu (see [chapter 12](#admin)).

## Messages, loading and error pages {#messages}

- **Messages** appear in the top corner. Success and information messages close after a few seconds; error messages stay until you close them.
- The **loader** (an animated honey bee) appears when a page or a save takes longer than a moment.
- **Error pages**: "You can't open this page" (403) means your role does not allow the page; ask your organisation admin. "This page isn't here" (404) means the address does not exist or the record is not visible to you. A 500 page shows a short reference; quote it when you report the problem. Each error page has a button back to **My work** and a **Go back** button.
