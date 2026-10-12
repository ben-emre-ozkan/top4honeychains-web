# 13. Confianza y privacidad {#trust}

## El registro de auditoría {#trust-audit}

Cada cambio en la plataforma (cada guardado, aprobación, finalización, envío, inicio de sesión o cambio de permisos) se escribe en el **Registro de auditoría**. Los administradores de la plataforma lo abren en **Gestión de Usuarios** → **Registro de auditoría**.

Cada fila muestra:

- **Cuándo**: hora local; pase el cursor por encima para ver la hora UTC;
- **Quién**: el usuario y la organización;
- **Acción**: por ejemplo Crear, Actualizar, Aprobar, Finalizar, Verificar, Iniciar sesión, con indicadores como "Autoaprobado";
- **Registro**: qué registro, con un enlace cuando existe una página de administración;
- **Cambios**: cada campo modificado como **anterior → nuevo**, con el nombre del campo en su idioma; los identificadores de usuarios y organizaciones se muestran por su nombre;
- **Hash**: el comienzo de la huella de la fila.

**Mostrar datos sin procesar** abre los datos almacenados exactamente como se escribieron (valores anteriores y nuevos).

Filtre por tipo de registro, acción, nombre de usuario o indicador (por ejemplo `SelfApproved`) y haga clic en **Filtrar**.

<!-- SHOT: audit-log -->
Figure: El registro de auditoría con los cambios anterior → nuevo y los datos sin procesar.

### Solo adición y cadena de hash {#trust-hash-chain}

Al registro de auditoría solo se le pueden añadir entradas. La base de datos rechaza cualquier intento de modificar o eliminar una fila. Cada fila contiene una huella SHA-256 de su propio contenido y de la fila anterior, lo que forma una cadena.

**Verificar la cadena de hash** vuelve a calcular toda la cadena. "Cadena de hash: Las … filas se verificaron correctamente." significa que no se ha modificado nada. En caso contrario, el mensaje indica la primera fila que se modificó o que no apunta a la fila anterior.

## Registros que no cambian {#trust-locked}

- Un registro de PCC aprobado conserva su aprobación solo mientras sus valores no cambien.
- Una partida finalizada y sus lotes están bloqueados.
- Los resultados de laboratorio aprobados se convierten en valores del PCC 1 de solo lectura.
- Las correcciones requieren dos administradores y se registran con el motivo.

## Archivos y huellas {#trust-files}

Los formularios de entrega, los PDF de los informes de laboratorio, los documentos de las organizaciones, los comprobantes de registro y los documentos de consentimiento se almacenan fuera de la carpeta web pública, con un nombre aleatorio y su **huella SHA-256**. Los archivos subidos se comprueban por su contenido, no solo por el nombre del archivo: PDF de hasta 10 MB, o JPG, PNG o WEBP de hasta 8 MB. Antes de usar un informe de laboratorio aprobado, la plataforma comprueba que el PDF almacenado sigue coincidiendo con su huella. Cualquier persona puede comparar el PDF de un informe con su huella en la página pública de verificación, sin subirlo.

## Lo que puede leer el público {#trust-public}

Las páginas públicas (Historia de la miel, página del apicultor, verificación de informes) muestran solo información aprobada y verificada:

- **Lugar**: solo la **provincia** (la página del apicultor también indica el distrito). La provincia se calcula en el servidor; las coordenadas de los apiarios nunca salen del servidor. El mapa colorea provincias, nunca puntos; por ahora cubre Türkiye, y están previstos los mapas de los demás países socios.
- **Personas**: la Historia de la miel nombra a los apicultores que aportaron miel a la partida; la página del apicultor muestra el nombre solo con su consentimiento escrito (si no, un código neutro). Nunca un número de teléfono, una fecha de nacimiento ni la foto de una persona, y ningún nombre de usuario de la plataforma.
- **Datos**: los datos de laboratorio y de los puntos de control; un veredicto dentro/fuera del límite solo para los **límites verificados**. Las respuestas de texto libre y los archivos no se publican, salvo que un administrador lo haya permitido expresamente para un PCC cuyas respuestas no contengan datos personales.
- **Informes**: el código del informe, el laboratorio, la acreditación, la fecha y la huella; nunca el lote ni el apicultor.

- **Fotos**: solo fotos de los apiarios que suministraron la partida, servidas sin sus metadatos (sin posición GPS ni datos de la cámara).

Solo existe una Historia de la miel para las partidas finalizadas, y una página del apicultor solo mientras el consentimiento esté activo.

<!-- SHOT: public-map -->
Figure: Orígenes por provincia en la Historia de la miel pública.
