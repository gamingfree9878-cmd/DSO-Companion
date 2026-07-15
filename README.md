# DSO Companion

## Version 0.6.1 – Web-Design Absturz behoben

Behoben:

- Absturz beim Öffnen des Edelstein-Reiters
- `TotalDust` und `TotalGold` werden jetzt korrekt als OneWay-Bindings angezeigt
- Web-Design der Edelsteinseite bleibt unverändert
- Plus/Minus, Filter, Summen und Speicherung bleiben erhalten

Ursache:

Die berechneten Eigenschaften `TotalDust` und `TotalGold` sind schreibgeschützt.
WPF hatte versucht, diese wie bearbeitbare Werte zu behandeln.
