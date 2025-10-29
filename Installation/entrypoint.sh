#!/usr/bin/env bash

echo "[entrypoint] Démarrage service SSH au premier plan"

exec /usr/sbin/sshd -D #Tourner en premier plan le service ssh