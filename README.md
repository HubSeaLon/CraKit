## 📋 Présentation

**CraKit** est un outil permettant de centraliser et utiliser les outils de crackages développé en C#.

---

## 🛠 Outils Intégrés

CraKit centralise les outils suivants :

### 🔐 Audit de Mots de Passe (Cracking)
* **[Hashcat](https://hashcat.net/hashcat/)** : L'outil de récupération de mots de passe le plus rapide au monde.
* **[John The Ripper](https://github.com/openwall/john)** : Suite logicielle de cassage de mots de passe.
* **[Hydra](https://www.kali.org/tools/hydra/)** : Outil d'attaque de mot de passes brute-force. 
### 🌐 Énumération & Reconnaissance
* **[dnsmap](https://www.kali.org/tools/dnsmap/)** : Outil de découverte de sous-domaines.

---

## 🚀 Installation

```bash
# Git clone le projet
git clone https://github.com/HubSeaLon/CraKit.git
```

### Pré-requis

Avant de commencer, assurez-vous de disposer des éléments suivants :

#### 1. Environnement .NET
CraKit nécessite le SDK **.NET 8**.

* [Télécharger .NET 8](https://dotnet.microsoft.com/fr-fr/download/dotnet/8.0)
* [Vérifier OS compatibilité](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md)

Vérifier l'installation :

```bash
    dotnet --version
```

  
#### 2. IDE 
Nous vous conseillons l'IDE gratuit Rider (nécessite un compte) :

* [Rider JetBrains](https://www.jetbrains.com/fr-fr/rider/download/?section=windows)

#### 3\. Docker (Environnement Kali)

CraKit utilise un conteneur Docker pour exécuter les outils Linux natifs en toute sécurité via une connexion SSH locale.

1.  Ouvrez **Docker Desktop**.
2.  Naviguez dans le répertoire `/Installation` (`cd Installation`) du projet.
3.  Exécutez les commandes suivantes :

<!-- end list -->

```bash
# Construction et démarrage du conteneur en arrière-plan
docker compose up -d
```
**Commandes utiles pour la gestion du conteneur :**
```bash
# Vérifier que l'image "kali-crakit" existe
docker images

# Vérifier que le conteneur est en cours d'exécution
docker ps -a
# Résultat attendu : kali-crakit:latest ... Up x minutes ... 0.0.0.0:2222->22/tcp

# Arrêter / Redémarrer le conteneur
docker compose stop
docker compose start
```

#### 4. Dépendances & Packages


Installez les dépendances nécessaires (notamment le framework UI Avalonia) :

```bash
# Installer les packages
dotnet restore
dotnet clean
```

```bash
# Vérifier les packages
dotnet package list

# Compiler et lancer
dotnet build
dotnet run
```

#### 5\. Design

⚠️ Si l'application est peu lisible au démarrage, vérifiez que le mode sombre est bien activé. ⚠️

-----


## 📐 Conception et Architecture

Pour comprendre la structure interne et le flux de données de CraKit :

* **📘 Diagramme de Classes (Squelette)** : [Voir sur Draw.io](https://app.diagrams.net/#G1UbUJwg6TBZXoDjet9roBA-ND3-4c_nKP#%7B%22pageId%22%3A%22iCKKW3toqpHFSzha94B6%22%7D)
* **🎨 Maquette UX/UI** : [Voir le prototype Figma](https://www.figma.com/proto/IwhjoJBby0OitEgHiEIXr1/Prototype-CraKit?node-id=1-46&t=g9DDKSZ4qGne3Atx-1)

-----
