# 4. El flujo completo {#flow}

Este capítulo sigue una partida de miel a través de la plataforma: quién realiza cada paso y qué debe estar terminado antes de que pueda empezar el siguiente ([Figure](#fig-flow-overview)). Los capítulos de cada rol (5–11) explican cada pantalla en detalle.

<!-- SHOT: flow-overview -->
Figure: El **Flujo del proceso** de una partida finalizada: cada etapa con su estado y su organización responsable.

## Los puntos de control (PCC) {#ccp-list}

Un **punto crítico de control (PCC)** es un paso en el que se mide o se comprueba algo. Cada PCC tiene parámetros (por ejemplo, temperatura o humedad), límites y un estado. Los PCC 0–3 pertenecen al **lote de miel cruda**; los PCC 4–10, a la **partida** ([Table](#tbl-ccps)).

Table: Los puntos críticos de control (PCC) y sus roles predeterminados {#tbl-ccps}

| PCC | Pertenece a | Qué se controla | Lo ingresa (predeterminado) | Lo aprueba (predeterminado) |
|---|---|---|---|---|
| PCC 0 | Lote | Auditoría primaria en la recepción, **antes** del análisis de laboratorio | Responsable de recepción de muestras de la planta | Responsable de aseguramiento de la calidad de la planta |
| PCC 1 | Lote | Muestreo y resultados de laboratorio | Se toma del informe de laboratorio aprobado | Responsable de laboratorio (o el Responsable de aseguramiento de la calidad de la planta, ver más abajo) |
| PCC 2 | Lote | Monitoreo de la temperatura (calentamiento) | Personal de la planta | Responsable de aseguramiento de la calidad |
| PCC 3 | Lote | Control del estado del filtro | Personal | Responsable de aseguramiento de la calidad |
| PCC 4 | Partida | Control de la homogeneidad (mezcla) | Personal del propietario de la partida | Responsable de aseguramiento de la calidad |
| PCC 5 | Partida | Control de tiempo y temperatura (pasteurización) | Personal | Responsable de aseguramiento de la calidad |
| PCC 6 | Partida | Eliminación de espuma y sedimentos | Personal | Responsable de aseguramiento de la calidad |
| PCC 7 | Partida | Control de la cantidad (llenado) | Personal (Envasado) | Responsable de aseguramiento de la calidad |
| PCC 8 | Partida | Verificación de la exactitud de la etiqueta | Personal (Envasado) | Responsable de aseguramiento de la calidad |
| PCC 9 | Partida | Control de la integridad del envase | Personal (Envasado) | Responsable de aseguramiento de la calidad |
| PCC 10 | Partida | Monitoreo de temperatura y humedad en el almacenamiento | Personal (Comercio mayorista, Comercio minorista o Envasado) | Responsable de aseguramiento de la calidad |

El PCC 10 aparece dos veces en la vida de una partida: primero **en la partida, en la planta** (debe aprobarse antes de que la partida pueda sellarse) y después como **registro de almacenamiento en cada receptor** tras un envío.

Un registro de PCC pasa por estos estados: **Asignado** (aún sin datos) → **En curso** (algunos datos) → **Enviado** (todos los datos obligatorios ingresados; a la espera de aprobación) → **Completado** con una aprobación. Un PCC que no corresponde puede marcarse como **No aplicable** con un motivo; aun así debe aprobarse. Cambiar cualquier dato anula la aprobación existente.

## Paso a paso {#flow-steps}

La [Table](#tbl-flow-steps) enumera cada paso, quién lo realiza y qué bloquea.

Table: El flujo completo, paso a paso {#tbl-flow-steps}

| # | Paso | Quién | Dónde | Bloquea el paso siguiente hasta que… |
|---|---|---|---|---|
| 1 | Se registran el apicultor y el apiario | Administrador de la plataforma | **Apicultores**, **Apiarios** | El apicultor está activo y tiene un apiario |
| 2 | Se verifica el número de registro y se vincula la asociación | Administrador de la plataforma | **Registro de apicultores y asociaciones** | (Necesario para la página pública del apicultor, no para las entregas) |
| 3 | Se registra la entrega: se crea un lote de miel cruda | Responsable de recepción de muestras (planta) | **Entregas (recepción)** → **Registrar una entrega** | – (los PCC 0–3 se asignan al lote automáticamente) |
| 4 | Se precinta la muestra y se envía a un laboratorio | Responsable de recepción de muestras | **Muestras y laboratorio** → **Tomar una muestra** | El laboratorio la recibe |
| 5 | Se ingresa el PCC 0 | Responsable de recepción de muestras | **Mis tareas** → PCC 0 | Queda enviado |
| 6 | Se aprueba el PCC 0 | Responsable de aseguramiento de la calidad (otra persona) | **Mis tareas** → PCC 0 | **Los resultados de laboratorio solo pueden ingresarse después de aprobar el PCC 0** |
| 7 | Se recibe la muestra y se controla el precinto | Analista o Responsable de laboratorio | **Mis tareas** → **Muestras por recibir** | Un precinto roto rechaza la muestra; hay que tomar una nueva |
| 8 | Se ingresan los resultados de laboratorio, se sube el PDF y se envía el informe | Analista | **Mis tareas** → **Resultados de laboratorio por ingresar** | Están todos los valores obligatorios, el PDF, el número del informe y la fecha |
| 9 | Se aprueba (o se devuelve) el informe de laboratorio | Responsable de laboratorio (otra persona) | **Mis tareas** → **Informes de laboratorio por aprobar** | La aprobación escribe los resultados en el PCC 1 del lote |
| 10 | Se ingresan y aprueban los PCC 2–3 | Personal y, después, Responsable de aseguramiento de la calidad | **Mis tareas** | Un lote solo puede entrar en una partida cuando **todos** sus PCC están completados o son no aplicables, **y están aprobados** |
| 11 | Se crea la partida, se agregan los lotes, se define el propietario y se asignan los PCC 4–10 | **Administrador de la plataforma** (por ahora) | **Producciones Finales de Miel** | La partida tiene lotes y sus PCC |
| 12 | Se ingresan y aprueban los PCC 4–10, incluido el almacenamiento del PCC 10 en la planta | Personal y, después, Responsable de aseguramiento de la calidad | **Mis tareas** | Todos los PCC de la partida deben estar aprobados |
| 13 | Se solicita la finalización | Personal o Administrador de la organización del propietario | **Mi organización** → partida → **Solicitar la finalización** | El botón está desactivado mientras falte algo |
| 14 | Se confirma la finalización: la partida queda sellada | Responsable de aseguramiento de la calidad (otra persona) | **Mis tareas** → **Solicitudes de finalización** | – La Historia de la miel y el código QR se publican; la partida y sus lotes quedan bloqueados |
| 15 | Se crea el envío | Personal o Administrador de la organización del poseedor | **Envíos y existencias** → **Enviar** | El receptor confirma |
| 16 | Se confirma (o se rechaza) la recepción | Personal o Administrador de la organización del receptor | **Mis tareas** → **Envíos por confirmar** | La custodia se transfiere solo con la confirmación |
| 17 | Se registra y aprueba el almacenamiento (PCC 10) en el receptor | Personal del receptor y, después, su Responsable de aseguramiento de la calidad | **Envíos y existencias** → **Almacenamiento** | – |
| 18 | El receptor lee los registros del productor | Receptor (cualquier rol con derecho de lectura de PCC) | Páginas de PCC, solo lectura | – |
| 19 | El consumidor escanea el código QR | Cualquier visitante público (sin cuenta) | Historia de la miel | – |

### Por qué el PCC 0 va antes del análisis de laboratorio {#why-ccp0-first}

El PCC 0 es la auditoría primaria en la recepción: ¿esta miel es aceptable? Por eso el laboratorio no puede guardar, enviar ni aprobar resultados hasta que el PCC 0 del lote esté completado **y aprobado**. Hasta entonces, la tarea de laboratorio muestra "El PCC 0 (auditoría primaria) de este lote aún no está completo; los resultados de laboratorio se registran y aprueban solo después." Mientras tanto, sí se puede devolver un informe.

### Por qué se necesita el PCC 10 de la partida antes de sellarla {#why-ccp10}

La finalización sella la partida solo cuando todos los PCC asignados a ella están aprobados. El PCC 10 de la partida registra las condiciones de almacenamiento en la propia planta, de modo que el registro de almacenamiento de la planta debe existir antes del sellado. Cada poseedor posterior lleva su propio registro de almacenamiento PCC 10 para sus existencias.

### Por ahora, solo el administrador crea partidas {#batch-admin-only}

Un administrador de la plataforma crea la partida, elige sus lotes y cantidades, define la organización propietaria y asigna los PCC de la partida. Después, el personal de la planta ingresa los PCC de la partida. El proyecto decidirá si el personal de la planta podrá crear partidas por sí mismo.

## Qué bloquea qué: resumen {#flow-blocks}

- **No hay entrega sin** una organización de Procesamiento verificada y un rol con el derecho de recepción.
- **No hay resultados de laboratorio sin** una muestra recibida con el precinto intacto y un PCC 0 aprobado.
- **No puede aprobar un informe** la persona que ingresó los resultados.
- **Ningún lote entra en una partida** hasta que todos sus PCC estén completados o sean no aplicables y estén aprobados, y el lote pertenezca al mayorista de la partida.
- **No hay finalización** mientras la partida no tenga miel, falte un PCC de la partida o no esté aprobado, o haya un PCC de lote abierto; la lista "Falta antes de finalizar:" indica cada problema.
- **No puede confirmar la finalización** la persona que la solicitó.
- **No hay envío** de una partida que no esté finalizada, ni de una cantidad mayor que la que usted tiene en su poder.
- **No hay cambios** en una partida finalizada ni en sus lotes, salvo mediante una corrección hecha por dos administradores.
