# Master-Stand
Dies ist der Master-Stand dieses Repositories.
Eine Kopie davon wird regelmässig OpenSource publiziert.
Das zugehörige ReadMe befindet sich hier: 
[OpenSource-Readme](os_readme.md)

## Anleitung zur OpenSource-Veröffentlichung
Siehe:
https://cminformatik.atlassian.net/wiki/spaces/VIAD/pages/2771812482/Sourcecode+Opensource+publizieren

# NPM-Package
Dieses Repo wird als NPM-Package publiziert und durch cmi-viaduc-web-frontend und cmi-viaduc-web-management eingebunden.
Es ist möglich dieses Repo auch lokal einzubinden. Dazu muss dieses Projekt gebuildet werden `npm run build` und anschliessend im Zielprojekt (z.B. cmi-viaduc-web-frontend) verlinkt werden (`npm run link`).

Das NPM Package wird durch Teamcity automatisch erzeugt und landet anschliessend in der MyGet-Registry (https://www.myget.org/feed/cmiag-nuget/package/npm/@cmi/viaduc-web-core)
- Builds ab einem PR ergeben ein x.x.x-beta NPM-Package
- Builds ab develop oder master ergeben ein x.x.x.x NPM-Package
- Die ersten beiden Versionsnummern in `version.json` müssen gemäss Semantic-Versioning von Hand angepasst werden (Breaking Change -> Neue Major-Version)

Um das Package aus dem CMI-Internen Feed zu konsumieren wird ein MyGet-Account benötigt, welcher auf den cmiag-nuget Feed berechtigt wird.
Danach muss via NPM folgende Befehle abgesetzt werden:
- `npm login --registry https://www.myget.org/F/cmiag-nuget/npm/ --scope=@cmi`
- Es folgt die Eingabe des MyGet Benutzername
- Anschliessend muss der API-Key [hier zu finden](https://www.myget.org/feed/Details/cmiag-nuget) eingegeben werden
- Schlussendlich noch die geschäftliche E-Mailadresse

anschliessend kann das Package mittels `npm i` aus MyGet installiert werden.

# Gitflow & Reviews
Wir benutzen den Gitflow (CMI-Version)
Siehe https://cminformatik.atlassian.net/wiki/spaces/EN/pages/234553356/Git-Workflow+Guideline

Jegliche Änderungen werden nur in separaten Featurebranches via PullRequest und mind. einem "approved" Review gemerged.
Info's wie wir das Review durchführen siehe: 
https://cminformatik.atlassian.net/wiki/spaces/EN/pages/234553356/Git-Workflow+Guideline#Git-WorkflowGuideline-Code-Review

# Guidelines
- Es wird nichts committet, was nicht buildet (auch TSLint-Regeln)
- Anzahl Codezeilen sind vereinbar mit der Komplexität des geschriebenen Codes
- Businesslogik gehört in Services
- private Methoden und Properties und Fields müssen mit einem Underscore ('_' prefixt werden)
- AngularJS / Typescriptfeatures wie truthy/falsy sind zu gebraucehn, um "unnötige singleline" Methoden zu vermeiden
- Alle arbeiten mit demselben Indent (Tabs), die Webstorm Config dafür kann bei jedem Mitarbeiter verlangt werden
- Über Webstorm -> Code -> Reformat Code stellt man sicher, dass bestehender Code die richtigen Indents haben
- neue Pages (über's Routing "Routes.ts" erreichbare Components) gehören in /app/component, restliche componenten gehören in /client/components/
- Components-CSS dürfen nur Styles enthalten, die die jenige Component betrifft und mit einem Namespace geprifxt werden (z.B. tree-node-link)
- Services die im Konstruktor injected werden und ausserhalb vom Konstruktor verwendet werden, werden mit "private" direkt zu einem Field gemacht
- ToDo's müssen als "// ToDo: {Bemerkung} " ausgewiesen sein und spezifizieren was noch offen ist, 
z.B. // ToDo: Favoriten Service anbinden
- Auskommentierter Code muss beim Merge in den develop entfernt werden
- Nicht mehr benötigte console.logs dürfen nicht in den develop gemerged werden
- Aus laufender Erfahrung können weitere Punkte hinzukommen

# UX
- Mockup anhand Bundes CI/CD Guinelines (https://swiss.github.io/styleguide/de/) Elementen umsetzen
- Flex Elemente können abgeändert werden
- Wenn Fixe Elemente abgeändert werden, muss der Kunde im Ticket entsprechend informiert werden

## Icons
- Erste Anlaufstelle:
https://swiss.github.io/styleguide/de/general.html#02-general-08-icons
- Zweite Anlaufstelle:
https://getbootstrap.com/docs/3.3/components/ (Glyphicons)

# Best-Pracites
- Bestehende Elemente verwenden (Fragen, ob etwas bereits da ist)

# CI / Automated Testing
- In den Angular Repositories wird bei jedem Pullrequest ein CI Build durch Teamcity ausgelöst. Er führt einen provisorischen Merge mit dem develop Branch durch und meldet das Resultat in Github zurück (Sichtbar im Pullrequest).

# Testing
- Fokus auf Logik in Services

## End of Sprint Process 
Voraussetzung:
- Alle Featurebranches welche im Sprint enthalten sein müssen, müssen gemerged sein (oder sonst muss bewusst Entschieden werden, ob das Feature weggelassen werden kann)

Ablauf:
- Kontrollieren, ob alle benötigtigen Featurebranches gemerged wurden
- Develop Branch in den Master mergen
- Gemergde Feature Branches (ausgehend vom Develop) löschen
- Teamcity Releasebuild erstellen
- Develop im Github Tag / Release ziehen
- Mitarbeiter die noch pendente Feature Branches haben, müssen den neuen Sprintbranch in ihren Branch ziehen um Merge Konflikte zu verhindern

