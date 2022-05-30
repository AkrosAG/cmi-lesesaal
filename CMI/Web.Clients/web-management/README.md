# cmi-viaduc-web-management
Dies ist der Management-Client für den Onlinezugang des Bundesarchivs

# cmi-viaduc-web-core (NPM-Package)
Dieses Repo hängt von einem anderen Projekt ab, welches auf MyGet verfügbar ist (CMI-interner Nuget/Npm Feed).
Es ist möglich das `cmi-viaduc-web-core` Repo auch lokal einzubinden. Dazu muss das Core-Projekt gebuildet werden `npm run build` und anschliessend in diesem Projekt (cmi-viaduc-web-management) verlinkt werden (`npm run link`).

Um das Package aus dem CMI-Internen Feed zu konsumieren wird ein MyGet-Account benötigt, welcher auf den cmiag-nuget Feed berechtigt wird.
Danach muss via NPM folgende Befehle abgesetzt werden:
- `npm login --registry https://www.myget.org/F/cmiag-nuget/npm/ --scope=@cmi`
- Es folgt die Eingabe des MyGet Benutzername
- Anschliessend muss der API-Key [hier zu finden](https://www.myget.org/feed/Details/cmiag-nuget) eingegeben werden
- Schlussendlich noch die geschäftliche E-Mailadresse

anschliessend kann das Package mittels `npm i` aus MyGet installiert werden.