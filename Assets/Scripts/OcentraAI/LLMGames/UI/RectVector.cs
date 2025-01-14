namespace OcentraAI.LLMGames.UI
{
    [System.Serializable]
    public struct RectVector
    {
        public float X;
        public float Y;
        public float Z;
        public float Width;
        public float Height;

        public RectVector(float x, float y, float z, float width, float height)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return $"(X: {X}, Y: {Y}, Z: {Z}, Width: {Width}, Height: {Height})";
        }
    }
}