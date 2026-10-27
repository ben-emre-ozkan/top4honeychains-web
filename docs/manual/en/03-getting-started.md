# 3. Getting started {#getting-started}

## Signing in {#sign-in}

1. Open the platform address and click **Sign in** on the landing page, or go straight to `/Account/Login`.
2. Enter your **Username** (your user name, not your e-mail address) and your **Password**.
3. Tick **Remember me** if you want to stay signed in after closing the browser. Leave it empty on a shared computer.
4. Click **Log In**.

<!-- SHOT: sign-in -->
Figure: The sign-in page.

After signing in you land on **My work**, the list of tasks waiting for you. A beekeeper lands on the beekeeper portal instead (see [chapter 14](#portal)).

**Who can sign in.** You need either the platform administrator role or at least one active membership (one organization × one role). A platform administrator gives you the membership (see [Members](#admin-members)). Beekeepers sign in with a portal account that their registration officer sends them.

**Common messages**

- "Invalid user name or password. After too many failed attempts the account is locked for a while." The user name or password is wrong, or the account is locked. After **5 failed attempts the account is locked for 15 minutes**. Wait and try again, or use **Forgot your password?**.
- "This account has no access to the panel yet: it is not a member of any organization. …" Your account has no active membership. Ask a platform administrator to add you to your organization.

To sign out, open the menu under your avatar (top right) and choose **Logout**.

### Your first sign-in {#first-sign-in}

New accounts get their sign-in details by **SMS**: the user name and a temporary password of 10 characters, valid for **72 hours**. Nobody else ever sees this password. (A platform administrator can also set an initial password instead and give it to you in person.)

1. Sign in with the user name and the temporary password.
2. The page **Choose your password** opens: "You signed in with a temporary password. Choose your own password before you continue."
3. Enter the **New Password** twice and click **Update Password**.

A password needs **at least 10 characters**, must not be your user name and must not be a common password. Signing in with the temporary password also confirms your mobile phone number. If the 72 hours have passed, ask for new sign-in details: the platform administrator (members) or the registration officer (beekeepers) sends them again.

## Forgot your password {#password-reset}

The password is reset with a code sent by SMS to your **verified** mobile phone:

1. On the sign-in page click **Forgot your password?**.
2. Enter your **User Name** and click **Send code**. The page always answers "If the account exists and has a verified mobile phone, a code was sent by SMS." (It does not reveal which accounts exist.)
3. Enter the **Code from the SMS (6 digits)**, then the **New Password** twice, and confirm. The code is valid for **5 minutes**; **Send a new code** sends another one after a minute.

"No verified mobile phone? Ask the person who gave you your sign-in details to send new ones." A code can be tried 3 times; at most 3 reset codes are sent per account and hour.

<!-- SHOT: forgot-password -->
Figure: Password reset: the code from the SMS and the new password.

(If the platform is configured to reset passwords by e-mail, the page asks for the **Email** address instead and sends a link that is valid for one day.)

## Your profile {#profile}

Open the avatar menu and choose **Profile**.

- **Username** and **Email** are shown but cannot be changed here. Ask a platform administrator.
- **Phone Number** shows whether the number is **verified**. To change it, enter the new number and click **Send code**, then enter the **Code from the SMS (6 digits)** and click **Confirm**. "A new number is used only after you enter the code we send to it. A verified number lets you reset your password by SMS."
- **Change Password** opens a dialog: enter the **Current Password**, the **New Password** and **Confirm Password**, then click **Update Password**.

To leave a shared computer, use **Logout**; there is no lock screen.

<!-- SHOT: profile -->
Figure: The profile page with the phone check and the **Change Password** dialog.

## The privacy notice {#notice-accept}

Before you work in the platform you read the **privacy notice**: how your personal data is processed, by the platform and by each organization you work for, and who the **data controller** is (see [Notices, consents and the data controller](#trust-notices)).

1. While a notice waits for you, a banner says "Please read the privacy notice and confirm that you have read it." Click it. (When the platform is configured to require it, the notice page opens by itself after signing in and the panel stays closed until you confirm.)
2. Read the notice. Each organization's notice names its data controller and the **Contact for requests**.
3. Tick **I have read the notice above.** and click **Confirm**.
4. Roles that sign on behalf of their organization (Organization admin, Compliance Officer, Beekeeper registration officer, Quality Assurance Officer, Lab manager) confirm with a code: "Your role signs on behalf of your organization, so you confirm with a code sent to your mobile phone." Click **Send code** and enter the **Code from the SMS (6 digits)**.

When a new version of a notice is published, you are asked again. "You have accepted the current notices. Nothing else is needed." means you are up to date.

<!-- SHOT: notice-accept -->
Figure: Confirming the privacy notice with an SMS code.

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
- your avatar, with **Profile**, **Show the tour**, the **Show animations** switch and **Logout**.

<!-- SHOT: top-bar-role-chip -->
Figure: The top bar with the organization · role chip.

The chip shows a check mark when your organization is verified, an exclamation mark while it is waiting for verification, and a stop mark on a red background when it is suspended. While an organization is not verified, its members can read its data, and its organization admin can upload documents, but **nobody can enter data**. A banner says: "Your organization is not verified yet. …", or for a suspended organization "Your organization is suspended. You can read its data, but nobody can enter data until a platform administrator reinstates it."

If you have several memberships, click the chip to switch between them; see [Multiple memberships](#memberships).

<!-- SHOT: role-switch -->
Figure: Switching membership: the **Your roles** list of the chip.

## The tour and the animations {#animations}

**The first-use tour.** The first time you work in a role, a short tour (**A quick tour**, 3 to 5 steps) points at the menu entries of that role, one line each. **Next** and **Back** move through it; **Skip the tour** or Esc closes it. The platform remembers per user and role that you have seen it, also on another computer or phone. **Show the tour** in the avatar menu opens it again.

**Explanatory animations.** At a few places a short scene of at most one and a half seconds shows what a button does or what just happened, with a one-line caption: for example the cap of a sample jar pressing down when you take a sample, a truck when a delivery is recorded, a seal clicking shut, the jars travelling to the laboratory, the check at reception and a shipment leaving. A scene never blocks the page: a key press, a click or Esc ends it at once, and its last frame stays as a still picture. The ⓘ button **Show what happens** plays it again.

<!-- SHOT: scene-static -->
Figure: The still picture of an explanatory scene (the sample jar) with its caption.

**Show animations** in the avatar menu turns the scenes off or on for your account ("Short scenes that show what a button does and what just happened. When off, a still picture is shown."). When your device asks for reduced motion, only the still pictures are shown.

## My work {#my-work}

**My work** is your home page ([Figure](#fig-my-work)). It lists everything that is waiting for you in the active membership, grouped by kind, oldest first ([Table](#tbl-mywork-groups)). Each group has a tile at the top; click it to jump to the group.

<!-- SHOT: my-work -->
Figure: **My work** of a Quality Assurance Officer, with the group tiles.

Table: Groups in **My work** {#tbl-mywork-groups}

| Group | What it means | Who gets it |
|---|---|---|
| **Values to enter** | A control point (CCP) of a lot or batch needs data | The roles that enter that CCP |
| **Values to approve** | A CCP record or a CCP 10 storage record was submitted, or completed or marked "Not applicable" without an approval, and needs a second person. **My work** never lists records you entered or marked yourself. | The roles that approve that CCP |
| **Finalize requests** | Someone requested finalizing a batch | Quality Assurance Officer of the batch owner |
| **Samples to receive** | A sample jar was sent to your laboratory | Analyst, Lab manager |
| **Lab results to enter** | A received sample has no report yet, or the report was returned | Analyst |
| **Lab reports to approve** | A report was submitted | Lab manager |
| **Shipments to confirm** | A shipment to your organization is waiting | Staff, Organization admin of the receiver |
| **Review signed photos** | A registration or a delivery was confirmed with a signed photo instead of an SMS code | Compliance Officer of the recording organization (never the person who recorded it) |
| **Reviews** | A delivery by photo, a rejected sample or a broken seal waits for a sampling-quality review | Compliance Officer |
| **Data-subject requests** | A request of a person about their data is open (answer within 30 days) | Compliance Officer |
| **Organizations to verify** | An organization uploaded documents and waits for verification | Platform administrator (associations: the central association, when configured) |
| **Registry numbers to verify** | A beekeeper's enterprise registry number waits for verification | Platform administrator |
| **Corrections to decide** | Another administrator requested a correction of CCP data or of delivery facts | Platform administrator |

Each item shows a title (for example "CCP 0 · #20070" and the word **lot**, **batch** or **storage**), how long it has waited (items older than three days get an amber mark) and a **New** chip if you have not seen it before. A lab item can also show why it must wait, for example: "CCP 0 (primary audit) of this lot is not completed yet; lab results are entered and approved only after it."

The **My work** link in the menu carries a badge with the number of waiting items. The page checks for new work every minute while it is open; when something new arrives, the link glows once and a small notice lists the groups that grew. When nothing is waiting, the page says "Nothing is waiting for you." and offers links to the pages you can open.

## The menu {#menu}

Organization members have a short menu ([Table](#tbl-menu)). Each entry appears only if your organization has the capability and your role has the right; the pages check the same rights when you open them by their address:

Table: The menu of organization members {#tbl-menu}

| Menu entry | Opens | Shown when |
|---|---|---|
| **My work** | Your tasks | Always |
| **My organization** | Your organization's lots and batches | Always |
| **Deliveries (intake)** | Recorded deliveries | Processing organization; roles with intake read right |
| **Samples and lab** | Samples sent and received, lab reports | Processing or laboratory organization; roles with sample read right |
| **Shipments and stock** | Batches you hold, incoming and outgoing shipments, storage records | Processing, packing, wholesale or retail organization; roles with shipment read right |
| **Beekeeper pages** | Consents and public beekeeper pages | Association; roles with storefront read right |
| **Beekeeper registration** | Registrations of your directorate | Beekeeper registration organization; Beekeeper registration officer |
| **Compliance** | Reviews, signed photos, requests, approvals, agreements, audit entries | Compliance Officer |

Platform administrators have the full administration menu (see [chapter 15](#admin)).

## Messages, loading and error pages {#messages}

- **Messages** appear in the top corner. Success and information messages close after a few seconds; error messages stay until you close them. A message that refers to a role names the role, never its code.
- The **loader** (an animated honey bee) appears when a page or a save takes longer than a moment.
- **Error pages**: "You can't open this page" (403) means your role does not allow the page; ask your organization admin. "This page isn't here" (404) means the address does not exist or the record is not visible to you. A 500 page shows a short reference; quote it when you report the problem. Each error page has a button back to **My work** and a **Go back** button.
