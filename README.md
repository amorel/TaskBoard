# Task Board - Tableau de bord collaboratif en temps réel

# Présentation générale

Ce projet est une application Blazor Server qui implémente un tableau kanban collaboratif en temps réel, permettant à plusieurs utilisateurs de gérer des tâches simultanément avec des mises à jour instantanées. L'application est hébergée sur Azure et bénéficie d'un déploiement continu via GitHub Actions.

Pour expérimenter la fonctionnalité de collaboration en temps réel, ouvrez l'application dans deux navigateurs différents ou dans deux fenêtres en mode privé du même navigateur - vous pourrez ainsi voir les mises à jour instantanées lorsqu'une tâche est créée, modifiée ou déplacée dans un des navigateurs.

Les fonctionnalités sont validées par des tests unitaires traditionnels ainsi que des tests end-to-end automatisés avec Playwright qui simulent les interactions utilisateur réelles, notamment la collaboration en temps réel entre plusieurs navigateurs.

## Technologies utilisées

### Frontend

- **Blazor Server**: Framework permettant une expérience temps réel grâce à SignalR
- **Bootstrap 5**: Framework CSS pour un design responsive
- **JavaScript**: Pour la gestion du drag & drop natif HTML5

### Backend

- **NET 9.0**: La dernière version du framework .NET
- **Entity Framework Core**: ORM pour la gestion de la base de données
- **SQLite**: Base de données légère et embarquée
- **SignalR**: Pour la communication en temps réel

### Tests

- **xUnit**: Framework de tests
- **bUnit**: Pour tester les composants Blazor
- **FluentAssertions**: Pour des assertions plus lisibles
- **Moq**: Pour le mocking dans les tests
- **Playwright**: Pour les tests end-to-end et la simulation d'interactions utilisateur

### Hébergement & CI/CD

- **Azure App Service**: Pour l'hébergement de l'application
- **GitHub Actions**: Pour l'automatisation du déploiement
- **Azure Application Insights**: Pour la télémétrie et le monitoring

## Architecture & Bonnes Pratiques

### Clean Architecture

L'application est structurée en 4 couches distinctes :

1. **Domain**: Contient les entités et interfaces du domaine
2. **Application**: Contient la logique métier (CQRS pattern)
3. **Infrastructure**: Gère la persistance et les services externes
4. **Presentation**: Interface utilisateur Blazor

### Patterns Implémentés

- CQRS: Séparation des opérations de lecture et d'écriture
- Repository Pattern: Abstraction de la couche de données
- MVVM: Pour la séparation des préoccupations dans l'UI
- Dependency Injection: Pour un couplage faible entre les composants

### Tests

- Tests unitaires pour chaque couche
- Tests d'intégration pour la persistance
- Tests de composants UI avec bUnit
- Tests de performance pour les opérations critiques

### Temps Réel

- Utilisation de SignalR pour la communication bidirectionnelle
- Hub dédié pour la gestion des événements temps réel
- Gestion optimisée des connexions et reconnexions

### Sécurité & Performance

- Protection CSRF automatique via Blazor
- Gestion des erreurs centralisée
- Logging structuré avec différents niveaux de détails
- Optimisation des requêtes avec Entity Framework Core

## Déploiement

### Infrastructure Azure

- App Service Plan en région France Central
- Base de données SQLite hébergée dans le système de fichiers de l'App Service
- Application Insights pour le monitoring

### Pipeline CI/CD

Le déploiement est entièrement automatisé via GitHub Actions :

1. Déclenchement sur push vers la branche master
2. Exécution des tests unitaires et d'intégration
3. Build de l'application en mode Release
4. Déploiement vers Azure App Service
5. Validation post-déploiement

## [Task Board - DEMO - Azure](https://taskboard-demo-ghepg6c0d9dqhfex.francecentral-01.azurewebsites.net "Task Board - Tableau de bord collaboratif en temps réel")

## Exécution des tests

### Exécuter tous les tests

```bash
dotnet test
```

### Exécuter les tests par projet

```bash
# Tests du Domain
dotnet test TaskBoard.Domain.Tests/TaskBoard.Domain.Tests.csproj

# Tests de l'Application
dotnet test TaskBoard.Application.Tests/TaskBoard.Application.Tests.csproj

# Tests de l'Infrastructure
dotnet test TaskBoard.Infrastructure.Tests/TaskBoard.Infrastructure.Tests.csproj

# Tests de la couche présentation (Blazor)
dotnet test TaskBoard.BlazorServer.Tests/TaskBoard.BlazorServer.Tests.csproj
```

