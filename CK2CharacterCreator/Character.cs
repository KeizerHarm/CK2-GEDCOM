using System;
using System.Collections.Generic;

namespace CK2CharacterCreator
{
    public class Character
    {
        public string GedcomId { get; set; }

        public string Birth { get; set; }
        public string Death { get; set; }
        public string Name { get; set; }
        private string religion { get; set; }
        public string Religion
        {
            get
            {
                if (string.IsNullOrWhiteSpace(religion))
                {
                    if (Dynasty != null)
                    {
                        if (!string.IsNullOrWhiteSpace(Dynasty.Religion))
                        {
                            religion = Dynasty.Religion;
                            return religion;
                        }
                    }
                    return "noreligion";
                }
                else return religion;
            }
            set
            {
                religion = value;
            }
        }
        private string culture { get; set; }
        public string Culture {
            get {
                if (string.IsNullOrWhiteSpace(culture))
                {
                    if (Dynasty != null)
                    {
                        if (!string.IsNullOrWhiteSpace(Dynasty.Culture))
                        {
                            culture = Dynasty.Culture;
                            return culture;
                        }
                    }
                    return "noculture";
                }
                else return culture;
            }
            set {
                culture = value;
            }
        }
        public string DNA { get; set; }
        public string Properties { get; set; }
        private int id { get; set; }
        public int Id
        {
            get
            {
                if (id != 0) return id;
                id = Program.StartingCharId++;
                return id;
            }
            set
            {
                id = value;
            }
        }
        public Dynasty Dynasty { get; set; }

        public List<string> Traits { get; set; } = new List<string>();
        public bool Female { get; set; }

        public Character Father { get; set; }
        public Character Mother { get; set; }
        public Marriage Parents
        {
            set
            {
                Mother = value.Char1.Female ? value.Char1 : value.Char2;
                Father = value.Char1.Female ? value.Char2 : value.Char1;
            }
        }
        public List<Marriage> Marriages { get; set; } = new List<Marriage>();
        public Marriage CurrentMarriage
        {
            get
            {
                return Marriages.Find(x => x.EndDate == null);
            }
        }

        public Character()
        {
            Program.Characters.Add(this);
        }

        public void Marry(Character spouse, string startDate, string endDate = null)
        {
            Marriages.Add(new Marriage()
            {
                Char1 = this,
                Char2 = spouse,
                StartDate = startDate,
                EndDate = endDate
            });

            if (spouse.Marriages.Find(x => x.StartDate == startDate) == null)
            {
                spouse.Marry(this, startDate, endDate);
            }
        }

        public override string ToString()
        {
            string txt = Id + " = {" + Environment.NewLine +
                "    name = \"" + Name + "\"" + Environment.NewLine +
                "    culture = \"" + Culture + "\"" + Environment.NewLine +
                "    religion = \"" + Religion + "\"" + Environment.NewLine;
            if (Female) txt += "    female = yes" + Environment.NewLine;
            if (Dynasty != null) txt += "    dynasty = " + Dynasty.Id + " #" + Dynasty.Name + "" + Environment.NewLine;
            if (Father != null) txt += "    father = " + Father.Id + " #" + Father.Name + "" + Environment.NewLine;
            if (Mother != null) txt += "    mother = " + Mother.Id + " #" + Mother.Name + "" + Environment.NewLine;
            if (DNA != null) txt += "    dna = \"" + DNA + "\"" + Environment.NewLine;
            if (Properties != null) txt += "    properties = \"" + Properties + "\"" + Environment.NewLine;

            foreach (var trait in Traits)
            {
                txt += "    add_trait = " + trait + "" + Environment.NewLine;
            }

            txt += "    " + Birth + " = {" + Environment.NewLine +
                "        birth = yes" + Environment.NewLine +
                "    }" + Environment.NewLine;

            foreach (var marriage in Marriages)
            {
                txt += "    " + marriage.StartDate + " = {" + Environment.NewLine +
                    "        add_spouse = " + marriage.Char2.Id + " #" + marriage.Char2.Name + "" + Environment.NewLine +
                    "    }" + Environment.NewLine;
                if (marriage.EndDate != null)
                {
                    txt += "    " + marriage.EndDate + " = {" + Environment.NewLine +
                    "        remove_spouse = " + marriage.Char2.Id + " #" + marriage.Char2.Name + "" + Environment.NewLine +
                    "    }" + Environment.NewLine;
                }
            }

            if(Death != null)
            {
                txt += "    " + Death + " = {" + Environment.NewLine +
                "        death = yes" + Environment.NewLine +
                "    }" + Environment.NewLine;
            }
            return txt +
                "}" + Environment.NewLine;
        }
    }

    public class Marriage
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public Character Char1 { get; set; }
        public Character Char2 { get; set; }
    }
}
