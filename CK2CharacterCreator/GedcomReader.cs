using System;
using System.Collections.Generic;
using System.Linq;
using GeneGenie.Gedcom;
using GeneGenie.Gedcom.Parser;

namespace CK2CharacterCreator
{
    public class GedcomReader
    {
        private static GedcomRecordReader gedcomReader;

        public static void ReadGedcom(string pathToGedcom, ref string errormsg)
        {
            gedcomReader = GedcomRecordReader.CreateReader(pathToGedcom);
            gedcomReader.ReplaceXRefs = false;
            
            if (gedcomReader.Parser.ErrorState != GeneGenie.Gedcom.Enums.GedcomErrorState.NoError)
            {
                errormsg = "Could not parse Gedcom! " + Environment.NewLine + gedcomReader.Parser.ErrorState;
            }

            foreach (var person in gedcomReader.Database.Individuals)
            {
                PersonToCharacter(person, ref errormsg);
            }
            
            //Can only add relationships once everyone has IDs
            foreach (var person in gedcomReader.Database.Individuals)
            {
                SetUpFamily(person, ref errormsg);
            }
        }



        private static void PersonToCharacter(GedcomIndividualRecord person, ref string errormsg)
        {

            var chara = new Character();

            chara.GedcomId = person.XRefID;
            if(person.SexChar == null)
            {
                errormsg += $"One character with ID {GetProperId(chara.GedcomId)} does not have a defined gender. I am sorry, but that is too progressive for medieaval times." + Environment.NewLine;
                return;
            }
            chara.Female = person.SexChar.ToString().ToLower() == "f";
            if (string.IsNullOrWhiteSpace(person.Names.First().Name))
            {
                errormsg += $"One character with ID {GetProperId(chara.GedcomId)} does not have a defined name. You are horrible for putting them through this." + Environment.NewLine;
            }
            chara.Name = person.Names.First().Name.Split('/')[0].Trim();
            

            string surname = GetSurname(person.Names.First());
            var dyn = Program.Dynasties.FirstOrDefault(x => x.Name == surname);

            if (dyn != null) chara.Dynasty = dyn;


            foreach (var evt in person.Events)
            {
                if (evt.GedcomTag == "BIRT" && evt.Date != null)
                {
                    chara.Birth = ParseDate(evt.Date.Date1);
                }
                if (evt.GedcomTag == "DEAT" && evt.Date != null)
                {
                    chara.Death = ParseDate(evt.Date.Date1);
                }
            }

            //If no birth date is defined, make one up:
            if(chara.Birth == null)
            {
                chara.Birth = "1000.1.1"; //No better way to make one atm
                errormsg = $"Character {chara.Name} {GetSurname(person.Names.First())} does not have a birthdate!" + Environment.NewLine;
            }

            //If no death date defined, make one up.
            if (chara.Death == null)
            {
                var seed = GetSeedFromName(chara.Name + " " + GetSurname(person.Names.First()));
                var random = new Random(seed).Next(50, 80);
                chara.Death = int.Parse(chara.Birth.Substring(0, 4)) + random + ".1.1";
            }

            //Notes
            foreach (var note in person.Notes)
            {
                var foundNote = person.Database.Notes.Find(x => x.XRefID == note);

                UseNotes(foundNote.Text, chara);
            }
        }

        private static string GetProperId(string xRefId)
        {
            var toString = gedcomReader.Parser.XrefCollection.ToString();
            var properKeyList = toString.Split(new char[] { '{', ',', '}' }).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var number = int.Parse(xRefId.Substring(4));
            var properId = properKeyList[number -1];
            return properId;
        }

        private static int GetSeedFromName(string name)
        {
            int sum = 0;
            foreach (var chr in name)
            {
                sum += chr;
            }
            return sum;
        }

