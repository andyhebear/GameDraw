namespace GameDraw
{
    using System;

    [Serializable]
    public class EdgeTrait
    {
        public int ID;

        public EdgeTrait(int id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }
    }
}

