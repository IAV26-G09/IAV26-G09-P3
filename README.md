# FPS Online

Se convierte el proyecto Unity original UPV-FPS de la versión 6000.3.9.f1 a *6000.3.12f1*, perdiendo registros del GitLab anterior. ¡Ojo, es importante trabajar también en esta versión, para tener la versión funcional con las herramientas de pruebas de modo multijugador de Unity! Recuerda descargarlo usando Git LFS.

Se ha borrado la plantilla de documentación que venía originalmente con el repositorio, y a continuación se expone una información mínima.

## Información básica

Human_Prefab representa al jugador humano y UCM_Bot es la IA que hay que programar si se quiere tener un bot contra el que enfrentarse.

Lo más relevante que puede encontrarse en la raíz del prefab como Human_Prefab:

* NetworkObject: identidad de red del jugador.
* PlayerInput (Input System): componente de Unity que gestiona dispositivos/mapas.
* NewMonoBehaviourScript (tu “ClientPlayerMove” real, el hombre es que no está bien puesto): habilita cámara/controles sólo para el propietario, crea el HUD del marcador, etc.
* PlayerRespawner: maneja muerte/respawn en red (RPC al server y respawn al cliente).
* ClientNetworkTransform: sincroniza transform (owner authority en tu setup).
* PlayerHealthSync: sincroniza vida/estado.
* PlayerVotingSync (solo en Human): sistema de votación/acciones especiales.
* PlayerNameTag: nombre/kills/deaths en red.
* ClientNetworkAnimator: animación sincronizada.
* Rigging / IK / Weapon sync: WeaponIKSync, ThirdPersonWeaponSync, RigBuilder, constraints, etc.
* UI (CanvasScaler, GraphicRaycaster, TMP): el canvas world-space del nametag y elementos.

En UCM_Bot encontramos componentes muy parecidos, aunque se ha añadido FSM como ejemplo de dónde podría ir una máquina de estados que tome las decisiones de ese bot.
