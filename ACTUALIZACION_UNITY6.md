# Reciclo — actualización Unity 6

Esta versión consolida el trabajo visual, técnico y jugable realizado sobre el proyecto **Reciclo** para Unity 6.3 LTS.

## Escena principal

- Escena recomendada: `Assets/Scenes/Reciclo2D_Game.unity`.
- Está incluida en Build Settings con índice `0`.
- Se corrigió una referencia duplicada de `RectTransform` que producía el error `Transform child has another parent` al abrirla.
- Se conservan `Ver1.0.0` y `Version 2.0.0` como escenas adicionales de referencia.

## Assets gráficos

- Sprites PNG con transparencia real y preparados para Unity.
- Importación sin compresión, sin mipmaps y con filtro Point para conservar el pixel art.
- Hojas independientes para Vero, Sr. Basura, residuos y tachos.
- Recortes corregidos para evitar fondos, halos y fragmentos de celdas vecinas.
- Pivotes inferiores conservados para personajes y objetos animables.
- Fondo de la plaza central integrado en las capas del escenario.
- Assets binarios administrados mediante Git LFS.

## Personajes y presentación

- Vero aparece como acompañante al lado izquierdo de los tachos.
- Sr. Basura permanece visible durante la batalla y está ubicado sobre la línea roja.
- Sprite feliz de Vero corregido para la pantalla de victoria, con proporciones conservadas.
- Barra de vida de Vero colocada en una capa superior para evitar que quede oculta.
- Barra de vida y texto actualizados inmediatamente al recibir daño o recuperar energía.

## Jugabilidad

- Sistema de cinco carriles y cinco categorías de residuos.
- El carril y la categoría se seleccionan de forma independiente, permitiendo que cualquier residuo aparezca en cualquier carril.
- El sprite del residuo sigue correspondiendo correctamente a su categoría.
- Clasificar correctamente causa daño a Sr. Basura y aumenta el combo.
- Los errores y residuos no recogidos reducen la vida de Vero.
- Bonificaciones de daño por combos y clasificaciones perfectas.
- Ataques por oleadas, dificultad progresiva y efecto de smog durante la batalla.
- Condiciones funcionales de victoria y Game Over.

## Pantalla de victoria

- Imagen propia de `Vero_Happy_0`, centrada y sin deformación.
- Mensajes ecológicos y datos de reciclaje elegidos aleatoriamente en cada victoria.
- Se evita repetir inmediatamente el mismo mensaje.
- Texto grande, blanco, en negrita, con autoajuste y contorno oscuro.
- Canvas del modal con Pixel Perfect y prioridad superior para mantener la interfaz legible.

## Interfaz y compatibilidad

- Adaptación para resoluciones móviles verticales y ventanas anchas del Editor.
- Menús de pausa y configuración.
- HUD de puntuación, combo, tiempo, vida de Vero y vida del jefe.
- Prefabs organizados para tachos y residuos.
- Carpetas temporales de Unity, respaldos locales y archivos de inspección excluidos del repositorio.

## Verificación

- La escena principal abre sin errores de serialización.
- Unity reconoce nuevamente todos los sprites y el fondo después de descargar los objetos de Git LFS.
- Se verificó el inicio de la partida, generación de residuos, daño a Vero y activación de Game Over.

## Enlaces del proyecto

- [GDD visual en Figma — ReCiclo: Ciudad Limpia](https://www.figma.com/board/IdHrxLbqrhKgEPP8PQBZ4D/ReCiclo---Ciudad-Limpia-%257C-GDD-Visual?node-id=0-1&p=f&t=5j3vT53LV4dv0gTQ-0)
- Video de presentación: enlace pendiente de agregar.
