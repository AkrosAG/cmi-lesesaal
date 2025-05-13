# Requirements

Digitaler Lesesaal is a software system that consists of various services and applications and is in turn integrated into various surrounding systems.

## Development environment

It is recommended to use Visual Studio 2019. The following components to be licensed are used:

* Aspose Total for .NET (Imaging and PDF are used).

**Important information:** Without the licenses for these components, the solutions won't run successfully

## Services and components used

| Name | Description |
| --- | --- |
| [RabbitMq](https://www.rabbitmq.com/) | RabbitMq is an open source message bus. All services of the solution communicate via the message bus. |
| [Elastic Search](https://www.elastic.co/) | For full-text search and storage of data from the AIS/DIR |
| [MS SQL Server](https://www.microsoft.com/de-ch/sql-server/sql-server-downloads) | For the storage of users, orders, insight requests and other data. |
| [Aspose](https://metrics.aspose.com/) | OCR for PDF's with text layer |
| Mail Server | For sending e-mails using an SMTP mail server.    |


MS SQL Server and Aspose are commercial products. For the licensing of these products, contact the respective manufacturer.

## External Peripheral Systems

| Name | Description |
| --- | --- |
| CMI CDWS | The descriptive metadata of archival records are managed through the **CMI CDWS** system.<br/>ETH Zürich uses CMI CDWS as a central integration point for metadata and primary data management. Each institution’s configuration can vary depending on specific needs.<br/>If the system is to be connected to another Archival Information Systems instance, this can be achieved by adjusting the configuration files.<br/>**Note:** Since CMI CDWS is a commercial product, SQL commands for accessing data have been removed and replaced by placeholders to comply with legal requirements.<br/><br/>**Connection to Other Systems:** The code is abstracted to facilitate integration with different metadata systems. |
| Rosetta |The primary data is preserved and managed in **Rosetta**, ETH Zürich’s digital repository system.<br/><br/>Rosetta enables various content providers to deposit content and make it available to library staff, the public, or to specific content consumers.To ensure the preservation of only high-quality, appropriate content, Rosetta provides various tools for managing content and content providers.<br/> Libraries have the option of storing material indefinitely or for a specified amount of time and then deleting it (tentatively or permanently).<br/><br/>Access to Rosetta is achieved through its **RESTful API**, enabling flexible integration with other systems.<br/>The code for creating working copies may require custom adjustments to align with ETH Zürich’s specific workflows. |
| Switch edu-ID | User identification for public users and internal management clients is handled via **Switch edu-ID**.<br/><br/>**Switch edu-ID** is the personal digital identity of all university members and other users.<br/> It can be used universally at all universities and beyond, for example at swisscovery remains valid indefinitely and supports lifelong learning.Data remain in Switzerland and are subject to Swiss data protection.It was developed in collaboration with the Swiss universities and the support of swissuniversities <br/> |

# Limitations

As already mentioned in the introduction, it is not possible to use the code 1:1 and compile it directly after forking. The code **MUST** be adapted to your own environment and systems. This includes (not exhaustive):

* Connection to your own AIS. Even if _scopeArchiv_ is used, various configurations must be adapted. E.g., the file `customFieldsConfig.json` and `templates.json`. For the generation of the file `templates.json` the console application `CMI.Utilities.FormTemplate.Helper` can be found within the repository.ETH uses **CMI CDWS**
* Connection to your own digital repository. We use metadata standard/schema called *Arelda*. The access, like the internal structure of the DIP has to be adapted according to your requirements.
* The Asset Manager service handles the conversion of the data exported from the Rosetta into a working copy (DIP). Depending on the metadata standard and requirements, the conversion process must be adapted.
* In some cases, commercial products are used. These must either be replaced, or also licensed. The license keys required for these products have been moved to configuration files and replaced by dummy entries.
  * Aspose (https://purchase.aspose.com/pricing/total/net)
