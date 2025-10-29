# CraKit
CraKit est un outil permettant de centraliser et utiliser les outils de crackages développé en C#. 

## Instructions d'installation
- Ouvrir Docker Desktop
- Dans le répertoire /Installation, suivez les commandes suivantes :
```cmd
# Build l'image et la machine Kali 
docker compose build 

# Créer et lancer le conteneur
docker compose up

# Arrêter le conteneur 
docker compose stop

# Relancer le conteneur 
docker compose start

# Pour vérifier que l'image a bien été créée, vous devriez avoir l'iamge "kali-crakit" 
docker images

# Pour vérifier que le conteneur a bien été créé et en marche 
docker ps -a

# Vous devriez avoir en sortie : 
<id>   kali-crakit:latest   "/bin/bash /entrypoi…"   x minutes ago   Up x minutes   0.0.0.0:2222->22/tcp, [::]:2222->22/tcp   kali-crakit
```




## Conception 
- Squelette (draw.io) : https://app.diagrams.net/#G1UbUJwg6TBZXoDjet9roBA-ND3-4c_nKP#%7B%22pageId%22%3A%22iCKKW3toqpHFSzha94B6%22%7D
- Prototype Figma : https://www.figma.com/proto/IwhjoJBby0OitEgHiEIXr1/Prototype-CraKit?node-id=1-46&t=g9DDKSZ4qGne3Atx-1

## Développements 
- .NET 8 
- SSH.NET 
- Avalonia 
- IDE : JetBrains Rider

## Outils utilisés

### Craking
- Hashcat (https://hashcat.net/hashcat/)
- John The Reaper (https://github.com/openwall/john)

### Enumeration
- dnsmap (https://www.kali.org/tools/dnsmap/)




