# Blackbird.io Appname

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Salesforce Marketing Cloud is a digital marketing platform that helps businesses automate and manage their marketing efforts across various channels, such as email, social media and mobile apps. It allows organizations to deliver personalized messages and analyze customer interactions to improve marketing strategies.

## Before setting up

Before you can connect, you need to have an active Salesforce Marketing Cloud account.

Follow [this guide](https://docs.informatica.com/integration-cloud/data-integration-connectors/current-version/salesforce-marketing-cloud-connector/introduction-to-salesforce-marketing-cloud-connector/administration-of-salesforce-marketing-cloud-connector/retrieving-the-client-id-and-client-secret.html) to create an installed package. 
to create an **Installed Package**.

While creating the package, make sure the following permissions are enabled:
- Assets (Read, Write)
- Saved Content (Read, Write)

Once the package is created, copy the following values:
- `Client ID`
- `Client Secret`
- Domain name (only the subdomain part). Example: https://mc-123abc456def.auth.marketingcloudapis.com/. Your subdomain is `mc-123abc456def`.

To retrieve the `Member ID`, click your username in the top-right corner of the navigation bar and copy the MID value.

![MID](image/README/mid.png)

## Connecting

1. Name your connection for future reference e.g. 'My organization'.
2. Paste the credentials you obtained earlier.
3. Click **Authorize connection** and wait for the process to complete.
4. Confirm that the connection has appeared and its status is **Connected**.

![connecting](image/README/connecting.png)

## Actions

### Content

- **Search content**. Search content (emails and content blocks) with specific criteria.
- **Get email details**. Get details of a specific email.
- **Create content block**. Create a new content block.
- **Download email**. Download email content.
- **Create new email**. Create new email from file.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
