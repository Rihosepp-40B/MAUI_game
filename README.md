# MAUI_game

Tegu on torni kaitse mänguga (Tower Defence).
Mängu käivitamisel luuakse kaart millel on juhusliku valikuga teekond (path). Üleval ribal näeb mängija oma elusid, kulda, skoori ning vastase rünnaklaine tuleku aega ning taset.

Mängijal on tornide menüü, kust saab valida torne. Üks klõks valib topelt klõks avab info akna torni detailidega.

Tornide ehitamiseks vajutab, hojab ja lohistab kaardil ning lahtilaskmisel luuakse torn asukohta. Lohistamise hetkel kuvatakse torni laskeulatust. Tornid maksavad raha, kui pole piisavalt kulda, siis torni ei looda. Sammuti ei saa torni luua väljapoole mängu ala, teise torni peale ega teele.

Vastas elaine alustab rünnakut kui Järgmise laine aeg jõuab nulli. Laine aeg seatakse jälle ettenähtud ajale, kui viimane vastane on tulnud platsile. Iga tase lisab teenitava kulla hulka 100 võrra. Vastast tekitakse väljale, et nende hävitamisel on võimalik teenida taseme jagu kulda. Vastase grupid laines tekivad ajalise intervalli järgi, kuigi iga laine tasemel korraga tuleb platsile tasemele võrdne arv vastat.

Alates 5 tasemest muutub vastane kaootiliseks. Tuuakse välja juhuvaliku alusel erinevad vastased (erinevad, suuruse, kiiruse ja tugevuse poolest).

Mäng saab läbi kui mängija on kaotanud kõik oma elud.

Mängul on pausi võimalus ning SQLite andembaas andmete hoidmiseks.


Kasutatud tehnoloogiad:

Visual Studio – Programmeerimis platvorm;
Microsoft.Maui.Controls - .NET MAUI rakenuste kasutajaliidesed (Button, Label, Entry, Grid jne);
Microsoft.NET.ILLink.Tasks – Rakendus optimeerimine (eemaldab kasutamata koodi);
Plugin.Maui.Audio – Heli esitamiseks .NET MAUI rakenduses;
Slite-net-pcl – SQLite andmebaasi (lokaalne) töötlemine;
C# - rakenduse loogikakeel
GitHub – projekti ja lähtekoodi säilitamine ja versiooni haldus.


Teemad ja põhimõtted:

MVVP,
OOP,
Andmebaas (SQLite),
Lokaliseerimine,
Teema vahetus,
Helid (torni tuli, vastase surm, plahvatused),
Drag $ Drop, Tap,
Seadete salvestamine (Preference)
