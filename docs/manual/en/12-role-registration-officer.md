# 12. Beekeeper registration officer {#role-ro}

## Your daily work {#ro-daily}

You work at a district directorate of agriculture, an organization with the capability **Beekeeper registration**. You register the beekeepers of your area and their apiaries, so that the plants can record their deliveries. For each beekeeper you:

1. check the enterprise registry number;
2. record the beekeeper and the first apiary;
3. let the beekeeper read the information notice and record their consent choices;
4. confirm the registration with the code the beekeeper receives by SMS, or with a signed and photographed form;
5. send the beekeeper the sign-in details for the beekeeper portal, when the portal is open.

The menu shows **Beekeeper registration**. You read and find only the registrations of your own directorate. A platform administrator can register beekeepers in the same way (for the platform's own organization).

## Find or register a beekeeper {#ro-find}

1. Open **Beekeeper registration**. The list shows the beekeepers registered by your organization, with the **Date**, the **Registry number**, how the registration was **Confirmed by** (SMS code or signed photo) and the state of the **Evidence**.
2. In **Registry number (AKS) or business number**, enter the number and click **Find or register**. "A known beekeeper is re-verified; a new registry number starts a new registration."

If the beekeeper belongs to another directorate: "This beekeeper is registered by another directorate."

<!-- SHOT: registration-list -->
Figure: **Beekeeper registration**: the registrations of a directorate.

## Register a beekeeper {#ro-register}

The form **Register a beekeeper** (or **Re-verify a beekeeper** for a known one) has four parts ([Figure](#fig-registration-form)).

1. **Registry number**: `TR`, the province plate code and 10 digits, for example `TR281234500004` ("TR, the province plate code and 10 digits. Compared with the directorate's province."). If the number belongs to another province, write the reason under **If the number belongs to another province: the reason**.
2. **The beekeeper**: **First Name**, **Last Name**, **Birth Date**, **Mobile phone**, the **Language of the SMS**, and the optional education level, beekeeping purpose and beekeeping type. Tick **The beekeeper has no mobile phone (signed photo, no account)** if there is none.
3. **Apiary**: the place of the first apiary (province, district and locality, which also give the apiary its title), its position and the number of hives. Further apiaries can be added later on the registration page.
4. **Information notice and consent**: "Let the beekeeper read this notice, or read it aloud". Then record the beekeeper's choices:
   - **The beekeeper agrees that their name is shown on public pages (optional; without it a neutral code is shown)**;
   - **The beekeeper agrees to SMS notifications about deliveries and lab results (optional)**.
   **Full texts of the consents** opens the texts.
5. **Your attestation**: tick **I have seen the beekeeper's identity document and the registry record.** Tick **Also send the beekeeper the sign-in details by SMS** if the beekeeper should get a portal account now (shown only while the portal is open).

<!-- SHOT: registration-form -->
Figure: Registering a beekeeper: registry number, identity, apiary, notice and consents.

### Confirm with the SMS code {#ro-sms}

1. Under **Confirmation**, click **Send code**. "The beekeeper gets an SMS with your organization, the notice link, the name choice and a code. Changing any field afterwards needs a new code."
2. Enter the **Code from the SMS (6 digits)** that the beekeeper reads out, and click **Confirm the registration**.

"The beekeeper is registered and verified." The registration is recorded with the confirmed content, the consents take effect, and the beekeeper can deliver at once. The code rules are the same as at a delivery: valid 5 minutes, 3 tries, a new code after a minute.

### No SMS possible: signed photo {#ro-photo}

If the beekeeper has no mobile phone, or the SMS cannot be sent:

1. Open **No SMS possible: signed photo** and click **Print the form**. The printed page carries the beekeeper's choices and the line "I have read the information notice; my choices above are correct. Signature of the beekeeper and date:".
2. Let the beekeeper sign it, take a photo, choose it under **Photo of the signed form** and write **Why no SMS code**.
3. Click **Save with the signed photo**: "The registration is saved with the signed photo. It waits for the Compliance Officer's review."

Until your directorate's Compliance Officer accepts the photo, the beekeeper cannot deliver; a rejected photo also withdraws the consents of the registration. You cannot review a registration you recorded yourself. A beekeeper without a mobile phone gets no portal account.

## The registration page {#ro-details}

Click a registration in the list to open it. It shows the registration, the **Registry number** (with "other province" when it differs), the **Mobile phone**, the **Consents** (with "withdrawn" when one was withdrawn), the **Evidence** and its review, and the **Apiaries** with their hives.

- **Add an apiary** records a further apiary of the beekeeper.
- **Send sign-in details by SMS** sends the beekeeper a user name and a temporary password for the beekeeper portal (only while the portal is open, and only for a registration confirmed by the SMS code: "A sign-in needs a registration confirmed by the SMS code (no account without a mobile phone)."). The beekeeper chooses their own password at the first sign-in.

<!-- SHOT: registration-details -->
Figure: A confirmed registration with its consents and apiaries.

The attested facts (identity, registry number, phone) are read-only, also on the administrators' beekeeper page; they change only with a new registration of the same beekeeper (re-verification), confirmed again.

**Common errors**

Table: Common errors when registering a beekeeper {#tbl-registration-errors}

| Message | What to do |
|---|---|
| "Enter the registry number (or the business number of a registered beekeeper)." | Enter a number before **Find or register**. |
| "The number must be TR, a province code 01-81 and 10 digits (e.g. TR061234567890)." | Correct the registry number. |
| "This beekeeper is registered by another directorate." | Ask that directorate. |
| "Wrong code. … tries left." / "The code has expired. Send a new code." | Ask the beekeeper to read the code again, or send a new one. |
| "The details changed after the code was sent. Send a new code." | You changed a field after sending the code. |
| "Too many codes to this phone number. Try again later." | At most 5 codes an hour and 10 a day go to one phone number. |
