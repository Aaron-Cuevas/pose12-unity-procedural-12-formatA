using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class PoseCsvLoader
{
    const int JOINT_COUNT = 12;

    // CSV esperado: frame,index,x,y,z
    public static List<Vector3[]> LoadFrames(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("No existe el archivo: " + filePath);

        var frames = new List<Vector3[]>();
        var ci = CultureInfo.InvariantCulture;

        using var sr = new StreamReader(filePath);

        int currentFrame = -1;
        Vector3[] current = null;

        string line;
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("#")) continue;
            if (char.IsLetter(line[0])) continue; // encabezado: frame,index,x,y,z

            var p = line.Split(',');
            if (p.Length < 5) continue;

            if (!int.TryParse(p[0], out int fr)) continue;
            if (!int.TryParse(p[1], out int idx)) continue;
            if (idx < 0 || idx >= JOINT_COUNT) continue;

            if (!float.TryParse(p[2], NumberStyles.Float, ci, out float x)) continue;
            if (!float.TryParse(p[3], NumberStyles.Float, ci, out float y)) continue;
            if (!float.TryParse(p[4], NumberStyles.Float, ci, out float z)) continue;

            if (currentFrame == -1)
            {
                currentFrame = fr;
                current = new Vector3[JOINT_COUNT];
            }

            if (fr != currentFrame)
            {
                frames.Add(current);
                currentFrame = fr;
                current = new Vector3[JOINT_COUNT];
            }

            current[idx] = new Vector3(x, y, z);
        }

        if (current != null) frames.Add(current);

        if (frames.Count == 0)
            throw new Exception("El CSV no produjo fotogramas. Formato esperado: frame,index,x,y,z");

        return frames;
    }
}
