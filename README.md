# Task Board - Tableau de bord collaboratif en temps réel

## À propos du projet
Ce projet est un tableau de bord collaboratif en temps réel développé avec Blazor Server pour la gestion de projets. Il permet aux utilisateurs de gérer des tâches de manière interactive et collaborative, avec des mises à jour en temps réel pour tous les utilisateurs connectés.

## Architecture et choix techniques
- **Clean Architecture** : Le projet est structuré selon les principes de la Clean Architecture pour assurer une séparation claire des responsabilités et faciliter la maintenance et les tests.
- **Blazor Server** : Choisi pour ses capacités de mise à jour en temps réel via SignalR et son intégration naturelle avec .NET.
- **MVVM (Model-View-ViewModel)** : Pattern utilisé pour séparer la logique de présentation des vues et faciliter les tests unitaires.
- **SQLite** : Base de données légère et sans serveur, parfaite pour le développement et le prototypage.

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
- Mise à jour du fichier _Imports.razor pour inclure tous les namespaces nécessaires :
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