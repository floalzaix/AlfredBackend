# FOY-01 — modèle foyer aligné

Le modèle suit `Modele_Foyer_Alfred.pdf` : `Household`, `HouseholdMember`, `HouseholdRole` (`Admin`, `Member`).

- `Household.Name`: requis, 100 caractères maximum.
- `Household.InvitationCode`: nullable, 16 caractères maximum, index unique.
- `Household.CreatedAt`: UTC.
- `HouseholdMember`: clé primaire composite `(HouseholdId, UserId)`.
- Les clés étrangères vers `Household` et `User` utilisent `ON DELETE CASCADE`.
- `HouseholdMember.Role` est persisté sous forme de texte.
- `POST /api/households`: le créateur est inséré comme `Admin` dans la même transaction.

Migration `20261124093000_AlignHouseholdModel` : renomme les tables Home historiques et applique le modèle officiel sans perdre les memberships existants.