        //TODO: read from additional input files
        private static readonly string[] TRAITS = { "abdominal_pain", "administrator", "adventurer", "affectionate", "aggressive_leader", "amateurish_plotter", "ambitious", "arbitrary", "architect", "ares_own", "aryika", "ashari", "augustus", "bad_priest_aztec", "bad_priest_christian", "bad_priest_muslim", "bad_priest_norse", "bad_priest_tengri", "bad_priest_zoroastrian", "baltic_leader", "baptized_by_bishop", "baptized_by_pope", "baptized_by_satan", "bastard", "berserker", "bhikkhu", "bhikkhuni", "blinded", "bloodthirsty_gods_1", "bloodthirsty_gods_2", "bloodthirsty_gods_3", "bon_leader", "born_in_the_purple", "brahmin", "brave", "brilliant_strategist", "brooding", "cancer", "cannibal_trait", "cat", "cavalry_leader", "celibate", "charismatic_negotiator", "charitable", "chaste", "chest_pain", "child_of_consort", "child_of_consort_male", "clubfooted", "conscientious", "content", "cough", "cramps", "craven", "crowned_by_bishop", "crowned_by_myself", "crowned_by_pope", "crowned_by_priest", "cruel", "crusader", "curious", "cynical", "dancing_plague", "decadent", "deceitful", "defensive_leader", "depressed", "desert_terrain_leader", "detached_priest", "diarrhea", "digambara_jain", "diligent", "disfigured", "drunkard", "duelist", "dull", "dwarf", "dynastic_kinslayer", "dysentery", "eagle_warrior", "elusive_shadow", "envious", "erudite", "eunuch", "excommunicated", "experimenter", "fair", "falconer", "familial_kinslayer", "faqih", "fatigue", "feeble", "fever", "finnish_leader", "flamboyant_schemer", "flanker", "flat_terrain_leader", "flu", "food_poisoning", "fortune_builder", "fussy", "gamer", "gardener", "genius", "giant", "gluttonous", "gondi_shahansha", "gout", "greedy", "gregarious", "grey_eminence", "groomed", "hafiz", "hajjaj", "hard_pregnancy", "harelip", "has_aztec_disease", "has_bubonic_plague", "has_measles", "has_small_pox", "has_tuberculosis", "has_typhoid_fever", "has_typhus", "haughty", "headache", "heavy_infantry_leader", "hedonist", "hellenic_leader", "heresiarch", "holy_warrior", "homosexual", "honest", "horse", "humble", "hunchback", "hunter", "idolizer", "ill", "imbecile", "immortal", "impaler", "in_hiding", "inbred", "incapable", "indian_pilgrim", "indolent", "indulgent_wastrel", "infection", "infirm", "inspiring_leader", "intricate_webweaver", "is_fat", "is_malnourished", "jungle_terrain_leader", "just", "kailash_guardian", "kind", "kinslayer", "kow_tow_completed_tier_1", "kow_tow_completed_tier_2", "kow_tow_completed_tier_3", "kow_tow_travels", "kshatriya", "lefthanded", "legit_bastard", "leper", "levy_coordinator", "light_foot_leader", "lisp", "logistics_expert", "lovers_pox", "lunatic", "lustful", "mahayana_buddhist", "maimed", "malaise", "mangled", "martial_cleric", "master_of_flame", "mastermind_theologian", "midas_touched", "mirza", "misguided_warrior", "monk", "mountain_terrain_leader", "mujahid", "muni", "mutazilite", "mystic", "naive_appeaser", "narrow_flank_leader", "norse_leader", "nun", "nyames_shield", "on_hajj", "on_indian_pilgrimage", "on_pilgrimage", "one_eyed", "one_handed", "one_legged", "organizer", "pagan_branch_1", "pagan_branch_2", "pagan_branch_3", "pagan_branch_4", "paranoid", "patient", "peasant_leader", "peruns_chosen", "physician", "pilgrim", "pirate", "playful", "pneumonic", "poet", "possessed", "pregnancy_finishing", "pregnant", "proud", "quick", "rabies", "rash", "ravager", "reincarnation", "robust", "romuvas_own", "rough_terrain_leader", "rowdy", "sanyasi", "sanyasini", "saoshyant", "saoshyant_descendant", "sapper", "sayyid", "scarred", "scarred_high", "scarred_mid", "schemer", "scholar", "scholarly_theologian", "scurvy", "sea_queen", "seaking", "seducer", "seductress", "severely_injured", "shaddai", "shaivist_hindu", "shaktist_hindu", "shieldmaiden", "shrewd", "shy", "sickly", "siege_leader", "skilled_tactician", "slavic_leader", "slothful", "slow", "smartist_hindu", "socializer", "strategist", "stressed", "strong", "stubborn", "sturdy", "stutter", "sun_warrior", "svetambara_jain", "sympathy_christendom", "sympathy_indian", "sympathy_islam", "sympathy_judaism", "sympathy_pagans", "sympathy_zoroastrianism", "syphilitic", "temperate", "tengri_leader", "tengri_warrior", "theologian", "theravada_buddhist", "thrifty_clerk", "timid", "tough_soldier", "travelling", "tribal_kinslayer", "trickster", "troubled_pregnancy", "trusting", "twin", "ugly", "ukkos_shield", "uncouth", "underhanded_rogue", "unyielding_leader", "vaishnavist_hindu", "vaishya", "vajrayana_buddhist", "valhalla_bound", "varangian", "viking", "vomiting", "war_elephant_leader", "weak", "west_african_leader", "willful", "winter_soldier", "wounded", "wroth", "zealous", "zodiac_aquarius", "zodiac_aries", "zodiac_cancer", "zodiac_capricorn", "zodiac_gemini", "zodiac_leo", "zodiac_libra", "zodiac_pisces", "zodiac_sagittarius", "zodiac_scorpio", "zodiac_taurus", "zodiac_virgo", "zun_leader" };
        private static readonly string[] CULTURES = { "afghan", "alan", "andalusian_arabic", "arberian", "armenian", "ashkenazi", "assamese", "assyrian", "avar", "baloch", "basque", "bear", "bedouin_arabic", "bengali", "bodpa", "bohemian", "bolghar", "bosnian", "breton", "bulgarian", "carantanian", "castillan", "cat", "catalan", "coptic", "crimean_gothic", "croatian", "cuman", "daju", "dalmatian", "danish", "dog_culture", "dragon_culture", "duck_culture", "dutch", "egyptian_arabic", "elephant_culture", "english", "ethiopian", "finnish", "frankish", "frisian", "georgian", "german", "greek", "gujurati", "han", "hausa", "hedgehog_culture", "hindustani", "horse", "hungarian", "ilmenian", "irish", "italian", "jurchen", "kannada", "kanuri", "karluk", "khanty", "khazar", "khitan", "kirghiz", "komi", "kurdish", "lappish", "lettigallish", "levantine_arabic", "lithuanian", "lombard", "maghreb_arabic", "manden", "marathi", "meshchera", "mongol", "mordvin", "nahuatl", "nepali", "norman", "norse", "norwegian", "nubian", "occitan", "old_frankish", "old_saxon", "oriya", "outremer", "panjabi", "pecheneg", "persian", "pictish", "polish", "pommeranian", "portuguese", "prussian", "rajput", "red_panda", "roman", "romanian", "russian", "saka", "samoyed", "saxon", "scottish", "sephardi", "serbian", "severian", "sindhi", "sinhala", "sogdian", "somali", "songhay", "soninke", "suebi", "sumpa", "swedish", "tamil", "tangut", "telugu", "tocharian", "turkish", "ugricbaltic", "uyghur", "visigothic", "volhynian", "welsh", "zaghawa", "zhangzhung" };
        private static readonly string[] RELIGIONS = { "aztec", "aztec_reformed", "baltic_pagan", "baltic_pagan_reformed", "bektashi", "bogomilist", "bon", "bon_reformed", "buddhist", "cathar", "druze", "finnish_pagan", "finnish_pagan_reformed", "fraticelli", "hellenic_pagan", "hellenic_pagan_reformed", "hurufi", "ibadi", "iconoclast", "jain", "karaite", "kharijite", "khurmazta", "lollard", "manichean", "mazdaki", "messalian", "miaphysite", "monophysite", "monothelite", "nestorian", "norse_pagan_reformed", "orthodox", "pagan", "paulician", "samaritan", "shiite", "slavic_pagan", "slavic_pagan_reformed", "taoist", "tengri_pagan", "tengri_pagan_reformed", "waldensian", "west_african_pagan", "west_african_pagan_reformed", "yazidi", "zikri", "zun_pagan", "zun_pagan_reformed", "catholic", "hindu", "jewish", "norse_pagan", "sunni", "zoroastrian" };

