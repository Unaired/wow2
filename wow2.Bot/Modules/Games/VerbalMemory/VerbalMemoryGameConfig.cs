using System.Collections.Generic;
using System.Text.Json.Serialization;
using Discord;

namespace wow2.Bot.Modules.Games.VerbalMemory
{
    public class VerbalMemoryGameConfig : GameConfig
    {
        [JsonIgnore]
        public List<string> UnseenWords { get; set; } = new List<string>() { "abominably", "abort", "abraded", "absentees", "absquatulates", "acroter", "administrated", "advertisements", "aeromedical", "aerophagias", "aga", "aggressor", "ailurophile", "aleurones", "alewives", "alignments", "aliunde", "allopatric", "alternating", "ambient", "ambivalence", "ament", "amphetamine", "annular", "anonyms", "anorectic", "antidisestablishmentarian", "antigenicity", "apartment", "apperceives", "applause", "appraisement", "apprehensible", "archive", "ascriptions", "aseptically", "assailing", "assumption", "atlas", "attestations", "auricled", "automatism", "automats", "averagely", "aversive", "aviators", "babbitt", "backbends", "backroom", "balliest", "banishment", "bankruptcy", "beagle", "beaver", "bedizening", "bedrocks", "beefeaters", "beestings", "befuddled", "begriming", "billabong", "bioecology", "blames", "bleat", "blintzes", "boatel", "bowers", "braveness", "brazes", "bribery", "bridgeboard", "broadsheets", "buckminsterfullerene", "bunches", "buskin", "butcherer", "butterfish", "cachinnates", "caliph", "camouflager", "cancels", "cannonades", "canteen", "caper", "capercaillie", "carbon", "carper", "carpings", "carte", "carting", "cartographical", "cascara", "caseates", "catastrophist", "cation", "caucusing", "ceded", "ceilometer", "cesareans", "chaetognaths", "chaffiest", "cheeseburger", "cherry", "chimneypiece", "china", "chitarrone", "chivaree", "christenings", "chromyl", "chunder", "cleaner", "cloakroom", "clots", "coasters", "coatroom", "coextensive", "coition", "coitus", "collaboratively", "collectors", "columella", "columniations", "comical", "complicates", "compunction", "concertinaing", "confer", "confides", "congregate", "contingency", "controller", "conventual", "coons", "copaibas", "corkwoods", "corrector", "corselet", "cosmology", "cosmopolitanism", "costings", "costmaries", "countess", "countrysides", "crawliest", "created", "crimsoning", "crinkliest", "crossbeam", "crustacean", "curium", "cussedly", "cyclostyle", "dazzle", "dealate", "declarations", "deflocculate", "delicatessen", "developer", "devolving", "diablerie", "dignity", "dill", "discomforting", "disforested", "disgruntles", "disjoints", "dislocations", "dismantle", "dismantling", "dispatchers", "dispensational", "divagations", "dizziest", "dizziness", "dock", "documental", "doffs", "dogie", "dolichocephaly", "domestication", "domiciles", "doubt", "downier", "downing", "dramas", "dreary", "dungs", "dustcart", "ebullition", "ecphoneses", "edgily", "efflux", "egesting", "egging", "eloquence", "emasculator", "emerging", "emfs", "empathic", "enchantment", "englutted", "eosins", "error", "escargots", "espousal", "eucalypti", "exact", "excommunication", "excretes", "exercises", "experimenter", "extrapolation", "faction", "fantailed", "farther", "farthest", "fashionistas", "faves", "felony", "fencing", "fenestella", "fermion", "ferriage", "fib", "fichus", "fifteenth", "fixatives", "flaming", "fledgling", "fluoroscopic", "fondness", "footworn", "forbearing", "foregrounding", "forthwith", "foxgloves", "frisson", "frumpy", "gadgeteer", "gauziness", "gearboxes", "geomorphology", "gladiatorial", "glob", "gluey", "gore", "gory", "governorship", "graduator", "grandfatherly", "granges", "grassy", "graybeards", "greasiest", "greenshank", "grievance", "haecceity", "hamburger", "handbarrow", "handpicking", "harp", "hatch", "haughtiness", "hellishly", "holmium", "homelands", "houselights", "hulas", "humidly", "hushing", "hutches", "hyenas", "hyphenated", "hypothetical", "iceboating", "idleness", "ileac", "immovability", "imperium", "impermanence", "impermissible", "importuning", "impulse", "inapprehensible", "inapt", "incombustibility", "indict", "indigestible", "indignant", "infuse", "inocula", "inoculable", "insults", "interdict", "internal", "invalidism", "ironist", "irrespectively", "isochronism", "issuing", "ja", "jackass", "jejunum", "joiners", "joke", "judders", "judiciousness", "karstification", "kenaf", "kep", "kidder", "kidnap", "kremlins", "lackadaisicalness", "ladybird", "landmark", "laps", "latria", "lebensraum", "leftovers", "leisure", "licensee", "lifters", "lightyear", "linebacker", "lire", "listserver", "litchi", "literalist", "lithiases", "lithophyte", "littering", "loathings", "locule", "luging", "luminescence", "luminousness", "lumisterol", "lutetium", "lysis", "mackerel", "made", "maelstrom", "mappers", "matriculate", "mayweed", "meatloaf", "meets", "megs", "menfolk", "mermen", "meshugga", "mesoderms", "minatory", "minstrels", "misidentifies", "misstating", "mnemonically", "molested", "mollycoddling", "monohydroxy", "monophonically", "mothproofer", "motoneuron", "multipliers", "munitioning", "murderously", "mushers", "mushing", "mythical", "mythologists", "nainsooks", "neolithic", "newspaperwoman", "nigh", "nightstands", "nihils", "nimble", "nomadism", "noncommissioned", "nosepiece", "novel", "numismatology", "objet", "obscener", "occlusive", "occultation", "oculars", "oculist", "oersteds", "ogival", "oleograph", "oozes", "ophthalmoscope", "orangy", "ordinand", "ornithosis", "outstretching", "over", "overattached", "overbooked", "overbuild", "overcame", "overcompetetive", "overkill", "oversensitive", "oversteer", "overstimulation", "overwintering", "pacifistic", "palpably", "papilla", "paramecium", "paramountcy", "pardner", "paretic", "partan", "partible", "parulis", "passant", "pastille", "patisseries", "paw", "pedals", "pegboard", "pelotas", "peltings", "penitent", "pennon", "pensive", "perspectively", "petrodollar", "phenotypes", "piazzas", "picofarads", "pinchcock", "pipage", "pisciculture", "plebby", "plummeting", "pneumatometer", "politicker", "polyvinyl", "pontifical", "prearrangement", "predestined", "prediagnostic", "predication", "prejudice", "presager", "prewarm", "prickings", "princeliness", "procreative", "profascist", "promycelium", "pronates", "prophetically", "proscriptive", "protanopia", "protectionists", "psalteries", "psychopaths", "pug", "pullout", "purifier", "pyramidical", "pyromaniacal", "pyroxenes", "quadrupeds", "quadruplicate", "qualification", "quarterings", "quarterstaves", "queasiness", "quieting", "quintillion", "quotation", "radiographic", "rages", "rainwater", "ransomed", "rant", "rapidly", "rapports", "rasping", "rate", "ratifying", "reasoner", "reassessed", "reconfiguring", "reconfirmation", "recused", "redemptive", "reducing", "refilling", "refiner", "refits", "reflection", "refrain", "refusal", "regaling", "regelate", "relaunching", "releases", "relocates", "remands", "remaps", "rematching", "reminds", "remise", "remunerated", "remunerates", "remunerators", "renovation", "reoriented", "represents", "reproduce", "resend", "restrengthens", "rethinks", "retrenchment", "retrocession", "reunited", "rev", "revolts", "rhizocarpous", "rodents", "rogue", "rubbishing", "rubble", "ruffling", "rumbly", "ruminantly", "sadder", "salaam", "saliencies", "salves", "sanative", "satiric", "sauce", "scab", "scandent", "scherzo", "scillas", "scooting", "scrabbler", "scrubbers", "sedatest", "segregationists", "seigniorages", "sensual", "sequitur", "servicewoman", "sewage", "shipway", "shoetree", "shovelnose", "shows", "showstopper", "signor", "silvern", "sirdar", "skeins", "skewnesses", "slanderers", "sliding", "snagging", "snoopiest", "sootier", "soundstage", "souse", "spearer", "spectacularly", "spectrophotometers", "speiss", "spermicide", "spinnakers", "spongiform", "spuriously", "staghound", "stagnated", "starlike", "steamrollering", "stepped", "stuffing", "sty", "stylistic", "subarctic", "subfloor", "subordinated", "subsume", "subtitling", "suffers", "sumptuous", "superheated", "superposable", "supervisions", "swelled", "swiping", "syllogistically", "sylphs", "synchroscope", "systematist", "tabes", "tabulators", "tallowy", "tarnish", "teaberry", "tearfully", "teddies", "televise", "tenuity", "termites", "terrorist", "theirs", "theoreticians", "thermotensile", "thievery", "thigmotaxis", "thirteenths", "tingling", "topsides", "torn", "tractableness", "transmissions", "transom", "trendsetters", "tricker", "triply", "trolley", "trommel", "truculently", "truelove", "turds", "typical", "ufologists", "umbles", "unassailed", "unchain", "unconscionable", "uncrossed", "unionists", "unpaired", "unsays", "unteachable", "uproariously", "vagaries", "valving", "vellicate", "vena", "vernaculars", "via", "villus", "visaged", "vise", "vitalism", "warpaths", "wavefront", "welded", "welters", "whaps", "whined", "whipstitch", "whirlybird", "whopped", "whortleberries", "winnows", "wireworks", "wispier", "withstand", "zaniest", "zootechnics" };

        [JsonIgnore]
        public List<string> SeenWords { get; set; } = new List<string>();

        [JsonIgnore]
        public int Turns { get; set; } = 0;

        /// <summary>Gets or sets the message with the current word.</summary>
        [JsonIgnore]
        public IUserMessage CurrentWordMessage { get; set; }

        public List<VerbalMemoryLeaderboardEntry> LeaderboardEntries { get; set; } = new();
    }
}