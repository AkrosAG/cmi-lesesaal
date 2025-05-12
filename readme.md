# cmi-lesesaal-backend

- [cmi-lesesaal](https://github.com/AkrosAG/cmi-lesesaal)
  - [cmi-lesesaal-web-core](/CMI/Web.Clients/web-core)
  - [cmi-lesesaal-web-frontend](/CMI/Web.Clients/web-frontend)
  - [cmi-lesesaal-web-management](/CMI/Web.Clients/web-management)
  - **[cmi-lesesaal-backend](https://github.com/AkrosAG/cmi-lesesaal)** :triangular_flag_on_post:

# Context

The [lesesaal](https://github.com/AkrosAG/cmi-lesesaal) project includes 1 code repositories with 4 different Projects. This current project `cmi-lesesaal-backend` is the backend for order management, consultation requests, administrative access and other settings. It was developed using C#. It includes several services and two API's.
The other projects include the applications _public access_ ([cmi-lesesaal-web-frontend](/CMI/Web/CMI.Web.Frontend)) and the _internal management_ ([cmi-lesesaal-web-management](/CMI/Web.Clients/web-management));  both are Angular applications that access basic services of another Angular library called ([cmi-lesesaal-web-core](/CMI/Web.Clients/web-core)).

![The Big-Picture](docs/imgs/context.svg)

> Note: A general description of the repositories can be found in the repository [cmi-lesesaal](https://github.com/AkrosAG/cmi-lesesaal).

# Table of contents

- [Requirements / Limitations](docs/requirements.md)
- [Architecture and components of the solution](docs/architecture.md)
- [Connection to AIS](docs/connection-ais.md)
- [Connection to Digital Repository](docs/connection-dir.md)
- [Email generation](docs/dataBuilder.md)

# Authors

- [CM Informatik AG](https://cmiag.ch)
- [Evelix GmbH](https://evelix.ch)
- [Akros AG](https://www.akros.ch)

# License

GNU Affero General Public License (AGPLv3), see [LICENSE](LICENSE.TXT)

# Contribute

Pull requests and merge on these repositories is restricted. However, independent copies (forks) are possible under consideration of the AGPLV3 license.

# Contact

- Technical questions or problems concerning the source code can be posted here on GitHub via the "Issues" interface.