        private static void UseNotes(string text, Character chara)
        {
            foreach (var note in text.ToLower().Split(new char[] { ' ', '\n', '\r' }))
            {
                if (TRAITS.Contains(note)) chara.Traits.Add(note);
                if (RELIGIONS.Contains(note)) chara.Religion = note;
                if (CULTURES.Contains(note)) chara.Culture = note;
            }
        }

        private static void SetUpFamily(GedcomIndividualRecord person, ref string errormsg)
        {
            var chara = FindCharacter(person.XRefID);

            foreach (var family in person.ChildIn)
            {
                string familyId = family.Family;

                var properfam = person.Database.Families.FirstOrDefault(x => x.XRefID == familyId);
                if(properfam == null)
                {
                    errormsg += $"Character {chara.Name} is a child in the family {GetProperId(familyId)}, which does not seem to exist. Either they are a compulsive liar or your GEDCOM has issues..." + Environment.NewLine;
                    return;
                }

                var father = FindCharacter(properfam.Husband);
                var mother = FindCharacter(properfam.Wife);

                if (father != null) chara.Father = father;
                if (mother != null) chara.Mother = mother;
            }


            foreach (var family in person.SpouseIn)
            {
                string familyId = family.Family;

                var properfam = person.Database.Families.FirstOrDefault(x => x.XRefID == familyId);
                if(properfam == null)
                {
                    errormsg += $"The family ID {GetProperId(familyId)}, which {chara.Name} is a spouse in, refers to a family that does not exist." + Environment.NewLine;
                    return;
                }

                //Only do it for ladies, otherwise marriages are duplicated
                if (chara.Female)
                {
                    var father = FindCharacter(properfam.Husband);
                    if (father != null && properfam.Marriage != null) {

                        var marryDate = properfam.Marriage.Date != null 
                            ? ParseDate(properfam.Marriage.Date.Date1) 
                            : int.Parse(chara.Birth.Substring(0, 4)) + 17 + ".1.1";
                       
                        var divorce = properfam.Events.FirstOrDefault(x => x.GedcomTag == "DIV" || x.GedcomTag == "ANUL");
                        if (divorce != null)
                        {
                            chara.Marry(father, marryDate, ParseDate(divorce.Date.Date1));
                        } else chara.Marry(father, marryDate);

                    }
                }
            }

        }

        private static Character FindCharacter(string xRefID)
        {
            return Program.Characters.FirstOrDefault(x => x.GedcomId == xRefID);
        }

        private static readonly List<string> MONTHS = new List<string>(new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" });
        private static string ParseDate(string date)
        {
            //Full date
            var dateSplit = date.ToUpper().Split(" ");
            if (dateSplit.Length == 3 && MONTHS.Contains(dateSplit[1]))
            {
                return dateSplit[2] + "." + (MONTHS.IndexOf(dateSplit[1]) + 1) + "." + dateSplit[0];
            }

            //Just year
            if (int.TryParse(date, out int year))
            {
                return year + ".1.1";
            }

            return "";
        }

        private static string GetSurname(GedcomName name)
        {
            if (name.Name.Split('/').Length > 1)
            {
                return name.Name.Split('/')[1].Trim();
            }
            else if (!string.IsNullOrWhiteSpace(name.Surname))
            {
                return name.SurnamePrefix.Trim() + " " + name.Surname.Trim();
            }
            else
            {
                return "";
            }
        }
    }
}
