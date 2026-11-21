# Outline of the user manual (all languages)

Source: `inventory.md` and the running application (fictional database `TOP4HoneyChainsDb_Manual`, the Şile cast with fictional organisation names). The manual describes the platform with the organization model, custody intake, beekeeper accounts and consumer accounts switched on (owner's decision of 21 November 2026, recorded in `docs/GO-LIVE.md`). Phase 1b (21 November 2026) added the conceptual and reference chapters of the October manual, updated to the code of D-94.

Chapters follow the journey of the honey. Picture names are `<chapter>-<number>-<name>.png`; `capture/capture.mjs` takes every one of them. Phone-sized pictures are marked (phone).

The journey that `capture.mjs` carries out (each picture is taken where the step happens):
Hatice Turan is registered (murat.kaya) → delivers chestnut honey in two tins (hasan.demir) → CCP 0 entered (hasan.demir) and approved (mehmet.ozturk) → the laboratory receives the jars, makes the composite sample, enters results (can.yildiz), the lab manager approves (deniz.koc) → lot control sheet entered (zeynep.arslan), approved (mehmet.ozturk) → new batch (zeynep.arslan) → batch control sheet entered and approved → finalizing requested and confirmed → shipment to Çarşı Market (retailer), receipt and storage (gizem.polat), storage approved (okan.kurt) → Honey Story on a phone → a consumer saves the honey and sends feedback → the association answers (mehmet.ozturk). The beekeeper portal is shown with the beekeeper Ali Yazıcı, who already has two deliveries.

## About this manual {-}
Who it is for, how it is organised (chapters per role along the journey), conventions (bold labels, numbered steps, numbered marks in pictures, menu paths), the sample data in the pictures is fictional.

## 1 The platform at a glance
- What the platform is for (project, partner countries, funding by the EU and the national agencies; definition of data). The journey of the honey: what people do, what the platform records, first and second person. Who the platform is for (table incl. consumer and visitor). The control points (table CCP 0 to CCP 10). The four-eyes principle (table of first and second person). What the consumer reads.
- Pictures: 01-01-home (the public start page), 01-02-flow (process flow of the finalized batch).

## 2 Capabilities, organisations and roles
- What decides what you may do. Capabilities as abilities. Value-chain activities and platform records. Organisation types and status. The role table (13 roles incl. Beekeeper, Consumer, Visitor). Data rights (CRUD + Approve) with notes. Permission matrix per platform activity. What an administrator can change. Ground rules. Memberships.
- Pictures: none (tables).

## 3 Getting started
| Task | Pictures |
|---|---|
| Open the platform and choose the language | 02-01-language (sign-in page, language links marked) |
| Sign in | 02-02-sign-in |
| Sign in for the first time with the details from the SMS | 02-03-first-password |
| Confirm the privacy notice | 02-04-notice-banner, 02-05-notice-confirm |
| Find your way: My work, the menu, the role, the user menu | 02-06-my-work (marked), 02-07-role-switch, 02-08-user-menu |
| Take the tour | 02-09-tour |
| Verify your mobile phone | 02-10-profile |
| Reset a forgotten password | 02-11-forgot-password |
| Sign out | (02-08) |

## 4 Beekeeper registration officer
| Task | Pictures |
|---|---|
| Find a beekeeper or start a registration | 03-01-registration-list |
| Register a new beekeeper | 03-02-form-person, 03-03-form-apiary, 03-04-form-consents, 03-05-code |
| Add another apiary | 03-06-add-apiary |
| Send the beekeeper the sign-in details | 03-07-details |
| Registration without a mobile phone (signed photo) | (text only) |

## 5 Sample acceptance officer: honey delivery
| Task | Pictures |
|---|---|
| See the deliveries | 04-01-deliveries |
| Record a honey delivery | 04-02-find, 04-03-delivery-form, 04-04-containers, 04-05-code |
| Print the labels | 04-06-delivery, 04-07-labels |
| Enter CCP 0 (primary audit) | 04-08-my-work, 04-09-ccp0 |

## 6 Laboratory: analyst and lab manager
| Task | Pictures |
|---|---|
| Receive the sample jars | 05-01-my-work, 05-02-receive |
| Make the composite sample | 05-03-composite |
| Enter the results | 05-04-report |
| Submit the report | 05-05-submit |
| Approve or return a report (lab manager) | 05-06-approve |
| Verify a lab report | 05-07-verify (public page) |

## 7 Staff: processing and packing
| Task | Pictures |
|---|---|
| See your lots and batches | 06-01-my-organization |
| Fill in the control sheet of a lot (CCP 2 and 3) | 06-02-lot-sheet |
| Create a batch | 06-03-new-batch |
| Fill in the control sheet of a batch (CCP 4 to 10) | 06-04-batch-sheet |
| Request finalizing | 06-05-request |
| Ship honey to another organization | 06-06-shipments, 06-07-ship |

## 8 Quality Assurance Officer
| Task | Pictures |
|---|---|
| Approve CCP 0 | 07-01-my-work, 07-02-approve-ccp0 |
| Approve a control sheet | 07-03-approve-sheet |
| Finalize a batch | 07-04-finalize, 07-05-finalized |
| Answer consumer feedback | 07-06-feedback |

## 9 Wholesaler and retailer: receiving and storing honey
| Task | Pictures |
|---|---|
| Confirm a shipment | 08-01-incoming |
| Record the storage conditions (CCP 10) | 08-02-storage |
| Approve the storage record (Quality Assurance Officer) | 08-03-approve-storage |

## 10 Organization admin
| Task | Pictures |
|---|---|
| See your organization's page and documents | 09-01-organization |
| Publish a beekeeper page (association) | 09-02-beekeeper-pages |

## 11 Compliance Officer
| Task | Pictures |
|---|---|
| Overview: tasks and where to find them | 09-03-compliance |
| Review a signed photo; review the sampling quality; handle a data-subject request; record a consent withdrawal; check the approval mechanism; keep agreements and documents; keep the data controller details; read the organisation's audit entries; what happens next | (text and tables) |

## 12 Beekeeper portal (phone)
| Task | Pictures |
|---|---|
| Sign in and read the start page | 10-01-home (phone) |
| See your deliveries and enter your price | 10-02-deliveries (phone), 10-03-price (phone) |
| See lab results, batches and Honey Story pages | 10-04-samples (phone) |
| Manage your consents | 10-05-privacy (phone) |
| Account: phone, language, password | 10-06-account (phone) |

## 13 The Honey Story page (phone)
| Part | Pictures |
|---|---|
| Open the page from the QR code | 11-01-top (phone) |
| The journey of this honey | 11-02-journey (phone) |
| Where it comes from | 11-03-origins (phone) |
| Lab results | 11-04-lab (phone) |
| Process checks | 11-05-process (phone) |

## 14 Consumer account (phone)
| Task | Pictures |
|---|---|
| Create an account | 12-01-sign-up (phone) |
| Save a honey | 12-02-save (phone) |
| Send feedback to the producer | 12-03-feedback (phone) |
| Read the answer, manage your account | 12-04-my-honeys (phone), 12-05-account (phone) |

## 15 Platform administration
| Task | Pictures |
|---|---|
| The dashboard | 13-01-dashboard |
| Create and verify an organization | 13-02-organizations, 13-03-new-organization, 13-04-organization-details |
| Add a member | 13-05-permissions |
| Who enters and approves each control point | 13-06-ccp-responsibilities |
| Users and sign-in details | 13-07-users |
| Records, audit log, corrections | 13-08-records, 13-09-audit |
| Notices and translations | 13-10-notices, 13-11-translations |
| Change the status of an organisation; capabilities; four-eyes exemption; revoke a membership; data controller | (13-04) |
| Change the rights of a role; who enters and approves a control point | (13-05, 13-06) |
| Edit a control point or a parameter; set the limits; mark limits as verified | 13-12-limits |
| Maintain and translate the lists of the forms | (13-11) |
| Verify a registry number; change a beekeeper's association | 13-13-registry |
| Request and decide a correction; correct the facts of a delivery | (text) |
| Open the recall report | 13-14-recall |
| Publish a new version of a notice | (13-10) |
| Check the data retention report | 13-15-retention |
| Search the audit log; check the hash chain | (13-09) |
| Settings in the server configuration | (table) |

## 16 Trust and privacy
Text and tables: the audit log and its hash chain; records that do not change; codes and file fingerprints; notices, consents and the data controller; what the public can read; how long data is kept; consumer accounts and pseudonyms.

## 17 Problems and questions
Text only (Phase 1 questions plus the October questions that still apply): locked account (5 attempts, 15 minutes), temporary password expired (72 hours), no SMS code, the code was refused (details changed after sending, 5 minutes), a menu item is missing (role, capability, organization not verified), a value outside its limit (confirm dialog), a lot that does not appear in New batch, a finalized batch cannot be changed, file too large or wrong type, seal numbers must be different.

## Glossary {-}
Terms of the interface and of the conceptual chapters: batch, lot, CCP, control sheet, composite sample, counter-sample, four-eyes, Honey Story, business number, registry number (AKS), seal, finalize, capability, role, organization, consent.

## Left out (and why)
- Pages for development and demonstrations only: SMS outbox, the phone page, the demo inbox.
- Pages that exist only with the organization model switched off (the v1.0 data-entry menu).
- Single control point pages are mentioned but not shown (the control sheets cover the daily work).
- Field inspector: same pages as Staff, no task of its own in the default responsibilities; named in the role table only.
