using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RigPose12UpperBodyProcedural : MonoBehaviour
{
    const int JOINT_COUNT = 12;

    [Header("Archivo (StreamingAssets)")]
    public string nombreArchivo = "pose12_formatoA.csv"; // frame,index,x,y,z

    [Header("Reproducción")]
    public float fps = 30f;
    public bool reproducirAlIniciar = true;

    [Header("Unidades y ejes")]
    public float escala = 0.001f;              // típico: mm -> m
    public Vector3 signoEjes = Vector3.one;    // (-1,1,1) invierte X, etc.

    [Header("Geometría")]
    public float radioArticulacion = 0.03f;
    public float grosorHueso = 0.02f;

    // Conexiones SOLO parte superior (sin piernas).
    static readonly (int a, int b)[] Huesos = new (int, int)[]
    {
        // Cabeza (diagnóstico mínimo)
        (0,1),(0,2),(1,3),(2,4),

        // Hombros y brazos
        (5,6),
        (5,7),(7,9),
        (6,8),(8,10),

        // Tronco hacia ancla (11)
        (5,11),(6,11)
    };

    List<Vector3[]> frames;
    Transform[] joints = new Transform[JOINT_COUNT];
    Transform[] boneObjs;

    int frameIndex = 0;
    bool playing = true;
    float acumulador = 0f;

    void Start()
    {
        playing = reproducirAlIniciar;

        string ruta = Path.Combine(Application.streamingAssetsPath, nombreArchivo);
        frames = PoseCsvLoader.LoadFrames(ruta);

        CrearArticulaciones();
        CrearHuesos();
        AplicarFrame(0);
    }

    void Update()
    {
        if (frames == null || frames.Count == 0) return;

        if (WasPressedSpace()) playing = !playing;

        if (WasPressedLeft())
        {
            playing = false;
            Paso(-1);
        }
        if (WasPressedRight())
        {
            playing = false;
            Paso(+1);
        }

        if (!playing) return;

        acumulador += Time.deltaTime;
        float dt = 1f / Mathf.Max(1e-6f, fps);

        while (acumulador >= dt)
        {
            acumulador -= dt;
            Paso(+1);
        }
    }

    void Paso(int delta)
    {
        frameIndex = (frameIndex + delta) % frames.Count;
        if (frameIndex < 0) frameIndex += frames.Count;
        AplicarFrame(frameIndex);
    }

    void AplicarFrame(int idxFrame)
    {
        var f = frames[idxFrame];

        // 1) Articulaciones: el array manda.
        for (int i = 0; i < JOINT_COUNT; i++)
        {
            Vector3 v = Vector3.Scale(f[i] * escala, signoEjes);
            joints[i].localPosition = v;
        }

        // 2) Huesos: segmento entre dos articulaciones.
        for (int i = 0; i < Huesos.Length; i++)
        {
            int a = Huesos[i].a;
            int b = Huesos[i].b;

            Vector3 pa = joints[a].position;
            Vector3 pb = joints[b].position;

            Vector3 dir = pb - pa;
            float len = dir.magnitude;

            if (len < 1e-6f)
            {
                boneObjs[i].gameObject.SetActive(false);
                continue;
            }
            boneObjs[i].gameObject.SetActive(true);

            Vector3 mid = (pa + pb) * 0.5f;
            boneObjs[i].position = mid;
            boneObjs[i].rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);

            // Cilindro Unity: altura base 2 => escalaY = len/2
            boneObjs[i].localScale = new Vector3(grosorHueso, len * 0.5f, grosorHueso);
        }
    }

    void CrearArticulaciones()
    {
        for (int i = 0; i < JOINT_COUNT; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"J{i:00}";
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * (radioArticulacion * 2f);

            var col = go.GetComponent<Collider>();
            if (col) Destroy(col);

            joints[i] = go.transform;
        }
    }

    void CrearHuesos()
    {
        boneObjs = new Transform[Huesos.Length];
        for (int i = 0; i < Huesos.Length; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"Hueso{i:00}_{Huesos[i].a}-{Huesos[i].b}";
            go.transform.SetParent(transform, false);

            var col = go.GetComponent<Collider>();
            if (col) Destroy(col);

            boneObjs[i] = go.transform;
        }
    }

    // Entrada compatible (Input System / Input Manager)
    bool WasPressedSpace()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space);
#else
        return false;
#endif
    }

    bool WasPressedLeft()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.leftArrowKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.LeftArrow);
#else
        return false;
#endif
    }

    bool WasPressedRight()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.rightArrowKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.RightArrow);
#else
        return false;
#endif
    }
}
