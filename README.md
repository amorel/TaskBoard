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