### Exécuter les tests end-to-end avec Playwright

```bash
# Tests Playwright (nécessite que l'application soit en cours d'exécution)
dotnet test TaskBoard.PlaywrightTests/TaskBoard.PlaywrightTests.csproj
```

## Journal de développement

### Étape 1 - Mise en place de la structure du projet (14/12/2024)

- Création de la solution avec une architecture en couches (Domain, Application, Infrastructure, Presentation)
- Mise en place des dépendances entre les projets
- Choix de la Clean Architecture pour permettre :
  - Une meilleure séparation des responsabilités
  - Une meilleure testabilité
  - Une meilleure maintenabilité
  - Une indépendance des couches externes (UI, Base de données)

### Étape 2 - Création des entités du domaine (14/12/2024)

- Création de l'entité Task avec ses propriétés essentielles
- Utilisation d'un enum pour représenter les statuts des tâches (Todo, InProgress, Done)
- Choix d'un enum plutôt qu'une entité séparée pour :
  - Simplifier l'implémentation initiale
  - Garantir un ensemble fixe de statuts bien définis
  - Faciliter la maintenance et la compréhension du code

### Étape 3 - Définition des interfaces du repository (14/12/2024)

- Création de l'interface ITaskRepository définissant toutes les opérations CRUD
- Ajout des opérations asynchrones suivantes :
  - GetAllAsync : Récupération de toutes les tâches
  - GetByIdAsync : Récupération d'une tâche par son ID
  - AddAsync : Ajout d'une nouvelle tâche
  - UpdateAsync : Mise à jour d'une tâche existante
  - DeleteAsync : Suppression d'une tâche
  - GetByStateAsync : Récupération des tâches par état

### Étape 4 - Mise en place de la couche Application (14/12/2024)

- Implémentation du pattern CQRS pour séparer les opérations de lecture et d'écriture
- Création des interfaces de base pour le CQRS :
  - IQuery : Interface marqueur pour les requêtes
  - IQueryHandler : Interface générique pour les gestionnaires de requêtes
  - ICommandHandler : Interface générique pour les gestionnaires de commandes
- Implémentation de la première Query GetAllTasks :
  - GetAllTasksQuery : Définition de la requête
  - GetAllTasksQueryHandler : Gestionnaire qui utilise le repository pour récupérer les tâches

### Étape 5 - Implémentation des Commands et Queries (14/12/2024)

- Implémentation des Queries (opérations de lecture) :
  - GetTaskById : Récupération d'une tâche spécifique
  - GetTasksByState : Filtrage des tâches par état
