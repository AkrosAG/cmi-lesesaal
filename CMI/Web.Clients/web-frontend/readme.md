# Master-Stand
Dies ist der Master-Stand dieses Repositories.
Eine Kopie davon wird regelmässig OpenSource publiziert.
Das zugehörige ReadMe befindet sich hier: 
[OpenSource-Readme](os_readme.md)

## Anleitung zur OpenSource-Veröffentlichung
Vorbereitung:
- GitHub Personal Access Token erstellen (Repository-Rechte) unter https://github.com/settings/tokens

Durchführung
- Powershell öffnen
- Ins Verzeichnis /scripts/ navigieren
- Befehl ausführen: `.\export-opensource.ps1 -ProjectName cmi-viaduc-web-core -GitPesonalAccessToken xxx -WorkingDirectory C:\Temp` 

#### Pre-Requirements
client-build:
- NodeJS (https://nodejs.org/en/)
- Angular-CLI (see chapter "Get ready")
Also recommended:
- Git-Bash (https://git-scm.com/)
- SourceTree (git-client)

#### Get ready
##### Angular Projects
- Make sure you uninstalled any old version of angular/cli:
	* npm uninstall angular-cli (old version)
	* npm uninstall @angular/cli (potential old version)
	* npm cache clean --force
- Install angular/cli with "npm install -g @angular/cli"
- Clone cmi-viaduc-web-core (into a root like C:\Viaduc)
- Clone cmi-viaduc-web-frontend (same root, like C:\Viaduc)
- Change directory to cmi-viaduc-web-core
	* Run "npm i" to install the dependencies
	* Run "npm run build" to build the library
- Change directory to cmi-viaduc-web-frontend
	* Run "npm run link" to link cmi-viaduc-web-core (*)
	* Run "npm i" to install all the dependencies
	* Run "npm run build" to build the project into 'dist' folder of cmi-viaduc-web-frontend
(* cmi-viaduc-web-core)
Dieses Repo hängt von einem anderen Projekt ab, welches auf MyGet verfügbar ist (CMI-interner Nuget/Npm Feed).
Es ist möglich das `cmi-viaduc-web-core` Repo auch lokal einzubinden. Dazu muss das Core-Projekt gebuildet werden `npm run build` und anschliessend in diesem Projekt (cmi-viaduc-web-frontend) verlinkt werden (`npm run link`).
Um das Package aus dem CMI-Internen Feed zu konsumieren wird ein MyGet-Account benötigt, welcher auf den cmiag-nuget Feed berechtigt wird.
Danach muss via NPM folgende Befehle abgesetzt werden:
- `npm login --registry https://www.myget.org/F/cmiag-nuget/npm/ --scope=@cmi`
- Es folgt die Eingabe des MyGet Benutzername
- Anschliessend muss der API-Key [hier zu finden](https://www.myget.org/feed/Details/cmiag-nuget) eingegeben werden
- Schlussendlich noch die geschäftliche E-Mailadresse
anschliessend kann das Package mittels `npm i` aus MyGet installiert werden.

##### C# Projects
1. Clone the current masterbranch with your Git-client to a local folder (ex. C:\Viaduc)
2. Open cmd as Administrator, change the directory to your local "Viaduc" Clone.
Type the following:
	* cd "CMI\Web\CMI.Web.Frontend" 
	* mklink /J client "{INSERT-PATH-TO-cmi-viaduc-web-frontend}\dist"
3. Open the Viaduc Solution in Visual Studio as Administrator.
	* Check the "Web" Settings in the Projectproperties of "CMI.Web.Frontend"
	* Servers should be set to IIS Express, and the URL should be http://localhost/viaduc
	* Click on "Create Virtual Directory"
4. Make sure the password for your local SQL Server is set to metatool$15
	* Create an empty database called 'Viaduc'
5. Authentifizierung
	* Installier das stubidp.sustainsys.com Zertifikat und den zugehörigen Private-Key (ask a team member)
	* Zertifikat installieren 
		* "Lokaler Computer", "Weiter"
		* Wähle "Eigene Zertifikate" bzw. "Personal" als Zertifikatsspeicher, "Weiter"
		* "Fertig stellen"
	* Private-Key installieren
		* "Lokaler Computer", "Weiter"
		* "Weiter"
		* Kein Passwort eingeben, "Weiter"
		* Wähle "Eigene Zertifikate" bzw. "Personal" als Zertifikatsspeicher, "Weiter"
		* "Fertig stellen"
    * Vom Sharepoint die Datei 'Credentials for develop.json' holen und ein Verzeichnis oberhalb der Solution Datei ablegen.
6. RabbitMQ
    * Installation des rabbitmq servers
	* Login auf http://localhost:15672/ mit user und passwort guest
    * Den Benutzer viaduc mit Passwort 123 hinzufügen und Recht auf den "Virtual Host" viaduc.
7.  Run the following projects 
    1. CMI.Host.Parameter
	2. CMI.Host.Viaduc
	3. CMI.Host.Asset
	4. CMI.Host.Cache
	5. CMI.Host.DocumentConverter
	6. CMI.Host.ExternalContent
	7. CMI.Host.Repository
	8. CMI.Host.Notification
	9. CMI.Host.Order	
	10. CMI.Web.Frontend
	11. CMI.Web.Management
    
##### Things to know
Start Build with Filewatcher
	* npm run start
	
Start Build without Filewatcher
	* npm run build
	
Start Prod Build (used in Teamcity)
	* npm run build-prod
	
Run Tests (as Server)
	* npm run test
	
Run Tests as Teamcity would do
	* npm run test-teamcity
	
Run Lint Checks 
	* npm run lint
	
Add components/modules/directives/etc via angular-cli
	* see https://github.com/angular/angular-cli/wiki/generate