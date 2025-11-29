FROM kalilinux/kali-rolling:latest

# Installation des outils et packages nécessaires 
RUN apt-get update && \
    apt-get -y install \
        openssh-server \
        hashid \
        john \
        hashcat \
        hydra \
        dnsmap \
        wordlists \
        seclists && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* # clean et rm -rf pour réduire la taille

# Définir le mot de passe à "123"
RUN echo "root:123" | chpasswd

# Autoriser root par mot de passe
RUN sed -i 's/^#PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config 

# Décompresser rockyou
RUN gunzip /usr/share/wordlists/rockyou.txt.gz 

# Copier et donner droit d'exécution à l'entrypoint
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Création des répertoires ou seront stockés les wordlists, userlists et hashfiles de l'user
RUN mkdir /root/hashfiles
RUN mkdir /root/wordlists
RUN mkdir /root/userlists
RUN mkdir /root/combolists

# Conteneur écoute sur le port 22
EXPOSE 22

# Point d'entrée toujours executé au démarrage pour redémarrer le service ssh
ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]