- Implémentation des Commands (opérations d'écriture) :
  - CreateTask : Création d'une nouvelle tâche
  - UpdateTask : Mise à jour d'une tâche existante
  - DeleteTask : Suppression d'une tâche
- Ajout de la classe Unit pour gérer les commandes sans retour de valeur
- Gestion des erreurs basique avec InvalidOperationException pour les cas d'erreur simples

### Étape 6 - Configuration de l'Infrastructure avec Entity Framework Core (14/12/2024)

- Mise en place de la persistance avec SQLite
- Configuration d'Entity Framework Core :
  - TaskBoardDbContext : Configuration du contexte de base de données
  - Configuration des entités avec Fluent API
  - Implémentation du TaskRepository
  - Mise en place de l'injection de dépendances
- Choix techniques :
  - Utilisation de SQLite pour sa simplicité et sa portabilité
  - Configuration des contraintes de base de données via Fluent API
  - Implémentation du pattern Repository

### Étape 7 - Configuration des Migrations Entity Framework Core (14/12/2024)

- Configuration de la chaîne de connexion SQLite dans appsettings.json
- Création de la migration initiale pour la table Tasks
- Application de la migration pour créer la base de données
- Intégration de l'infrastructure dans le projet Blazor Server

### Étape 8 - Configuration de la couche Présentation (14/12/2024)

- Mise en place du pattern MVVM :
  - Création des ViewModels pour les tâches
  - Implémentation du service de gestion des tâches
  - Configuration du système de notification pour les mises à jour en temps réel
- Architecture de la couche présentation :

  - Séparation des responsabilités avec les ViewModels
  - Service façade pour simplifier l'interaction avec la couche Application
  - Système d'événements pour les mises à jour en temps réel

  ### Étape 9 - Configuration finale et préparation au lancement (14/12/2024)

- Mise à jour du fichier \_Imports.razor pour inclure tous les namespaces nécessaires :
  - Composants Microsoft AspNetCore
  - Composants personnalisés
  - ViewModels et Services
- Mise à jour de Program.cs :
  - Enregistrement des services d'infrastructure
  - Configuration des handlers CQRS :
    - Handlers de requêtes (GetAllTasks, GetTaskById, GetTasksByState)
    - Handlers de commandes (CreateTask, UpdateTask, DeleteTask)
  - Configuration de Blazor Server et des routes
- Organisation de la structure finale du projet :
  - Séparation claire entre les composants et les pages
  - Configuration des dépendances pour l'injection
  - Préparation de l'environnement de développement

### Étape 10 - Mise en place des tests unitaires (15/12/2024)

- Création de quatre projets de tests unitaires :
  - TaskBoard.Domain.Tests : Tests de la couche domaine
  - TaskBoard.Application.Tests : Tests de la couche application
  - TaskBoard.Infrastructure.Tests : Tests de la couche infrastructure
  - TaskBoard.BlazorServer.Tests : Tests de la couche présentation
- Configuration des dépendances :
  - Ajout des références aux projets principaux
  - Installation des packages de test : xUnit, Moq, FluentAssertions
- Choix techniques :
  - xUnit : Framework de test moderne et flexible
  - Moq : Pour la création de mocks
  - FluentAssertions : Pour des assertions plus lisibles
  - Convention AAA (Arrange-Act-Assert) pour la structure des tests

### Étape 11 - Tests de la couche Domain (15/12/2024)

- Mise en place des tests unitaires pour l'entité BoardTask
- Tests des différents scénarios :
  - Création avec valeurs par défaut
  - Validation des propriétés
  - Gestion des chaînes vides
  - Gestion des identifiants
  - Comportement des dates de création et modification
- Choix techniques :
  - Utilisation de xUnit comme framework de test
  - FluentAssertions pour des assertions plus lisibles et expressives
  - Tests basés sur le comportement attendu de l'entité

![Résultats des tests unitaires pour la couche Domain](/assets/img/TaskBoard.Domain.Tests.png "Résultats des tests unitaires pour la couche Domain")

### Étape 12 - Tests de la couche Application (15/12/2024)

- Implémentation des tests pour tous les handlers CQRS
- Tests des Queries :
  - GetAllTasksQueryHandler
  - GetTaskByIdQueryHandler
  - GetTasksByStateQueryHandler
- Tests des Commands :
  - CreateTaskCommandHandler
  - UpdateTaskCommandHandler
  - DeleteTaskCommandHandler
- Choix techniques :
  - Utilisation de Moq pour simuler les dépendances
  - Tests des cas normaux et des cas d'erreur
  - Vérification des interactions avec le repository
  - Couverture complète des scénarios d'utilisation
- Points clés testés :
  - Validation des données
  - Gestion des erreurs
  - Comportement des dates
  - Intégrité des données lors des opérations CRUD

![Résultats des tests unitaires pour la couche Application](/assets/img/TaskBoard.Application.Tests.png "Résultats des tests unitaires pour la couche Application")

### Étape 13 - Tests de la couche Infrastructure (15/12/2024)

- Mise en place des tests utilisant SQLite en mémoire
- Tests du Repository :
  - Opérations CRUD complètes (Create, Read, Update, Delete)
  - Filtrage des tâches par état
  - Gestion des erreurs
  - Tests de performance
- Tests de la configuration du DbContext :
  - Vérification des configurations des entités
  - Tests des contraintes de la base de données
  - Validation des clés primaires et des index
- Tests des migrations :
  - Vérification de la création de la base de données
  - Validation du schéma de la base de données
  - Tests des migrations automatiques
- Choix techniques :
  - Utilisation de SQLite en mémoire pour les tests
  - Base de données propre pour chaque test
  - Framework xUnit pour les tests
  - FluentAssertions pour des assertions lisibles
- Points clés testés :
  - Isolation des tests
  - Performance des requêtes
  - Intégrité des données
  - Gestion des contraintes de base de données

![Résultats des tests unitaires pour la couche Infrastructure](/assets/img/TaskBoard.Infrastructure.Tests.png "Résultats des tests unitaires pour la couche Infrastructure")

### Étape 14 - Tests de la couche Presentation (15/12/2024)

- Tests des Services :
  - TaskService : Tests du mapping entre ViewModels et entités
  - Tests des notifications de changements (OnChange)
  - ReadmeService : Tests du parsing Markdown en HTML
- Choix techniques :
  - Moq pour simuler les handlers CQRS
  - FluentAssertions pour des assertions lisibles
  - Isolation des services pour les tests unitaires
- Points clés testés :
  - Conversion des données entre les couches
  - Communication avec la couche Application
  - Gestion des événements de mise à jour
  - Traitement du contenu Markdown
  - Gestion des erreurs et cas limites

### Étape 15 - Tests des composants Blazor avec bUnit (15/12/2024)

La mise en place de tests pour les composants Blazor à l'aide de bUnit permet de vérifier le bon fonctionnement de l'interface utilisateur. Les tests du TaskDialog démontrent les aspects clés suivants :

- Tests du rendu initial :

  - Vérification de l'affichage correct du formulaire vide pour une nouvelle tâche
  - Vérification de l'affichage des données existantes pour une tâche en édition
  - Validation de l'état initial des champs du formulaire

- Tests des interactions utilisateur :

  - Simulation des saisies utilisateur dans les champs du formulaire
  - Vérification du comportement lors de la soumission du formulaire
  - Test des événements de fermeture du dialogue
  - Validation des données avant soumission

- Tests de validation :

  - Vérification des messages d'erreur pour les champs requis
  - Test des différents scénarios de validation (champs vides, espaces)
  - Comportement du formulaire avec des données invalides

- Choix techniques :

  - Utilisation de bUnit pour le rendu des composants
  - FluentAssertions pour des assertions lisibles
  - Tests basés sur les scénarios d'utilisation réels
  - Isolation des composants pour les tests unitaires

- Points clés :
  - Tests du cycle de vie complet des composants
  - Vérification des interactions entre composants
  - Test des événements et callbacks
  - Couverture des différents états du composant

Ces tests permettent d'assurer la qualité et la fiabilité de l'interface utilisateur en vérifiant automatiquement le bon fonctionnement des composants Blazor.

### Étape 16 - Tests des pages Blazor avec bUnit (15/12/2024)

La mise en place de tests pour les pages Blazor avec bUnit permet de vérifier l'intégration des composants et le comportement global de l'application. Les tests de la page TaskBoard démontrent les aspects clés suivants :

- Tests du rendu de la structure de la page :

  - Vérification de l'affichage des trois colonnes (À faire, En cours, Terminé)
  - Validation de l'affichage des cartes de tâches dans les bonnes colonnes
  - Test de l'affichage des boutons d'ajout pour chaque colonne

- Tests des interactions avec le service de tâches :

  - Vérification de la récupération initiale des tâches
  - Test de l'ajout d'une nouvelle tâche
  - Validation de la mise à jour en temps réel
  - Test de la suppression d'une tâche

- Tests de l'organisation des tâches :
  - Vérification du tri des tâches dans chaque colonne
  - Test de l'affichage correct des détails des tâches
  - Validation de la répartition des tâches par état
- Tests des mises à jour en temps réel :
  - Test du rafraîchissement automatique lors de modifications
  - Vérification de la synchronisation entre les colonnes
  - Validation des événements de mise à jour

![Résultats des tests unitaires pour la couche Presentation](/assets/img/TaskBoard.BlazorServer.Tests.png "Résultats des tests unitaires pour la couche Presentation")

### Étape 17 - Refactorisation en composants et Drag & Drop (16/12/2024)

- Création des composants dédiés :

  - TaskColumn pour gérer les colonnes individuelles
  - Amélioration du TaskCard pour le drag & drop
  - Styles CSS modulaires pour les animations

- Choix techniques :

  - Composants réutilisables pour la maintenabilité
  - Drag & drop natif HTML5 pour les performances
  - JavaScript minimal pour le support cross-browser

- Améliorations :
  - Feedback visuel pendant le drag & drop
  - Animations fluides pour les déplacements
  - Validation visuelle des zones de drop

Cette refactorisation améliore l'expérience utilisateur tout en rendant le code plus maintenable et extensible.
