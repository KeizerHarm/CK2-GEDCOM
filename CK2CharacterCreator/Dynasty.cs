using System;

namespace CK2CharacterCreator
{
    public class Dynasty
    {
        public string Name { get; set; }
        private int id { get; set;}
        public int Id {
            get
            {
                if (id != 0) return id;
                id = Program.StartingDynId++;
                return id;
            }
            set
            {
                id = value;
                if (value >= Program.StartingDynId)
                {
                    Program.StartingDynId = ++value;
                }
            }
        }
        public string Culture { get; set; }
        public string Religion { get; set; } //Not necessary for game, just to make assigning religions to characters easier
        private int lastCharacterId { get; set; }
        public int LastCharacterId
        {
            get
            {
                if(lastCharacterId == 0)
                {
                    lastCharacterId = Id;
                    return lastCharacterId;
                }
                return lastCharacterId;
            }

            set
            {
                lastCharacterId = value;
            }
        }

        public override string ToString()
        {
            return
                Id + " = {" + Environment.NewLine +
                "    name = \"" + Name + "\"" + Environment.NewLine +
                "    culture = \"" + Culture + "\"" + Environment.NewLine +
                "}" + Environment.NewLine;
        }

        public Dynasty()
        {
            Program.Dynasties.Add(this);
        }
    }
}
