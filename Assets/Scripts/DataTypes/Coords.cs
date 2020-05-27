using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Coords
{
    public float X;
    public float Y;
    public Coords(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }

    public Coords(Vector3 vector)
    {
        this.X = vector.x;
        this.Y = vector.z;
    }

    public static float SqrDistance(Coords a, Coords b)
    {
        return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    }

    public static float Distance(Coords a, Coords b)
    {
        return (float)System.Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
    }

    public static bool AreNeighbours(Coords a, Coords b)
    {
        return System.Math.Abs(a.X - b.X) <= 0.5f && System.Math.Abs(a.Y - b.Y) <= 0.5f;
    }

    public static Coords invalid
    {
        get
        {
            return new Coords(-1f, -1f);
        }
    }

    public static Coords up
    {
        get
        {
            return new Coords(0f, 1f);
        }
    }

    public static Coords down
    {
        get
        {
            return new Coords(0f, -1f);
        }
    }

    public static Coords left
    {
        get
        {
            return new Coords(-1f, 0f);
        }
    }

    public static Coords right
    {
        get
        {
            return new Coords(1f, 0f);
        }
    }

    public static Coords operator +(Coords a, Coords b)
    {
        return new Coords(a.X + b.X, a.Y + b.Y);
    }

    public static Coords operator -(Coords a, Coords b)
    {
        return new Coords(a.X - b.X, a.Y - b.Y);
    }

    public static bool operator ==(Coords a, Coords b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator ==(Coords a, Vector3 b)
    {
        return ((Mathf.Abs(a.X) - Mathf.Abs(b.x)) < 0.1f) && ((Mathf.Abs(a.Y) - Mathf.Abs(b.z)) < 0.1f);
    }

    public static bool operator !=(Coords a, Coords b)
    {
        return a.X != b.X || a.Y != b.Y;
    }

    public static bool operator !=(Coords a, Vector3 b)
    {
        return ((Mathf.Abs(a.X) - Mathf.Abs(b.x)) > 0.1f) || ((Mathf.Abs(a.Y) - Mathf.Abs(b.z)) > 0.1f);
    }

    public static implicit operator Vector2(Coords v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static implicit operator Vector3(Coords v)
    {
        return new Vector3(v.X, 0, v.Y);
    }

    public override bool Equals(object other)
    {
        return (Coords)other == this;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override string ToString()
    {
        return "(" + this.X + " ; " + this.Y + ")";
    }
}
