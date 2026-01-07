# Unity: rig procedural (12 articulaciones) desde CSV `frame,index,x,y,z`

Este proyecto construye un esqueleto 3D procedural en Unity a partir de datos numéricos de pose. La intención es validar de forma directa el paso:

**CSV (12×3 por frame) → Transformaciones en Unity → Esqueleto visible y animado**

El esqueleto se renderiza como:
- **Articulaciones**: esferas
- **Huesos**: cilindros orientados y escalados entre pares de articulaciones

No hay retargeting a un humanoide ni skinning. Aquí se comprueba lo fundamental: que los puntos 3D son suficientes para reconstruir pose y continuidad temporal.

---

## 1) Formato de datos

El loader espera un único formato:

```
frame,index,x,y,z
0,0, ...
0,1, ...
...
1,0, ...
```

- `frame`: fotograma (0, 1, 2, ...)
- `index`: articulación (0..11)
- `x,y,z`: coordenadas 3D del joint en ese frame

Representación interna:
- `frames` es `List<Vector3[]>`
- `frames[f]` es un `Vector3[12]`
- `frames[f][i]` es la posición 3D del joint `i` en el frame `f`

Ejemplos incluidos en `Assets/StreamingAssets/`:
- `pose12_formatoA.csv` (1 frame)
- `demo_pose12_formatoA.csv` (120 frames para verificar reproducción)

---

## 2) Conversión del array a movimiento

La “animación” es la reproducción de frames del CSV a una tasa `fps`.

En cada frame se hace:

### 2.1 Articulaciones
Para cada índice `i`:
1) se lee `J[i] = (x,y,z)` del frame actual  
2) se ajustan unidades y ejes  
3) se asigna a la esfera correspondiente

Código:
```csharp
Vector3 v = Vector3.Scale(f[i] * escala, signoEjes);
joints[i].localPosition = v;
```

- `escala` convierte unidades (por defecto `0.001` para mm → m).
- `signoEjes` invierte ejes cuando el sensor y Unity no comparten convención.

### 2.2 Huesos
Un hueso es un par `(a,b)` de índices. Con `p_a` y `p_b` en mundo:
- centro `mid = (p_a + p_b)/2`
- dirección `dir = p_b - p_a`
- longitud `len = |dir|`

El cilindro se coloca en `mid`, se orienta para que su eje Y apunte a `dir`, y se escala para medir `len`.
El factor `len * 0.5` se debe a que el cilindro primitivo de Unity tiene altura base 2.

---

## 3) Conexiones (parte superior, sin piernas)

Las conexiones por defecto están en `RigPose12UpperBodyProcedural.cs` en el arreglo `Huesos`.
Se pueden editar para coincidir con tu convención de índices.

---

## 4) Uso en Unity

1) Abre el proyecto en Unity.  
2) Crea un GameObject vacío (p. ej. `RigPose12`).  
3) Añade el componente: `RigPose12UpperBodyProcedural`.  
4) En el inspector, elige el CSV:
   - `pose12_formatoA.csv` (pose fija)
   - `demo_pose12_formatoA.csv` (secuencia con movimiento)
5) Play.

Controles:
- Espacio: pausar/reanudar
- Flecha izquierda/derecha: frame anterior/siguiente

---

## 5) Archivos relevantes

- `Assets/Scripts/PoseCsvLoader.cs`
- `Assets/Scripts/RigPose12UpperBodyProcedural.cs`
- `Assets/StreamingAssets/pose12_formatoA.csv`
- `Assets/StreamingAssets/demo_pose12_formatoA.csv`
