# ReCiclo

![Versión](https://img.shields.io/badge/versión-0.2.0-2ea44f)
![Unity](https://img.shields.io/badge/Unity-6000.3.24f1-000000?logo=unity)
![Estado](https://img.shields.io/badge/estado-versión%20preliminar-f2a900)

Juego educativo 2D de clasificación de residuos ambientado en una ciudad andina. Arrastra cada residuo al contenedor correcto, derrota a los guardianes de cada zona y transforma escenarios contaminados en espacios limpios.

![Portada de ReCiclo](Assets/Resources/ReCicloUI/MainMenu_Background_Wide.png)

## Características

- Tres niveles seleccionables desde un mapa: Plaza, Mercado y Basurero.
- Cinco categorías de residuos: papel, plástico, vidrio, orgánico y electrónico.
- Sistema de arrastrar y soltar compatible con ratón y pantalla táctil.
- Tres enfrentamientos: Can Basura, Señor Basura del Mercado y Señor Basura Final.
- El jefe final recupera vida cuando el jugador clasifica incorrectamente.
- Fondos contaminados y restaurados que cambian según el resultado.
- Música propia para cada nivel.
- Sistema de puntuación, precisión, combos, estrellas y progreso guardado.
- Menús de pausa y victoria con regreso directo al mapa.
- Interfaz adaptable a distintas relaciones de aspecto.

## Niveles

| Nivel | Zona | Jefe | Música |
|---|---|---|---|
| 1 | Plaza Central | Can Basura | `Sorting_the_Chaos` |
| 2 | Mercado Municipal | Señor Basura | `Plaza_Morning_Sort` |
| 3 | Basurero Final | Señor Basura Final | `Final_Reclamation` |

### Mercado

| Contaminado | Restaurado |
|---|---|
| ![Mercado contaminado](Assets/Resources/Backgrounds/Mercado_Dirty.png) | ![Mercado restaurado](Assets/Resources/Backgrounds/Mercado_Clean.png) |

### Basurero

| Contaminado | Restaurado |
|---|---|
| ![Basurero contaminado](Assets/Resources/Backgrounds/Basurero_Dirty.png) | ![Centro restaurado](Assets/Resources/Backgrounds/Basurero_Clean.png) |

## Controles

- Ratón o toque: seleccionar y arrastrar residuos.
- Soltar sobre un tacho: clasificar el residuo.
- `Esc`: abrir o cerrar el menú de pausa.
- Los botones del final del nivel y del menú de pausa permiten volver al mapa.

## Requisitos

- Unity `6000.3.24f1`.
- Git.
- Sistema operativo compatible con Unity 6.

## Abrir el proyecto

1. Clona el repositorio:

   ```bash
   git clone https://github.com/AntonyGGL/Reciclo.git
   ```

2. Abre la carpeta clonada desde Unity Hub con Unity `6000.3.24f1`.
3. Espera a que Unity restaure los paquetes e importe los recursos.
4. Abre `Assets/Scenes/Inicio.unity`.
5. Pulsa **Play**.

Las escenas incluidas en Build Settings son:

1. `Inicio`
2. `MapaMundial`
3. `Reciclo2D_Game`

## Estructura principal

```text
Assets/
├── Resources/
│   ├── Audio/          Música del juego
│   ├── Backgrounds/    Escenarios limpios y contaminados
│   ├── Bosses/         Hojas de sprites de jefes
│   └── ReCicloUI/      Fondos y elementos de interfaz
├── Scenes/             Escenas de Unity
├── Scripts/            Sistemas y lógica del juego
└── SpritePackUnity6/   Recursos visuales del juego
```

## Desarrollo y contribuciones

Consulta [CONTRIBUTING.md](CONTRIBUTING.md) antes de proponer cambios. Las novedades de cada versión se registran en [CHANGELOG.md](CHANGELOG.md).

## Estado de la licencia

El código y los recursos se publican actualmente para consulta y evaluación. No se concede una licencia de redistribución o explotación comercial hasta que el equipo documente el origen y los permisos de todos los recursos gráficos y musicales.

## Créditos

Proyecto desarrollado como videojuego educativo sobre reciclaje y cuidado ambiental, ambientado visualmente en la región andina del Perú.
