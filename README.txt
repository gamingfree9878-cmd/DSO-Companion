DSO COMPANION – WPF VERSION 0.1

DIES IST DER SAUBERE NEUSTART DES WINDOWS-PROGRAMMS.

Enthalten:
- mehrere Charakterprofile
- mehrere Builds pro Charakter
- vollständige Ausrüstungsslots
- Itemname, Gegenstandsstufe, Basiswerte, Verzauberungen
- Edelsteine, Runen, Juwel und Notizen je Ausrüstungsteil
- Build kopieren, umbenennen, löschen
- Build als .dsobuild exportieren und importieren
- automatische lokale Speicherung
- Mortis/BGH-Rechner mit:
  Mortis Infernal: 25
  Mortis Gnadenlos: 35
  Mortis Blutvergießen: 35
  Inferno Infernal: 40
  Inferno Gnadenlos: 50
  Inferno Blutvergießen: 60
  Wächter: 5
  Wächtergruppe: 25

START:
1. .NET 8 SDK auf dem PC installieren.
2. BUILD_AND_RUN.bat doppelklicken.

PORTABLE EXE ERSTELLEN:
PUBLISH_PORTABLE.bat doppelklicken.
Die fertige EXE liegt anschließend im angezeigten publish-Ordner.

SPEICHERORT:
%APPDATA%\DSO Companion\state.json

BUILDS TEILEN:
- Build auswählen
- Exportieren
- .dsobuild-Datei an einen Freund schicken
- Freund nutzt Importieren

NÄCHSTE VERSIONEN:
0.2 Edelstein- und Runenmodule
0.3 Juwelenkatalog
0.4 Itemdatenbank und automatische Buildwerte
0.5 vollständiger Mortisplaner mit Empfehlungen


GITHUB-AUTOMATISIERUNG
Dieses Projekt enthält unter .github/workflows zwei automatische Abläufe:

1. build-windows.yml
   Erstellt nach jedem Upload automatisch eine portable Windows-EXE.
   Der Download befindet sich im erfolgreichen Actions-Lauf unter „Artifacts“.

2. release.yml
   Erstellt bei einem Versions-Tag wie v0.1 automatisch einen GitHub-Release.

Für die Nutzung über GitHub muss auf dem eigenen PC kein .NET SDK installiert sein.
