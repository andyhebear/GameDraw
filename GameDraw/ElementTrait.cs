namespace GameDraw
{
    using System;

    [Serializable]
    public class ElementTrait
    {
        public int ID;

        public ElementTrait(int id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }
    }
}

