namespace ObjParser.Types
{
    public class Color : IType
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public Color()
        {
            R = 1f;
            G = 1f;
            B = 1f;
        }

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length != 4) return;
            R = float.Parse(data[1]);
            G = float.Parse(data[2]);
            B = float.Parse(data[3]);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", R, G, B);
        }
    }
}
