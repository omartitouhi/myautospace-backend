# Guide CI/CD backend MyAutoSpace

Ce repository contient une pipeline GitHub Actions pour valider le backend .NET et les images Docker principales avant integration.

## Fichier de pipeline

Le workflow est defini dans :

```text
.github/workflows/backend-ci.yml
```

## Quand la pipeline s'execute

La pipeline se lance automatiquement dans les cas suivants :

- `push` vers `master`
- `push` vers `main`
- `push` vers `develop`
- `pull_request` vers `master`
- `pull_request` vers `main`
- `pull_request` vers `develop`

Elle ne pousse aucune image Docker pour le moment. Elle fait uniquement de la validation CI.

## Jobs de la pipeline

### Restore, build and test .NET solution

Ce job valide la solution backend :

1. Checkout du repository.
2. Installation du SDK .NET `10.0.x`.
3. Cache des packages NuGet dans `~/.nuget/packages`.
4. `dotnet restore` sur `myautospace-backend.sln`.
5. `dotnet build` en configuration `Release`.
6. Recherche de projets de test `*Tests.csproj` ou `*Test.csproj`.
7. Execution de `dotnet test` uniquement si au moins un projet de test existe.

Si aucun projet de test n'existe, l'etape de tests est ignoree proprement et la pipeline continue.

### Validate Docker builds and compose file

Ce job valide la partie Docker :

1. Checkout du repository.
2. Validation de `docker-compose.yml` avec `docker compose config`.
3. Build local des images principales :
   - `ApiGateway`
   - `AuthService`
   - `UserService` si son Dockerfile est present

Les images sont taguees localement avec le suffixe `:ci` et ne sont pas publiees.

## Comment lire les erreurs

Dans GitHub, ouvrir l'onglet **Actions**, choisir le workflow **Backend CI**, puis ouvrir le run en erreur.

- Erreur dans **Restore solution** : verifier les references de projets, les versions de packages NuGet ou la disponibilite du SDK.
- Erreur dans **Build solution** : verifier les erreurs C# affichees par `dotnet build`, souvent avec le fichier et la ligne concernes.
- Erreur dans **Run unit tests when present** : ouvrir la sortie du projet de test indique juste avant l'echec.
- Erreur dans **Verify docker compose config** : verifier la syntaxe, les noms de services, les variables ou les sections du fichier `docker-compose.yml`.
- Erreur dans **Build main Docker images** : verifier le Dockerfile du service concerne, les chemins `COPY`, le restore ou le publish dans l'image.

## Ajouter des tests plus tard

Creer un projet de test dont le nom se termine par `Tests` ou `Test`, par exemple :

```powershell
dotnet new xunit -n AuthService.Tests
dotnet sln myautospace-backend.sln add AuthService.Tests/AuthService.Tests.csproj
dotnet add AuthService.Tests/AuthService.Tests.csproj reference AuthService/AuthService.csproj
```

La pipeline detectera automatiquement les projets :

```text
*Tests.csproj
*Test.csproj
```

Aucune modification du workflow n'est necessaire si les projets suivent cette convention.

## Ajouter un push Docker Hub plus tard

Pour publier les images Docker, ajouter une etape de login apres le checkout du job Docker :

```yaml
- name: Login to Docker Hub
  uses: docker/login-action@v3
  with:
    username: ${{ secrets.DOCKERHUB_USERNAME }}
    password: ${{ secrets.DOCKERHUB_TOKEN }}
```

Puis remplacer ou completer les builds locaux avec des commandes `docker build` taguees pour Docker Hub :

```bash
docker build --file ApiGateway/Dockerfile --tag dockerhub-user/myautospace-apigateway:latest .
docker push dockerhub-user/myautospace-apigateway:latest
```

Il faudra d'abord creer les secrets GitHub suivants dans **Settings > Secrets and variables > Actions** :

- `DOCKERHUB_USERNAME`
- `DOCKERHUB_TOKEN`

## Commandes locales equivalentes

Depuis la racine du repository :

```powershell
dotnet restore myautospace-backend.sln
dotnet build myautospace-backend.sln --configuration Release --no-restore
```

Pour lancer les tests si des projets existent :

```powershell
Get-ChildItem -Recurse -Filter "*Tests.csproj" | ForEach-Object {
  dotnet test $_.FullName --configuration Release --no-restore
}
Get-ChildItem -Recurse -Filter "*Test.csproj" | ForEach-Object {
  dotnet test $_.FullName --configuration Release --no-restore
}
```

Pour valider Docker :

```powershell
docker compose -f docker-compose.yml config
docker build --file ApiGateway/Dockerfile --tag myautospace-apigateway:ci .
docker build --file AuthService/Dockerfile --tag myautospace-authservice:ci .
docker build --file UserService/Dockerfile --tag myautospace-userservice:ci .
```
