# 16. Confianza y privacidad {#trust}

## El registro de auditoría {#trust-audit}

Cada cambio en la plataforma (cada guardado, aprobación, finalización, envío, inicio de sesión, cambio de permisos, descarga de pruebas y ejecución de la conservación de datos) se escribe en el **Registro de auditoría**. Los administradores de la plataforma lo abren en **Gestión de Usuarios** → **Registro de auditoría**; un Responsable de cumplimiento lee las entradas de su propia organización en la página **Cumplimiento**.

Cada fila muestra:

- **Cuándo**: hora local; pase el cursor por encima para ver la hora UTC;
- **Quién**: el usuario y la organización;
- **Acción**: por ejemplo Crear, Actualizar, Aprobar, Finalizar, Verificar, Iniciar sesión, Descargar, con indicadores como "Autoaprobado";
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
- Los datos de una entrega confirmada están bloqueados; a los envases, los eventos de precinto, los registros de apicultores, las entradas de precio, los mensajes SMS, los avisos y sus aceptaciones solo se les pueden añadir entradas.
- Las correcciones requieren dos administradores y se registran con el motivo.

## Códigos que confirman un contenido {#trust-codes}

El apicultor confirma un registro y una entrega con un código de 6 dígitos recibido por SMS, y los roles que firman confirman el aviso de privacidad de la misma manera. Cada código está vinculado a una huella de exactamente el contenido para el que se envió: cambiar cualquier cosa después lo anula. Los códigos son válidos durante 5 minutos y se pueden probar 3 veces; solo se guarda un hash del código. Donde no es posible un SMS, una foto firmada sustituye al código, y una segunda persona (el Responsable de cumplimiento) la acepta o la rechaza.

## Archivos y huellas {#trust-files}

Los formularios de entrega, las fotos de registro, los PDF de los informes de laboratorio, los documentos de las organizaciones, los comprobantes de registro y los documentos de consentimiento se almacenan fuera de la carpeta web pública, con un nombre aleatorio y su **huella SHA-256**. Los archivos subidos se comprueban por su contenido, no solo por el nombre del archivo: PDF de hasta 10 MB, o JPG, PNG o WEBP de hasta 8 MB. Antes de usar un informe de laboratorio aprobado, la plataforma comprueba que el PDF almacenado sigue coincidiendo con su huella. Cualquier persona puede comparar el PDF de un informe con su huella en la página pública de verificación, sin subirlo.

**Quién descarga las pruebas.** Los archivos de prueba solo los descargan el rol que los registra, el Responsable de cumplimiento y el administrador de la organización, y los administradores de la plataforma; otro rol de la organización recibe 403, otra organización 404, y cada descarga queda escrita en el registro de auditoría (consulte [Derechos sobre los datos](#crud)).

## Avisos, consentimientos y responsable del tratamiento {#trust-notices}

**Avisos de privacidad.** La plataforma y cada organización publican un aviso de privacidad: cómo se tratan los datos personales, con qué finalidad, cuánto tiempo se conservan y quién es el **responsable del tratamiento**. Los usuarios confirman los avisos de la plataforma y de cada organización para la que trabajan (consulte [El aviso de privacidad](#notice-accept)); los apicultores leen su aviso en el registro y lo confirman con su código SMS. Los avisos tienen versiones: una versión nueva vuelve a preguntar a todos, y las aceptaciones anteriores se conservan como prueba. Los textos aún no aprobados por la oficina de protección de datos llevan la marca **BORRADOR**. La página pública **Privacidad y consentimiento** (`/privacy`) muestra los avisos vigentes.

**El responsable del tratamiento.** Cada organización es la responsable del tratamiento de los datos que registra; su razón social, su contacto y su dirección figuran en su aviso, y los mantienen en la página de la organización su Responsable de cumplimiento o un administrador de la plataforma. Un apicultor ve en el portal qué organizaciones tienen sus datos (**Quién tiene sus datos**).

**Consentimientos.** Todo lo que va más allá de lo que necesita el trabajo es opcional y se pide por separado:

- el nombre del apicultor en las páginas públicas;
- SMS informativos sobre entregas y resultados;
- el acceso de la asociación a los precios solicitados por el apicultor;
- una página pública de apicultor, registrada por la asociación.

Un consentimiento se otorga en el registro (confirmado con el código SMS), en el portal o, para la página del apicultor, por la asociación con un documento firmado. Una retirada surte efecto de inmediato: en el portal, o registrada por el Responsable de cumplimiento cuando llega por carta o por teléfono.

**Solicitudes sobre datos personales.** Las solicitudes de acceso, rectificación, supresión y oposición las responde el Responsable de cumplimiento de la organización en un plazo de 30 días (consulte [Solicitudes de los interesados](#co-requests)).

## Lo que puede leer el público {#trust-public}

Las páginas públicas (Historia de la miel, página del apicultor, verificación de informes) muestran solo información aprobada y verificada:

- **Lugar**: solo la **provincia** (la página del apicultor también indica el distrito). La provincia se calcula en el servidor; las coordenadas de los apiarios nunca salen del servidor. El mapa colorea provincias, nunca puntos; por ahora cubre Türkiye, y están previstos los mapas de los demás países socios.
- **Personas**: la Historia de la miel nombra a un apicultor **solo con un consentimiento de nombre activo**; si no, muestra el código neutro de la página del apicultor. Los apicultores registrados antes del procedimiento de consentimiento se muestran con su código hasta que den el consentimiento (o, si la plataforma está configurada así, conservan su nombre hasta que se registren de nuevo). Nunca un número de teléfono, una fecha de nacimiento ni la foto de una persona, y ningún nombre de usuario de la plataforma. Los nombres de colmenar escritos antes de esta versión suelen contener el nombre del apicultor, por eso solo se muestran donde puede mostrarse su nombre; si no, el colmenar se identifica por su provincia ("Colmenar en …"). Lo mismo vale para las descripciones de las fotos.
- **Datos**: los datos de laboratorio y de los puntos de control; un veredicto dentro/fuera del límite solo para los **límites verificados**. Las respuestas de texto libre y los archivos no se publican, salvo que un administrador lo haya permitido expresamente para un PCC cuyas respuestas no contengan datos personales.
- **Informes**: solo se pueden verificar los informes **aprobados**; la página muestra el código del informe, el laboratorio, la acreditación, la fecha y la huella; nunca el lote ni el apicultor.
- **Precios**: nunca.
- **Fotos**: solo fotos de los apiarios que suministraron la partida, servidas sin sus metadatos (sin posición GPS ni datos de la cámara).

Solo existe una Historia de la miel para las partidas finalizadas, y una página del apicultor solo mientras el consentimiento esté activo.

<!-- SHOT: public-map -->
Figure: Orígenes por provincia en la Historia de la miel pública.

## Cuánto tiempo se conservan los datos {#trust-retention}

Los plazos de conservación figuran en la [Table](#tbl-retention): por ejemplo, los códigos de un solo uso 30 días, los mensajes SMS 1 año, los precios 5 años, los lotes y sus registros 5 años después de la fecha de consumo preferente de la partida, y el registro de auditoría 10 años. La ejecución de la conservación está **desactivada**: los administradores de la plataforma ven una simulación que solo cuenta, y la primera ejecución real requiere la aprobación del propietario y la decisión sobre el formato de los seudónimos (consulte [Conservación de datos](#admin-retention)).
