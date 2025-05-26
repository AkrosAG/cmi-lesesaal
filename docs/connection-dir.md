# Connection to the Rosetta

The commercial system **Rosetta** is used at **ETH Zürich** for long-term preservation and management of digital information. This system is custom-tailored to meet ETH Zürich's institutional needs.  
Large parts of the provided codebase are specifically designed for Rosetta’s configuration and cannot be directly reused with other repositories.

### Mandatory Adjustments
The following projects must be adapted for integration with Rosetta:
- `CMI.Access.Repository`
- `CMI.Engine.PackageMetadata`
- `CMI.Manager.Repository`

---

### Repository Access

Access to the repository is achieved through the **Rosetta API interface**.  

If the repository you are using provides an API interface similar to Rosetta, the existing code can be adapted with changes in specific areas. The most significant modifications will likely be required in the `CMI.Engine.PackageMetadata` project, where metadata is read and processed according to the **Arelda standard**.Rosetta provides **METS Standard** which was altered to **Arelda standard**  for internal use.

If the repository used does not provide an API compatible with Rosetta, the existing projects can be used as examples. Similar actions must be implemented in your codebase to replicate the required functionality.  
**Important**: Ensure that the same messages are sent via the message bus at the end of the process to maintain system compatibility.
