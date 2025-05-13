# Connection to the Archive Information System (AIS)

The **CMI CDWS** is metadata delivery system (**AIS**) from CMI and can be replaced. This system is designed to create a generalized representation of a unit of description (UoD) record from **CMI CDWS** data and use it in the application. 

> **Note**  
> The current implementation can be altered for **scopeArchiv AIS** system as well.This can be achived by enabling connection to a scopeArchiv database via configuration file adjustments without requiring changes to the code.

>Since scopeArchiv is a commercial product, SQL statements used to access the database are not included in the source code due to legal restrictions. Placeholders for all SQL statements can be found in the file `SqlStatements.cs` within the project `CMI.Access.Harvest`.

---

## Connection to AIS 

### Elasticsearch Configuration
- The Elasticsearch server and index must be customized based on the scopeArchiv database configuration.
- Define:
  - Which fields should be stored.
  - How the fields should be stored.
  - Which fields should be copied into the "all" field for searchability.

- Configurations are stored in the file `ElasticRecordMapping.json` and must be updated **manually**.
- The index must also be created manually in Elasticsearch before synchronization is started.

### Definition of Fields to be Transferred
- Specify the fields to be included in synchronization in the file `customFieldsConfig.json`.  
- Fields not listed in this file will be excluded from the synchronization process.

### Display Forms
- The web application uses display forms for presenting archival unit details.
- By default, it employs the same form definitions as those in scopeArchiv or **CMI CDWS**, but these are not queried in real time.
- Definitions are stored in the file `templates.json`.

To generate `templates.json`, use the console application `CMI.Utilities.FormTemplate.Helper`.  
Alternatively, independent form definitions can be created, allowing specific fields to be hidden.  
The form used by a record is determined by the `FormId` field.

---

## Connection to Other AIS Systems

### Implementation of Interfaces
To connect to a different **AIS** system, implement the interfaces in the project `CMI.Contract.Harvest` within the `CMI.Access.Harvest` data access component:

- `IDbMetadataAccess`
- `IDbDigitizationOrderAccess`
- `IDbTestAccess`

Additional interfaces may be needed:
- `IDbResyncAccess`
- `IDbMutationQueueAccess`
- `IDbStatusAccess`

The latter interfaces are specific to **scopeArchiv or  CMI CDWS’s mutation reporting**.  
For other **AIS** systems, the implementation can vary, or these interfaces may not be required at all.

The returned object for a UoD must adhere to the schema defined in `ViaducDataSchema.xsd`.  
This schema outlines the `ArchiveRecord` data structure, which serves as a universally valid standard.  
Once a **AIS** system provides this structure for archival records, synchronization is complete.

### Elasticsearch Configuration
- Elasticsearch index configuration is identical to that for **scopeArchiv or CMI CDWS**:
  - Customize the Elasticsearch server and index based on the **AIS** configuration.
  - For more details, refer to the [Elasticsearch Configuration](#elasticsearch-configuration) section under scopeArchiv or CMIDWS **AIS**.

### Display Forms
- Display forms for another **AIS** system must also be specified in the `templates.json` file.
- These forms are defined using the `FormId` fields and the `externalDisplayTemplateName` field of the `ArchiveRecord`.
- If the **AIS** system lacks a direct `FormId`, you must define one and encode it into the metadata accordingly.

For more details, refer to the [Display Forms](#display-forms) section under scopeArchiv or  **CMI CDWS**.

---

## Use Case: ETH Libraries and CMI CDWS

ETH Libraries use **CMI CDWS** to manage their archival data.  
The **AIS** serves as the data source for metadata and primary data used in the virtual reading room.

- Data transfer is facilitated via the **AIS interface** connected to the database.
- The specific data to be transferred is defined within **AIS**, and the VLS (virtual reading room system) retrieves the data from **AIS**.

### AIS Indices and Definitions
- Each archive uses a separate **AIS**.
- Separate **AIS** instances are maintained for test and production systems.
- For the three archives (HSA, TMA, MFA), a total of six **CMI CDWS** instances exist.

---

### File References

| File Name                   | Purpose                                                                                      |
|-----------------------------|----------------------------------------------------------------------------------------------|
| `SqlStatements.cs`          | Placeholders for SQL statements used for database access.                                    |
| `ElasticRecordMapping.json` | Configuration for Elasticsearch fields and index structure.                                  |
| `customFieldsConfig.json`   | Specifies which fields should be included during synchronization.                            |
| `templates.json`            | Contains form definitions for displaying archival unit details in the web application.       |
| `ViaducDataSchema.xsd`      | Schema defining the `ArchiveRecord` data structure.                                          |
| `CMI.Utilities.FormTemplate.Helper` | Console application to generate the `templates.json` file from form definitions.   |

---

### Additional Notes
1. **Customization Flexibility**: Each archive’s **AIS** configuration is unique, and the system can be customized accordingly.
2. **Manual Steps**: Elasticsearch configurations and form definitions require manual updates.
3. **Extensibility**: The implementation supports connections to other **AIS** systems via custom interfaces.

