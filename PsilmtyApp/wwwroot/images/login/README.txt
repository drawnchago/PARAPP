============================================================
 LOGOS DE LOGIN POR PARROQUIA
============================================================

Coloca aquí las imágenes/logos de cada parroquia para el login.

FORMATO RECOMENDADO:
  - Archivo: PNG con fondo transparente (o SVG)
  - Tamaño:  256 x 256 px (cuadrado)
  - Peso:    menos de 200 KB

CÓMO SE USA:
  El logo se referencia desde la base de datos en el campo
  `parishes.logo_url` con la RUTA RELATIVA dentro de la app, ej:

      images/login/san-isidro.png

  Y en la app se muestra como <img src="images/login/san-isidro.png">.

PARA SAN ISIDRO LABRADOR MTY:
  1. Abre  C:\Users\Santiago\Documents\Iglesia\sello1.psd  en Photoshop
  2. Archivo → Exportar → Exportar como... → PNG
  3. Ajusta a 256x256 px
  4. Guarda como:  san-isidro.png  en esta carpeta
  5. En Configuración → "Logo del login", selecciona "san-isidro.png"

  (El archivo .psd NO se puede usar directamente: la app web solo
   muestra PNG/JPG/SVG.)
