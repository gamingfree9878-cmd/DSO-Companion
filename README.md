# DSO Companion 1.4.1 – Varnok Crash-Fix

Diese Version behebt den Absturz beim Öffnen des Varnok-Reiters.

## Änderung

Beim erstmaligen Anzeigen der Varnok-Karten wurden alle Textfelder gleichzeitig
initialisiert. Jedes Feld hat dabei sofort eine Neuberechnung und Speicherung
ausgelöst.

Jetzt gilt:

- Beim Öffnen werden keine künstlichen Eingaben verarbeitet.
- Nur ein Feld mit aktivem Tastaturfokus verarbeitet Textänderungen.
- Speichern erfolgt nur nach echter Eingabe oder über Plus/Minus.
- Fehlende alte Varnok-Daten werden automatisch repariert.
- Ein unerwarteter Fehler zeigt eine Meldung, statt das ganze Programm zu schließen.

Alle Funktionen aus Version 1.4 bleiben erhalten.
